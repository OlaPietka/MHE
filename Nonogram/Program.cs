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
            var result = new Result(Methods.HillClimb(boardValues));
            watch.Stop();

            var time = watch.ElapsedMilliseconds / 1000f;

            BoardHelper.Print(result, "Found board:");

            JsonHelper.WriteJsonFile(result, time, "output.json");
            Console.ReadKey();
        }
    }
}