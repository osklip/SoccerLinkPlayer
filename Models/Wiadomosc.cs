using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerLinkPlayer.Models
{
    public class Wiadomosc
    {
        public int WiadomoscID { get; set; }
        public string TypNadawcy { get; set; } // Oczekujemy wartości "Trener"
        public int NadawcaID { get; set; }     // ID Trenera
        public int OdbiorcaID { get; set; }    // ID Zawodnika (zalogowanego)
        public string Tresc { get; set; }
        public DateTime DataWyslania { get; set; }
        public string Temat { get; set; }
    }
}
