using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    public static class BoardHelper
    {
        public static int CheckForErrors(BoardValues boardValues, bool[,] filledBoard)
        {
            var errors = 0;

            void CountErrors(List<int>[] correctNumbers, List<int>[] filledNumbers, int dimension)
            {
                for (var i = 0; i < filledBoard.GetLength(dimension); i++)
                {
                    var correctNumbersCount = correctNumbers[i].Count;
                    var filedNumbersCount = filledNumbers[i].Count;

                    if (correctNumbersCount == filedNumbersCount)
                    {
                        foreach (var (Correct, Filled) in correctNumbers[i].Zip(filledNumbers[i], (c, f) => (Correct: c, Filled: f)))
                            if (Correct != Filled)
                                errors += Math.Abs(Correct - Filled);
                    }
                    else if (correctNumbersCount == 0 ^ filedNumbersCount == 0)
                        foreach (var x in correctNumbersCount == 0 ? filledNumbers[i] : correctNumbers[i])
                            errors += x;
                    else
                    {
                        (List<int> longerList, int shorterListCount) = correctNumbersCount < filedNumbersCount ?
                            (filledNumbers[i], correctNumbersCount) : (correctNumbers[i], filedNumbersCount);

                        for (var j = 0; j < shorterListCount; j++)
                            if (correctNumbers[i][j] != filledNumbers[i][j])
                                errors += Math.Abs(correctNumbers[i][j] - filledNumbers[i][j]);

                        longerList.GetRange(shorterListCount, longerList.Count - shorterListCount).ForEach(x => {
                            errors += x;
                        });
                    }
                }
            }

            var filledRowsNumbers = Generator.GenerateNumbers(filledBoard, dimension: 0);
            var filledColumnsNumbers = Generator.GenerateNumbers(filledBoard, dimension: 1);

            CountErrors(boardValues.RowsNumbers, filledRowsNumbers, dimension: 0);
            CountErrors(boardValues.ColumnsNumbers, filledColumnsNumbers, dimension: 1);

            return errors;
        }

        public static List<bool[,]> GetNeighbours(bool[,] currentBoard)
        {
            var neighbours = new List<bool[,]>();

            for (var i = 0; i < currentBoard.GetLength(0); i++)
                for (var j = 0; j < currentBoard.GetLength(1); j++)
                {
                    var newBoard = currentBoard.Clone() as bool[,];
                    newBoard[i, j] = !newBoard[i, j];
                    neighbours.Add(newBoard);
                }

            return neighbours;
        }

        public static void Print(Result result, string someNote = "")
        {
            Console.WriteLine(someNote);
            for (var i = 0; i < result.Board.GetLength(0); i++)
            {
                for (var j = 0; j < result.Board.GetLength(1); j++)
                    if (result.Board[i, j])
                        Console.Write('X');
                    else
                        Console.Write('O');
                Console.Write("\n");
            }
            Console.WriteLine($"Error: {result.Error}\n");
        }
    }
}