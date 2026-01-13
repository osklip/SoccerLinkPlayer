using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerLinkPlayer.Models
{
    public class Wydarzenie
    {
        public int WydarzenieID { get; set; } // Klucz główny
        public string Nazwa { get; set; }
        public string Miejsce { get; set; }
        public string Data { get; set; } // Przechowywane jako tekst w bazie (opcjonalne, jeśli używamy DataRozpoczecia)
        public DateTime DataRozpoczecia { get; set; }
        public DateTime DataZakonczenia { get; set; }
        public string Opis { get; set; }
        public int TrenerID { get; set; }

        // Właściwości pomocnicze do wyświetlania w widokach (nie są w bazie, ale ułatwiają bindowanie)
        public string DataDisplay => DataRozpoczecia.ToString("dd.MM.yyyy");
        public string GodzinaRangeDisplay => $"{DataRozpoczecia:HH:mm} - {DataZakonczenia:HH:mm}";
    }
}
