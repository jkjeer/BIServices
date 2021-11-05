using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Archie;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    public class BackupTableManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse Process(SqlConnection cnCorpInt, SqlConnection cnArchCfg, SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Truncate Ledger Table: {cnTarget.DataSource}.{cnTarget.Database}.{targetTable.SchemaName}.{targetTable.TableName}", true);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) response.ProcessName = $"{response.ProcessName} - Test Mode No Delete";
            response.ServerName = targetTable.ServerName;
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnTarget", cnTarget.ConnectionString));
            try
            {
                var step1ValidateTable = new ProcessStep($@"Step 1. Validate Table ({cnTarget.DataSource}.{cnTarget.Database}.{targetTable.SchemaName}.{targetTable.TableName})");
                try
                {
                    targetTable.ValidationErrors = Validate(cnTarget, targetTable);
                    step1ValidateTable.RowsAffected = targetTable.RowsAffected;
                    response.RowsAffected = step1ValidateTable.RowsAffected;
                }
                catch (Exception e)
                {
                    step1ValidateTable.Errors.Add(new Error(e));
                }
                step1ValidateTable.Done();
                response.ProcessSteps.Add(step1ValidateTable);

                if (!targetTable.ValidationErrors.Any())
                {
                    var step2TruncateTable = new ProcessStep($@"Step 2. Truncate Table ({cnTarget.DataSource}.{cnTarget.Database}.{targetTable.SchemaName}.{targetTable.TableName})");
                    try
                    {
                        var rowCount = BackupTable(cnTarget, targetTable);
                        step2TruncateTable.RowsAffected = rowCount;
                    }
                    catch (Exception e)
                    {
                        step2TruncateTable.Errors.Add(new Error(e));
                    }
                    step2TruncateTable.Done();
                    response.ProcessSteps.Add(step2TruncateTable);
                }
                else
                {
                    foreach (var validationError in targetTable.ValidationErrors)
                    {
                        response.Errors.Add(new Error(validationError.Message));
                    }
                    targetTable.TruncateStatus = "E";
                    targetTable.TruncateMessage = targetTable.ValidationStatus;
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
            return response;
        }

        private static ValidationErrors Validate(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var validationErrors = new ValidationErrors();
            try
            {
                if (!targetTable.TableName.EndsWith("LEDGER", StringComparison.CurrentCultureIgnoreCase))
                {
                    var validationError = new ValidationError($@"Table {targetTable.ServerName}.{targetTable.DatabaseName}.{targetTable.SchemaName}.{targetTable.TableName} is not like *LEDGER", "Validation", "Error", true);
                    validationErrors.Add(validationError);
                    targetTable.ValidationStatus = validationError.Message;
                }
                else if (!targetTable.DatabaseName.Equals("INTEGRATION", StringComparison.CurrentCultureIgnoreCase))
                {
                    var validationError = new ValidationError($@"Table {targetTable.ServerName}.{targetTable.DatabaseName}.{targetTable.SchemaName}.{targetTable.TableName} is not in the INTEGRATION database", "Validation", "Error", true);
                    validationErrors.Add(validationError);
                    targetTable.ValidationStatus = validationError.Message;
                }
                else
                {
                    var isDatabaseExist = TableManager.CheckDatabaseExists(cnTarget, targetTable);
                    if (!isDatabaseExist)
                    {
                        var validationError = new ValidationError($@"Database {targetTable.ServerName}.{targetTable.DatabaseName} not found", "Validation", "Error", true);
                        validationErrors.Add(validationError);
                        targetTable.ValidationStatus = validationError.Message;
                    }
                    var isSchemaExist = TableManager.CheckSchemaExists(cnTarget, targetTable);
                    if (!isSchemaExist)
                    {
                        var validationError = new ValidationError($@"Schema {targetTable.ServerName}.{targetTable.DatabaseName}.{targetTable.SchemaName} not found", "Validation", "Error", true);
                        validationErrors.Add(validationError);
                        targetTable.ValidationStatus = validationError.Message;
                    }
                    var isTableExist = TableManager.CheckTableExists(cnTarget, targetTable);
                    if (!isTableExist)
                    {
                        var validationError = new ValidationError($@"Table {targetTable.ServerName}.{targetTable.DatabaseName}.{targetTable.SchemaName}.{targetTable.TableName} not found", "Validation", "Error", true);
                        validationErrors.Add(validationError);
                        targetTable.ValidationStatus = validationError.Message;
                    }
                    //else
                    //{
                    //    targetTable.RowsAffected = TableManager.CountTableRows(cnTarget, targetTable);
                    //    if (targetTable.RowsAffected == 0)
                    //    {
                    //        var validationError = new ValidationError($@"Table {targetTable.ServerName}.{targetTable.DatabaseName}.{targetTable.SchemaName}.{targetTable.TableName} is empty", "Validation", "Error", true);
                    //        validationErrors.Add(validationError);
                    //        targetTable.ValidationStatus = validationError.Message;
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Logger.Error($@"ERROR: {cnTarget.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($@"{cnTarget.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            finally
            {
                if (!validationErrors.Any()) targetTable.ValidationStatus = "Valid";
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return validationErrors;
        }

        public static BackupTableConfig GetBackupTableConfig(SqlConnection cnArchCfg, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var config = new BackupTableConfig();
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();
            var rowCount = 0;
            try
            {
                filters.Add("[ServerName] = @serverName AND\r\n");
                parameters.Add(new SqlParameter("@serverName", SqlDbType.VarChar));
                parameters.Last().Value = targetTable.ServerName;
                filters.Add("[DatabaseName] = @databaseName AND\r\n");
                parameters.Add(new SqlParameter("@databaseName", SqlDbType.VarChar));
                parameters.Last().Value = targetTable.DatabaseName;
                filters.Add("[SchemaName] = @schemaName AND\r\n");
                parameters.Add(new SqlParameter("@schemaName", SqlDbType.VarChar));
                parameters.Last().Value = targetTable.SchemaName;
                filters.Add("[TableName] = @tableName");
                parameters.Add(new SqlParameter("@tableName", SqlDbType.VarChar));
                parameters.Last().Value = targetTable.TableName;
                var cmd = GetBackupTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], string.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], string.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], string.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], string.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastBackupDate = CastDbNull.To(reader["LastBackupDate"], DateTime.MinValue);
                        config.BackupSF = CastDbNull.To(reader["BackupSF"], false);
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.BackupFrequencyDays = CastDbNull.To(reader["BackupFrequencyDays"], 0);
                        config.DeletedRows = CastDbNull.To<long>(reader["DeletedRows"], 0);
                        config.Plant = PlantManager.GetPlant(CastDbNull.To(reader["PlantCode"], string.Empty).Trim());
                        rowCount += 1;
                    }
                    reader.Close();
                }
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new Exception($"{cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return config;
        }

        private static SqlCommand GetBackupTableConfig(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters) filterString.Append(filter);
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    SELECT [ID]
                          ,[PlantCode]
                          ,[ServerName]
                          ,[DatabaseName]
                          ,[SchemaName]
                          ,[TableName]
                          ,[IsActive]
                          ,[IsEnabled]
                          ,[LastRunStatus]
                          ,[HostName]
                          ,[LastBackupDate]
                          ,[BackupSF]
                          ,[DateColumn]
                          ,[RetentionDays]
                          ,[BackupFrequencyDays]
                          ,[NextBackupDate]
                          ,[DeletedRows]
                          ,[execution_instance_guid]
                          ,[BITimeStamp]
                          ,[J_BU_Plant_srccd]
                    FROM [dbo].[cfgTablesBackup]
                    WHERE
                     {filterString} 
                ";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return cmd;
        }

        public static BackupTableConfigs GetBackupTableConfigs(string plantCode, string serverName, SqlConnection cnArchCfg)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var configs = new BackupTableConfigs();
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();
            try
            {
                filters.Add("[PlantCode] = @plantCode AND\r\n");
                parameters.Add(new SqlParameter("@plantCode", SqlDbType.VarChar));
                parameters.Last().Value = plantCode;
                filters.Add("[ServerName] = @serverName");
                parameters.Add(new SqlParameter("@serverName", SqlDbType.VarChar));
                parameters.Last().Value = serverName;
                var cmd = GetBackupTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var config = new BackupTableConfig();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], string.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], string.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], string.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], string.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastBackupDate = CastDbNull.To(reader["LastBackupDate"], DateTime.MinValue);
                        config.BackupSF = CastDbNull.To(reader["BackupSF"], false);
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.BackupFrequencyDays = CastDbNull.To(reader["BackupFrequencyDays"], 0);
                        config.DeletedRows = CastDbNull.To<long>(reader["DeletedRows"], 0);
                        config.Plant = PlantManager.GetPlant(CastDbNull.To(reader["PlantCode"], string.Empty).Trim());
                        configs.Add(config);
                    }
                    reader.Close();
                }
                Logger.Trace($@"ROWS: {configs.Count}");
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new Exception($"{cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return configs;
        }

        private static int UpdateBackupTableConfig(SqlConnection cnArchCfg, BackupTableConfig backupTableConfigFinal)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            int rowCount;
            try
            {
                var serviceContext = new ServiceContext();
                backupTableConfigFinal.HostName = serviceContext.ServerAddress;
                var cmd = UpdateBackupTableConfig(backupTableConfigFinal);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new Exception($"{cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        private static SqlCommand UpdateBackupTableConfig(BackupTableConfig backupTableConfigFinal)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    UPDATE xx
                    SET 
                        [LastRunStatus] = {backupTableConfigFinal.LastRunStatus}
                        ,[HostName] = '{backupTableConfigFinal.HostName}'
                        ,[LastBackupDate] = '{backupTableConfigFinal.LastBackupDate}'
                        ,[NextBackupDate] = '{backupTableConfigFinal.NextBackupDate}'
                        ,[DeletedRows] = {backupTableConfigFinal.DeletedRows}
                        ,[execution_instance_guid] = '{backupTableConfigFinal.Guid}'
                        ,[BITimeStamp] = GETDATE()
                    FROM [dbo].[cfgTablesBackup] xx
                    WHERE [ID] = {backupTableConfigFinal.ID}
                ";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return cmd;
        }

        public static long GetRowCount(SqlConnection cnTarget, Table targetTable, BackupTableConfig backupTableConfig)
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
                        SELECT COUNT(*) 
                        FROM [{targetTable.SchemaName}].[{targetTable.TableName}]
                        WHERE [{backupTableConfig.DateColumn}] < GETDATE() - {backupTableConfig.RetentionDays}"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = (long)cmd.ExecuteScalar();
                Logger.Trace("ROWS: " + rowCount);
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

        public static long BackupTable(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            long rowCount = 0;
            var testModeNoDelete = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]);
            try
            {
                if (!testModeNoDelete)
                {
                    //var cmd = new SqlCommand
                    //{
                    //    Connection = cnTarget,
                    //    CommandType = CommandType.Text,
                    //    CommandText = $@"TRUNCATE TABLE [{targetTable.SchemaName}].[{targetTable.TableName}]"
                    //};
                    //Logger.Trace(Logging.DbCmdString(cmd));
                    //rowCount = cmd.ExecuteNonQuery();
                    //Logger.Trace("ROWS: " + rowCount);
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
            return rowCount;
        }

    }
}