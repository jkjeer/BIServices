using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class BusinessSystemSessionManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static BusinessSystemSession GetBusinessSessionPY(ConnectionString connectionString = null)
        {
            var e1Session = new BusinessSystemSession();
            try
            {
                var dbConnectionString = ConnectionManager.GetDb2ConnectionStringPY();
                e1Session.DbConnection = ConnectionManager.GetDb2Connection(dbConnectionString);
                e1Session.DbCommands = new Dictionary<string, iDB2Command>();

                var cache = MemoryCache.Default;
                if (cache.Contains("PlantMap"))
                {
                    e1Session.PlantMap = (PlantMap)cache.Get("PlantMap");
                }
                else
                {
                    var map = new PlantMap();
                    var cacheItemPolicy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddHours(24)
                    };
                    cache.Add("PlantMap", map, cacheItemPolicy);
                    e1Session.PlantMap = map;
                }
                e1Session.DbConnection.Open();
                e1Session.IsOpen = true;
                Logger.Info(e1Session.DbConnection.ConnectionString + " JobName: " + e1Session.DbConnection.JobName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + ": " + e.Message + ": " + e.StackTrace);
            }
            return e1Session;
        }

        public static BusinessSystemSession GetBusinessSession(ConnectionString connectionString = null)
        {
            var e1Session = new BusinessSystemSession();
            try
            {
                var dbConnectionString = ConnectionManager.GetDb2ConnectionString();
                e1Session.DbConnection = ConnectionManager.GetDb2Connection(dbConnectionString);
                e1Session.DbCommands = new Dictionary<string, iDB2Command>();

                var cache = MemoryCache.Default;
                if (cache.Contains("PlantMap"))
                {
                    e1Session.PlantMap = (PlantMap)cache.Get("PlantMap");
                }
                else
                {
                    var map = new PlantMap();
                    var cacheItemPolicy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddHours(24)
                    };
                    cache.Add("PlantMap", map, cacheItemPolicy);
                    e1Session.PlantMap = map;
                }
                e1Session.DbConnection.Open();
                e1Session.IsOpen = true;
                Logger.Info(e1Session.DbConnection.ConnectionString + " JobName: " + e1Session.DbConnection.JobName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + ": " + e.Message + ": " + e.StackTrace);
            }
            return e1Session;
        }

        public void CloseBusinessSession(BusinessSystemSession e1Session)
        {
            try
            {
                if (e1Session.IsOpen)
                {
                    e1Session.DbConnection.Close();
                    e1Session.IsOpen = false;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
        }

        //public void Prepare()
        //{
        //    foreach (var command in DbCommands.Values)
        //    {
        //        command.Connection = DbConnection;
        //        command.Prepare();
        //    }
        //}

        /**********************************************************************
         *  Current Time Stamp
         *********************************************************************/
        //        public iDB2Command GetCurrentTimestampStmt()
        //        {
        //            var cmd = new iDB2Command
        //            {
        //                CommandType = CommandType.Text,
        //                CommandText = @"
        //            SELECT
        //              CURRENT TIMESTAMP AS TS
        //            FROM
        //              SYSIBM.SYSDUMMY1"
        //            };
        //            return cmd;
        //        }

        //public DateTime GetCurrentTimestamp()
        //{
        //    var dt = new DateTime();
        //    try
        //    {
        //        var cmd = GetCurrentTimestampStmt();
        //        cmd.Connection = DbConnection;
        //        using( IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
        //        while (reader.Read())
        //        {
        //            try
        //            {
        //                dt = Utilities.CastDbNull.To(reader["TS"], new DateTime(1900, 1, 1));
        //            }
        //            catch (Exception e)
        //            {
        //                throw new ApplicationException(e.ToString());
        //            }
        //        }
        //        reader.Close();
        //    }
        //    catch (Exception err)
        //    {
        //        throw new ApplicationException(err.ToString());
        //    }
        //    return dt;
        //}
    }
}
