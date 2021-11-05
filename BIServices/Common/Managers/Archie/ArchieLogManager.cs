using System;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Types.Archie;
using BIServices.Common.Utilities;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    public class ArchieLogManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static long LogDetail(SqlConnection cnArchCfg, ArchieLogDetail archieLog)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            long rowCount;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnArchCfg,
                    CommandType = CommandType.Text,
                    CommandText = $@"INSERT INTO [Archiver].[dbo].[LoggingDetail]
                        (
                            [event_type]
                            ,[task_name]
                            ,[command]
                              ,[delete_rows]
                              ,[test_mode]
                            ,[written_date]
                            ,[Plant]
                            ,[ServerName]
                            ,[DatabaseName]
                            ,[SchemaName]
                            ,[TableName]
                            ,[execution_instance_guid]
                        )
	                    VALUES 
	                    (
                            '{archieLog.EventType}'
                            ,'{archieLog.TaskName}'
                            ,'{archieLog.Command}'
                            ,{archieLog.DeleteRows}
                            ,{(archieLog.TestMode ? 1 : 0)}
                            ,'{archieLog.WrittenDate}'
                            ,'{archieLog.PlantCode}'
                            ,'{archieLog.ServerName}'
                            ,'{archieLog.DatabaseName}'
                            ,'{archieLog.SchemaName}'
                            ,'{archieLog.TableName}'
                            ,'{archieLog.Guid}'
                        )"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnArchCfg.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnArchCfg.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return rowCount;
        }
        public static long LogActivity(SqlConnection cnArchCfg, ArchieLog archieLog)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            long rowCount;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnArchCfg,
                    CommandType = CommandType.Text,
                    CommandText = $@"INSERT INTO [Archiver].[dbo].[Logging]
                        (
                            [event_type]
                            ,[service_name]
                            ,[task_name]
                            ,[version]
                            ,[user_name]
                            ,[comments]
                            ,[command]
                            ,[date_column]
                            ,[retention_days]
                            ,[initial_rowcount]
                            ,[delete_rows]
                            ,[final_rowcount]
                            ,[test_mode]
                            ,[written_date]
                            ,[GroupName]
                            ,[Plant]
                            ,[ServerName]
                            ,[DatabaseName]
                            ,[SchemaName]
                            ,[TableName]
                            ,[execution_instance_guid]
                        )
	                    VALUES 
	                    (
                            '{archieLog.EventType}'
                            ,'{archieLog.HostName}'
                            ,'{archieLog.TaskName}'
                            ,'{archieLog.Version}'
                            ,'{archieLog.UserName}'
                            ,'{archieLog.Comments}'
                            ,'{archieLog.Command}'
                            ,'{archieLog.DateColumn}'
                            ,{archieLog.RetentionDays}
                            ,{archieLog.InitialRowCount}
                            ,{archieLog.DeleteRows}
                            ,{archieLog.FinalRowCount}
                            ,{(archieLog.TestMode ? 1 : 0)}
                            ,'{archieLog.WrittenDate}'
                            ,'{archieLog.GroupName}'
                            ,'{archieLog.PlantCode}'
                            ,'{archieLog.ServerName}'
                            ,'{archieLog.DatabaseName}'
                            ,'{archieLog.SchemaName}'
                            ,'{archieLog.TableName}'
                            ,'{archieLog.Guid}'
                        )"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnArchCfg.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnArchCfg.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return rowCount;
        }

    }
}