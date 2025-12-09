using Libsql.Client;
using SoccerLinkPlayer.Models;

namespace SoccerLinkPlayer.Services
{
    public class DatabaseService
    {
        private IDatabaseClient _dbClient;
        private const string DatabaseUrl = "https://soccerlinkdb-enbixd.aws-eu-west-1.turso.io";
        private const string AuthToken = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ9.eyJhIjoicnciLCJnaWQiOiJjYTliNTRlNy0zMGNkLTQwOWEtOGEzMy03MjJkZmQxYmFmNGIiLCJpYXQiOjE3NjUyNzg0NjQsInJpZCI6ImEwYjU0YzNmLWZmZGMtNDIyMi1iNmExLWRkYWU3MTdiNTJmOCJ9.m7-Qj3ltdSqebDrDH4Q_c96gAbb6Bqs9xknYfDsodpwEkEslNRGAQ6N10XemtTxRvF9p4S1fzShGjWBRGvcCCQ";

        public async Task InitializeAsync()
        {
            if (_dbClient != null) return;

            _dbClient = await DatabaseClient.Create(opts => {
                opts.Url = DatabaseUrl;
                opts.AuthToken = AuthToken;
            });
        }

        public async Task<Zawodnik?> LoginAsync(string email, string haslo)
        {
            await InitializeAsync();

            // WAŻNE: Wymieniamy kolumny jawnie, aby mieć pewność co do kolejności indeksów (0, 1, 2...)
            var sql = @"
            SELECT 
                Id, 
                AdresEmail, 
                Haslo, 
                NumerTelefonu, 
                Imie, 
                Nazwisko, 
                DataUrodzenia, 
                Pozycja, 
                NumerKoszulki, 
                CzyDyspozycyjny, 
                LepszaNoga, 
                ProbyLogowania, 
                TrenerID
            FROM Zawodnik 
            WHERE AdresEmail = ? AND Haslo = ? 
            LIMIT 1";

            try
            {
                var rs = await _dbClient.Execute(sql, email, haslo);

                // Pobieramy pierwszy wiersz jako IEnumerable
                var rowEnumerable = rs.Rows.FirstOrDefault();

                if (rowEnumerable == null) return null;

                // NAPRAWA BŁĘDU: Zamieniamy IEnumerable na tablicę, aby móc używać indeksów [0], [1] itd.
                var row = rowEnumerable.ToArray();

                return new Zawodnik
                {
                    Id = int.Parse(row[0]?.ToString() ?? "0"),
                    AdresEmail = row[1]?.ToString(),
                    Haslo = row[2]?.ToString(),
                    NumerTelefonu = row[3]?.ToString(),
                    Imie = row[4]?.ToString(),
                    Nazwisko = row[5]?.ToString(),
                    // Obsługa daty (zakładamy format tekstowy YYYY-MM-DD w bazie)
                    DataUrodzenia = DateOnly.TryParse(row[6]?.ToString(), out var date) ? date : DateOnly.FromDateTime(DateTime.MinValue),
                    Pozycja = row[7]?.ToString(),
                    NumerKoszulki = int.Parse(row[8]?.ToString() ?? "0"),
                    CzyDyspozycyjny = int.Parse(row[9]?.ToString() ?? "0"),
                    LepszaNoga = row[10]?.ToString(),
                    ProbyLogowania = int.Parse(row[11]?.ToString() ?? "0"),
                    TrenerID = int.Parse(row[12]?.ToString() ?? "0")
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd bazy danych: {ex.Message}");
                throw;
            }
        }
    }
}