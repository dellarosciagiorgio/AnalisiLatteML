using System;
using System.Collections.Generic;
using System.Text;

namespace LatteMarcheML
{
    public class ValorePrelievo
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Uom { get; set; }
        public float Valore { get; set; }
        public float FuoriSoglia { get; set; }
        public string AnalisiId { get; set; }
    }
}