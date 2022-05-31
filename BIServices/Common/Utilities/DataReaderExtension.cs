using System;
using System.Data;

namespace BIServices.Common.Utilities
{
    public static class DataReaderExtension
    {

        public static T Get<T>(this IDataReader r, string columnName, T defaultValue = default(T))
        {
            var i = r.GetOrdinal(columnName);
            object obj;
            try
            {
                if (r.IsDBNull(i))
                {
                    return defaultValue;
                }
                else
                {
                    switch (typeof(T).Name.ToLower())
                    {
                        case "string":
                            obj = r.GetString(i).Trim();
                            break;
                        case "int16":
                            obj = r.GetInt16(i);
                            break;
                        case "int":
                            obj = r.GetInt32(i);
                            break;
                        case "int32":
                            obj = r.GetInt32(i);
                            break;
                        case "int64":
                            obj = r.GetInt64(i);
                            break;
                        case "decimal":
                            obj = r.GetDecimal(i);
                            break;
                        case "double":
                            obj = r.GetDouble(i);
                            break;
                        case "float":
                            obj = r.GetFloat(i);
                            break;
                        case "datetime":
                            obj = r.GetDateTime(i);
                            break;
                        case "boolean":
                            obj = r.GetBoolean(i);
                            break;
                        case "byte":
                            obj = r.GetByte(i);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(columnName);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"DataReader casting failure for - {columnName}");
            }
            return (T)obj;
        }
    }
}
