using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class IncidentHasAttributeDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadIncidentHasAttribute(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving IncidentHasAttribute records...");
                TruncateIncidentHasAttribute(cnDW);
                var query = from i in context.IncidentHasAttribute
                                //where task.DueDate > new DateTime(2016, 12, 15)
                            select i;
                foreach (var o in query)
                {
                    //Console.WriteLine($"IncidentTypeHasAgencyId ID: {o.IncidentTypeHasAgencyId}");
                    Console.Write(".");
                    rowCount += InsertIncidentHasAttributeEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing IncidentTypeHasAttribute records");
                LoadIncidentHasAttribute(cnDW);
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

        public static int TruncateIncidentHasAttribute(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_INCIDENT_HAS_ATTRIBUTE]"
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

        public static int InsertIncidentHasAttributeEntity(SqlConnection cnDW, IncidentHasAttributeEntity incidentHasAttributeEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_INCIDENT_HAS_ATTRIBUTE]
                            ( [Id]
                              ,[IncidentId]
                              ,[AttributeId])
                        VALUES
                            ( @Id
                                ,@IncidentId
                                ,@AttributeId)"
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AttributeId", SqlDbType.Int).Direction = ParameterDirection.Input;

                cmd.Parameters["@Id"].Value = incidentHasAttributeEntity.Id;
                cmd.Parameters["@IncidentId"].Value = incidentHasAttributeEntity.IncidentId;
                cmd.Parameters["@AttributeId"].Value = incidentHasAttributeEntity.AttributeId;

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

        public static int LoadIncidentHasAttribute(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_INCIDENT_HAS_ATTRIBUTE]"
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
