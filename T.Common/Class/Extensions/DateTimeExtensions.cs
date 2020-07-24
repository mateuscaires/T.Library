using System;

namespace T.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this object obj)
        {
            if (obj.IsNull())
                return default(DateTime);

            return obj.ToString().ToDateTime();
        }

        public static DateTime ToDateTime(this string text)
        {
            try
            {
                if (text.IsNullOrEmpty())
                    return new DateTime(1900, 1, 1);

                DateTime data;
                if (!DateTime.TryParse(text, out data))
                {
                    text = text.Replace("/", string.Empty);
                    text = text.Replace("-", string.Empty);

                    if (text.Length == 6)
                        text = string.Concat(text, "01");

                    int dia = text.Substring(0, 2).ToInt32();
                    int mes = text.Substring(2, 2).ToInt32();
                    int ano = text.Substring(4, 4).ToInt32();

                    try
                    {
                        data = new DateTime(ano, mes, dia);
                    }
                    catch
                    {
                        dia = text.Substring(6, 2).ToInt32();
                        mes = text.Substring(4, 2).ToInt32();
                        ano = text.Substring(0, 4).ToInt32();
                    }

                    data = new DateTime(ano, mes, dia);
                }
                return data;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
