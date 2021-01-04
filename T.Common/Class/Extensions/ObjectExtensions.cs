using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace T.Common
{
    public static class ObjectExtensions
    {
        public static object GetValue<T>(this T TObject, string propname)
        {
            var type = TObject.GetType();
            var properties = type.GetProperties();
            PropertyInfo pi = properties.Where(a => a.Name.ToLower() == propname.ToLower()).FirstOrDefault();

            if (!pi.HasValue())
            {
                ColumnAttribute columnattribute;

                foreach (var prop in properties)
                {
                    columnattribute = prop.GetCustomAttributes(typeof(ColumnAttribute), true).SingleOrDefault() as ColumnAttribute;

                    if (columnattribute != null)
                    {
                        if (columnattribute.Name.ToLower() == propname.ToLower())
                        {
                            pi = prop;
                            break;
                        }
                    }
                    else
                    {
                        if (prop.Name.ToLower() == propname.ToLower())
                        {
                            pi = prop;
                            break;
                        }
                    }
                }
            }

            if (!pi.HasValue())
                return string.Empty;

            object value = pi.GetValue(TObject, null);

            return value;
        }

        public static object GetValueObject(this DataRow row, string columnName)
        {
            try
            {
                if (row.HasColumn(columnName))
                    return row[columnName];
            }
            catch
            {
                return null;
            }

            return null;
        }

        public static object GetValueObject(this DataRow row, Type type, string columnName)
        {
            try
            {
                if (row.HasColumn(columnName))
                    return Convert.ChangeType(row[columnName], type);
            }
            catch
            {
                return null;
            }

            return null;
        }

        public static object GetDefaultValue(this Type type)
        {
            Type underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return Activator.CreateInstance(underlying);
            return Activator.CreateInstance(type);
        }
    }
}
