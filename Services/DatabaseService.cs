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
            // Konfiguracja nagłówka autoryzacji dla wszystkich zapytań
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);
        }

        public async Task InitializeAsync()
        {
            // W przypadku HTTP nie musimy utrzymywać stałego połączenia,
            // więc ta metoda może być pusta lub sprawdzać dostęp do sieci.
            await Task.CompletedTask;
        }

        public async Task<Zawodnik?> LoginAsync(string email, string haslo)
        {
            var sql = @"
            SELECT 
                ZawodnikID, 
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
                // Wykonujemy zapytanie przez naszą metodę pomocniczą HTTP
                var rows = await ExecuteSqlAsync(sql, new object[] { email, haslo });

                var row = rows.FirstOrDefault();

                if (row == null) return null;

                // Mapowanie wyników (row to teraz tablica stringów/nulli)
                return new Zawodnik
                {
                    ZawodnikID = int.Parse(row[0] ?? "0"),
                    AdresEmail = row[1],
                    Haslo = row[2],
                    NumerTelefonu = row[3],
                    Imie = row[4],
                    Nazwisko = row[5],
                    DataUrodzenia = DateOnly.TryParse(row[6], out var date) ? date : DateOnly.FromDateTime(DateTime.MinValue),
                    Pozycja = row[7],
                    NumerKoszulki = int.Parse(row[8] ?? "0"),
                    CzyDyspozycyjny = int.Parse(row[9] ?? "0"),
                    LepszaNoga = row[10],
                    ProbyLogowania = int.Parse(row[11] ?? "0"),
                    TrenerID = int.Parse(row[12] ?? "0")
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd bazy danych (HTTP): {ex.Message}");
                throw; // Rzucamy dalej, żebyś widział błąd w UI jeśli wystąpi
            }
        }

        // --- Metody pomocnicze do obsługi Turso HTTP API ---

        private async Task<List<string?[]>> ExecuteSqlAsync(string sql, object[] args)
        {
            var endpoint = $"{DatabaseUrl.TrimEnd('/')}/v2/pipeline";

            // Konwersja argumentów na format rozumiany przez Turso
            var tursoArgs = args.Select(a => new TursoArg
            {
                Type = a is int || a is long ? "integer" : "text",
                Value = a?.ToString()
            }).ToArray();

            // Budowa obiektu żądania
            var requestPayload = new TursoPipelineRequest
            {
                Requests = new[]
                {
                new TursoRequest
                {
                    Type = "execute",
                    Stmt = new TursoStatement { Sql = sql, Args = tursoArgs }
                }
            }
            };

            var response = await _httpClient.PostAsJsonAsync(endpoint, requestPayload);
            var jsonContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"ODPOWIEDŹ Z TURSO: {jsonContent}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Błąd HTTP {response.StatusCode}: {errorContent}");
            }

            var resultData = await response.Content.ReadFromJsonAsync<TursoPipelineResponse>();

            // Wyciąganie danych z zagnieżdżonego JSONa Turso
            // Struktura: results -> response -> result -> rows -> row -> value
            var rows = new List<string?[]>();

            if (resultData?.Results != null)
            {
                foreach (var res in resultData.Results)
                {
                    if (res.Response?.Result?.Rows != null)
                    {
                        foreach (var row in res.Response.Result.Rows)
                        {
                            // Konwertujemy każdy wiersz na tablicę stringów
                            var cleanRow = row.Select(col => col.Value?.ToString()).ToArray();
                            rows.Add(cleanRow);
                        }
                    }
                    // Obsługa błędów zwróconych przez bazę (np. błąd SQL)
                    if (res.Response?.Error != null)
                    {
                        throw new Exception($"Błąd SQL z Turso: {res.Response.Error.Message}");
                    }
                }
            }

            return rows;
        }

        // --- Klasy DTO do mapowania JSON z Turso API ---

        private class TursoPipelineRequest
        {
            [JsonPropertyName("requests")]
            public TursoRequest[] Requests { get; set; }
        }

        private class TursoRequest
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("stmt")]
            public TursoStatement Stmt { get; set; }
        }

        private class TursoStatement
        {
            [JsonPropertyName("sql")]
            public string Sql { get; set; }
            [JsonPropertyName("args")]
            public TursoArg[] Args { get; set; }
        }

        private class TursoArg
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("value")]
            public string? Value { get; set; }
        }

        private class TursoPipelineResponse
        {
            [JsonPropertyName("results")]
            public List<TursoResultItem> Results { get; set; }
        }

        private class TursoResultItem
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("response")]
            public TursoResponseDetail Response { get; set; }
        }

        private class TursoResponseDetail
        {
            [JsonPropertyName("result")]
            public TursoQueryResult Result { get; set; }
            [JsonPropertyName("error")]
            public TursoError Error { get; set; }
        }

        private class TursoError
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        private class TursoQueryResult
        {
            [JsonPropertyName("rows")]
            public List<List<TursoValue>> Rows { get; set; }
        }

        private class TursoValue
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("value")]
            public object? Value { get; set; } // Może być stringiem lub liczbą w JSON
        }
    }
}