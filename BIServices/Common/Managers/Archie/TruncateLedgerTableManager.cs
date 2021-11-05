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
    public class TruncateLedgerTableManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse Process(SqlConnection cnCorpInt, SqlConnection cnArchCfg, SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Truncate Ledger Table: {targetTable.QualifiedName}";
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) taskName = $@"{taskName} - Test Mode No Delete";
            Logger.Info($"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($@"{taskName}", true);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) response.ProcessName = $"{response.ProcessName} - Test Mode No Delete";
            //if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) response.Messages.Add(new Message("msg", "Test Mode No Delete"));
            response.ServerName = targetTable.ServerName;
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnTarget", cnTarget.ConnectionString));
            var truncateTableConfigInit = new TruncateTableConfig();
            var truncateTableConfigFinal = new TruncateTableConfig();
            var guid = Guid.NewGuid();
            try
            {
                var step1ValidateTable = new ProcessStep($@"Step 1. Validate Table {targetTable.QualifiedName}");
                try
                {
                    truncateTableConfigInit = GetTruncateTableConfig(cnArchCfg, targetTable);
                    targetTable.ValidationErrors = Validate(cnArchCfg,cnTarget, targetTable);
                    step1ValidateTable.RowsAffected = targetTable.RowsAffected;
                    response.RowsAffected = step1ValidateTable.RowsAffected;
                }
                catch (Exception e)
                {
                    step1ValidateTable.Errors.Add(new Error(e));
                }
                step1ValidateTable.Done();
                response.ProcessSteps.Add(step1ValidateTable);

                if (targetTable.ValidationErrors.Any())
                {
                    foreach (var validationError in targetTable.ValidationErrors)
                    {
                        response.Errors.Add(new Error(validationError.Message));
                    }
                    targetTable.TruncateStatus = "E";
                    targetTable.TruncateMessage = targetTable.ValidationStatus;
                    ArchieLogManager.LogActivity(cnArchCfg, new ArchieLog("Error", "TruncateLedgerTable",
                        targetTable.TruncateMessage, "", "", 0, truncateTableConfigInit.InitRowCount, step1ValidateTable.RowsAffected, 0,
                        Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]),
                        truncateTableConfigInit.GroupName, truncateTableConfigInit.PlantCode, truncateTableConfigInit.ServerName, truncateTableConfigInit.DatabaseName, truncateTableConfigInit.SchemaName,
                        truncateTableConfigInit.TableName, guid));
                }
                else
                {
                    var step2GatherStatistics = new ProcessStep($@"Step 2. Gather statistics for {targetTable.QualifiedName}");
                    try
                    {
                        truncateTableConfigInit.InitRowCount = GetRowCount(cnTarget, targetTable);
                        truncateTableConfigFinal = truncateTableConfigInit;
                        truncateTableConfigFinal.Plant = truncateTableConfigInit.Plant;
                        truncateTableConfigFinal.PlantCode = truncateTableConfigInit.PlantCode;
                        truncateTableConfigFinal.ServerName = truncateTableConfigInit.ServerName;
                        truncateTableConfigFinal.DatabaseName = truncateTableConfigInit.DatabaseName;
                        truncateTableConfigFinal.SchemaName = truncateTableConfigInit.SchemaName;
                        truncateTableConfigFinal.TableName = truncateTableConfigInit.TableName;
                        truncateTableConfigFinal.Guid = guid;
                        truncateTableConfigFinal.LastRunStatus = 0;
                        step2GatherStatistics.RowsAffected = truncateTableConfigInit.InitRowCount;
                    }
                    catch (Exception e)
                    {
                        step2GatherStatistics.Errors.Add(new Error(e));
                    }
                    step2GatherStatistics.Done();
                    response.ProcessSteps.Add(step2GatherStatistics);
                    if (!step2GatherStatistics.Errors.Any())
                    {
                        var step3TruncateTable = new ProcessStep($@"Step 3. Truncate Table {targetTable.QualifiedName}");
                        try
                        {
                            var rowCount = TruncateTable(cnTarget, targetTable);
                            step3TruncateTable.RowsAffected = (rowCount == 0 ) ? 0 : truncateTableConfigFinal.InitRowCount;
                            response.RowsAffected = step3TruncateTable.RowsAffected;
                            truncateTableConfigFinal.TruncatedRows = step3TruncateTable.RowsAffected;
                            truncateTableConfigFinal.LastRunStatus = 1;
                            truncateTableConfigFinal.LastTruncateDate = DateTime.Now;
                            truncateTableConfigFinal.NextTruncateDate = DateTime.Now.AddDays(truncateTableConfigFinal.TruncateFrequencyDays).AddHours(-1);
                            ArchieLogManager.LogActivity(cnArchCfg,new ArchieLog("Event", "TruncateLedgerTable",
                                taskName, "", "", 0, truncateTableConfigFinal.InitRowCount, step3TruncateTable.RowsAffected, 0,
                                Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]),
                                truncateTableConfigFinal.GroupName, truncateTableConfigFinal.PlantCode, truncateTableConfigFinal.ServerName, truncateTableConfigFinal.DatabaseName, truncateTableConfigFinal.SchemaName, 
                                truncateTableConfigFinal.TableName,truncateTableConfigFinal.Guid));
                        }
                        catch (Exception e)
                        {
                            step3TruncateTable.Errors.Add(new Error(e));
                        }

                        step3TruncateTable.Done();
                        response.ProcessSteps.Add(step3TruncateTable);
                        if (!step3TruncateTable.Errors.Any())
                        {
                            var step4UpdateConfigTable = new ProcessStep($@"Step 4. Update Config statistics in [{cnArchCfg.DataSource}].[Archiver].[dbo].[cfgTablesTruncate]");
                            try
                            {
                                var rowCount = UpdateTruncateTableConfig(cnArchCfg, truncateTableConfigFinal);
                                step4UpdateConfigTable.RowsAffected = rowCount;
                            }
                            catch (Exception e)
                            {
                                step4UpdateConfigTable.Errors.Add(new Error(e));
                            }

                            step4UpdateConfigTable.Done();
                            response.ProcessSteps.Add(step4UpdateConfigTable);
                        }
                    }
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

        private static ValidationErrors Validate(SqlConnection cnArchCfg, SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var validationErrors = new ValidationErrors();
            try
            {
                if (!targetTable.TableName.EndsWith("LEDGER", StringComparison.CurrentCultureIgnoreCase))
                {
                    var validationError = new ValidationError($@"Table {targetTable.QualifiedName} is not like *LEDGER", "Validation", "Error", true);
                    validationErrors.Add(validationError);
                    targetTable.ValidationStatus = validationError.Message;
                }
                else if (!targetTable.DatabaseName.Equals("INTEGRATION", StringComparison.CurrentCultureIgnoreCase))
                {
                    var validationError = new ValidationError($@"Table {targetTable.QualifiedName} is not in the INTEGRATION database", "Validation", "Error", true);
                    validationErrors.Add(validationError);
                    targetTable.ValidationStatus = validationError.Message;
                }
                else
                {
                    var tableConfig = GetTruncateTableConfig(cnArchCfg, targetTable);
                    if (tableConfig.TableName == null)
                    {
                        var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for Truncation.", "Validation", "Fatal", true);
                        validationErrors.Add(validationError);
                        targetTable.ValidationStatus = validationError.Message;
                    }
                    else
                    { 
                        if (!tableConfig.IsActive)
                        {
                            var validationError = new ValidationError($"{targetTable.QualifiedName} is Inactive.", "Validation", "Fatal", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        if (!tableConfig.IsEnabled)
                        {
                            var validationError = new ValidationError($"{targetTable.QualifiedName} is Disabled.", "Validation", "Fatal", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        if (!tableConfig.TruncateSF)
                        {
                            var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for Truncation.", "Validation", "Fatal", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }

                        //var isServerExist = TableManager.CheckServerExists(cnTarget, targetTable);
                        //if (!isServerExist)
                        //{
                        //    var validationError = new ValidationError($@"Server [{tableConfig.ServerName}] not found", "Validation", "Error", true);
                        //    validationErrors.Add(validationError);
                        //    targetTable.ValidationStatus = validationError.Message;
                        //}
                        var isDatabaseExist = TableManager.CheckDatabaseExists(cnTarget, targetTable);
                        if (!isDatabaseExist)
                        {
                            var validationError = new ValidationError($@"Database [{tableConfig.ServerName}].[{tableConfig.DatabaseName}] not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        var isSchemaExist = TableManager.CheckSchemaExists(cnTarget, targetTable);
                        if (!isSchemaExist)
                        {
                            var validationError = new ValidationError($@"Schema [{tableConfig.ServerName}].[{tableConfig.DatabaseName}].[{tableConfig.SchemaName}] not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        var isTableExist = TableManager.CheckTableExists(cnTarget, targetTable);
                        if (!isTableExist)
                        {
                            var validationError = new ValidationError($@"Table {targetTable.QualifiedName} not found", "Validation", "Error", true);
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

        public static TruncateTableConfig GetTruncateTableConfig(SqlConnection cnArchCfg, Table targetTable)
        {
            var config = new TruncateTableConfig();
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
                var cmd = GetTruncateTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        config.GroupName = CastDbNull.To(reader["GroupName"], String.Empty).Trim();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], String.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], String.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], String.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], String.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], String.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastTruncateDate = CastDbNull.To(reader["LastTruncateDate"], DateTime.MinValue);
                        config.TruncateSF = CastDbNull.To(reader["TruncateSF"], false);
                        config.TruncateFrequencyDays = CastDbNull.To(reader["TruncateFrequencyDays"], 0);
                        config.TruncatedRows = CastDbNull.To<long>(reader["TruncatedRows"], 0);
                        config.Plant = PlantManager.GetPlant(CastDbNull.To(reader["PlantCode"], String.Empty).Trim());
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
            return config;
        }

        private static SqlCommand GetTruncateTableConfig(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters) filterString.Append(filter);
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    SELECT [ID]
                            ,[GroupName]
                          ,[PlantCode]
                          ,[ServerName]
                          ,[DatabaseName]
                          ,[SchemaName]
                          ,[TableName]
                          ,[IsActive]
                          ,[IsEnabled]
                          ,[LastRunStatus]
                          ,[HostName]
                          ,[LastTruncateDate]
                          ,[TruncateSF]
                          ,[TruncateFrequencyDays]
                          ,[NextTruncateDate]
                          ,[TruncatedRows]
                          ,[execution_instance_guid]
                          ,[BITimeStamp]
                          ,[J_BU_Plant_srccd]
                    FROM [dbo].[cfgTablesTruncate]
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

        public static TruncateTableConfigs GetTargetTableConfigs(string plantCode, string serverName, SqlConnection cnArchCfg)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var configs = new TruncateTableConfigs();
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
                var cmd = GetTruncateTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var config = new TruncateTableConfig();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], String.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], String.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], String.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], String.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], String.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastTruncateDate = CastDbNull.To(reader["LastTruncateDate"], DateTime.MinValue);
                        config.TruncateSF = CastDbNull.To(reader["TruncateSF"], false);
                        config.TruncateFrequencyDays = CastDbNull.To(reader["TruncateFrequencyDays"], 0);
                        config.TruncatedRows = CastDbNull.To<long>(reader["TruncatedRows"], 0);
                        config.Plant = PlantManager.GetPlant(CastDbNull.To(reader["PlantCode"], String.Empty).Trim());
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


        private static int UpdateTruncateTableConfig(SqlConnection cnArchCfg, TruncateTableConfig truncateTableConfigFinal)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            int rowCount;
            try
            {
                var serviceContext = new ServiceContext();
                truncateTableConfigFinal.HostName = serviceContext.ServerAddress;
                var cmd = UpdateTruncateTableConfig(truncateTableConfigFinal);
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

        private static SqlCommand UpdateTruncateTableConfig(TruncateTableConfig truncateTableConfigFinal)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    UPDATE xx
                    SET 
                        [LastRunStatus] = {truncateTableConfigFinal.LastRunStatus}
                        ,[HostName] = '{truncateTableConfigFinal.HostName}'
                        ,[LastTruncateDate] = '{truncateTableConfigFinal.LastTruncateDate}'
                        ,[NextTruncateDate] = '{truncateTableConfigFinal.NextTruncateDate}'
                        ,[TruncatedRows] = {truncateTableConfigFinal.TruncatedRows}
                        ,[execution_instance_guid] = '{truncateTableConfigFinal.Guid}'
                        ,[BITimeStamp] = GETDATE()
                    FROM [dbo].[cfgTablesTruncate] xx
                    WHERE [ID] = {truncateTableConfigFinal.ID}
                ";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return cmd;
        }

        public static long GetRowCount(SqlConnection cnTarget, Table targetTable)
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
                    CommandText = $@"SELECT COUNT(*) FROM [{targetTable.SchemaName}].[{targetTable.TableName}]"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = (int)cmd.ExecuteScalar();
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

        public static long TruncateTable(SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            long rowCount = 0;
            var testModeNoDelete = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]);
            try
            {
                if (!testModeNoDelete)
                {
                    var cmd = new SqlCommand
                    {
                        Connection = cnTarget,
                        CommandType = CommandType.Text,
                        CommandText = $@"TRUNCATE TABLE [{targetTable.SchemaName}].[{targetTable.TableName}]"
                    };
                    Logger.Trace(Logging.DbCmdString(cmd));
                    rowCount = cmd.ExecuteNonQuery();
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
            return rowCount;
        }
    }
}