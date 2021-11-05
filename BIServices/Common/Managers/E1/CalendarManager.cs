using System;
using System.Collections.Generic;
using System.Data;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class CalendarManager
    {

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public CalendarManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("CalendarManager.GetCalendar", GetCalendar());
            foreach (var keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        public static DateTime GetNextBusinessDate(BusinessSystemSession session, Plant plant, string shift, string key, DateTime referenceDate)
        {
            var dateTime = referenceDate.AddDays(1);
            bool isBusinessDate;
            int attempts = 1;

            do
            {
                isBusinessDate = IsBusinessDate(session, plant, shift, key, dateTime);
                if (!isBusinessDate)
                {
                    dateTime = dateTime.AddDays(1);
                    attempts++;
                }
            } while (isBusinessDate == false && attempts < 30);
            return dateTime;
        }

        public static DateTime GetBusinessDate(BusinessSystemSession session, Plant plant, string shift, string key, DateTime referenceDate, int offsetDays)
        {
            int incr = -1;
            int absDays = Math.Abs(offsetDays);
            if (offsetDays > 0)
            {
                incr = 1;
            }

            var dateTime = DateTime.MinValue;
            try
            {
                int count = -1;
                do
                {
                    if (IsBusinessDate(session, plant, shift, key, referenceDate))
                    {
                        dateTime = referenceDate;
                        count++;
                    }
                    referenceDate = referenceDate.AddDays(incr);
                } while (count < absDays);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + e.Message);
                throw new ApplicationException(e.Message);
            }
            return dateTime;
        }

        public static bool IsBusinessDate(BusinessSystemSession session, Plant plant, string shift, string key, DateTime referenceDate)
        {
            var ret = false;
            try
            {
                var cmd = (session.DbCommands.ContainsKey("GetCalendar")) ? session.DbCommands["GetCalendar"] : GetCalendar();
                cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                cmd.Parameters[1].Value = 1;
                cmd.Parameters[2].Value = Convert.ToInt32(referenceDate.Year.ToString().Substring(2, 2));
                cmd.Parameters[3].Value = referenceDate.Month;
                cmd.Parameters[4].Value = shift;
                cmd.Parameters[5].Value = key;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var name = Data.CastDbNull.To<String>(reader["NAME"], String.Empty).Trim();
                        var val = Data.CastDbNull.To<String>(reader["VAL"], String.Empty).Trim();
                        var dayVal = Convert.ToInt32(name.Substring(2, 2));
                        if (dayVal == referenceDate.Day && val == "W")
                        {
                            ret = true;
                            break;
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return ret;
        }
        /******************************************************************
        * Next Business Day
        ******************************************************************/
        public static iDB2Command GetCalendar()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            CZMMCU,
	            CZCTRY,
	            CZYR,
	            CZMT,
	            CZSHFT,
	            CZWDCT,
	            NAME,
	            VAL
            FROM 
                F0007
                CROSS JOIN TABLE (VALUES
                ('TD01', CAST(CZTD01 AS GRAPHIC(1))) ,
                ('TD02', CAST(CZTD02 AS GRAPHIC(1))) ,
                ('TD03', CAST(CZTD03 AS GRAPHIC(1))) ,
                ('TD04', CAST(CZTD04 AS GRAPHIC(1))) ,
                ('TD05', CAST(CZTD05 AS GRAPHIC(1))) ,
                ('TD06', CAST(CZTD06 AS GRAPHIC(1))) ,
                ('TD07', CAST(CZTD07 AS GRAPHIC(1))) ,
                ('TD08', CAST(CZTD08 AS GRAPHIC(1))) ,
                ('TD09', CAST(CZTD09 AS GRAPHIC(1))) ,
                ('TD10', CAST(CZTD10 AS GRAPHIC(1))) ,
                ('TD11', CAST(CZTD11 AS GRAPHIC(1))) ,
                ('TD12', CAST(CZTD12 AS GRAPHIC(1))) ,
                ('TD13', CAST(CZTD13 AS GRAPHIC(1))) ,
                ('TD14', CAST(CZTD14 AS GRAPHIC(1))) ,
                ('TD15', CAST(CZTD15 AS GRAPHIC(1))) ,
                ('TD16', CAST(CZTD16 AS GRAPHIC(1))) ,
                ('TD17', CAST(CZTD17 AS GRAPHIC(1))) ,
                ('TD18', CAST(CZTD18 AS GRAPHIC(1))) ,
                ('TD19', CAST(CZTD19 AS GRAPHIC(1))) ,
                ('TD20', CAST(CZTD20 AS GRAPHIC(1))) ,
                ('TD21', CAST(CZTD21 AS GRAPHIC(1))) ,
                ('TD22', CAST(CZTD22 AS GRAPHIC(1))) ,
                ('TD23', CAST(CZTD23 AS GRAPHIC(1))) ,
                ('TD24', CAST(CZTD24 AS GRAPHIC(1))) ,
                ('TD25', CAST(CZTD25 AS GRAPHIC(1))) ,
                ('TD26', CAST(CZTD26 AS GRAPHIC(1))) ,
                ('TD27', CAST(CZTD27 AS GRAPHIC(1))) ,
                ('TD28', CAST(CZTD28 AS GRAPHIC(1))) ,
                ('TD29', CAST(CZTD29 AS GRAPHIC(1))) ,
                ('TD30', CAST(CZTD30 AS GRAPHIC(1))) ,
                ('TD31', CAST(CZTD31 AS GRAPHIC(1))) 
                    ) AS T (NAME, VAL)
            WHERE
                CZMMCU = ? AND
                CZCTRY = ? AND
                CZYR = ? AND
                CZMT = ? AND
                CZSHFT = ? AND
                CZWDCT = ?
            ORDER BY
                CZYR,
                CZMT, 
                NAME";
            cmd.Parameters.Add(new iDB2Parameter("CZMMCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("CZCTRY", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CZCTRY"].Precision = 2;
            cmd.Parameters["CZCTRY"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("CZYR", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CZYR"].Precision = 2;
            cmd.Parameters["CZYR"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("CZMT", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CZMT"].Precision = 2;
            cmd.Parameters["CZMT"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("CZSHFT", iDB2DbType.iDB2Graphic, 1));
            cmd.Parameters.Add(new iDB2Parameter("CZWDCT", iDB2DbType.iDB2Graphic, 10));
            return cmd;
        }
    }
}
