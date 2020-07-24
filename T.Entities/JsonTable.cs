using System.Collections.Generic;
using System.Data;

namespace T.Entities
{
    public class JsonTable
    {
        private DataTable _table;
        private List<string> _columns;

        public JsonTable(DataTable table)
        {
            _table = table;
            SetColumns();
        }

        public JsonTable() : this(null)
        {

        }

        public DataTable Table { get { return _table; } set { _table = value; SetColumns(); } }
        public List<string> Columns { get { return _columns; } }

        private void SetColumns()
        {
            _columns = new List<string>();
            if (_table != null && _table.Rows.Count > 0)
            {
                foreach (DataColumn item in _table.Columns)
                {
                    _columns.Add(item.ColumnName);
                }
            }
        }
    }
}
