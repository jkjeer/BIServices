
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.MonitorSvc
{
    public class MonitorProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse DuplicateUdcAuditE1()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Check E1 Duplicate Udc Short Description");
            var e1Session = BusinessSystemSessionManager.GetBusinessSession();
            try
            {
                response.ConnectionStrings.Add(new ConnectionString("cnE1", e1Session.DbConnection.ConnectionString));
                var e1F0005s =  new F0005s();
                var step1GetDuplicateDRSPHD = new ProcessStep($@"Step {response.ProcessSteps.Count + 1}. Get Duplicate E1 DRSPHDs");
                var duplicateE1SPHDs = F0005Manager.GetDuplicateDRSPHD(e1Session);
                step1GetDuplicateDRSPHD.RowsAffected = duplicateE1SPHDs.Count;
                step1GetDuplicateDRSPHD.Done();
                response.ProcessSteps.Add(step1GetDuplicateDRSPHD);
                if (duplicateE1SPHDs.Any())
                {
                    var stepGetF0005s = new ProcessStep($@"Step {response.ProcessSteps.Count + 1}. Get E1 F0005s");
                    e1F0005s = F0005Manager.GetF0005sByDRSPHD(e1Session, duplicateE1SPHDs);
                    stepGetF0005s.RowsAffected = e1F0005s.Count;
                    stepGetF0005s.Done();
                    response.ProcessSteps.Add(stepGetF0005s);
                }
                if (e1F0005s.Any())
                {
                    response.TransferStatus = "Y";
                    response.Done();
                    SummarizeDuplicateUdcAuditE1(response, e1F0005s);
                    NotifyDuplicateUdcAuditE1(response);
                }              
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                e1Session.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static void SummarizeDuplicateUdcAuditE1(ProcessResponse response, F0005s f0005s)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var itemLines = f0005s.ToList();
                response.SerializedObjects.Add("ItemLine", itemLines, "ItemLines");

                var headerSummary = new
                {
                    ItemLines = itemLines.Count,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }

        public static void NotifyDuplicateUdcAuditE1(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.DuplicateUdcAuditE1ResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = $@"BI [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}] E1 Duplicate Udc Audit Monitor";
                if (response.HasErrors()) subject = $@"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"], ConfigurationManager.AppSettings["MailFromAddress"], toConfigAddresses, null, subject, xslOutput);
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

        public static ProcessResponse DuplicateUdcAuditDw()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Check DW Duplicate Udc Short Description");
            var cnDW = GetDWSqlConnection();
            try
            {
                response.ConnectionStrings.Add(new ConnectionString("cnDW", cnDW.ConnectionString));
                var dwF0005s = new F0005s();
                var step2GetDuplicateDWShortDescription = new ProcessStep($@"Step {response.ProcessSteps.Count + 1}. Get Duplicate DW DRSPHDs");
                var duplicateDwSPHDs = DWManager.GetDuplicateDWShortDescriptions(cnDW);
                step2GetDuplicateDWShortDescription.RowsAffected = duplicateDwSPHDs.Count;
                step2GetDuplicateDWShortDescription.Done();
                response.ProcessSteps.Add(step2GetDuplicateDWShortDescription);
                if (duplicateDwSPHDs.Any())
                {
                    var stepGetShortDescriptions = new ProcessStep($@"Step {response.ProcessSteps.Count + 1}. Get DW F0005s");
                    dwF0005s = DWManager.GetDwF0005sByDRSPHD(cnDW, duplicateDwSPHDs);
                    stepGetShortDescriptions.RowsAffected = dwF0005s.Count;
                    stepGetShortDescriptions.Done();
                    response.ProcessSteps.Add(stepGetShortDescriptions);
                }
                if (dwF0005s.Any())
                {
                    response.TransferStatus = "Y";
                    response.Done();
                    SummarizeDuplicateUdcAuditDw(response, dwF0005s);
                    NotifyDuplicateUdcAuditDw(response);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                cnDW.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static void SummarizeDuplicateUdcAuditDw(ProcessResponse response, F0005s f0005s)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var itemLines = f0005s.ToList();
                response.SerializedObjects.Add("ItemLine", itemLines, "ItemLines");

                var headerSummary = new
                {
                    ItemLines = itemLines.Count,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }

        public static void NotifyDuplicateUdcAuditDw(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.DuplicateUdcAuditDwResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = $@"BI [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}] DW Duplicate Udc Audit Monitor";
                if (response.HasErrors()) subject = $@"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"], ConfigurationManager.AppSettings["MailFromAddress"], toConfigAddresses, null, subject, xslOutput);
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

        public static ProcessResponse CheckE1HALatency()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Check E1 HA Latency");
            var cnDW = new SqlConnection();
            try
            {
                cnDW = GetDWSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnDW", cnDW.ConnectionString));
                var stepE1HALatency = new ProcessStep("Step 1. Check E1 HA Latency");
                var isE1HALatent = DWManager.GetE1HALatency(cnDW);
                stepE1HALatency.RowsAffected = isE1HALatent ? 2 : 1;
                stepE1HALatency.Done();
                response.ProcessSteps.Add(stepE1HALatency);
                if (isE1HALatent)
                {
                    response.TransferStatus = "Y";
                    SummarizeGetE1HALatency(response);
                    NotifyGetE1HALatency(response);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                cnDW.Close();
                response.Done();
                Logging.InsertProcessLog(response);
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static void SummarizeGetE1HALatency(ProcessResponse response)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var headerSummary = new
                {
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }

        public static void NotifyGetE1HALatency(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.E1HALatencyResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI E1HA Latency Monitor";
                if (response.HasErrors()) subject = $@"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"], ConfigurationManager.AppSettings["MailFromAddress"], toConfigAddresses, null, subject, xslOutput);
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

        public static ProcessResponse GetProcessLogErrors(int lookBackMinutes)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("E1 Get ProcessLog Errors");
            var cnCorpInt = new SqlConnection();
            try
            {
                cnCorpInt = GetCorpIntSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnCorpInt", cnCorpInt.ConnectionString));
                var step1GetProcessLogErrors = new ProcessStep("Step 1. Get ProcessLog Errors");
                var processLogErrors = ProcessLogManager.GetProcesslogErrors(cnCorpInt, lookBackMinutes);
                step1GetProcessLogErrors.RowsAffected = processLogErrors.Count;
                step1GetProcessLogErrors.Done();
                response.ProcessSteps.Add(step1GetProcessLogErrors);
                if (processLogErrors.Any())
                {
                    response.TransferStatus = "Y";
                    SummarizeGetProcessLogErrors(response, lookBackMinutes, processLogErrors);
                    NotifyGetProcessLogErrors(response);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                cnCorpInt.Close();
                response.Done();
                Logging.InsertProcessLog(response);
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static void SummarizeGetProcessLogErrors(ProcessResponse response, int lookBackMinutes, ProcessLogErrors processLogErrors)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var processLogErrorLines = processLogErrors.ToList();
                response.SerializedObjects.Add("ProcessLogErrorLine", processLogErrorLines, "ProcessLogErrorLines");

                var headerSummary = new
                {
                    LookBackMinutes = lookBackMinutes,
                    ProcessLogErrors = processLogErrorLines.Count,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }

        public static void NotifyGetProcessLogErrors(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ProcessLogErrorsResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = $@"BI [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}] ProcessLog Error Monitor";
                if (response.HasErrors()) subject = $@"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"], ConfigurationManager.AppSettings["MailFromAddress"], toConfigAddresses, null, subject, xslOutput);
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

        public static ProcessResponse GetSSISLogErrors(int lookBackSeconds)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("E1 Get SSIS Log Errors");
            var cnSSIS = new SqlConnection();
            try
            {
                cnSSIS = GetSSISSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnSSIS", cnSSIS.ConnectionString));
                var step1GetSSISLogErrors = new ProcessStep("Step 1. Get SSIS Log Errors");
                var ssisLogErrors = SSISLogManager.GetSSISLogErrors(cnSSIS, lookBackSeconds);
                step1GetSSISLogErrors.RowsAffected = ssisLogErrors.Count;
                step1GetSSISLogErrors.Done();
                response.ProcessSteps.Add(step1GetSSISLogErrors);
                if (ssisLogErrors.Any())
                {
                    response.TransferStatus = "Y";
                    SummarizeGetSSISLogErrors(response, lookBackSeconds, ssisLogErrors);
                    NotifyGetSSISLogErrors(response);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                cnSSIS.Close();
                response.Done();
                Logging.InsertProcessLog(response);
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return response;
        }

        public static void SummarizeGetSSISLogErrors(ProcessResponse response, int lookBackSeconds, SSISLogErrors ssisLogErrors)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var ssisLogErrorLines = ssisLogErrors.ToList();
                response.SerializedObjects.Add("SSISLogErrorLine", ssisLogErrorLines, "SSISLogErrorLines");

                var headerSummary = new
                {
                    LookBackSeconds = lookBackSeconds,
                    SSISLogErrors = ssisLogErrorLines.Count,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }

        public static void NotifyGetSSISLogErrors(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailSSISConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.SSISLogErrorsResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = $@"BI [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}] SSIS Log Error Monitor";
                if (response.HasErrors()) subject = $@"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"], ConfigurationManager.AppSettings["MailFromAddress"], toConfigAddresses, null, subject, xslOutput);
                Logger.Info($@"SSISLogErrors email sent to {toConfigAddresses}");
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

        public static ProcessResponses GetSSISLogExecutions(int lookBackSeconds)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var responses = new ProcessResponses();
            var cnSSIS = new SqlConnection();
            try
            {
                cnSSIS = GetSSISSqlConnection();
                var step1GetSSISLogExecutions = new ProcessStep("Step 1. Get SSIS Log Executions");
                var ssisLogExecutions = SSISLogManager.GetSSISLogExecutions(cnSSIS, lookBackSeconds);
                step1GetSSISLogExecutions.RowsAffected = ssisLogExecutions.Count;
                step1GetSSISLogExecutions.Done();

                if (ssisLogExecutions.Any())
                {
                    foreach (var execution in ssisLogExecutions)
                    {
                        var response = new ProcessResponse("Get SSIS Log Executions");
                        try
                        {
                            responses.Add(response);
                            response.TransferStatus = "Y";
                            response.ConnectionStrings.Add(new ConnectionString("cnSSIS", cnSSIS.ConnectionString));
                            response.ProcessSteps.Add(step1GetSSISLogExecutions);
                            SummarizeGetSSISLogExecutions(response, lookBackSeconds, execution);
                            NotifyGetSSISLogExecutions(response, execution.Package, execution.Result);

                        }
                        finally
                        {
                            response.Done();
                            Logging.InsertProcessLog(response);
                        }
                    }
                }
                else
                {
                    var response = new ProcessResponse("Get SSIS Log Executions");
                    responses.Add(response);
                    response.ConnectionStrings.Add(new ConnectionString("cnSSIS", cnSSIS.ConnectionString));
                    response.ProcessSteps.Add(step1GetSSISLogExecutions);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                cnSSIS.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return responses;
        }

        public static void SummarizeGetSSISLogExecutions(ProcessResponse response, int lookBackSeconds, SSISLogExecution ssisLogExecution)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                response.SerializedObjects.Add("SSISLogExecutionSummary", ssisLogExecution.Summary.ToList(), "SSISLogExecutionSteps");
                response.SerializedObjects.Add("SSISLogExecutionPackage", ssisLogExecution.Detail.ToList(), "SSISLogExecutionPackages");

                var executionSummary = new
                {
                    ssisLogExecution.Package,
                    ssisLogExecution.Result,
                    StartDate = ssisLogExecution.ExecutionStartTime,
                    ssisLogExecution.ExecutionID
                };
                response.SerializedObjects.Add(new SerializedObject("ExecutionNode", XmlSerializationUtility.SerializeAnonymousType(executionSummary, "Execution")));
                
                var parameters = new
                {
                    ssisLogExecution.UseHighWaterMarks,
                    ssisLogExecution.HighWaterOffsetMinutes,
                    ssisLogExecution.StartDate,
                    ssisLogExecution.EndDate,
                    ssisLogExecution.StartRelativeDays,
                    ssisLogExecution.EndRelativeDays
                };
                response.SerializedObjects.Add(new SerializedObject("ParameterNode", XmlSerializationUtility.SerializeAnonymousType(parameters, "Parameters")));

                var headerSummary = new
                {
                    LookBackSeconds = lookBackSeconds,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }

        public static void NotifyGetSSISLogExecutions(ProcessResponse response, string packageName, string result)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailSSISConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.SSISLogExecutionResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = $@"BI [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}] SSIS [{packageName}]";
                if (response.HasErrors() || result == "failed" || result == "ended unexpectedly") subject = $@"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"], ConfigurationManager.AppSettings["MailFromAddress"], toConfigAddresses, null, subject, xslOutput);
                Logger.Info($@"SSISLogExecutions email sent to {toConfigAddresses}");
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

        private static SqlConnection GetCorpIntSqlConnection()
        {
            var cnCorpInt = new SqlConnection();
            try
            {
                cnCorpInt = ConnectionManager.GetSqlConnectionFromKey("CORP.INTEGRATION", false);
                cnCorpInt.Open();
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnCorpInt.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{cnCorpInt.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            return cnCorpInt;
        }

        private static SqlConnection GetDWSqlConnection()
        {
            var cnDW = new SqlConnection();
            try
            {
                cnDW = ConnectionManager.GetSqlConnectionFromKey("DW", false);
                cnDW.Open();
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnDW.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{cnDW.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            return cnDW;
        }

        private static SqlConnection GetSSISSqlConnection()
        {
            var cnSSIS = new SqlConnection();
            try
            {
                cnSSIS = ConnectionManager.GetSqlConnectionFromKey("SSIS", false);
                cnSSIS.Open();
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnSSIS.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{cnSSIS.ConnectionString}  {e.Message}: {e.StackTrace}");
            }
            return cnSSIS;
        }

        public static string GetHostName(string serviceAliasName)
        {
            //var name3 = System.Net.Dns.GetHostEntry(serviceAliasName);
            var ip = Dns.GetHostEntry(serviceAliasName).AddressList[0];
            var ipHostEntry = Dns.GetHostEntry(ip);
            var hostName = ipHostEntry.HostName;
            return hostName;
        }
    }
}
