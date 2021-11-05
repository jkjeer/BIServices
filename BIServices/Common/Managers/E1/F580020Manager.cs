using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.E1
{
    public class F580020Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /*********************************************************************************
         * Get F580020s Dynamic
         *******************************************************************************/
        public static iDB2Command GetF580020Status(IReadOnlyCollection<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        S6EDBT,
                        S6MCU,
                        S6Z58SCN,
                        S6Z58SCD,
                        S6Z58SFS,
                        S6TSC,
                        S6EDSP,
                        S6MATH06,
                        S6WOPST,
                        S6DS50, 
                        S7UPMJ,
                        S7TDAY, 
                        S7Z58SCTYP,
                        S7SHAN,
                        S7ALPH,
                        S7Z58SCDDJ,
                        DRDL01,
                        COUNT(S7DOCO) AS WOCOUNT,
                        SUM(S7Z58QTY) AS QUANTITY 
                    FROM F580020 
		            LEFT OUTER JOIN F580021 F21A
                        ON F21A.S7MCU = S6MCU 
						AND F21A.S7Z58SCN = S6Z58SCN
						AND F21A.S7EDBT = S6EDBT
                    LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005
                        ON  DRSY = '58' 
                        AND DRRT = 'SY' 
                        AND TRIM(DRKY) = S6Z58SFS
                    WHERE 
                        S6Z58SCN < 1000 AND
                        {filterString}
                    GROUP BY S6EDBT,
                        S6MCU,
                        S6Z58SCN,
                        S6Z58SCD,
                        S6Z58SFS,
                        S6TSC,
                        S6EDSP,
                        S6MATH06,
                        S6WOPST,
                        S6DS50, 
                        S7UPMJ,
                        S7TDAY, 
                        S7Z58SCTYP,
                        S7SHAN,
                        S7ALPH,
                        S7Z58SCDDJ,
                        DRDL01
                    ORDER BY S6EDBT"
            };
            return cmd;
        }

        private static iDB2Command GetOverdueF580020s(IReadOnlyCollection<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        S6EDBT,
                        S6MCU,
                        S6Z58SCN,
                        S6Z58SCD,
                        S6Z58SFS,
                        S6TSC,
                        S6EDSP,
                        S6MATH06,
                        S6WOPST,
                        S6DS50, 
                        F21A.S7UPMJ, 
                        F21A.S7TDAY,
                        DRDL01,
                        WO.WOCount,
                        S1Z58TQSC,
						SW.ScheduledWorkorders
                    FROM F580020 
		            LEFT OUTER JOIN F580021 F21A
                        ON F21A.S7MCU = S6MCU 
						AND F21A.S7Z58SCN = S6Z58SCN
						AND F21A.S7EDBT = S6EDBT
		            LEFT OUTER JOIN F5834A01 
                        ON S1MCU = S6MCU 
						AND S1Z58SCN = S6Z58SCN
						AND S1Z58SCD = S6Z58SCD
                    LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005
                        ON  DRSY = '58' 
                        AND DRRT = 'SY' 
                        AND TRIM(DRKY) = S6Z58SFS
		            LEFT OUTER JOIN (SELECT S7MCU, S7Z58SCN, S7EDBT, CAST(COUNT(S7DOCO) AS INT) AS WOCount 
					            FROM F580021 F21B 
					            GROUP BY S7MCU, S7Z58SCN, S7EDBT) WO ON WO.S7MCU = S6MCU 
															            AND WO.S7Z58SCN = S6Z58SCN
															            AND WO.S7EDBT = S6EDBT
		            LEFT OUTER JOIN (SELECT S5MCU, S5Z58SCN, S5Z58SCD, CAST(COUNT(S5DOCO) AS INT) AS ScheduledWorkorders 
					            FROM F5834A11 A11 
					            GROUP BY S5MCU, S5Z58SCN, S5Z58SCD) SW ON SW.S5MCU = S6MCU 
															            AND SW.S5Z58SCN = S6Z58SCN
															            AND SW.S5Z58SCD = S6Z58SCD
                    WHERE  
                        F21A.S7EDLN = 1000 AND 
                        {filterString}
                    ORDER BY F21A.S7MCU, F21A.S7Z58SCN, F21A.S7UPMJ, F21A.S7TDAY 
                    FOR READ ONLY"
            };
            return cmd;
        }

        /*********************************************************************************
         * Methods
         ********************************************************************************/
        public static F580020s GetF580020s(BusinessSystemSession e1Session, int lookBackMinutes)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var f580020s = new F580020s();
            var filters = new List<string>();
            try
            {
                var lookBackDateTime = DateTime.Now.AddMinutes(-lookBackMinutes);
                var lookBackDate = Utilities.Common.CastToE1Date(lookBackDateTime);
                var lookBackTime = Utilities.Common.CastToE1Time(lookBackDateTime);
                filters.Add($"(F21A.S7UPMJ < {lookBackDate} OR (F21A.S7UPMJ = {lookBackDate} AND F21A.S7TDAY <= {lookBackTime})) AND \r\n");
                //filters.Add("F21A.S7EDLN = 1 AND \r\n");
                // Unprocessed
                //filters.Add("S6TSC = 'Y' AND \r\n");
                filters.Add($@"( ( S6EDSP = 'N' ) OR ( S6EDSP = 'U' AND S6DS50 <= CHAR( CURRENT_TIMESTAMP - {ConfigurationManager.AppSettings["F580020UExclusion"]} MINUTES ) ) )");
                var cmd = GetOverdueF580020s(filters);
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var f580020 = new F580020
                        {
                            S6EDBT = CastDbNull.To(reader["S6EDBT"], string.Empty).Trim(),
                            S6MCU = CastDbNull.To(reader["S6MCU"], string.Empty).Trim(),
                            S6Z58SCN = CastDbNull.To<decimal>(reader["S6Z58SCN"], 0),
                            S6Z58SCD = CastDbNull.To<decimal>(reader["S6Z58SCD"], 0),
                            S6Z58SFS = CastDbNull.To(reader["S6Z58SFS"], string.Empty).Trim(),
                            S6TSC = CastDbNull.To(reader["S6TSC"], string.Empty).Trim(),
                            S6EDSP = CastDbNull.To(reader["S6EDSP"], string.Empty).Trim(),
                            S6MATH06 = CastDbNull.To<decimal>(reader["S6MATH06"], 0),
                            S6WOPST = CastDbNull.To(reader["S6WOPST"], string.Empty).Trim(),
                            S6DS50 = CastDbNull.To(reader["S6DS50"], string.Empty).Trim(),
                            S6UPMJ = CastDbNull.To<decimal>(reader["S7UPMJ"], 0),
                            S6TDAY = CastDbNull.To<decimal>(reader["S7TDAY"], 0),
                            S7UPMJ = CastDbNull.To<decimal>(reader["S7UPMJ"], 0),
                            S7TDAY = CastDbNull.To<decimal>(reader["S7TDAY"], 0),
                            DRDL01 = CastDbNull.To(reader["DRDL01"], string.Empty).Trim(),
                            WOCount = CastDbNull.To(reader["WOCount"], 0),
                            S1Z58TQSC = CastDbNull.To<decimal>(reader["S1Z58TQSC"], 0),
                            ScheduledWorkorders = CastDbNull.To(reader["ScheduledWorkorders"], 0)
                        };
                        f580020s.Add(f580020);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: {f580020s.Count}");
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e1Session.DbConnection.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e1Session.DbConnection.ConnectionString }  { e.Message}: { e.StackTrace}");
            }
            finally
            {
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return f580020s;
        }

        public static F580020s GetOverdueF580020s(BusinessSystemSession e1Session, int lookBackMinutes)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var f580020s = new F580020s();
            var filters = new List<string>();
            try
            {
                var lookBackDateTime = DateTime.Now.AddMinutes(-lookBackMinutes);
                var lookBackDate = Utilities.Common.CastToE1Date(lookBackDateTime);
                var lookBackTime = Utilities.Common.CastToE1Time(lookBackDateTime);
                filters.Add($"(F21A.S7UPMJ < {lookBackDate} OR (F21A.S7UPMJ = {lookBackDate} AND F21A.S7TDAY <= {lookBackTime})) AND \r\n");
                //filters.Add("F21A.S7EDLN = 1 AND \r\n");
                // Unprocessed
                //filters.Add("S6TSC = 'Y' AND \r\n");
                int lookbackMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["EDSPLookbackMinutes"]);
                filters.Add($@"( ( S6EDSP = 'N' ) OR ( S6EDSP = 'R' ) OR ( S6EDSP = 'U' AND ( ( S6UPMJ < {E1TimeManager.CastToE1Date(DateTime.Now.ToLocalTime())} ) OR ( S6UPMJ = {E1TimeManager.CastToE1Date(DateTime.Now.ToLocalTime())} AND S6UPMT < {E1TimeManager.CastToE1Time(DateTime.Now.ToLocalTime().AddMinutes(-lookbackMinutes))} ) ) ) )");
                var cmd = GetOverdueF580020s(filters);
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var f580020 = new F580020
                        {
                            S6EDBT = CastDbNull.To(reader["S6EDBT"], string.Empty).Trim(),
                            S6MCU = CastDbNull.To(reader["S6MCU"], string.Empty).Trim(),
                            S6Z58SCN = CastDbNull.To<decimal>(reader["S6Z58SCN"], 0),
                            S6Z58SCD = CastDbNull.To<decimal>(reader["S6Z58SCD"], 0),
                            S6Z58SFS = CastDbNull.To(reader["S6Z58SFS"], string.Empty).Trim(),
                            S6TSC = CastDbNull.To(reader["S6TSC"], string.Empty).Trim(),
                            S6EDSP = CastDbNull.To(reader["S6EDSP"], string.Empty).Trim(),
                            S6MATH06 = CastDbNull.To<decimal>(reader["S6MATH06"], 0),
                            S6WOPST = CastDbNull.To(reader["S6WOPST"], string.Empty).Trim(),
                            S6DS50 = CastDbNull.To(reader["S6DS50"], string.Empty).Trim(),
                            S6UPMJ = CastDbNull.To<decimal>(reader["S7UPMJ"], 0),
                            S6TDAY = CastDbNull.To<decimal>(reader["S7TDAY"], 0),
                            S7UPMJ = CastDbNull.To<decimal>(reader["S7UPMJ"], 0),
                            S7TDAY = CastDbNull.To<decimal>(reader["S7TDAY"], 0),
                            DRDL01 = CastDbNull.To(reader["DRDL01"], string.Empty).Trim(),
                            WOCount = CastDbNull.To(reader["WOCount"], 0),
                            S1Z58TQSC = CastDbNull.To<decimal>(reader["S1Z58TQSC"], 0),
                            ScheduledWorkorders = CastDbNull.To(reader["ScheduledWorkorders"], 0)
                        };
                        f580020s.Add(f580020);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: {f580020s.Count}");
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e1Session.DbConnection.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e1Session.DbConnection.ConnectionString }  { e.Message}: { e.StackTrace}");
            }
            finally
            {
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return f580020s;
        }
    }
}
