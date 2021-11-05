using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class IncidentHasPeriodInfoDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadIncidentHasPeriodInfo(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving IncidentHasPeriodInfo records...");
                TruncateIncidentHasPeriodInfo(cnDW);
                var query = from i in context.IncidentHasPeriodInfo
                                //where task.DueDate > new DateTime(2016, 12, 15)
                            select i;
                foreach (var o in query)
                {
                    //Console.WriteLine($"IncidentTypeHasAgencyId ID: {o.IncidentTypeHasAgencyId}");
                    Console.Write(".");
                    rowCount += InsertIncidentHasPeriodInfoEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing IncidentTypeHasPeriodInfo records");
                LoadIncidentHasPeriodInfo(cnDW);
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

        public static int TruncateIncidentHasPeriodInfo(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_INCIDENT_HAS_PERIOD_INFO]"
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

        public static int InsertIncidentHasPeriodInfoEntity(SqlConnection cnDW, IncidentHasPeriodInfoEntity incidentHasPeriodInfoEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_INCIDENT_HAS_PERIOD_INFO]
                            ( [Id]
                              ,[IncidentId]
                              ,[PeriodInfoId])
                        VALUES
                            ( @Id
                                ,@IncidentId
                                ,@PeriodInfoId)"
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PeriodInfoId", SqlDbType.Int).Direction = ParameterDirection.Input;

                cmd.Parameters["@Id"].Value = incidentHasPeriodInfoEntity.Id;
                cmd.Parameters["@IncidentId"].Value = incidentHasPeriodInfoEntity.IncidentId;
                cmd.Parameters["@PeriodInfoId"].Value = incidentHasPeriodInfoEntity.PeriodInfoId;

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

        public static int LoadIncidentHasPeriodInfo(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_INCIDENT_HAS_PERIOD_INFO]"
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
