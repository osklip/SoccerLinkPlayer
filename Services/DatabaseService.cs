using SoccerLinkPlayer.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;

namespace SoccerLinkPlayer.Services
{
    public class DatabaseService
    {
        private const string DatabaseUrl = "https://soccerlinkdb-enbixd.aws-eu-west-1.turso.io";
        private const string AuthToken = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ9.eyJhIjoicnciLCJnaWQiOiJjYTliNTRlNy0zMGNkLTQwOWEtOGEzMy03MjJkZmQxYmFmNGIiLCJpYXQiOjE3NjUyNzg0NjQsInJpZCI6ImEwYjU0YzNmLWZmZGMtNDIyMi1iNmExLWRkYWU3MTdiNTJmOCJ9.m7-Qj3ltdSqebDrDH4Q_c96gAbb6Bqs9xknYfDsodpwEkEslNRGAQ6N10XemtTxRvF9p4S1fzShGjWBRGvcCCQ";

        private readonly HttpClient _httpClient;

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

        // --- LOGOWANIE ---
        public async Task<Zawodnik?> LoginAsync(string email, string haslo)
        {
            var sql = @"SELECT ZawodnikID, AdresEmail, Haslo, NumerTelefonu, Imie, Nazwisko, DataUrodzenia, Pozycja, NumerKoszulki, CzyDyspozycyjny, LepszaNoga, ProbyLogowania, TrenerID FROM Zawodnik WHERE AdresEmail = ? AND Haslo = ? LIMIT 1";

            try
            {
                var rows = await ExecuteSqlAsync(sql, new object[] { email, haslo });
                var row = rows.FirstOrDefault();

                if (row == null)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG LOGOWANIE] Nie znaleziono użytkownika w bazie.");
                    return null;
                }

                var z = new Zawodnik
                {
                    ZawodnikID = int.Parse(row[0] ?? "0"),
                    AdresEmail = row[1],
                    Haslo = row[2],
                    NumerTelefonu = row[3],
                    Imie = row[4],
                    Nazwisko = row[5],
                    TrenerID = int.Parse(row[12] ?? "0")
                };

