using System;
using System.Collections.Generic;
using System.Reflection;

namespace T.Common
{
    public static class VoidExtensions
    {
        public static void SetProperty<T>(this T TObject, string propertyName, object value)
        {
            try
            {
                Type type = TObject.GetType();
                var propriedades = new List<PropertyInfo>(type.GetProperties());

                foreach (var p in propriedades)
                {
                    if (p.Name.ToUpper() == propertyName.ToUpper())
                    {
                        p.SetValue(TObject, value, null);

                        break;
                    }
                }
            }
            catch
            {

            }
        }

        public static void SetInstance<T>(this object o) where T : class, new()
        {
            o = new T();
        }

        public static void Instanciate<T>(this T obj) where T : class, new()
        {
            obj = new T();
        }
    }
}
