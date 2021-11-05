using System;
using System.Data;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.E1
{
    public class F580200Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static iDB2Command GetMaxE1PYF580200BIUKID()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = @"SELECT MAX(BIUKID) MaxBIUKID FROM F580200"
            };
            return cmd;
        }

        public static decimal GetMaxE1PYF580200BIUKID(BusinessSystemSession e1Session)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            decimal maxE1PYBIUKID = 0;
            try
            {
                var cmd = GetMaxE1PYF580200BIUKID();
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        maxE1PYBIUKID = CastDbNull.To<decimal>(reader["MaxBIUKID"], 0);
                    }

                    reader.Close();
                }

                Logger.Trace("ROWS: 1");
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
            return maxE1PYBIUKID;
        }

        private static iDB2Command GetMaxE1PYF580200BIEDBT()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = @"SELECT MAX(BIEDBT) MaxBIEDBT FROM F580200"
            };
            return cmd;
        }

        public static decimal GetMaxE1PYF580200BIEDBT(BusinessSystemSession e1Session)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            decimal maxE1PYBIEDBT = 0;
            var maxE1PYBIEDBTstr = "0";
            try
            {
                var cmd = GetMaxE1PYF580200BIEDBT();
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        maxE1PYBIEDBTstr = CastDbNull.To<string>(reader["MaxBIEDBT"], "0");
                    }

                    reader.Close();
                }

                maxE1PYBIEDBT = Convert.ToDecimal(maxE1PYBIEDBTstr);
                Logger.Trace("ROWS: 1");
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
            return maxE1PYBIEDBT;
        }
    }
}
