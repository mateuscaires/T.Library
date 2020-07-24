using System;

namespace T.Common
{
    public static class UlongExtensions
    {
        public static ulong ToUInt64(this object obj)
        {
            try
            {
                return (obj ?? string.Empty).ToString().ToUInt64();
            }
            catch
            {
                return 0;
            }
        }

        public static ulong ToUInt64(this string text)
        {
            try
            {
                return Convert.ToUInt64(text.Trim());
            }
            catch
            {
                return 0;
            }
        }
    }
}
