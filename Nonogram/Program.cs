using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Nonogram
{
    class Program
    {
        static void Main(string[] args)
        {
            do {
                Console.Clear();
                var program = Menu();

                if (program == 6)
                {
                    Console.WriteLine("Ilosc zadan z inputs.json (max 17) [int]: ");
                    var range = int.Parse(Console.ReadLine());
                    Console.WriteLine("Czy wywolac bez metody brutforce? [true/false]: ");
                    var withoutBruteForce = bool.Parse(Console.ReadLine());

                    Stats.GenerateStats(inputsFilename: "inputs_list.json", outputFilename: "stats.csv", range: range, withoutBruteForce: withoutBruteForce);
                }
                else
                {
                    var boardValues = JsonHelper.ReadInputFile("input.json");
                    var result = new Result();
                    var parameters = new List<object>();
                    parameters.Add(boardValues);

                    var watch = Stopwatch.StartNew();
                    switch (program)
                    {
                        case 1:
                            result = Method.BruteForce(boardValues);
                            break;

                        case 2:
                            parameters.AddRange(ChoseParameters("Ilosc itearcji [int]"));
                            result = Invoke(nameof(Method.HillClimb), parameters);
                            break;

                        case 3:
                            parameters.AddRange(ChoseParameters("Ilosc itearcji [int]", "Rozmiar tabu [int]"));
                            result = Invoke(nameof(Method.Tabu), parameters);
                            break;
                        case 4:
                            Console.WriteLine("\n\nPuscic eksperyment? [t/n] :");
                            if (Console.ReadKey().KeyChar == 'T')
                            {
                                (var iteration, var parameter) = Other.RunExperiment(boardValues);

                                parameters.Add(iteration);
                                parameters.Add(parameter);
                            }
                            else
                                parameters.AddRange(ChoseParameters("Ilosc itearcji [int]", "Parametr temperatury [double]", "Wypisywac iteracje? [bool]"));

                            result = Invoke(nameof(Method.SimulatedAnnealing), parameters);
                            break;
                        case 5:
                            parameters.AddRange(ChoseParameters("Rozmiar populacji [int]", "Ilosc iteracji [int]", "Prawdopodobienstwo krzyzowania [double]",
                                "Prawdopodobienstwo mutacji [double]", "Metoda krzyzowania [OnePoint/TwoPoints]",
                                "Metoda selekcji [Rulet/Rank/Tournament]", "Warunek zakonczenia [Iteration/Mean/Deviation]"));
                            result = Invoke(nameof(Method.Genetic), parameters);
                            break;
                    }
                    watch.Stop();

                    result.Time = watch.ElapsedMilliseconds / 1000f;

                    BoardHelper.Print(result, "Found board:");

                    JsonHelper.WriteOutputFile(result, "output.json");
                }

                Console.WriteLine("0. POWROT DO MENU");
                Console.WriteLine("1. POWTÓRZ");
            } while (Console.ReadKey().KeyChar == '0');
        }


        private static int Menu()
        {
            Console.WriteLine("WYBIERZ PROGRAM:");
            Console.WriteLine("- Znajdz rozwiazanie dla przykladu z input.json metoda:");
            Console.WriteLine("     1. BruteForce");
            Console.WriteLine("     2. HillClimb");
            Console.WriteLine("     3. Tabu");
            Console.WriteLine("     4. Simulated Annealing");
            Console.WriteLine("     5. Genetic");
            Console.WriteLine("6. Statystka dla wszystkich metod i przykladow z inputs.json");
            return int.Parse(Console.ReadKey().KeyChar.ToString());
        }

        private static List<object> ChoseParameters(params string[] parameters)
        {
            var chosenParameters = new List<object>();

            Console.WriteLine("\n\nPARAMETRY:");

            foreach (var parameter in parameters)
            {
                Console.WriteLine($"{parameter}: ");
                var input = Console.ReadLine();

                if (input == "")
                    break;

                var parseInt = 0;
                var parseDouble = 0.0;
                var parseBool = false;
                if (int.TryParse(input, out parseInt))
                    chosenParameters.Add(parseInt);
                else if (bool.TryParse(input, out parseBool))
                    chosenParameters.Add(parseBool);
                else if (double.TryParse(input, out parseDouble))
                    chosenParameters.Add(parseDouble);
                else
                    chosenParameters.Add(input);
            }

            return chosenParameters;
        }

        private static Result Invoke(string methodName, List<object> parameters)
        {
            var method = Type.GetType("Nonogram.Method").GetMember(methodName).OfType<MethodInfo>().First();
            var methodParameters = method.GetParameters();

            if (parameters.Count != methodParameters.Length)
            {
                for (var i = parameters.Count; i < methodParameters.Length; i++)
                    parameters.Add(methodParameters[i].DefaultValue);
            }

            return method.Invoke(Type.GetType("Nonogram.Method"), parameters.ToArray()) as Result;
        }
    }
}