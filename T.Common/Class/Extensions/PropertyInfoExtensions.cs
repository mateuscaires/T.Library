using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace T.Common
{
    public static class PropertyInfoExtensions
    {
        public static PropertyInfo GetProperty<T>(this T obj, string name)
        {
            foreach (PropertyInfo p in obj.GetType().GetProperties())
            {
                if (p.Name.ToLower() == name.ToLower())
                    return p;
            }
            return null;
        }
    }
}
