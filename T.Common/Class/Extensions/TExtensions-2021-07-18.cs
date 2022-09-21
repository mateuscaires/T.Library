// Decompiled with JetBrains decompiler
// Type: T.Common.TExtensions
// Assembly: T.Common, Version=1.20.1.1, Culture=neutral, PublicKeyToken=null
// MVID: 4A736707-E2CE-4803-8DEB-5C8961EEDB7C
// Assembly location: C:\Storage\References\T.Library\T.Common.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using T.Entities;

namespace T.Common
{
  public static class TExtensions
  {
    public static T ExecuteMethod<T>(this string classname, string assemblyname, string methodname, object[] parameters)
    {
      try
      {
        Assembly assembly = Assembly.Load(assemblyname);
        Type type = assembly.GetType(classname);
        MethodInfo method = type.GetMethod(methodname);
        if (method != (MethodInfo) null)
        {
          ParameterInfo[] parameters1 = method.GetParameters();
          if (method.IsStatic)
          {
            if (!(method.ReturnType.Name == "Void"))
              return (T) method.Invoke((object) assembly, ((IEnumerable<ParameterInfo>) parameters1).Count<ParameterInfo>() > 0 ? parameters : (object[]) null);
            method.Invoke((object) assembly, ((IEnumerable<ParameterInfo>) parameters1).Count<ParameterInfo>() > 0 ? parameters : (object[]) null);
            return default (T);
          }
          object instance = Activator.CreateInstance(type);
          if (!(method.ReturnType.Name == "Void"))
            return (T) method.Invoke(instance, ((IEnumerable<ParameterInfo>) parameters1).Count<ParameterInfo>() > 0 ? parameters : (object[]) null);
          method.Invoke(instance, ((IEnumerable<ParameterInfo>) parameters1).Count<ParameterInfo>() > 0 ? parameters : (object[]) null);
          return default (T);
        }
        throw new MissingMethodException("O método " + methodname + " da classe " + classname + " não foi encontrado.");
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static T TryCast<T>(this string value, T result)
    {
      return TExtensions.TryCast<T>(value, result);
    }

    public static T TryCast<T>(this object value, T result)
    {
      Type type1 = typeof (T);
      if (type1.IsNullable() && (value == null || value == DBNull.Value))
        return default (T);
      Type type2 = Nullable.GetUnderlyingType(type1);
      if ((object) type2 == null)
        type2 = type1;
      Type conversionType = type2;
      try
      {
        if (!(conversionType == typeof (Guid)))
          return (T) Convert.ChangeType(value, conversionType);
        if (value is string)
          value = (object) new Guid(value as string);
        if (value is byte[])
          value = (object) new Guid(value as byte[]);
        return (T) Convert.ChangeType(value, conversionType);
      }
      catch
      {
        return default (T);
      }
    }

    public static T ToEnum<T>(this string str)
    {
      try
      {
        if (str.IsNullOrEmpty())
          return default (T);
        str = ((IEnumerable<string>) Enum.GetNames(typeof (T))).Where<string>((Func<string, bool>) (a => a.ToLower() == str.ToLower())).FirstOrDefault<string>();
        return (T) Enum.Parse(typeof (T), str);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static T GetAttribute<T, K>(this K item) where T : Attribute
    {
      T obj = ((IEnumerable<object>) item.GetType().GetCustomAttributes(typeof (T), true)).FirstOrDefault<object>() as T;
      if ((object) obj != null)
        return obj;
      return default (T);
    }

    public static T GetAttribute<T, K>() where T : Attribute
    {
      T obj = ((IEnumerable<object>) typeof (K).GetCustomAttributes(typeof (T), true)).FirstOrDefault<object>() as T;
      if ((object) obj != null)
        return obj;
      return default (T);
    }

    public static T GetAttribute<T>(this PropertyInfo prop) where T : Attribute
    {
      return ((IEnumerable<object>) prop.GetCustomAttributes(typeof (T), true)).FirstOrDefault<object>() as T;
    }

    public static T Clone<T>(this T instance)
    {
      if ((object) instance == null)
        return instance;
      Type type = typeof (T);
      T obj = default (T);
      if (type.IsSerializable)
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          BinaryFormatter binaryFormatter = new BinaryFormatter();
          binaryFormatter.Serialize((Stream) memoryStream, (object) instance);
          memoryStream.Position = 0L;
          obj = (T) binaryFormatter.Deserialize((Stream) memoryStream);
        }
      }
      else
      {
        obj = (T) Activator.CreateInstance(type);
        PropertyInfo[] properties = type.GetProperties();
        foreach (PropertyInfo propertyInfo1 in properties)
        {
          PropertyInfo item = propertyInfo1;
          PropertyInfo propertyInfo2 = ((IEnumerable<PropertyInfo>) properties).Where<PropertyInfo>((Func<PropertyInfo, bool>) (a => a.Name == item.Name)).FirstOrDefault<PropertyInfo>();
          try
          {
            if (propertyInfo2 != (PropertyInfo) null)
              propertyInfo2.SetValue((object) obj, item.GetValue((object) instance, (object[]) null), (object[]) null);
          }
          catch
          {
          }
        }
      }
      return obj;
    }

    public static T Map<T>(this DataTable table)
    {
      return table.Map<T>("key", "value");
    }

    public static T Map<T>(this DataTable table, string colKey, string colValue)
    {
      try
      {
        T instance = Activator.CreateInstance<T>();
        string value = string.Empty;
        PropertyInfo[] properties = instance.GetType().GetProperties();
        Func<PropertyInfo, DataRow, string, bool> func = (Func<PropertyInfo, DataRow, string, bool>) ((p, r, key) =>
        {
          if (!key.Equals((value ?? string.Empty).ToString(), StringComparison.InvariantCultureIgnoreCase))
            return false;
          try
          {
            value = (r[colValue] ?? (object) string.Empty).ToString();
            p.SetValue((object) instance, Convert.ChangeType((object) value, p.PropertyType));
          }
          catch
          {
          }
          return true;
        });
        foreach (PropertyInfo prop in properties)
        {
          foreach (DataRow row in (InternalDataCollectionBase) table.Rows)
          {
            value = (row[colKey] ?? (object) string.Empty).ToString();
            if (!func(prop, row, prop.Name))
            {
              AliasAttribute attribute = prop.GetAttribute<AliasAttribute>();
              if (attribute.HasValue())
              {
                if (func(prop, row, attribute.Value ?? string.Empty))
                  break;
              }
            }
            else
              break;
          }
        }
        return instance;
      }
      catch
      {
        return default (T);
      }
    }

    public static T GetValue<T>(this DataRow row, string columnName)
    {
      try
      {
        if (row.HasColumn(columnName))
          return (T) Convert.ChangeType(row[columnName], typeof (T));
      }
      catch
      {
        return default (T);
      }
      return default (T);
    }

    public static T TryConvert<T>(this string value, string typeName)
    {
      try
      {
        Type type = Type.GetType(typeName);
        return (T) Convert.ChangeType((object) value, type);
      }
      catch
      {
        return default (T);
      }
    }

    public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
    {
      List<T> objList = new List<T>();
      foreach (DataRow row in (InternalDataCollectionBase) table.Rows)
        objList.Add(row.DataRowToTObject<T>());
      return objList;
    }

    public static T DataRowToTObject<T>(this DataRow row)
    {
      Type conversionType = typeof (T);
      T obj1 = default (T);
      if (conversionType != typeof (string) && ((object) obj1).IsNull())
        obj1 = Activator.CreateInstance<T>();
      if (conversionType.IsValueType)
      {
        if (conversionType.IsPrimitive)
        {
          foreach (DataColumn column in (InternalDataCollectionBase) row.Table.Columns)
          {
            if (column.DataType == conversionType)
            {
              obj1 = (T) Convert.ChangeType(row[column.ColumnName], conversionType);
              break;
            }
          }
        }
      }
      else if (conversionType == typeof (string))
      {
        foreach (DataColumn column in (InternalDataCollectionBase) row.Table.Columns)
        {
          if (column.DataType == conversionType)
          {
            obj1 = (T) Convert.ChangeType(row[column.ColumnName], conversionType);
            break;
          }
        }
      }
      else
      {
        PropertyInfo[] properties = conversionType.GetProperties();
        foreach (DataColumn column1 in (InternalDataCollectionBase) row.Table.Columns)
        {
          DataColumn column = column1;
          PropertyInfo propertyInfo = ((IEnumerable<PropertyInfo>) properties).Where<PropertyInfo>((Func<PropertyInfo, bool>) (a => a.Name == column.ColumnName)).FirstOrDefault<PropertyInfo>();
          if (!propertyInfo.IsNull())
          {
            object obj2 = row[column.ColumnName];
            try
            {
              obj2 = obj2 == DBNull.Value ? (object) null : Convert.ChangeType(obj2, propertyInfo.PropertyType);
            }
            catch
            {
            }
            propertyInfo.SetValue((object) obj1, obj2, (object[]) null);
          }
        }
      }
      return obj1;
    }
  }
}
