using System;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Types.Archie;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    public class TableManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static bool CheckServerExists(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var isExist = false;
            var rowCount = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"
                       SELECT [name]
                        FROM  sys.databases
                        WHERE [name] = '{targetTable.DatabaseName}' 
                        "
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        isExist = true;
                        rowCount += 1;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + rowCount);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return isExist;
        }

        public static bool CheckDatabaseExists(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var isExist = false;
            var rowCount = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"
                       SELECT [name]
                        FROM  sys.databases
                        WHERE [name] = '{targetTable.DatabaseName}' 
                        "
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        isExist = true;
                        rowCount += 1;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + rowCount);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return isExist;
        }

        public static bool CheckSchemaExists(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var isExist = false;
            var rowCount = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"
                       SELECT [TABLE_SCHEMA]
                        FROM  [{targetTable.DatabaseName}].[INFORMATION_SCHEMA].[TABLES]
                        WHERE [TABLE_CATALOG] = '{targetTable.DatabaseName}' AND
                              [TABLE_SCHEMA] = '{targetTable.SchemaName}' 
                        "
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        isExist = true;
                        rowCount += 1;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + rowCount);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return isExist;
        }

        public static bool CheckTableExists(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var isExist = false;
            var rowCount = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"
                       SELECT [TABLE_NAME]
                        FROM  [{targetTable.DatabaseName}].[INFORMATION_SCHEMA].[TABLES]
                        WHERE [TABLE_CATALOG] = '{targetTable.DatabaseName}' AND
                              [TABLE_SCHEMA] = '{targetTable.SchemaName}' AND 
                              [TABLE_NAME] = '{targetTable.TableName}' 
                        "
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        isExist = true;
                        rowCount += 1;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + rowCount);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return isExist;
        }

        public static long CountTableRows(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            long rowCount;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"
                        SELECT  COUNT(*)
                        FROM    [{targetTable.SchemaName}].[{targetTable.TableName}]"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = (long)cmd.ExecuteScalar();
                Logger.Trace($@"ROWS: {rowCount}");
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }
        public static bool CheckTriggerExists(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var isExist = false;
            var rowCount = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"
                       select *
                        from sys.triggers
                        where object_id = object_id(N'[Ledger].[Trig_Insert_{targetTable.TableName}]')"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        isExist = true;
                        rowCount += 1;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + rowCount);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTarget.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }

            return isExist;
        }

    }
}
