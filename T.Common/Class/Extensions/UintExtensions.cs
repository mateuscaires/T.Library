using System;

namespace T.Common
{
    public static class UintExtensions
    {
        public static uint ToUInt32(this object text)
        {
            try
            {
                return Convert.ToUInt32(text.ToString());
            }
            catch
            {
                return 0;
            }
        }
    }
}
