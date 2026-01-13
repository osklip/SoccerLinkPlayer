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
        public int SkladMeczowyID { get; set; } // Ważne pole do powiązania ze składem
        public string Przeciwnik { get; set; }
        public DateTime DataRozpoczecia { get; set; }
        public string Miejsce { get; set; }
        public int TrenerID { get; set; }

        // Właściwości pomocnicze
        public string DataDisplay => DataRozpoczecia.ToString("dd.MM.yyyy");
        public string GodzinaDisplay => DataRozpoczecia.ToString("HH:mm");

    }
}
