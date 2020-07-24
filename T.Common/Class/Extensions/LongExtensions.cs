using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T.Common
{
    public static class LongExtensions
    {
        public static long ToInt64(this object obj)
        {
            try
            {
                return (obj ?? string.Empty).ToString().ToInt64();
            }
            catch
            {
                return 0;
            }
        }

        public static long ToInt64(this string text)
        {
            try
            {
                return Convert.ToInt64(text.Trim());
            }
            catch
            {
                return 0;
            }
        }

        public static long FatorialRecusivo(this long valor)
        {
            if (valor < 2)
                return 1;
            return valor * (valor - 1).FatorialRecusivo();
        }

        public static long Fatorial(this long valor)
        {
            long result = valor;

            while (valor > 1)
            {
                valor--;
                result = result * valor;
            }
            return result;
        }
    }
}
