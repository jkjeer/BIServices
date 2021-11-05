using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using BIServices.Common.Types.Archie;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using NLog;
// ReSharper disable TooWideLocalVariableScope
// ReSharper disable StringLiteralTypo

namespace BIServices.Common.Managers.Archie
{
    public class DeleteNLogFileManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse Process(SqlConnection cnCorpInt, SqlConnection cnArchCfg, string serverName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete NLog Files: {serverName} Retention Days: {retentionDays}", true);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) response.ProcessName = $"{response.ProcessName} - Test Mode No Delete";
            response.ServerName = serverName;
            response.RetentionDays = retentionDays;
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var server = new Server();
            var filePathNLog = $@"\\{serverName}\c$\temp\logs\";
            try
            {
                var step1GetServerConfig = new ProcessStep($@"Step 1. Get Server Config {serverName}");
                try
                {
                    server = LogDeleteManager.GetServerConfig(cnArchCfg, serverName);
                    step1GetServerConfig.RowsAffected = 1;
                    response.RowsAffected = step1GetServerConfig.RowsAffected;
                }
                catch (Exception e)
                {
                    step1GetServerConfig.Errors.Add(new Error(e));
                }
                step1GetServerConfig.Done();
                response.ProcessSteps.Add(step1GetServerConfig);

                var step2ValidateServer = new ProcessStep($@"Step 2. Validate Server Configuration {serverName}");
                try
                {
                    server.ValidationErrors = LogDeleteManager.Validate(server, retentionDays);
                    step2ValidateServer.RowsAffected = server.ValidationErrors.Count;
                    response.RowsAffected = step2ValidateServer.RowsAffected;
                }
                catch (Exception e)
                {
                    step2ValidateServer.Errors.Add(new Error(e));
                }
                step2ValidateServer.Done();
                response.ProcessSteps.Add(step2ValidateServer);

                if (!server.ValidationErrors.Any())
                {
                    var step3FetchNLogDateDirectories = new ProcessStep($@"Step 3. Fetch NLog Date Directories <{filePathNLog}>");
                    var dirNamesNLogDates = new List<string>();
                    try
                    {
                        var retentionDateStr = $"{DateTime.Now.AddDays(-retentionDays):yyyy-MM-dd}"; 
                        DirectoryInfo dirInfoNLogDates = new DirectoryInfo(filePathNLog);
                        var dirsNLogDates = dirInfoNLogDates.EnumerateDirectories().Where(dir => string.Compare(dir.Name, retentionDateStr, StringComparison.Ordinal) <= 0);
                        dirNamesNLogDates.AddRange(dirsNLogDates.Select(dir => dir.Name));
                        step3FetchNLogDateDirectories.RowsAffected = dirNamesNLogDates.Count;
                    }
                    catch (Exception e)
                    {
                        step3FetchNLogDateDirectories.Errors.Add(new Error(e));
                    }
                    step3FetchNLogDateDirectories.Done();
                    response.ProcessSteps.Add(step3FetchNLogDateDirectories);
                    if (dirNamesNLogDates.Any())
                    {
                        var step4FetchNLogServiceDirectories = new ProcessStep($@"Step 4. Delete NLog Service Directories <{filePathNLog}>");
                        var stepsDeleteDateDirectories = new ProcessSteps();
                        try
                        {
                            foreach (var dirNameDate in dirNamesNLogDates)
                            {
                                var fullPathNLogDate = $@"{filePathNLog}{dirNameDate}\";
                                try
                                {
                                    DirectoryInfo dirInfoNLogService = new DirectoryInfo(fullPathNLogDate);
                                    var dirsNLogService = dirInfoNLogService.EnumerateDirectories();
                                    var dirNamesNLogService = new List<string>();
                                    dirNamesNLogService.AddRange(dirsNLogService.Select(dir => dir.Name));
                                    if (dirNamesNLogService.Any())
                                    {
                                        step4FetchNLogServiceDirectories.RowsAffected += dirNamesNLogService.Count;
                                        var step5DeleteNLogServiceDirectories = new ProcessStep($@"Delete NLog Date Directory <{fullPathNLogDate}>");
                                        var stepsDelete = new ProcessSteps();
                                        try
                                        {
                                            foreach (var dirNameNLogService in dirNamesNLogService)
                                            {
                                                var fullPathNLogService = $@"{fullPathNLogDate}{dirNameNLogService}\";
                                                try
                                                {
                                                    var stepDeleteNLogServiceDirectory = new ProcessStep($@"Delete <{fullPathNLogService}>");
                                                    DirectoryInfo di = new DirectoryInfo(fullPathNLogService);
                                                    stepDeleteNLogServiceDirectory.Size = di.EnumerateFiles().Sum(file => file.Length) / 1024 / 1024; //mb
                                                    step5DeleteNLogServiceDirectories.Size += stepDeleteNLogServiceDirectory.Size;
                                                    step4FetchNLogServiceDirectories.Size += stepDeleteNLogServiceDirectory.Size;
                                                    stepDeleteNLogServiceDirectory.RowsAffected = Directory.GetFiles(fullPathNLogService).Length;
                                                    stepDeleteNLogServiceDirectory.Done();
                                                    stepsDelete.Add(stepDeleteNLogServiceDirectory);
                                                    step5DeleteNLogServiceDirectories.RowsAffected += stepDeleteNLogServiceDirectory.RowsAffected;
                                                    response.RowsAffected += stepDeleteNLogServiceDirectory.RowsAffected;
                                                }
                                                catch (Exception e)
                                                {
                                                    step5DeleteNLogServiceDirectories.Errors.Add(new Error($"{fullPathNLogService} {e.Message} {e.StackTrace}"));
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            step5DeleteNLogServiceDirectories.Errors.Add(new Error(e));
                                        }
                                        Logger.Info($"Directory.Delete({fullPathNLogDate}, true)");
                                        if (!Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) Directory.Delete(fullPathNLogDate, true);
                                        step5DeleteNLogServiceDirectories.Done();
                                        stepsDeleteDateDirectories.Add(step5DeleteNLogServiceDirectories);
                                        stepsDeleteDateDirectories.AddRange(stepsDelete);
                                    }
                                }
                                catch (Exception e)
                                {
                                    step4FetchNLogServiceDirectories.Errors.Add(new Error($"{fullPathNLogDate} {e.Message} {e.StackTrace}"));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            step4FetchNLogServiceDirectories.Errors.Add(new Error(e));
                        }
                        step4FetchNLogServiceDirectories.Done();
                        response.ProcessSteps.Add(step4FetchNLogServiceDirectories);
                        response.ProcessSteps.AddRange(stepsDeleteDateDirectories);
                        response.Size = step4FetchNLogServiceDirectories.Size;
                    }
                }
                else
                {
                    foreach (var validationError in server.ValidationErrors)
                    {
                        response.Errors.Add(new Error(validationError.Message));
                    }
                    //targetTable.TruncateStatus = "E";
                    //targetTable.TruncateMessage = targetTable.ValidationStatus;
                }
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
            return response;
        }

    }
}
