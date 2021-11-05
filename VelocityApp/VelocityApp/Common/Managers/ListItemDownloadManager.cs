using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    internal class ListItemDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadListItem(SqlConnection cnDW, KMIEHS context, DateTime lookback)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving ListItem records...");
                TruncateListItem(cnDW);
                var query = from l in context.ListItem.Expand("History")
                            where l.History.LastEdited > new DateTime(lookback.Year, lookback.Month, lookback.Day)
                            select l;
                foreach (var o in query)
                {
                    //Console.WriteLine($"ListItemId ID: {o.ListItemId}");
                    Console.Write(".");
                    rowCount += InsertListItemEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing ListItem records");
                LoadListItem(cnDW);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int TruncateListItem(SqlConnection cnDW)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            int rowCount;
            try
            {
                var cmdTruncate = new SqlCommand
                {
                    Connection = cnDW,
                    CommandType = CommandType.Text,
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_LIST_ITEM]"
                };
                rowCount = cmdTruncate.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int InsertListItemEntity(SqlConnection cnDW, ListItemEntity listItemEntity)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            int rowCount;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnDW,
                    CommandType = CommandType.Text,
                    CommandText = @"
                        INSERT INTO [SOURCE].[VELOCITY_LIST_ITEM]
                            ([Item]
                              ,[ShortName]
                              ,[Description]
                              ,[UnlocalizedItem]
                              ,[UnlocalizedShortName]
                              ,[UnlocalizedDescription]
                              ,[ListItemId]
                              ,[ListTypeCode]
                              ,[ParentId]
                              ,[ClientId]
                              ,[LocationId]
                              ,[ListOrder]
                              ,[Code]
                              ,[Active]
                              ,[HistoryId])
                        VALUES
                            ( @Item
                                ,@ShortName
                                ,@Description
                                ,@UnlocalizedItem
                                ,@UnlocalizedShortName
                                ,@UnlocalizedDescription
                                ,@ListItemId
                                ,@ListTypeCode
                                ,@ParentId
                                ,@ClientId
                                ,@LocationId
                                ,@ListOrder
                                ,@Code
                                ,@Active
                                ,@HistoryId)"
                };
                cmd.Parameters.Add("@Item", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ShortName", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Description", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@UnlocalizedItem", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@UnlocalizedShortName", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@UnlocalizedDescription", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ListItemId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ListTypeCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ParentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ClientId", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ListOrder", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Code", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Active", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@HistoryId", SqlDbType.Int).Direction = ParameterDirection.Input;

                cmd.Parameters["@Item"].Value = listItemEntity.Item ?? (object)DBNull.Value;
                cmd.Parameters["@ShortName"].Value = listItemEntity.ShortName ?? (object)DBNull.Value;
                cmd.Parameters["@Description"].Value = listItemEntity.Description ?? (object)DBNull.Value;
                cmd.Parameters["@UnlocalizedItem"].Value = listItemEntity.UnlocalizedItem ?? (object)DBNull.Value;
                cmd.Parameters["@UnlocalizedShortName"].Value = listItemEntity.UnlocalizedShortName ?? (object)DBNull.Value;
                cmd.Parameters["@UnlocalizedDescription"].Value = listItemEntity.UnlocalizedDescription ?? (object)DBNull.Value;
                cmd.Parameters["@ListItemId"].Value = listItemEntity.ListItemId; 
                cmd.Parameters["@ListTypeCode"].Value = listItemEntity.ListTypeCode ?? (object)DBNull.Value;
                if (listItemEntity.ParentId != null) {cmd.Parameters["@ParentId"].Value = listItemEntity.ParentId;} else {cmd.Parameters["@ParentId"].Value = DBNull.Value;}
                cmd.Parameters["@ClientId"].Value = listItemEntity.ClientId ?? (object)DBNull.Value;
                if (listItemEntity.LocationId != null) { cmd.Parameters["@LocationId"].Value = listItemEntity.LocationId; } else { cmd.Parameters["@LocationId"].Value = DBNull.Value; }
                if (listItemEntity.ListOrder != null) { cmd.Parameters["@ListOrder"].Value = listItemEntity.ListOrder; } else { cmd.Parameters["@ListOrder"].Value = DBNull.Value; }
                cmd.Parameters["@Code"].Value = listItemEntity.Code ?? (object)DBNull.Value;
                cmd.Parameters["@Active"].Value = listItemEntity.Active;
                if (listItemEntity.HistoryId != null) { cmd.Parameters["@HistoryId"].Value = listItemEntity.HistoryId; } else { cmd.Parameters["@HistoryId"].Value = DBNull.Value; }

                Logger.Trace("CN: " + cnDW.ConnectionString);
                Logger.Trace("SQL: " + Environment.NewLine + cmd.CommandText);
                Logger.Trace("PARMS: " + string.Join(", ", cmd.Parameters.Cast<SqlParameter>().AsQueryable().Select(parm => parm.Value)));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int LoadListItem(SqlConnection cnDW)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            int rowCount;
            try
            {
                var cmdTruncate = new SqlCommand
                {
                    Connection = cnDW,
                    CommandType = CommandType.Text,
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_LIST_ITEM]"
                };
                rowCount = cmdTruncate.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }
    }
}
