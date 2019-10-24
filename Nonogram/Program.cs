using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Nonogram
{
    class Program
    {
        static void Main(string[] args)
        {
            var boardValues = JsonHelper.ReadJsonFile("input.json");

            var watch = Stopwatch.StartNew();
            var result = Methods.Tabu(boardValues);
            watch.Stop();

            result.Time = watch.ElapsedMilliseconds / 1000f;

            BoardHelper.Print(result, "Found board:");

            JsonHelper.WriteJsonFile(result, "output.json");
            Console.ReadKey();
        }
    }
}