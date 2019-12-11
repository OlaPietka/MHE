using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nonogram
{
    public static class Methods
    {
        public static Result BruteForce(BoardValues boardValues)
        {
            Console.WriteLine("---BRUTEFORCE---\n");

            var bestBoard = new bool[boardValues.RowCount, boardValues.ColumnCount];
            var minError = int.MaxValue;

            Parallel.ForEach(Generator.GenerateCombinations(boardValues.RowCount, boardValues.ColumnCount), board =>
            {
                var error = BoardHelper.CheckForErrors(boardValues, board);

                if (error < minError)
                {
                    minError = error;
                    bestBoard = board;
                }
            });

            return new Result(bestBoard, boardValues);
        }

        public static Result HillClimb(BoardValues boardValues, int iteration = 1000)
        {
            Console.WriteLine("---HILLCLIMB---\n");

            var randomBoard = Generator.GenerateRandomBoard(boardValues);
            var currentResult = new Result(randomBoard, boardValues);

            var newResult = new Result(currentResult);

            //BoardHelper.Print(currentResult, "Generated random board:");

            for (var i = 0; i < iteration; i++)
            {
                foreach (var neighbour in Generator.GenerateNeighbours(currentResult, boardValues))
                {
                    if (currentResult.Error > neighbour.Error)
                    {
                        newResult = new Result(neighbour);
                    }
                }
                
                if(currentResult == newResult)
                {
                    Console.WriteLine($"===End after {i} iteration===");
                    break;
                }

                currentResult = new Result(newResult);
            }

            return currentResult;
        }

        public static Result Tabu(BoardValues boardValues, int iteration = 1000, int tabuSize = 100)
        {
            Console.WriteLine("---TABU---\n");

            var tabuList = new List<Result>();

            var randomBoard = Generator.GenerateRandomBoard(boardValues);
            var currentResult = new Result(randomBoard, boardValues);

            tabuList.Add(currentResult);

            var globalBest = tabuList.Last();

            //BoardHelper.Print(currentResult, "Generated random board:");

            for (var i = 0; i < iteration; i++)
            {
                var neighbours = Generator.GenerateNeighbours(tabuList.Last(), boardValues);

                tabuList.ForEach(t =>
                {
                    neighbours.RemoveAll(x => x == t);
                });

                if (neighbours.Count <= 0)
                    if (tabuList.Count > 1)
                        tabuList.RemoveAt(0);
                    else
                        return globalBest;
                else
                {
                    var currentBest = neighbours.Last();

                    foreach(var neighbour in neighbours)
                    {
                        if (neighbour.Error < currentBest.Error)
                            currentBest = new Result(neighbour);
                    }

                    tabuList.Add(currentBest);

                    if (currentBest.Error < globalBest.Error)
                        globalBest = new Result(currentBest);

                    if (tabuList.Count >= tabuSize)
                        tabuList.RemoveAt(0);
                }
            }

            return globalBest;
        }

        public static Result SimulatedAnnealing(BoardValues boardValues, int iteration = 10000, double parameter = 40000.0, bool write = true, Func<int, double> T = null)
        {
            if (T == null)
                T = x => { return parameter / x; };

            var s = new List<Result>();
            var randomBoard = Generator.GenerateRandomBoard(boardValues);
            var currentResult = new Result(randomBoard, boardValues);

            s.Add(currentResult);

            for (var k = 0; k < iteration; k++)
            {
                var newSolution = Generator.GetRandomNeighbour(s.Last(), boardValues);

                if(newSolution.Error < s.Last().Error)
                    s.Add(newSolution);
                else
                {
                    double u = new Random().NextDouble();

                    var tkError = newSolution.Error;
                    var skError = s.Last().Error;

                    s.Add(u < Math.Exp(-Math.Abs(tkError - skError) / T(k)) ? newSolution : s.Last());
                }

                if(write && k % 100 == 0)
                    Console.WriteLine($"iteracja {k} - error {s.Last().Error}");
            }

            return s.Find(result => result.Error == s.Min(minResult => minResult.Error));
        }

        public static bool[,] Genetic(List<bool[,]> initialPopulation, Func<bool[,], double> fitness,
            Func<List<double>, int> selection, Func<bool[,], bool[,], (bool[,], bool[,])> crossover,
            Func<bool[,], bool[,]> mutation, Func<List<bool[,]>, bool> termCondition, double crossoverPropability,
            double mutationPropability)
        {
            var population = initialPopulation;

            while (termCondition(population))
            {
                var fit = new List<double>();
                var parents = new List<bool[,]>();
                var children = new List<bool[,]>();

                foreach (var specimen in population)
                    fit.Add(fitness(specimen));

                for (var i = 0; i < initialPopulation.Count; i++)
                    parents.Add(population[selection(fit)]);

                for (var i = 0; i < initialPopulation.Count - 1; i += 2)
                {
                    var u = new Random().NextDouble();

                    if (crossoverPropability < u)
                    {
                        (var a, var b) = crossover(parents[i], parents[i + 1]);
                        children.Add(a);
                        children.Add(b);
                    }
                    else
                    {
                        children.Add(parents[i]);
                        children.Add(parents[i + 1]);
                    }
                }

                for (var i = 0; i < initialPopulation.Count - 1; i += 2)
                {
                    var u = new Random().NextDouble();

                    if (mutationPropability < u)
                        children[i] = mutation(children[i]);
                }

                population = children;
            }

            return population.Find(x => fitness(x) == population.Min(y => fitness(y)));
        }
    }
}
