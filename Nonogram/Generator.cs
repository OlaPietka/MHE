using System;
using System.Collections.Generic;

namespace Nonogram
{
    public static class Generator
    {
        private static readonly Random _random = new Random();

        public static bool[,] GenerateRandomBoard(int n, int m)
        {
            var board = new bool[n, m];

            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    board[i, j] = _random.Next(0, 10) % 2 != 0;

            return board;
        }

        public static List<int>[] GenerateNumbers(bool[,] board, int dimension)
        {
            var n = board.GetLength(0);
            var m = board.GetLength(1);

            (var x, var y) = dimension == 0 ? (n, m) : (m, n);

            var counter = 0;
            var numbers = new List<int>[x];

            for (var i = 0; i < x; i++)
                numbers[i] = new List<int>();

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    if (dimension == 0 ? board[i, j] : board[j, i])
                    {
                        counter++;

                        if (j == y - 1)
                            numbers[i].Add(counter);
                    }
                    else
                    {
                        if (counter > 0)
                            numbers[i].Add(counter);
                        counter = 0;
                    }
                }
                counter = 0;
            }

            return numbers;
        }

        public static IEnumerable<bool[,]> GenerateCombinations(int rows, int columns)
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

                yield return t;
            }
        }
    }
}
