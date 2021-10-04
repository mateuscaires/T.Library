using T.Common;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace T.Tools
{
    public class Excel
    {
        private static Excel _instance;
        private IWorkbook _workbook;
        private int _sheetIndex;
        private string @string;
        private double @double;
        private bool _treatsNumber;
        private string _sheetName;
        private List<string> _sheetNames;
        private const string CT_SHEET_NAME = "Planilha";

        public static Excel Instance
        {
            get
            {
                if (Excel._instance == null)
                    Excel._instance = new Excel();
                return Excel._instance;
            }
        }

        public Excel()
        {
            this._sheetNames = new List<string>();
        }

        public DataTable ExcelToDataTable(string path)
        {
            DataTable dataTable1 = (DataTable)null;
            if (!File.Exists(path))
                return dataTable1;
            DataTable dataTable2 = new DataTable();
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                return this.ExcelToDataTable((Stream)fileStream);
        }

        public DataTable ExcelToDataTable(byte[] content)
        {
            return this.ExcelToDataTable((Stream)new MemoryStream(content));
        }

        public DataTable ExcelToDataTable(Stream stream)
        {
            return this.GetDataTableFromSheet(new XSSFWorkbook(stream).GetSheetAt(0));
        }

        public DataSet ExcelToDataSet(string path)
        {

            XSSFWorkbook workbook;

            DataSet dataSet = new DataSet();

            Action<string> ReadFile = (file) =>
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    workbook = new XSSFWorkbook(fs);

                    for (int i = 0; i < workbook.NumberOfSheets; i++)
                    {
                        ISheet sheet = workbook.GetSheetAt(i);

                        DataTable table = GetDataTableFromSheet(sheet);
                        if (table.HasRows())
                            dataSet.Tables.Add(table);
                    }
                }
            };

            try
            {
                ReadFile(path);
            }
            catch (IOException)
            {
                try
                {
                    FileInfo fi = new FileInfo(path);

                    string tempPath = path.Insert(path.IndexOf(fi.Extension), Guid.NewGuid().ToString());

                    File.Copy(path, tempPath);

                    ReadFile(tempPath);

                    File.Delete(tempPath);
                }
                catch
                {

                }
            }

            return dataSet;
        }

        public void OjectsToExcel<T>(IEnumerable<T> items, string path) where T : class
        {
            this.OjectsToExcel<T>(items, path, false);
        }

        public void OjectsToExcel<T>(IEnumerable<T> items, string path, bool treatsNumber) where T : class
        {
            this.DataTableToExcel(items.ToDataTable<T>(), path, treatsNumber);
        }

        public void DataSetToExcel(DataSet dataSet, string path)
        {
            this.DataSetToExcel(dataSet, path, false);
        }

        public void DataSetToExcel(DataSet dataSet, string path, bool treatsNumber)
        {
            DataTable[] tables = new DataTable[dataSet.Tables.Count];
            for (int index = 0; index < tables.Length; ++index)
                tables[index] = dataSet.Tables[index];
            this.DataTableToExcel(tables, path, treatsNumber);
        }

        public void DataTableToExcel(DataTable[] tables, string path)
        {
            this.DataTableToExcel(tables, path, false);
        }

        public void DataTableToExcel(DataTable[] tables, string path, bool treatsNumber)
        {
            this._treatsNumber = treatsNumber;
            this._sheetNames.Clear();
            this._sheetIndex = 0;
            foreach (DataTable table in tables)
                this.Write(table);
            this.Save(path);
        }

        public byte[] DataTableToExcel(DataTable data)
        {
            string tempPath = Path.GetTempPath();
            string path = (tempPath.EndsWith("\\") ? (object)tempPath : (object)(tempPath + "\\")).ToString() + (object)Guid.NewGuid() + ".xlsx";
            this.DataTableToExcel(data, path, false);
            byte[] numArray = File.ReadAllBytes(path);
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
            return numArray;
        }

        public void DataTableToExcel(DataTable data, string path)
        {
            this.DataTableToExcel(data, path, false);
        }

        public void DataTableToExcel(DataTable data, string path, bool treatsNumber)
        {
            this._treatsNumber = treatsNumber;
            this._sheetIndex = 0;
            this.Write(data);
            this.Save(path);
        }

        private void SetSheetName(DataTable table)
        {
            ++this._sheetIndex;
            this._sheetName = string.Empty;
            this._sheetName = !table.HasValue() || table.TableName.IsNullOrEmpty() ? "Planilha" + " " + (object)this._sheetIndex : table.TableName.RemoveSpecialChar('-');
            this._sheetNames.Add(this._sheetName);
        }

        private void Write(DataTable data)
        {
            if (this._sheetIndex == 0)
                this._workbook = (IWorkbook)new XSSFWorkbook();
            this.SetSheetName(data);
            ISheet sheet = this._workbook.CreateSheet(this._sheetName);
            IRow row1 = sheet.CreateRow(0);
            Type valueType = (Type)null;
            IDataFormat dataFormat = this._workbook.CreateDataFormat();
            ICellStyle cellDateStyle = this._workbook.CreateCellStyle();
            cellDateStyle.BorderBottom = BorderStyle.None;
            cellDateStyle.BorderLeft = BorderStyle.None;
            cellDateStyle.BorderTop = BorderStyle.None;
            cellDateStyle.BorderRight = BorderStyle.None;
            ICell cell;
            DateTime date;
            Action<IRow, int, object> action = (Action<IRow, int, object>)((row, index, value) =>
            {
                if (this._treatsNumber)
                {
                    this.@string = value.ToString();
                    if (this.@string.IsNumeric() && double.TryParse(this.@string, out this.@double))
                    {
                        cell = row.CreateCell(index, CellType.Numeric);
                        cell.SetCellValue(this.@double);
                        return;
                    }
                }
                cell = row.CreateCell(index);
                switch (Type.GetTypeCode(valueType))
                {
                    case TypeCode.Boolean:
                        cell.SetCellValue((bool)value);
                        break;
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        if (double.TryParse(value.ToString(), out this.@double))
                        {
                            cell.SetCellValue(this.@double);
                            break;
                        }
                        cell.SetCellValue(value.ToString());
                        break;
                    case TypeCode.DateTime:
                        date = (DateTime)value;
                        cellDateStyle.DataFormat = date.Hour <= 0 && date.Minute <= 0 && date.Second <= 0 ? dataFormat.GetFormat("dd/MM/yyyy") : dataFormat.GetFormat("dd/MM/yyyy HH:mm:ss");
                        cell.SetCellValue((DateTime)value);
                        cell.CellStyle = cellDateStyle;
                        break;
                    default:
                        cell.SetCellValue(value.ToString());
                        break;
                }
            });
            if (data.IsNull())
                return;
            foreach (DataColumn column in (InternalDataCollectionBase)data.Columns)
                action(row1, column.Ordinal, (object)column.ColumnName);
            int rownum = 0;
            foreach (DataRow row2 in (InternalDataCollectionBase)data.Rows)
            {
                ++rownum;
                IRow row3 = sheet.CreateRow(rownum);
                foreach (DataColumn column in (InternalDataCollectionBase)data.Columns)
                {
                    valueType = row2[column].GetType();
                    action(row3, column.Ordinal, row2[column]);
                }
            }
        }

        private void Save(string path)
        {
            if (path.IsNullOrEmpty())
                return;
            char[] ignore = new char[3] { '\\', '-', '_' };
            path = path.RemoveSpecialChar(ignore);
            FileMode mode = File.Exists(path) ? FileMode.Truncate : FileMode.Create;
            using (FileStream fileStream = new FileStream(path, mode, FileAccess.ReadWrite))
            {
                this._workbook.Write((Stream)fileStream);
                this._workbook = null;
                this._treatsNumber = false;
                this._sheetIndex = 0;
            }
        }
        
        private DataTable GetDataTableFromSheet(ISheet sheet)
        {
            if (sheet.IsNull())
                return null;

            DataTable table = new DataTable();

            table.TableName = sheet.SheetName;

            IRow row = sheet.GetRow(0);

            if (row.IsNull())
                return null;

            string colName = string.Empty;

            foreach (ICell cell in row.Cells)
            {
                switch (cell.CellType)
                {
                    case CellType.Numeric:
                        colName = cell.NumericCellValue.ToString();
                        break;
                    case CellType.String:
                    case CellType.Formula:
                        colName = cell.StringCellValue;
                        break;
                    case CellType.Blank:
                        colName = string.Empty;
                        break;
                    case CellType.Boolean:
                        colName = cell.BooleanCellValue.ToString();
                        break;
                    case CellType.Unknown:
                    case CellType.Error:
                    default:
                        colName = string.Empty;
                        break;
                }

                table.Columns.Add(colName.Trim());
            }

            int rowIndex = 1;

            row = sheet.GetRow(rowIndex);

            Func<DataRow, bool> IsEmptyRow = (dr) =>
            {
                foreach (var item in dr.ItemArray)
                {
                    if (item.HasValue())
                    {
                        return false;
                    }
                }
                return true;
            };

            while (row.HasValue())
            {
                DataRow dr = table.NewRow();

                foreach (ICell cell in row.Cells)
                {
                    if (cell.HasValue() && cell.ColumnIndex < table.ColumnsCount())
                    {
                        switch (cell.CellType)
                        {
                            case CellType.Numeric:
                                {
                                    if (cell.ToString() == cell.NumericCellValue.ToString())
                                        dr[cell.ColumnIndex] = cell.NumericCellValue;
                                    else
                                        dr[cell.ColumnIndex] = cell.DateCellValue;
                                }
                                break;
                            case CellType.Boolean:
                                dr[cell.ColumnIndex] = cell.BooleanCellValue;
                                break;
                            case CellType.Formula:
                                dr[cell.ColumnIndex] = cell.CellFormula;
                                break;
                            case CellType.Error:
                                dr[cell.ColumnIndex] = ((XSSFCell)cell).ErrorCellString;
                                break;
                            case CellType.String:
                            default:
                                dr[cell.ColumnIndex] = cell.StringCellValue;
                                break;
                        }
                    }
                }

                if (!IsEmptyRow(dr))
                    table.Rows.Add(dr);

                rowIndex++;
                row = sheet.GetRow(rowIndex);
            }

            return table;
        }
    }
}
