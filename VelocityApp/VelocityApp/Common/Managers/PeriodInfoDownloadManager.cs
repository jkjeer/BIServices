using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class PeriodInfoDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadPeriodInfo(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving PeriodInfo records...");
                TruncatePeriodInfo(cnDW);
                var query = from p in context.PeriodInfo
                                //where task.DueDate > new DateTime(2016, 12, 15)
                            select p;
                foreach (var o in query)
                {
                    //Console.WriteLine($"PeriodInfoId ID: {o.PeriodInfoId}");
                    Console.Write(".");
                    rowCount += InsertPeriodInfoEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing PeriodInfo records");
                LoadPeriodInfo(cnDW);
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

        public static int TruncatePeriodInfo(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_PERIOD_INFO]"
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

        public static int InsertPeriodInfoEntity(SqlConnection cnDW, PeriodInfoEntity periodInfoEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_PERIOD_INFO]
                            ( [PeriodInfoId]
                              ,[PeriodInfoTypeId]
                              ,[CalendarTypeId]
                              ,[PeriodTypeId]
                              ,[PeriodLabelId]
                              ,[AttributeId]
                              ,[StartDate]
                              ,[EndDate]
                              ,[Year]
                              ,[CalculatedStartDate]
                              ,[CalculatedEndDate])
                        VALUES
                            ( @PeriodInfoId
                                ,@PeriodInfoTypeId
                                ,@CalendarTypeId
                                ,@PeriodTypeId
                                ,@PeriodLabelId
                                ,@AttributeId
                                ,@StartDate
                                ,@EndDate
                                ,@Year
                                ,@CalculatedStartDate
                                ,@CalculatedEndDate)"
                };
                cmd.Parameters.Add("@PeriodInfoId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PeriodInfoTypeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@CalendarTypeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PeriodTypeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PeriodLabelId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AttributeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Year", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@CalculatedStartDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@CalculatedEndDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;

                cmd.Parameters["@PeriodInfoId"].Value = periodInfoEntity.PeriodInfoId;
                cmd.Parameters["@PeriodInfoTypeId"].Value = periodInfoEntity.PeriodInfoTypeId;
                if (periodInfoEntity.CalendarTypeId != null) { cmd.Parameters["@CalendarTypeId"].Value = periodInfoEntity.CalendarTypeId; } else { cmd.Parameters["@CalendarTypeId"].Value = DBNull.Value; }
                if (periodInfoEntity.PeriodTypeId != null) { cmd.Parameters["@PeriodTypeId"].Value = periodInfoEntity.PeriodTypeId; } else { cmd.Parameters["@PeriodTypeId"].Value = DBNull.Value; }
                if (periodInfoEntity.PeriodLabelId != null) { cmd.Parameters["@PeriodLabelId"].Value = periodInfoEntity.PeriodLabelId; } else { cmd.Parameters["@PeriodLabelId"].Value = DBNull.Value; }
                if (periodInfoEntity.AttributeId != null) { cmd.Parameters["@AttributeId"].Value = periodInfoEntity.AttributeId; } else { cmd.Parameters["@AttributeId"].Value = DBNull.Value; }
                cmd.Parameters["@StartDate"].Value = periodInfoEntity.StartDate ?? (object)DBNull.Value;
                cmd.Parameters["@EndDate"].Value = periodInfoEntity.EndDate ?? (object)DBNull.Value;
                if (periodInfoEntity.Year != null) { cmd.Parameters["@Year"].Value = periodInfoEntity.Year; } else { cmd.Parameters["@Year"].Value = DBNull.Value; }
                cmd.Parameters["@CalculatedStartDate"].Value = periodInfoEntity.CalculatedStartDate ?? (object)DBNull.Value;
                cmd.Parameters["@CalculatedEndDate"].Value = periodInfoEntity.CalculatedEndDate ?? (object)DBNull.Value;

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

        public static int LoadPeriodInfo(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_PERIOD_INFO]"
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
