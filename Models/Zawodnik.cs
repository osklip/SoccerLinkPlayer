namespace SoccerLinkPlayer.Models;

public class Zawodnik
{
    public int Id { get; set; }
    public string AdresEmail { get; set; }
    public string Haslo { get; set; }
    public string NumerTelefonu { get; set; }
    public string Imie { get; set; }
    public string Nazwisko { get; set; }
    public DateOnly DataUrodzenia { get; set; }

    public string Pozycja { get; set; }
    public int NumerKoszulki { get; set; }
    public int CzyDyspozycyjny { get; set; }
    public string LepszaNoga { get; set; }
    public int ProbyLogowania   { get; set; }
    public int TrenerID { get; set; }
}