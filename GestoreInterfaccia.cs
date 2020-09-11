using System;
using System.Collections.Generic;
using System.Text;

namespace LatteMarcheML
{
    class GestoreInterfaccia
    {
        public static void StampaDatiSingoloProduttore(List<Produttore> datiProduttori)
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
                            if (datiProduttore.Analisi[indiceAnalisi].Valori[indiceValori].FuoriSoglia == 1f)
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPremi qualsiasi tasto per avviare la previsione dei dati.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
        }

        public static void StampaPresentazioneModelli()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("I modelli sono stati salvati in:\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void StampaPercorsiModelli(string percorsoCartellaModelloCompleto)
        {
            Console.WriteLine(percorsoCartellaModelloCompleto);
        }

        public static void StampaIntestazionePrevisione(string nomeModello)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\n\n=========== Visualizza 10 previsioni per ogni modello: {nomeModello}.zip ===========\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void StampaConfrontoDatoRealeControPrevisto(string datoPrevisto, string datoReale)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Dato reale:     {datoReale}");
            Console.WriteLine($"Dato previsto:  {datoPrevisto}");
        }

        public static void StampaPrevisioneDati(List<string> datiPrevisti)
        {
            Console.WriteLine("\n");
            bool primaRiga = true;
            foreach (var singoloValore in datiPrevisti)
            {
                if (primaRiga)
                {
                    primaRiga = false;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{singoloValore}\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    goto ciclo;
                }
                Console.WriteLine(singoloValore);
                ciclo:;
            }
            Console.WriteLine("\n\n");
        }

        public static void TerminaEsecuzione()
        {
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPremi qualsiasi tasto per terminare l'esecuzione.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
        }
    }
}