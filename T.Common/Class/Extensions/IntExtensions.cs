using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Common
{
    public static class IntExtensions
    {
        public static int ToInt32(this string text)
        {
            try
            {
                int retorno = 0;
                if (int.TryParse(text, out retorno))
                    return retorno;
                else
                    return 0;

            }
            catch
            {
                return 0;
            }
        }

        public static int ToInt32(this Enum value)
        {
            int val = 0;
            try
            {
                val = (value.AmbienteValue() ?? string.Empty).ToInt32();

                if (val == 0)
                    val = value.GetHashCode();
            }
            catch
            {
                val = 0;
            }

            return val;
        }

        public static int ToInt32(this object text)
        {
            try
            {
                return Convert.ToInt32(text.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static int GetColumnIndex(this DataTable table, string columnName)
        {
            columnName = (columnName ?? string.Empty).Trim();

            string currentCol = string.Empty;

            int index = -1;

            try
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    currentCol = table.Columns[i].ColumnName.Trim();

                    if (columnName.Equals(currentCol, StringComparison.InvariantCultureIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }
            catch
            {

            }

            return index;
        }

        public static int ColumnsCount(this DataTable table)
        {
            try
            {
                if (table == null)
                    return 0;
                return table.Columns.Count;
            }
            catch
            {
                return 0;
            }
        }

        public static int Count(this DataTable table)
        {
            try
            {
                if (!table.HasRows())
                    return 0;
                return table.Rows.Count;
            }
            catch
            {
                return 0;
            }
        }

        public static int Rows(this DataTable table)
        {
            if (table.HasRows())
                return table.Rows.Count;
            return 0;
        }

        public static int GetIndex<T>(this IEnumerable<T> items, T item)
        {
            int index = -1;
            if (items.HasItems())
            {
                foreach (T obj in items)
                {
                    index++;

                    if (item is string)
                    {
                        if ((((obj as string) ?? string.Empty).Trim().Replace("\r", string.Empty).Replace("\n", string.Empty)).Equals(((item as string) ?? string.Empty), StringComparison.InvariantCultureIgnoreCase))
                            return index;
                    }
                    else if (obj.Equals(item))
                        return index;
                }
            }

            return -1;
        }
    }
}
