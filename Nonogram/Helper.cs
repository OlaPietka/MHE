using System;
using System.Collections.Generic;

namespace Nonogram
{
    public static class Helper
    {
        private static readonly Random _random = new Random(); //lol

        public static bool[,] GenerateRandomGameBoard(int n, int m)
        {
            var board = new bool[n, m];

            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    board[i, j] = Convert.ToBoolean(_random.Next(0, 10) % 2);

            return board;
        }

        public static List<int>[] GenerateRowsNumbers(bool[,] board)
        {
            var n = board.GetLength(0);
            var m = board.GetLength(1);
            var rowsNumbers = new List<int>[n];
            var counter = 0;

            Initialize(rowsNumbers, n);

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    if (board[i, j])
                    {
                        counter++;

                        if (j == m - 1)
                            rowsNumbers[i].Add(counter);
                    }
                    else
                    {
                        if (counter > 0)
                            rowsNumbers[i].Add(counter);
                        counter = 0;
                    }
                }
                counter = 0;
            }

            return rowsNumbers;
        }

        public static List<int>[] GenerateColumnsNumbers(bool[,] board)
        {
            var n = board.GetLength(0);
            var m = board.GetLength(1);
            var columsnNumbers = new List<int>[m];
            var counter = 0;

            Initialize(columsnNumbers, m);

            for (var i = 0; i < m; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (board[j, i])
                    {
                        counter++;

                        if (j == n - 1)
                            columsnNumbers[i].Add(counter);
                    }
                    else
                    {
                        if (counter > 0)
                            columsnNumbers[i].Add(counter);
                        counter = 0;
                    }
                }
                counter = 0;
            }

            return columsnNumbers;
        }

        private static void Initialize(List<int>[] array, int lenght)
        {
            for (var i = 0; i < lenght; i++)
                array[i] = new List<int>();
        }
    }
}