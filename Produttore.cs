using System;
using System.Collections.Generic;
using System.Text;

namespace LatteMarcheML
{
    public class Produttore
    {
        public string Codice { get; set; }
        public string Nome { get; set; }
        public string Id { get; set; }
        public string IdAllevamento { get; set; }
        public string TipoLatte { get; set; }
        public string IdTipoLatte { get; set; }
        public List<AnalisiLatte> Analisi { get; set; }
        public Produttore()
        {
            Analisi = new List<AnalisiLatte>();
        }
    }
}