using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerLinkPlayer.Models
{
    public class Mecz
    {
        public int MeczID { get; set; }
        public int SkladMeczowyID { get; set; }
        public string Przeciwnik { get; set; }

        // ZMIANA: Rozbicie na datę i godzinę (tekst)
        public string Data { get; set; }                // np. "15.05.2024"
        public string GodzinaRozpoczecia { get; set; }  // np. "14:00"

        public string Miejsce { get; set; }
        public int TrenerID { get; set; }

        // Właściwości pomocnicze do wyświetlania
        public string DataDisplay => Data;
        public string GodzinaDisplay => GodzinaRozpoczecia;

        // Pomocnicze pole do sortowania/logiki (nie z bazy)
        public DateTime DataRozpoczeciaPelna { get; set; }
    }
}
