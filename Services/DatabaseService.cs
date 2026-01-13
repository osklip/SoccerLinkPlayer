using Libsql.Client;
using SoccerLinkPlayer.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SoccerLinkPlayer.Services
{
    public class DatabaseService
    {
        private readonly HttpClient _httpClient;
        private const string DatabaseUrl = "https://soccerlinkdb-enbixd.aws-eu-west-1.turso.io";
        private const string AuthToken = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ9.eyJhIjoicnciLCJnaWQiOiJjYTliNTRlNy0zMGNkLTQwOWEtOGEzMy03MjJkZmQxYmFmNGIiLCJpYXQiOjE3NjUyNzg0NjQsInJpZCI6ImEwYjU0YzNmLWZmZGMtNDIyMi1iNmExLWRkYWU3MTdiNTJmOCJ9.m7-Qj3ltdSqebDrDH4Q_c96gAbb6Bqs9xknYfDsodpwEkEslNRGAQ6N10XemtTxRvF9p4S1fzShGjWBRGvcCCQ";

        public DatabaseService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        // --- LOGOWANIE (Bez zmian) ---
        public async Task<Zawodnik?> LoginAsync(string email, string haslo)
        {
            var sql = @"SELECT ZawodnikID, AdresEmail, Haslo, NumerTelefonu, Imie, Nazwisko, DataUrodzenia, Pozycja, NumerKoszulki, CzyDyspozycyjny, LepszaNoga, ProbyLogowania, TrenerID FROM Zawodnik WHERE AdresEmail = ? AND Haslo = ? LIMIT 1";

            try
            {
                var rows = await ExecuteSqlAsync(sql, new object[] { email, haslo });
                var row = rows.FirstOrDefault();
                if (row == null) return null;

                return new Zawodnik
                {
                    ZawodnikID = int.Parse(row[0] ?? "0"),
                    AdresEmail = row[1],
                    // ... (reszta pól jak wcześniej)
                    Imie = row[4],
                    Nazwisko = row[5],
                    TrenerID = int.Parse(row[12] ?? "0")
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd logowania: {ex.Message}");
                throw;
            }
        }

        // --- WIADOMOŚCI (Bez zmian) ---
        public async Task<List<Wiadomosc>> GetMessagesForPlayerAsync(int playerId)
        {
            var sql = "SELECT WiadomoscID, TypNadawcy, NadawcaID, OdbiorcaID, Tresc, DataWyslana, Temat FROM Wiadomosc WHERE OdbiorcaID = ? AND TypNadawcy = 'Trener' ORDER BY DataWyslana DESC";
            var rows = await ExecuteSqlAsync(sql, new object[] { playerId });

            var list = new List<Wiadomosc>();
            foreach (var row in rows)
            {
                list.Add(new Wiadomosc
                {
                    WiadomoscID = int.Parse(row[0] ?? "0"),
                    Tresc = row[4],
                    DataWyslania = DateTime.TryParse(row[5], out var d) ? d : DateTime.MinValue,
                    Temat = row[6]
                });
            }
            return list;
        }

        // --- POBIERANIE DANYCH DO KALENDARZA (NOWA METODA) ---
        public async Task<List<ElementKalendarza>> GetFullCalendarAsync(int trenerId)
        {
            var fullList = new List<ElementKalendarza>();

            try
            {
                // 1. Pobierz MECZE
                // Zakładam kolumny: MeczID, SkladMeczowyID, Przeciwnik, DataRozpoczecia, Miejsce
                var sqlMecze = "SELECT MeczID, Przeciwnik, DataRozpoczecia, Miejsce FROM Mecz WHERE TrenerID = ? AND DataRozpoczecia >= date('now')";
                var rowsMecze = await ExecuteSqlAsync(sqlMecze, new object[] { trenerId });

                foreach (var row in rowsMecze)
                {
                    var data = DateTime.TryParse(row[2], out var d) ? d : DateTime.MinValue;
                    fullList.Add(new ElementKalendarza
                    {
                        Id = int.Parse(row[0] ?? "0"),
                        Tytul = $"Mecz: {row[1]}", // np. Mecz: Grom
                        Podtytul = row[3], // Miejsce
                        Typ = "MECZ",
                        Data = data,
                        GodzinaDisplay = data.ToString("HH:mm"),
                        KolorTypu = Colors.Green // Zielony dla meczów
                    });
                }

                // 2. Pobierz TRENINGI
                // Zakładam kolumny: TreningID, Typ, DataRozpoczecia, DataZakonczenia, Miejsce
                var sqlTreningi = "SELECT TreningID, Typ, DataRozpoczecia, DataZakonczenia, Miejsce FROM Trening WHERE TrenerID = ? AND DataRozpoczecia >= date('now')";
                var rowsTreningi = await ExecuteSqlAsync(sqlTreningi, new object[] { trenerId });

                foreach (var row in rowsTreningi)
                {
                    var start = DateTime.TryParse(row[2], out var d1) ? d1 : DateTime.MinValue;
                    var end = DateTime.TryParse(row[3], out var d2) ? d2 : DateTime.MinValue;
                    fullList.Add(new ElementKalendarza
                    {
                        Id = int.Parse(row[0] ?? "0"),
                        Tytul = $"Trening: {row[1]}", // np. Trening: Siłowy
                        Podtytul = row[4], // Miejsce
                        Typ = "TRENING",
                        Data = start,
                        GodzinaDisplay = $"{start:HH:mm}-{end:HH:mm}",
                        KolorTypu = Colors.Orange // Pomarańczowy dla treningów
                    });
                }

                // 3. Pobierz WYDARZENIA (INNE)
                // Zakładam kolumny: WydarzenieID, Nazwa, Miejsce, DataRozpoczecia, DataZakonczenia
                var sqlWydarzenia = "SELECT WydarzenieID, Nazwa, Miejsce, DataRozpoczecia, DataZakonczenia FROM Wydarzenie WHERE TrenerID = ? AND DataRozpoczecia >= date('now')";
                var rowsWydarzenia = await ExecuteSqlAsync(sqlWydarzenia, new object[] { trenerId });

                foreach (var row in rowsWydarzenia)
                {
                    var start = DateTime.TryParse(row[3], out var d1) ? d1 : DateTime.MinValue;
                    fullList.Add(new ElementKalendarza
                    {
                        Id = int.Parse(row[0] ?? "0"),
                        Tytul = row[1], // Nazwa
                        Podtytul = row[2], // Miejsce
                        Typ = "WYDARZENIE",
                        Data = start,
                        GodzinaDisplay = start.ToString("HH:mm"),
                        KolorTypu = Colors.Blue // Niebieski dla innych
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd pobierania kalendarza: {ex.Message}");
            }

            // Sortowanie po dacie rosnąco
            return fullList.OrderBy(x => x.Data).ToList();
        }

        // --- METODA DLA PULPITU (NAJBLIŻSZY MECZ) ---
        public async Task<Mecz?> GetNextMatchAsync(int trenerId)
        {
            // Pobieramy najbliższy mecz
            var sql = "SELECT MeczID, SkladMeczowyID, Przeciwnik, DataRozpoczecia, Miejsce FROM Mecz WHERE TrenerID = ? AND DataRozpoczecia >= date('now') ORDER BY DataRozpoczecia ASC LIMIT 1";

            var rows = await ExecuteSqlAsync(sql, new object[] { trenerId });
            var row = rows.FirstOrDefault();

            if (row == null) return null;

            return new Mecz
            {
                MeczID = int.Parse(row[0] ?? "0"),
                SkladMeczowyID = int.Parse(row[1] ?? "0"),
                Przeciwnik = row[2],
                DataRozpoczecia = DateTime.TryParse(row[3], out var d) ? d : DateTime.MinValue,
                Miejsce = row[4],
                TrenerID = trenerId
            };
        }

        // --- HTTP HELPER (BEZ ZMIAN - SKOPIUJ Z POPRZEDNIEJ WERSJI) ---
        // Pamiętaj, aby wkleić tu metodę ExecuteSqlAsync i klasy pomocnicze Turso...
        // Jeśli ich tu nie wkleję, pamiętaj aby ich nie usuwać z pliku!

        private async Task<List<string?[]>> ExecuteSqlAsync(string sql, object[] args)
        {
            var endpoint = $"{DatabaseUrl.TrimEnd('/')}/v2/pipeline";

            var tursoArgs = args.Select(a => new TursoArg
            {
                Type = a is int || a is long ? "integer" : "text",
                Value = a?.ToString()
            }).ToArray();

            var requestPayload = new TursoPipelineRequest
            {
                Requests = new[] { new TursoRequest { Type = "execute", Stmt = new TursoStatement { Sql = sql, Args = tursoArgs } } }
            };

            var response = await _httpClient.PostAsJsonAsync(endpoint, requestPayload);
            if (!response.IsSuccessStatusCode) throw new Exception(await response.Content.ReadAsStringAsync());

            var resultData = await response.Content.ReadFromJsonAsync<TursoPipelineResponse>();
            var resultRows = new List<string?[]>();

            if (resultData?.Results != null)
            {
                foreach (var res in resultData.Results)
                {
                    if (res.Response?.Result?.Rows != null)
                    {
                        foreach (var row in res.Response.Result.Rows)
                            resultRows.Add(row.Select(col => col.Value?.ToString()).ToArray());
                    }
                }
            }
            return resultRows;
        }

        // Klasy DTO (TursoPipelineRequest itp.) - muszą tu być!
        private class TursoPipelineRequest { [JsonPropertyName("requests")] public TursoRequest[] Requests { get; set; } }
        private class TursoRequest { [JsonPropertyName("type")] public string Type { get; set; } [JsonPropertyName("stmt")] public TursoStatement Stmt { get; set; } }
        private class TursoStatement { [JsonPropertyName("sql")] public string Sql { get; set; } [JsonPropertyName("args")] public TursoArg[] Args { get; set; } }
        private class TursoArg { [JsonPropertyName("type")] public string Type { get; set; } [JsonPropertyName("value")] public string? Value { get; set; } }
        private class TursoPipelineResponse { [JsonPropertyName("results")] public List<TursoResultItem> Results { get; set; } }
        private class TursoResultItem { [JsonPropertyName("response")] public TursoResponseDetail Response { get; set; } }
        private class TursoResponseDetail { [JsonPropertyName("result")] public TursoQueryResult Result { get; set; } }
        private class TursoQueryResult { [JsonPropertyName("rows")] public List<List<TursoValue>> Rows { get; set; } }
        private class TursoValue { [JsonPropertyName("value")] public object? Value { get; set; } }
    }
}