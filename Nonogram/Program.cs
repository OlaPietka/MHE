using System;
using System.Diagnostics;

namespace Nonogram
{
    class Program
    {
        static void Main(string[] args)
        {
            do {
                Console.Clear();
                var program = Menu();

                if (program == 4)
                    Stats.GenerateStats(inputsFilename: "inputs_list.json", outputFilename: "stats.csv");
                else
                {
                    var boardValues = JsonHelper.ReadInputFile("input.json");
                    var result = new Result();

                    var watch = Stopwatch.StartNew();
                    switch (program)
                    {
                        case 1:
                            result = Methods.BruteForce(boardValues);
                            break;

                        case 2:
                            result = Methods.HillClimb(boardValues);
                            break;

                        case 3:
                            result = Methods.Tabu(boardValues);
                            break;
                    }
                    watch.Stop();

                    result.Time = watch.ElapsedMilliseconds / 1000f;

                    BoardHelper.Print(result, "Found board:");

                    JsonHelper.WriteOutputFile(result, "output.json");
                }

                Console.WriteLine("0. POWROT DO MENU");
            } while (Console.ReadKey().KeyChar == '0');
        }


        private static int Menu()
        {
            Console.WriteLine("WYBIERZ PROGRAM:");
            Console.WriteLine("- Znajdz rozwiazanie dla przykladu z input.json metoda:");
            Console.WriteLine("     1. BruteForce");
            Console.WriteLine("     2. HillClimb");
            Console.WriteLine("     3. Tabu");
            Console.WriteLine("4. Statystka dla wszystkich metod i przykladow z inputs.json");
            return int.Parse(Console.ReadKey().KeyChar.ToString());
        }
    }
}