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
    public class F580030Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static iDB2Command GetMaxE1PYF580030SFUKID()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = @"SELECT MAX(SFUKID) MaxSFUKID FROM F580030"
            };
            return cmd;
        }

        public static decimal GetMaxE1PYF580030SFUKID(BusinessSystemSession e1Session)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            decimal maxE1PYSFUKID = 0;
            try
            {
                var cmd = GetMaxE1PYF580030SFUKID();
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        maxE1PYSFUKID = CastDbNull.To<decimal>(reader["MaxSFUKID"], 0);
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
            return maxE1PYSFUKID;
        }

        private static iDB2Command GetMaxE1PYF580030SFEDBT()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = @"SELECT MAX(SFEDBT) MaxSFEDBT FROM F580030"
            };
            return cmd;
        }

        public static decimal GetMaxE1PYF580030SFEDBT(BusinessSystemSession e1Session)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            decimal maxE1PYSFEDBT = 0;
            var maxE1PYSFEDBTstr = "0";
            try
            {
                var cmd = GetMaxE1PYF580030SFEDBT();
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        maxE1PYSFEDBTstr = CastDbNull.To<string>(reader["MaxSFEDBT"], "0");
                    }

                    reader.Close();
                }
                maxE1PYSFEDBT = Convert.ToDecimal(maxE1PYSFEDBTstr);
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
            return maxE1PYSFEDBT;
        }
    }
}
