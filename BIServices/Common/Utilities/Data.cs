using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BIServices.Common.Utilities
{
    public class Data
    {
        public static class CastDbNull
        {
            public static T To<T>(object value, T defaultValue)
            {
                return (value != DBNull.Value) ? (T)value : defaultValue;
            }
        }

        public static class CastNull
        {
            public static T To<T>(object value, T defaultValue)
            {
                return (value != null) ? (T)Convert.ChangeType(value, typeof(T)) : defaultValue;
            }

        }

        public static class CastIsNullEmpty
        {
            public static T To<T>(object value, object check, T defaultValue)
            {
                return (value != check) ? (T)value : defaultValue;
            }
        }

        public static string PersistSqlString(string inputString)
        {
            var retValue = "";
            if (!string.IsNullOrEmpty(inputString))
            {
                retValue = inputString.Replace("'", "''");
            }
            return retValue;
        }

        public static string RetrieveSqlString(string inputString)
        {
            return inputString.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
        }

        public static string OmitBrackets(string inputString)
        {
            return inputString.Replace("[", "").Replace("]", "");
        }


        public static decimal CastStringToDecimal(string inputValue)
        {
            try
            {
                return Convert.ToDecimal(inputValue);
            }
            catch (Exception e)
            {
                var errorMsgx = e.Message;
                return 0;
            }
        }

        public static decimal ParseStringToDecimal(string inputValue)
        {
            decimal result;
            try
            {
                if (Decimal.TryParse(inputValue, out result))
                {
                    return result;
                }
                return 0;
            }
            catch (Exception e)
            {
                var errorMsgx = e.Message;
                return 0;
            }
        }

        public static DateTime ParseStringToDateTime(string inputValue)
        {
            DateTime result;
            try
            {
                if (DateTime.TryParse(inputValue, out result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public static string CastToFractionalDimension(decimal whole, decimal num, decimal den)
        {
            StringBuilder sb = new StringBuilder();
            if (den > 0)
            {
                sb.Append(Convert.ToString(whole, CultureInfo.CurrentCulture));
                sb.Append(" ");
                sb.Append(Convert.ToString(num, CultureInfo.CurrentCulture));
                sb.Append("/");
                sb.Append(Convert.ToString(den, CultureInfo.CurrentCulture));
            }
            else
            {
                sb.Append(Convert.ToString(whole, CultureInfo.CurrentCulture));
            }

            if (sb.ToString() == "0")
            {
                return "";
            }
            else
            {
                return sb.ToString();
            }

        }


        public static class UnboxParm
        {
            public static T To<T>(Dictionary<string, object> parms, string name, T defaultValue)
            {
                try
                {
                    var value = CollectionUtils.GetValueOrDefault(parms, name);
                    return (value != null) ? (T)value : defaultValue;
                }
                catch
                {
                    throw new Exception($"Unboxing Error for Column {name}");
                }
            }

            public static List<string> ToStringList(Dictionary<string, object> parms, string name)
            {
                var value = CollectionUtils.GetValueOrDefault(parms, name);
                return (value != null) ? value.ToString().Split(',').ToList() : new List<string>();
            }

            public static void Validate(Dictionary<string, object> parms, string[] validNames)
            {
                var invalidParms = parms.Keys.Where(x => validNames.All(x2 => x2 != x)).ToList();
                if (invalidParms.Any())
                {
                    throw new Exception($"Invalid Parameters {string.Join(",", invalidParms)}");
                }
            }
        }
    }

}
