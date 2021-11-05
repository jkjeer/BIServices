using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using BIServices.Common.Types.Response;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    [DataContract(Namespace = "")]
    public class BulkCopyManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SqlBulkCopyResponse BulkTable(string tableName, DataTable dt, SqlConnection cn, SqlTransaction txn = null)
        {
            var response = new SqlBulkCopyResponse
            {
                ConnectionString = cn.ConnectionString,
                TableName = tableName,
                StartTime = DateTime.Now
            };
            try
            {
                var bulk = new SqlBulkCopy(cn, SqlBulkCopyOptions.FireTriggers, txn)
                {
                    DestinationTableName = tableName,
                    BulkCopyTimeout = ConfigurationManager.AppSettings["BulkCopyTimeout"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["BulkCopyTimeout"]) : 300,
                    BatchSize = 25000
                };
                try
                {
                    var rowsCopiedField = bulk.GetType().GetField("_rowsCopied",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    response.StartTime = DateTime.Now;
                    foreach (DataColumn col in dt.Columns)
                    {
                        var map = new SqlBulkCopyColumnMapping(col.ColumnName, col.ColumnName);
                        bulk.ColumnMappings.Add(map);
                    }
                    bulk.WriteToServer(dt);
                    var rowsCopiedValue = (int)rowsCopiedField.GetValue(bulk);
                    response.RowsCopied = rowsCopiedValue;
                }
                catch (Exception e)
                {
                    throw new Exception($"{tableName} {cn.ConnectionString} {e.Message}");
                }
                finally
                {
                    bulk.Close();
                }
            }
            catch (Exception e)
            {
                response.Errors.Add(new Error(e));
                throw new Exception($"{cn.ConnectionString} {e.Message}");
            }
            finally
            {
                response.EndTime = DateTime.Now;
            }
            return response;
        }

        public static SqlBulkCopyResponse BulkReader(string tableName, IDataReader dr, SqlConnection cn)
        {
            var response = new SqlBulkCopyResponse
            {
                ConnectionString = cn.ConnectionString,
                TableName = tableName,
                StartTime = DateTime.Now
            };
            try
            {
                var bulk = new SqlBulkCopy(cn, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.TableLock, null)
                {
                    DestinationTableName = tableName,
                    BulkCopyTimeout = ConfigurationManager.AppSettings["BulkCopyTimeout"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["BulkCopyTimeout"]) : 300,
                    BatchSize = 25000
                };
                var columnName = "";
                try
                {
                    var rowsCopiedField = bulk.GetType().GetField("_rowsCopied", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    response.StartTime = DateTime.Now;
                    var schemaTable = dr.GetSchemaTable();
                    if (schemaTable != null)
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            columnName = row["ColumnName"].ToString();
                            var map = new SqlBulkCopyColumnMapping(columnName, columnName);
                            bulk.ColumnMappings.Add(map);
                        }
                    bulk.WriteToServer(dr);
                    var rowsCopiedValue = (int)rowsCopiedField.GetValue(bulk);
                    response.RowsCopied = rowsCopiedValue;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("SourceColumn: " + columnName + ", DestinationColumn: " + columnName + ": " + e.ToString());
                }
                finally
                {
                    bulk.Close();
                }
            }
            catch (Exception e)
            {
                response.Errors.Add(new Error(e));
                response.Message = e.ToString();
                Logger.Error("ERROR: " + cn.ConnectionString + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cn.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            response.EndTime = DateTime.Now;
            return response;
        }

        //public static SqlBulkCopyResponse BulkReaderList<T>(string tableName, IList<T> list, SqlConnection cn)
        //{
        //    var response = new SqlBulkCopyResponse
        //    {
        //        ConnectionString = cn.ConnectionString,
        //        TableName = tableName,
        //        //HasErrors = false,
        //        StartTime = DateTime.Now
        //    };
        //    try
        //    {
        //        var dr = list.GetDataReader();
        //        var bulk = new SqlBulkCopy(cn, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.TableLock, null)
        //        {
        //            DestinationTableName = tableName,
        //            BulkCopyTimeout = ConfigurationManager.AppSettings["BulkCopyTimeout"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["BulkCopyTimeout"]) : 300,
        //            BatchSize = 25000
        //        };
        //        var columnName = "";
        //        try
        //        {
        //            var rowsCopiedField = bulk.GetType().GetField("_rowsCopied", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //            response.StartTime = DateTime.Now;
        //            var schemaTable = dr.GetSchemaTable();
        //            if (schemaTable != null)
        //                foreach (DataRow row in schemaTable.Rows)
        //                {
        //                    columnName = row["ColumnName"].ToString();
        //                    var map = new SqlBulkCopyColumnMapping(columnName, columnName);
        //                    bulk.ColumnMappings.Add(map);
        //                }
        //            bulk.WriteToServer(dr);
        //            var rowsCopiedValue = (int)rowsCopiedField.GetValue(bulk);
        //            response.RowsCopied = rowsCopiedValue;
        //        }
        //        catch (Exception e)
        //        {
        //            throw new ApplicationException("SourceColumn: " + columnName + ", DestinationColumn: " + columnName + ": " + e.ToString());
        //        }
        //        finally
        //        {
        //            bulk.Close();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //response.HasErrors = true;
        //        response.Errors.Add(new Error(e));
        //        response.Message = e.ToString();
        //        Logger.Error("ERROR: " + cn.ConnectionString + e.Message + ": " + e.StackTrace);
        //        throw new ApplicationException(cn.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
        //    }
        //    response.EndTime = DateTime.Now;
        //    return response;
        //}
    }
}
