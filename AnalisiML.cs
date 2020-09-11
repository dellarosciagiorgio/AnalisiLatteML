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
        [ColumnName("Score")]
        public float GrassoPerCalcoloPrevisto;
    }

    public class AnalisiML
    {
        //TODO: se funziona: commit + inserire anche gli altri attributi + prevedere anche le proteine
        [LoadColumn(0)]
        public string Campione { get; set; }
        [LoadColumn(1)]
        public string NomeProduttore { get; set; }
        [LoadColumn(2)]
        public string IdProduttore { get; set; }
        [LoadColumn(3)]
        public float GrassoPerCalcolo { get; set; }
        [LoadColumn(4)]
        public float GrassoPerCalcoloFuoriSoglia { get; set; }
        [LoadColumn(5)]
        public float Giorno { get; set; }
        [LoadColumn(6)]
        public float Mese { get; set; }
        [LoadColumn(7)]
        public float Anno { get; set; }
        [LoadColumn(3)]
        [ColumnName("Label")]
        public float GrassoPv { get; set; }
    }

    public static class ModelloML
    {
        public static void CalcolaPrevisioni(MLContext mlContext, string nomeModello, PredictionEngine<AnalisiML, AnalisiMLPrevisioni> algoritmoDiPrevisione, int numeroDiPrevisioni, List<AnalisiML> listaDatiDiTest)
        {
            List<string> valoriPrevisti = new List<string>();
            valoriPrevisti.Add("Previsioni grasso (per calcolo): "); //TODO: sostituire stringa con attributo contenente dato da prevedere
            for (int i = 1; i < numeroDiPrevisioni; i++)
            {
                var risultatoPrevisione = algoritmoDiPrevisione.Predict(listaDatiDiTest[i]);
                valoriPrevisti.Add(risultatoPrevisione.GrassoPerCalcoloPrevisto.ToString());
                GestoreInterfaccia.StampaConfrontoDatoRealeControPrevisto(risultatoPrevisione.GrassoPerCalcoloPrevisto.ToString(), listaDatiDiTest[i].GrassoPerCalcolo.ToString()); 
            }
            GestoreInterfaccia.StampaPrevisioneDati(valoriPrevisti);
        }
    }
}