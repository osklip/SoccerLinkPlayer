using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace SoccerLinkPlayer.Models
{
    public class ElementKalendarza
    {
        public int Id { get; set; }
        public string Tytul { get; set; }       // Np. "vs Grom" (Mecz) lub "Trening Siłowy"
        public string Podtytul { get; set; }    // Np. Lokalizacja
        public string Typ { get; set; }         // "MECZ", "TRENING", "WYDARZENIE"
        public DateTime Data { get; set; }
        public string GodzinaDisplay { get; set; }

        // Kolor znacznika (opcjonalnie, dla lepszego wyglądu UI)
        public Color KolorTypu { get; set; }
    }
}
