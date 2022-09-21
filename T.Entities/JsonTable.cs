using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace T.Entities
{
  public class JsonTable
  {
    private DataTable _table;
    private List<TableHeader> _headers;
    private bool _hasRows;
    private int _rowsCount;
    private StringBuilder _content;

    public JsonTable(DataTable table)
    {
      this._table = table;
      this._hasRows = this._table != null && this._table.Rows.Count > 0;
      this.Fill();
    }

    public List<TableHeader> Headers
    {
      get
      {
        return this._headers;
      }
    }

    public DataTable Table
    {
      get
      {
        return this._table;
      }
    }

    private void Fill()
    {
      this.SetHeaders();
    }

    private void SetHeaders()
    {
      this._headers = new List<TableHeader>();
      if (!this._hasRows)
        return;
      this._rowsCount = this._table.Rows.Count;
      int num = 0;
      foreach (DataColumn column in (InternalDataCollectionBase) this._table.Columns)
      {
        ++num;
        this._headers.Add(new TableHeader()
        {
          Id = num,
          Value = column.ColumnName,
          DataType = column.DataType.Name
        });
      }
    }

    private void SetContent()
    {
      this._content = new StringBuilder();
      if (!this._hasRows)
        return;
      int index = 0;
      this._content.Append("[");
      foreach (DataRow row in (InternalDataCollectionBase) this._table.Rows)
      {
        this._content.Append(this.GetRowContent(row, index));
        ++index;
      }
      this._content.Append("]");
    }

    private string GetRowContent(DataRow row, int index)
    {
      StringBuilder stringBuilder = new StringBuilder();
      DataColumnCollection columns = row.Table.Columns;
      stringBuilder.Append("{");
      for (int index1 = 0; index1 < columns.Count; ++index1)
        stringBuilder.Append("\"").Append(columns[index1].ColumnName.ToString()).Append("\"").Append(":").Append("\"").Append(row[index1].ToString()).Append(index1 == columns.Count - 1 ? "\"" : "\",");
      if (index == this._rowsCount - 1)
        stringBuilder.Append("}").Append(Environment.NewLine);
      else
        stringBuilder.Append("},");
      return stringBuilder.ToString();
    }
  }
}
