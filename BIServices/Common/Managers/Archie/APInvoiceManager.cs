using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable StringLiteralTypo

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Archie
{
    public class APInvoiceManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ProcessResponse ImportAPInvoices(SqlConnection cnTCC, SqlConnection cnTCCSUPPORT, bool verbose)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Import AP Invoices {ConfigurationManager.AppSettings["Environment"]}: ", true);
            response.ConnectionStrings.Add(new ConnectionString("cnTCC", cnTCC.ConnectionString));
            response.ConnectionStrings.Add(new ConnectionString("cnTCCSUPPORT", cnTCCSUPPORT.ConnectionString));
            var invoiceFilePathTXT = ConfigurationManager.AppSettings["APInvoiceFilePathTXT"];
            response.ConnectionStrings.Add(new ConnectionString("APInvoiceFilePathTXT", invoiceFilePathTXT));
            var invoiceFilePathPDF = ConfigurationManager.AppSettings["APInvoiceFilePathPDF"];
            response.ConnectionStrings.Add(new ConnectionString("APInvoiceFilePathPDF", invoiceFilePathPDF));
            var invoices = new APInvoices();
            var filesTXT = new FileInfo[0];
            var filesPDF = new FileInfo[0];
            var catID = 0;
            var importGUID = Guid.NewGuid();
            var fileNamesTXT = new List<string>();
            var fileNamesPDF = new List<string>();
            var fromPathPDF = "";
            var toPathPDF = "";
            var fromPathTXT = "";
            var toPathTXT = "";
            var retryMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["APInvoiceRetryMinutes"]);

            try
            {
                var step1FindNewInvoices = new ProcessStep($@"Step 1. Find new invoice TXT files");
                try
                {
                    DirectoryInfo invoiceDirTXT = new DirectoryInfo(invoiceFilePathTXT);
                    filesTXT = invoiceDirTXT.GetFiles("*.txt");
                    fileNamesTXT.AddRange(filesTXT.Select(file => file.Name.Replace(".txt","")));
                    step1FindNewInvoices.RowsAffected = filesTXT.Length;
                    response.RowsAffected = step1FindNewInvoices.RowsAffected;
                }
                catch (Exception e)
                {
                    step1FindNewInvoices.Errors.Add(new Error(e));
                }
                step1FindNewInvoices.Done();
                response.ProcessSteps.Add(step1FindNewInvoices);

                var step2 = new ProcessStep($@"Step 2. Find new invoice PDF files");
                try
                {
                    DirectoryInfo invoiceDirPDF = new DirectoryInfo(invoiceFilePathPDF);
                    filesPDF = invoiceDirPDF.GetFiles("*.pdf");
                    fileNamesPDF.AddRange(filesPDF.Select(file => file.Name.Replace(".pdf","")));
                    step2.RowsAffected = filesPDF.Length;
                }
                catch (Exception e)
                {
                    step2.Errors.Add(new Error(e));
                }
                step2.Done();
                response.ProcessSteps.Add(step2);

                var pdfWithoutTXT = fileNamesPDF.Except(fileNamesTXT).ToList();
                var txtWithoutPDF = fileNamesTXT.Except(fileNamesPDF).ToList();

                if (filesTXT.Length > 0 && filesPDF.Length > 0 && !step1FindNewInvoices.HasErrors() && !step2.HasErrors())
                {
                    var step3 = new ProcessStep($@"Step 3. Parse text files and import pdfs");
                    try
                    {
                        foreach (FileInfo file in filesTXT)
                        {
                            //string invoiceTXT = File.ReadAllText(file.FullName);
                            string[] invoiceTXTLines = File.ReadAllLines(file.FullName);
                            var i = 1;
                            foreach (var invoiceTXT in invoiceTXTLines)
                            {
                                if (i <= 2)
                                {
                                    // Skip the first 2 lines
                                    i++;
                                }
                                else
                                {
                                    var invoice = ParseInvoiceText(invoiceTXT, file.FullName);
                                    if (invoice.ValidationError == null)
                                    {
                                        AddPDF(invoice, filesPDF);
                                        invoices.Add(invoice);
                                    }
                                    else
                                    {
                                        invoice.ValidationError.Message = $"Invoice file {file.FullName} does not have 34 columns";
                                    }

                                    if (invoice.ValidationError != null) response.Messages.Add(new Message(file.FullName, invoice.ValidationError.Message));

                                }
                            }
                        }
                        step3.RowsAffected = invoices.Count;
                    }
                    catch (Exception e)
                    {
                        step3.Errors.Add(new Error(e));
                    }
                    step3.Done();
                    response.ProcessSteps.Add(step3);

                    if ( !step3.HasErrors())
                    {
                        var step4 = new ProcessStep($@"Step 4. Get AP Invoice Category ID");
                        try
                        {
                            catID = GetCatID(cnTCC);
                            step4.RowsAffected = 1;
                        }
                        catch (Exception e)
                        {
                            step4.Errors.Add(new Error(e));
                        }
                        step4.Done();
                        response.ProcessSteps.Add(step4);

                        var step5 = new ProcessStep($@"Step 5. Map text file columns to database table columns");
                        try
                        {
                            foreach (var invoice in invoices)
                            {
                                if (invoice.HasPDF && invoice.HasTXT)
                                {
                                    MapInvoiceToSQL(cnTCC, invoice, catID);
                                    step5.RowsAffected += 1;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            step5.Errors.Add(new Error(e));
                        }
                        step5.Done();
                        response.ProcessSteps.Add(step5);

                        if (!step5.HasErrors())
                        {

                            var step6 = new ProcessStep($@"Step 6. Insert SQL Document Header");
                            try
                            {
                                foreach (var invoice in invoices)
                                {
                                    if (invoice.HasPDF && invoice.HasTXT && invoice.ValidationError == null)
                                    {
                                        InsertSQLDocHeader(cnTCC, invoice);
                                        invoice.IsInsertedDocHeader = true;
                                        step6.RowsAffected += 1;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                step6.Errors.Add(new Error(e));
                            }
                            step6.Done();
                            response.ProcessSteps.Add(step6);

                            if (!step6.HasErrors())
                            {

                                var step7 = new ProcessStep($@"Step 7. Insert SQL AP Invoice");
                                try
                                {
                                    foreach (var invoice in invoices)
                                    {
                                        if (invoice.HasPDF && invoice.HasTXT && invoice.ValidationError == null)
                                        {
                                            InsertSQLAPInvoice(cnTCC, invoice);
                                            invoice.IsInsertedAPInvoice = true;
                                            step7.RowsAffected += 1;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    step7.Errors.Add(new Error(e));
                                }
                                step7.Done();
                                response.ProcessSteps.Add(step7);

                                if (!step7.HasErrors())
                                {
                                    var invoiceFilePathArchive = ConfigurationManager.AppSettings["APInvoiceFilePathArchive"];
                                    response.ConnectionStrings.Add(new ConnectionString("APInvoiceFilePathArchive", invoiceFilePathArchive));
                                    Directory.CreateDirectory(invoiceFilePathArchive);
                                    var invoiceFilePathErrors = ConfigurationManager.AppSettings["APInvoiceFilePathErrors"];
                                    response.ConnectionStrings.Add(new ConnectionString("APInvoiceFilePathErrors", invoiceFilePathErrors));
                                    Directory.CreateDirectory(invoiceFilePathErrors);

                                    var step8 = new ProcessStep($@"Step 8. Move imported files to the archive");
                                    try
                                    {
                                        foreach (var invoice in invoices)
                                        {
                                            if (invoice.HasPDF && invoice.HasTXT && invoice.ValidationError == null && invoice.IsInsertedDocHeader && invoice.IsInsertedAPInvoice)
                                            {
                                                try
                                                {
                                                    var fileTXT = filesTXT.First(file => file.FullName == invoice.txtFilePath);
                                                    fromPathTXT = fileTXT.FullName;
                                                    toPathTXT = $@"{invoiceFilePathArchive}\{fileTXT.Name}";
                                                    Logger.Trace($"Move <{fromPathTXT}> to <{toPathTXT}>");
                                                    File.Move(fromPathTXT, toPathTXT);
                                                    invoice.IsArchivedTXT = true;
                                                }
                                                catch (Exception e)
                                                {
                                                    Logger.Error($"ERROR: Unable to move <{invoice.txtFilePath}> {e.Message} : {e.StackTrace}");
                                                    step8.Errors.Add(new Error($"Unable to move file <{fromPathTXT}> to <{toPathTXT}>", false));
                                                    //step8.Errors.Add(new Error(e));
                                                }
                                                try
                                                {
                                                    var filePDF = filesPDF.First(file => file.FullName == invoice.pdfFilePath);
                                                    fromPathPDF = filePDF.FullName;
                                                    toPathPDF = $@"{invoiceFilePathArchive}\{filePDF.Name}";
                                                    Logger.Trace($"Move <{fromPathPDF}> to <{toPathPDF}>");
                                                    File.Move(fromPathPDF, toPathPDF);
                                                    invoice.IsArchivedPDF = true;
                                                    step8.RowsAffected += 1;
                                                }
                                                catch (Exception e)
                                                {
                                                    Logger.Error($"ERROR: Unable to move <{invoice.pdfFilePath}> {e.Message} : {e.StackTrace}");
                                                    step8.Errors.Add(new Error($"Unable to move file <{fromPathPDF}> to <{toPathPDF}>", false));
                                                    //step8.Errors.Add(new Error(e));
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error("ERROR:  " + e.Message + ": " + e.StackTrace);
                                        step8.Errors.Add(new Error(e));
                                    }
                                    step8.Done();
                                    response.ProcessSteps.Add(step8);

                                    //var step9 = new ProcessStep($@"Step 9. Move validation files to the errors folder");
                                    //try
                                    //{
                                    //    foreach (var invoice in invoices)
                                    //    {
                                    //        if (invoice.HasPDF && invoice.HasTXT && invoice.ValidationError != null && !invoice.IsInsertedDocHeader && !invoice.IsInsertedAPInvoice)
                                    //        {
                                    //            try
                                    //            {
                                    //                var fileTXT = filesTXT.First(file => file.FullName == invoice.txtFilePath);
                                    //                if (fileTXT.CreationTime < DateTime.Now.ToLocalTime().AddMinutes(-retryMinutes))
                                    //                {
                                    //                    fromPathTXT = fileTXT.FullName;
                                    //                    toPathTXT = $@"{invoiceFilePathErrors}\{fileTXT.Name}";
                                    //                    Logger.Trace($"Move <{fromPathTXT}> to <{toPathTXT}>");
                                    //                    File.Move(fromPathTXT, toPathTXT);
                                    //                    response.Errors.Add(new Error($"File <{fromPathTXT}> has validation errors and has been moved to the Errors folder", false));
                                    //                }
                                    //            }
                                    //            catch (Exception e)
                                    //            {
                                    //                Logger.Error($"ERROR: Unable to move <{fromPathTXT}> {e.Message} : {e.StackTrace}");
                                    //                step9.Errors.Add(new Error($"Unable to move file <{fromPathTXT}> to <{toPathTXT}>", false));
                                    //            }

                                    //            try
                                    //            {
                                    //                var filePDF = filesPDF.First(file => file.FullName == invoice.pdfFilePath);
                                    //                if (filePDF.CreationTime < DateTime.Now.ToLocalTime().AddMinutes(-retryMinutes))
                                    //                {
                                    //                    fromPathPDF = filePDF.FullName;
                                    //                    toPathPDF = $@"{invoiceFilePathErrors}\{filePDF.Name}";
                                    //                    Logger.Trace($"Move <{fromPathPDF}> to <{toPathPDF}>");
                                    //                    File.Move(fromPathPDF, toPathPDF);
                                    //                    response.Errors.Add(new Error($"File <{fromPathPDF}> has validation errors and has been moved to the Errors folder", false));
                                    //                    step9.RowsAffected += 1;
                                    //                }
                                    //            }
                                    //            catch (Exception e)
                                    //            {
                                    //                Logger.Error($"ERROR: Unable to move <{invoice.pdfFilePath}> {e.Message} : {e.StackTrace}");
                                    //                step9.Errors.Add(new Error($"Unable to move file <{fromPathPDF}> to <{toPathPDF}>", false));
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    //catch (Exception e)
                                    //{
                                    //    Logger.Error("ERROR:  " + e.Message + ": " + e.StackTrace);
                                    //    step9.Errors.Add(new Error(e));
                                    //}
                                    //step9.Done();
                                    //response.ProcessSteps.Add(step9);

                                    //var step10 = new ProcessStep($@"Step 10. Move orphan files to the errors folder");
                                    //try
                                    //{
                                    //    foreach (var file in pdfWithoutTXT)
                                    //    {
                                    //        fromPathPDF = $@"{invoiceFilePathPDF}\{file}.pdf";
                                    //        toPathPDF = $@"{invoiceFilePathErrors}\{file}.pdf";
                                    //        Logger.Trace($"Move <{fromPathPDF}> to <{toPathPDF}>");
                                    //        File.Move(fromPathPDF, toPathPDF);
                                    //        response.Errors.Add(new Error($"File <{fromPathPDF}> has no related .txt file and has been moved to the Errors folder", false));
                                    //    }
                                    //}
                                    //catch (Exception e)
                                    //{
                                    //    step10.Errors.Add(new Error($"Unable to move file <{fromPathPDF}> to <{toPathPDF}>", false));
                                    //    step10.Errors.Add(new Error(e));
                                    //}
                                    //try
                                    //{
                                    //    foreach (var file in txtWithoutPDF)
                                    //    {
                                    //        fromPathTXT = $@"{invoiceFilePathPDF}\{file}.txt";
                                    //        toPathTXT = $@"{invoiceFilePathErrors}\{file}.txt";
                                    //        Logger.Trace($"Move <{fromPathTXT}> to <{toPathTXT}>");
                                    //        File.Move(fromPathTXT, toPathTXT);
                                    //        response.Errors.Add(new Error($"File <{fromPathTXT}> has no related .pdf file and has been moved to the Errors folder", false));
                                    //        step10.RowsAffected += 1;
                                    //    }
                                    //}
                                    //catch (Exception e)
                                    //{
                                    //    step10.Errors.Add(new Error($"Unable to move file <{fromPathTXT}> to <{toPathTXT}>", false));
                                    //    step10.Errors.Add(new Error(e));
                                    //}
                                    //step10.Done();
                                    //response.ProcessSteps.Add(step10);

                                    var step11 = new ProcessStep($@"Step 11. Insert SQL Process Log");
                                    try
                                    {
                                        foreach (var invoice in invoices)
                                        {
                                            if (invoice.HasPDF && invoice.HasTXT && invoice.ValidationError == null && invoice.IsInsertedDocHeader && invoice.IsInsertedAPInvoice)
                                            {
                                                InsertSQLProcessLog(cnTCCSUPPORT, invoice, importGUID);
                                                step11.RowsAffected += 1;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        step11.Errors.Add(new Error(e));
                                    }
                                    step11.Done();
                                    response.ProcessSteps.Add(step11);
                                }
                            }
                        }
                    }
                }

                response.Close();
                SummarizeAPInvoiceImport(response, invoices, pdfWithoutTXT, txtWithoutPDF);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return response;
        }

        public static APInvoice AddPDF(APInvoice invoice, FileInfo[] filesPDF)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"AddPDF {invoice.DOC} BEGIN: {DateTime.Now.ToLocalTime()}");
            try
            {
                //var filePDF = filesPDF.First(file => file.FullName == invoice.GTFILENM);
                var filePDF = filesPDF.First(file => file.FullName == invoice.pdfFilePath);
                using (FileStream fStream = File.OpenRead(filePDF.FullName))
                {
                    byte[] contents = new byte[fStream.Length];
                    fStream.Read(contents, 0, (int) fStream.Length);
                    fStream.Close();
                    invoice.PDF = contents;
                    invoice.HasPDF = true;
                }
            }
            catch (Exception)
            {
                invoice.HasPDF = false;
                invoice.ValidationError = new ValidationError($"Invoice {invoice.DOC} has no PDF", "", "", true);
                Logger.Error($"ERROR:  Invoice {invoice.DOC} has no PDF");
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return invoice;
        }

        private static SqlCommand InsertSQLAPInvoice()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    INSERT INTO [TCC].[webrepadmin].[AP Invoice]
                    (
                        [DocID]
                        ,[SatID]
                        ,[DateStored]
                        ,[DateDelete]
                        ,[DocType]
                        ,[OfflineDate]
                        ,[DeleteFlag]
                        ,[FS_InboxOwner]
                        ,[FS_InboxSubject]
                        ,[FS_ReprintServer]
                        ,[FS_ReprintPrinter]
                        ,[BATCHNUMBER]
                        ,[BUSINESSUNIT]
                        ,[DOCNUMBER]
                        ,[DOCUMENTTYPE]
                        ,[INVOICENUMBER]
                        ,[INVOICEDATE]
                        ,[PURCHASEORDER]
                        ,[VENDORNUMBER]
                        ,[VENDORNAME]
                        ,[GROSSAMOUNT]
                        ,[REMITTANCE]
                        ,[GLDATE]
                        ,[OBJECTACCOUNT]
                        ,[SUBLEDGERTYPE]
                        ,[ASSETNUMBER]
                    )
                    VALUES
                    (
                        @DocID
                        ,@SatID
                        ,@DateStored
                        ,@DateDelete
                        ,@DocType
                        ,@OfflineDate
                        ,@DeleteFlag
                        ,@FS_InboxOwner
                        ,@FS_InboxSubject
                        ,@FS_ReprintServer
                        ,@FS_ReprintPrinter
                        ,@BATCHNUMBER
                        ,@BUSINESSUNIT
                        ,@DOCNUMBER
                        ,@DOCUMENTTYPE
                        ,@INVOICENUMBER
                        ,@INVOICEDATE
                        ,@PURCHASEORDER
                        ,@VENDORNUMBER
                        ,@VENDORNAME
                        ,@GROSSAMOUNT
                        ,@REMITTANCE
                        ,@GLDATE
                        ,@OBJECTACCOUNT
                        ,@SUBLEDGERTYPE
                        ,@ASSETNUMBER
                    )                    
                "
            };
            cmd.Parameters.Add(new SqlParameter("@DocID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@SatID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@DateStored", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@DateDelete", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@DocType", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@OfflineDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@DeleteFlag", SqlDbType.SmallInt));
            cmd.Parameters.Add(new SqlParameter("@FS_InboxOwner", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@FS_InboxSubject", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@FS_ReprintServer", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@FS_ReprintPrinter", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@BATCHNUMBER", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@BUSINESSUNIT", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@DOCNUMBER", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@DOCUMENTTYPE", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@INVOICENUMBER", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@INVOICEDATE", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@PURCHASEORDER", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@VENDORNUMBER", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@VENDORNAME", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@GROSSAMOUNT", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@REMITTANCE", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@GLDATE", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@OBJECTACCOUNT", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@SUBLEDGERTYPE", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@ASSETNUMBER", SqlDbType.NVarChar));
            return cmd;
        }

        public static int InsertSQLAPInvoice(SqlConnection cnTCC, APInvoice invoice)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Insert SQL APInvoice {ConfigurationManager.AppSettings["Environment"]}: document {invoice.DOCNUMBER}", true);
            int rowCount = 0;
            try
            {
                var cmd = InsertSQLAPInvoice();
                cmd.Parameters["@DocID"].Value = invoice.DocID;
                cmd.Parameters["@SatID"].Value = invoice.SatID;
                cmd.Parameters["@DateStored"].Value = invoice.DateStored;
                cmd.Parameters["@DateDelete"].Value = invoice.DateDelete ?? (object)DBNull.Value;
                cmd.Parameters["@DocType"].Value = invoice.DocType;
                cmd.Parameters["@OfflineDate"].Value = invoice.OfflineDate ?? (object)DBNull.Value;
                cmd.Parameters["@DeleteFlag"].Value = invoice.DeleteFlag ?? 0;
                cmd.Parameters["@FS_InboxOwner"].Value = invoice.FS_InboxOwner ?? (object)DBNull.Value;
                cmd.Parameters["@FS_InboxSubject"].Value = invoice.FS_InboxSubject ?? (object)DBNull.Value;
                cmd.Parameters["@FS_ReprintServer"].Value = invoice.FS_ReprintServer ?? (object)DBNull.Value;
                cmd.Parameters["@FS_ReprintPrinter"].Value = invoice.FS_ReprintPrinter ?? (object)DBNull.Value;
                cmd.Parameters["@BATCHNUMBER"].Value = invoice.BATCHNUMBER;
                cmd.Parameters["@BUSINESSUNIT"].Value = invoice.BUSINESSUNIT;
                cmd.Parameters["@DOCNUMBER"].Value = invoice.DOCNUMBER;
                cmd.Parameters["@DOCUMENTTYPE"].Value = invoice.DOCUMENTTYPE;
                cmd.Parameters["@INVOICENUMBER"].Value = invoice.INVOICENUMBER;
                cmd.Parameters["@INVOICEDATE"].Value = invoice.INVOICEDATE;
                cmd.Parameters["@PURCHASEORDER"].Value = invoice.PURCHASEORDER;
                cmd.Parameters["@VENDORNUMBER"].Value = invoice.VENDORNUMBER;
                cmd.Parameters["@VENDORNAME"].Value = invoice.VENDORNAME;
                cmd.Parameters["@GROSSAMOUNT"].Value = invoice.GROSSAMOUNT;
                cmd.Parameters["@REMITTANCE"].Value = invoice.REMITTANCE;
                cmd.Parameters["@GLDATE"].Value = invoice.GLDATE;
                cmd.Parameters["@OBJECTACCOUNT"].Value = invoice.OBJECTACCOUNT;
                cmd.Parameters["@SUBLEDGERTYPE"].Value = invoice.SUBLEDGERTYPE;
                cmd.Parameters["@ASSETNUMBER"].Value = invoice.ASSETNUMBER;
                cmd.Connection = cnTCC;
                response.Init(cmd);
                Logger.Trace(Logging.DbCmdString(cmd));
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        private static SqlCommand InsertSQLDocHeader()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    INSERT INTO [TCC].[webrepadmin].[FS_Documents]
                    (
                        [DocID]
                        ,[Version]
                        ,[Image]
                        ,[DeleteFlag]
                        ,[SatID]
                        ,[DocType]
                        ,[DateStored]
                        ,[DateDelete]
                        ,[OfflinePath]
                        ,[OfflineFlag]
                        ,[OfflineDate]
                        ,[CatID]
                    )
                    VALUES 
                    (
                        @DocID
                        ,@Version
                        ,@Image
                        ,@DeleteFlag
                        ,@SatID
                        ,@DocType
                        ,@DateStored
                        ,@DateDelete
                        ,@OfflinePath
                        ,@OfflineFlag
                        ,@OfflineDate
                        ,@CatID
                    )
                    "
            };
            cmd.Parameters.Add(new SqlParameter("@DocID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@Image", SqlDbType.Image));
            cmd.Parameters.Add(new SqlParameter("@DeleteFlag", SqlDbType.SmallInt));
            cmd.Parameters.Add(new SqlParameter("@SatID", SqlDbType.Char));
            cmd.Parameters.Add(new SqlParameter("@DocType", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@DateStored", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@DateDelete", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@OfflinePath", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@OfflineFlag", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@OfflineDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@CatID", SqlDbType.Int));
            return cmd;
        }

        public static int InsertSQLDocHeader(SqlConnection cnTCC, APInvoice invoice)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Insert SQL Doc Header {ConfigurationManager.AppSettings["Environment"]}: document {invoice.DOCNUMBER}", true);
            int rowCount = 0;
            try
            {
                var cmd = InsertSQLDocHeader();
                cmd.Parameters["@DocID"].Value = invoice.DocID;
                cmd.Parameters["@Version"].Value = invoice.Version;
                cmd.Parameters["@Image"].Value = invoice.Image;
                cmd.Parameters["@DeleteFlag"].Value = invoice.DeleteFlag ?? (object)DBNull.Value;
                cmd.Parameters["@SatID"].Value = invoice.SatID.ToString();
                cmd.Parameters["@DocType"].Value = invoice.DocType;
                cmd.Parameters["@DateStored"].Value = invoice.DateStored;
                cmd.Parameters["@DateDelete"].Value = invoice.DateDelete ?? (object)DBNull.Value;
                cmd.Parameters["@OfflinePath"].Value = invoice.OfflinePath ?? (object)DBNull.Value;
                cmd.Parameters["@OfflineFlag"].Value = invoice.OfflineFlag ?? (object)DBNull.Value;
                cmd.Parameters["@OfflineDate"].Value = invoice.OfflineDate ?? (object)DBNull.Value;
                cmd.Parameters["@CatID"].Value = invoice.CatID;
                cmd.Connection = cnTCC;
                response.Init(cmd);
                Logger.Trace(Logging.DbCmdString(cmd));
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        private static SqlCommand InsertSQLProcessLog()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    INSERT INTO [TCCSUPPORT].[dbo].[ImportedInvoices]
                    (
                        [DocID]
                        ,[SatID]
                        ,[ImportDate]
                        ,[ImportGUID]
                        ,[DOCNUMBER]
                    )
                    VALUES
                    (
                        @DocID
                        ,@SatID
                        ,@ImportDate
                        ,@ImportGUID
                        ,@DOCNUMBER
                    )                    
                "
            };
            cmd.Parameters.Add(new SqlParameter("@DocID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@SatID", SqlDbType.Char));
            cmd.Parameters.Add(new SqlParameter("@ImportDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@ImportGUID", SqlDbType.UniqueIdentifier));
            cmd.Parameters.Add(new SqlParameter("@DOCNUMBER", SqlDbType.NVarChar));
            return cmd;
        }

        public static int InsertSQLProcessLog(SqlConnection cnTCCSUPPORT, APInvoice invoice, Guid importGUID)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"BEGIN: {DateTime.Now.ToLocalTime()}");
            var response = new ProcessResponse($"Insert SQL Process Log {ConfigurationManager.AppSettings["Environment"]}: document {invoice.DOCNUMBER}", true);
            int rowCount = 0;
            try
            {
                var cmd = InsertSQLProcessLog();
                cmd.Parameters["@DocID"].Value = invoice.DocID;
                cmd.Parameters["@SatID"].Value = invoice.SatID.ToString();
                cmd.Parameters["@ImportDate"].Value = invoice.DateStored;
                cmd.Parameters["@ImportGUID"].Value = importGUID;
                cmd.Parameters["@DOCNUMBER"].Value = invoice.DOCNUMBER;
                cmd.Connection = cnTCCSUPPORT;
                response.Init(cmd);
                Logger.Trace(Logging.DbCmdString(cmd));
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
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

        public static int GetCatID(SqlConnection cnTCC)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"GetCatID BEGIN: {DateTime.Now.ToLocalTime()}");
            int categoryID = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    CommandType = CommandType.Text,
                    CommandText = @"
                        SELECT [CategoryID]
                          FROM [TCC].[webrepadmin].[FS_Categories]
                          WHERE [CategoryName] = @CategoryName"
                };
                cmd.Parameters.Add(new SqlParameter("@CategoryName", SqlDbType.NVarChar, 50));
                cmd.Parameters["@CategoryName"].Value = ConfigurationManager.AppSettings["APInvoiceCategoryName"];
                cmd.Connection = cnTCC;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        categoryID = CastDbNull.To(reader["CategoryID"], 0);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return categoryID;
        }

        public static int GetNewDocID(SqlConnection cnTCC)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"GetNewDocID BEGIN: {DateTime.Now.ToLocalTime()}");
            int docID;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnTCC,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "webrepadmin.FS_sp_GetNewID",
                    CommandTimeout = 0
                };
                cmd.Parameters.Add("@p_NewID", SqlDbType.Int).Direction = ParameterDirection.Output;
                Logger.Trace(Logging.DbCmdString(cmd));
                cmd.ExecuteNonQuery();
                docID = Convert.ToInt32(cmd.Parameters["@p_NewID"].Value);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnTCC.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return docID;
        }

        public static APInvoice MapInvoiceToSQL(SqlConnection cnTCC, APInvoice invoice, int catID)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"MapInvoiceToSQL {invoice.DOC} BEGIN: {DateTime.Now.ToLocalTime()}");
            try
            {
                // FS_Documents
                invoice.DocID = GetNewDocID(cnTCC);
                invoice.Version = 1;
                invoice.Image = invoice.PDF;
                invoice.DeleteFlag = null;
                invoice.SatID = 1;
                invoice.DocType = "pdf"; 
                invoice.DateStored = DateTime.Now;
                invoice.DateDelete = null;
                invoice.OfflinePath = null;
                invoice.OfflineFlag = null;
                invoice.OfflineDate = null;
                invoice.CatID = catID;

                // AP Invoice
                invoice.FS_InboxOwner = null;
                invoice.FS_InboxSubject = "";
                invoice.FS_ReprintServer = null;
                invoice.FS_ReprintPrinter = null;
                invoice.BATCHNUMBER = invoice.BATCHNUMBER;
                invoice.BUSINESSUNIT = invoice.MCU;
                invoice.DOCNUMBER = invoice.DOC;
                invoice.DOCUMENTTYPE = invoice.DCT;
                invoice.INVOICENUMBER = invoice.VINV;
                DateTime.TryParse(invoice.DIVJ, out var divj);
                invoice.INVOICEDATE = divj;
                invoice.PURCHASEORDER = invoice.PO;
                invoice.VENDORNUMBER = invoice.AN8;
                invoice.VENDORNAME = invoice.ALPH;
                invoice.GROSSAMOUNT = invoice.AG;
                invoice.REMITTANCE = invoice.AA;
                DateTime.TryParse(invoice.DGJ, out var dgj);
                invoice.GLDATE = dgj;
                invoice.OBJECTACCOUNT = invoice.OBJ;
                invoice.SUBLEDGERTYPE = invoice.SBLT;
                invoice.ASSETNUMBER = invoice.ANI;
            }
            catch (Exception e)
            {
                Logger.Error("ERROR:  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return invoice;
        }

        public static APInvoice ParseInvoiceText(string invoiceTXT, string txtFileName) 
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info($"ParseInvoiceText BEGIN: {DateTime.Now.ToLocalTime()}");
            Logger.Info(invoiceTXT);
            var invoice = new APInvoice();
            invoice.txtFilePath = txtFileName;
            invoice.pdfFilePath = txtFileName.Replace(".txt", ".pdf");

            try
            {
                invoiceTXT = invoiceTXT.Replace(".pdf\r\n", ".pdf");
                string[] columns = invoiceTXT.Split('|');
                if (columns.Length == 34)
                {
                    invoice.BATCHNUMBER = columns[0];
                    invoice.DOC = columns[1];
                    invoice.ALPH = columns[2];
                    invoice.RP30 = columns[3];
                    invoice.MCU = columns[4];
                    invoice.RP31 = columns[5];
                    invoice.AN8 = columns[6];
                    invoice.RMK = columns[7];
                    invoice.EXA = columns[8];
                    invoice.DCT = columns[9];
                    invoice.AG = columns[10];
                    invoice.AAP = columns[11];
                    invoice.DIVJ = columns[12];
                    invoice.DGJ = columns[13];
                    invoice.DDJ = columns[14];
                    invoice.VINV = columns[15];
                    invoice.PO = columns[16];
                    invoice.SBL = columns[17];
                    invoice.DL01 = columns[18];
                    invoice.OBJ = columns[19];
                    invoice.SUB = columns[20];
                    invoice.KCO = columns[21];
                    invoice.ANI = columns[22];
                    invoice.AA = columns[23];
                    invoice.LNID = columns[24];
                    invoice.EXA2 = columns[25];
                    invoice.JELN = columns[26];
                    invoice.AN8GCA8 = columns[27];
                    invoice.SBLT = columns[28];
                    invoice.ADSC = columns[29];
                    invoice.DDNJ = columns[30];
                    invoice.PST = columns[31];
                    invoice.ASID = columns[32];
                    invoice.GTFILENM = columns[33];
                    invoice.HasTXT = true;
                }
                else
                {
                    invoice.ValidationError = new ValidationError($"Invoice file does not have 34 columns", "Error", "Error", true);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR:  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return invoice;
        }

        public static void SummarizeAPInvoiceImport(ProcessResponse response, APInvoices docs, List<string> pdfWithoutTXT, List<string> txtWithoutPDF)
        {
            var stepSummarize = new ProcessStep("Summary Collections");
            try
            {
                //foreach (var file in pdfWithoutTXT) response.Messages.Add(new Message($"{file}.pdf has no related .txt file", ""));
                //foreach (var file in txtWithoutPDF) response.Messages.Add(new Message($"{file}.txt has no related .pdf file", ""));

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
