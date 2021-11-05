using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using BIServices.Common.Types.Archie;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    public class TCCManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse TCCDeleteDocs(SqlConnection cnTCCSUPPORT, int rows, bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"TCC Delete Docs {ConfigurationManager.AppSettings["PrimaryEnvironment"]}: {rows} Rows", true);
            response.ConnectionStrings.Add(new ConnectionString("cnTCCSUPPORT", cnTCCSUPPORT.ConnectionString));
            var docs = new TCCUnions();
            var deleteGUID = Guid.NewGuid();
            try
            {
                var step1GetOldestDeleteDates = new ProcessStep($@"Step 1. Find the oldest rows");
                try
                {
                    // Command
                    var cmd = new SqlCommand
                    {
                        Connection = cnTCCSUPPORT,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = $"dbo.usp_GetOldestDeleteDates",
                        CommandTimeout = 300
                    };
                    cmd.Parameters.AddWithValue("@rows", rows);
                    var dmlResponse = new SqlDmlResponse();
                    dmlResponse.Init(cmd);
                    Logger.Info(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var doc = new TCCUnion();
                            doc.Filter = CastDbNull.To(reader["Filter"], string.Empty);
                            doc.fsdDocID = CastDbNull.To(reader["fsdDocID"], 0);
                            doc.PDFFileName = CastDbNull.To(reader["PDFFileName"], string.Empty);
                            //doc.PDFExists = CastDbNull.To(reader["PDFExists"], false);
                            doc.PDFPath = CastDbNull.To(reader["PDFPath"], string.Empty);
                            doc.XMLFileName = CastDbNull.To(reader["XMLFileName"], string.Empty);
                            //doc.XMLExists = CastDbNull.To(reader["XMLExists"], false);
                            doc.XMLPath = CastDbNull.To(reader["XMLPath"], string.Empty);
                            doc.Version = CastDbNull.To(reader["Version"], 0);
                            //doc.ImageSize = CastDbNull.To(reader["ImageSize"], 0);
                            doc.ImageStr = CastDbNull.To(reader["ImageStr"], string.Empty);
                            doc.fsdDeleteFlag = CastDbNull.To<short>(reader["fsdDeleteFlag"], 0);
                            doc.fsdSatID = CastDbNull.To(reader["fsdSatID"], string.Empty);
                            doc.fsdDocType = CastDbNull.To(reader["fsdDocType"], string.Empty);
                            doc.fsdDateStored = CastDbNull.To(reader["fsdDateStored"], new DateTime(1900, 1, 1));
                            doc.fsdDateDelete = CastDbNull.To(reader["fsdDateDelete"], new DateTime(1900, 1, 1));
                            doc.OfflinePath = CastDbNull.To(reader["OfflinePath"], string.Empty);
                            doc.OfflineFlag = CastDbNull.To(reader["OfflineFlag"], 0);
                            doc.fsdOfflineDate = CastDbNull.To(reader["fsdOfflineDate"], new DateTime(1900, 1, 1));
                            doc.CatID = CastDbNull.To(reader["CatID"], 0);
                            doc.DocID = CastDbNull.To(reader["DocID"], 0);
                            doc.SatID = CastDbNull.To(reader["SatID"], 0);
                            doc.DateStored = CastDbNull.To(reader["DateStored"], new DateTime(1900, 1, 1));
                            doc.DateDelete = CastDbNull.To(reader["DateDelete"], new DateTime(1900, 1, 1));
                            doc.DocType = CastDbNull.To(reader["DocType"], string.Empty);
                            doc.OfflineDate = CastDbNull.To(reader["OfflineDate"], new DateTime(1900, 1, 1));
                            doc.DeleteFlag = CastDbNull.To(reader["DeleteFlag"], 0);
                            doc.FS_InboxOwner = CastDbNull.To(reader["FS_InboxOwner"], 0);
                            doc.FS_InboxSubject = CastDbNull.To(reader["FS_InboxSubject"], string.Empty);
                            doc.FS_ReprintServer = CastDbNull.To(reader["FS_ReprintServer"], string.Empty);
                            doc.FS_ReprintPrinter = CastDbNull.To(reader["FS_ReprintPrinter"], string.Empty);
                            doc.CATEGORY = CastDbNull.To(reader["CATEGORY"], string.Empty);
                            doc.DOCSUBJECT = CastDbNull.To(reader["DOCSUBJECT"], string.Empty);
                            doc.DOCLOCATION = CastDbNull.To(reader["DOCLOCATION"], string.Empty);
                            doc.DOCNUMBER1 = CastDbNull.To(reader["DOCNUMBER1"], string.Empty);
                            doc.DOCNUMBER2 = CastDbNull.To(reader["DOCNUMBER2"], string.Empty);
                            doc.DOCDESC1 = CastDbNull.To(reader["DOCDESC1"], string.Empty);
                            doc.DOCDATE_CALC = CastDbNull.To(reader["DOCDATE_CALC"], new DateTime(1900, 1, 1));
                            doc.LOADNUM = CastDbNull.To(reader["LOADNUM"], string.Empty);
                            doc.CARRIER = CastDbNull.To(reader["CARRIER"], string.Empty);
                            doc.BOL = CastDbNull.To(reader["BOL"], string.Empty);
                            doc.PO1 = CastDbNull.To(reader["PO1"], string.Empty);
                            doc.DOCDATE = CastDbNull.To(reader["DOCDATE"], string.Empty);
                            doc.DOCTYPE1 = CastDbNull.To(reader["DOCTYPE1"], string.Empty);
                            doc.DOCTYPE2 = CastDbNull.To(reader["DOCTYPE2"], string.Empty);
                            doc.DOCDESC2 = CastDbNull.To(reader["DOCDESC2"], string.Empty);
                            doc.DOCTITLE = CastDbNull.To(reader["DOCTITLE"], string.Empty);
                            doc.DOCAUTHOR = CastDbNull.To(reader["DOCAUTHOR"], string.Empty);
                            doc.DOCDATE1 = CastDbNull.To(reader["DOCDATE1"], new DateTime(1900, 1, 1));
                            doc.DOCDATE2 = CastDbNull.To(reader["DOCDATE2"], new DateTime(1900, 1, 1));
                            doc.PO2 = CastDbNull.To(reader["PO2"], string.Empty);
                            doc.PO3 = CastDbNull.To(reader["PO3"], string.Empty);
                            doc.PO4 = CastDbNull.To(reader["PO4"], string.Empty);
                            doc.PO5 = CastDbNull.To(reader["PO5"], string.Empty);
                            doc.DOCSTAMP = CastDbNull.To(reader["DOCSTAMP"], string.Empty);
                            doc.DOCNAME = CastDbNull.To(reader["DOCNAME"], string.Empty);
                            doc.Hex2str = CastDbNull.To(reader["Hex2str"], string.Empty);
                            doc.Hex3str = CastDbNull.To(reader["Hex3str"], string.Empty);
                            doc.AllowDeleting = CastDbNull.To(reader["AllowDeleting"], false);
                            docs.Add(doc);
                        }

                        dmlResponse.RowsAffected = docs.Count;
                        response.SqlDmlResponses.Add(dmlResponse);
                        reader.Close();
                    }
                    step1GetOldestDeleteDates.RowsAffected = dmlResponse.RowsAffected;
                    response.RowsAffected = step1GetOldestDeleteDates.RowsAffected;
                }
                catch (Exception e)
                {
                    step1GetOldestDeleteDates.Errors.Add(new Error(e));
                    Logger.Error($"ERROR: {e.Message}: {e.StackTrace}");
                    throw new Exception(e.Message);
                }
                step1GetOldestDeleteDates.Done();
                response.ProcessSteps.Add(step1GetOldestDeleteDates);

                if (!step1GetOldestDeleteDates.HasErrors())
                {
                    var step2 = new ProcessStep($@"Step 2. Delete files");
                    try
                    {
                        foreach (var doc in docs)
                        {
                            // Delete the PDF file
                            if (File.Exists(doc.PDFPath))
                            {
                                try
                                {
                                    File.Delete(doc.PDFPath);
                                }
                                catch (Exception e)
                                {
                                    step2.Errors.Add(new Error($"{doc.Filter} document {doc.fsdDocID} {e.Message} {e.StackTrace}"));
                                    InsertDocDeleteError(cnTCCSUPPORT, doc, $"{doc.Filter} document {doc.fsdDocID} PDF {doc.PDFPath} {e.Message}", deleteGUID);
                                }
                            }
                            else
                            {
                                step2.Errors.Add(new Error($"{doc.Filter} document {doc.fsdDocID} PDF not found.", false));
                                step2.RowsAffected += InsertDocDeleteError(cnTCCSUPPORT, doc, $"{doc.Filter} document {doc.fsdDocID} PDF not found.", deleteGUID);
                            }
                            // Delete the XML file
                            if (File.Exists(doc.XMLPath))
                            {
                                try
                                {
                                    File.Delete(doc.XMLPath);
                                }
                                catch (Exception e)
                                {
                                    step2.Errors.Add(new Error($"{doc.Filter} document {doc.fsdDocID} {e.Message} {e.StackTrace}"));
                                    InsertDocDeleteError(cnTCCSUPPORT, doc, $"{doc.Filter} document {doc.fsdDocID} XML {doc.XMLPath} {e.Message}", deleteGUID);
                                }
                            }
                            else
                            {
                                step2.Errors.Add(new Error($"{doc.Filter} document {doc.fsdDocID} XML not found.", false));
                                step2.RowsAffected += InsertDocDeleteError(cnTCCSUPPORT, doc, $"{doc.Filter} document {doc.fsdDocID} XML not found.", deleteGUID);
                            }
                            // Log the SQL delete
                            step2.RowsAffected += InsertDocDeleted(cnTCCSUPPORT, doc, deleteGUID);
                            // Delete the SQL records
                            DeleteSQLDoc(cnTCCSUPPORT, doc);
                        }
                    }
                    catch (Exception e)
                    {
                        step2.Errors.Add(new Error(e));
                    }
                    step2.Done();
                    response.ProcessSteps.Add(step2);
                }
                var step3 = new ProcessStep($@"Step 3. Capture errors");
                try
                {
                    step3.RowsAffected = 0;
                    foreach (var step in response.ProcessSteps)
                    {
                        foreach (var err in step.Errors)
                        {
                            InsertProcessError(cnTCCSUPPORT, "TCCDeleteDocs", err, deleteGUID);
                            step3.RowsAffected += 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    step3.Errors.Add(new Error(e));
                }
                step3.Done();
                response.ProcessSteps.Add(step3);

                response.Close();
                SummarizeTCCDeleteDocs(response, docs, deleteGUID);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return response;
        }

        public static int InsertProcessError(SqlConnection cnTCCSUPPORT, string process, Error err, Guid deleteGUID)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            //var response = new ProcessResponse($"Insert TCC Process Errors {ConfigurationManager.AppSettings["PrimaryEnvironment"]}.", true);
            int rowCount;
            try
            {
                // Command
                var cmd = new SqlCommand
                {
                    Connection = cnTCCSUPPORT,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = $"dbo.usp_InsertProcessError",
                    CommandTimeout = 30
                };
                cmd.Parameters.AddWithValue("@process", process);
                cmd.Parameters.AddWithValue("@errMsg", err.Message);
                cmd.Parameters.AddWithValue("@errSource", err.Source);
                cmd.Parameters.AddWithValue("@errStackTrace", err.StackTrace);
                cmd.Parameters.AddWithValue("@deleteGUID", deleteGUID);
                Logger.Info(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }


        public static int DeleteSQLDoc(SqlConnection cnTCCSUPPORT, TCCUnion doc)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            //var response = new ProcessResponse($"Delete TCC SQL Document Row {ConfigurationManager.AppSettings["PrimaryEnvironment"]}: {doc.Filter} document {doc.fsdDocID}.", true);
            int rowCount;
            try
            {
                // Command
                var cmd = new SqlCommand
                {
                    Connection = cnTCCSUPPORT,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = $"dbo.usp_DeleteSQLDoc",
                    CommandTimeout = 30
                };
                cmd.Parameters.AddWithValue("@fsdDocID", doc.fsdDocID);
                cmd.Parameters.AddWithValue("@fsdSatID", doc.fsdSatID);
                cmd.Parameters.AddWithValue("@Filter", doc.Filter);
                Logger.Info(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int InsertDocDeleted(SqlConnection cnTCCSUPPORT, TCCUnion doc, Guid deleteGUID)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            int rowCount;
            try
            {
                // Command
                var cmd = new SqlCommand
                {
                    Connection = cnTCCSUPPORT,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = $"dbo.usp_InsertDocDeleted",
                    CommandTimeout = 30
                };
                cmd.Parameters.AddWithValue("@fsdDocID", doc.fsdDocID);
                cmd.Parameters.AddWithValue("@deleteGUID", deleteGUID);
                Logger.Info(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int InsertDocDeleteError(SqlConnection cnTCCSUPPORT, TCCUnion doc, string errMsg, Guid deleteGUID)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            int rowCount;
            try
            {
                // Command
                var cmd = new SqlCommand
                {
                    Connection = cnTCCSUPPORT,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = $"dbo.usp_InsertDocumentDeleteError",
                    CommandTimeout = 30
                };
                cmd.Parameters.AddWithValue("@fsdDocID", doc.fsdDocID);
                cmd.Parameters.AddWithValue("@errMsg", errMsg);
                cmd.Parameters.AddWithValue("@deleteGUID", deleteGUID);
                Logger.Info(Logging.DbCmdString(cmd));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }
        
        public static void SummarizeTCCDeleteDocs(ProcessResponse response, TCCUnions docs, Guid deleteGUID)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
                response.SerializedObjects.Add("ResultLine", docs, "ResultLines");

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
                    ElapsedSeconds = (decimal)(response.EndDate - response.StartDate).TotalSeconds,
                    deleteGUID,
                    response.RowsAffected
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

        public static ProcessResponse UpdateTCCDeleteDate(SqlConnection cnTCCSUPPORT, int rows, bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Update TCC Delete Date {ConfigurationManager.AppSettings["PrimaryEnvironment"]}: {rows} Rows", true);
            response.ConnectionStrings.Add(new ConnectionString("cnTCCSUPPORT", cnTCCSUPPORT.ConnectionString));
            var results = new TCCResults();
            try
            {
                var step1UpdateTCCDeleteDate = new ProcessStep($@"Step 1. Update TCC Delete Date");
                try
                {
                    // Command
                    var cmd = new SqlCommand
                    {
                        Connection = cnTCCSUPPORT,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "dbo.usp_UpdateDeleteDate",
                        CommandTimeout = 300
                    };
                    cmd.Parameters.AddWithValue("@rows", rows);
                    var dmlResponse = new SqlDmlResponse();
                    dmlResponse.Init(cmd);
                    Logger.Info(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var result = new TCCResult();
                            result.Step = CastDbNull.To(reader["Step"], string.Empty).Trim();
                            result.Selected = CastDbNull.To(reader["Selected"], 0);
                            result.Updated = CastDbNull.To(reader["Updated"], 0);
                            result.Deleted = CastDbNull.To(reader["Deleted"], 0);
                            result.StartDate = CastDbNull.To(reader["StartDate"], DateTime.MinValue);
                            result.EndDate = CastDbNull.To(reader["EndDate"], DateTime.MinValue);
                            result.GUID = CastDbNull.To(reader["OfflineGUID"], Guid.Empty).ToString();
                            result.ElapsedSeconds = CastDbNull.To(reader["ElapsedSeconds"], 0);
                            results.Add(result);
                        }

                        dmlResponse.RowsAffected = results.Count;
                        response.SqlDmlResponses.Add(dmlResponse);
                        reader.Close();
                    }

                    step1UpdateTCCDeleteDate.RowsAffected = dmlResponse.RowsAffected;
                    response.RowsAffected = step1UpdateTCCDeleteDate.RowsAffected;
                }
                catch (Exception e)
                {
                    step1UpdateTCCDeleteDate.Errors.Add(new Error(e));
                }
                step1UpdateTCCDeleteDate.Done();
                response.ProcessSteps.Add(step1UpdateTCCDeleteDate);
                response.Close();
                SummarizeUpdateTCCDeleteDate(response, results);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return response;
        }

        public static ProcessResponse TCCFindNewCategories(SqlConnection cnTCCSUPPORT, bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"TCC Find New Categories {ConfigurationManager.AppSettings["PrimaryEnvironment"]}", true);
            response.ConnectionStrings.Add(new ConnectionString("cnTCCSUPPORT", cnTCCSUPPORT.ConnectionString));
            var configs = new TCCConfigurations();
            try
            {
                var step1TCCFindNewCategories = new ProcessStep($@"Step 1. TCC Find New Categories");
                try
                {
                    // Command
                    var cmd = new SqlCommand
                    {
                        Connection = cnTCCSUPPORT,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "dbo.usp_FindNewCategories",
                        CommandTimeout = 300
                    };
                    var dmlResponse = new SqlDmlResponse();
                    dmlResponse.Init(cmd);
                    Logger.Info(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var config = new TCCConfiguration();
                            config.Category = CastDbNull.To(reader["Category"], string.Empty).Trim();
                            config.DOCSUBJECT = CastDbNull.To(reader["DOCSUBJECT"], string.Empty).Trim();
                            config.Documents = CastDbNull.To(reader["Documents"], 0);
                            config.Offlined = CastDbNull.To(reader["Offlined"], 0);
                            config.ToDo = CastDbNull.To(reader["ToDo"], 0);
                            config.OfflineDays = CastDbNull.To(reader["OfflineDays"], 0);
                            config.DeleteMonths = CastDbNull.To(reader["DeleteMonths"], 0);
                            config.WrittenDate = CastDbNull.To(reader["WrittenDate"], DateTime.Now);
                            config.AllowDeleting = CastDbNull.To(reader["AllowDeleting"], false);
                            config.AllowDeletingString = config.AllowDeleting ? "True" : "False";
                            configs.Add(config);
                        }

                        dmlResponse.RowsAffected = configs.Count;
                        response.SqlDmlResponses.Add(dmlResponse);
                        reader.Close();
                    }

                    step1TCCFindNewCategories.RowsAffected = dmlResponse.RowsAffected;
                    response.RowsAffected = step1TCCFindNewCategories.RowsAffected;
                }
                catch (Exception e)
                {
                    step1TCCFindNewCategories.Errors.Add(new Error(e));
                }
                step1TCCFindNewCategories.Done();
                response.ProcessSteps.Add(step1TCCFindNewCategories);
                response.Close();
                SummarizeUpdateTCCFindNewCategories(response, configs);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCCSUPPORT.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return response;
        }

        public static void SummarizeUpdateTCCFindNewCategories(ProcessResponse response, TCCConfigurations configs)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
                response.SerializedObjects.Add("ResultLine", configs, "ResultLines");

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
                    ElapsedSeconds = (decimal) (response.EndDate - response.StartDate).TotalSeconds
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

        public static void SummarizeUpdateTCCDeleteDate(ProcessResponse response, TCCResults results)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
                response.SerializedObjects.Add("ResultLine", results, "ResultLines");

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
    }
}