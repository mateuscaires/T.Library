using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using T.Interfaces;

namespace T.Common
{
    public static class BoolExtensions
    {
        public static bool HasItems<T>(this T[] collection)
        {
            if (collection.IsNull())
                return false;
            return collection.Count() > 0;
        }

        public static bool HasItems<T>(this T[] collection, Expression<Func<T, bool>> funk)
        {
            if (collection.IsNull())
                return false;

            return collection.Where(funk.Compile()).Count() > 0;
        }

        public static bool HasItems<T>(this List<T> items, Expression<Func<T, bool>> funk)
        {
            if (items.IsNull())
                return false;

            return items.Where(funk.Compile()).Count() > 0;
        }

        public static bool HasItems<T>(this List<T> items)
        {
            if (items.IsNull())
                return false;

            return items.Count > 0;
        }
        
        public static bool ToBool(this string text)
        {
            try
            {
                if (text.IsNullOrEmpty())
                    return false;

                text = text.ToLower();

                switch (text)
                {
                    case "true":
                    case "1":
                        return true;
                    case "false":
                    case "0":
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool IsBool(this string text)
        {
            try
            {
                if (text.IsNullOrEmpty())
                    return false;

                text = text.ToLower();

                switch (text)
                {
                    case "true":
                    case "false":
                    case "0":
                    case "1":
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ToBool(this int text)
        {
            try
            {
                return ToBool(text.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ToBool(this object text)
        {
            try
            {
                return ToBool(text.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool HasValue(this object obj)
        {
            try
            {
                return obj != null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsNull(this object obj)
        {
            try
            {
                return obj == null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool HasRows(this DataTable table)
        {
            try
            {
                if (table == null)
                    return false;
                return table.Rows.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool Empty(this DataTable table)
        {
            try
            {
                if (table == null)
                    return true;
                return table.Rows.Count == 0;
            }
            catch
            {
                return true;
            }
        }

        public static bool HasColumn(this DataRow row, string columnName)
        {
            try
            {
                return row.Table.HasColumn(columnName);
            }
            catch
            {
                return false;
            }
        }

        public static bool HasColumn(this DataTable table, string columnName)
        {
            columnName = (columnName ?? string.Empty).Trim();

            string currentCol = string.Empty;

            try
            {
                foreach (DataColumn col in table.Columns)
                {
                    currentCol = col.ColumnName.Trim();

                    if (columnName.Equals(currentCol, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasItems<T>(this IEnumerable<T> items, Expression<Func<T, bool>> funk)
        {
            try
            {
                if (items == null)
                    return false;
                return items.Where(funk.Compile()).Count() > 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasItems<T>(this IEnumerable<T> items)
        {
            try
            {
                if (items == null)
                    return false;
                return items.Any();
            }
            catch
            {
                return false;
            }
        }

        public static bool Empty<T>(this IEnumerable<T> items)
        {
            try
            {
                return !items.HasItems();
            }
            catch
            {
                return true;
            }
        }

        public static bool HasText(this string s)
        {
            return !(s ?? string.Empty).IsNullOrEmpty();
        }

        public static bool HasText(this object s)
        {
            return !(s ?? string.Empty).ToString().IsNullOrEmpty();
        }

        public static bool IsValid(this IValidate obj)
        {
            return (obj.HasValue() && obj.Validate());
        }

        public static bool IsCPF(this string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", string.Empty).Replace("-", string.Empty);

            if (cpf.Length > 11)
                return false;

            while (cpf.Length < 11)
            {
                cpf = string.Concat("0", cpf);
            }

            switch (cpf)
            {
                case "00000000000":
                    return false;
                case "11111111111":
                    return false;
                case "22222222222":
                    return false;
                case "33333333333":
                    return false;
                case "44444444444":
                    return false;
                case "55555555555":
                    return false;
                case "66666666666":
                    return false;
                case "77777777777":
                    return false;
                case "88888888888":
                    return false;
                case "99999999999":
                    return false;
            }

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCpf = tempCpf + digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        public static bool IsCnpj(this string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;

            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cnpj.Length != 14)
                return false;

            tempCnpj = cnpj.Substring(0, 12);

            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

        public static bool IsNullOrEmpty(this string text)
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

        public static bool IsNumeric(this string t)
        {
            if (!string.IsNullOrWhiteSpace(t))
            {
                t = t.Replace(".", string.Empty).Replace(",", string.Empty).Replace("-", string.Empty);
                Regex reg = new Regex(@"^[0-9]+$");
                return reg.IsMatch(t.Trim());
            }
            return false;
        }

        public static bool IsZero(this int value)
        {
            return value == 0;
        }

        public static bool IsZero(this decimal value)
        {
            return value == 0;
        }

        public static bool IsDate(this string text, out DateTime data)
        {
            try
            {
                if (DateTime.TryParse(text, out data))
                    return true;
                text = text.Replace("/", "");
                text = text.Replace("-", "");
                string ano = text.Substring(0, 4);
                string mes = text.Substring(4, 2);
                string dia = text.Substring(6, 2);

                if (DateTime.TryParse(ano + "/" + mes + "/" + dia, out data))
                    return true;

                dia = text.Substring(0, 2);
                mes = text.Substring(2, 2);
                ano = text.Substring(4, 4);

                if (DateTime.TryParse(ano + "/" + mes + "/" + dia, out data))
                    return true;

                mes = text.Substring(0, 2);
                dia = text.Substring(2, 2);
                ano = text.Substring(4, 4);

                if (DateTime.TryParse(ano + "/" + mes + "/" + dia, out data))
                    return true;

                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsDate(this string text)
        {
            try
            {
                DateTime data;
                if (DateTime.TryParse(text, out data))
                    return true;
                text = text.Replace("/", "");
                text = text.Replace("-", "");
                string ano = text.Substring(0, 4);
                string mes = text.Substring(4, 2);
                string dia = text.Substring(6, 2);

                if (DateTime.TryParse(ano + "/" + mes + "/" + dia, out data))
                    return true;

                dia = text.Substring(0, 2);
                mes = text.Substring(2, 2);
                ano = text.Substring(4, 4);

                if (DateTime.TryParse(ano + "/" + mes + "/" + dia, out data))
                    return true;

                mes = text.Substring(0, 2);
                dia = text.Substring(2, 2);
                ano = text.Substring(4, 4);

                if (DateTime.TryParse(ano + "/" + mes + "/" + dia, out data))
                    return true;

                else
                    return false;
            }
            catch { return false; }
        }

        public static bool CanAssignValue(this PropertyInfo p, object value)
        {
            return value == null ? p.IsNullable() : p.PropertyType.IsInstanceOfType(value);
        }

        public static bool IsNullable(this PropertyInfo p)
        {
            return p.PropertyType.IsNullable();
        }

        public static bool IsNullable(this Type t)
        {
            return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
        }

        public static bool IsWeekend(this DateTime date)
        {
            return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }

        public static bool IsEmail(this string email)
        {
            bool valid = false;
            var indexArr = email.IndexOf('@');
            if (indexArr > 0)
            {
                var indexDot = email.IndexOf('.', indexArr);
                if (indexDot - 1 > indexArr)
                {
                    if (indexDot + 1 < email.Length)
                    {
                        var indexDot2 = email.Substring(indexDot + 1, 1);
                        if (indexDot2 != ".")
                        {
                            valid = true;
                        }
                    }
                }
            }
            return valid;
        }

        public static bool HasSpecialChar(this string s)
        {
            return s.HasSpecialChar(default(char));
        }

        public static bool HasSpecialChar(this string s, char ignore)
        {
            var invalids = CharExtensions.GetSpecialChars();

            foreach (var item in s)
            {
                if (item == ignore)
                    continue;
                if (invalids.Where(a => a == item).Count() > 0)
                    return true;
            }
            return false;
        }

        public static bool IsValid(this string text, Validators validator)
        {
            switch (validator)
            {
                case Validators.NENHUM: return true;
                case Validators.CPF: return text.IsCPF();
                case Validators.CNPJ: return text.IsCnpj();
                case Validators.EMAIL: return text.IsEmail();
                case Validators.NUMERO: return text.IsNumeric();
                case Validators.DATA: return text.IsDate();
                case Validators.TEXTO: return text.HasText();
                case Validators.BIT: return text.ToBool();
                default: return false;
            }
        }

        public static bool IsPrimaryKey(this PropertyInfo prop)
        {
            var attribute = prop.GetCustomAttributes(typeof(PrimaryKey), true)
                            .FirstOrDefault() as PrimaryKey;

            return attribute.HasValue();
        }

        public static bool IsIdentity(this PropertyInfo prop)
        {
            var attribute = prop.GetCustomAttributes(typeof(IdentityAttribute), true)
                            .FirstOrDefault() as IdentityAttribute;

            return attribute.HasValue();
        }

        public static bool True(this string text)
        {
            return text.ToBool();
        }

        public static bool False(this string text)
        {
            return !text.ToBool();
        }

        public static bool True(this char text)
        {
            return text.ToBool();
        }

        public static bool False(this char text)
        {
            return !text.ToBool();
        }

        public static bool IsPrime(this int number)
        {
            if (number == 1)
                return false;

            if (number == 2)
                return true;

            if (number % 2 == 0)
                return false;

            int boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }

            return true;
        }

        public static bool IsOdd(this int val)
        {
            return val % 2 > 0;
        }
    }
}
