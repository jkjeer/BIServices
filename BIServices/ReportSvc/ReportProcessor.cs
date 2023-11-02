using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using NLog;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Linq;
using BIServices.Common.Utilities;
using System.Text;

namespace BIServices.ReportSvc
{
    public class ReportProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProcessResponse PBITest()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"PDI Test");
            try
            {

            }
            catch (Exception e)
            {
                response.Errors.Add(new Error(e));
            }
            response.Close();
            var endTime = DateTime.Now.ToLocalTime();
            Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");

            return response;
        }

        public ProcessResponse PublishFile(string folderName, string fileName, string workspaceNameTarget, string reportNameTarget, string parameterName, string newValue)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Publish File");
            //string decodedUrl = DecodeUrlString(filePath);
            string decodedUrl = "";
            try
            {
                newValue = newValue.ToUpper();
                var oneDrivePath = ConfigurationManager.AppSettings["PublishFileOneDrivePath"];
                var ps1FilePath = ConfigurationManager.AppSettings["PublishFilePowerShellScriptPath"];
                var ps1FileName = ConfigurationManager.AppSettings["PublishFilePowerShellScriptFileName"];
                var ps1File = $@"{ps1FilePath}\{ps1FileName}";

                decodedUrl = $@"{oneDrivePath}\{folderName}\{fileName}";

                RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
                Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
                runspace.Open();

                PowerShell ps = PowerShell.Create();
                ps.Runspace = runspace;
                ps.AddCommand(ps1File);

                ps.AddArgument(decodedUrl);
                ps.AddArgument(workspaceNameTarget);
                ps.AddArgument(reportNameTarget);
                ps.AddArgument(parameterName);
                ps.AddArgument(newValue);

                Collection<PSObject> psObjects;
                psObjects = ps.Invoke();

                runspace.Close();

                var a = psObjects.ToArray();
                var b = psObjects.ToString();
                foreach (var obj in psObjects)
                {
                    var bo = obj.BaseObject;
                    switch (bo.ToString())
                    {
                        case "Microsoft.PowerBI.Common.Abstractions.PowerBIProfile":
                            break;
                        case "Microsoft.PowerBI.Common.Api.Workspaces.Workspace":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Reports.Report":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Shared.Datasource":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Shared.DatasourceConnectionDetails":
                            // code block
                            break;
                        case "FilePath:":
                            break;
                        case "DatasourceSource:":
                            break;
                        case "WorkspaceTarget:":
                            break;
                        case "ReportTarget:":
                            break;
                        case "Before:":
                            break;
                        case "":
                            break;
                        case "After:":
                            break;
                        default:
                            var step = new ProcessStep(bo.ToString());
                            step.Done();
                            response.ProcessSteps.Add(step);
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                response.Errors.Add(new Error(e));
            }
            response.Close();
            var endTime = DateTime.Now.ToLocalTime();
            var headerSummary = new
            {
                StartDate = startTime,
                StartDateString = startTime.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                EndDate = endTime,
                EndDateString = endTime.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                ElapsedSeconds = (decimal)(endTime - startTime).TotalSeconds,
                FilePath = decodedUrl,
                WorkspaceNameTarget = workspaceNameTarget,
                ReportNameTarget = reportNameTarget,
                ParameterName = parameterName,
                NewValue = newValue
            };
            response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            NotifyPublishFile(response);
            Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");

            return response;
        }

        private static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }
        public static void SummarizePublishFile(ProcessResponse response)
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

        public static void NotifyPublishFile(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["PublishFileMailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.PublishFileResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Publish File";
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

        public ProcessResponse PublishReport(string workspaceNameSource, string reportNameSource, string workspaceNameTarget, string reportNameTarget, string parameterName, string newValue)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Publish Report");
            try
            {
                newValue = newValue.ToUpper();
                var ps1FilePath = ConfigurationManager.AppSettings["PublishReportPowerShellScriptPath"];
                var ps1FileName = ConfigurationManager.AppSettings["PublishReportPowerShellScriptFileName"];
                var ps1File = $@"{ps1FilePath}\{ps1FileName}";

                RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
                Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
                runspace.Open();

                PowerShell ps = PowerShell.Create();
                ps.Runspace = runspace;
                ps.AddCommand(ps1File);

                ps.AddArgument(workspaceNameSource);
                ps.AddArgument(reportNameSource);
                ps.AddArgument(workspaceNameTarget);
                ps.AddArgument(reportNameTarget);
                ps.AddArgument(parameterName);
                ps.AddArgument(newValue);

                Collection<PSObject> psObjects;
                psObjects = ps.Invoke();

                var a = psObjects.ToArray();
                var b = psObjects.ToString();
                foreach (var obj in psObjects)
                {
                    var bo = obj.BaseObject;
                    switch (bo.ToString())
                    {
                        case "Microsoft.PowerBI.Common.Abstractions.PowerBIProfile":
                            break;
                        case "Microsoft.PowerBI.Common.Api.Workspaces.Workspace":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Reports.Report":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Shared.Datasource":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Shared.DatasourceConnectionDetails":
                            // code block
                            break;
                        case "WorkspaceSource:":
                            break;
                        case "ReportSource:":
                            break;
                        case "DatasourceSource:":
                            break;
                        case "WorkspaceTarget:":
                            break;
                        case "ReportTarget:":
                            break;
                        case "Before:":
                            break;
                        case "":
                            break;
                        case "After:":
                            break;
                        default:
                            var step = new ProcessStep(bo.ToString());
                            step.Done();
                            response.ProcessSteps.Add(step);
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                response.Errors.Add(new Error(e));
            }
            response.Close();
            var endTime = DateTime.Now.ToLocalTime();
            var headerSummary = new
            {
                StartDate = startTime,
                StartDateString = startTime.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                EndDate = endTime,
                EndDateString = endTime.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                ElapsedSeconds = (decimal)(endTime - startTime).TotalSeconds,
                WorkspaceNameSource = workspaceNameSource,
                ReportNameSource = reportNameSource,
                WorkspaceNameTarget = workspaceNameTarget,
                ReportNameTarget = reportNameTarget,
                ParameterName = parameterName,
                NewValue = newValue
            };
            response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            NotifyPublishReport(response);
            Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");

            return response;
        }

        public static void SummarizePublishReport(ProcessResponse response)
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

        public static void NotifyPublishReport(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["PublishReportMailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.PublishReportResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Publish Report";
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

        public ProcessResponse UpdateReportParameter(string workspaceName, string reportName, string parameterName, string newValue)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Update Report Parameter");
            try
            {
                newValue = newValue.ToUpper();
                var ps1FilePath = ConfigurationManager.AppSettings["ReportUpdatePowerShellScriptPath"];
                var ps1FileName = ConfigurationManager.AppSettings["ReportUpdatePowerShellScriptFileName"];
                var ps1File = $@"{ps1FilePath}\{ps1FileName}";

                RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
                Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
                runspace.Open();

                PowerShell ps = PowerShell.Create();
                ps.Runspace = runspace;
                ps.AddCommand(ps1File);

                ps.AddArgument(workspaceName);
                ps.AddArgument(reportName);
                ps.AddArgument(parameterName);
                ps.AddArgument(newValue);

                Collection<PSObject> psObjects;
                psObjects = ps.Invoke();

                var a = psObjects.ToArray();
                var b = psObjects.ToString();
                foreach (var obj in psObjects)
                {
                    var bo = obj.BaseObject;
                    switch (bo.ToString())
                    {
                        case "Microsoft.PowerBI.Common.Abstractions.PowerBIProfile":
                            break;
                        case "Microsoft.PowerBI.Common.Api.Workspaces.Workspace":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Reports.Report":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Shared.Datasource":
                            // code block
                            break;
                        case "Microsoft.PowerBI.Common.Api.Shared.DatasourceConnectionDetails":
                            // code block
                            break;
                        case "Workspace:":
                            break;
                        case "Report:":
                            break;
                        case "Datasource:":
                            break;
                        case "Before:":
                            break;
                        case "":
                            break;
                        case "After:":
                            break;
                        default:
                            var step = new ProcessStep(bo.ToString());
                            step.Done();
                            response.ProcessSteps.Add(step);
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                response.Errors.Add(new Error(e));
            }
            response.Close();
            var endTime = DateTime.Now.ToLocalTime();
            var headerSummary = new
            {
                StartDate = startTime,
                StartDateString = startTime.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                EndDate = endTime,
                EndDateString = endTime.ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                ElapsedSeconds = (decimal)(endTime - startTime).TotalSeconds,
                WorkspaceName = workspaceName,
                ReportName = reportName,
                ParameterName = parameterName,
                NewValue = newValue
            };
            response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
            NotifyUpdateReportParameter(response);
            Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");

            return response;
        }

        public static void SummarizeUpdateReportParameter(ProcessResponse response)
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

        public static void NotifyUpdateReportParameter(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["ReportUpdateMailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.UpdateReportParameterResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Update Report Parameter";
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


    }
}