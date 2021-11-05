using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.Archie;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

namespace BIServices.ArchieSvc
{
    public class APInvoiceProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse ImportAPInvoices(bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"Import AP Invoices: BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Import AP Invoices {ConfigurationManager.AppSettings["Environment"]}:", true);
            var cnTCC = new SqlConnection();
            var cnTCCSUPPORT = new SqlConnection();
            try
            {
                cnTCC = GetTCCSqlConnection();
                cnTCCSUPPORT = GetTCCSUPPORTSqlConnection();
                response = APInvoiceManager.ImportAPInvoices(cnTCC, cnTCCSUPPORT, verbose);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnTCC.ConnectionString}  {e.Message}");
                response.Errors.Add(new Error($"ERROR: {cnTCC.ConnectionString}  {e.Message}"));
                response.Close();
                SummarizeAPInvoiceImport(response);
                NotifyAPInvoiceImport(response);
            }
            finally
            {
                cnTCC.Close();
                cnTCCSUPPORT.Close();
                Logger.Info($"Import AP Invoices: END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
                if (response.HasErrors() || response.Messages.Any() || verbose)
                {
                    NotifyAPInvoiceImport(response);
                }
                Logging.InsertProcessLog(response);
            }
            return response;
        }

        public static void SummarizeAPInvoiceImport(ProcessResponse response)
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

        public static void NotifyAPInvoiceImport(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.APInvoiceImport.xslt", Encoding.UTF8);
                Logger.Info($"Report XML: {xslOutput}");
                var subject = $@"AP Invoice Importer [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}]";
                if (response.HasErrors())
                {
                    subject = $"Error - {subject}";

                }
                else if (response.Messages.Any())
                {
                    subject = $"Warning - {subject}";
                }
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

        private static SqlConnection GetTCCSqlConnection()
        {
            var cnTCC = new SqlConnection();
            try
            {
                cnTCC = ConnectionManager.GetSqlConnectionFromKey(@"TCC");
                cnTCC.Open();
            }
            catch (Exception e)
            {
                //Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"TCCSUPPORT {cnTCC.ConnectionString }  {e.Message}: {e.StackTrace}");
            }
            return cnTCC;
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
