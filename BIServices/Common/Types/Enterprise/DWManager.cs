using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Types.Enterprise
{
    public class DWManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static F0005s GetDwF0005sByDRSPHD(SqlConnection cnDW, F0005s duplicateDwF0005s)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var f0005s = new F0005s();
            try
            {
                foreach (var d in duplicateDwF0005s)
                {
                    var filters = new List<string>
                    {
                        $@"[DRSY] = '{d.DRSY}' AND ",
                        $@"[DRRT] = '{d.DRRT}' AND ",
                        $@"UPPER([DRSPHD]) = '{d.DRSPHD}' "
                    };
                    var cmd = GetDwDRSPHD(filters);
                    cmd.Connection = cnDW;
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var f0005 = new F0005
                            {
                                DRSY = CastDbNull.To(reader["DRSY"], string.Empty).Trim(),
                                DRRT = CastDbNull.To(reader["DRRT"], string.Empty).Trim(),
                                DRKY = CastDbNull.To(reader["DRKY"], string.Empty).Trim(),
                                DRDL01 = CastDbNull.To(reader["DRDL01"], string.Empty).Trim(),
                                DRDL02 = CastDbNull.To(reader["DRDL02"], string.Empty).Trim(),
                                DRSPHD = CastDbNull.To(reader["DRSPHD"], string.Empty).Trim(),
                                DRUDCO = CastDbNull.To(reader["DRUDCO"], string.Empty).Trim(),
                                DRHRDC = CastDbNull.To(reader["DRHRDC"], string.Empty).Trim(),
                                DRUSER = CastDbNull.To(reader["DRUSER"], string.Empty).Trim(),
                                DRPID = CastDbNull.To(reader["DRPID"], string.Empty).Trim(),
                                DRUPMJ = CastDbNull.To<decimal>(reader["DRUPMJ"], 0),
                                DRJOBN = CastDbNull.To(reader["DRJOBN"], string.Empty).Trim(),
                                DRUPMT = CastDbNull.To<decimal>(reader["DRUPMT"], 0)
                            };
                            f0005s.Add(f0005);
                        }

                        reader.Close();
                        Logger.Trace($"ROWS: {f0005s.Count}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnDW.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnDW.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return f0005s;
        }

        private static SqlCommand GetDwDRSPHD(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        [DRSY]
                          ,[DRRT]
                          ,[DRKY]
                          ,[DRDL01]
                          ,[DRDL02]
                          ,[DRSPHD]
                          ,[DRUDCO]
                          ,[DRHRDC]
                          ,[DRUSER]
                          ,[DRPID]
                          ,[DRUPMJ]
                          ,[DRJOBN]
                          ,[DRUPMT]
                    FROM
                        [PROD].[PS_JWS_F0005] 
	                WHERE 
		                {filterString} "
            };
            return cmd;
        }

        public static F0005s GetDuplicateDWShortDescriptions(SqlConnection cnDW)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var f0005s = new F0005s();
            try
            {
                var cmd = GetDuplicateDRSPHD();
                cmd.Connection = cnDW;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var f0005 = new F0005
                        {
                            DRSY = CastDbNull.To(reader["DRSY"], string.Empty).Trim(),
                            DRRT = CastDbNull.To(reader["DRRT"], string.Empty).Trim(),
                            DRSPHD = CastDbNull.To(reader["DRSPHD"], string.Empty).Trim()
                        };
                        f0005s.Add(f0005);
                    }

                    reader.Close();
                    Logger.Trace($"ROWS: {f0005s.Count}");
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnDW.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnDW.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return f0005s;
        }

        private static SqlCommand GetDuplicateDRSPHD()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
	                SELECT 
                        [DRSY],
                        [DRRT],
		                UPPER([DRSPHD]) DRSPHD,
		                COUNT(*) FREQ
                    FROM
                        [PSBI_WH_PROD].[PROD].[PS_JWS_F0005] 
	                WHERE 
		                [DRSY] = '57'
		                AND [DRRT] IN ('TH','CL','TM','C1','CT','PT','LM') 
		                AND [DRSPHD] > ''
	                GROUP BY 
                        [DRSY],
                        [DRRT],
		                UPPER([DRSPHD])
	                HAVING COUNT(*) > 1 "
            };
            return cmd;
        }

        public static bool GetE1HALatency(SqlConnection cnDW)
        {
            // returns true if latent
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            bool isLatent;
            try
            {
                var cmd = GetE1HALatency(); // returns 1 row if in sync, 2 rows if latent
                cmd.Connection = cnDW;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var rows = 0;
                    while (reader.Read())
                    {
                        rows++;
                    }

                    reader.Close();
                    isLatent = (rows > 1);
                    Logger.Trace($"ROWS: {rows}");
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnDW.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnDW.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return isLatent;
        }

        public static SqlCommand GetE1HALatency()
        {
            // query returns 1 row if in sync, 2 rows if latent
            var cmd = new SqlCommand();
            try
            {
                var dateVal = Utilities.Common.CastToE1Date(DateTime.Today);
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    SELECT * FROM OPENQUERY(E1PD, '
	                    SELECT *
	                    FROM F580020 
	                    WHERE S6UPMJ = {dateVal}
	                    ORDER BY S6UPMJ DESC, S6UPMT DESC
	                    FETCH FIRST 1 ROWS ONLY
	                    FOR READ ONLY
                    ') 
                    UNION 
                    SELECT * FROM OPENQUERY(E1HA, '
	                    SELECT *
	                    FROM F580020 
	                    WHERE S6UPMJ = {dateVal}
	                    ORDER BY S6UPMJ DESC, S6UPMT DESC
	                    FETCH FIRST 1 ROWS ONLY
	                    FOR READ ONLY
                    ')";
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }
    }
}