                System.Diagnostics.Debug.WriteLine($"[DEBUG LOGOWANIE] Zalogowano: {z.Imie} {z.Nazwisko}, TrenerID: {z.TrenerID}");
                return z;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG LOGOWANIE BŁĄD]: {ex.Message}");
                throw;
            }
        }

        // --- WIADOMOŚCI ---
        public async Task<List<Wiadomosc>> GetMessagesForPlayerAsync(int playerId)
        {
            var sql = "SELECT WiadomoscID, TypNadawcy, NadawcaID, OdbiorcaID, Tresc, DataWyslania, Temat FROM Wiadomosc WHERE OdbiorcaID = ? ORDER BY DataWyslania DESC";

            try
            {
                var rows = await ExecuteSqlAsync(sql, new object[] { playerId });
                var list = new List<Wiadomosc>();

                foreach (var row in rows)
                {
                    string typ = row[1]?.ToString() ?? "";
                    if (!typ.Equals("Trener", StringComparison.OrdinalIgnoreCase)) continue;

                    list.Add(new Wiadomosc
                    {
                        WiadomoscID = int.Parse(row[0] ?? "0"),
                        TypNadawcy = typ,
                        NadawcaID = int.Parse(row[2] ?? "0"),
                        OdbiorcaID = int.Parse(row[3] ?? "0"),
                        Tresc = row[4],
                        DataWyslania = ParseDate(row[5]),
                        Temat = row[6]
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG WIADOMOŚCI BŁĄD]: {ex.Message}");
                return new List<Wiadomosc>();
            }
        }

        // --- POBIERANIE DANYCH DO KALENDARZA ---
        public async Task<List<ElementKalendarza>> GetFullCalendarAsync(int trenerId)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG KALENDARZ] Pobieram dane i filtruję w C# dla Trenera ID: {trenerId}");
            var fullList = new List<ElementKalendarza>();

            try
            {
                // 1. MECZE (Pobieramy wszystkie, filtrujemy w pętli)
                // Dodano TrenerID na końcu SELECT (indeks 5)
                var sqlMecze = "SELECT MeczID, Przeciwnik, Data, GodzinaRozpoczecia, Miejsce, TrenerID FROM Mecz";
                var rowsMecze = await ExecuteSqlAsync(sqlMecze, new object[] { });

                foreach (var row in rowsMecze)
                {
                    // FILTROWANIE PO STRONIE APLIKACJI
                    int dbTrenerId = int.Parse(row[5] ?? "0");
                    if (dbTrenerId != trenerId) continue;

                    string dataStr = row[2];
                    string godzStr = row[3];
                    var data = ParseDate($"{dataStr} {godzStr}");
                    if (data == DateTime.MinValue) data = ParseDate(dataStr);

                    if (data.Date < DateTime.Now.Date) continue;

                    fullList.Add(new ElementKalendarza
                    {
                        Id = int.Parse(row[0] ?? "0"),
                        Tytul = $"Mecz: {row[1]}",
                        Podtytul = row[4],
                        Typ = "MECZ",
                        Data = data,
                        GodzinaDisplay = godzStr,
                        KolorTypu = Colors.Green
                    });
                }

                // 2. TRENINGI
                // Dodano TrenerID na końcu (indeks 6)
                var sqlTreningi = "SELECT TreningID, Typ, ListaObecnosciID Data, GodzinaRozpoczecia, GodzinaZakonczenia, Miejsce, TrenerID FROM Trening";
                var rowsTreningi = await ExecuteSqlAsync(sqlTreningi, new object[] { });

                foreach (var row in rowsTreningi)
                {
                    // FILTROWANIE
                    int dbTrenerId = int.Parse(row[6] ?? "0");
                    if (dbTrenerId != trenerId) continue;

                    string dataStr = row[2];
                    string godzStartStr = row[3];
                    string godzKoniecStr = row[4];
                    string miejsce = row[5];

                    var data = ParseDate($"{dataStr} {godzStartStr}");
                    if (data == DateTime.MinValue) data = ParseDate(dataStr);

                    if (data.Date < DateTime.Now.Date) continue;

                    fullList.Add(new ElementKalendarza
                    {
                        Id = int.Parse(row[0] ?? "0"),
                        Tytul = $"Trening: {row[1]}",
                        Podtytul = miejsce,
                        Typ = "TRENING",
                        Data = data,
                        GodzinaDisplay = $"{godzStartStr}-{godzKoniecStr}",
                        KolorTypu = Colors.Orange
                    });
                }

                // 3. WYDARZENIA
                // Dodano TrenerID na końcu (indeks 6)
                var sqlWydarzenia = "SELECT WydarzenieID, Nazwa, Miejsce, Data, GodzinaStart, GodzinaKoniec, Opis, TrenerID FROM Wydarzenie";
                var rowsWydarzenia = await ExecuteSqlAsync(sqlWydarzenia, new object[] { });

                foreach (var row in rowsWydarzenia)
                {
                    // FILTROWANIE
                    int dbTrenerId = int.Parse(row[6] ?? "0");
                    if (dbTrenerId != trenerId) continue;

                    string nazwa = row[1];
                    string miejsce = row[2];
                    string dataStr = row[3];
                    string godzStartStr = row[4];
                    string godzKoniecStr = row[5];

                    var data = ParseDate($"{dataStr} {godzStartStr}");
                    if (data == DateTime.MinValue) data = ParseDate(dataStr);

                    if (data.Date < DateTime.Now.Date) continue;

                    fullList.Add(new ElementKalendarza
                    {
                        Id = int.Parse(row[0] ?? "0"),
                        Tytul = nazwa,
                        Podtytul = miejsce,
                        Typ = "WYDARZENIE",
                        Data = data,
                        GodzinaDisplay = $"{godzStartStr}-{godzKoniecStr}",
                        KolorTypu = Colors.Blue
                    });
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG KALENDARZ BŁĄD]: {ex.Message}");
            }

            return fullList.OrderBy(x => x.Data).ToList();
        }

        // --- METODA DLA PULPITU (NAJBLIŻSZY MECZ) ---
        public async Task<Mecz?> GetNextMatchAsync(int trenerId)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG PULPIT] Pobieram WSZYSTKIE mecze i szukam ID: {trenerId}");

            // Pobieramy wszystko bez WHERE, dodajemy TrenerID do SELECT (indeks 6)
            var sql = "SELECT MeczID, SkladMeczowyID, Przeciwnik, Data, Godzina, Miejsce, TrenerID FROM Mecz";

            try
            {
                var rows = await ExecuteSqlAsync(sql, new object[] { });
                System.Diagnostics.Debug.WriteLine($"[DEBUG PULPIT] Pobranno łącznie {rows.Count} meczy z bazy (przed filtrowaniem).");

                var mecze = new List<Mecz>();

                foreach (var row in rows)
                {
                    // --- FILTROWANIE C# (Gwarancja działania) ---
                    // Pobieramy ID z bazy, parsujemy i porównujemy liczbami
                    int dbTrenerId = int.Parse(row[6] ?? "0");

                    if (dbTrenerId != trenerId)
                    {
                        continue; // To nie jest mecz naszego trenera
                    }
                    // ---------------------------------------------

                    string dataStr = row[3];
                    string godzStr = row[4];

                    var pelnaData = ParseDate($"{dataStr} {godzStr}");
                    if (pelnaData == DateTime.MinValue) pelnaData = ParseDate(dataStr);

                    mecze.Add(new Mecz
                    {
                        MeczID = int.Parse(row[0] ?? "0"),
                        SkladMeczowyID = int.Parse(row[1] ?? "0"),
                        Przeciwnik = row[2],
                        Data = dataStr,
                        GodzinaRozpoczecia = godzStr,
                        Miejsce = row[5],
                        TrenerID = dbTrenerId,
                        DataRozpoczeciaPelna = pelnaData
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG PULPIT] Po filtrowaniu zostało {mecze.Count} meczy dla trenera {trenerId}.");

                var nextMatch = mecze
                    .Where(m => m.DataRozpoczeciaPelna.Date >= DateTime.Now.Date)
                    .OrderBy(m => m.DataRozpoczeciaPelna)
                    .FirstOrDefault();

                if (nextMatch != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG PULPIT] Wybrano najbliższy: {nextMatch.Przeciwnik}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG PULPIT] Brak nadchodzących meczów.");
                }

                return nextMatch;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG PULPIT BŁĄD]: {ex.Message}");
                return null;
            }
        }

        private DateTime ParseDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return DateTime.MinValue;

            var culture = CultureInfo.InvariantCulture;
            DateTime result;

            if (DateTime.TryParseExact(dateString, "dd.MM.yyyy HH:mm", culture, DateTimeStyles.None, out result)) return result;
            if (DateTime.TryParseExact(dateString, "dd.MM.yyyy HH:mm:ss", culture, DateTimeStyles.None, out result)) return result;
            if (DateTime.TryParseExact(dateString, "dd.MM.yyyy", culture, DateTimeStyles.None, out result)) return result;
            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm", culture, DateTimeStyles.None, out result)) return result;
            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", culture, DateTimeStyles.None, out result)) return result;

            if (DateTime.TryParse(dateString, out result)) return result;

            System.Diagnostics.Debug.WriteLine($"[DEBUG DATE ERROR] Nie udało się sparsować daty: '{dateString}'");
            return DateTime.MinValue;
        }

        private async Task<List<string?[]>> ExecuteSqlAsync(string sql, object[] args)
        {
            var endpoint = $"{DatabaseUrl.TrimEnd('/')}/v2/pipeline";

            var tursoArgs = args.Select(a => new TursoArg { Type = a is int || a is long ? "integer" : "text", Value = a?.ToString() }).ToArray();
            var requestPayload = new TursoPipelineRequest { Requests = new[] { new TursoRequest { Type = "execute", Stmt = new TursoStatement { Sql = sql, Args = tursoArgs } } } };

            var response = await _httpClient.PostAsJsonAsync(endpoint, requestPayload);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG HTTP ERROR] Status: {response.StatusCode}, Content: {err}");
                throw new Exception(err);
            }

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
                    if (res.Response?.Error != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG SQL ERROR Z BAZY] {res.Response.Error.Message}");
                    }
                }
            }
            return resultRows;
        }

        // Klasy DTO
        private class TursoPipelineRequest { [JsonPropertyName("requests")] public TursoRequest[] Requests { get; set; } }
        private class TursoRequest { [JsonPropertyName("type")] public string Type { get; set; } [JsonPropertyName("stmt")] public TursoStatement Stmt { get; set; } }
        private class TursoStatement { [JsonPropertyName("sql")] public string Sql { get; set; } [JsonPropertyName("args")] public TursoArg[] Args { get; set; } }
        private class TursoArg { [JsonPropertyName("type")] public string Type { get; set; } [JsonPropertyName("value")] public string? Value { get; set; } }
        private class TursoPipelineResponse { [JsonPropertyName("results")] public List<TursoResultItem> Results { get; set; } }
        private class TursoResultItem { [JsonPropertyName("response")] public TursoResponseDetail Response { get; set; } }
        private class TursoResponseDetail { [JsonPropertyName("result")] public TursoQueryResult Result { get; set; } [JsonPropertyName("error")] public TursoError Error { get; set; } }
        private class TursoError { [JsonPropertyName("message")] public string Message { get; set; } }
        private class TursoQueryResult { [JsonPropertyName("rows")] public List<List<TursoValue>> Rows { get; set; } }
        private class TursoValue { [JsonPropertyName("value")] public object? Value { get; set; } }
    }
}