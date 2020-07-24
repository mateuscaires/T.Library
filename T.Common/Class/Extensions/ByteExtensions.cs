using System;
using System.Text;

namespace T.Common
{
    public static class ByteExtensions
    {
        public static byte[] FromBase64String(this string str)
        {
            try
            {
                return Convert.FromBase64String(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] ToByteArray(this string str)
        {
            return str.ToByteArray(Encoding.Default);
        }

        public static byte[] ToByteArray(this string str, Encoding encoding)
        {
            return encoding.GetBytes(str);
        }

        public static string GetGuid(this object obj)
        {
            string guid = Guid.NewGuid().ToString();

            guid = guid.Replace("-", string.Empty).ToUpper();

            return guid;
        }

        public static byte[] ToByteArray(this char[] chars)
        {
            try
            {
                string s = new string(chars);

                return s.ToByteArray();
            }
            catch (Exception)
            {
                return default(byte[]);
            }
        }
    }
}
