using System;
using System.Configuration;
using System.Data;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.E1
{
    public class F0005Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static F0005s GetDuplicateDRSPHD(BusinessSystemSession e1Session)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var f0005s = new F0005s();
            try
            {
                var cmd = GetDuplicateDRSPHD();
                cmd.Connection = e1Session.DbConnection;
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
                }

                Logger.Trace($"ROWS: {f0005s.Count}");
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return f0005s;
        }

        private static iDB2Command GetDuplicateDRSPHD()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        DRSY,
                        DRRT,
		                TRIM(DRSPHD) DRSPHD,
		                COUNT(*) FREQ
                    FROM
                        {ConfigurationManager.AppSettings["ControlLib"]}.F0005 
	                WHERE 
		                DRSY = '57'
		                AND ( ( DRRT IN ('CL','TM','C1','CT','PT') )
                        OR ( DRRT IN ('TH') 
		                    AND DRSPHD > '' )
                        OR ( DRRT IN ('LM') 
		                    AND DRSPHD > '' ) )
	                GROUP BY 
                        DRSY,
                        DRRT,
		                TRIM(DRSPHD)
	                HAVING COUNT(*) > 1 
                "
            };
            return cmd;
        }

        public static F0005s GetF0005sByDRSPHD(BusinessSystemSession e1Session, F0005s duplicateDRSPHDs)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var f0005s = new F0005s();
            try
            {
                foreach (var d in duplicateDRSPHDs)
                {
                    var cmd = GetF0005sByDRSPHD();
                    cmd.Parameters["DRSY"].Value = d.DRSY;
                    cmd.Parameters["DRRT"].Value = d.DRRT;
                    cmd.Parameters["DRSPHD"].Value = d.DRSPHD;
                    cmd.Connection = e1Session.DbConnection;
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var f0005 = new F0005
                            {
                                DRSY = CastDbNull.To(reader["DRSY"], ""),
                                DRRT = CastDbNull.To(reader["DRRT"], ""),
                                DTDL01 = CastDbNull.To(reader["DTDL01"], ""),
                                DRKY = CastDbNull.To(reader["DRKY"], ""),
                                DRDL01 = CastDbNull.To(reader["DRDL01"], ""),
                                DRDL02 = CastDbNull.To(reader["DRDL02"], ""),
                                DRSPHD = CastDbNull.To(reader["DRSPHD"], ""),
                                DRUPMJ = CastDbNull.To<decimal>(reader["DRUPMJ"], 0),
                                DRUPMT = CastDbNull.To<decimal>(reader["DRUPMT"], 0)
                            };
                            f0005s.Add(f0005);
                        }

                        reader.Close();
                    }

                    Logger.Trace($@"ROWS: {f0005s.Count}");
                }
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
            return f0005s;
        }

        private static iDB2Command GetF0005sByDRSPHD()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        DRSY,
                        DRRT,
                        DTDL01,
                        DRKY,
                        DRDL01,
                        DRDL02,
                        DRSPHD,
                        DRUPMJ,
                        DRUPMT
                    FROM
                        {ConfigurationManager.AppSettings["ControlLib"]}.F0005 
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0004
                        ON DRSY = DTSY AND
                           DRRT = DTRT
                    WHERE 
                        DRSY = ? AND
                        DRRT = ? AND
                        UPPER(TRIM(DRSPHD)) = ?"
            };
            cmd.Parameters.Add(new iDB2Parameter("DRSY", iDB2DbType.iDB2Graphic, 4));
            cmd.Parameters.Add(new iDB2Parameter("DRRT", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("DRSPHD", iDB2DbType.iDB2Graphic, 10));
            return cmd;
        }
    }
}
