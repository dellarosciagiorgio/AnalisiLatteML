using ClosedXML.Excel;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LatteMarcheML
{
    public class Previsioni
    {
        [ColumnName("VALORE")]
        public double Grassi { get; set; }
        public double Proteine { get; set; }
    }

    public class Program
    {
        public const string percorsoFileExcel = @"C:\Users\Giorgio Della Roscia\Desktop\ML\Progetti\Database\Analisi latte.xlsx";
        static void Main(string[] args)
        {
            Console.SetWindowSize(150, 50);
            var fileExcel = new XLWorkbook(percorsoFileExcel);
            var foglioExcelAnalisi = fileExcel.Worksheet("Analisi");
            var listaAnalisiLatte = PrendiDatiFoglioAnalisi(foglioExcelAnalisi);
            var datiProduttori = PrendiDati(listaAnalisiLatte);
            var datiProduttore = PrendiDatiSingoloProduttore(datiProduttori);
            PrevisioniML(datiProduttore);
            Console.ReadLine();
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
            var FileExcel = new XLWorkbook(percorsoFileExcel);
            var foglioExcelValori = FileExcel.Worksheet("Valori");
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
                    DataPrelievo = rowAnalisi.DataPrelievo,
                    Valori = rowAnalisi.Valori
                });
            }
            return datiProduttori;
        }

        private static Produttore PrendiDatiSingoloProduttore(List<Produttore> datiProduttori)
        {
            Console.Write("Inserire l'ID del produttore per visualizzarne i dati dei prelievi: ");
            var idProduttore = Console.ReadLine();
            Console.Clear();
            Produttore app = null;
            foreach (var datiProduttore in datiProduttori)
            {
                if (idProduttore == datiProduttore.Id)
                {
                    Console.WriteLine($"{datiProduttore.Nome}, ID: {datiProduttore.Id}, codice: {datiProduttore.Codice}, ID allevamento: {datiProduttore.IdAllevamento}, tipo latte: {datiProduttore.TipoLatte}, ID tipo latte: {datiProduttore.IdTipoLatte}.\n");
                    int indiceAnalisi, indiceValori;
                    datiProduttore.Analisi.ForEach(analisi =>
                    {
                        indiceAnalisi = datiProduttore.Analisi.IndexOf(analisi);
                        Console.WriteLine($"\n\nCampione: {analisi.Campione}, data rapporto di prova: {analisi.DataRapportoDiProva.Value:dd/MM/yyyy}, data accettazione: {analisi.DataAccettazione.Value:dd/MM/yyyy}, data prelievo: {analisi.DataPrelievo.Value:dd/MM/yyyy}\n");
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
                    app = datiProduttore;
                }
            }
            Console.ReadLine();
            return app;
        }

        private static void PrevisioniML(Produttore datiProduttore)
        {
            MLContext mlContext = new MLContext();

            // 1. Import or create training data
            Produttore[] datiPrevisioneLatte =
            {
                new Produttore() { Codice = datiProduttore.Codice },
                new Produttore() { Nome = datiProduttore.Nome },
                new Produttore() { Id = datiProduttore.Id },
                new Produttore() { IdAllevamento = datiProduttore.IdAllevamento },
                new Produttore() { TipoLatte = datiProduttore.TipoLatte },
                new Produttore() { IdTipoLatte = datiProduttore.IdTipoLatte },
                new Produttore() { Analisi = datiProduttore.Analisi }
            };
            var datiDiTraining = mlContext.Data.LoadFromEnumerable(datiPrevisioneLatte);

            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "data prelievo" })
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "grassi", maximumNumberOfIterations: 100))
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "proteine", maximumNumberOfIterations: 100));

            // 3. Train model
            var modelloDiTraning = pipeline.Fit(datiDiTraining);

            // 4. Make a prediction: tempo/grassi
            //var size = new HouseData() { Size = 2.5F };

            foreach (var prelievo in datiPrevisioneLatte)
            {

            }
            Produttore dataPrelievo = new Produttore() { /*datiProduttore.Analisi[0].DataPrelievo =*/ };
            Previsioni grassi = mlContext.Model.CreatePredictionEngine<Produttore, Previsioni>(modelloDiTraning).Predict(dataPrelievo);
            Previsioni proteine = mlContext.Model.CreatePredictionEngine<Produttore, Previsioni>(modelloDiTraning).Predict(dataPrelievo);
            //Console.WriteLine($"Previsione valori grassi e proteine : {dataPrelievo.Analisi[0].DataPrelievo * 1000} {grassi.Grassi * 1000} sq ft= {proteine.Proteine * 100:C}k");

            //var stima = grassi.Predict(dataPrelievo);
        }
    }
}