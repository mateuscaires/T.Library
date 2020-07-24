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
                if (_instance == null)
                    _instance = new Excel();

                return _instance;
            }
        }

        public Excel()
        {
            _sheetNames = new List<string>();
        }

        public DataTable ExcelToDataTable(string path)
        {
            DataTable table = null;

            if (File.Exists(path))
            {
                table = new DataTable();
            }
            else
                return table;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return ExcelToDataTable(fs);
            }            
        }

        public DataTable ExcelToDataTable(byte[] content)
        {
            return ExcelToDataTable(new MemoryStream(content));
        }

        public DataTable ExcelToDataTable(Stream stream)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(stream);

            ISheet sheet = workbook.GetSheetAt(0);

            DataTable table = GetDataTableFromSheet(sheet);
            
            return table;
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
                        if(table.HasRows())
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

        public void OjectsToExcel<T>(IEnumerable<T> items, string path)
        {
            OjectsToExcel(items, path, false);
        }

        public void OjectsToExcel<T>(IEnumerable<T> items, string path, bool treatsNumber)
        {
            DataTable table = items.ToDataTable();
            
            DataTableToExcel(table, path, treatsNumber);
        }

        public void DataSetToExcel(DataSet dataSet, string path)
        {
            DataSetToExcel(dataSet, path, false);
        }

        public void DataSetToExcel(DataSet dataSet, string path, bool treatsNumber)
        {
            DataTable[] tables = new DataTable[dataSet.Tables.Count];

            for (int i = 0; i < tables.Length; i++)
            {
                tables[i] = dataSet.Tables[i];
            }            

            DataTableToExcel(tables, path, treatsNumber);
        }

        public void DataTableToExcel(DataTable[] tables, string path)
        {
            DataTableToExcel(tables, path, false);
        }

        public void DataTableToExcel(DataTable[] tables, string path, bool treatsNumber)
        {
            _treatsNumber = treatsNumber;
            _sheetNames.Clear();
            _sheetIndex = 0;

            foreach (DataTable table in tables)
            {
                Write(table);
            }

            Save(path);
        }

        public byte[] DataTableToExcel(DataTable data)
        {
            string path = Path.GetTempPath();

            path = path.EndsWith(@"\") ? path : string.Concat(path, @"\");

            path = string.Concat(path, Guid.NewGuid(), ".xlsx");

            DataTableToExcel(data, path, false);

            byte[] content = File.ReadAllBytes(path);

            try
            {
                File.Delete(path);
            }
            catch
            {

            }

            return content;
        }

        public void DataTableToExcel(DataTable data, string path)
        {
            DataTableToExcel(data, path, false);
        }

        public void DataTableToExcel(DataTable data, string path, bool treatsNumber)
        {
            _treatsNumber = treatsNumber;
            _sheetIndex = 0;
            Write(data);
            Save(path);
        }

        private void SetSheetName(DataTable table)
        {
            _sheetIndex++;
            _sheetName = string.Empty;

            if (!table.TableName.IsNullOrEmpty())
                _sheetName = table.TableName.RemoveSpecialChar('-');
            else
                _sheetName = string.Concat(CT_SHEET_NAME, " ", _sheetIndex);

            _sheetNames.Add(_sheetName);
        }

        private void Write(DataTable data)
        {
            if(_sheetIndex == 0)
                _workbook = new XSSFWorkbook();

            SetSheetName(data);

            ISheet sheet = _workbook.CreateSheet(_sheetName);

            //make a header row  
            IRow header = sheet.CreateRow(0);

            Type valueType = default(Type);
            ICell cell;

            IDataFormat dataFormat = _workbook.CreateDataFormat();
            
            ICellStyle cellDateStyle = _workbook.CreateCellStyle();
            cellDateStyle.BorderBottom = BorderStyle.None;
            cellDateStyle.BorderLeft = BorderStyle.None;
            cellDateStyle.BorderTop = BorderStyle.None;
            cellDateStyle.BorderRight = BorderStyle.None;
            
            DateTime date;
            
            Action<IRow, int, object> CreateCell = (row, index, value) =>
            {
                if (_treatsNumber)
                {
                    @string = value.ToString();

                    if (@string.IsNumeric())
                    {
                        if (double.TryParse(@string, out @double))
                        {
                            cell = row.CreateCell(index, CellType.Numeric);
                            cell.SetCellValue(@double);
                            return;
                        }
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
                        if (double.TryParse(value.ToString(), out @double))
                            cell.SetCellValue(@double);
                        else
                            cell.SetCellValue(value.ToString());
                        break;
                    case TypeCode.DateTime:
                        {
                            date = (DateTime)value;

                            if(date.Hour > 0 || date.Minute > 0 || date.Second > 0)
                                cellDateStyle.DataFormat = dataFormat.GetFormat("dd/MM/yyyy HH:mm:ss");
                            else
                                cellDateStyle.DataFormat = dataFormat.GetFormat("dd/MM/yyyy");

                            cell.SetCellValue(((DateTime)value));
                            cell.CellStyle = cellDateStyle;
                        }
                        break;
                    case TypeCode.String:
                    default:
                        cell.SetCellValue(value.ToString());
                        break;
                }
            };

            foreach (DataColumn column in data.Columns)
            {
                CreateCell(header, column.Ordinal, column.ColumnName);
            }

            int rowIndex = 0;

            foreach (DataRow datarow in data.Rows)
            {
                rowIndex++;
                IRow row = sheet.CreateRow(rowIndex);
                foreach (DataColumn column in data.Columns)
                {
                    valueType = datarow[column].GetType();
                    CreateCell(row, column.Ordinal, datarow[column]);
                }
            }
        }

        private void Save(string path)
        {
            if (path.IsNullOrEmpty())
                return;

            char[] ignore = new[] { '\\', '-', '_'};

            path = path.RemoveSpecialChar(ignore);

            FileMode mode = File.Exists(path) ? FileMode.Truncate : FileMode.Create;

            using (FileStream fs = new FileStream(path, mode, FileAccess.ReadWrite))
            {
                _workbook.Write(fs);
                _workbook = null;
                _treatsNumber = false;
                _sheetIndex = 0;
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
                        if(item.HasValue())
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
                                    if(cell.ToString() == cell.NumericCellValue.ToString())
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

                if(!IsEmptyRow(dr))
                    table.Rows.Add(dr);

                rowIndex++;
                row = sheet.GetRow(rowIndex);
            }

            return table;
        }
    }
}
