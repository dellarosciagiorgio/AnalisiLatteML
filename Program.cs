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
            Console.SetWindowSize(190, 70);
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
            FileInfo cartellaDati = new FileInfo(typeof(Program).Assembly.Location);
            string percorsoCartellaAssembly = cartellaDati.Directory.FullName;
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
                        /*GrassoPerCalcolo = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore.ToString() == "" ? 0f : analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso (per calcolo)").Valore,
                        GrassoFuoriSoglia = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso").FuoriSoglia.ToString() == "" ? 0f : analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == "Grasso").FuoriSoglia,
                        Giorno = analisi.DataPrelievo == "" ? 0f : float.Parse(analisi.DataPrelievo.Substring(0, 2)),*/
                        Campione = analisi.Campione,
                        NomeProduttore = produttore.Nome,
                        IdProduttore = produttore.Id,
                        Grasso = PrendiValore(analisi, "Grasso"),
                        GrassoFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Grasso"),
                        GrassoPerCalcolo = PrendiValore(analisi, "Grasso (per calcolo)"),
                        GrassoPerCalcoloFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Grasso (per calcolo)"),
                        Proteine = PrendiValore(analisi, "Proteine"),
                        ProteineFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Proteine"),
                        ProteinePerCalcolo = PrendiValore(analisi, "Proteine (per calcolo)"),
                        ProteinePerCalcoloFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Proteine (per calcolo)"),
                        Lattosio = PrendiValore(analisi, "Lattosio"),
                        LattosioFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Lattosio"),
                        ResiduoSeccoMagro = PrendiValore(analisi, "Residuo secco magro"),
                        ResiduoSeccoMagroFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Residuo secco magro"),
                        Ph = PrendiValore(analisi, "pH"),
                        PhFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "pH"),
                        IndiceCrioscopico = PrendiValore(analisi, "Indice Crioscopico"),
                        IndiceCrioscopicoFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Indice Crioscopico"),
                        ContenutoInAcquaAggiunta = PrendiValore(analisi, "Contenuto in acqua aggiunta"),
                        ContenutoInAcquaAggiuntaFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Contenuto in acqua aggiunta"),
                        CelluleSomatiche = PrendiValore(analisi, "Cellule somatiche"),
                        CelluleSomaticheFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Cellule somatiche"),
                        CaricaBattericaTotale = PrendiValore(analisi, "Carica Batterica Totale"),
                        CaricaBattericaTotaleFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Carica Batterica Totale"),
                        MediaCelluleTrimestrePrecendente = PrendiValore(analisi, "Media Cellule Trimestre Precendente"),
                        MediaCelluleTrimestrePrecendenteFuoriSoglia = PrendiFlagFuoriSoglia(analisi, "Media Cellule Trimestre Precendente"),
                        Giorno = PrendiData(analisi, 0, 2),
                        Mese = PrendiData(analisi, 3, 2),
                        Anno = PrendiData(analisi, 6, 4),
                        GrassoPv = PrendiValore(analisi, "Grasso (per calcolo)"),
                        ProteinePv = PrendiValore(analisi, "Proteine (per calcolo)")
                    });
                }
            }
            return listaDatiAnalisiML;
        }

        private static float PrendiValore(AnalisiLatte analisi, string datoInteressato)
        {
            if (analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == datoInteressato).Valore.ToString() != "")
            {
                if (analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == datoInteressato).Valore.ToString() != "assenti")
                {
                    float valore = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == datoInteressato).Valore;
                    return valore;
                }
            }
            return 0f;
        }

        private static float PrendiFlagFuoriSoglia(AnalisiLatte analisi, string datoInteressato)
        {
            if (analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == datoInteressato).FuoriSoglia.ToString() != "")
            {
                if (analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == datoInteressato).FuoriSoglia.ToString() != "assenti")
                {
                    var flagFuoriFoglia = analisi.Valori.FirstOrDefault(a => a.Nome.Trim() == datoInteressato).FuoriSoglia;
                    return flagFuoriFoglia;
                }
            }
            return 0f;
        }

        private static float PrendiData(AnalisiLatte analisi, int indiceInizio, int lunghezza)
        {
            if (analisi.DataPrelievo != "")
            {
                var dato = float.Parse(analisi.DataPrelievo.Substring(indiceInizio, lunghezza));
                return dato;
            }
            return 0f;
        }

        private static List<AnalisiMLPrevisioni> PrevisioniML(List<AnalisiML> analisiML)
        {
            const string percorsoCartella = @"C:\Users\Giorgio Della Roscia\source\repos\LatteMarcheML\Data";
             var mlContext = new MLContext(seed: 0);
            var datiDiTrain = mlContext.Data.LoadFromEnumerable<AnalisiML>(analisiML);
            List<AnalisiML> listaDatiDiTest = new List<AnalisiML>();
            for (int i = 0; i < 10; i++)
            {
                listaDatiDiTest.Add(analisiML[i*2]);
            }
            var datiDiTest = mlContext.Data.LoadFromEnumerable<AnalisiML>(listaDatiDiTest);
            var grassoPipeline = mlContext.Transforms.CopyColumns("Label", "GrassoPV").Append(mlContext.Transforms.Concatenate("Features",
            #region
                nameof(AnalisiML.Grasso),
                nameof(AnalisiML.GrassoFuoriSoglia),
                nameof(AnalisiML.GrassoPerCalcolo),
                nameof(AnalisiML.GrassoPerCalcoloFuoriSoglia),
                nameof(AnalisiML.Proteine),
                nameof(AnalisiML.ProteineFuoriSoglia),
                nameof(AnalisiML.ProteinePerCalcolo),
                nameof(AnalisiML.ProteinePerCalcoloFuoriSoglia),
                nameof(AnalisiML.Lattosio),
                nameof(AnalisiML.LattosioFuoriSoglia),
                nameof(AnalisiML.ResiduoSeccoMagro),
                nameof(AnalisiML.ResiduoSeccoMagroFuoriSoglia),
                nameof(AnalisiML.Ph),
                nameof(AnalisiML.PhFuoriSoglia),
                nameof(AnalisiML.IndiceCrioscopico),
                nameof(AnalisiML.IndiceCrioscopicoFuoriSoglia),
                nameof(AnalisiML.ContenutoInAcquaAggiunta),
                nameof(AnalisiML.ContenutoInAcquaAggiuntaFuoriSoglia),
                nameof(AnalisiML.CelluleSomatiche),
                nameof(AnalisiML.CelluleSomaticheFuoriSoglia),
                nameof(AnalisiML.CaricaBattericaTotale),
                nameof(AnalisiML.CaricaBattericaTotaleFuoriSoglia),
                nameof(AnalisiML.MediaCelluleTrimestrePrecendente),
                nameof(AnalisiML.MediaCelluleTrimestrePrecendenteFuoriSoglia),
                nameof(AnalisiML.Giorno),
                nameof(AnalisiML.Mese),
                nameof(AnalisiML.Anno)).AppendCacheCheckpoint(mlContext));
            #endregion
            var proteinePipeline = mlContext.Transforms.CopyColumns("Label", "ProteinePV").Append(mlContext.Transforms.Concatenate("Features",
            #region
                nameof(AnalisiML.Grasso),
                nameof(AnalisiML.GrassoFuoriSoglia),
                nameof(AnalisiML.GrassoPerCalcolo),
                nameof(AnalisiML.GrassoPerCalcoloFuoriSoglia),
                nameof(AnalisiML.Proteine),
                nameof(AnalisiML.ProteineFuoriSoglia),
                nameof(AnalisiML.ProteinePerCalcolo),
                nameof(AnalisiML.ProteinePerCalcoloFuoriSoglia),
                nameof(AnalisiML.Lattosio),
                nameof(AnalisiML.LattosioFuoriSoglia),
                nameof(AnalisiML.ResiduoSeccoMagro),
                nameof(AnalisiML.ResiduoSeccoMagroFuoriSoglia),
                nameof(AnalisiML.Ph),
                nameof(AnalisiML.PhFuoriSoglia),
                nameof(AnalisiML.IndiceCrioscopico),
                nameof(AnalisiML.IndiceCrioscopicoFuoriSoglia),
                nameof(AnalisiML.ContenutoInAcquaAggiunta),
                nameof(AnalisiML.ContenutoInAcquaAggiuntaFuoriSoglia),
                nameof(AnalisiML.CelluleSomatiche),
                nameof(AnalisiML.CelluleSomaticheFuoriSoglia),
                nameof(AnalisiML.CaricaBattericaTotale),
                nameof(AnalisiML.CaricaBattericaTotaleFuoriSoglia),
                nameof(AnalisiML.MediaCelluleTrimestrePrecendente),
                nameof(AnalisiML.MediaCelluleTrimestrePrecendenteFuoriSoglia),
                nameof(AnalisiML.Giorno),
                nameof(AnalisiML.Mese),
                nameof(AnalisiML.Anno)).AppendCacheCheckpoint(mlContext));
            #endregion
            (string name, IEstimator<ITransformer> value)[] modelliDiRegressione =
            {
                ("FastTree", mlContext.Regression.Trainers.FastTree()),
                ("Poisson", mlContext.Regression.Trainers.LbfgsPoissonRegression()),
                //("SDCA", mlContext.Regression.Trainers.Sdca()),
                ("FastTreeTweedie", mlContext.Regression.Trainers.FastTreeTweedie())
            };

            GestoreInterfaccia.StampaPresentazioneModelli();
            foreach (var modello in modelliDiRegressione)
            {
                var pipelineDiTraining = grassoPipeline.Append(modello.value).Append(mlContext.Transforms.CopyColumns(outputColumnName: "grassopercalcoloprevisto", inputColumnName: "Score")).Append(mlContext.Transforms.CopyColumns(outputColumnName: "proteinepercalcolopreviste", inputColumnName: "Score"));
                //.Append(mlContext.Transforms.CopyColumns(outputColumnName: "proteinepercalcolopreviste", inputColumnName: "Score")));                
                var modelloAllenato = pipelineDiTraining.Fit(datiDiTrain);
                IDataView previsioni = modelloAllenato.Transform(datiDiTest);
                var metricsGrasso = mlContext.Regression.Evaluate(data: previsioni, labelColumnName: "GrassoPV", scoreColumnName: "grassopercalcoloprevisto"); //.Evaluate(data: previsioni, labelColumnName: "ProteinePV", scoreColumnName: "proteinepercalcolopreviste");
                var metricsProteine = mlContext.Regression.Evaluate(data: previsioni, labelColumnName: "ProteinePV", scoreColumnName: "proteinepercalcolopreviste");   
                string percorsoCartellaModello = $@"{percorsoCartella}\Models\{modello.name}.zip";
                string percorsoCartellaModelloCompleto = PrendiPercorsoCompleto(percorsoCartellaModello);
                mlContext.Model.Save(modelloAllenato, datiDiTrain.Schema, percorsoCartellaModelloCompleto);
                GestoreInterfaccia.StampaPercorsiModelli(percorsoCartellaModelloCompleto);
            }
            foreach (var modello in modelliDiRegressione)
            {
                string percorsoCartellaModello = $@"{percorsoCartella}\Models\{modello.name}.zip";
                string percorsoCartellaModelloCompleto = PrendiPercorsoCompleto(percorsoCartellaModello);
                ITransformer modelloAllenato = mlContext.Model.Load(percorsoCartellaModelloCompleto, out var inputSchemaModello);
                var algoritmoDiPrevisione = mlContext.Model.CreatePredictionEngine<AnalisiML, AnalisiMLPrevisioni>(modelloAllenato);
                GestoreInterfaccia.StampaIntestazionePrevisione(modello.name);
                ModelloML.CalcolaPrevisioni(mlContext, modello.name, algoritmoDiPrevisione, 10, listaDatiDiTest);
            }
            return null; //dati previsti da inserire nel csv con i reali
        }

        //previsioni multiple
        #region metodo 1
        /*MLContext mlContext = new MLContext();
        DataViewSchema predictionPipelineSchema;
        ITransformer predictionPipeline = mlContext.Model.Load("model.zip", out predictionPipelineSchema);
        //AnalisiML[] analisiML = new AnalisiML[]
        //{
        //    new AnalisiML
        //    {
        //        //dati reali
        //    },
        //    new AnalisiML
        //    {
        //        //dati reali
        //    }
        //};
        //IDataView predictions = predictionPipeline.Transform(inputData);*/
        #endregion
        #region metodo 2
        //private static ITransformer Train(MLContext mlContext, string trainDataPath)
        //{
        //    IDataView dataView = _textLoader.Read(trainDataPath);
        //    var pipelinePerGrassoPv = mlContext.Transforms.CopyColumns("Label", "grassopercalcoloprevisto")
        //    .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId"))
        //    .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode"))
        //    .Append(mlContext.Transforms.Categorical.OneHotEncoding("PaymentType"))
        //    .Append(mlContext.Transforms.Concatenate("Features", "VendorId", "RateCode", "PassengerCount", "TripDistance", "PaymentType"))
        //    .Append(mlContext.Regression.Trainers.FastTree())
        //    .Append(mlContext.Transforms.CopyColumns(outputcolumn: "tripTime", inputcolumn: "Score"));

        //    var pipelinePerProteinePv = mlContext.Transforms.CopyColumns("Label", "proteinepercalcolopreviste")
        //    .Append(mlContext.Transforms.Categorical.OneHotEncoding("VendorId"))
        //    .Append(mlContext.Transforms.Categorical.OneHotEncoding("RateCode"))
        //    .Append(mlContext.Transforms.Categorical.OneHotEncoding("PaymentType"))
        //    .Append(mlContext.Transforms.Concatenate("Features", "VendorId", "RateCode", "PassengerCount", "TripDistance", "PaymentType"))
        //    .Append(mlContext.Regression.Trainers.FastTree())
        //    .Append(mlContext.Transforms.CopyColumns(outputcolumn: "fareAmount", inputcolumn: "Score"));



        //    var model = pipelineForTripTime.Append(pipelineForFareAmount).Fit(dataView);
        //    SaveModelAsFile(mlContext, model);
        //    return model;
        //}
        #endregion

        private static void CreaCsv(List<AnalisiML> analisiML, List<AnalisiMLPrevisioni> datiPrevisti)
        {
            
        }
    }
}