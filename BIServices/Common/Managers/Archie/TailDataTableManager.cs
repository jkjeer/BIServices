using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Archie;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    public class TailDataTableManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); 

        public static ProcessResponse Process(SqlConnection cnCorpInt, SqlConnection cnArchCfg, SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Tail Data Table: {targetTable.QualifiedName}";
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) taskName = $@"{taskName} - Test Mode No Delete";
            Logger.Info($"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($@"{taskName}", true);
            response.ServerName = targetTable.ServerName;
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnTarget", cnTarget.ConnectionString));
            var tailTableConfigInit = new TailTableConfig();
            var tailTableConfigFinal = new TailTableConfig();
            var guid = Guid.NewGuid();
            try
            {
                var step1ValidateTable = new ProcessStep($@"Step 1. Validate Table {targetTable.QualifiedName}");
                try
                {
                    tailTableConfigInit = GetTailTableConfig(cnArchCfg, targetTable);
                    targetTable.ValidationErrors = Validate(cnArchCfg, cnTarget, targetTable);
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
                    targetTable.TailStatus = "E";
                    targetTable.TailMessage = targetTable.ValidationStatus;
                    ArchieLogManager.LogActivity(cnArchCfg, new ArchieLog("Error", "TailDataTable",targetTable.TailMessage,
                        "", tailTableConfigInit.DateColumn, tailTableConfigInit.RetentionDays, tailTableConfigInit.InitialRowCount, step1ValidateTable.RowsAffected, tailTableConfigInit.InitialRowCount - step1ValidateTable.RowsAffected,
                        Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]), 
                        tailTableConfigInit.GroupName, tailTableConfigInit.PlantCode, tailTableConfigInit.ServerName, tailTableConfigInit.DatabaseName, tailTableConfigInit.SchemaName,
                        tailTableConfigInit.TableName, guid));
                }
                else
                {
                    var step2GatherStatistics = new ProcessStep($@"Step 2. Gather statistics for {targetTable.QualifiedName}");
                    try
                    {
                        tailTableConfigInit.InitialRowCount = GetRowCount(cnTarget, targetTable);
                        tailTableConfigInit.RowsToDelete = GetRowCount(cnTarget, targetTable, $"WHERE [{ tailTableConfigInit.DateColumn}] < GETDATE() - { tailTableConfigInit.RetentionDays}");
                        tailTableConfigInit.FinalRowCount = tailTableConfigInit.InitialRowCount - tailTableConfigInit.RowsToDelete;
                        tailTableConfigFinal = tailTableConfigInit;
                        tailTableConfigFinal.Plant = tailTableConfigInit.Plant;
                        tailTableConfigFinal.GroupName = tailTableConfigInit.GroupName;
                        tailTableConfigFinal.PlantCode = tailTableConfigInit.PlantCode;
                        tailTableConfigFinal.ServerName = tailTableConfigInit.ServerName;
                        tailTableConfigFinal.DatabaseName = tailTableConfigInit.DatabaseName;
                        tailTableConfigFinal.SchemaName = tailTableConfigInit.SchemaName;
                        tailTableConfigFinal.TableName = tailTableConfigInit.TableName;
                        tailTableConfigFinal.Guid = guid;
                        tailTableConfigFinal.LastRunStatus = 0;
                        step2GatherStatistics.RowsAffected = tailTableConfigFinal.RowsToDelete;
                    }
                    catch (Exception e)
                    {
                        step2GatherStatistics.Errors.Add(new Error(e));
                    }
                    step2GatherStatistics.Done();
                    response.ProcessSteps.Add(step2GatherStatistics);
                    if (!step2GatherStatistics.Errors.Any() && tailTableConfigFinal.RowsToDelete > 0)
                    {
                        var step3TailTable = new ProcessStep($@"Step 3. Tail Table {targetTable.QualifiedName}");
                        var deleteSteps = new ProcessSteps();
                        try
                        {
                            deleteSteps = TailTable(cnTarget, targetTable, ref tailTableConfigFinal);
                            step3TailTable.RowsAffected = tailTableConfigFinal.DeletedRows;
                            response.RowsAffected = step3TailTable.RowsAffected;
                            tailTableConfigFinal.LastRunStatus = 1;
                            tailTableConfigFinal.LastTailDate = DateTime.Now;
                            tailTableConfigFinal.NextTailDate = DateTime.Now.AddDays(tailTableConfigFinal.TailFrequencyDays).AddHours(-1);
                            tailTableConfigFinal.FinalRowCount = GetRowCount(cnTarget, targetTable);
                            ArchieLogManager.LogActivity(cnArchCfg, new ArchieLog("Event", "TailLedgerTable",
                                taskName, tailTableConfigFinal.Command.Trim(), tailTableConfigFinal.DateColumn, tailTableConfigFinal.RetentionDays,
                                tailTableConfigFinal.InitialRowCount, step3TailTable.RowsAffected, tailTableConfigFinal.FinalRowCount,
                                Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]),
                               tailTableConfigInit.GroupName, tailTableConfigFinal.PlantCode, tailTableConfigFinal.ServerName, tailTableConfigFinal.DatabaseName, tailTableConfigFinal.SchemaName,
                                tailTableConfigFinal.TableName, tailTableConfigFinal.Guid));
                        }
                        catch (Exception e)
                        {
                            step3TailTable.Errors.Add(new Error(e));
                        }

                        step3TailTable.Done();
                        response.ProcessSteps.Add(step3TailTable);
                        response.ProcessSteps.AddRange(deleteSteps);
                        response.InitialRowCount = tailTableConfigFinal.InitialRowCount;
                        response.FinalRowCount = tailTableConfigFinal.FinalRowCount;
                        response.TestMode = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]) ? "True" : "False";
                        response.RetentionDays = tailTableConfigFinal.RetentionDays;
                        response.Command = tailTableConfigFinal.Command.Trim();
                        if (!step3TailTable.Errors.Any())
                        {
                            var step4UpdateConfigTable = new ProcessStep($@"Step 4. Update Config statistics in [{cnArchCfg.DataSource}].[Archiver].[dbo].[cfgTablesTail]");
                            try
                            {
                                var rowCount = UpdateTailTableConfig(cnArchCfg, tailTableConfigFinal);
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
                var tableConfig = GetTailTableConfig(cnArchCfg, targetTable);
                var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
                //if (requestedRetentionDays < server.LogFileRetentionDays)
                //{
                //    var validationError = new ValidationError($"{targetTable.QualifiedName} Requested Retention Days {requestedRetentionDays} is less than {targetTable.QualifiedName} configuration.", "Validation", "Fatal", true);
                //    validationErrors.Add(validationError);
                //    targetTable.ValidationStatus = validationError.Message;
                //}
                //if (requestedRetentionDays < minRetentionDays)
                //{
                //    var validationError = new ValidationError($"{targetTable.ServerName} Requested Retention Days {requestedRetentionDays} is less than Minimum Retention Days.", "Validation", "Fatal", true);
                //    validationErrors.Add(validationError);
                //    targetTable.ValidationStatus = validationError.Message;
                //}
                if (tableConfig.TableName == null)
                {
                    var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for Tailing.", "Validation", "Fatal", true);
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
                    if (!tableConfig.TailSF)
                    {
                        var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for Tailing.", "Validation", "Fatal", true);
                        validationErrors.Add(validationError);
                        targetTable.ValidationStatus = validationError.Message;
                    }
                    if (string.IsNullOrEmpty(tableConfig.DateColumn))
                    {
                        var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for DateColumn.", "Validation", "Fatal", true);
                        validationErrors.Add(validationError);
                        targetTable.ValidationStatus = validationError.Message;
                    }
                    if (tableConfig.RetentionDays < minRetentionDays)
                    {
                        var validationError = new ValidationError($"{targetTable.QualifiedName} configured RetentionDays is less than the minimum ({minRetentionDays}).", "Validation", "Fatal", true);
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

        public static TailTableConfig GetTailTableConfig(SqlConnection cnArchCfg, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var config = new TailTableConfig();
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
                var cmd = GetTailTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                //Logger.Trace(Logging.DbCmdString(cmd));
                Logger.Trace($@"Data Source={cmd.Connection.DataSource};Database={cmd.Connection.Database}");
                Logger.Trace(cmd.CommandText);
                StringBuilder sb = new StringBuilder();
                foreach (var p in parameters) sb.Append(p.Value).Append(", ");
                Logger.Trace($@"Parameters={sb};");
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        config.GroupName = CastDbNull.To(reader["GroupName"], string.Empty).Trim();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], string.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], string.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], string.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], string.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastTailDate = CastDbNull.To(reader["LastTailDate"], DateTime.MinValue);
                        config.TailSF = CastDbNull.To(reader["TailSF"], false);
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.TailFrequencyDays = CastDbNull.To(reader["TailFrequencyDays"], 0);
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

        private static SqlCommand GetTailTableConfig(List<string> filters)
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
                          ,[LastTailDate]
                          ,[TailSF]
                          ,[DateColumn]
                          ,[RetentionDays]
                          ,[TailFrequencyDays]
                          ,[NextTailDate]
                          ,[DeletedRows]
                          ,[execution_instance_guid]
                          ,[BITimeStamp]
                          ,[J_BU_Plant_srccd]
                    FROM [dbo].[cfgTablesTail]
                    WHERE
                     {filterString}";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return cmd;
        }

        public static TailTableConfigs GetTailTableConfigs(string plantCode, string serverName, SqlConnection cnArchCfg)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var configs = new TailTableConfigs();
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
                var cmd = GetTailTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var config = new TailTableConfig();
                        config.GroupName = CastDbNull.To(reader["GroupName"], string.Empty).Trim();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], string.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], string.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], string.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], string.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastTailDate = CastDbNull.To(reader["LastTailDate"], DateTime.MinValue);
                        config.TailSF = CastDbNull.To(reader["TailSF"], false);
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.TailFrequencyDays = CastDbNull.To(reader["TailFrequencyDays"], 0);
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

        public static TailTableConfigs GetTailTableGroupConfigs(string groupName, SqlConnection cnArchCfg)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var configs = new TailTableConfigs();
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();
            try
            {
                filters.Add("[GroupName] = @groupName");
                parameters.Add(new SqlParameter("@groupName", SqlDbType.VarChar));
                parameters.Last().Value = groupName;
                var cmd = GetTailTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var config = new TailTableConfig();
                        config.GroupName = CastDbNull.To(reader["GroupName"], string.Empty).Trim();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], string.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], string.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], string.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], string.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastTailDate = CastDbNull.To(reader["LastTailDate"], DateTime.MinValue);
                        config.TailSF = CastDbNull.To(reader["TailSF"], false);
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.TailFrequencyDays = CastDbNull.To(reader["TailFrequencyDays"], 0);
                        config.DeletedRows = CastDbNull.To<long>(reader["DeletedRows"], 0);
                        config.Plant = PlantManager.GetPlant(config.ServerName.Substring(0,4));
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

        private static int UpdateTailTableConfig(SqlConnection cnArchCfg, TailTableConfig tailTableConfigFinal)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            int rowCount;
            try
            {
                var serviceContext = new ServiceContext();
                tailTableConfigFinal.HostName = serviceContext.ServerAddress;
                var cmd = UpdateTailTableConfig(tailTableConfigFinal);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                //Logger.Trace(Logging.DbCmdString(cmd));
                Logger.Trace($@"Data Source={cmd.Connection.DataSource};Database={cmd.Connection.Database}");
                Logger.Trace(cmd.CommandText);
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

        private static SqlCommand UpdateTailTableConfig(TailTableConfig tailTableConfigFinal)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    UPDATE xx
                    SET 
                        [LastRunStatus] = {tailTableConfigFinal.LastRunStatus}
                        ,[HostName] = '{tailTableConfigFinal.HostName}'
                        ,[LastTailDate] = '{tailTableConfigFinal.LastTailDate}'
                        ,[NextTailDate] = '{tailTableConfigFinal.NextTailDate}'
                        ,[DeletedRows] = {tailTableConfigFinal.DeletedRows}
                        ,[execution_instance_guid] = '{tailTableConfigFinal.Guid}'
                        ,[BITimeStamp] = GETDATE()
                    FROM [dbo].[cfgTablesTail] xx
                    WHERE [ID] = {tailTableConfigFinal.ID}";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return cmd;
        }

        public static long GetRowCount(SqlConnection cnTarget, Table targetTable, string whereClause = null) 
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
                    CommandText = $@"SELECT COUNT(*) AS 'ROWS' FROM [{targetTable.SchemaName}].[{targetTable.TableName}] {whereClause}"
                };
                //Logger.Trace(Logging.DbCmdString(cmd));
                Logger.Trace($@"Data Source={cmd.Connection.DataSource};Database={cmd.Connection.Database}");
                Logger.Trace(cmd.CommandText);
                rowCount = Convert.ToInt64(cmd.ExecuteScalar());
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

        public static ProcessSteps TailTable(SqlConnection cnTarget, Table targetTable, ref TailTableConfig tailTableConfig)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var processSteps = new ProcessSteps(); 
            long totalDeletedRows = 0;
            long rowsRemaining = tailTableConfig.RowsToDelete;
            long maxDeleteRows = Convert.ToInt64(ConfigurationManager.AppSettings["MaxDeleteRows"]);
            var testModeNoDelete = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]);
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTarget,
                    CommandType = CommandType.Text,
                    CommandText = $@"DELETE TOP ({maxDeleteRows}) FROM [{targetTable.SchemaName}].[{targetTable.TableName}] WHERE [{tailTableConfig.DateColumn}] < ( GETDATE() - {tailTableConfig.RetentionDays} )"
                };
                if (testModeNoDelete)
                {
                    cmd.CommandText = cmd.CommandText + " --TestModeNoDelete";
                    while (totalDeletedRows < tailTableConfig.RowsToDelete)
                    {
                        //Logger.Trace(Logging.DbCmdString(cmd));
                        Logger.Trace($@"Data Source={cmd.Connection.DataSource};Database={cmd.Connection.Database}");
                        Logger.Trace(cmd.CommandText);
                        rowsRemaining = tailTableConfig.RowsToDelete - totalDeletedRows;
                        var delRows = rowsRemaining > maxDeleteRows ? maxDeleteRows : rowsRemaining; //cmd.ExecuteNonQuery();
                        totalDeletedRows += delRows;
                        Logger.Trace($@"ROWS DELETED: {delRows}");
                        Logger.Trace($@"ROWS REMAINING: {rowsRemaining}");
                        Logger.Trace($@"TOTAL ROWS DELETED: {totalDeletedRows}");
                        Logger.Trace("SLEEP 1 SECOND...");
                        //Thread.Sleep(1000);
                    }
                }
                else
                {
                    while (totalDeletedRows < tailTableConfig.RowsToDelete)
                    {
                        Logger.Trace($@"Data Source={cmd.Connection.DataSource};Database={cmd.Connection.Database}");
                        Logger.Trace(cmd.CommandText);
                        var delRows = cmd.ExecuteNonQuery();
                        totalDeletedRows += delRows;
                        Logger.Trace($@"ROWS DELETED: {delRows}");
                        Logger.Trace("SLEEP 1 SECOND...");
                        Thread.Sleep(1000);
                    }
                }
                tailTableConfig.DeletedRows = totalDeletedRows;
                tailTableConfig.Command = cmd.CommandText;
                Logger.Trace("ROWS: " + totalDeletedRows);
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
            return processSteps;
        }

    }
}
