using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using BIServices.Common.Managers.Archie;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Archie;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.ArchieSvc
{
    public class ArchieProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse ArchiveTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Archive Table: [{ serverName}].[{databaseName}].[{schemaName}].[{tableName}] BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Archive Table: [{ serverName}].[{databaseName}].[{schemaName}].[{tableName}]", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                cnTarget = GetTargetSqlConnection(serverName, databaseName);
                var targetTable = new Table(serverName, databaseName, schemaName, tableName);
                response = ArchiveTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "ArchiveTableResponseBody", "ArchiveTable");
                }
                cnCorpInt.Close();
                Logger.Info($"Archive Table: [{serverName}].[{databaseName}].[{schemaName}].[{tableName}] END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse ArchiveTables(string plantCode, string serverName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Archive Data Tables: [{plantCode}].[{serverName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                var configs = ArchiveTableManager.GetArchiveTableConfigs(plantCode, serverName, cnArchCfg);
                foreach (var config in configs)
                {
                    var targetTable = new Table(config.ServerName, config.DatabaseName, config.SchemaName, config.TableName);
                    cnTarget = GetTargetSqlConnection(targetTable.ServerName, targetTable.DatabaseName);
                    response = ArchiveTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
                    cnTarget.Close();
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "ArchiveTableResponseBody", $@"{taskName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "ArchiveTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse BackupTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Backup Table: [{ serverName}].[{databaseName}].[{schemaName}].[{tableName}] BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Backup Table: [{serverName}].[{databaseName}].[{schemaName}].[{tableName}]", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                cnTarget = GetTargetSqlConnection(serverName, databaseName);
                var targetTable = new Table(serverName, databaseName, schemaName, tableName);
                response = BackupTableManager.Process(cnCorpInt,cnArchCfg, cnTarget, targetTable);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "BackupTableResponseBody", "BackupTable");
                }
                cnCorpInt.Close();
                Logger.Info($"Backup Table: [{ serverName}].[{databaseName}].[{schemaName}].[{tableName}] END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse BackupTables(string plantCode, string serverName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Backup Data Tables: [{plantCode}].[{serverName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                var configs = BackupTableManager.GetBackupTableConfigs(plantCode, serverName, cnArchCfg);
                foreach (var config in configs)
                {
                    var targetTable = new Table(config.ServerName, config.DatabaseName, config.SchemaName, config.TableName);
                    cnTarget = GetTargetSqlConnection(targetTable.ServerName, targetTable.DatabaseName);
                    response = BackupTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
                    cnTarget.Close();
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "BackupTableResponseBody", $@"{taskName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "BackupTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse DeleteIISLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete IISLog Files: {serverName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete IISLog Files: {serverName} Retention Days: {retentionDays}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                response = DeleteIISLogFileManager.Process(cnCorpInt, cnArchCfg, serverName, retentionDays, sendReport);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                response.Close();
                Logger.Info($"Delete IISLog Files: {serverName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "DeleteIISLogFilesResponseBody", $@"Delete IISLog Files - {response.ServerName}");
                }
                cnCorpInt.Close();
            }
            return response;
        }

        public static ProcessResponse DeleteIISLogFilesClient(string plantName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete IISLog Files: {plantName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete IISLog Files: {plantName} Retention Days: {retentionDays}", true);
            response.Plant = PlantManager.GetPlant(plantName);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                var servers = LogDeleteManager.GetPlantConfig(cnArchCfg, plantName);
                foreach (var server in servers)
                {
                    response = DeleteIISLogFileManager.Process(cnCorpInt,cnArchCfg, server.ServerName, retentionDays, sendReport);
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "DeleteIISLogFilesResponseBody", $@"Delete IISLog Files - {response.ServerName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
                response.Close();
                Notify(response, cnCorpInt, "DeleteIISLogFilesResponseBody", $@"Delete IISLog Files - {response.Plant.PlantCode}");
            }
            finally
            {
                cnArchCfg.Close();
                cnCorpInt.Close();
                Logger.Info($"Delete IISLog Files: {plantName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse DeleteNLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete NLog Files: {serverName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete NLog Files: {serverName} Retention Days: {retentionDays}", true);
            response.ServerName = serverName;
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                response = DeleteNLogFileManager.Process(cnCorpInt, cnArchCfg, serverName, retentionDays, sendReport);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                response.Close();
                Logger.Info($"Delete NLog Files: {serverName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "DeleteNLogFilesResponseBody", $@"Delete NLog Files - {response.ServerName}");
                }
                cnCorpInt.Close();
            }
            return response;
        }

        public static ProcessResponse DeleteNLogFilesClient(string plantName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete NLog Files: {plantName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete NLog Files: {plantName} Retention Days: {retentionDays}", true);
            response.Plant = PlantManager.GetPlant(plantName);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                var servers = LogDeleteManager.GetPlantConfig(cnArchCfg, plantName);
                foreach (var server in servers)
                {
                    response = DeleteNLogFileManager.Process(cnCorpInt, cnArchCfg, server.ServerName, retentionDays, sendReport);
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "DeleteNLogFilesResponseBody", $@"Delete NLog Files - {response.ServerName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
                response.Close();
                Notify(response, cnCorpInt, "DeleteNLogFilesResponseBody", $@"Delete NLog Files - {response.Plant.PlantCode}");
            }
            finally
            {
                cnArchCfg.Close();
                cnCorpInt.Close();
                Logger.Info($"Delete NLog Files: {plantName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static ProcessResponse DeletePSLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete PSLog Files: {serverName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete PSLog Files: {serverName} Retention Days: {retentionDays}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                response = DeletePSLogFileManager.Process(cnCorpInt, cnArchCfg, serverName, retentionDays, sendReport);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                response.Close();
                Logger.Info($"Delete PSLog Files: {serverName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "DeletePSLogFilesResponseBody", $@"Delete PSLog Files - {response.ServerName}");
                }
                cnCorpInt.Close();
            }
            return response;
        }

        public static ProcessResponse DeletePSLogFilesClient(string plantName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete PSLog Files: {plantName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete PSLog Files: {plantName} Retention Days: {retentionDays}", true);
            response.Plant = PlantManager.GetPlant(plantName);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                var servers = LogDeleteManager.GetPlantConfig(cnArchCfg, plantName);
                foreach (var server in servers)
                {
                    response = DeletePSLogFileManager.Process(cnCorpInt, cnArchCfg, server.ServerName, retentionDays, sendReport);
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "DeletePSLogFilesResponseBody", $@"Delete PSLog Files - {response.ServerName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
                response.Done();
                Notify(response, cnCorpInt, "DeletePSLogFilesResponseBody", $@"Delete PSLog Files - {response.Plant.PlantCode}");
            }
            finally
            {
                cnArchCfg.Close();
                cnCorpInt.Close();
                Logger.Info($"Delete PSLog Files: {plantName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static ProcessResponse DeleteSOALogFiles(string serverName, int retentionDays, bool sendReport, string serviceName)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete SOALog Files: {serverName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete SOALog Files: {serverName} Retention Days: {retentionDays}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                response = DeleteSOALogFileManager.Process(cnArchCfg, serverName, retentionDays, sendReport, serviceName);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                response.Close();
                Logger.Info($"Delete SOALog Files: {serverName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "DeleteSOALogFilesResponseBody", $@"Delete SOA Files - {response.ServerName}");
                }
                cnCorpInt.Close();
            }
            return response;
        }

        public static ProcessResponse DeleteSvcLogFiles(string serverName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete SvcLog Files: {serverName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete SvcLog Files: {serverName} Retention Days: {retentionDays}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                response = DeleteSvcLogFileManager.Process(cnArchCfg, serverName, retentionDays, sendReport);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                response.Close();
                Logger.Info($"Delete SvcLog Files: {serverName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "DeleteSvcLogFilesResponseBody", $@"Delete SvcLog Files - {response.ServerName}");
                }
                cnCorpInt.Close();
            }
            return response;
        }

        public static ProcessResponse DeleteSvcLogFilesClient(string plantName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Delete SvcLog Files: {plantName} Retention Days: {retentionDays} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete SvcLog Files: {plantName} Retention Days: {retentionDays}", true);
            response.Plant = PlantManager.GetPlant(plantName);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            try
            {
                var servers = LogDeleteManager.GetPlantConfig(cnArchCfg, plantName);
                foreach (var server in servers)
                {
                    response = DeleteSvcLogFileManager.Process(cnArchCfg, server.ServerName, retentionDays, sendReport);
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "DeleteSvcLogFilesResponseBody", $@"Delete SvcLog Files - {response.ServerName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}"));
                response.Close();
                Notify(response, cnCorpInt, "DeleteSvcLogFilesResponseBody", $@"Delete SvcLog Files - {response.Plant.PlantCode}");
            }
            finally
            {
                cnArchCfg.Close();
                cnCorpInt.Close();
                Logger.Info($"Delete NLog Files: {plantName} Retention Days: {retentionDays} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse TailDataTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Tail Data Table: [{serverName}].[{databaseName}].[{schemaName}].[{tableName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                cnTarget = GetTargetSqlConnection(serverName, databaseName);
                var targetTable = new Table(serverName, databaseName, schemaName, tableName);
                response = TailDataTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "TailDataTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse TailDataTables(string plantCode, string serverName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Tail Data Tables: [{plantCode}].[{serverName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                var configs = TailDataTableManager.GetTailTableConfigs(plantCode, serverName, cnArchCfg);
                foreach (var config in configs)
                {
                    var targetTable = new Table(config.ServerName, config.DatabaseName, config.SchemaName, config.TableName);
                    cnTarget = GetTargetSqlConnection(targetTable.ServerName, targetTable.DatabaseName);
                    response = TailDataTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
                    cnTarget.Close();
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "TailDataTableResponseBody", $@"{taskName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "TailDataTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse TailDataTableGroup(string groupName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Tail Data Table Group: [{groupName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                var configs = TailDataTableManager.GetTailTableGroupConfigs(groupName, cnArchCfg);
                foreach (var config in configs)
                {
                    var targetTable = new Table(config.ServerName, config.DatabaseName, config.SchemaName, config.TableName);
                    cnTarget = GetTargetSqlConnection(targetTable.ServerName, targetTable.DatabaseName);
                    response = TailDataTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
                    cnTarget.Close();
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "TailDataTableResponseBody", $@"{taskName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "TailDataTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }


        public static ProcessResponse TruncateLedgerTable(string serverName, string databaseName, string schemaName, string tableName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Truncate Ledger Table: [{serverName}].[{databaseName}].[{schemaName}].[{tableName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                cnTarget = GetTargetSqlConnection(serverName, databaseName);
                var targetTable = new Table(serverName, databaseName, schemaName, tableName);
                response = TruncateLedgerTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "TruncateLedgerTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static ProcessResponse TruncateLedgerTables(string plantCode, string serverName, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var taskName = $@"Truncate Ledger Tables: [{plantCode}].[{serverName}]";
            Logger.Info($@"{taskName} BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"{taskName}", true);
            var cnArchCfg = GetArchConfigSqlConnection();
            var cnCorpInt = GetCorpIntSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var cnTarget = new SqlConnection();
            try
            {
                var configs = TruncateLedgerTableManager.GetTargetTableConfigs(plantCode, serverName, cnArchCfg);
                foreach (var config in configs)
                {
                    var targetTable = new Table(config.ServerName, config.DatabaseName, config.SchemaName, config.TableName);
                    cnTarget = GetTargetSqlConnection(targetTable.ServerName, targetTable.DatabaseName);
                    response = TruncateLedgerTableManager.Process(cnCorpInt, cnArchCfg, cnTarget, targetTable);
                    cnTarget.Close();
                    response.Close();
                    if (sendReport || response.HasErrors())
                    {
                        Notify(response, cnCorpInt, "TruncateLedgerTableResponseBody", $@"{taskName}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTarget.ConnectionString}  {e.Message}"));
            }
            finally
            {
                cnArchCfg.Close();
                cnTarget.Close();
                response.Close();
                if (sendReport || response.HasErrors())
                {
                    Notify(response, cnCorpInt, "TruncateLedgerTableResponseBody", $@"{taskName}");
                }
                cnCorpInt.Close();
                Logger.Info($"{taskName} END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static void Notify(ProcessResponse response, SqlConnection cnCorpInt, string xsltName, string subject)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                Summarize(response);
                var recipients = new List<string>();
                recipients.AddRange(Mail.GetRecipients(cnCorpInt, "ARCHIE", "ADMIN"));
                var serializedXml = response.Serialize();
                Logger.Info($"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, $@"BIServices.Common.Xslt.{xsltName}.xslt", Encoding.UTF8);
                Logger.Info($"Report XML: {xslOutput}");
                if (response.HasErrors()) subject = $"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"],
                    ConfigurationManager.AppSettings["MailFromAddress"],
                    recipients,
                    null,
                    subject,
                    xslOutput);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSendMail.Errors.Add(new Error(e));
            }
            finally
            {
                stepSendMail.Done();
                response.ProcessSteps.Add(stepSendMail);
            }
        }

        public static void Summarize(ProcessResponse response)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
                response.TestMode = Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"]) ? "True" : "False";
                var headerSummary = new
                {
                    response.TransferStatus,
                    response.TransferDate,
                    HasErrors = response.HasErrors(),
                    response.TransferMessage,
                    response.StartDate,
                    StartDateString = response.StartDate.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    response.EndDate,
                    EndDateString = response.EndDate.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.EndDate - response.StartDate).TotalSeconds,
                    response.InitialRowCount,
                    response.RowsAffected,
                    response.FinalRowCount,
                    response.TestMode,
                    response.Command,
                    response.RetentionDays,
                    response.Size
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarize.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarize.Done();
                response.ProcessSteps.Add(stepSummarize);
            }
        }

        private static SqlConnection GetArchConfigSqlConnection()
        {
            var cnArchCfg = new SqlConnection();
            try
            {
                cnArchCfg = ConnectionManager.GetSqlConnectionFromKey(@"ARCH.CONFIG");
                cnArchCfg.Open();
            }
            catch (Exception e)
            {
                //Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"ARCH.CONFIG {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            return cnArchCfg;
        }

        private static SqlConnection GetCorpIntSqlConnection()
        {
            var cnCorpInt = new SqlConnection();
            try
            {
                cnCorpInt = ConnectionManager.GetSqlConnectionFromKey(@"CORP.INTEGRATION");
                cnCorpInt.Open();
            }
            catch (Exception e)
            {
                //Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"CORP.INTEGRATION {cnCorpInt.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            return cnCorpInt;
        }

        private static SqlConnection GetTargetSqlConnection(string serverName, string databaseName)
        {
            SqlConnection cnTarget;
            try
            {
                cnTarget = ConnectionManager.GetSqlConnectionFromParts(serverName, databaseName);
                cnTarget.Open();
            }
            catch (Exception e)
            {
                //Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($@"[{serverName}].[{databaseName}]  {e.Message}: {e.StackTrace}");
            }
            return cnTarget;
        }
       
    }
}