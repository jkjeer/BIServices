using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    internal class IncidentTypeHasAgencyDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadIncidentTypeHasAgency(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving IncidentTypeHasAgency records...");
                TruncateIncidentTypeHasAgency(cnDW);
                var query = from i in context.IncidentTypeHasAgency
                            select i;
                foreach (var o in query)
                {
                    //Console.WriteLine($"IncidentTypeHasAgencyId ID: {o.Id}");
                    Console.Write(".");
                    rowCount += InsertIncidentTypeHasAgencyEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing IncidentTypeHasAgency records");
                LoadIncidentTypeHasAgency(cnDW);
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

        public static int TruncateIncidentTypeHasAgency(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_INCIDENT_TYPE_HAS_AGENCY]"
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

        public static int InsertIncidentTypeHasAgencyEntity(SqlConnection cnDW, IncidentTypeHasAgencyEntity IncidentTypeHasAgencyEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_INCIDENT_TYPE_HAS_AGENCY]
                            ( [Id]
                              ,[IncidentId]
                              ,[IncidentTypeId]
                              ,[AgencyId]
                              ,[ReportDate]
                              ,[ReportNumber]
                              ,[AgencyInfo])
                        VALUES
                            ( @Id
                                ,@IncidentId
                                ,@IncidentTypeId
                                ,@AgencyId
                                ,@ReportDate
                                ,@ReportNumber
                                ,@AgencyInfo)"
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentTypeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AgencyId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ReportDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ReportNumber", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AgencyInfo", SqlDbType.VarChar).Direction = ParameterDirection.Input;

                cmd.Parameters["@Id"].Value = IncidentTypeHasAgencyEntity.Id;
                cmd.Parameters["@IncidentId"].Value = IncidentTypeHasAgencyEntity.IncidentId;
                cmd.Parameters["@IncidentTypeId"].Value = IncidentTypeHasAgencyEntity.IncidentTypeId;
                cmd.Parameters["@AgencyId"].Value = IncidentTypeHasAgencyEntity.AgencyId;
                cmd.Parameters["@ReportDate"].Value = IncidentTypeHasAgencyEntity.ReportDate ?? (object)DBNull.Value;
                cmd.Parameters["@ReportNumber"].Value = IncidentTypeHasAgencyEntity.ReportNumber ?? (object)DBNull.Value;
                cmd.Parameters["@AgencyInfo"].Value = IncidentTypeHasAgencyEntity.AgencyInfo ?? (object)DBNull.Value;

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

        public static int LoadIncidentTypeHasAgency(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_INCIDENT_TYPE_HAS_AGENCY]"
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
