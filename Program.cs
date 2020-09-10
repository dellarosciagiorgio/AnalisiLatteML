using ClosedXML.Excel;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LatteMarcheML
{
    public class DataPoint
    {
        public double[] Features { get; set; }
    }

    public class DataPointVector
    {
        [VectorType(5)]
        public string[] Features { get; set; }
    }

    public class Program
    {
        public const string percorsoFileExcel = @"C:\Users\Giorgio Della Roscia\source\repos\LatteMarcheML\Data\Analisi latte.xlsx";
        static void Main(string[] args)
        {
            Console.SetWindowSize(150, 60);
            var fileExcel = new XLWorkbook(percorsoFileExcel);
            var foglioExcelAnalisi = fileExcel.Worksheet("Analisi");
            var listaAnalisiLatte = PrendiDatiFoglioAnalisi(foglioExcelAnalisi);
            var datiProduttori = PrendiDati(listaAnalisiLatte);
            //StampaDatiSingoloProduttore(datiProduttori);
            var datiInteressati = PrendiDatiInteressati(datiProduttori);
            var contenutoCsv = TrasformaDati(datiInteressati);
            var analisiML = TrasformaProduttoreToAnalisiML(datiProduttori);
            //CreaFileCsv(contenutoCsv);
            //PrevisioniML(contenutoCsv);
            Console.WriteLine("FINE");
            Console.ReadLine();
        }

        private static List<AnalisiML> TrasformaProduttoreToAnalisiML(List<Produttore> datiProduttori)
        {
            List<AnalisiML> listaAnalisiML = new List<AnalisiML>();
            foreach (var produttore in datiProduttori)
            {
                foreach (var analisi in produttore.Analisi)
                {
                    var analisiML = new AnalisiML();
                    analisiML.NomeProduttore = produttore.Nome;
                    //analisiML.GrassoPerCalcolo = float.Parse(analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore.Value)
                        );

                    //foreach (var valori in analisi.Valori)
                    //{

                    //}
                }
            }
            throw new NotImplementedException();
        }

        private static List<AnalisiLatteExcel> PrendiDatiFoglioAnalisi(IXLWorksheet foglioExcelAnalisi)
        {
            var tabellaAnalisi = foglioExcelAnalisi.Tables.First();
            var listaAnalisiLatte = tabellaAnalisi.DataRange.Rows().Select(riga => new AnalisiLatteExcel()
            {
                Campione = riga.Field("CAMPIONE").GetString().Trim(),
                CodiceProduttore = riga.Field("CODICE_PRODUTTORE").GetString().Trim(),
                NomeProduttore = riga.Field("NOME_PRODUTTORE").GetString().Trim(),
                IdProduttore = riga.Field("ID_PRODUTTORE").GetString().Trim(),
                IdAllevamento = riga.Field("ID_ALLEVAMENTO").GetString().Trim(),
                CodiceAsl = riga.Field("CODICE_ASL").GetString().Trim(),
                TipoLatte = riga.Field("TIPO_LATTE").GetString().Trim(),
                IdTipoLatte = riga.Field("ID_TIPO_LATTE").GetString().Trim(),
                DataRapportoDiProva = riga.Field("DATA_RAPPORTO_DI_PROVA").DataType == XLDataType.DateTime ? riga.Field("DATA_RAPPORTO_DI_PROVA").GetDateTime().Date : (DateTime?)null,
                DataAccettazione = riga.Field("DATA_ACCETTAZIONE").DataType == XLDataType.DateTime ? riga.Field("DATA_ACCETTAZIONE").GetDateTime().Date : (DateTime?)null,
                DataPrelievo = riga.Field("DATA_PRELIEVO").DataType == XLDataType.DateTime ? riga.Field("DATA_PRELIEVO").GetDateTime().Date : (DateTime?)null
            }).ToList();
            return listaAnalisiLatte;
        }

        private static List<ValorePrelievo> PrendiDatiFoglioValori(IXLWorksheet foglioExcelValori)
        {
            var tabellaValori = foglioExcelValori.Tables.First();
            var listaValoriPrelievi = tabellaValori.DataRange.Rows().Select(riga => new ValorePrelievo()
            {
                Id = riga.Field("ID").GetString().Trim(),
                Nome = riga.Field("NOME").GetString().Trim(),
                Uom = riga.Field("UOM").GetString().Trim(),
                Valore = riga.Field("VALORE").IsEmpty() ? (Double?)null :
                         (riga.Field("VALORE").DataType == XLDataType.Text) ? ((String.Compare(riga.Field("VALORE").GetString().Trim(), "assenti") == 0) ? (Double?)null :
                         Double.Parse(riga.Field("VALORE").GetString().Trim())) :
                         ((riga.Field("VALORE").DataType == XLDataType.Number) ? riga.Field("VALORE").GetDouble() :
                         (Double?)null),
                FuoriSoglia = riga.Field("VALORE").IsEmpty() ? (Boolean?)null :
                         (riga.Field("VALORE").DataType == XLDataType.Text) ? ((String.Compare(riga.Field("VALORE").GetString().Trim(), "assenti") == 0) ? (Boolean?)null :
                         Convert.ToBoolean(riga.Field("FUORI_SOGLIA").GetValue<Int32>())) :
                         ((riga.Field("VALORE").DataType == XLDataType.Number) ? Convert.ToBoolean(riga.Field("FUORI_SOGLIA").GetValue<Int32>()) :
                         (Boolean?)null),
                AnalisiId = riga.Field("Analisi_Id").GetString().Trim(),
            }).ToList();
            return listaValoriPrelievi;
        }

        private static List<Produttore> PrendiDati(List<AnalisiLatteExcel> listaAnalisiLatte)
        {
            var fileExcel = new XLWorkbook(percorsoFileExcel);
            var foglioExcelValori = fileExcel.Worksheet("Valori");
            var listaValoriPrelievi = PrendiDatiFoglioValori(foglioExcelValori);
            foreach (var riga in listaValoriPrelievi)
            {
                var indice = listaAnalisiLatte.FindIndex(analisi => analisi.Campione == riga.AnalisiId);
                listaAnalisiLatte[indice].Valori.Add(riga);
            }
            var datiProduttori = PrendiDatiSingoloProduttore(listaAnalisiLatte);
            return datiProduttori;
        }

        private static List<Produttore> PrendiDatiSingoloProduttore(List<AnalisiLatteExcel> listaAnalisiLatte)
        {
            var datiProduttori = new List<Produttore>();
            foreach (var rowAnalisi in listaAnalisiLatte)
            {
                if (!datiProduttori.Exists(p => p.Id == rowAnalisi.IdProduttore))
                {
                    datiProduttori.Add(new Produttore()
                    {
                        Nome = rowAnalisi.NomeProduttore,
                        Codice = rowAnalisi.CodiceProduttore,
                        Id = rowAnalisi.IdProduttore,
                        IdAllevamento = rowAnalisi.IdAllevamento,
                        TipoLatte = rowAnalisi.TipoLatte,
                        IdTipoLatte = rowAnalisi.IdTipoLatte
                    });
                }

                datiProduttori[datiProduttori.FindIndex(i => i.Id == rowAnalisi.IdProduttore)].Analisi.Add(new AnalisiLatte()
                {
                    Campione = rowAnalisi.Campione,
                    DataRapportoDiProva = rowAnalisi.DataRapportoDiProva,
                    DataAccettazione = rowAnalisi.DataAccettazione,
                    DataPrelievo = rowAnalisi.DataPrelievo.ToString(),
                    Valori = rowAnalisi.Valori
                });
            }
            return datiProduttori;
        }

        private static void StampaDatiSingoloProduttore(List<Produttore> datiProduttori)
        {
            Console.Clear();
            input:;
            Console.Write("Inserire l'ID del produttore per visualizzarne i dati dei prelievi: ");
            var idProduttore = Console.ReadLine();
            Console.Clear();
            Produttore appoggioDatiProduttore = null;
            foreach (var datiProduttore in datiProduttori)
            {
                if (idProduttore == datiProduttore.Id)
                {
                    Console.WriteLine($"{datiProduttore.Nome}, ID: {datiProduttore.Id}, codice: {datiProduttore.Codice}, ID allevamento: {datiProduttore.IdAllevamento}, tipo latte: {datiProduttore.TipoLatte}, ID tipo latte: {datiProduttore.IdTipoLatte}.\n");
                    int indiceAnalisi, indiceValori;
                    datiProduttore.Analisi.ForEach(analisi =>
                    {
                        indiceAnalisi = datiProduttore.Analisi.IndexOf(analisi);
                        Console.WriteLine($"\n\nCampione: {analisi.Campione}, data rapporto di prova: {analisi.DataRapportoDiProva.Value:dd/MM/yyyy}, data accettazione: {analisi.DataAccettazione.Value:dd/MM/yyyy}, data prelievo: {analisi.DataPrelievo.Substring(0, 10)}\n");
                        datiProduttore.Analisi[indiceAnalisi].Valori.ForEach(valori =>
                        {
                            string fuorisoglia = "no";
                            indiceValori = datiProduttore.Analisi[indiceAnalisi].Valori.IndexOf(valori);
                            if (datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].FuoriSoglia == true)
                            {
                                fuorisoglia = "sì";
                            }
                            Console.WriteLine($"Analisi ID: {datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Id}, ID: {datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Id}, {datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Nome}: {datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Valore} {datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Uom}, fuori soglia: {fuorisoglia}.");
                        });
                    });
                    appoggioDatiProduttore = datiProduttore;
                }
            }
            if (appoggioDatiProduttore == null)
            {
                Console.Clear();
                Console.Write("ID non valido, riprova.\n\n");
                goto input;
            }
            Console.ReadLine();
        }

        private static List<string> PrendiDatiInteressati(List<Produttore> datiProduttori)
        {
            List<string> datiInteressati = new List<string>();
            string intestazione = "Campione,NomeProduttore,IdProduttore,NomeValore,Valore,FuoriSoglia,DataPrelievo"; //TODO: nel csv inserire la data completa (unendo contenutoCsv), quella divisa in dd mm yyyy la andrò a prendere dalla lista "contenutoCsv"
            datiInteressati.Add(intestazione);
            foreach (var datiProduttore in datiProduttori)
            {
                int indiceAnalisi, indiceValori;
                datiProduttore.Analisi.ForEach(analisi =>
                {
                    indiceAnalisi = datiProduttore.Analisi.IndexOf(analisi);
                    datiProduttore.Analisi[indiceAnalisi].Valori.ForEach(valori =>
                    {
                        indiceValori = datiProduttore.Analisi[indiceAnalisi].Valori.IndexOf(valori);
                        datiInteressati.Add($"{datiProduttore.Analisi[indiceAnalisi].Campione}," +
                            $"{datiProduttore.Nome}," +
                            $"{datiProduttore.Id}," +
                            $"{datiProduttore.Analisi[indiceAnalisi].DataPrelievo.ToString()}, " +
                            $"{datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Nome}," +
                            $"{datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].Valore.ToString().Replace(',', '.')}," +
                            $"{datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].FuoriSoglia.ToString()}");
                    });
                });
            }
            return datiInteressati;
        }

        private static List<AnalisiML> TrasformaDati(List<string> datiInteressati)
        {
            var contenutoCsv = new List<AnalisiML>();
            bool rigaIntestazione = true;
            foreach (var rigaDatiInteressati in datiInteressati)
            {
                if (rigaIntestazione == true)
                {
                    rigaIntestazione = false;
                }
                else
                {
                    var data = rigaDatiInteressati.Split(',');
                    contenutoCsv.Add(new AnalisiML()
                    {
                        Campione = data[0],
                        NomeProduttore = data[1],
                        IdProduttore = data[2],
                        NomeValore = data[4],
                        Valore = data[5] == "" ? 0f : float.Parse(data[5]),
                        FuoriSoglia = data[6] == "true" ? 1f : 0f,
                        Giorno = data[3] == "" ? 0f : float.Parse(data[3].Trim().Substring(0, 2)),
                        Mese = data[3] == "" ? 0f : float.Parse(data[3].Trim().Substring(3, 2)),
                        Anno = data[3] == "" ? 0f : float.Parse(data[3].Trim().Substring(6, 4))
                    });
                }
            }
            return contenutoCsv;
        }

        private static void CreaFileCsv(List<AnalisiML> contenutoCsv)
        {
            /*const string percorsoFileCsv = @"C:\Users\Giorgio Della Roscia\source\repos\LatteMarcheML\Data\Dati Produttori.csv";
            string intestazione = "Campione,NomeProduttore,IdProduttore,NomeValore,Valore,FuoriSoglia,DataPrelievo";
            k:;
            if (!File.Exists(percorsoFileCsv))
            {
                var processoFileCsv = File.Create(percorsoFileCsv);
                processoFileCsv.Close();
                File.WriteAllText(percorsoFileCsv, intestazione);
                foreach (var riga in contenutoCsv)
                {
                    File.WriteAllText(percorsoFileCsv, riga);
                }
            }
            else
            {
                File.Delete(percorsoFileCsv);
                goto k;
            }
            Console.Clear();
            Console.WriteLine("\n******************** DATI PRODUTTORI ********************\n\n");
            var datiLettiDalCsv = File.ReadAllText(percorsoFileCsv);
            Console.WriteLine(datiLettiDalCsv);
            Console.ReadLine();*/
        }

        private static void PrevisioniML(List<AnalisiML> contenutoCsv)
        {
            
        }
    }
}