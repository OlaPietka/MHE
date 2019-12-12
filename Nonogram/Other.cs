using System;

namespace Nonogram
{
    public static class Other
    {
        public static (int iteration, int parameter) RunExperiment(BoardValues boardValues)
        {
            var iteration = 0;
            var parameter = 0;
            var repeat = 10;

            while(true)
            {
                var sol = Method.SimulatedAnnealing(boardValues, iteration, parameter, write: false);

                Console.WriteLine($"iteration: {iteration} - parameter {parameter} - error {sol.Error}");

                if (sol.Error == 0)
                {
                    --repeat;

                    if (repeat == 0)
                        break;
                }
                else
                {
                    repeat = 10;
                    parameter += 100;
                    iteration += 50;
                }
            }

            Console.WriteLine($"--- ITERATION: {iteration}, PARAMETER: {parameter} ---");
            return (iteration, parameter);
        }
    }
}
