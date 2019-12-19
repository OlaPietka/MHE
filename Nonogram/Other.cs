using System;
using System.Collections.Generic;

namespace Nonogram
{
    public static class Other
    {
        public static (int iteration, int parameter) RunExperimentSimulatedAnnealing(BoardValues boardValues)
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

        public static (int populationSize, int iterationCount, double crossoverPropability,
            double mutationPropability, string crossoverMethod, string selectionMethod,
            string termConditionMethod)
            RunExperimentGenetic(BoardValues boardValues)
        {
            var iteration = 10;
            var bestResult = new Result();
            var bestParameters = (0, 0, 0.0, 0.0, "", "", "");

            foreach (var crossoverMethod in new List<string>() { "OnePoint", "TwoPoints" })
                foreach (var selectionMethod in new List<string>() { "Rulet", "Rank", "Tournament" })
                    foreach(var termConditionMethod in new List<string>() { "Iteration", "Mean", "Deviation" })
                        for(var crossoverProp = 0.0; crossoverProp <= 1.0; crossoverProp += 0.1)
                            for (var mutationProp = 0.0; mutationProp <= 1.0; mutationProp += 0.1)
                                for(var popSize = 10; popSize <= 100; popSize += 10)
                                {
                                    var result = Method.Genetic(boardValues, popSize, iteration, crossoverProp, mutationProp, crossoverMethod, selectionMethod, termConditionMethod);

                                    Console.WriteLine($"Error {result.Error}\n\n");
                                    Console.WriteLine($"Parameters {crossoverMethod} {selectionMethod} {termConditionMethod} {crossoverProp} {mutationProp} {popSize}");
                                    if (bestResult.Board == null) bestResult = new Result(result);

                                    if (bestResult.Error > result.Error)
                                    {
                                        bestResult = new Result(result);
                                        bestParameters = (popSize, iteration, crossoverProp, mutationProp, crossoverMethod, selectionMethod, termConditionMethod);
                                    }
                                }

            return bestParameters;
        }
    }
}
