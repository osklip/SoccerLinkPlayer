using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerLinkPlayer.Models
{
    public class Wydarzenie
    {
        public int WydarzenieID { get; set; }
        public string Nazwa { get; set; }
        public string Miejsce { get; set; }

        // ZMIANA: Pola odpowiadające kolumnom w Twojej bazie (tekstowe)
        public string Data { get; set; }                // np. "15.05.2024"
        public string GodzinaRozpoczecia { get; set; }  // np. "14:00"
        public string GodzinaZakonczenia { get; set; }  // np. "16:00"

        public string Opis { get; set; }
        public int TrenerID { get; set; }

        // Właściwości pomocnicze do wyświetlania
        public string DataDisplay => Data;
        public string GodzinaRangeDisplay => $"{GodzinaRozpoczecia} - {GodzinaZakonczenia}";
    }
}
