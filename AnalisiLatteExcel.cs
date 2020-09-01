using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML;
using ClosedXML.Excel;

namespace LatteMarcheML
{
    public class AnalisiLatteExcel
    {
        public string Campione { get; set; }
        public string CodiceProduttore { get; set; }
        public string NomeProduttore { get; set; }
        public string IdProduttore { get; set; }
        public string IdAllevamento { get; set; }
        public string CodiceAsl { get; set; }
        public string TipoLatte { get; set; }
        public string IdTipoLatte { get; set; }
        public DateTime? DataRapportoDiProva { get; set; }
        public DateTime? DataAccettazione { get; set; }
        public DateTime? DataPrelievo { get; set; }
        public List<ValorePrelievo> Valori { get; set; }
        public List<Produttore> DatiProduttore { get; set; }
        public AnalisiLatteExcel()
        {
            Valori = new List<ValorePrelievo>();
        }
    }
}