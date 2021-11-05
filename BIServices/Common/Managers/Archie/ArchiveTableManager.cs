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
    public class ArchiveTableManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse Process(SqlConnection cnCorpInt, SqlConnection cnArchCfg, SqlConnection cnTarget, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Archive Data Table: {targetTable.QualifiedName}";
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) taskName = $@"{taskName} - Test Mode No Delete";
            Logger.Info($"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($@"{taskName}", true);
            response.ServerName = targetTable.ServerName;
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnTarget", cnTarget.ConnectionString));
            var archiveTableConfigInit = new ArchiveTableConfig();
            var archiveTableConfigFinal = new ArchiveTableConfig();
            var guid = Guid.NewGuid();
            var cnArchRepo = new SqlConnection();
            try
            {
                var step1ValidateTable = new ProcessStep($@"Step 1. Validate Table {targetTable.QualifiedName}");
                try
                {
                    archiveTableConfigInit = GetArchiveTableConfig(cnArchCfg, targetTable);
                    cnArchRepo = GetArchRepoSqlConnection(archiveTableConfigInit);
                    response.ConnectionStrings.Add(new ConnectionString("cnArchRepo", cnArchRepo.ConnectionString));
                    //targetTable.ValidationErrors = Validate(cnArchCfg, cnTarget, targetTable, cnArchRepo);
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
                    ArchieLogManager.LogActivity(cnArchCfg, new ArchieLog("Error", "TailDataTable", targetTable.TailMessage,
                        "", archiveTableConfigInit.DateColumn, archiveTableConfigInit.RetentionDays, archiveTableConfigInit.InitialRowCount, step1ValidateTable.RowsAffected, archiveTableConfigInit.InitialRowCount - step1ValidateTable.RowsAffected,
                        Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]),
                        archiveTableConfigInit.GroupName, archiveTableConfigInit.PlantCode, archiveTableConfigInit.ServerName, archiveTableConfigInit.DatabaseName, archiveTableConfigInit.SchemaName,
                        archiveTableConfigInit.TableName, guid));
                }
                else
                {
                    var step2GatherStatistics = new ProcessStep($@"Step 2. Gather statistics for {targetTable.QualifiedName}");
                    try
                    {
                        archiveTableConfigInit.InitialRowCount = 1000; //GetRowCount(cnTarget, targetTable);
                        archiveTableConfigInit.RowsToDelete = 1000; //GetRowCount(cnTarget, targetTable, $"WHERE [{ archiveTableConfigInit.DateColumn}] < GETDATE() - { archiveTableConfigInit.RetentionDays}");
                        archiveTableConfigInit.FinalRowCount = archiveTableConfigInit.InitialRowCount - archiveTableConfigInit.RowsToDelete;
                        archiveTableConfigFinal = archiveTableConfigInit;
                        archiveTableConfigFinal.Plant = archiveTableConfigInit.Plant;
                        archiveTableConfigFinal.PlantCode = archiveTableConfigInit.PlantCode;
                        archiveTableConfigFinal.ServerName = archiveTableConfigInit.ServerName;
                        archiveTableConfigFinal.DatabaseName = archiveTableConfigInit.DatabaseName;
                        archiveTableConfigFinal.SchemaName = archiveTableConfigInit.SchemaName;
                        archiveTableConfigFinal.TableName = archiveTableConfigInit.TableName;
                        archiveTableConfigFinal.Guid = guid;
                        archiveTableConfigFinal.LastRunStatus = 0;
                        step2GatherStatistics.RowsAffected = archiveTableConfigFinal.RowsToDelete;
                    }
                    catch (Exception e)
                    {
                        step2GatherStatistics.Errors.Add(new Error(e));
                    }
                    step2GatherStatistics.Done();
                    response.ProcessSteps.Add(step2GatherStatistics);
                    if (!step2GatherStatistics.Errors.Any() && archiveTableConfigFinal.RowsToDelete > 0)
                    {
                        var step3ArchiveTable = new ProcessStep($@"Step 3. Archive Table {targetTable.QualifiedName}");
                        var archiveSteps = new ProcessSteps();
                        var getReaderStep = new ProcessStep();
                        var bcpStep = new ProcessStep();
                        var validateStep = new ProcessStep();
                        long totalReadRows = 0;
                        long totalMovedRows = 0;
                        long missingRows = 0;
                        long totalDeletedRows = 0;
                        long maxDeleteRows = Convert.ToInt64(ConfigurationManager.AppSettings["MaxDeleteRows"]);
                        try
                        {
                            // Get batch of source data into DataReader
                            getReaderStep = new ProcessStep($@"Read oldest {maxDeleteRows} rows from {targetTable.QualifiedName}");
                            var cmdGetReader = new SqlCommand {Connection = cnTarget, CommandType = CommandType.Text, CommandText = $@"SELECT TOP ({maxDeleteRows}) * FROM [{targetTable.SchemaName}].[{targetTable.TableName}] WHERE [{archiveTableConfigFinal.DateColumn}] < ( GETDATE() - {archiveTableConfigFinal.RetentionDays} ) ORDER BY [{archiveTableConfigFinal.DateColumn}]"};
                            Logger.Trace(Logging.DbCmdString(cmdGetReader));
                            archiveTableConfigFinal.Command = cmdGetReader.CommandText;
                            using (SqlDataReader reader = cmdGetReader.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                if (reader.HasRows)
                                {
                                    // BulkCopy the reader data to ArchRepository
                                    var bulkResponse = BulkCopyManager.BulkReader($"Ledger.{targetTable.TableName}", reader, cnArchRepo);
                                    var movedRows = bulkResponse.RowsCopied;
                                    bcpStep = new ProcessStep($@"BulkCopy {movedRows} rows from {targetTable.QualifiedName} to [Ledger].[{targetTable.TableName}]");
                                    totalMovedRows += movedRows;
                                    Logger.Trace($@"ROWS: {movedRows}");

                                    // Verify that all reader rows exist in ArchRepository before deleting them
                                    validateStep = new ProcessStep($@"Validate {movedRows} rows in [{targetTable.SchemaName}].[{targetTable.TableName}]");
                                    // Create temp table for key list.
                                    var cmdCreateTemp = new SqlCommand {CommandType = CommandType.Text, CommandText = @"CREATE TABLE #TempKeyList (KeyValue bigint NULL)", Connection = cnArchRepo};
                                    cmdCreateTemp.ExecuteNonQuery();

                                    // Load the Key Values from the reader into the temp table.
                                    using (SqlBulkCopy bulk = new SqlBulkCopy(cnArchRepo, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.TableLock, null))
                                    {
                                        bulk.DestinationTableName = archiveTableConfigFinal.TableName;
                                        bulk.BulkCopyTimeout = ConfigurationManager.AppSettings["BulkCopyTimeout"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["BulkCopyTimeout"]) : 300;
                                        bulk.BatchSize = 25000;
                                        var map = new SqlBulkCopyColumnMapping($"{archiveTableConfigFinal.KeyColumn}", "KeyValue");
                                        bulk.ColumnMappings.Add(map);
                                        bulk.DestinationTableName = "#TempKeyList";
                                        bulk.WriteToServer(reader);
                                    }

                                    // Query the ArchRepository table to see if all Key Values exist.
                                    var cmd = new SqlCommand {Connection = cnArchRepo, CommandType = CommandType.Text, CommandText = $@"SELECT Count(*) FROM #TempKeyList K WHERE NOT EXISTS ( SELECT * FROM [{targetTable.SchemaName}].[{targetTable.TableName}] L WHERE L.[{archiveTableConfigFinal.KeyColumn}] = K.[KeyValue])"};
                                    Logger.Trace(Logging.DbCmdString(cmd));
                                    missingRows = Convert.ToInt64(cmd.ExecuteScalar());

                                    // Drop the temp table.
                                    var dropTempTableStep = new ProcessStep($@"Drop temp table");
                                    var cmdDropTemp = new SqlCommand { CommandType = CommandType.Text, CommandText = @"DROP TABLE IF EXISTS #TempKeyList", Connection = cnArchRepo };
                                    cmdDropTemp.ExecuteNonQuery();

                                    // Truncate the Ledger table.
                                    var truncateLedgerTableStep = new ProcessStep($@"Truncate [Ledger].[{targetTable.TableName}]");
                                    var cmdTruncateLedger = new SqlCommand { CommandType = CommandType.Text, CommandText = @"TRUNCATE TABLE [Ledger].[{targetTable.TableName}]", Connection = cnArchRepo };
                                    cmdTruncateLedger.ExecuteNonQuery();

                                    var deleteStep = new ProcessStep($@"Delete {movedRows} rows from {targetTable.QualifiedName}");
                                    if (missingRows == 0)
                                    {
                                        var testModeNoDelete = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]);
                                            var cmdDelete = new SqlCommand {Connection = cnTarget, CommandType = CommandType.Text, CommandText = $@"DELETE TOP ({maxDeleteRows}) FROM [{targetTable.SchemaName}].[{targetTable.TableName}] WHERE [{archiveTableConfigFinal.DateColumn}] < ( GETDATE() - {archiveTableConfigFinal.RetentionDays} )"};
                                            if (testModeNoDelete)
                                            {
                                                cmd.CommandText = cmd.CommandText + " --TestModeNoDelete";
                                                Logger.Trace(Logging.DbCmdString(cmd));
                                            }
                                            else
                                            {
                                                while (totalDeletedRows < archiveTableConfigFinal.RowsToDelete)
                                                {
                                                    Logger.Trace(Logging.DbCmdString(cmd));
                                                    var delRows = archiveTableConfigFinal.RowsToDelete; //cmd.ExecuteNonQuery();
                                                    totalDeletedRows += delRows;
                                                    Logger.Trace($@"ROWS: {delRows}");
                                                    Logger.Trace("SLEEP 1 SECOND...");
                                                    Thread.Sleep(1000);
                                                }
                                            }
                                            archiveTableConfigFinal.DeletedRows = totalDeletedRows;
                                            archiveTableConfigFinal.Command = cmd.CommandText;
                                            Logger.Trace("ROWS: " + totalDeletedRows);
                                    }


                                    archiveSteps.Add(getReaderStep);
                                    archiveSteps.Add(bcpStep);
                                    archiveSteps.Add(validateStep);
                                    archiveSteps.Add(dropTempTableStep);
                                    archiveSteps.Add(truncateLedgerTableStep);
                                    archiveSteps.Add(deleteStep);
                                }
                                reader.Close();
                            }


                            archiveTableConfigFinal.MovedRows = totalMovedRows;
                            archiveTableConfigFinal.DeletedRows = totalDeletedRows;
                            Logger.Trace("ROWS: " + totalDeletedRows);

                            step3ArchiveTable.RowsAffected = archiveTableConfigFinal.DeletedRows;
                            response.RowsAffected = step3ArchiveTable.RowsAffected;
                            archiveTableConfigFinal.LastRunStatus = 1;
                            archiveTableConfigFinal.LastArchiveDate = DateTime.Now;
                            archiveTableConfigFinal.NextArchiveDate = DateTime.Now.AddDays(archiveTableConfigFinal.ArchiveFrequencyDays).AddHours(-1);
                            archiveTableConfigFinal.FinalRowCount = GetRowCount(cnTarget, targetTable);
                            ArchieLogManager.LogActivity(cnArchCfg, new ArchieLog("Event", "ArchiveTable",
                                taskName, archiveTableConfigFinal.Command.Trim(), archiveTableConfigFinal.DateColumn, archiveTableConfigFinal.RetentionDays,
                                archiveTableConfigFinal.InitialRowCount, step3ArchiveTable.RowsAffected, archiveTableConfigFinal.FinalRowCount,
                                Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]),
                                archiveTableConfigFinal.GroupName, archiveTableConfigFinal.PlantCode, archiveTableConfigFinal.ServerName, archiveTableConfigFinal.DatabaseName, archiveTableConfigFinal.SchemaName,
                                archiveTableConfigFinal.TableName, archiveTableConfigFinal.Guid));
                        }
                        catch (Exception e)
                        {
                            step3ArchiveTable.Errors.Add(new Error(e));
                        }

                        step3ArchiveTable.Done();
                        response.ProcessSteps.Add(step3ArchiveTable);
                        response.ProcessSteps.AddRange(archiveSteps);
                        response.InitialRowCount = archiveTableConfigFinal.InitialRowCount;
                        response.FinalRowCount = archiveTableConfigFinal.FinalRowCount;
                        response.TestMode = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]) ? "True" : "False";
                        response.RetentionDays = archiveTableConfigFinal.RetentionDays;
                        response.Command = archiveTableConfigFinal.Command.Trim();
                        //if (!step3ArchiveTable.Errors.Any() && (step3ArchiveTable.RowsAffected == archiveTableConfigFinal.RowsToDelete ))
                        //{
                        //    var step4TailTable = new ProcessStep($@"Step 4. Tail Table {targetTable.QualifiedName}");
                        //    var deleteSteps = new ProcessSteps();
                        //    try
                        //    {
                        //        deleteSteps = TailTable(cnTarget, targetTable, ref archiveTableConfigFinal);
                        //        step4TailTable.RowsAffected = archiveTableConfigFinal.DeletedRows;
                        //        response.RowsAffected = step4TailTable.RowsAffected;
                        //        archiveTableConfigFinal.LastRunStatus = 1;
                        //        archiveTableConfigFinal.LastArchiveDate = DateTime.Now;
                        //        archiveTableConfigFinal.NextArchiveDate = DateTime.Now.AddDays(archiveTableConfigFinal.ArchiveFrequencyDays).AddHours(-1);
                        //        archiveTableConfigFinal.FinalRowCount = GetRowCount(cnTarget, targetTable);
                        //        ArchieLogManager.LogActivity(cnArchCfg, new ArchieLog("Event", "ArchiveTable",
                        //            taskName, archiveTableConfigFinal.Command.Trim(), archiveTableConfigFinal.DateColumn, archiveTableConfigFinal.RetentionDays,
                        //            archiveTableConfigFinal.InitialRowCount, step4TailTable.RowsAffected, archiveTableConfigFinal.FinalRowCount,
                        //            Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]),
                        //            archiveTableConfigFinal.PlantCode, archiveTableConfigFinal.ServerName, archiveTableConfigFinal.DatabaseName, archiveTableConfigFinal.SchemaName,
                        //            archiveTableConfigFinal.TableName, archiveTableConfigFinal.Guid));
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        step4TailTable.Errors.Add(new Error(e));
                        //    }

                        //    step4TailTable.Done();
                        //    response.ProcessSteps.Add(step4TailTable);
                        //    response.ProcessSteps.AddRange(deleteSteps);
                        //    response.InitialRowCount = archiveTableConfigFinal.InitialRowCount;
                        //    response.FinalRowCount = archiveTableConfigFinal.FinalRowCount;
                        //    response.TestMode = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]) ? "True" : "False";
                        //    response.RetentionDays = archiveTableConfigFinal.RetentionDays;
                        //    response.Command = archiveTableConfigFinal.Command.Trim();
                        //    if (!step4TailTable.Errors.Any())
                        //    {
                        //        var step4UpdateConfigTable = new ProcessStep($@"Step 4. Update Config statistics in [{cnArchCfg.DataSource}].[Archiver].[dbo].[cfgTablesArchive]");
                        //        try
                        //        {
                        //            var rowCount = UpdateArchiveTableConfig(cnArchCfg, archiveTableConfigFinal);
                        //            step4UpdateConfigTable.RowsAffected = rowCount;
                        //        }
                        //        catch (Exception e)
                        //        {
                        //            step4UpdateConfigTable.Errors.Add(new Error(e));
                        //        }

                        //        step4UpdateConfigTable.Done();
                        //        response.ProcessSteps.Add(step4UpdateConfigTable);
                        //    }
                        //}
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
                cnArchRepo.Close();
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return response;
        }

        private static SqlConnection GetArchRepoSqlConnection(ArchiveTableConfig archiveTableConfig)
        {
            var cnArchRepo = new SqlConnection();
            try
            {
                //cnArchRepo = ConnectionManager.GetSqlConnectionFromKey(@"ARCH.REPO");
                var repoServer = ConfigurationManager.AppSettings["ArchieRepositoryServer"];
                cnArchRepo.ConnectionString = ConnectionManager.GetSqlConnectionFromParts(repoServer, $"{archiveTableConfig.Plant.PlantCode}_{archiveTableConfig.DatabaseName}_Archive").ConnectionString;
                cnArchRepo.Open();
            }
            catch (Exception e)
            {
                //Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"ARCH.REPO {cnArchRepo.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            return cnArchRepo;
        }

        private static ValidationErrors Validate(SqlConnection cnArchCfg, SqlConnection cnTarget, Table targetTable, SqlConnection cnArchRepo)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var validationErrors = new ValidationErrors();
            try
            {
                var tableConfig = GetArchiveTableConfig(cnArchCfg, targetTable);
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
                    var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for Archiving.", "Validation", "Fatal", true);
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
                    //if (!tableConfig.TailSF)
                    //{
                    //    var validationError = new ValidationError($"{targetTable.QualifiedName} is not configured for Archiving.", "Validation", "Fatal", true);
                    //    validationErrors.Add(validationError);
                    //    targetTable.ValidationStatus = validationError.Message;
                    //}
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
                    else
                    {
                        var repoTable = new Table(cnArchRepo.DataSource, tableConfig.RepoDatabaseName, tableConfig.SchemaName, tableConfig.TableName);
                        var isRepoDatabaseExist = TableManager.CheckDatabaseExists(cnArchRepo, repoTable);
                        if (!isRepoDatabaseExist)
                        {
                            var validationError = new ValidationError($@"Archie Repository Database [{repoTable.ServerName}].[{repoTable.DatabaseName}] not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        var isRepoSchemaExist = TableManager.CheckSchemaExists(cnArchRepo, repoTable);
                        if (!isRepoSchemaExist)
                        {
                            var validationError = new ValidationError($@"Archie Repository Schema [{repoTable.ServerName}].[{repoTable.DatabaseName}].[{repoTable.SchemaName}] not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        var isRepoTableExist = TableManager.CheckTableExists(cnArchRepo, repoTable);
                        if (!isRepoTableExist)
                        {
                            var validationError = new ValidationError($@"Archie Repository Table {repoTable.QualifiedName} not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        var ledgerTable = new Table(cnArchRepo.DataSource, tableConfig.RepoDatabaseName, "Ledger", tableConfig.TableName);
                        var isRepoLedgerTableExist = TableManager.CheckTableExists(cnArchRepo, ledgerTable);
                        if (!isRepoLedgerTableExist)
                        {
                            var validationError = new ValidationError($@"Archie Repository Ledger Table {ledgerTable.QualifiedName} not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
                        var isRepoLedgerTriggerExist = TableManager.CheckTriggerExists(cnArchRepo, ledgerTable);
                        if (!isRepoLedgerTriggerExist)
                        {
                            var validationError = new ValidationError($@"Archie Repository Ledger Trigger [Ledger].[Trig_Insert_{tableConfig.TableName}] not found", "Validation", "Error", true);
                            validationErrors.Add(validationError);
                            targetTable.ValidationStatus = validationError.Message;
                        }
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

        public static ArchiveTableConfig GetArchiveTableConfig(SqlConnection cnArchCfg, Table targetTable)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var config = new ArchiveTableConfig();
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
                var cmd = GetArchiveTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
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
                        config.LastArchiveDate = CastDbNull.To(reader["LastArchiveDate"], DateTime.MinValue);
                        config.TailSF = CastDbNull.To(reader["TailSF"], false);
                        config.KeyColumn = CastDbNull.To(reader["KeyColumn"], string.Empty).Trim();
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.ArchiveFrequencyDays = CastDbNull.To(reader["ArchiveFrequencyDays"], 0);
                        //config.DeletedRows = CastDbNull.To<long>(reader["DeletedRows"], 0);
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

        private static SqlCommand GetArchiveTableConfig(List<string> filters)
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
                          ,[IsRunning]
                          ,[HostName]
                          ,[LastArchiveDate]
                          ,[InitializeFromSF]
                          ,[HasBeenInitialized]
                          ,[BackupFromSF]
                          ,[TailSF]
                          ,[ArchiveFrequencyDays]
                          ,[NextArchiveDate]
                          ,[RetentionDays]
                          ,[MarginDays]
                          ,[KeyColumn]
                          ,[DateColumn]
                          ,[TimeStampColumn]
                          ,[OtherCondition]
                          ,[Description]
                          ,[UniqueKey]
                          ,[MaxDeleteRows]
                          ,[DateColumnCandidates]
                          ,[IsReplicated]
                          ,[SFRows]
                          ,[SFDataSize]
                          ,[SFIndexSize]
                          ,[SFMinDate]
                          ,[SFMaxDate]
                          ,[SFDays]
                          ,[ArcRows]
                          ,[ArcDataSize]
                          ,[ArcIndexSize]
                          ,[ArcMinDate]
                          ,[ArcMaxDate]
                          ,[ArcDays]
                          ,[execution_instance_guid]
                          ,[BITimeStamp]
                          ,[RestoreToOLTP]
                          ,[J_BU_Plant_srccd]
                    FROM [dbo].[cfgTablesArchive]
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

        public static ArchiveTableConfigs GetArchiveTableConfigs(string plantCode, string serverName, SqlConnection cnArchCfg)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var configs = new ArchiveTableConfigs();
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
                var cmd = GetArchiveTableConfig(filters);
                foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var config = new ArchiveTableConfig();
                        config.PlantCode = CastDbNull.To(reader["PlantCode"], string.Empty).Trim();
                        config.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                        config.DatabaseName = CastDbNull.To(reader["DatabaseName"], string.Empty).Trim();
                        config.SchemaName = CastDbNull.To(reader["SchemaName"], string.Empty).Trim();
                        config.TableName = CastDbNull.To(reader["TableName"], string.Empty).Trim();
                        config.ID = CastDbNull.To(reader["ID"], 0);
                        config.LastRunStatus = CastDbNull.To(reader["LastRunStatus"], 0);
                        config.IsActive = CastDbNull.To(reader["IsActive"], false);
                        config.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                        config.LastArchiveDate = CastDbNull.To(reader["LastArchiveDate"], DateTime.MinValue);
                        config.TailSF = CastDbNull.To(reader["TailSF"], false);
                        config.DateColumn = CastDbNull.To(reader["DateColumn"], string.Empty).Trim();
                        config.RetentionDays = CastDbNull.To(reader["RetentionDays"], 0);
                        config.ArchiveFrequencyDays = CastDbNull.To(reader["ArchiveFrequencyDays"], 0);
                        //config.DeletedRows = CastDbNull.To<long>(reader["DeletedRows"], 0);
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

        private static long UpdateArchiveTableConfig(SqlConnection cnArchCfg, ArchiveTableConfig archiveTableConfigFinal)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            long rowCount;
            try
            {
                var serviceContext = new ServiceContext();
                archiveTableConfigFinal.HostName = serviceContext.ServerAddress;
                var cmd = UpdateArchiveTableConfig(archiveTableConfigFinal);
                cmd.Connection = cnArchCfg;
                //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                Logger.Trace(Logging.DbCmdString(cmd));
                rowCount = Convert.ToInt64(cmd.ExecuteNonQuery());
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

        private static SqlCommand UpdateArchiveTableConfig(ArchiveTableConfig archiveTableConfigFinal)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                    UPDATE xx
                    SET 
                        [LastRunStatus] = {archiveTableConfigFinal.LastRunStatus}
                        ,[HostName] = '{archiveTableConfigFinal.HostName}'
                        ,[LastArchiveDate] = '{archiveTableConfigFinal.LastArchiveDate}'
                        ,[NextArchiveDate] = '{archiveTableConfigFinal.NextArchiveDate}'
                        ,[DeletedRows] = {archiveTableConfigFinal.DeletedRows}
                        ,[execution_instance_guid] = '{archiveTableConfigFinal.Guid}'
                        ,[BITimeStamp] = GETDATE()
                    FROM [dbo].[cfgTablesArchive] xx
                    WHERE [ID] = {archiveTableConfigFinal.ID}
                ";
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
                    CommandText = $@"
                        SELECT COUNT(*) 
                        FROM [{targetTable.SchemaName}].[{targetTable.TableName}]
                        {whereClause}"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
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


    }
}