using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerLinkPlayer.Models
{
    public class Trening
    {
        public int TreningID { get; set; }
        public string Typ { get; set; } // np. "Siłowy", "Techniczny"
        public int ListaObecnosciID { get; set; }
        public DateTime DataRozpoczecia { get; set; }
        public DateTime DataZakonczenia { get; set; }
        public string Miejsce { get; set; }
        public int TrenerID { get; set; }

        // Właściwości pomocnicze
        public string DataDisplay => DataRozpoczecia.ToString("dd.MM.yyyy");
        public string GodzinaRangeDisplay => $"{DataRozpoczecia:HH:mm} - {DataZakonczenia:HH:mm}";

    }
}
