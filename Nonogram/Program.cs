using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    class Program
    {
        static void Main(string[] args)
        {
            var boardValues = JsonHelper.ReadJsonFile("input.json");

            var min = int.MaxValue;
            var finalBoard = new bool[boardValues.RowCount, boardValues.ColumnCount];

            foreach (var board in GenerateCombinations(boardValues.RowCount, boardValues.ColumnCount))
            {
                var errors = CheckForErrors(boardValues, board);

                if (errors < min)
                {
                    min = errors;
                    finalBoard = board;
                }
            }

            for (var i = 0; i < finalBoard.GetLength(0); i++)
            {
                for (var j = 0; j < finalBoard.GetLength(1); j++)
                    if (finalBoard[i, j])
                        Console.Write('X');
                    else
                        Console.Write('O');
                Console.Write("\n");
            }

            JsonHelper.WriteJsonFile(finalBoard, min, "output.json");

            Console.ReadKey();
        }

        private static int CheckForErrors(BoardValues boardValues, bool[,] filledBoard)
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

            var filledRowsNumbers = BoardHelper.GenerateNumbers(filledBoard, dimension: 0);
            var filledColumnsNumbers = BoardHelper.GenerateNumbers(filledBoard, dimension: 1);

            CountErrors(boardValues.RowsNumbers, filledRowsNumbers, dimension: 0);
            CountErrors(boardValues.ColumnsNumbers, filledColumnsNumbers, dimension: 1);

            return errors;
        }

        private static List<bool[,]> GenerateCombinations(int rows, int columns)
        {
            IEnumerable<int> Generator(int limit, long duplicates)
            {
                while (true)
                {
                    for (var i = 0; i < limit; i++)
                    {
                        for (var c = 0; c < duplicates; c++)
                            yield return i;
                    }
                }
            }

            var valueLimit = (int)Math.Pow(2, columns);
            var combinations = (long)Math.Pow(valueLimit, rows);

            var enumerators = new List<IEnumerator<int>>();
            for (var i = 0; i < rows; i++)
                enumerators.Add(Generator(valueLimit, (long)Math.Pow(valueLimit, rows - i - 1)).GetEnumerator());

            var result = new List<bool[,]>();
            for (long i = 0; i < combinations; i++)
            {
                var t = new bool[rows, columns];

                for (var y = 0; y < rows; y++)
                {
                    var e = enumerators[y];
                    e.MoveNext();
                    var current = e.Current;
                    for (var x = 0; x < columns; x++)
                        t[y, x] = ((current >> (columns - x - 1)) & 1) == 1;

                }

                result.Add(t);
            }

            return result;
        }
    }
}