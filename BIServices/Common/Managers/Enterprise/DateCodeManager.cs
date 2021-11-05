using System;
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
    }
}
