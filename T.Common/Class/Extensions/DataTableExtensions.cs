using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

namespace T.Common
{
    public static class DataTableExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> ien) where T : class
        {
            DataTable dataTable = new DataTable();
            Type type = (Type)null;
            foreach (T obj1 in ien)
            {
                object obj2 = (object)obj1;
                if (type.IsNull())
                    type = obj2.GetType();
                PropertyInfo[] properties = type.GetProperties();
                if (dataTable.Columns.Count == 0)
                {
                    foreach (PropertyInfo prop in properties)
                    {
                        if (!prop.IsIdentity())
                            dataTable.Columns.Add(prop.Name);
                    }
                }
                DataRow row = dataTable.NewRow();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        object obj3 = propertyInfo.GetValue(obj2, (object[])null);
                        row[propertyInfo.Name] = obj3;
                    }
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        public static DataTable CsvToDataTable(this string filePath)
        {
            if (File.Exists(filePath))
                return new FileInfo(filePath).CsvToDataTable();
            return (DataTable)null;
        }

        public static DataTable CsvToDataTable(this FileInfo fi)
        {
            try
            {
                DataTable dataTable = new DataTable();
                using (StreamReader streamReader = new StreamReader(fi.FullName, Encoding.GetEncoding("windows-1254")))
                {
                    string[] strArray1 = streamReader.ReadLine().Split(';');
                    foreach (string columnName in strArray1)
                        dataTable.Columns.Add(columnName);
                    while (!streamReader.EndOfStream)
                    {
                        string[] strArray2 = streamReader.ReadLine().Split(';');
                        DataRow row = dataTable.NewRow();
                        for (int index = 0; index < strArray1.Length; ++index)
                            row[index] = (object)strArray2[index];
                        dataTable.Rows.Add(row);
                    }
                }
                return dataTable;
            }
            catch
            {
                return (DataTable)null;
            }
        }

        public static DataTable CsvToDataTable(this string[] content)
        {
            try
            {
                DataTable dataTable = new DataTable();
                string[] strArray1 = content[0].Split(';');
                foreach (string columnName in strArray1)
                    dataTable.Columns.Add(columnName);
                for (int index1 = 1; index1 < content.Length; ++index1)
                {
                    string[] strArray2 = content[index1].Split(';');
                    DataRow row = dataTable.NewRow();
                    for (int index2 = 0; index2 < strArray1.Length; ++index2)
                        row[index2] = (object)strArray2[index2];
                    dataTable.Rows.Add(row);
                }
                return dataTable;
            }
            catch
            {
                return null;
            }
        }
    }
}
