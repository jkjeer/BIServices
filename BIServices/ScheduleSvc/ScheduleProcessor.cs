using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.ShopFloor;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.ScheduleSvc
{
    public class ScheduleProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ScheduleEvents GetScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty, string options = null)
        {
            Logger.Info($@"/GetScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&Options={options}");
            var session = BusinessSystemSessionManager.GetBusinessSession();
            try
            {
                plantCode = plantCode.Replace("'", "");
                dateCode = dateCode?.Replace("'", "");
                dateProperty = dateProperty?.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                Logger.Info($@"/GetScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&Options={options}");
                return ScheduleEventsManager.GetScheduleEventSummary(session, plant, dateCode, startDate, endDate, dateProperty, options);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
            }
        }

        public List<ScheduleSummary> GetScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                var mgr = new ScheduleManager(session);
                return mgr.GetScheduleSummary(session, plant, dateCode, startDate, endDate, dateProperty);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
            }
        }

        public List<ScheduleSummary> GetSfScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cnSf = new SqlConnection();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);

                cnSf = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".SHOPFLOOR");
                cnSf.Open();

                var mgr = new ScheduleManagerShopFloor(session);
                return mgr.GetScheduleSummary(session, cnSf, plant, dateCode, startDate, endDate, dateProperty);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cnSf.Close();
            }
        }

        public static ProcessResponse GetOverdueSchedules(int lookBackMinutes)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("E1 F580020 Get Overdue Schedule Events");
            var e1Session = new BusinessSystemSession();
            try
            {
                e1Session = BusinessSystemSessionManager.GetBusinessSession();
                response.ConnectionStrings.Add(new ConnectionString("cnE1", e1Session.DbConnection.ConnectionString));
                var step1GetUnprocessedSchedules = new ProcessStep("Step 1. Get Overdue Schedules");
                var scheduleEvents = ScheduleEventsManager.GetOverdueScheduleEvents(e1Session, lookBackMinutes);
                step1GetUnprocessedSchedules.RowsAffected = scheduleEvents.Count;
                step1GetUnprocessedSchedules.Done();
                response.ProcessSteps.Add(step1GetUnprocessedSchedules);
                if (scheduleEvents.Any())
                {
                    response.TransferStatus = "Y";
                    response.Done();
                    SummarizeGetOverdueSchedules(response, lookBackMinutes, scheduleEvents);
                    NotifyGetOverdueSchedules(response, scheduleEvents);
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

        public static void SummarizeGetOverdueSchedules(ProcessResponse response, int lookBackMinutes, ScheduleEvents scheduleEvents) 
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var scheduleEventLines = scheduleEvents.ToList();
                response.SerializedObjects.Add("ScheduleEventLine", scheduleEventLines, "ScheduleEventLines");

                var headerSummary = new
                {
                    LookBackMinutes = lookBackMinutes,
                    OverdueSchedules = scheduleEventLines.Count,
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

        public static void NotifyGetOverdueSchedules(ProcessResponse response, ScheduleEvents scheduleEvents) 
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var scheduleEvent = scheduleEvents.First();
                if (scheduleEvent.ScheduleNum < 1000)
                {
                    var actionLinks = new ActionLinks();
                    actionLinks.Add(new ActionLink("Force", "Force Download ... Download with Validation Override", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/DownloadMTOSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}&Force=true"));
                    actionLinks.Add(new ActionLink("Backout", "Backout Schedule ... Initiate E1 Backout Process ... F580030 BX transaction", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/BackoutMtoSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Reset", "Reset Schedule Status ... F580020 EDSP = N", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/ResetSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Complete", "Complete Schedule Status ... F580020 EDSP = C", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/CompleteSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Cancel", "Error Schedule Status ... F580020 EDSP = E", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/CancelSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Success", "Success Schedule Status ... F580020 EDSP = Y", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/SuccessSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    response.SerializedObjects.Add("ActionLinks", actionLinks);
                }
                else
                {
                    var actionLinks = new ActionLinks();
                    actionLinks.Add(new ActionLink("Force", "Force Download ... Download with Validation Override", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/DownloadMTOSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}&Force=true"));
                    actionLinks.Add(new ActionLink("Backout", "Backout Schedule ... Initiate E1 Backout Process", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/BackoutMTSSchedule?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Reset", "Reset Schedule Status ... F580020 EDSP = N", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/ResetMTSScheduleEvent?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Complete", "Complete Schedule Status ... F580020 EDSP = C", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/CompleteMTSScheduleEvent?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Cancel", "Error Schedule Status ... F580020 EDSP = E", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/CancelMTSScheduleEvent?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    actionLinks.Add(new ActionLink("Success", "Success Schedule Status ... F580020 EDSP = Y", $"http://{Dns.GetHostName()}/IGServices/IGServices.ScheduleRestService.svc/SuccessMTSScheduleEvent?Plant={scheduleEvent.PlantCode}&ScheduleNum={scheduleEvent.ScheduleNum}"));
                    response.SerializedObjects.Add("ActionLinks", actionLinks);
                }

                var toConfigAddresses = ConfigurationManager.AppSettings["MailOverdueSchedulesToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.OverdueSchedulesResponseBody.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = $@"BI [{ConfigurationManager.AppSettings["PrimaryEnvironment"]}] E1 F580020 Overdue Schedules Monitor";
                if (response.HasErrors()) subject = $@"Error - {subject}";
                if (scheduleEvents.Count == 1)
                {
                    subject = $@"{subject} {scheduleEvent.PlantCode}-{scheduleEvent.PlantNum} {scheduleEvent.ShopFloorSystemName} Sched {scheduleEvent.ScheduleNum}";
                }
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