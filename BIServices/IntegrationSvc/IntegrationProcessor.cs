using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.Integration;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;
using Subscriptions = BIServices.Common.Types.Integration.Subscriptions;

// ReSharper disable InconsistentNaming

namespace BIServices.IntegrationSvc
{
    public class IntegrationProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse ResetPYHighWaterMarks()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Reset PY HighWaterMarks");
            var cnPlantPY = new SqlConnection();
            var cnPlantPD = new SqlConnection();
            try
            {
                foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                {
                    var name = item.Name;
                    if (name.EndsWith("ShopFloor") || name.EndsWith("INTEGRATION"))
                    {
                        var company = name.Substring(2, 2);
                        if ((company == "IG" || company == "CG" || company == "CT") && name.EndsWith("ShopFloor"))
                        {
                            var cStrPY = item.ConnectionString;
                            var builder = new SqlConnectionStringBuilder(cStrPY);
                            var dsPY = builder.DataSource;
                            if (dsPY.EndsWith("SRV")) dsPY = dsPY.Substring(0, dsPY.Length - 3);

                            if (!dsPY.EndsWith("PY"))
                            {
                                dsPY += "PY";
                                builder.DataSource = dsPY;
                                cStrPY = builder.ConnectionString;
                            }

                            try
                            {
                                if (!builder.MultiSubnetFailover) ConnectionManager.IsPingable(builder.DataSource);
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                                throw new Exception(e.Message);
                            }

                            try
                            {
                                cnPlantPY.ConnectionString = cStrPY;
                                cnPlantPY.Open();
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"ERROR: {cnPlantPY.ConnectionString}  {e.Message}: {e.StackTrace}");
                                throw new ApplicationException($"{cnPlantPY.ConnectionString}  {e.Message}: {e.StackTrace}");
                            }

                            response.ConnectionStrings.Add(new ConnectionString(name + "PY", cStrPY));

                            builder = new SqlConnectionStringBuilder(cStrPY);
                            var dsPD = dsPY.Substring(0, dsPY.Length - 2);
                            if (dsPD.Substring(2, 2) == "IG") dsPD += "SRV";
                            builder.DataSource = dsPD;
                            var cStrPD = builder.ConnectionString;
                            try
                            {
                                if (!builder.MultiSubnetFailover) ConnectionManager.IsPingable(builder.DataSource);
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                                throw new Exception(e.Message);
                            }

                            try
                            {
                                cnPlantPD.ConnectionString = cStrPD;
                                cnPlantPD.Open();
                                response.ConnectionStrings.Add(new ConnectionString(name + "PD", cStrPD));

                                // Get PD HighWaterMarks
                                var marksPD = HighWaterMarkManager.GetHighWaterMarks(cnPlantPD);
                                cnPlantPD.Close();

                                // Apply PD HighWaterMarks to PY.
                                HighWaterMarkManager.UpdateHighWaterMarks(cnPlantPY, marksPD);
                                cnPlantPY.Close();
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"ERROR: {cnPlantPD.ConnectionString}  {e.Message}: {e.StackTrace}");
                                throw new ApplicationException($"{cnPlantPD.ConnectionString}  {e.Message}: {e.StackTrace}");
                            }
                        }
                    }
                }

                response.TransferStatus = "Y";
                response.Close();
                SummarizeResetPYHighWaterMarks(response);
                NotifyResetPYHighWaterMarks(response);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static void SummarizeResetPYHighWaterMarks(ProcessResponse response)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                //var headerSummary = new
                //{
                //    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                //    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                //    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                //    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                //    ElapsedSeconds = (decimal)(response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
                //};
                //response.SerializedObjects.Add(new SerializedObject("SummaryNode", XmlSerializationUtility.SerializeAnonymousType(headerSummary, "Header")));
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

        public static void NotifyResetPYHighWaterMarks(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ResetPYHighWaterMarks.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Reset PY HighWaterMarks";
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

        public static ProcessResponse ResetPYIntegrationQAUKID()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Reset PY Integration QAUKID");
            var e1SessionPY = new BusinessSystemSession();
            var cnCorpIntPY = new SqlConnection();
            try
            {
                e1SessionPY = BusinessSystemSessionManager.GetBusinessSessionPY();
                response.ConnectionStrings.Add(new ConnectionString("cnE1PY", e1SessionPY.DbConnection.ConnectionString));
                cnCorpIntPY = GetCorpIntPYSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnCorpIntPY", cnCorpIntPY.ConnectionString));
                var step1GetCurrentCorpIntQAUKID = new ProcessStep("Step 1. Get Current CorpInt QAUKID");
                var currentCorpIntQAUKID = IntegrationManager.GetCurrentCorpIntQAUKID(cnCorpIntPY);
                step1GetCurrentCorpIntQAUKID.RowsAffected = 1;
                step1GetCurrentCorpIntQAUKID.Done();
                response.ProcessSteps.Add(step1GetCurrentCorpIntQAUKID);
                var step2GetMaxE1PYF580030AQAUKID = new ProcessStep("Step 2. Get Max E1 PY F580030A QAUKID");
                var maxE1PYQAUKID = F580030AManager.GetMaxE1PYF580030AQAUKID(e1SessionPY);
                step2GetMaxE1PYF580030AQAUKID.RowsAffected = 1;
                step2GetMaxE1PYF580030AQAUKID.Done();
                response.ProcessSteps.Add(step2GetMaxE1PYF580030AQAUKID);
                var step3UpdateCorpIntegrationQAUKID = new ProcessStep("Step 3. Update Corp Integration QAUKID");
                var newQAUKID = maxE1PYQAUKID + 2;
                var sqlResponse = IntegrationManager.UpdateCorpIntegrationQAUKID(cnCorpIntPY, newQAUKID);
                response.SqlDmlResponses.Add(sqlResponse);
                step3UpdateCorpIntegrationQAUKID.RowsAffected = 1;
                step3UpdateCorpIntegrationQAUKID.Done();
                response.ProcessSteps.Add(step3UpdateCorpIntegrationQAUKID);
                response.TransferStatus = "Y";
                response.Close();
                SummarizeResetPYIntegrationQAUKID(response, currentCorpIntQAUKID, maxE1PYQAUKID, newQAUKID);
                NotifyResetPYIntegrationQAUKID(response);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                e1SessionPY.Close();
                cnCorpIntPY.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static ProcessResponse ResetPYIntegrationBIUKID()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Reset PY Integration BIUKID");
            var e1SessionPY = new BusinessSystemSession();
            var cnCorpIntPY = new SqlConnection();
            try
            {
                e1SessionPY = BusinessSystemSessionManager.GetBusinessSessionPY();
                response.ConnectionStrings.Add(new ConnectionString("cnE1PY", e1SessionPY.DbConnection.ConnectionString));
                cnCorpIntPY = GetCorpIntPYSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnCorpIntPY", cnCorpIntPY.ConnectionString));
                var step1GetCurrentCorpIntBIUKID = new ProcessStep("Step 1. Get Current CorpInt BIUKID");
                var currentCorpIntBIUKID = IntegrationManager.GetCurrentCorpIntBIUKID(cnCorpIntPY);
                step1GetCurrentCorpIntBIUKID.RowsAffected = 1;
                step1GetCurrentCorpIntBIUKID.Done();
                response.ProcessSteps.Add(step1GetCurrentCorpIntBIUKID);
                var step2GetMaxE1PYF580200BIUKID = new ProcessStep("Step 2. Get Max E1 PY F580200 BIUKID");
                var maxE1PYBIUKID = F580200Manager.GetMaxE1PYF580200BIUKID(e1SessionPY);
                step2GetMaxE1PYF580200BIUKID.RowsAffected = 1;
                step2GetMaxE1PYF580200BIUKID.Done();
                response.ProcessSteps.Add(step2GetMaxE1PYF580200BIUKID);
                var step3UpdateCorpIntegrationBIUKID = new ProcessStep("Step 3. Update Corp Integration BIUKID");
                var newBIUKID = maxE1PYBIUKID + 2;
                var sqlResponse = IntegrationManager.UpdateCorpIntegrationBIUKID(cnCorpIntPY, newBIUKID);
                response.SqlDmlResponses.Add(sqlResponse);
                step3UpdateCorpIntegrationBIUKID.RowsAffected = 1;
                step3UpdateCorpIntegrationBIUKID.Done();
                response.ProcessSteps.Add(step3UpdateCorpIntegrationBIUKID);
                response.TransferStatus = "Y";
                response.Close();
                SummarizeResetPYIntegrationBIUKID(response, currentCorpIntBIUKID, maxE1PYBIUKID, newBIUKID);
                NotifyResetPYIntegrationBIUKID(response);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                e1SessionPY.Close();
                cnCorpIntPY.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static ProcessResponse ResetPYIntegrationSFUKID()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Reset PY Integration SFUKID");
            var e1SessionPY = new BusinessSystemSession();
            var cnCorpIntPY = new SqlConnection();
            try
            {
                e1SessionPY = BusinessSystemSessionManager.GetBusinessSessionPY();
                response.ConnectionStrings.Add(new ConnectionString("cnE1PY", e1SessionPY.DbConnection.ConnectionString));
                cnCorpIntPY = GetCorpIntPYSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnCorpIntPY", cnCorpIntPY.ConnectionString));
                var step1GetCurrentCorpIntSFUKID = new ProcessStep("Step 1. Get Current CorpInt SFUKID");
                var currentCorpIntSFUKID = IntegrationManager.GetCurrentCorpIntSFUKID(cnCorpIntPY);
                step1GetCurrentCorpIntSFUKID.RowsAffected = 1;
                step1GetCurrentCorpIntSFUKID.Done();
                response.ProcessSteps.Add(step1GetCurrentCorpIntSFUKID);
                var step2GetMaxE1PYF580030SFUKID = new ProcessStep("Step 2. Get Max E1 PY F580030 SFUKID");
                var maxE1PYSFUKID = F580030Manager.GetMaxE1PYF580030SFUKID(e1SessionPY);
                step2GetMaxE1PYF580030SFUKID.RowsAffected = 1;
                step2GetMaxE1PYF580030SFUKID.Done();
                response.ProcessSteps.Add(step2GetMaxE1PYF580030SFUKID);
                var step3UpdateCorpIntegrationSFUKID = new ProcessStep("Step 3. Update Corp Integration SFUKID");
                var newSFUKID = maxE1PYSFUKID + 2;
                var sqlResponse = IntegrationManager.UpdateCorpIntegrationSFUKID(cnCorpIntPY, newSFUKID);
                response.SqlDmlResponses.Add(sqlResponse);
                step3UpdateCorpIntegrationSFUKID.RowsAffected = 1;
                step3UpdateCorpIntegrationSFUKID.Done();
                response.ProcessSteps.Add(step3UpdateCorpIntegrationSFUKID);
                response.TransferStatus = "Y";
                response.Close();
                SummarizeResetPYIntegrationSFUKID(response, currentCorpIntSFUKID, maxE1PYSFUKID, newSFUKID);
                NotifyResetPYIntegrationSFUKID(response);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                e1SessionPY.Close();
                cnCorpIntPY.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static ProcessResponse ResetPYIntegrationBIEDBT()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Reset PY Integration BIEDBT");
            var e1SessionPY = new BusinessSystemSession();
            var cnCorpIntPY = new SqlConnection();
            try
            {
                e1SessionPY = BusinessSystemSessionManager.GetBusinessSessionPY();
                response.ConnectionStrings.Add(new ConnectionString("cnE1PY", e1SessionPY.DbConnection.ConnectionString));
                cnCorpIntPY = GetCorpIntPYSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnCorpIntPY", cnCorpIntPY.ConnectionString));
                var step1GetCurrentCorpIntBIEDBT = new ProcessStep("Step 1. Get Current CorpInt BIEDBT");
                var currentCorpIntBIEDBT = IntegrationManager.GetCurrentCorpIntBIEDBT(cnCorpIntPY);
                step1GetCurrentCorpIntBIEDBT.RowsAffected = 1;
                step1GetCurrentCorpIntBIEDBT.Done();
                response.ProcessSteps.Add(step1GetCurrentCorpIntBIEDBT);
                var step2GetMaxE1PYF580200BIEDBT = new ProcessStep("Step 2. Get Max E1 PY F580200 BIEDBT");
                var maxE1PYBIEDBT = F580200Manager.GetMaxE1PYF580200BIEDBT(e1SessionPY);
                step2GetMaxE1PYF580200BIEDBT.RowsAffected = 1;
                step2GetMaxE1PYF580200BIEDBT.Done();
                response.ProcessSteps.Add(step2GetMaxE1PYF580200BIEDBT);
                var step3UpdateCorpIntegrationBIEDBT = new ProcessStep("Step 3. Update Corp Integration BIEDBT");
                var newBIEDBT = maxE1PYBIEDBT + 2;
                var sqlResponse = IntegrationManager.UpdateCorpIntegrationBIEDBT(cnCorpIntPY, newBIEDBT);
                response.SqlDmlResponses.Add(sqlResponse);
                step3UpdateCorpIntegrationBIEDBT.RowsAffected = 1;
                step3UpdateCorpIntegrationBIEDBT.Done();
                response.ProcessSteps.Add(step3UpdateCorpIntegrationBIEDBT);
                response.TransferStatus = "Y";
                response.Close();
                SummarizeResetPYIntegrationBIEDBT(response, currentCorpIntBIEDBT, maxE1PYBIEDBT, newBIEDBT);
                NotifyResetPYIntegrationBIEDBT(response);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                e1SessionPY.Close();
                cnCorpIntPY.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        public static ProcessResponse ResetPYIntegrationSFEDBT()
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse("Reset PY Integration SFEDBT");
            var e1SessionPY = new BusinessSystemSession();
            var cnCorpIntPY = new SqlConnection();
            try
            {
                e1SessionPY = BusinessSystemSessionManager.GetBusinessSessionPY();
                response.ConnectionStrings.Add(new ConnectionString("cnE1PY", e1SessionPY.DbConnection.ConnectionString));
                cnCorpIntPY = GetCorpIntPYSqlConnection();
                response.ConnectionStrings.Add(new ConnectionString("cnCorpIntPY", cnCorpIntPY.ConnectionString));
                var step1GetCurrentCorpIntSFEDBT = new ProcessStep("Step 1. Get Current CorpInt SFEDBT");
                var currentCorpIntSFEDBT = IntegrationManager.GetCurrentCorpIntSFEDBT(cnCorpIntPY);
                step1GetCurrentCorpIntSFEDBT.RowsAffected = 1;
                step1GetCurrentCorpIntSFEDBT.Done();
                response.ProcessSteps.Add(step1GetCurrentCorpIntSFEDBT);
                var step2GetMaxE1PYF580200SFEDBT = new ProcessStep("Step 2. Get Max E1 PY F580030 SFEDBT");
                var maxE1PYSFEDBT = F580030Manager.GetMaxE1PYF580030SFEDBT(e1SessionPY);
                step2GetMaxE1PYF580200SFEDBT.RowsAffected = 1;
                step2GetMaxE1PYF580200SFEDBT.Done();
                response.ProcessSteps.Add(step2GetMaxE1PYF580200SFEDBT);
                var step3UpdateCorpIntegrationSFEDBT = new ProcessStep("Step 3. Update Corp Integration SFEDBT");
                var newSFEDBT = maxE1PYSFEDBT + 2;
                var sqlResponse = IntegrationManager.UpdateCorpIntegrationSFEDBT(cnCorpIntPY, newSFEDBT);
                response.SqlDmlResponses.Add(sqlResponse);
                step3UpdateCorpIntegrationSFEDBT.RowsAffected = 1;
                step3UpdateCorpIntegrationSFEDBT.Done();
                response.ProcessSteps.Add(step3UpdateCorpIntegrationSFEDBT);
                response.TransferStatus = "Y";
                response.Close();
                SummarizeResetPYIntegrationSFEDBT(response, currentCorpIntSFEDBT, maxE1PYSFEDBT, newSFEDBT);
                NotifyResetPYIntegrationSFEDBT(response);
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{e.Message}: {e.StackTrace}");
            }
            finally
            {
                e1SessionPY.Close();
                cnCorpIntPY.Close();
                response.Close();
                Logger.Info($"END: {Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds)} SECONDS");
            }

            return response;
        }

        private static SqlConnection GetCorpIntPYSqlConnection()
        {
            var cnCorpIntPY = new SqlConnection();
            try
            {
                cnCorpIntPY = ConnectionManager.GetSqlConnectionFromKey("CORP.INTEGRATION.PY", false);
                cnCorpIntPY.Open();
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnCorpIntPY.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new ApplicationException($"{cnCorpIntPY.ConnectionString}  {e.Message}: {e.StackTrace}");
            }

            return cnCorpIntPY;
        }

        public static void SummarizeResetPYIntegrationQAUKID(ProcessResponse response, decimal currentCorpIntQAUKID, decimal maxE1PYQAUKID, decimal newQAUKID)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var headerSummary = new
                {
                    OldValue = currentCorpIntQAUKID,
                    E1Value = maxE1PYQAUKID,
                    NewValue = newQAUKID,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal) (response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
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

        public static void SummarizeResetPYIntegrationSFEDBT(ProcessResponse response, decimal currentCorpIntSFEDBT, decimal maxE1PYSFEDBT, decimal newSFEDBT)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var headerSummary = new
                {
                    OldValue = currentCorpIntSFEDBT,
                    E1Value = maxE1PYSFEDBT,
                    NewValue = newSFEDBT,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal) (response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
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

        public static void SummarizeResetPYIntegrationBIEDBT(ProcessResponse response, decimal currentCorpIntBIEDBT, decimal maxE1PYBIEDBT, decimal newBIEDBT)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var headerSummary = new
                {
                    OldValue = currentCorpIntBIEDBT,
                    E1Value = maxE1PYBIEDBT,
                    NewValue = newBIEDBT,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal) (response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
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

        public static void SummarizeResetPYIntegrationBIUKID(ProcessResponse response, decimal currentCorpIntBIUKID, decimal maxE1PYBIUKID, decimal newBIUKID)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var headerSummary = new
                {
                    OldValue = currentCorpIntBIUKID,
                    E1Value = maxE1PYBIUKID,
                    NewValue = newBIUKID,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal) (response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
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

        public static void SummarizeResetPYIntegrationSFUKID(ProcessResponse response, decimal currentCorpIntSFUKID, decimal maxE1PYSFUKID, decimal newSFUKID)
        {
            var stepSummarizeDownload = new ProcessStep("Summary Collections");
            try
            {
                var headerSummary = new
                {
                    OldValue = currentCorpIntSFUKID,
                    E1Value = maxE1PYSFUKID,
                    NewValue = newSFUKID,
                    StartDate = response.ProcessSteps.Min(x => x.StartDate),
                    StartDateString = response.ProcessSteps.Min(x => x.StartDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    EndDate = response.ProcessSteps.Max(x => x.EndDate),
                    EndDateString = response.ProcessSteps.Min(x => x.EndDate).ToString("M/dd/yyyy hh:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US")),
                    ElapsedSeconds = (decimal) (response.ProcessSteps.Max(x => x.EndDate) - response.ProcessSteps.Min(x => x.StartDate)).TotalSeconds
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

        public static void NotifyResetPYIntegrationQAUKID(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ResetPYIntegrationQAUKID.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Reset PY Integration QAUKID";
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

        public static void NotifyResetPYIntegrationSFEDBT(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ResetPYIntegrationSFEDBT.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Reset PY Integration SFEDBT";
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

        public static void NotifyResetPYIntegrationBIEDBT(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ResetPYIntegrationBIEDBT.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Reset PY Integration BIEDBT";
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

        public static void NotifyResetPYIntegrationBIUKID(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ResetPYIntegrationBIUKID.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Reset PY Integration BIUKID";
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

        public static void NotifyResetPYIntegrationSFUKID(ProcessResponse response)
        {
            var stepSendMail = new ProcessStep("Send Mail");
            try
            {
                var toConfigAddresses = ConfigurationManager.AppSettings["MailConfirmToAddress"];
                var serializedXml = response.Serialize();
                Logger.Info($@"Serialized Process: {serializedXml}");
                var xslOutput = XsltManager.Transform(serializedXml, "BIServices.Common.Xslt.ResetPYIntegrationSFUKID.xslt", Encoding.UTF8);
                Logger.Info($@"Report XML: {xslOutput}");
                var subject = @"BI Reset PY Integration SFUKID";
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

        public static Subscriptions GetSubscriptions(string plantCode)
        {
            try
            {
                var mgr = new IntegrationManager();
                return mgr.GetEnabledSubscriptions(plantCode);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static List<string> GetPlantsWithInactiveSubscriptions()
        {
            try
            {
                var mgr = new IntegrationManager();
                return mgr.GetPlantsWithInactiveSubscriptions();
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static SqlDmlResponse ActivateSubscriptions(string plantCode)
        {
            try
            {
                return IntegrationManager.ActivateSubscriptions(plantCode);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static SqlDmlResponse InactivateSubscriptions(string plantCode)
        {
            try
            {
                var mgr = new IntegrationManager();
                return mgr.InactivateSubscriptions(plantCode);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static string GetScheduleAgentStatus()
        {
            try
            {
                var hostName = ConfigurationManager.AppSettings["ScheduleAgentHostName"];
                var svcName = ConfigurationManager.AppSettings["ScheduleAgentServiceName"];
                var serviceCtl = new ServiceController(svcName, hostName);
                return serviceCtl.Status.ToString();
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static bool StartScheduleAgent()
        {
            try
            {
                var hostName = ConfigurationManager.AppSettings["ScheduleAgentHostName"];
                var svcName = ConfigurationManager.AppSettings["ScheduleAgentServiceName"];
                var serviceCtl = new ServiceController(svcName, hostName);
                serviceCtl.Start();
                return true;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static bool StopScheduleAgent()
        {
            try
            {
                var hostName = ConfigurationManager.AppSettings["ScheduleAgentHostName"];
                var svcName = ConfigurationManager.AppSettings["ScheduleAgentServiceName"];
                var serviceCtl = new ServiceController(svcName, hostName);
                serviceCtl.Stop();
                return true;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }

        public static PingResults GetPingResults(string plant, string startDate, string endDate)
        {
            try
            {
                var mgr = new IntegrationManager();
                return mgr.GetPingResults(plant, startDate, endDate);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }
    }
}