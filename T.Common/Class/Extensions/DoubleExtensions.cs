using System;

namespace T.Common
{
    public static class DoubleExtensions
    {
        public static double ToDouble(this string text)
        {
            try
            {
                return Convert.ToDouble(text.Trim());

            }
            catch
            {
                return 0;
            }
        }

        public static double Truncate(this double value, int decimalPlaces)
        {
            if (decimalPlaces < 0)
                return value;

            var modifier = Convert.ToDouble(0.5 / Math.Pow(10, decimalPlaces));
            return Math.Round(value >= 0 ? value - modifier : value + modifier, decimalPlaces);
        }

        public static double Truncate(this double value)
        {
            return value.Truncate(2);
        }
    }
}
