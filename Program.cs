using ClosedXML.Excel;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LatteMarcheML
{

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
            //GestoreInterfaccia.StampaDatiSingoloProduttore(datiProduttori);
            var analisiML = TrasformaProduttoreInAnalisiML(datiProduttori);
            var datiPrevisti = PrevisioniML(analisiML);
            //CreaFileCsv(analisiML, datiPrevisti);
            GestoreInterfaccia.TerminaEsecuzione();
        }

        public static string PrendiPercorsoCompleto(string percorsoParziale)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string percorsoCartellaAssembly = _dataRoot.Directory.FullName;
            string percorsoCompleto = Path.Combine(percorsoCartellaAssembly, percorsoParziale);
            return percorsoCompleto;
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
            var listaValoriPrelievi = new List<ValorePrelievo>();
            listaValoriPrelievi = tabellaValori.DataRange.Rows().Select(riga => new ValorePrelievo()
            {
                Id = riga.Field("ID").GetString().Trim(),
                Nome = riga.Field("NOME").GetString().Trim(),
                Uom = riga.Field("UOM").GetString().Trim(),
                Valore = PrendiDatoSingolaCella(riga.Field("VALORE")),
                FuoriSoglia = PrendiDatoSingolaCella(riga.Field("FUORI_SOGLIA")),
                AnalisiId = riga.Field("Analisi_Id").GetString().Trim()
            }).ToList();
            return listaValoriPrelievi;
        }

        private static float PrendiDatoSingolaCella(IXLCell contenutoCella)
        {
            if(!contenutoCella.IsEmpty())
            {
                if (contenutoCella.DataType == XLDataType.Text)
                {
                    if (String.Compare(contenutoCella.GetString().Trim(), "assenti") == 0)
                    {
                        return 0f;
                    }
                    else
                    {
                        var datoCella = float.Parse(contenutoCella.GetString().Trim());
                        return datoCella;
                    }
                }
                else if (contenutoCella.DataType == XLDataType.Number)
                {
                    var datoCella = float.Parse(contenutoCella.GetString().Trim());
                    return datoCella;
                }
            }
            return 0f;
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

        private static List<AnalisiML> TrasformaProduttoreInAnalisiML(List<Produttore> datiProduttori)
        {
            List<AnalisiML> listaDatiAnalisiML = new List<AnalisiML>();
            foreach (var produttore in datiProduttori)
            {
                foreach (var analisi in produttore.Analisi)
                {
                    listaDatiAnalisiML.Add(new AnalisiML()
                    {
                        Campione = analisi.Campione,
                        NomeProduttore = produttore.Nome,
                        GrassoPv = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore.ToString() == "" ? 0f : analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore,
                        GrassoPerCalcolo = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore.ToString() == "" ? 0f : analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore,
                        GrassoPerCalcoloFuoriSoglia = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").FuoriSoglia.ToString() == "" ? 0f : analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").FuoriSoglia,
                        Giorno = analisi.DataPrelievo == "" ? 0f : float.Parse(analisi.DataPrelievo.Substring(0, 2)),
                        Mese = analisi.DataPrelievo == "" ? 0f : float.Parse(analisi.DataPrelievo.Substring(3, 2)),
                        Anno = analisi.DataPrelievo == "" ? 0f : float.Parse(analisi.DataPrelievo.Substring(6, 4))
                    });
                }
            }
            return listaDatiAnalisiML;
        }

        private static List<AnalisiMLPrevisioni> PrevisioniML(List<AnalisiML> analisiML)
        {
            const string percorsoCartella = @"C:\Users\Giorgio Della Roscia\source\repos\LatteMarcheML\Data";
            //string percorsoFileDiTraining = $@"{percorsoCartella}\DatiProduttori - train.csv";
            //string percorsoFileDiTest = $@"{percorsoCartella}\DatiProduttori - test.csv";
            //string percorsoFileDiTrainingCompleto = PrendiPercorsoCompleto(percorsoFileDiTraining);
            //string percorsoFileDiTestCompleto = PrendiPercorsoCompleto(percorsoFileDiTest);
            var mlContext = new MLContext(seed: 0);
            var datiDiTrain = mlContext.Data.LoadFromEnumerable<AnalisiML>(analisiML);
            //var datiDiTest = mlContext.Data.LoadFromTextFile<AnalisiML>(percorsoFileDiTestCompleto, separatorChar: ',', hasHeader: true);
            //var datiDiTest = mlContext.Data.LoadFromEnumerable<AnalisiML>(ModelloML.LeggiDatiDiTestDaCsv(10));
            List<AnalisiML> listaDatiDiTest = new List<AnalisiML>();
            for (int i = 0; i < 10; i++)
            {
                //var singolaAnalisi = analisiML[i * 2]; 
                //AnalisiML app = singolaAnalisi.Campione, //TODO: come si inseriscono i dati direttamentein un oggetto?
                listaDatiDiTest.Add(analisiML[i*2]);
            }
            var datiDiTest = mlContext.Data.LoadFromEnumerable<AnalisiML>(listaDatiDiTest);            
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", nameof(AnalisiML.GrassoPerCalcolo), nameof(AnalisiML.GrassoPerCalcoloFuoriSoglia), /*nameof(AnalisiML.ProteinePerCalcolo), nameof(AnalisiML.ProteinePerCalcoloFuoriSoglia),*/ nameof(AnalisiML.Giorno), nameof(AnalisiML.Mese), nameof(AnalisiML.Anno)).AppendCacheCheckpoint(mlContext);
            (string name, IEstimator<ITransformer> value)[] modelliDiRegressione = {
                ("FastTree", mlContext.Regression.Trainers.FastTree()),
                ("Poisson", mlContext.Regression.Trainers.LbfgsPoissonRegression()),
                ("SDCA", mlContext.Regression.Trainers.Sdca()),
                ("FastTreeTweedie", mlContext.Regression.Trainers.FastTreeTweedie())
            };
            Console.WriteLine("I modelli sono stati salvati in:\n");
            foreach (var modello in modelliDiRegressione)
            {
                var trainingPipeline = dataProcessPipeline.Append(modello.value);
                var trainedModel = trainingPipeline.Fit(datiDiTrain);

                IDataView previsioni = trainedModel.Transform(datiDiTest); 
                var metrics = mlContext.Regression.Evaluate(data: previsioni, labelColumnName: "Label", scoreColumnName: "Score");   
                string percorsoCartellaModello = $@"{percorsoCartella}\Models\{modello.name}.zip";
                string percorsoCartellaModelloCompleto = PrendiPercorsoCompleto(percorsoCartellaModello);
                mlContext.Model.Save(trainedModel, datiDiTrain.Schema, percorsoCartellaModelloCompleto);
                GestoreInterfaccia.StampaPercorsiModelli(percorsoCartellaModelloCompleto);
            }
            foreach (var modello in modelliDiRegressione)
            {
                string percorsoCartellaModello = $@"{percorsoCartella}\Models\{modello.name}.zip";
                string percorsoCartellaModelloCompleto = PrendiPercorsoCompleto(percorsoCartellaModello);
                ITransformer trainedModel = mlContext.Model.Load(percorsoCartellaModelloCompleto, out var modelInputSchema);
                var algoritmoDiPrevisione = mlContext.Model.CreatePredictionEngine<AnalisiML, AnalisiMLPrevisioni>(trainedModel);
                GestoreInterfaccia.StampaIntestazionePrevisione(modello.name);
                ModelloML.CalcolaPrevisioni(mlContext, modello.name, algoritmoDiPrevisione, 10, listaDatiDiTest);
            }
            return null;
        }

        private static void CreaCsv(List<AnalisiML> analisiML, List<AnalisiMLPrevisioni> datiPrevisti)
        {
            
        }
    }
}