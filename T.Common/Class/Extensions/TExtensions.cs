using System;
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
                MethodInfo methodinfo = type.GetMethod(methodname);
                if (methodinfo != null)
                {
                    ParameterInfo[] parametersinfo = methodinfo.GetParameters();
                    if (methodinfo.IsStatic)
                    {
                        switch (methodinfo.ReturnType.Name)
                        {
                            case "Void":
                                {
                                    methodinfo.Invoke(assembly, parametersinfo.Count() > 0 ? parameters : null);
                                    return default(T);
                                }
                            default:
                                {
                                    return (T)methodinfo.Invoke(assembly, parametersinfo.Count() > 0 ? parameters : null);
                                }
                        }
                    }
                    else
                    {
                        var instance = Activator.CreateInstance(type);
                        switch (methodinfo.ReturnType.Name)
                        {
                            case "Void":
                                {
                                    methodinfo.Invoke(instance, parametersinfo.Count() > 0 ? parameters : null);
                                    return default(T);
                                }
                            default:
                                {
                                    return (T)methodinfo.Invoke(instance, parametersinfo.Count() > 0 ? parameters : null);
                                }
                        }
                    }
                }
                else
                    throw new MissingMethodException(string.Concat("O método ", methodname, " da classe ", classname, " não foi encontrado."));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static T TryCast<T>(this string value, T result)
        {
            return TryCast((object)value, result);
        }

        public static T TryCast<T>(this object value, T result)
        {
            var type = typeof(T);

            // If the type is nullable and the result should be null, set a null value.
            if (type.IsNullable() && (value == null || value == DBNull.Value))
            {
                return default(T);
            }

            // Convert.ChangeType fails on Nullable<T> types.  We want to try to cast to the underlying type anyway.
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            try
            {
                // Just one edge case you might want to handle.
                if (underlyingType == typeof(Guid))
                {
                    if (value is string)
                    {
                        value = new Guid(value as string);
                    }
                    if (value is byte[])
                    {
                        value = new Guid(value as byte[]);
                    }

                    return (T)Convert.ChangeType(value, underlyingType);
                }

                return (T)Convert.ChangeType(value, underlyingType);

            }
            catch
            {
                return default(T);

            }
        }

        public static T ToEnum<T>(this string str)
        {
            try
            {
                string[] array = Enum.GetNames(typeof(T));

                str = array.Where(a => a.ToLower() == str.ToLower()).FirstOrDefault();

                T res = (T)Enum.Parse(typeof(T), str);

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static T GetAttribute<T, K>(this K item) where T : Attribute
        {
            T attr = item.GetType().GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
            if (attr != null)
            {
                return attr;
            }
            return default(T);
        }

        public static T GetAttribute<T, K>() where T : Attribute
        {
            T attr = typeof(K).GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
            if (attr != null)
            {
                return attr;
            }
            return default(T);
        }

        public static T GetAttribute<T>(this PropertyInfo prop) where T : Attribute
        {
            T attr = prop.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;

            return attr;
        }

        public static T Clone<T>(this T instance)
        {
            if (instance == null)
                return instance;

            Type type = typeof(T);
            T retorno = default(T);

            if (type.IsSerializable)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, instance);
                    ms.Position = 0;
                    retorno = (T)formatter.Deserialize(ms);
                }
            }
            else
            {
                retorno = (T)Activator.CreateInstance(type);

                PropertyInfo[] propsInstance = type.GetProperties();

                foreach (PropertyInfo item in propsInstance)
                {
                    PropertyInfo prop = propsInstance.Where(a => a.Name == item.Name).FirstOrDefault();
                    try
                    {
                        if (prop != null)
                            prop.SetValue(retorno, item.GetValue(instance, null), null);
                    }
                    catch
                    {
                    }
                }
            }

            return retorno;
        }

        public static T Map<T>(this DataTable table)
        {
            return Map<T>(table, "key", "value");
        }

        public static T Map<T>(this DataTable table, string colKey, string colValue)
        {
            try
            {
                T instance = Activator.CreateInstance<T>();

                string value = string.Empty;
                AliasAttribute alias = null;

                PropertyInfo[] pi = instance.GetType().GetProperties();

                Func<PropertyInfo, DataRow, string, bool> compare = (p, r, key) =>
                {
                    if (key.Equals((value ?? string.Empty).ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            value = (r[colValue] ?? string.Empty).ToString();
                            p.SetValue(instance, Convert.ChangeType(value, p.PropertyType));
                        }
                        catch
                        {

                        }

                        return true;
                    }

                    return false;
                };

                foreach (PropertyInfo p in pi)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        value = (row[colKey] ?? string.Empty).ToString();

                        if (compare(p, row, p.Name))
                        {
                            break;
                        }
                        else
                        {
                            alias = p.GetAttribute<AliasAttribute>();
                            if (alias.HasValue())
                            {
                                if (compare(p, row, (alias.Value ?? string.Empty)))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                return instance;
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetValue<T>(this DataRow row, string columnName)
        {
            try
            {
                if (row.HasColumn(columnName))
                    return (T)Convert.ChangeType(row[columnName], typeof(T));
            }
            catch
            {
                return default(T);
            }

            return default(T);
        }

        public static T TryConvert<T>(this string value, string typeName)
        {
            try
            {
                Type t = Type.GetType(typeName);

                return (T)Convert.ChangeType(value, t);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
