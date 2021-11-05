using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class LocationHasPeriodInfoDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadLocationHasPeriodInfo(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving LocationHasPeriodInfo records...");
                TruncateLocationHasPeriodInfo(cnDW);
                var query = from l in context.LocationHasPeriodInfo
                                //where task.DueDate > new DateTime(2016, 12, 15)
                            select l;
                foreach (var o in query)
                {
                    //Console.WriteLine($"LocationHasPeriodInfoId ID: {o.LocationHasPeriodInfoId}");
                    Console.Write(".");
                    rowCount += InsertLocationHasPeriodInfoEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing LocationHasPeriodInfo records");
                LoadLocationHasPeriodInfo(cnDW);
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

        public static int TruncateLocationHasPeriodInfo(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_LOCATION_HAS_PERIOD_INFO]"
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

        public static int InsertLocationHasPeriodInfoEntity(SqlConnection cnDW, LocationHasPeriodInfoEntity locationHasPeriodInfoEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_LOCATION_HAS_PERIOD_INFO]
                            ( [Id]
                              ,[LocationId]
                              ,[PeriodInfoId])
                        VALUES
                            ( @Id
                                ,@LocationId
                                ,@PeriodInfoId)"
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PeriodInfoId", SqlDbType.Int).Direction = ParameterDirection.Input;

                cmd.Parameters["@Id"].Value = locationHasPeriodInfoEntity.Id;
                cmd.Parameters["@LocationId"].Value = locationHasPeriodInfoEntity.LocationId;
                cmd.Parameters["@PeriodInfoId"].Value = locationHasPeriodInfoEntity.PeriodInfoId;

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

        public static int LoadLocationHasPeriodInfo(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_LOCATION_HAS_PERIOD_INFO]"
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
