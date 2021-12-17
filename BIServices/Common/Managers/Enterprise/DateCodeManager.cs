using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Utilities;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public static class DateCodeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /*********************************************************************************
         * Requested Date
         *******************************************************************************/
        public static SqlCommand GetDateCodeRange()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              MIN(CAL_DATE) AS MIN_CAL_DATE,
              MAX(CAL_DATE) AS MAX_CAL_DATE
            FROM
              DW.CALENDAR_DATE_TYPE CDT
              INNER JOIN DW.CALENDAR_DATE_ATTRIBUTES CDA
              ON CDA.CAL_DATE_TYPE_ID = CDT.DATE_TYPE_ID
            WHERE
              CDT.DATE_TYPE_CD = @DateCode
			HAVING MIN(CAL_DATE) IS NOT NULL;";
            cmd.Parameters.Add(new SqlParameter("@DateCode", SqlDbType.VarChar, 30));
            return cmd;
        }

        public static DateTime[] GetDateCodeRange(string code)
        {
            var dtArray = new DateTime[2];
            var cn = new SqlConnection();
            try
            {
                cn = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn.Open();
                var cmd = GetDateCodeRange();
                cmd.Connection = cn;
                cmd.Parameters[0].Value = code;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        dtArray[0] = CastDbNull.To(reader["MIN_CAL_DATE"], new DateTime(1900, 1, 1));
                        dtArray[1] = CastDbNull.To(reader["MAX_CAL_DATE"], new DateTime(1900, 1, 1));
                    }
                    else
                    {
                        dtArray[0] = new DateTime(1900, 1, 1);
                        dtArray[1] = new DateTime(1900, 1, 1);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                cn.Close();
            }
            return dtArray;
        }

        public static DateTime GetDateCodeValue(string code)
        {
            var dt = new DateTime(1900, 1, 1);
            var cn = new SqlConnection();
            try
            {
                cn = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn.Open();
                var cmd = GetDateCodeRange();
                cmd.Connection = cn;
                cmd.Parameters[0].Value = code;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        dt = CastDbNull.To(reader["MAX_CAL_DATE"], new DateTime(1900, 1, 1));
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                cn.Close();
            }

            return dt;
        }

        public static bool IsDate(string code)
        {
            DateTime temp;
            if (DateTime.TryParse(code, out temp))
            {
                return true;
            }
            return false;
        }


        public static DateTime GetCodeDate(string code)
        {
            var dt = DateTime.Now;
            try
            {
                if (code.StartsWith("CY"))
                {
                    if (code.Contains("["))
                    {
                        var indexer = code.Substring(code.IndexOf("[") + 1, code.IndexOf("]") - code.IndexOf("[") - 1);
                        dt = dt.AddYears(int.TryParse(indexer, out var value) ? value : 0);
                    }
                    if (code.ToUpper().Contains("START"))
                    {
                        return new DateTime(dt.Year, 1, 1);
                    }
                    else if (code.ToUpper().Contains("END"))
                    {
                        return new DateTime(dt.Year, 12, 31);
                    }
                    else
                    {
                        return dt;
                    }
                }
                else if (code.StartsWith("CM"))
                {
                    if (code.Contains("["))
                    {
                        var indexer = code.Substring(code.IndexOf("[") + 1, code.IndexOf("]") - code.IndexOf("[") - 1);
                        dt = dt.AddMonths(int.TryParse(indexer, out var value) ? value : 0);
                    }
                    if (code.ToUpper().Contains("START"))
                    {
                        return new DateTime(dt.Year, dt.Month, 1);
                    }
                    else if (code.ToUpper().Contains("END"))
                    {
                        return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
                    }
                    else
                    {
                        return dt;
                    }
                }
                else if (code.StartsWith("CW"))
                {
                    if (code.Contains("["))
                    {
                        var indexer = code.Substring(code.IndexOf("[") + 1, code.IndexOf("]") - code.IndexOf("[") - 1);
                        dt = dt.AddDays(int.TryParse(indexer, out var value) ? value * 7 : 0);
                    }
                    if (code.ToUpper().Contains("START"))
                    {
                        return dt.AddDays(-1 * ((7 + (dt.DayOfWeek - DayOfWeek.Sunday)) % 7));
                    }
                    else if (code.ToUpper().Contains("END"))
                    {
                        return dt.AddDays(-1 * ((7 + (dt.DayOfWeek - DayOfWeek.Saturday)) % 7));
                    }
                    else
                    {
                        return dt;
                    }
                }
                else if (code.StartsWith("CD"))
                {
                    if (code.Contains("["))
                    {
                        var indexer = code.Substring(code.IndexOf("[") + 1, code.IndexOf("]") - code.IndexOf("[") - 1);
                        dt = dt.AddDays(int.TryParse(indexer, out var value) ? value : 0);
                    }
                    if (code.ToUpper().Contains("START"))
                    {
                        return new DateTime(dt.Year, dt.Month, dt.Day);
                    }
                    else if (code.ToUpper().Contains("END"))
                    {
                        return new DateTime(dt.Year, dt.Month, dt.Day).AddHours(23).AddMinutes(59).AddSeconds(59);
                    }
                    else
                    {
                        return dt;
                    }
                }
                else if (code.StartsWith("BD"))
                {
                    if (code.Contains("["))
                    {
                        var indexer = code.Substring(code.IndexOf("[") + 1, code.IndexOf("]") - code.IndexOf("[") - 1);
                        dt = AddBusinessDays(dt.Date, int.TryParse(indexer, out var value) ? value : 0);
                    }
                    if (code.ToUpper().Contains("START"))
                    {
                        return new DateTime(dt.Year, dt.Month, dt.Day);
                    }
                    else if (code.ToUpper().Contains("END"))
                    {
                        return new DateTime(dt.Year, dt.Month, dt.Day).AddHours(23).AddMinutes(59).AddSeconds(59);
                    }
                    else
                    {
                        return dt;
                    }
                }
                else if (DateTime.TryParse(code, out var value))
                {
                    return value;
                }
                else
                {
                    throw new Exception($"Invalid Date Format - {code}");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public static DateTime AddBusinessDays(DateTime pActualDate, int pNumberofWorkDays)
        {
            List<DateTime> holidays = new List<DateTime>();
            holidays.Add(new DateTime(DateTime.Now.Year, 12, 24));
            holidays.Add(new DateTime(DateTime.Now.Year, 12, 25));
            holidays.Add(new DateTime(DateTime.Now.Year, 1, 1));
            holidays.Add(new DateTime(DateTime.Now.Year + 1, 1, 1));

            var multiplier = (pNumberofWorkDays < 0) ? -1 : 1;
            int i = default(int);
            while (i < Math.Abs(pNumberofWorkDays))
            {
                pActualDate = pActualDate.AddDays(multiplier);
                if (pActualDate.DayOfWeek == DayOfWeek.Saturday || pActualDate.DayOfWeek == DayOfWeek.Sunday
                    || (holidays != null && holidays.Contains(pActualDate))) { }
                else
                { i++; }
            }
            return pActualDate;
        }
    }
}
