using System;
using System.Configuration;
using System.Data.Services.Client;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using KMI.Orm.Linq;
using NLog;
using VelocityApp.Common.Managers;
using VelocityApp.Common.Types;
using VelocityApp.Common.Utilities;

// ReSharper disable UseObjectOrCollectionInitializer

namespace VelocityApp
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var response = new ProcessResponse("Download Velocity Data");
            var cnDW = GetDWSqlConnection();
            response.ConnectionStrings.Add(new ConnectionString("cnDW", cnDW.ConnectionString));

            // Define the URI of the Velocity OData service.
            var velocityUri = new Uri(ConfigurationManager.AppSettings["Velocity.ServiceUri"], UriKind.Absolute);
            response.ConnectionStrings.Add(new ConnectionString("velocityUri", velocityUri.AbsoluteUri));
            
            // Create a new instance of the typed DataServiceContext.
            var context = new KMIEHS(velocityUri);
            context.SendingRequest2 += SendBaseAuthCredsOnTheRequest;
            try
            {
                var lookback = DateTime.Now.ToLocalTime().AddDays(-7);
                response.LookbackDate = lookback;

                var step1GetAttribute = new ProcessStep("Step 1. Get Attribute");
                step1GetAttribute.RowsAffected = AttributeDownloadManager.DownloadAttribute(cnDW, context);
                step1GetAttribute.Done();
                response.ProcessSteps.Add(step1GetAttribute);

                var step2GetPeriodInfo = new ProcessStep("Step 2. Get Incident");
                step2GetPeriodInfo.RowsAffected = IncidentDownloadManager.DownloadIncident(cnDW, context, lookback);
                step2GetPeriodInfo.Done();
                response.ProcessSteps.Add(step2GetPeriodInfo);

                var step3GetPeriodInfo = new ProcessStep("Step 3. Get IncidentHasListItem");
                step3GetPeriodInfo.RowsAffected = IncidentHasListItemDownloadManager.DownloadIncidentHasListItem(cnDW, context);
                step3GetPeriodInfo.Done();
                response.ProcessSteps.Add(step3GetPeriodInfo);

                var step4GetPeriodInfo = new ProcessStep("Step 4. Get IncidentHasType");
                step4GetPeriodInfo.RowsAffected = IncidentHasTypeDownloadManager.DownloadIncidentHasType(cnDW, context);
                step4GetPeriodInfo.Done();
                response.ProcessSteps.Add(step4GetPeriodInfo);

                var step5GetPeriodInfo = new ProcessStep("Step 5. Get IncidentInjury");
                step5GetPeriodInfo.RowsAffected = IncidentInjuryDownloadManager.DownloadIncidentInjury(cnDW, context);
                step5GetPeriodInfo.Done();
                response.ProcessSteps.Add(step5GetPeriodInfo);

                var step6GetPeriodInfo = new ProcessStep("Step 6. Get IncidentTypeHasAgency");
                step6GetPeriodInfo.RowsAffected = IncidentTypeHasAgencyDownloadManager.DownloadIncidentTypeHasAgency(cnDW, context);
                step6GetPeriodInfo.Done();
                response.ProcessSteps.Add(step6GetPeriodInfo);

                var step7GetPeriodInfo = new ProcessStep("Step 7. Get ListItem");
                step7GetPeriodInfo.RowsAffected = ListItemDownloadManager.DownloadListItem(cnDW, context, lookback);
                step7GetPeriodInfo.Done();
                response.ProcessSteps.Add(step7GetPeriodInfo);

                var step8GetPeriodInfo = new ProcessStep("Step 8. Get Location");
                step8GetPeriodInfo.RowsAffected = LocationDownloadManager.DownloadLocation(cnDW, context, lookback);
                step8GetPeriodInfo.Done();
                response.ProcessSteps.Add(step8GetPeriodInfo);

                var step9GetPeriodInfo = new ProcessStep("Step 9. Get LocationHasPeriodInfo");
                step9GetPeriodInfo.RowsAffected = LocationHasPeriodInfoDownloadManager.DownloadLocationHasPeriodInfo(cnDW, context);
                step9GetPeriodInfo.Done();
                response.ProcessSteps.Add(step9GetPeriodInfo);

                var step10GetPeriodInfo = new ProcessStep("Step 10. Get PeriodInfo");
                step10GetPeriodInfo.RowsAffected = PeriodInfoDownloadManager.DownloadPeriodInfo(cnDW, context);
                step10GetPeriodInfo.Done();
                response.ProcessSteps.Add(step10GetPeriodInfo);

                var step11GetIncidentHasAttribute = new ProcessStep("Step 11. Get IncidentHasAttribute");
                step11GetIncidentHasAttribute.RowsAffected = IncidentHasAttributeDownloadManager.DownloadIncidentHasAttribute(cnDW, context);
                step11GetIncidentHasAttribute.Done();
                response.ProcessSteps.Add(step11GetIncidentHasAttribute);

                var step12GetIncidentHasPeriodInfo = new ProcessStep("Step 12. Get IncidentHasPeriodInfo");
                step12GetIncidentHasPeriodInfo.RowsAffected = IncidentHasPeriodInfoDownloadManager.DownloadIncidentHasPeriodInfo(cnDW, context);
                step12GetIncidentHasPeriodInfo.Done();
                response.ProcessSteps.Add(step12GetIncidentHasPeriodInfo);

                var step13EnrichPlantHours = new ProcessStep("Step 13. Enrich PlantHours");
                step13EnrichPlantHours.RowsAffected = EnrichManager.LoadPlantHours(cnDW);
                step13EnrichPlantHours.Done();
                response.ProcessSteps.Add(step13EnrichPlantHours);

                var step14EnrichRestrictedDays = new ProcessStep("Step 14. Enrich RestrictedDays");
                step14EnrichRestrictedDays.RowsAffected = EnrichManager.LoadRestrictedDays(cnDW);
                step14EnrichRestrictedDays.Done();
                response.ProcessSteps.Add(step14EnrichRestrictedDays);

                var step15EnrichLostDays = new ProcessStep("Step 15. Enrich LostDays");
                step15EnrichLostDays.RowsAffected = EnrichManager.LoadLostDays(cnDW);
                step15EnrichLostDays.Done();
                response.ProcessSteps.Add(step15EnrichLostDays);

                var step16EnrichPlantDim = new ProcessStep("Step 16. Enrich PlantDimension");
                step16EnrichPlantDim.RowsAffected = EnrichManager.LoadPlantDim(cnDW);
                step16EnrichPlantDim.Done();
                response.ProcessSteps.Add(step16EnrichPlantDim);

                var step17EnrichIncidentRates = new ProcessStep("Step 17. Enrich INCIDENT_RATES");
                step17EnrichIncidentRates.RowsAffected = EnrichManager.LoadIncidentRates(cnDW);
                step17EnrichIncidentRates.Done();
                response.ProcessSteps.Add(step17EnrichIncidentRates);

                response.TransferStatus = "Y";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                response.Errors.Add(new Error(e));
                response.TransferStatus = "E";
                //Console.ReadLine();  // For testing only.  Comment out before deployment.
            }
            finally
            {
                cnDW.Close();
                response.Done();
                SummarizeVelocityDownload(response);
                NotifyVelocityDownload(response);
            }
        }

        public static void NotifyVelocityDownload(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses1 = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var toAddrs1 = $"{toConfigAddresses1}";
                var serializedXml = response.Serialize();
                var xslOutput = XsltManager.Transform(serializedXml, "VelocityApp.Common.Xslt.VelocityDownloadReport.xslt", Encoding.UTF8);
                var subject = response.ProcessName + " [" + ConfigurationManager.AppSettings["Environment"] + "]";
                if (response.HasErrors()) subject = "Error - " + subject;
                Mail.SendHtmlAsync(ConfigurationManager.AppSettings["SmtpHost"],
                    ConfigurationManager.AppSettings["MailFromAddress"],
                    toAddrs1,
                    null,
                    subject,
                    xslOutput);
            }
            catch (Exception e)
            {
                stepSendMail.Errors.Add(new Error(e));
            }
            finally
            {
                stepSendMail.Done();
                response.ProcessSteps.Add(stepSendMail);
            }
        }

        public static void SummarizeVelocityDownload(ProcessResponse response)
        {
            var stepSummarizeDownload = new ProcessStep("Download Summary Collections");
            try
            {
                var transferSummary = new
                {
                    response.TransferStatus,
                    response.TransferDate,
                    HasErrors = response.HasErrors(),
                    response.TransferMessage,
                    response.LookbackDate,
                    LookbackDays = DateTime.Now.ToLocalTime().Subtract(response.LookbackDate).Days,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                };
                response.SerializedObjects.Add(new SerializedObject("TransferNode", XmlSerializationUtility.SerializeAnonymousType(transferSummary, "Transfer")));
            }
            catch (Exception e)
            {
                stepSummarizeDownload.Errors.Add(new Error(e));
            }
            finally
            {
                stepSummarizeDownload.Done();
                response.ProcessSteps.Add(stepSummarizeDownload);
            }
        }


        private static void SendBaseAuthCredsOnTheRequest(object sender, SendingRequest2EventArgs e)
        {
            var username = ConfigurationManager.AppSettings["Velocity.UserName"];
            var password = ConfigurationManager.AppSettings["Velocity.Password"];
            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            e.RequestMessage.SetHeader("Authorization", "Basic " + authHeaderValue); //this is where you pass the creds.
        }

        private static SqlConnection GetDWSqlConnection()
        {
            var cnDW = new SqlConnection();
            try
            {
                cnDW = ConnectionManager.GetSqlConnection(ConfigurationManager.ConnectionStrings["DW"].ConnectionString);
                cnDW.Open();
            }
            catch (Exception e)
            {
                throw new ApplicationException(cnDW.ConnectionString + "  " + e);
            }
            return cnDW;
        }
    }
}
