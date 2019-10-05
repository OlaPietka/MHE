using System;

namespace Nonogram
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = 3;
            var m = 5;

            var board = new Board(n, m);
            var playerBoard = Helper.GenerateRandomGameBoard(n, m);

            #region Visualisation
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                    Console.Write(board.GetCorrectBoard()[i, j] ? 'O' : 'X');
                Console.Write("\n");
            }

            Console.Write("\n");

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                    Console.Write(playerBoard[i, j] ? 'O' : 'X');
                Console.Write("\n");
            }
            #endregion

            Console.WriteLine(board.Check(playerBoard));
            Console.ReadKey();
        }
    }
}