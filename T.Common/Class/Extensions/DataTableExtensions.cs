using System;
using System.Collections;
using System.Data;
using System.Reflection;

namespace T.Common
{
    public static class DataTableExtensions
    {
        public static DataTable ToDataTable(this IEnumerable ien)
        {
            DataTable table = new DataTable();
            Type type = null;
            PropertyInfo[] pis;

            foreach (object obj in ien)
            {
                if (type.IsNull())
                    type = obj.GetType();
                pis = type.GetProperties();

                if (table.Columns.Count == 0)
                {
                    foreach (PropertyInfo pi in pis)
                    {
                        if (!pi.IsIdentity())
                            table.Columns.Add(pi.Name);
                    }
                }

                DataRow dr = table.NewRow();
                foreach (PropertyInfo pi in pis)
                {
                    if (!table.Columns.Contains(pi.Name))
                        continue;

                    object value = pi.GetValue(obj, null);
                    dr[pi.Name] = value;
                }
                table.Rows.Add(dr);
            }
            return table;
        }
    }
}
