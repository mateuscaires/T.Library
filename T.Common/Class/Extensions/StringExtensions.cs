using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace T.Common
{
    public static class StringExtensions
    {
        public static string DecToHex(this string value)
        {
            return (value.ToInt32().ToString("X")).PadLeft(2, '0');
        }

        public static string DecToHex(this int x)
        {
            string result = string.Empty;
            string temp = string.Empty;

            while (x != 0)
            {
                if ((x % 16) < 10)
                    result = x % 16 + result;
                else
                {
                    switch (x % 16)
                    {
                        case 10: temp = "A"; break;
                        case 11: temp = "B"; break;
                        case 12: temp = "C"; break;
                        case 13: temp = "D"; break;
                        case 14: temp = "E"; break;
                        case 15: temp = "F"; break;
                    }

                    result = string.Concat(temp, result);
                }

                x /= 16;
            }

            return result;
        }

        public static string HexToBinary(this string hexvalue)
        {
            try
            {
                return string.Join(string.Empty, hexvalue.Select(c => Convert.ToString(Convert.ToUInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string BitToHex(this string binary)
        {
            return Convert.ToString(Convert.ToUInt32(binary, 2), 16).ToUpper();
        }

        public static string BitToDec(this string binary)
        {
            if (binary.HasText())
            {
                try
                {
                    return Convert.ToInt32(binary, 2).ToString();
                }
                catch
                {

                }
            }

            return string.Empty;
        }

        public static string GetValue(this DataRow row, int columnIndex)
        {
            try
            {
                return row[columnIndex].ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetValue(this DataRow row, string column)
        {
            try
            {
                return row[column].ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
                
        public static string GetXHTMLStackMessage(this Exception ex)
        {
            StringBuilder message = new StringBuilder();

            while (ex != null)
            {
                if (!(ex is TargetInvocationException))
                    message.Append(ex.Message).Append(Environment.NewLine);                    
                ex = ex.InnerException;
            }

            return message.ToString();
        }

        public static string ToCsvFile(this DataTable dataTable)
        {
            return dataTable.ToCsvFile(";");
        }

        public static string ToCsvFile(this DataTable dataTable, bool exportHeader)
        {
            return dataTable.ToCsvFile(";", exportHeader);
        }

        public static string ToCsvFile(this DataTable dataTable, string separator)
        {
            return dataTable.ToCsvFile(separator, true);
        }

        public static string ToCsvFile(this DataTable dataTable, string separator, bool exportHeader)
        {
            StringBuilder fileContent = new StringBuilder();
            string quote = @"""";
            string columnValue = string.Empty;
            int columnIndex = 0;
            int rowIndex = 0;

            Action addSeparator = () =>
            {
                if (columnIndex < dataTable.Columns.Count)
                {
                    fileContent.Append(separator);
                }
            };

            if (exportHeader)
            {
                foreach (var col in dataTable.Columns)
                {
                    columnIndex++;
                    fileContent.Append(col.ToString());
                    addSeparator();
                }

                fileContent.Append(Environment.NewLine);
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                rowIndex++;
                columnIndex = 0;
                foreach (DataColumn column in dataTable.Columns)
                {
                    columnIndex++;

                    columnValue = dr.GetColumnValue(column.ColumnName);

                    if (columnValue.Contains(separator))
                    {
                        fileContent.Append(quote)
                                   .Append(columnValue)
                                   .Append(quote);
                    }
                    else
                    {
                        fileContent.Append(columnValue);
                    }

                    addSeparator();
                }

                if (rowIndex < dataTable.Count())
                {
                    fileContent.Append(Environment.NewLine);
                }
            }

            return fileContent.ToString();
        }

        public static string ToTxtFile(this DataTable dataTable)
        {
            return dataTable.ToTxtFile(";");
        }

        public static string ToTxtFile(this DataTable dataTable, bool exportHeader)
        {
            return dataTable.ToTxtFile(";", exportHeader);
        }

        public static string ToTxtFile(this DataTable dataTable, string separator)
        {
            return dataTable.ToTxtFile(separator, true);
        }

        public static string ToTxtFile(this DataTable dataTable, string separator, bool exportHeader)
        {
            StringBuilder fileContent = new StringBuilder();

            if (exportHeader)
            {
                foreach (var col in dataTable.Columns)
                {
                    fileContent.Append(col.ToString()).Append(separator);
                }

                fileContent.Replace(separator, "\n", fileContent.Length - 1, 1);
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    fileContent.Append(dr.GetColumnValue(column.ColumnName))
                               .Append(separator);
                }

                fileContent.Replace(separator, "\n", fileContent.Length - 1, 1);
            }

            return fileContent.ToString();
        }

        public static string ToXml(this DataTable table)
        {
            using (DataSet ds = new DataSet())
            {
                ds.Tables.Add(table);

                return ds.GetXml();
            }
        }

        public static string ToTxtFile(this DataRow row)
        {
            return row.ToTxtFile(";");
        }

        public static string ToTxtFile(this DataRow row, string separator)
        {
            StringBuilder fileContent = new StringBuilder();

            foreach (DataColumn column in row.Table.Columns)
            {
                fileContent.Append(row.GetColumnValue(column.ColumnName))
                           .Append(separator);
            }

            fileContent.Replace(separator, Environment.NewLine, fileContent.Length - 1, 1);

            return fileContent.ToString();
        }

        public static string ToTxtFile(this string[] array)
        {
            return array.ToTxtFile(";");
        }

        public static string ToTxtFile(this string[] array, string separator)
        {
            StringBuilder fileContent = new StringBuilder();

            foreach (string value in array)
            {
                fileContent.Append(value).Append(separator);
            }

            fileContent.Replace(separator, string.Empty, fileContent.Length - 1, 1);

            return fileContent.ToString();
        }

        public static string ToCsvFile(this string[] array)
        {
            return array.ToCsvFile(";");
        }

        public static string ToCsvFile(this string[] array, string separator)
        {
            StringBuilder fileContent = new StringBuilder();

            string quote = @"""";

            foreach (string value in array)
            {
                fileContent.Append(quote)
                           .Append(value)
                           .Append(quote)
                           .Append(separator);
            }

            fileContent.Replace(separator, string.Empty, fileContent.Length - 1, 1);

            return fileContent.ToString();
        }

        public static string GetColumnValue(this DataRow row, string column)
        {
            Type t = row[column].GetType();

            string value = row[column].ToString();
            
            return value;
        }
        
        public static string RemoveSpecialChar(this string s)
        {
            return s.RemoveSpecialChar(default(char));
        }

        public static string RemoveSpecialChar(this string s, char ignore)
        {
            char[] invalids = new[] { ignore };

            return s.RemoveSpecialChar(invalids);
        }

        public static string RemoveSpecialChar(this string s, char[] ignore)
        {
            var invalids = CharExtensions.GetSpecialChars();

            string ret = s;

            foreach (var item in s)
            {
                if (ignore.Contains(item))
                    continue;

                if (invalids.Where(a => a == item).Count() > 0)
                    ret = ret.Replace(item.ToString(), string.Empty);
            }

            return ret.Trim();
        }

        public static string CaldulaIdade(this DateTime date)
        {
            var dt = Convert.ToDateTime(date);
            var ts = DateTime.Today - dt;
            var idade = (new DateTime() + ts).AddYears(-1).AddDays(-1);
            return (idade.Year.ToString());

        }

        public static string CaldulaIdade(this string strData)
        {
            if (string.IsNullOrEmpty(strData) || strData == "NULL") return null;
            if (!strData.IsDate()) return null;
            var dt = Convert.ToDateTime(strData);
            if (dt.Year == DateTime.Now.Year) return "0";
            var ts = DateTime.Today - dt;
            var idade = (new DateTime() + ts).AddYears(-1).AddDays(-1);
            return (idade.Year.ToString());

        }

        public static string ClearTelefone(this string tel)
        {
            if (string.IsNullOrWhiteSpace(tel))
                return string.Empty;
            return tel.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty);
        }

        public static string ClearCPF(this string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;
            return cpf.Replace(".", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).Trim();
        }

        public static string Upper(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return s.Trim().ToUpper();
        }

        public static string MakeIn<T>(this IEnumerable<T> content)
        {
            return content.MakeIn(",");
        }

        public static string MakeIn<T>(this IEnumerable<T> content, string separator)
        {
            if (content.IsNull())
                return string.Empty;

            return string.Concat(" IN(", string.Join(separator, content), ")");
        }

        public static string RemoveSpace(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            string ret = s.Replace("&nbsp;", string.Empty);

            return ret.Trim();
        }

        public static string DBVal(this decimal val)
        {
            return val.ToString().Replace(",", ".");
        }

        public static string GetValue(this DataTable table, int colIndex)
        {
            try
            {
                if (table.HasRows())
                    return table.Rows[0][colIndex].ToString();
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        public static string[] GetString(this Stream content)
        {
            BinaryReader binary = new BinaryReader(content);
            byte[] bytes = binary.ReadBytes((int)content.Length);
            return bytes.GetString();
        }

        public static string[] GetString(this byte[] content)
        {
            return Encoding.Default.GetString(content, 0, content.Length - 1).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public static string ByteArrayString(this byte[] bytes)
        {
            return bytes.ByteArrayString(Encoding.Default);
        }

        public static string ByteArrayString(this byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }
        
        public static string RemoveZeroRight(this string s, int preserv)
        {
            char dot = '.';

            for (int i = (s.Length - 1); i > 0; i--)
            {
                if (s[i] == '0')
                {
                    s = s.Substring(0, i);
                    if (s[i - 1] == dot)
                        break;
                }
                else
                    break;
            }

            if (s.Contains(dot))
            {
                int lentgh = s.Substring(s.IndexOf(dot) + 1).Length;
                if (lentgh <= preserv)
                    s = string.Concat(s, new string('0', (lentgh == 0) ? preserv : lentgh));
            }

            return s;
        }

        public static string GerarCpf()
        {
            int soma = 0, resto = 0;
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            Random rnd = new Random();
            string semente = rnd.Next(100000000, 999999999).ToString();

            for (int i = 0; i < 9; i++)
                soma += int.Parse(semente[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            semente = semente + resto;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(semente[i].ToString()) * multiplicador2[i];

            resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            semente = semente + resto;

            return semente;
        }

        public static string RemoveCentsSeparator(this decimal t)
        {
            try
            {
                string r = t.ToString();
                r = r.Replace(",", string.Empty);
                r = r.Replace(".", string.Empty);
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string RemoveCentsSeparator(this string t)
        {
            try
            {
                string r = t.Trim();
                r = r.Replace(",", string.Empty);
                r = r.Replace(".", string.Empty);
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ApplyCents(this string t, int index)
        {
            try
            {
                if (t.Length == 1 || index < 1)
                    return t;

                if (index > t.Length)
                    index = t.Length - 2;

                t = t.Insert(index, CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
                return t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetString(this object s)
        {
            try
            {
                return s.GetString(true);
            }
            catch { return null; }
        }

        public static string ToBase64String(this byte[] array)
        {
            try
            {
                return Convert.ToBase64String(array);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetString(this object s, bool aplicarTrim)
        {
            try
            {
                var ret = s.ToString();
                if (aplicarTrim)
                    return ret.ToString().Trim();
                else
                    return ret.ToString();
            }
            catch { return string.Empty; }
        }

        public static string Val(this object s)
        {
            try
            {
                if (s is decimal)
                {
                    return s.ToString().Replace(',', '.');
                }
                return (s ?? string.Empty).ToString().Trim();
            }
            catch { return null; }
        }

        public static string SubStringPaddingLeft(this string code, int len)
        {
            code = code.Trim();
            if (code.Length > len)
                return code.Substring(0, len);
            else
                return code.PadRight(len, ' ');
        }

        public static string SubStringPaddingRight(this string code, int len)
        {
            code = code.Trim();
            if (code.Length > len)
                return code.Substring(code.Length - len, len);
            else
                return code.PadLeft(len, ' ');
        }

        public static string Replace(this string str, string oldValue, int newValue)
        {
            return str.Replace(oldValue, newValue.ToString());
        }

        public static string ToDefaultFormat(this DateTime data)
        {
            return data.ToString("dd/MM/yyyy");
        }

        public static string ToDataBaseFormat(this DateTime data)
        {
            return data.ToString("yyyy-MM-dd");
        }

        public static string Remove(this string str, string value)
        {
            return str.Replace(value, string.Empty);
        }

        public static string Remove(this string str, char value)
        {
            return str.Replace(value, default(char));
        }

        public static string RemoveZeroLeft(this string s)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool removeu = false;
                foreach (var item in s.ToArray())
                {
                    if (item == '0' && !removeu)
                        continue;
                    else
                    {
                        removeu = true;
                        sb.Append(item);
                    }
                }
                return sb.ToString().IsNullOrEmpty() ? "0" : sb.ToString();
            }
            catch
            {
                return s;
            }
        }

        public static string ToCPForCNPJ(this string s, string tipo)
        {
            try
            {
                if (s.Length <= 11)
                    return s;
                switch (tipo.ToUpper())
                {
                    case "J":
                        {
                            if (s.Length == 14)
                                return s;
                            int startIndex = s.Length - 14;
                            if ((s.Length - startIndex) < 14)
                                return s;
                            s = s.Substring(startIndex);
                        }
                        break;
                    case "F":
                        {
                            int startIndex = s.Length - 11;
                            if ((s.Length - startIndex) < 11)
                                return s;
                            s = s.Substring(startIndex);
                        }
                        break;
                }
                return s;
            }
            catch { return s; }
        }

        public static string RepleceXorWhiteSpaceByZero(this string s)
        {
            try
            {
                if (s.IsNullOrEmpty())
                    return "0";
                string retorno = s;
                foreach (var item in s.ToArray())
                {
                    switch (item)
                    {
                        case 'x':
                            retorno = retorno.Replace('x', '0');
                            break;
                        case 'X':
                            retorno = retorno.Replace('X', '0');
                            break;
                        default:
                            break;
                    }
                }
                return retorno;
            }
            catch { return s; }
        }

        public static string Capitalize(this string v)
        {
            return v.Substring(0, 1).ToUpper() + v.Substring(1);
        }

        public static string RemoveAcentos(this string texto)
        {
            string textor = string.Empty;

            for (int i = 0; i < texto.Length; i++)
            {
                switch (texto[i].ToString())
                {
                    case "ã": textor += "a"; break;
                    case "á": textor += "a"; break;
                    case "à": textor += "a"; break;
                    case "â": textor += "a"; break;
                    case "ä": textor += "a"; break;
                    case "é": textor += "e"; break;
                    case "è": textor += "e"; break;
                    case "ê": textor += "e"; break;
                    case "ë": textor += "e"; break;
                    case "í": textor += "i"; break;
                    case "ì": textor += "i"; break;
                    case "ï": textor += "i"; break;
                    case "õ": textor += "o"; break;
                    case "ó": textor += "o"; break;
                    case "ò": textor += "o"; break;
                    case "ö": textor += "o"; break;
                    case "ú": textor += "u"; break;
                    case "ù": textor += "u"; break;
                    case "ü": textor += "u"; break;
                    case "ç": textor += "c"; break;
                    case "Ã": textor += "A"; break;
                    case "Á": textor += "A"; break;
                    case "À": textor += "A"; break;
                    case "Â": textor += "A"; break;
                    case "Ä": textor += "A"; break;
                    case "É": textor += "E"; break;
                    case "È": textor += "E"; break;
                    case "Ê": textor += "E"; break;
                    case "Ë": textor += "E"; break;
                    case "Í": textor += "I"; break;
                    case "Ì": textor += "I"; break;
                    case "Ï": textor += "I"; break;
                    case "Õ": textor += "O"; break;
                    case "Ó": textor += "O"; break;
                    case "Ò": textor += "O"; break;
                    case "Ö": textor += "O"; break;
                    case "Ú": textor += "U"; break;
                    case "Ù": textor += "U"; break;
                    case "Ü": textor += "U"; break;
                    case "Ç": textor += "C"; break;
                    default: textor += texto[i]; break;
                }
            }
            return textor;
        }

        public static string Description(this Enum e)
        {
            Type t = e.GetType();
            DescriptionAttribute[] att = { };

            if (Enum.IsDefined(t, e))
            {
                var fieldInfo = t.GetField(e.ToString());
                att = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            }
            return (att.Length > 0 ? att[0].Description ?? e.ToString() : e.ToString());
        }

        public static string Category(this Enum e)
        {
            Type t = e.GetType();
            CategoryAttribute[] att = { };

            if (Enum.IsDefined(t, e))
            {
                var fieldInfo = t.GetField(e.ToString());
                att = (CategoryAttribute[])fieldInfo.GetCustomAttributes(typeof(CategoryAttribute), false);
            }
            return (att.Length > 0 ? att[0].Category ?? e.ToString() : e.ToString());
        }

        public static string GetTableName<T>()
        {
            TableNameAttribute attr = TExtensions.GetAttribute<TableNameAttribute, T>();

            string name = null;

            if (attr != null)
            {
                return attr.Value;
            }
            else
            {
                try
                {
                    name = typeof(T).Name;
                }
                catch
                {

                }
            }

            return name;
        }

        public static string GetTableName<T>(this T e)
        {
            return GetTableName<T>();
        }

        public static string MountSelect<T>(this T e) where T : class
        {
            string select = (string.Concat("SELECT * FROM ", GetTableName<T>(), " WITH (NOLOCK)"));
            return select;
        }

        public static string GetPropertyValue<T>(this T obj, string propName)
        {
            string value = string.Empty;
            try
            {
                foreach (PropertyInfo p in obj.GetType().GetProperties())
                {
                    if (p.Name.ToLower() == propName.ToLower())
                    {
                        value = p.GetValue(obj, null).ToString();
                        break;
                    }
                }

                return value;
            }
            catch
            {

            }

            return value;
        }

        public static string GetPrimaryKey<T>(this T obj)
        {
            foreach (PropertyInfo p in obj.GetType().GetProperties())
            {
                if (p.IsPrimaryKey())
                    return p.Name;
            }
            return string.Empty;
        }

        public static string GetValueString<T>(this T TObject, string propname)
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
                return null;

            object value = pi.GetValue(TObject, null);

            if (value.HasValue())
            {
                if (pi.PropertyType.FullName.Contains("System.Decimal"))
                    return value.ToString().Replace(",", ".").Trim();
                if (pi.PropertyType.FullName.Contains("System.Int32"))
                    return value.ToString().Trim();
                if (pi.PropertyType.FullName.Contains("System.Double"))
                    return value.ToString().Replace(",", ".").Trim();
                if (pi.PropertyType.FullName.Contains("System.String"))
                    return value.ToString().Length > 1 ? value.ToString().Trim() : value.ToString();
                if (pi.PropertyType.FullName.Contains("System.DateTime"))
                {
                    DateTime date = value.ToString().ToDateTime();
                    string s = string.Concat(date.Year, "-", date.Month.ToString().PadLeft(2, '0'), "-", date.Day.ToString().PadLeft(2, '0'), " ", date.Hour.ToString().PadLeft(2, '0'), ":", date.Minute.ToString().PadLeft(2, '0'), ":", date.Second.ToString().PadLeft(2, '0'));
                    return s;
                }

                return value.ToString().Trim();
            }

            return null;
        }

        public static string AmbienteValue(this Enum e)
        {
            Type t = e.GetType();

            AmbientValueAttribute[] att = { };

            if (Enum.IsDefined(t, e))
            {
                var fieldInfo = t.GetField(e.ToString());
                att = (AmbientValueAttribute[])fieldInfo.GetCustomAttributes(typeof(AmbientValueAttribute), false);
            }

            return (att.Length > 0 ? att[0].Value.GetString() ?? e.ToString() : e.ToString());
        }

        public static string Left(this string t, int tamanho)
        {
            try
            {
                if (t.Length <= tamanho || tamanho < 1)
                    return t;
                return t.Substring(0, (tamanho > t.Length ? t.Length : tamanho));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string Right(this string t, int tamanho)
        {
            try
            {
                if (t.Length <= tamanho || tamanho < 1)
                    return t;
                int startIndex = t.Length - tamanho;
                return t.Substring(startIndex > t.Length ? t.Length : startIndex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public static string Concatenar(this string s, string text)
        {
            return string.Concat(s, text);
        }
        
        public static string Serialize<T>(this T obj)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                Type t = obj.GetType();

                string quot = "\"";

                PropertyInfo[] properties = t.GetProperties();

                sb.Append("{").Append(quot).Append("System Type:").Append(quot).Append(t.Name).Append(quot).Append(", ");

                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo item = properties[i];

                    sb.Append(quot).Append(item.Name).Append(quot).Append(":").Append(quot).Append(item.GetValue(obj, null)).Append(quot);

                    if (i < (properties.Length - 1))
                        sb.Append(", ");
                }

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetAssemblyGuid(this Assembly assembly)
        {
            return assembly.GetCustomAttribute<GuidAttribute>().Value.ToUpper();
        }
    }
}