using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class IncidentHasListItemDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadIncidentHasListItem(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving IncidentHasListItem records...");
                TruncateIncidentHasListItem(cnDW);
                var query = from i in context.IncidentHasListItem
                            select i;
                foreach (var o in query)
                {
                    //Console.WriteLine($"IncidentHasListItemId ID: {o.IncidentHasListItemId}");
                    Console.Write(".");
                    rowCount += InsertIncidentHasListItemEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing IncidentHasListItem records");
                LoadIncidentHasListItem(cnDW);
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

        public static int TruncateIncidentHasListItem(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_INCIDENT_HAS_LIST_ITEM]"
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

        public static int InsertIncidentHasListItemEntity(SqlConnection cnDW, IncidentHasListItemEntity incidentHasListItemEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_INCIDENT_HAS_LIST_ITEM]
                            ( [Id]
                              ,[IncidentId]
                              ,[ListItemId]
                              ,[ExtraInformation])
                        VALUES
                            ( @Id
                                ,@IncidentId
                                ,@ListItemId
                                ,@ExtraInformation)"
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ListItemId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ExtraInformation", SqlDbType.VarChar).Direction = ParameterDirection.Input;

                cmd.Parameters["@Id"].Value = incidentHasListItemEntity.Id;
                cmd.Parameters["@IncidentId"].Value = incidentHasListItemEntity.IncidentId;
                cmd.Parameters["@ListItemId"].Value = incidentHasListItemEntity.ListItemId;
                cmd.Parameters["@ExtraInformation"].Value = incidentHasListItemEntity.ExtraInformation ?? (object)DBNull.Value;

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

        public static int LoadIncidentHasListItem(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_INCIDENT_HAS_LIST_ITEM]"
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
