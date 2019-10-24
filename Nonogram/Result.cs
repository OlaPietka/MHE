namespace Nonogram
{
    public class Result
    {
        public bool[,] Board { get; set; }
        public int Error { get; set; }
        public float Time { get; set; }

        public Result(bool[,] board, int error)
        {
            Board = board;
            Error = error;
        }

        public Result(Result result)
        {
            Board = result.Board.Clone() as bool[,];
            Error = result.Error;
        }

        public static bool operator ==(Result a, Result b)
        {
            if (a.Board.Length != b.Board.Length)
                return false;

            for (var i = 0; i < a.Board.GetLength(0); i++)
                for (var j = 0; j < a.Board.GetLength(1); j++)
                    if (a.Board[i, j] != b.Board[i, j])
                        return false;

            return true;
        }

        public static bool operator !=(Result a, Result b)
        {
            if (a.Board.Length != b.Board.Length)
                return true;

            for (var i = 0; i < a.Board.GetLength(0); i++)
                for (var j = 0; j < a.Board.GetLength(1); j++)
                    if (a.Board[i, j] != b.Board[i, j])
                        return true;

            return false;
        }
    }
}
