using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class LocationDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadLocation(SqlConnection cnDW, KMIEHS context, DateTime lookback)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving Location records...");
                TruncateLocation(cnDW);
                var query = from l in context.Location.Expand("History")
                            where l.History.LastEdited > new DateTime(lookback.Year, lookback.Month, lookback.Day)
                            select l;
                foreach (var o in query)
                {
                    Console.WriteLine($"LocationId ID: {o.LocationId}");
                    Console.Write(".");
                    rowCount += InsertLocationEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing ListItem records");
                LoadLocation(cnDW);
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

        public static int TruncateLocation(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_LOCATION]"
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

        public static int InsertLocationEntity(SqlConnection cnDW, LocationEntity locationEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_LOCATION]
                            ( [LocationId]
                              ,[ParentLocationId]
                              ,[ClientId]
                              ,[LocationCategoryId]
                              ,[LocationStatusId]
                              ,[LocationTypeId]
                              ,[LocationTreeLevel]
                              ,[Lft]
                              ,[Rgt]
                              ,[LocationCode]
                              ,[ExternalReference]
                              ,[Location]
                              ,[ShortName]
                              ,[Description]
                              ,[Active]
                              ,[Archived]
                              ,[HistoryId])
                        VALUES
                            ( @LocationId
                                ,@ParentLocationId
                                ,@ClientId
                                ,@LocationCategoryId
                                ,@LocationStatusId
                                ,@LocationTypeId
                                ,@LocationTreeLevel
                                ,@Lft
                                ,@Rgt
                                ,@LocationCode
                                ,@ExternalReference
                                ,@Location
                                ,@ShortName
                                ,@Description
                                ,@Active
                                ,@Archived
                                ,@HistoryId)"
                };
                cmd.Parameters.Add("@LocationId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ParentLocationId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ClientId", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationCategoryId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationStatusId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationTypeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationTreeLevel", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Lft", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Rgt", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ExternalReference", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Location", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ShortName", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Description", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Active", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Archived", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@HistoryId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters["@LocationId"].Value = locationEntity.LocationId;  
                if (locationEntity.ParentLocationId != null) { cmd.Parameters["@ParentLocationId"].Value = locationEntity.ParentLocationId; } else { cmd.Parameters["@ParentLocationId"].Value = DBNull.Value; }
                cmd.Parameters["@ClientId"].Value = locationEntity.ClientId;
                if (locationEntity.LocationCategoryId != null) { cmd.Parameters["@LocationCategoryId"].Value = locationEntity.LocationCategoryId; } else { cmd.Parameters["@LocationCategoryId"].Value = DBNull.Value; }
                if (locationEntity.LocationStatusId != null) { cmd.Parameters["@LocationStatusId"].Value = locationEntity.LocationStatusId; } else { cmd.Parameters["@LocationStatusId"].Value = DBNull.Value; }
                if (locationEntity.LocationTypeId != null) { cmd.Parameters["@LocationTypeId"].Value = locationEntity.LocationTypeId; } else { cmd.Parameters["@LocationTypeId"].Value = DBNull.Value; }
                if (locationEntity.LocationTreeLevel != null) { cmd.Parameters["@LocationTreeLevel"].Value = locationEntity.LocationTreeLevel; } else { cmd.Parameters["@LocationTreeLevel"].Value = DBNull.Value; }
                if (locationEntity.Lft != null) { cmd.Parameters["@Lft"].Value = locationEntity.Lft; } else { cmd.Parameters["@Lft"].Value = DBNull.Value; }
                if (locationEntity.Rgt != null) { cmd.Parameters["@Rgt"].Value = locationEntity.Rgt; } else { cmd.Parameters["@Rgt"].Value = DBNull.Value; }
                cmd.Parameters["@LocationCode"].Value = locationEntity.LocationCode ?? (object)DBNull.Value;
                cmd.Parameters["@ExternalReference"].Value = locationEntity.ExternalReference ?? (object)DBNull.Value;
                cmd.Parameters["@Location"].Value = locationEntity.Location ?? (object)DBNull.Value;
                cmd.Parameters["@ShortName"].Value = locationEntity.ShortName ?? (object)DBNull.Value;
                cmd.Parameters["@Description"].Value = locationEntity.Description ?? (object)DBNull.Value;
                cmd.Parameters["@Active"].Value = locationEntity.Active;
                cmd.Parameters["@Archived"].Value = locationEntity.Archived;
                if (locationEntity.HistoryId != null) { cmd.Parameters["@HistoryId"].Value = locationEntity.HistoryId; } else { cmd.Parameters["@HistoryId"].Value = DBNull.Value; }
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

        public static int LoadLocation(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_LOCATION]"
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
