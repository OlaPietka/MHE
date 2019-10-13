using System.Collections.Generic;

namespace Nonogram
{
    public class BoardValues
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<int>[] RowsNumbers { get; set; }
        public List<int>[] ColumnsNumbers { get; set; }
    }
}