using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Mail
{
    internal static class Extensions
    {
        internal static bool Empty<T>(this IEnumerable<T> val)
        {
            if (val == null)
                return true;

            return val.Count() == 0;
        }

        internal static bool IsNullOrEmpty(this string val)
        {
            return string.IsNullOrWhiteSpace(val);
        }

        internal static bool IsEmail(this string email)
        {
            bool valid = false;
            var indexArr = email.IndexOf('@');
            if (indexArr > 0)
            {
                var indexDot = email.IndexOf('.', indexArr);
                if (indexDot - 1 > indexArr)
                {
                    if (indexDot + 1 < email.Length)
                    {
                        var indexDot2 = email.Substring(indexDot + 1, 1);
                        if (indexDot2 != ".")
                        {
                            valid = true;
                        }
                    }
                }
            }
            return valid;
        }

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
    }
}
