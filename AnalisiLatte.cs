using System;
using System.Collections.Generic;
using System.Text;

namespace LatteMarcheML
{
    public class AnalisiLatte
    {
        public string Campione { get; set; }
        public DateTime? DataRapportoDiProva { get; set; }
        public DateTime? DataAccettazione { get; set; }
        public DateTime? DataPrelievo { get; set; }
        public List<ValorePrelievo> Valori { get; set; }
        public AnalisiLatte()
        {
            Valori = new List<ValorePrelievo>();
        }
    }
}