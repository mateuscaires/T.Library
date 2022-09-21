// Decompiled with JetBrains decompiler
// Type: T.Common.StringExtensions
// Assembly: T.Common, Version=1.20.1.1, Culture=neutral, PublicKeyToken=null
// MVID: 4A736707-E2CE-4803-8DEB-5C8961EEDB7C
// Assembly location: C:\Storage\References\T.Library\T.Common.dll

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
      return IntExtensions.ToInt32(value).ToString("X").PadLeft(2, '0');
    }

    public static string DecToHex(this int x)
    {
      string str1 = string.Empty;
      string str2 = string.Empty;
      while ((uint) x > 0U)
      {
        if (x % 16 < 10)
        {
          str1 = (x % 16).ToString() + str1;
        }
        else
        {
          switch (x % 16)
          {
            case 10:
              str2 = "A";
              break;
            case 11:
              str2 = "B";
              break;
            case 12:
              str2 = "C";
              break;
            case 13:
              str2 = "D";
              break;
            case 14:
              str2 = "E";
              break;
            case 15:
              str2 = "F";
              break;
          }
          str1 = str2 + str1;
        }
        x /= 16;
      }
      return str1;
    }

    public static string HexToBinary(this string hexvalue)
    {
      try
      {
        return string.Join(string.Empty, hexvalue.Select<char, string>((Func<char, string>) (c => Convert.ToString((long) Convert.ToUInt32(c.ToString(), 16), 2).PadLeft(4, '0'))));
      }
      catch
      {
        return string.Empty;
      }
    }

    public static string BitToHex(this string binary)
    {
      return Convert.ToString((long) Convert.ToUInt32(binary, 2), 16).ToUpper();
    }

    public static string BitToDec(this string binary)
    {
      if (BoolExtensions.HasText(binary))
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
      StringBuilder stringBuilder = new StringBuilder();
      for (; ex != null; ex = ex.InnerException)
      {
        if (!(ex is TargetInvocationException))
          stringBuilder.Append(ex.Message).Append(Environment.NewLine);
      }
      return stringBuilder.ToString();
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
      string str = "\"";
      string empty = string.Empty;
      int columnIndex = 0;
      int num = 0;
      Action action = (Action) (() =>
      {
        if (columnIndex >= dataTable.Columns.Count)
          return;
        fileContent.Append(separator);
      });
      if (exportHeader)
      {
        foreach (object column in (InternalDataCollectionBase) dataTable.Columns)
        {
          columnIndex++;
          fileContent.Append(column.ToString());
          action();
        }
        fileContent.Append(Environment.NewLine);
      }
      foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
      {
        ++num;
        columnIndex = 0;
        foreach (DataColumn column in (InternalDataCollectionBase) dataTable.Columns)
        {
          columnIndex++;
          string columnValue = row.GetColumnValue(column.ColumnName);
          if (columnValue.Contains(separator))
            fileContent.Append(str).Append(columnValue).Append(str);
          else
            fileContent.Append(columnValue);
          action();
        }
        if (num < dataTable.Count())
          fileContent.Append(Environment.NewLine);
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
      StringBuilder stringBuilder = new StringBuilder();
      if (exportHeader)
      {
        foreach (object column in (InternalDataCollectionBase) dataTable.Columns)
          stringBuilder.Append(column.ToString()).Append(separator);
        stringBuilder.Replace(separator, "\n", stringBuilder.Length - 1, 1);
      }
      foreach (DataRow row in (InternalDataCollectionBase) dataTable.Rows)
      {
        foreach (DataColumn column in (InternalDataCollectionBase) dataTable.Columns)
          stringBuilder.Append(row.GetColumnValue(column.ColumnName)).Append(separator);
        stringBuilder.Replace(separator, "\n", stringBuilder.Length - 1, 1);
      }
      return stringBuilder.ToString();
    }

    public static string ToXml(this DataTable table)
    {
      using (DataSet dataSet = new DataSet())
      {
        dataSet.Tables.Add(table);
        return dataSet.GetXml();
      }
    }

    public static string ToTxtFile(this DataRow row)
    {
      return row.ToTxtFile(";");
    }

    public static string ToTxtFile(this DataRow row, string separator)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (DataColumn column in (InternalDataCollectionBase) row.Table.Columns)
        stringBuilder.Append(row.GetColumnValue(column.ColumnName)).Append(separator);
      stringBuilder.Replace(separator, Environment.NewLine, stringBuilder.Length - 1, 1);
      return stringBuilder.ToString();
    }

    public static string ToTxtFile(this string[] array)
    {
      return array.ToTxtFile(";");
    }

    public static string ToTxtFile(this string[] array, string separator)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in array)
        stringBuilder.Append(str).Append(separator);
      stringBuilder.Replace(separator, string.Empty, stringBuilder.Length - 1, 1);
      return stringBuilder.ToString();
    }

    public static string ToCsvFile(this string[] array)
    {
      return array.ToCsvFile(";");
    }

    public static string ToCsvFile(this string[] array, string separator)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str1 = "\"";
      foreach (string str2 in array)
        stringBuilder.Append(str1).Append(str2).Append(str1).Append(separator);
      stringBuilder.Replace(separator, string.Empty, stringBuilder.Length - 1, 1);
      return stringBuilder.ToString();
    }

    public static string GetColumnValue(this DataRow row, string column)
    {
      row[column].GetType();
      return row[column].ToString();
    }

    public static string RemoveSpecialChar(this string s)
    {
      return s.RemoveSpecialChar(char.MinValue);
    }

    public static string RemoveSpecialChar(this string s, char ignore)
    {
      char[] ignore1 = new char[1]{ ignore };
      return s.RemoveSpecialChar(ignore1);
    }

    public static string RemoveSpecialChar(this string s, char[] ignore)
    {
      char[] specialChars = CharExtensions.GetSpecialChars();
      string str = s;
      foreach (char ch in s)
      {
        char item = ch;
        if (!((IEnumerable<char>) ignore).Contains<char>(item) && ((IEnumerable<char>) specialChars).Where<char>((Func<char, bool>) (a => (int) a == (int) item)).Count<char>() > 0)
          str = str.Replace(item.ToString(), string.Empty);
      }
      return str.Trim();
    }

    public static string CaldulaIdade(this DateTime date)
    {
      DateTime dateTime = new DateTime() + (DateTime.Today - Convert.ToDateTime(date));
      dateTime = dateTime.AddYears(-1);
      return dateTime.AddDays(-1.0).Year.ToString();
    }

    public static string CaldulaIdade(this string strData)
    {
      if (string.IsNullOrEmpty(strData) || strData == "NULL" || !strData.IsDate())
        return (string) null;
      DateTime dateTime1 = Convert.ToDateTime(strData);
      int year1 = dateTime1.Year;
      DateTime dateTime2 = DateTime.Now;
      int year2 = dateTime2.Year;
      if (year1 == year2)
        return "0";
      TimeSpan timeSpan = DateTime.Today - dateTime1;
      dateTime2 = new DateTime();
      dateTime2 += timeSpan;
      dateTime2 = dateTime2.AddYears(-1);
      return dateTime2.AddDays(-1.0).Year.ToString();
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
      return content.MakeIn<T>(",");
    }

    public static string MakeIn<T>(this IEnumerable<T> content, string separator)
    {
      if (content.IsNull())
        return string.Empty;
      return " IN(" + string.Join<T>(separator, content) + ")";
    }

    public static string RemoveSpace(this string s)
    {
      if (string.IsNullOrWhiteSpace(s))
        return string.Empty;
      return s.Replace("&nbsp;", string.Empty).Trim();
    }

    public static string DBVal(this Decimal val)
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
      return StringExtensions.GetString(new BinaryReader(content).ReadBytes((int) content.Length));
    }

    public static string[] GetString(this byte[] content)
    {
      return Encoding.Default.GetString(content, 0, content.Length - 1).Split(new string[3]
      {
        "\r\n",
        "\r",
        "\n"
      }, StringSplitOptions.None);
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
      char ch = '.';
      for (int length = s.Length - 1; length > 0 && s[length] == '0'; --length)
      {
        s = s.Substring(0, length);
        if ((int) s[length - 1] == (int) ch)
          break;
      }
      if (s.Contains<char>(ch))
      {
        int length = s.Substring(s.IndexOf(ch) + 1).Length;
        if (length <= preserv)
          s += new string('0', length == 0 ? preserv : length);
      }
      return s;
    }

    public static string GerarCpf()
    {
      int num1 = 0;
      int[] numArray1 = new int[9]
      {
        10,
        9,
        8,
        7,
        6,
        5,
        4,
        3,
        2
      };
      int[] numArray2 = new int[10]
      {
        11,
        10,
        9,
        8,
        7,
        6,
        5,
        4,
        3,
        2
      };
      string str1 = new Random().Next(100000000, 999999999).ToString();
      for (int index = 0; index < 9; ++index)
        num1 += int.Parse(str1[index].ToString()) * numArray1[index];
      int num2 = num1 % 11;
      int num3 = num2 >= 2 ? 11 - num2 : 0;
      string str2 = str1 + (object) num3;
      int num4 = 0;
      for (int index = 0; index < 10; ++index)
        num4 += int.Parse(str2[index].ToString()) * numArray2[index];
      int num5 = num4 % 11;
      int num6 = num5 >= 2 ? 11 - num5 : 0;
      return str2 + (object) num6;
    }

    public static string RemoveCentsSeparator(this Decimal t)
    {
      try
      {
        return t.ToString().Replace(",", string.Empty).Replace(".", string.Empty);
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
        return t.Trim().Replace(",", string.Empty).Replace(".", string.Empty);
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
      catch
      {
        return (string) null;
      }
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
        string str = s.ToString();
        if (aplicarTrim)
          return str.ToString().Trim();
        return str.ToString();
      }
      catch
      {
        return string.Empty;
      }
    }

    public static string Val(this object s)
    {
      try
      {
        if (s is Decimal)
          return s.ToString().Replace(',', '.');
        return (s ?? (object) string.Empty).ToString().Trim();
      }
      catch
      {
        return (string) null;
      }
    }

    public static string SubStringPaddingLeft(this string code, int len)
    {
      code = code.Trim();
      if (code.Length > len)
        return code.Substring(0, len);
      return code.PadRight(len, ' ');
    }

    public static string SubStringPaddingRight(this string code, int len)
    {
      code = code.Trim();
      if (code.Length > len)
        return code.Substring(code.Length - len, len);
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
      return str.Replace(value, char.MinValue);
    }

    public static string RemoveZeroLeft(this string s)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        bool flag = false;
        foreach (char ch in s.ToArray<char>())
        {
          if (ch != '0' || flag)
          {
            flag = true;
            stringBuilder.Append(ch);
          }
        }
        return stringBuilder.ToString().IsNullOrEmpty() ? "0" : stringBuilder.ToString();
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
        string upper = tipo.ToUpper();
        if (!(upper == "J"))
        {
          if (upper == "F")
          {
            int startIndex = s.Length - 11;
            if (s.Length - startIndex < 11)
              return s;
            s = s.Substring(startIndex);
          }
        }
        else
        {
          if (s.Length == 14)
            return s;
          int startIndex = s.Length - 14;
          if (s.Length - startIndex < 14)
            return s;
          s = s.Substring(startIndex);
        }
        return s;
      }
      catch
      {
        return s;
      }
    }

    public static string RepleceXorWhiteSpaceByZero(this string s)
    {
      try
      {
        if (s.IsNullOrEmpty())
          return "0";
        string str = s;
        foreach (char ch in s.ToArray<char>())
        {
          switch (ch)
          {
            case 'X':
              str = str.Replace('X', '0');
              break;
            case 'x':
              str = str.Replace('x', '0');
              break;
          }
        }
        return str;
      }
      catch
      {
        return s;
      }
    }

    public static string Capitalize(this string v)
    {
      return v.Substring(0, 1).ToUpper() + v.Substring(1);
    }

    public static string RemoveAcentos(this string texto)
    {
      string str1 = string.Empty;
      for (int index = 0; index < texto.Length; ++index)
      {
        char ch = texto[index];
        switch (ch.ToString())
        {
          case "À":
            str1 += "A";
            break;
          case "Á":
            str1 += "A";
            break;
          case "Â":
            str1 += "A";
            break;
          case "Ã":
            str1 += "A";
            break;
          case "Ä":
            str1 += "A";
            break;
          case "Ç":
            str1 += "C";
            break;
          case "È":
            str1 += "E";
            break;
          case "É":
            str1 += "E";
            break;
          case "Ê":
            str1 += "E";
            break;
          case "Ë":
            str1 += "E";
            break;
          case "Ì":
            str1 += "I";
            break;
          case "Í":
            str1 += "I";
            break;
          case "Ï":
            str1 += "I";
            break;
          case "Ò":
            str1 += "O";
            break;
          case "Ó":
            str1 += "O";
            break;
          case "Õ":
            str1 += "O";
            break;
          case "Ö":
            str1 += "O";
            break;
          case "Ù":
            str1 += "U";
            break;
          case "Ú":
            str1 += "U";
            break;
          case "Ü":
            str1 += "U";
            break;
          case "à":
            str1 += "a";
            break;
          case "á":
            str1 += "a";
            break;
          case "â":
            str1 += "a";
            break;
          case "ã":
            str1 += "a";
            break;
          case "ä":
            str1 += "a";
            break;
          case "ç":
            str1 += "c";
            break;
          case "è":
            str1 += "e";
            break;
          case "é":
            str1 += "e";
            break;
          case "ê":
            str1 += "e";
            break;
          case "ë":
            str1 += "e";
            break;
          case "ì":
            str1 += "i";
            break;
          case "í":
            str1 += "i";
            break;
          case "ï":
            str1 += "i";
            break;
          case "ò":
            str1 += "o";
            break;
          case "ó":
            str1 += "o";
            break;
          case "õ":
            str1 += "o";
            break;
          case "ö":
            str1 += "o";
            break;
          case "ù":
            str1 += "u";
            break;
          case "ú":
            str1 += "u";
            break;
          case "ü":
            str1 += "u";
            break;
          default:
            string str2 = str1;
            ch = texto[index];
            string str3 = ch.ToString();
            str1 = str2 + str3;
            break;
        }
      }
      return str1;
    }

    public static string Description(this Enum e)
    {
      Type type = e.GetType();
      DescriptionAttribute[] descriptionAttributeArray = new DescriptionAttribute[0];
      if (Enum.IsDefined(type, (object) e))
        descriptionAttributeArray = (DescriptionAttribute[]) type.GetField(e.ToString()).GetCustomAttributes(typeof (DescriptionAttribute), false);
      return descriptionAttributeArray.Length != 0 ? descriptionAttributeArray[0].Description ?? e.ToString() : e.ToString();
    }

    public static string Category(this Enum e)
    {
      Type type = e.GetType();
      CategoryAttribute[] categoryAttributeArray = new CategoryAttribute[0];
      if (Enum.IsDefined(type, (object) e))
        categoryAttributeArray = (CategoryAttribute[]) type.GetField(e.ToString()).GetCustomAttributes(typeof (CategoryAttribute), false);
      return categoryAttributeArray.Length != 0 ? categoryAttributeArray[0].Category ?? e.ToString() : e.ToString();
    }

    public static string GetTableName<T>()
    {
      TableNameAttribute attribute = TExtensions.GetAttribute<TableNameAttribute, T>();
      string str = (string) null;
      if (attribute != null)
        return attribute.Value;
      try
      {
        str = typeof (T).Name;
      }
      catch
      {
      }
      return str;
    }

    public static string GetTableName<T>(this T e)
    {
      return StringExtensions.GetTableName<T>();
    }

    public static string GetPropertyValue<T>(this T obj, string propName)
    {
      string empty = string.Empty;
      try
      {
        foreach (PropertyInfo property in obj.GetType().GetProperties())
        {
          if (property.Name.ToLower() == propName.ToLower())
          {
            empty = property.GetValue((object) obj, (object[]) null).ToString();
            break;
          }
        }
        return empty;
      }
      catch
      {
      }
      return empty;
    }

    public static string GetPrimaryKey<T>(this T obj)
    {
      foreach (PropertyInfo property in obj.GetType().GetProperties())
      {
        if (property.IsPrimaryKey())
          return property.Name;
      }
      return string.Empty;
    }

    public static string GetValueString<T>(this T TObject, string propname)
    {
      PropertyInfo[] properties = TObject.GetType().GetProperties();
      PropertyInfo propertyInfo1 = ((IEnumerable<PropertyInfo>) properties).Where<PropertyInfo>((Func<PropertyInfo, bool>) (a => a.Name.ToLower() == propname.ToLower())).FirstOrDefault<PropertyInfo>();
      if (!propertyInfo1.HasValue())
      {
        foreach (PropertyInfo propertyInfo2 in properties)
        {
          ColumnAttribute columnAttribute = ((IEnumerable<object>) propertyInfo2.GetCustomAttributes(typeof (ColumnAttribute), true)).SingleOrDefault<object>() as ColumnAttribute;
          if (columnAttribute != null)
          {
            if (columnAttribute.Name.ToLower() == propname.ToLower())
            {
              propertyInfo1 = propertyInfo2;
              break;
            }
          }
          else if (propertyInfo2.Name.ToLower() == propname.ToLower())
          {
            propertyInfo1 = propertyInfo2;
            break;
          }
        }
      }
      if (!propertyInfo1.HasValue())
        return (string) null;
      object obj = propertyInfo1.GetValue((object) TObject, (object[]) null);
      if (!obj.HasValue())
        return (string) null;
      if (propertyInfo1.PropertyType.FullName.Contains("System.Decimal"))
        return obj.ToString().Replace(",", ".").Trim();
      if (propertyInfo1.PropertyType.FullName.Contains("System.Int32"))
        return obj.ToString().Trim();
      if (propertyInfo1.PropertyType.FullName.Contains("System.Double"))
        return obj.ToString().Replace(",", ".").Trim();
      if (propertyInfo1.PropertyType.FullName.Contains("System.String"))
        return obj.ToString().Length > 1 ? obj.ToString().Trim() : obj.ToString();
      if (!propertyInfo1.PropertyType.FullName.Contains("System.DateTime"))
        return obj.ToString().Trim();
      DateTime dateTime = DateTimeExtensions.ToDateTime(obj.ToString());
      object[] objArray = new object[11];
      objArray[0] = (object) dateTime.Year;
      objArray[1] = (object) "-";
      objArray[2] = (object) dateTime.Month.ToString().PadLeft(2, '0');
      objArray[3] = (object) "-";
      int index1 = 4;
      int num = dateTime.Day;
      string str1 = num.ToString().PadLeft(2, '0');
      objArray[index1] = (object) str1;
      objArray[5] = (object) " ";
      int index2 = 6;
      num = dateTime.Hour;
      string str2 = num.ToString().PadLeft(2, '0');
      objArray[index2] = (object) str2;
      objArray[7] = (object) ":";
      int index3 = 8;
      num = dateTime.Minute;
      string str3 = num.ToString().PadLeft(2, '0');
      objArray[index3] = (object) str3;
      objArray[9] = (object) ":";
      int index4 = 10;
      num = dateTime.Second;
      string str4 = num.ToString().PadLeft(2, '0');
      objArray[index4] = (object) str4;
      return string.Concat(objArray);
    }

    public static string AmbienteValue(this Enum e)
    {
      Type type = e.GetType();
      AmbientValueAttribute[] ambientValueAttributeArray = new AmbientValueAttribute[0];
      if (Enum.IsDefined(type, (object) e))
        ambientValueAttributeArray = (AmbientValueAttribute[]) type.GetField(e.ToString()).GetCustomAttributes(typeof (AmbientValueAttribute), false);
      return ambientValueAttributeArray.Length != 0 ? ambientValueAttributeArray[0].Value.GetString() ?? e.ToString() : e.ToString();
    }

    public static string Left(this string t, int tamanho)
    {
      try
      {
        if (t.Length <= tamanho || tamanho < 1)
          return t;
        return t.Substring(0, tamanho > t.Length ? t.Length : tamanho);
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
        int num = t.Length - tamanho;
        return t.Substring(num > t.Length ? t.Length : num);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static string Concatenar(this string s, string text)
    {
      return s + text;
    }

    public static string Serialize<T>(this T obj)
    {
      try
      {
        StringBuilder stringBuilder = new StringBuilder();
        Type type = obj.GetType();
        string str = "\"";
        PropertyInfo[] properties = type.GetProperties();
        stringBuilder.Append("{").Append(str).Append("System Type:").Append(str).Append(type.Name).Append(str).Append(", ");
        for (int index = 0; index < properties.Length; ++index)
        {
          PropertyInfo propertyInfo = properties[index];
          stringBuilder.Append(str).Append(propertyInfo.Name).Append(str).Append(":").Append(str).Append(propertyInfo.GetValue((object) obj, (object[]) null)).Append(str);
          if (index < properties.Length - 1)
            stringBuilder.Append(", ");
        }
        return stringBuilder.ToString();
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
