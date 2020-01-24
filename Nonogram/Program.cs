using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Nonogram
{
    class Program
    {
        private static Dictionary<string, object> _map = new Dictionary<string, object>();
        private static BoardValues _boardValues = JsonHelper.ReadInputFile("input.json");

        static void Main(string[] args)
        {
            var e = new EvolutionStrategy();
            Console.WriteLine(e.Run());

        menu:
            Menu();

        start:
            var parameters = new List<object>();
            var methodName = Read();
            parameters.AddRange(_map.Values);

        repeat:
            var watch = Stopwatch.StartNew();
            var result = Invoke(methodName, parameters);
            watch.Stop();

            result.Time = watch.ElapsedMilliseconds / 1000f;

            BoardHelper.Print(result, "Found board:");
            JsonHelper.WriteOutputFile(result, "output.json");

            Console.WriteLine("0. Powtorz");
            Console.WriteLine("1. Nowe");
            Console.WriteLine("2. Menu\n");
            var input = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");
            if (input == '0')
                goto repeat;
            else if(input == '1')
                goto start;
            else if(input == '2')
                goto menu;
        }

        private static void Menu()
        {
            Console.WriteLine("0. Wlasne wywolanie z lini komend");
            Console.WriteLine("1. Statystka dla wszystkich metod i przykladow z inputs.json");
            Console.WriteLine("2. Genetic parallel vs not parallel stats");
            var input = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");

            if (input == '1')
            {
                Console.WriteLine("Ilosc zadan z inputs.json (max 17) [int]: ");
                var range = (int)Parse(Console.ReadLine());
                Console.WriteLine("Czy wywolac bez metody brutforce? [true/false]: ");
                var withoutBruteForce = (bool)Parse(Console.ReadLine());

                Stats.GenerateStats("inputs_list.json", "stats.csv", range, withoutBruteForce);
            }
            else if (input == '2')
                Stats.ParallelVsNotParallel(_boardValues);
            else
                Console.Clear();
        }

        private static string Read()
        {
        start:
            var line = Console.ReadLine();
            var prms = line.Split(' ');
            var parametersName = new List<string>();
            var parametersValue = new List<object>();
            var methods = Type.GetType("Nonogram.Method").GetMembers().OfType<MethodInfo>().ToList();

            if (line.Contains("--help"))
                {
                    for (var i = 0; i < methods.Count - 4; i++)
                        Console.WriteLine("-" + methods[i].Name);
                    Console.WriteLine("\n");
                    goto start;
                }

            CreateMap(ValidString(prms[0]));

            if (line.Contains("-info"))
            {
                foreach (var m in _map)
                    Console.WriteLine("-" + m.Key + $" [{m.Value.GetType()}]");
                Console.WriteLine("\n");
                goto start;
            }

            foreach (var prm in prms)
                if (prm.StartsWith("-"))
                    parametersName.Add(ValidString(prm));
                else
                    parametersValue.Add(Parse(prm));

            ModifyMap(parametersName, parametersValue);

            return parametersName[0];
        } 

        private static void CreateMap(string methodName)
        {
            _map = new Dictionary<string, object>();
            var method = Type.GetType("Nonogram.Method").GetMember(methodName).OfType<MethodInfo>().First();

            foreach(var parameter in method.GetParameters())
                if (parameter.Name == "boardValues" && !_map.ContainsKey(parameter.Name))
                    _map.Add(parameter.Name, _boardValues);
                else
                    _map.Add(parameter.Name, parameter.DefaultValue);
        }

        private static void ModifyMap(List<string> names, List<object> values)
        {
            var copyMap = _map.ToDictionary(x => x.Key, x => x.Value);

            foreach (var key in copyMap.Keys)
                for (var i = 0; i < values.Count; i++)
                    if (key == names[i + 1])
                        _map[key] = values[i];
        }

        private static string ValidString(string str)
            => str.Remove(0, 1);

        private static object Parse(string value)
        {
            var parseInt = 0;
            var parseDouble = 0.0;
            var parseBool = false;
            if (int.TryParse(value, out parseInt))
                return parseInt;
            else if (bool.TryParse(value, out parseBool))
                return parseBool;
            else if (double.TryParse(value, out parseDouble))
                return parseDouble;
            else
                return value;
        }

        private static Result Invoke(string methodName, List<object> parameters)
        {
            var method = Type.GetType("Nonogram.Method").GetMember(methodName).OfType<MethodInfo>().First();
            return method.Invoke(Type.GetType("Nonogram.Method"), parameters.ToArray()) as Result;
        }
    }
}