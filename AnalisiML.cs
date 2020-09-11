using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LatteMarcheML
{
    public class AnalisiMLPrevisioni
    {
        [ColumnName("grassopercalcoloprevisto")]
        public float GrassoPerCalcoloPrevisto;
        [ColumnName("proteinepercalcolopreviste")]
        public float ProteinePerCalcoloPreviste;
    }

    public class AnalisiML
    {
        public string Campione { get; set; }
        public string NomeProduttore { get; set; }
        public string IdProduttore { get; set; }
        public float Grasso { get; set; }
        public float GrassoFuoriSoglia { get; set; }
        public float GrassoPerCalcolo { get; set; }
        public float GrassoPerCalcoloFuoriSoglia { get; set; }
        public float Proteine { get; set; }
        public float ProteineFuoriSoglia { get; set; }
        public float ProteinePerCalcolo { get; set; }
        public float ProteinePerCalcoloFuoriSoglia { get; set; }
        public float Lattosio { get; set; }
        public float LattosioFuoriSoglia { get; set; }
        public float ResiduoSeccoMagro { get; set; }
        public float ResiduoSeccoMagroFuoriSoglia { get; set; }
        public float Ph { get; set; }
        public float PhFuoriSoglia { get; set; }
        public float IndiceCrioscopico { get; set; }
        public float IndiceCrioscopicoFuoriSoglia { get; set; }
        public float ContenutoInAcquaAggiunta { get; set; }
        public float ContenutoInAcquaAggiuntaFuoriSoglia { get; set; }
        public float CelluleSomatiche { get; set; }
        public float CelluleSomaticheFuoriSoglia { get; set; }
        public float CaricaBattericaTotale { get; set; }
        public float CaricaBattericaTotaleFuoriSoglia { get; set; }
        public float MediaCelluleTrimestrePrecendente { get; set; }
        public float MediaCelluleTrimestrePrecendenteFuoriSoglia { get; set; }
        public float Giorno { get; set; }
        public float Mese { get; set; }
        public float Anno { get; set; }
        [ColumnName("GrassoPV")]
        public float GrassoPv { get; set; }
        [ColumnName("ProteinePV")]
        public float ProteinePv { get; set; }
    }

    public static class ModelloML
    {
        public static void CalcolaPrevisioni(MLContext mlContext, string nomeModello, PredictionEngine<AnalisiML, AnalisiMLPrevisioni> algoritmoDiPrevisione, int numeroDiPrevisioni, List<AnalisiML> listaDatiDiTest)
        {
            List<string> datiPrevisti = new List<string>();
            List<string> valoriGrassiPrevisti = new List<string>();
            valoriGrassiPrevisti.Add("Previsioni grasso (per calcolo): ");
            for (int i = 1; i < numeroDiPrevisioni; i++)
            {
                var risultatoPrevisione = algoritmoDiPrevisione.Predict(listaDatiDiTest[i]);
                valoriGrassiPrevisti.Add(risultatoPrevisione.ProteinePerCalcoloPreviste.ToString());
                GestoreInterfaccia.StampaConfrontoDatoRealeControPrevisto(risultatoPrevisione.GrassoPerCalcoloPrevisto.ToString(), listaDatiDiTest[i].GrassoPerCalcolo.ToString());
            }
            GestoreInterfaccia.StampaPrevisioneDati(valoriGrassiPrevisti);
            datiPrevisti.Add("Grasso (per calcolo) previsto");
            foreach (var valoreGrasso in valoriGrassiPrevisti)
            {
                datiPrevisti.Add(valoreGrasso);
            }
            List<string> valoriProteinePrevisti = new List<string>();
            valoriProteinePrevisti.Add("Previsioni proteine (per calcolo): ");
            for (int i = 1; i < numeroDiPrevisioni; i++)
            {
                var risultatoPrevisione = algoritmoDiPrevisione.Predict(listaDatiDiTest[i]);
                valoriProteinePrevisti.Add(risultatoPrevisione.ProteinePerCalcoloPreviste.ToString());
                GestoreInterfaccia.StampaConfrontoDatoRealeControPrevisto(risultatoPrevisione.ProteinePerCalcoloPreviste.ToString(), listaDatiDiTest[i].ProteinePerCalcolo.ToString());
            }
            GestoreInterfaccia.StampaPrevisioneDati(valoriProteinePrevisti);        
        }
    }
}