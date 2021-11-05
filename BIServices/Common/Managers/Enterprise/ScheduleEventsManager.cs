using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Enterprise
{
    public class ScheduleEventsManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ScheduleEvents GetScheduleEventSummary(BusinessSystemSession session, Plant plant, string dateCode, string startDate, string endDate, string dateProperty, string options)
        {
            ScheduleEvents scheduleEvents;
            var filters = new List<string>();
            var parameters = new List<iDB2Parameter>();
            try
            {
                filters.Add("S6MCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);
                DateTime startDateValue;
                DateTime endDateValue;
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) ||
                    !string.IsNullOrEmpty(dateCode))
                {
                    if (!string.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        startDateValue = dateValues[0];
                        endDateValue = dateValues[1];
                    }
                    else
                    {
                        startDateValue = !string.IsNullOrEmpty(startDate) ? DateCodeManager.GetDateCodeValue(startDate) : new DateTime(1900, 1, 1);
                        endDateValue = !string.IsNullOrEmpty(endDate) ? DateCodeManager.GetDateCodeValue(endDate) : new DateTime(2050, 12, 31);
                    }
                }
                else
                {
                    startDateValue = DateTime.Now.Date;
                    endDateValue = DateTime.Now.Date;
                }
                if (!string.IsNullOrEmpty(dateProperty))
                {
                    switch (dateProperty.ToUpper())
                    {
                        case "DUE":
                        case "REQ":
                        case "REQUEST":
                        case "REQUESTED":
                            filters.Add("S7Z58SCDDJ >= ? AND\r\n");
                            filters.Add("S7Z58SCDDJ <= ? AND\r\n");
                            break;
                        case "SCH":
                        case "SCHEDULE":
                        case "SCHEDULED":
                            filters.Add("S6Z58SCD >= ? AND\r\n");
                            filters.Add("S6Z58SCD <= ? AND\r\n");
                            break;
                        default:
                            filters.Add("S6Z58SCD >= ? AND\r\n");
                            filters.Add("S1Z58SCD <= ? AND\r\n");
                            break;
                    }
                }
                else
                {
                    filters.Add("S6Z58SCD >= ? AND\r\n");
                    filters.Add("S6Z58SCD <= ? AND\r\n");
                }
                parameters.Add(new iDB2Parameter("StartDate", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = Utilities.Common.CastToE1Date(startDateValue);
                parameters.Add(new iDB2Parameter("EndDate", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = Utilities.Common.CastToE1Date(endDateValue);
                if (!string.IsNullOrEmpty(options))
                {
                    filters.Add($"{options.ToUpper()} AND\r\n");
                }
                filters[filters.Count - 1] = filters.Last().Replace(" AND\r\n", "");
                iDB2Command cmd = F580020Manager.GetF580020Status(filters);
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                Logger.Info(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                { 
                    var f580020s = new F580020s();
                    while (reader.Read())
                    {
                        var f580020 = new F580020();
                        f580020.S6EDBT = CastDbNull.To(reader["S6EDBT"], string.Empty).Trim();
                        f580020.S6MCU = CastDbNull.To(reader["S6MCU"], string.Empty).Trim();
                        f580020.S6Z58SCN = CastDbNull.To<decimal>(reader["S6Z58SCN"], 0);
                        f580020.S6Z58SCD = CastDbNull.To<decimal>(reader["S6Z58SCD"], 0);
                        f580020.S6Z58SFS = CastDbNull.To(reader["S6Z58SFS"], string.Empty).Trim();
                        f580020.S6TSC = CastDbNull.To(reader["S6TSC"], string.Empty).Trim();
                        f580020.S6EDSP = CastDbNull.To(reader["S6EDSP"], string.Empty).Trim();
                        f580020.S6MATH06 = CastDbNull.To<decimal>(reader["S6MATH06"], 0);
                        f580020.S6WOPST = CastDbNull.To(reader["S6WOPST"], string.Empty).Trim();
                        f580020.S6DS50 = CastDbNull.To(reader["S6DS50"], string.Empty).Trim();
                        f580020.S6UPMJ = CastDbNull.To<decimal>(reader["S7UPMJ"], 0);
                        f580020.S6TDAY = CastDbNull.To<decimal>(reader["S7TDAY"], 0);
                        f580020.S7UPMJ = CastDbNull.To<decimal>(reader["S7UPMJ"], 0);
                        f580020.S7TDAY = CastDbNull.To<decimal>(reader["S7TDAY"], 0);
                        f580020.S7Z58SCTYP = CastDbNull.To(reader["S7Z58SCTYP"], string.Empty).Trim();
                        f580020.S7SHAN = CastDbNull.To<decimal>(reader["S7SHAN"], 0);
                        f580020.S7ALPH = CastDbNull.To(reader["S7ALPH"], string.Empty).Trim();
                        f580020.S7Z58SCDDJ = CastDbNull.To<decimal>(reader["S7Z58SCDDJ"], 0);
                        f580020.Quantity = CastDbNull.To<decimal>(reader["QUANTITY"], 0);
                        f580020.DRDL01 = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                        f580020.WOCount = CastDbNull.To(reader["WOCount"], 0);
                        f580020s.Add(f580020);
                    }
                    reader.Close();
                    Logger.Trace($"ROWS: {f580020s.Count}");
                    scheduleEvents = BuildScheduleStatusEventsFromF580020s(f580020s);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return scheduleEvents;
        }

        public static ScheduleEvents GetOverdueScheduleEvents(BusinessSystemSession e1Session, int lookBackMinutes)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var scheduleEvents = new ScheduleEvents();
            try
            {
                var f580020s = F580020Manager.GetOverdueF580020s(e1Session, lookBackMinutes);
                if (f580020s.Any())
                {
                    scheduleEvents = BuildScheduleEventsFromF580020s(f580020s);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return scheduleEvents;
        }

        public static ScheduleEvents BuildScheduleStatusEventsFromF580020s(F580020s f580020s)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var scheduleEvents = new ScheduleEvents();
            try
            {
                foreach (var f580020 in f580020s)
                {
                    var plant = PlantManager.GetPlant(f580020.S6MCU);
                    var scheduleEvent = new ScheduleEvent
                    {
                        BatchNum = f580020.S6EDBT,
                        CostCenter = f580020.S6MCU,
                        ScheduleNum = f580020.S6Z58SCN,
                        ScheduleDateE1 = f580020.S6Z58SCD,
                        ScheduleDate = Utilities.Common.CastToDateTime(f580020.S6Z58SCD),
                        ScheduleDueDateE1 = f580020.S7Z58SCDDJ,
                        ScheduleDueDate = Utilities.Common.CastToDateTime(f580020.S7Z58SCDDJ),
                        ShopFloorSystem = f580020.S6Z58SFS,
                        ScheduleType = f580020.S7Z58SCTYP,
                        StatusCode = f580020.S6TSC,
                        ProcessFlag = f580020.S6EDSP,
                        MathNumeric06 = f580020.S6MATH06,
                        OperationStatus = f580020.S6WOPST,
                        Description = f580020.S6DS50,
                        PlantNum = plant.PlantNum,
                        PlantCode = plant.PlantCode,
                        Location = $@"{plant.PlantCode} - {plant.PlantNum}",
                        UpdateDateTimeS6 = Utilities.Common.CastToDateTime(f580020.S6UPMJ, f580020.S6TDAY),
                        UpdateDateE1S6 = f580020.S6UPMJ,
                        UpdateTimeE1S6 = f580020.S6TDAY,
                        UpdateDateTimeS7 = Utilities.Common.CastToDateTime(f580020.S7UPMJ, f580020.S7TDAY),
                        UpdateDateE1S7 = f580020.S7UPMJ,
                        UpdateTimeE1S7 = f580020.S7TDAY,
                        UpdateDateTimeString = Utilities.Common.CastToDateTime(f580020.S6UPMJ, f580020.S6TDAY).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                        Age = Math.Round((decimal) (DateTime.Now - Utilities.Common.CastToDateTime(f580020.S6UPMJ, f580020.S6TDAY)).TotalMinutes, 0),
                        ShopFloorSystemName = f580020.DRDL01,
                        WorkOrderCount = f580020.WOCount,
                        Quantity = f580020.Quantity,
                        QuantityScheduled = f580020.S1Z58TQSC,
                        ScheduledWorkorders = f580020.ScheduledWorkorders,
                        //ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/DownloadMTOIncomingOrdersTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}&Force=true",
                        //CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CancelMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}"
                    };
                    scheduleEvents.Add(scheduleEvent);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{ e.Message}: { e.StackTrace}");
            }
            finally
            {
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return scheduleEvents;
        }

        public static ScheduleEvents BuildScheduleEventsFromF580020s(F580020s f580020s)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var scheduleEvents = new ScheduleEvents();
            try
            {
                foreach (var f580020 in f580020s)
                {
                    var plant = PlantManager.GetPlant(f580020.S6MCU);
                    var scheduleEvent = new ScheduleEvent
                    {
                        BatchNum = f580020.S6EDBT,
                        CostCenter = f580020.S6MCU,
                        ScheduleNum = f580020.S6Z58SCN,
                        ScheduleDateE1 = f580020.S6Z58SCD,
                        ScheduleDate = Utilities.Common.CastToDateTime(f580020.S6Z58SCD),
                        ShopFloorSystem = f580020.S6Z58SFS,
                        StatusCode = f580020.S6TSC,
                        ProcessFlag = f580020.S6EDSP,
                        MathNumeric06 = f580020.S6MATH06,
                        OperationStatus = f580020.S6WOPST,
                        Description = f580020.S6DS50,
                        PlantNum = plant.PlantNum,
                        PlantCode = plant.PlantCode,
                        Location = $@"{plant.PlantCode} - {plant.PlantNum}",
                        UpdateDateTimeS6 = Utilities.Common.CastToDateTime(f580020.S6UPMJ, f580020.S6TDAY),
                        UpdateDateE1S6 = f580020.S6UPMJ,
                        UpdateTimeE1S6 = f580020.S6TDAY,
                        UpdateDateTimeS7 = Utilities.Common.CastToDateTime(f580020.S7UPMJ, f580020.S7TDAY),
                        UpdateDateE1S7 = f580020.S7UPMJ,
                        UpdateTimeE1S7 = f580020.S7TDAY,
                        UpdateDateTimeString = Utilities.Common.CastToDateTime(f580020.S6UPMJ, f580020.S6TDAY).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                        Age = Math.Round((decimal)(DateTime.Now - Utilities.Common.CastToDateTime(f580020.S6UPMJ, f580020.S6TDAY)).TotalMinutes, 0),
                        ShopFloorSystemName = f580020.DRDL01,
                        WorkOrderCount = f580020.WOCount,
                        QuantityScheduled = f580020.S1Z58TQSC,
                        ScheduledWorkorders = f580020.ScheduledWorkorders
                    };
                    if (scheduleEvent.QuantityScheduled > 0)
                    {
                        scheduleEvent.RemainingMinutes = ((scheduleEvent.Age * scheduleEvent.WorkOrderCount) / scheduleEvent.QuantityScheduled) - scheduleEvent.Age;
                    }
                    switch (f580020.S6Z58SFS)
                    {
                        case "1": // CG MTO
                            if (plant.PlantCode.EndsWith("CT"))
                            {
                                scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CTServices/CTServices.ScheduleRestService.svc/DownloadMTOIncomingOrdersTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}&Force=true";
                                scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CTServices/CTServices.ScheduleRestService.svc/CancelMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                                scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CTServices/CTServices.ScheduleRestService.svc/ResetMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                                scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CTServices/CTServices.ScheduleRestService.svc/SuccessMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                                scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CTServices/CTServices.ScheduleRestService.svc/CompleteMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            }
                            else
                            {
                                scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/DownloadMTOIncomingOrdersTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}&Force=true";
                                scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/CancelMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                                scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/ResetMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                                scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/SuccessMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                                scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/CompleteMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            }
                            break;
                        case "2": // IG MTO
                            scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/DownloadMTOIncomingOrdersTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}&Force=true";
                            scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CancelMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/ResetMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/SuccessMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CompleteMTOScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            break;
                        case "3": // CG HV
                            scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/DownloadHVQueTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/CancelMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/ResetMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/SuccessMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/CompleteMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            break;
                        case "4": // FG 
                            break;
                        case "5": // TG 
                            break;
                        case "6": // IG MTS
                            scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/DownloadScheduleInfoMTSTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}&Force=true";
                            scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CancelMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/ResetMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/SuccessMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CompleteMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            break;
                        case "7": // IG MTS Cut in House
                            scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/DownloadScheduleInfoMTSTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}&Force=true";
                            scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CancelMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/ResetMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/SuccessMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/IGServices/IGServices.ScheduleRestService.svc/CompleteMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            break;
                        case "8": // ST
                            scheduleEvent.ForceLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/DownloadOutsideHVQueTable?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CancelLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/CancelMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.ResetLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/ResetMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.SuccessLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/SuccessMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            scheduleEvent.CompleteLink = $"http://svc{ConfigurationManager.AppSettings["PrimaryEnvironment"]}/CGServices/CGServices.ScheduleRestService.svc/CompleteMTSScheduleEvent?PlantCode={plant.PlantCode}&ScheduleNum={f580020.S6Z58SCN}";
                            break;
                        case "9": // LG
                            break;
                    }
                    scheduleEvents.Add(scheduleEvent);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{ e.Message}: { e.StackTrace}");
            }
            finally
            {
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }
            return scheduleEvents;
        }
    }
}
