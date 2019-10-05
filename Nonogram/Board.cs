using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonogram
{
    public class Board
    {
        private readonly List<int>[] _rowsNumbers; //to do 2
        private readonly List<int>[] _columnsNumbers;
        private readonly int _rowCount;
        private readonly int _columnCount;
        
        // Only for visualisation
        private readonly bool[,] _correctBoard;

        public Board(int rowCount, int columnCount)
        {
            _rowCount = rowCount;
            _columnCount = columnCount;
            _correctBoard = Helper.GenerateRandomGameBoard(rowCount, columnCount);
            _rowsNumbers = Helper.GenerateRowsNumbers(_correctBoard);
            _columnsNumbers = Helper.GenerateColumnsNumbers(_correctBoard);
        }

        // Only for visualisation
        public bool[,] GetCorrectBoard()
            => _correctBoard;

        public int Check(bool[,] filledBoard)
        {
            var filledRowsNumbers = Helper.GenerateRowsNumbers(filledBoard);
            var filledColumnsNumbers = Helper.GenerateColumnsNumbers(filledBoard);

            var numberOfErrors = 0;

            void CountErrors(List<int>[] listOfCorrectValues, List<int>[] listOfFillesValues)
            {
                for (var i = 0; i < filledBoard.GetLength(0); i++)
                {
                    if (listOfCorrectValues[i].Count == listOfFillesValues[i].Count)
                    {
                        foreach (var list in listOfCorrectValues[i].Zip(listOfFillesValues[i], (c, f) => (Correct: c, Filled: f)))
                            if (!(list.Correct == list.Filled))
                                numberOfErrors += Math.Abs(list.Correct - list.Filled);
                    }
                    else
                        foreach (var difference in listOfCorrectValues[i].Except(listOfFillesValues[i]))
                            numberOfErrors += difference;
                }
            }

            CountErrors(_rowsNumbers, filledRowsNumbers);
            CountErrors(_columnsNumbers, filledColumnsNumbers);

            return numberOfErrors;
        }
    }
}