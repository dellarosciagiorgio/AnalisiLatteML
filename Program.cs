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
    class Program
    {
        public const string xlsfilepath = @"C:\Users\Giorgio Della Roscia\Desktop\ML\Progetti\Database\Analisi latte.xlsx";
        static void Main(string[] args)
        {
            var workbook = new XLWorkbook(xlsfilepath);
            var sheet1 = workbook.Worksheet("Analisi");
            var sheet2 = workbook.Worksheet("Valori");
            var listaAnalisiLatte = GetSheet1Data(sheet1);
            var listaValoriPrelievi = GetSheet2Data(sheet2);
            List<ValorePrelievo> valori = listaValoriPrelievi;
            AssociaDati(valori, listaAnalisiLatte);
        }

        private static List<AnalisiLatte> GetSheet1Data(IXLWorksheet sheet1)
        {
            var table1 = sheet1.Tables.First();
            var list1 = table1.DataRange.Rows().Select(r => new AnalisiLatte()
            {
                Campione = r.Field("CAMPIONE").GetString().Trim(),
                CodiceProduttore = r.Field("CODICE_PRODUTTORE").GetString().Trim(),
                NomeProduttore = r.Field("NOME_PRODUTTORE").GetString().Trim(),
                IdProduttore = r.Field("ID_PRODUTTORE").GetString().Trim(),
                IdAllevamento = r.Field("ID_ALLEVAMENTO").GetString().Trim(),
                CodiceAsl = r.Field("CODICE_ASL").GetString().Trim(),
                TipoLatte = r.Field("TIPO_LATTE").GetString().Trim(),
                IdTipoLatte = r.Field("ID_TIPO_LATTE").GetString().Trim(),
                DataRapportoDiProva = r.Field("DATA_RAPPORTO_DI_PROVA").DataType == XLDataType.DateTime ? r.Field("DATA_RAPPORTO_DI_PROVA").GetDateTime() : (DateTime?)null,
                DataAccettazione = r.Field("DATA_ACCETTAZIONE").DataType == XLDataType.DateTime ? r.Field("DATA_ACCETTAZIONE").GetDateTime() : (DateTime?)null,
                DataPrelievo = r.Field("DATA_PRELIEVO").DataType == XLDataType.DateTime ? r.Field("DATA_PRELIEVO").GetDateTime() : (DateTime?)null
            }).ToList();
            return list1;
        }

        private static List<ValorePrelievo> GetSheet2Data(IXLWorksheet sheet2)
        {
            var table2 = sheet2.Tables.First();
            var list2 = table2.DataRange.Rows().Select(r => new ValorePrelievo()
            {
                Id = r.Field("ID").GetString().Trim(),
                Nome = r.Field("NOME").GetString().Trim(),
                Uom = r.Field("UOM").GetString().Trim(),
                Valore = r.Field("VALORE").IsEmpty() ? (Double?)null :
                         (r.Field("VALORE").DataType == XLDataType.Text) ? ((String.Compare(r.Field("VALORE").GetString().Trim(), "assenti") == 0) ? (Double?)null :
                         Double.Parse(r.Field("VALORE").GetString().Trim())) :
                         ((r.Field("VALORE").DataType == XLDataType.Number) ? r.Field("VALORE").GetDouble() :
                         (Double?)null),
                FuoriSoglia = r.Field("VALORE").IsEmpty() ? (Boolean?)null :
                         (r.Field("VALORE").DataType == XLDataType.Text) ? ((String.Compare(r.Field("VALORE").GetString().Trim(), "assenti") == 0) ? (Boolean?)null :
                         Convert.ToBoolean(r.Field("FUORI_SOGLIA").GetValue<Int32>())) :
                         ((r.Field("VALORE").DataType == XLDataType.Number) ? Convert.ToBoolean(r.Field("FUORI_SOGLIA").GetValue<Int32>()) :
                         (Boolean?)null),
                AnalisiId = r.Field("Analisi_Id").GetString().Trim()
            }).ToList();
            return list2;
        }

        static void AssociaDati(List<ValorePrelievo> valori, List<AnalisiLatte> listaAnalisiLatte)
        {
            foreach (var rowValore in valori)
            {
                var index = listaAnalisiLatte.FindIndex(a => a.Campione == rowValore.AnalisiId);
                listaAnalisiLatte[index].Valori.Add(rowValore);
                Console.WriteLine($"{listaAnalisiLatte[index].Valori.LastOrDefault().Nome}: {listaAnalisiLatte[index].Valori.LastOrDefault().Valore}");
            }
            //Valori.Add(new ValorePrelievo() { list2[i].Id, list2[i].Nome, list2[i].Uom, list2[i].Valore, list2[i].FuoriSoglia, list2[i].AnalisiId });
            //listaAnalisiLatte.Find(a => a.NomeProduttore == "S.A.M.").Valori.ForEach(v => Console.WriteLine($"{v.Nome}: {v.Valore}"));
            return;
        }
    }
}