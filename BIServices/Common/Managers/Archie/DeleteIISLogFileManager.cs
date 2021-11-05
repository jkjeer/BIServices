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
    public class DeleteIISLogFileManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse Process(SqlConnection cnCorpInt, SqlConnection cnArchCfg, string serverName, int retentionDays, bool sendReport)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Delete IISLog Files: {serverName} Retention Days: {retentionDays}", true);
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) response.ProcessName = $"{response.ProcessName} - Test Mode No Delete";
            response.ServerName = serverName;
            response.RetentionDays = retentionDays;
            response.ConnectionStrings.Add(new ConnectionString("cnArchCfg", cnArchCfg.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
            var server = new Server();
            var filePathIISLog = $@"\\{serverName}\c$\inetpub\logs\LogFiles\W3SVC1\";
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
                    var step3FetchIISLogFiles = new ProcessStep($@"Step 3. Fetch IISLog Files <{filePathIISLog}>");
                    var fileNamesIISLog = new List<string>();
                    try
                    {
                        var retentionDate = DateTime.Now.AddDays(-retentionDays);
                        DirectoryInfo dirInfoIISLog = new DirectoryInfo(filePathIISLog);
                        var filesIISLog = dirInfoIISLog.EnumerateFiles().Where(file => file.Extension == ".log" && file.LastWriteTime <= retentionDate).OrderBy(p => p.LastWriteTime);
                        fileNamesIISLog.AddRange(filesIISLog.Select(file => file.Name));
                        step3FetchIISLogFiles.RowsAffected = fileNamesIISLog.Count;
                    }
                    catch (Exception e)
                    {
                        step3FetchIISLogFiles.Errors.Add(new Error(e));
                    }
                    step3FetchIISLogFiles.Done();
                    response.ProcessSteps.Add(step3FetchIISLogFiles);
                    if (fileNamesIISLog.Any())
                    {
                        var step4DeleteIISLogFiles = new ProcessStep($@"Step 4. Delete IISLog Files ({serverName})");
                        var stepsDelete = new ProcessSteps();
                        try
                        {
                            foreach (var fileName in fileNamesIISLog)
                            {
                                var fullPathIISLog = $@"{filePathIISLog}{fileName}";
                                if (File.Exists(fullPathIISLog))
                                {
                                    try
                                    {
                                        FileInfo fi = new FileInfo(fullPathIISLog);
                                        var stepDeleteIISLogFile = new ProcessStep($@"Delete <{fullPathIISLog}> {fi.LastWriteTime}");
                                        stepDeleteIISLogFile.Size = fi.Length / 1024 / 1024; //mb
                                        step4DeleteIISLogFiles.Size += stepDeleteIISLogFile.Size;
                                        stepDeleteIISLogFile.RowsAffected = 1;
                                        stepsDelete.Add(stepDeleteIISLogFile);
                                        step4DeleteIISLogFiles.RowsAffected += stepDeleteIISLogFile.RowsAffected;
                                        response.RowsAffected += stepDeleteIISLogFile.RowsAffected;
                                        Logger.Info($"File.Delete({fullPathIISLog}, true)");
                                        if (!Convert.ToBoolean(ConfigurationManager.AppSettings["TestModeNoDelete"])) File.Delete(fullPathIISLog);
                                        stepDeleteIISLogFile.Done();
                                    }
                                    catch (Exception e)
                                    {
                                        step4DeleteIISLogFiles.Errors.Add(new Error($"{fullPathIISLog} {e.Message} {e.StackTrace}"));
                                        //InsertDocDeleteError(cnTCCSUPPORT, doc, $"{doc.Filter} document {doc.fsdDocID} PDF {doc.PDFPath} {e.Message}", deleteGUID);
                                    }
                                }
                                else
                                {
                                    step4DeleteIISLogFiles.Errors.Add(new Error($"{fullPathIISLog} not found.", false));
                                    step4DeleteIISLogFiles.RowsAffected = fileNamesIISLog.Count; //+= InsertDocDeleteError(cnTCCSUPPORT, doc, $"{doc.Filter} document {doc.fsdDocID} PDF not found.", deleteGUID);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            step4DeleteIISLogFiles.Errors.Add(new Error(e));
                        }
                        step4DeleteIISLogFiles.Done();
                        response.ProcessSteps.Add(step4DeleteIISLogFiles);
                        response.ProcessSteps.AddRange(stepsDelete);
                        response.Size = step4DeleteIISLogFiles.Size;
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
