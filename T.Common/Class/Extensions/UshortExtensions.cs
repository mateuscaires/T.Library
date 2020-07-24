using System;

namespace T.Common
{
    public static class UshortExtensions
    {
        public static ushort ToUInt16(this object text)
        {
            try
            {
                return Convert.ToUInt16(text.ToString());
            }
            catch
            {
                return 0;
            }
        }
    }
}
