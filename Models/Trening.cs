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

        // ZMIANA: Pola odpowiadające kolumnom w Twojej bazie
        public string Data { get; set; }                // np. "15.05.2024" lub "2024-05-15"
        public string GodzinaRozpoczecia { get; set; }  // np. "18:00"
        public string GodzinaZakonczenia { get; set; }  // np. "19:30"

        public string Miejsce { get; set; }
        public int TrenerID { get; set; }

        // Właściwości pomocnicze do wyświetlania
        public string DataDisplay => Data;
        public string GodzinaRangeDisplay => $"{GodzinaRozpoczecia} - {GodzinaZakonczenia}";

    }
}
