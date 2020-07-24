using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace T.Common
{
    public static class DecimalExtensions
    {
        public static decimal ToDecimal(this object text)
        {
            try
            {
                return text.ToDecimal(2);
            }
            catch
            {
                return 0;
            }
        }

        public static decimal ToDecimal(this object text, int precision)
        {
            try
            {
                try
                {
                    decimal a = text.ToString().ToDecimal();
                    return Math.Round(a, precision);

                }
                catch
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static decimal ToDecimal(this string text, bool applyCents)
        {
            if (applyCents)
                text = text.Insert(text.Length - 2, CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
            return text.ToDecimal();
        }

        public static decimal ToDecimal(this string text)
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

        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public static decimal Trunc(this decimal value, int decimalPlaces)
        {
            if (decimalPlaces < 0)
                throw new ArgumentException("decimalPlaces must be greater than or equal to 0.");

            value = value.Normalize();

            if (((SqlDecimal)value).Scale > decimalPlaces)
            {
                var modifier = Convert.ToDecimal(0.5 / Math.Pow(10, decimalPlaces));

                value = Math.Round(value >= 0 ? value - modifier : value + modifier, decimalPlaces);
            }

            return value;
        }

        public static decimal Trunc(this decimal value)
        {
            return Trunc(value, 2);
        }

        public static decimal Round(this decimal value)
        {
            return Round(value, 2);
        }

        public static decimal Round(this decimal value, int decimals)
        {
            return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        }
    }
}
