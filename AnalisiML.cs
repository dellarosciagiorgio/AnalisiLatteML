using System;
using System.Collections.Generic;
using System.Text;

namespace LatteMarcheML
{
    class AnalisiML
    {
        public string Campione { get; set; }
        public string NomeProduttore { get; set; }
        public string IdProduttore { get; set; }
        public string NomeValore { get; set; }
        public float GrassoPerCalcolo { get; set; }
        public float GrassoPerCalcoloFuoriSoglia { get; set; }
        public float? Valore { get; set; }
        public float? FuoriSoglia { get; set; }
        public float? Giorno { get; set; }
        public float? Mese { get; set; }
        public float? Anno { get; set; }
    }
}