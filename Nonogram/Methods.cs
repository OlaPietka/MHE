using System;
using System.Collections.Generic;
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
                foreach (var b in BoardHelper.GetNeighbours(currentResult.Board))
                {
                    var error = BoardHelper.CheckForErrors(boardValues, b);

                    if (currentResult.Error > error)
                    {
                        newResult.Error = error;
                        newResult.Board = b.Clone() as bool[,];
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
    }
}
