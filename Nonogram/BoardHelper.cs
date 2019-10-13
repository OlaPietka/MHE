using System;
using System.Collections.Generic;

namespace Nonogram
{
    public static class BoardHelper
    {
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
    }
}