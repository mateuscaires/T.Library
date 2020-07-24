
namespace T.Common
{
    public static class ShortExtensions
    {
        public static short ToInt16(this object text)
        {
            try
            {
                short a = 0;
                if (short.TryParse(text.ToString(), out a))
                    return a;
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
