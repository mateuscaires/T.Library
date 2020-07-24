using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Entities
{
    internal static class ExtensionsMethods
    {
        internal static decimal ToDecimal(this string text, bool applyCents)
        {
            if (applyCents)
                text = text.Insert(text.Length - 2, CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
            return text.ToDecimal();
        }

        internal static decimal ToDecimal(this string text)
        {
            try
            {
                if (text.IsNullOrEmpty())
                    return 0;
                string separator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;

                int centIndex = 0;

                text = text.Replace(".", separator);
                text = text.Replace(",", separator);

                centIndex = text.LastIndexOf(separator);
                text = text.RemoveCentsSeparator();
                text = text.ApplyCents(centIndex);
                return Convert.ToDecimal(text.Trim());

            }
            catch
            {
                return 0M;
            }
        }

        internal static bool IsNullOrEmpty(this string text)
        {
            try
            {
                return string.IsNullOrWhiteSpace(text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static string RemoveCentsSeparator(this string t)
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

        internal static string ApplyCents(this string t, int index)
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
    }
}
