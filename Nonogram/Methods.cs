using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

            return new Result(bestBoard, minError);
        }

        public static Result HillClimb(BoardValues boardValues, int iteration = 1000)
        {
            Console.WriteLine("---HILLCLIMB---\n");

            var randomBoard = Generator.GenerateRandomBoard(boardValues.RowCount, boardValues.ColumnCount);
            var currentResult = new Result(randomBoard, BoardHelper.CheckForErrors(boardValues, randomBoard));

            var newResult = new Result(currentResult);

            BoardHelper.Print(currentResult, "Generated random board:");

            for (var i = 0; i < iteration; i++)
            {
                foreach (var neighbour in Generator.GenerateNeighbours(currentResult))
                {
                    neighbour.Error = BoardHelper.CheckForErrors(boardValues, neighbour.Board);

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

            var randomBoard = Generator.GenerateRandomBoard(boardValues.RowCount, boardValues.ColumnCount);
            var currentResult = new Result(randomBoard, BoardHelper.CheckForErrors(boardValues, randomBoard));

            tabuList.Add(currentResult);

            var globalBest = tabuList.Last();

            BoardHelper.Print(currentResult, "Generated random board:");

            for (var i = 0; i < iteration; i++)
            {
                var neighbours = Generator.GenerateNeighbours(tabuList.Last());

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
                        neighbour.Error = BoardHelper.CheckForErrors(boardValues, neighbour.Board);
                        if (neighbour.Error < currentBest.Error)
                            currentBest = neighbour;
                    }

                    tabuList.Add(currentBest);

                    if (currentBest.Error < globalBest.Error)
                        globalBest = currentBest;

                    if (tabuList.Count >= tabuSize)
                        tabuList.RemoveAt(0);
                }
            }

            return globalBest;
        }
    }
}
