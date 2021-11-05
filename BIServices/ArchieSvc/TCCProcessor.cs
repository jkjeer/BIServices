using System;
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
    public class TCCProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse TCCDeleteDocs(int rows, bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"TCC Delete Docs: {rows} Rows BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"TCC Delete Docs {ConfigurationManager.AppSettings["PrimaryEnvironment"]}: {rows} Rows", true);
            var cnTCCSUPPORT = new SqlConnection();
            try
            {
                cnTCCSUPPORT = GetTCCSUPPORTSqlConnection();
                response = TCCManager.TCCDeleteDocs(cnTCCSUPPORT, rows, verbose);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTCCSUPPORT.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTCCSUPPORT.ConnectionString}  {e.Message}"));
                SummarizeTCCDeleteDocs(response);
                NotifyTCCDeleteDocs(response);
            }
            finally
            {
                cnTCCSUPPORT.Close();
                response.Close();
                Logger.Info($"TCC Delete Docs: {rows} Rows END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (response.HasErrors() || verbose) NotifyTCCDeleteDocs(response);                
                Logging.InsertProcessLog(response);
            }
            return response;
        }

        public static ProcessResponse TCCFindNewCategories(bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"TCC Find New Categories: BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"TCC Find New Categories {ConfigurationManager.AppSettings["PrimaryEnvironment"]}", true);
            var cnTCCSUPPORT = new SqlConnection();
            try
            {
                cnTCCSUPPORT = GetTCCSUPPORTSqlConnection();
                response = TCCManager.TCCFindNewCategories(cnTCCSUPPORT, verbose);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTCCSUPPORT.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTCCSUPPORT.ConnectionString}  {e.Message}"));
                SummarizeTCCFindNewCategories(response);
                NotifyTCCFindNewCategories(response);
            }
            finally
            {
                cnTCCSUPPORT.Close();
                response.Close();
                Logger.Info($"TCC Find New Categories END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (response.RowsAffected > 0 || response.HasErrors() || verbose) NotifyTCCFindNewCategories(response);                
                Logging.InsertProcessLog(response);
            }
            return response;
        }

        public static ProcessResponse UpdateTCCDeleteDate(int rows, bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Update TCC Delete Date: {rows} Rows BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Update TCC Delete Date {ConfigurationManager.AppSettings["PrimaryEnvironment"]}: {rows} Rows", true);
            var cnTCCSUPPORT = new SqlConnection();
            try
            {
                cnTCCSUPPORT = GetTCCSUPPORTSqlConnection();
                response = TCCManager.UpdateTCCDeleteDate(cnTCCSUPPORT, rows, verbose);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTCCSUPPORT.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTCCSUPPORT.ConnectionString}  {e.Message}"));
                SummarizeUpdateTCCDeleteDate(response);
                NotifyUpdateTCCDeleteDate(response);
            }
            finally
            {
                cnTCCSUPPORT.Close();
                response.Close();
                Logger.Info($"Update TCC Delete Date: {rows} Rows END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (response.HasErrors() || verbose)
                {
                    NotifyUpdateTCCDeleteDate(response);
                }
                Logging.InsertProcessLog(response);
            }
            return response;
        }

        public static void SummarizeTCCDeleteDocs(ProcessResponse response)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
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
                    ElapsedSeconds = (decimal)(response.EndDate - response.StartDate).TotalSeconds
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

        public static void SummarizeTCCFindNewCategories(ProcessResponse response)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
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
                    ElapsedSeconds = (decimal)(response.EndDate - response.StartDate).TotalSeconds
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

        public static void SummarizeUpdateTCCDeleteDate(ProcessResponse response)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
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
                    ElapsedSeconds = (decimal)(response.EndDate - response.StartDate).TotalSeconds
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

        public static void NotifyTCCDeleteDocs(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailTCCToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.TCCDeleteDocs.xslt", Encoding.UTF8);
                Logger.Info($"Report XML: {xslOutput}");
                var subject = $@"TCC Delete Docs [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}]";
                if (response.HasErrors()) subject = $"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"],
                    ConfigurationManager.AppSettings["MailFromAddress"],
                    toConfigAddresses,
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

        public static void NotifyTCCFindNewCategories(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailTCCToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.TCCFindNewCategories.xslt", Encoding.UTF8);
                Logger.Info($"Report XML: {xslOutput}");
                var subject = $@"TCC Find New Categories [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}]";
                if (response.HasErrors()) subject = $"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"],
                    ConfigurationManager.AppSettings["MailFromAddress"],
                    toConfigAddresses,
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

        public static void NotifyUpdateTCCDeleteDate(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailTCCToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.UpdateTCCDeleteDate.xslt", Encoding.UTF8);
                Logger.Info($"Report XML: {xslOutput}");
                var subject = $@"Update TCC Delete Date [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}]";
                if (response.HasErrors()) subject = $"Error - {subject}";
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"],
                    ConfigurationManager.AppSettings["MailFromAddress"],
                    toConfigAddresses,
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

        private static SqlConnection GetTCCSUPPORTSqlConnection()
        {
            var cnTCCSUPPORT = new SqlConnection();
            try
            {
                cnTCCSUPPORT = ConnectionManager.GetSqlConnectionFromKey(@"TCCSUPPORT");
                cnTCCSUPPORT.Open();
            }
            catch (Exception e)
            {
                //Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"TCCSUPPORT {cnTCCSUPPORT.ConnectionString }  {e.Message}: {e.StackTrace}");
            }
            return cnTCCSUPPORT;
        }

    }
}
