using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class AttributeDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadAttribute(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving Attribute records...");
                TruncateAttribute(cnDW);
                var query = from a in context.Attribute
                                //where task.DueDate > new DateTime(2016, 12, 15)
                            select a;
                foreach (var o in query)
                {
                    //Console.WriteLine($"AttributeId ID: {o.AttributeId}");
                    Console.Write(".");
                    rowCount += InsertAttributeEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing Attribute records");
                LoadAttribute(cnDW);
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

        public static int TruncateAttribute(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_ATTRIBUTE]"
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

        public static int InsertAttributeEntity(SqlConnection cnDW, AttributeEntity attributeEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_ATTRIBUTE]
                            ( [AttributeId]
                              ,[ClientId]
                              ,[AttributeTypeId]
                              ,[LocationId]
                              ,[PersonId]
                              ,[ListItemId]
                              ,[ListOptionId]
                              ,[BooleanValue]
                              ,[DateValue]
                              ,[NumericValue]
                              ,[UnitCode]
                              ,[AttributeName]
                              ,[TextValue]
                              ,[BinaryValue])
                        VALUES
                            ( @AttributeId
                                ,@ClientId
                                ,@AttributeTypeId
                                ,@LocationId
                                ,@PersonId
                                ,@ListItemId
                                ,@ListOptionId
                                ,@BooleanValue
                                ,@DateValue
                                ,@NumericValue
                                ,@UnitCode
                                ,@AttributeName
                                ,@TextValue
                                ,@BinaryValue)"
                };
                cmd.Parameters.Add("@AttributeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ClientId", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AttributeTypeId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PersonId", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ListItemId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ListOptionId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@BooleanValue", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DateValue", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@NumericValue", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@UnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AttributeName", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@TextValue", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@BinaryValue", SqlDbType.VarBinary).Direction = ParameterDirection.Input;

                cmd.Parameters["@AttributeId"].Value = attributeEntity.AttributeId;
                cmd.Parameters["@ClientId"].Value = attributeEntity.ClientId ?? (object)DBNull.Value;
                cmd.Parameters["@AttributeTypeId"].Value = attributeEntity.AttributeTypeId;
                if (attributeEntity.LocationId != null) { cmd.Parameters["@LocationId"].Value = attributeEntity.LocationId; } else { cmd.Parameters["@LocationId"].Value = DBNull.Value; }
                cmd.Parameters["@PersonId"].Value = attributeEntity.PersonId ?? (object)DBNull.Value;
                if (attributeEntity.ListItemId != null) { cmd.Parameters["@ListItemId"].Value = attributeEntity.ListItemId; } else { cmd.Parameters["@ListItemId"].Value = DBNull.Value; }
                if (attributeEntity.ListOptionId != null) { cmd.Parameters["@ListOptionId"].Value = attributeEntity.ListOptionId; } else { cmd.Parameters["@ListOptionId"].Value = DBNull.Value; }
                cmd.Parameters["@BooleanValue"].Value = attributeEntity.BooleanValue ?? (object)DBNull.Value;
                cmd.Parameters["@DateValue"].Value = attributeEntity.DateValue ?? (object)DBNull.Value;
                if (attributeEntity.NumericValue != null) { cmd.Parameters["@NumericValue"].Value = attributeEntity.NumericValue; } else { cmd.Parameters["@NumericValue"].Value = DBNull.Value; }
                cmd.Parameters["@UnitCode"].Value = attributeEntity.UnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@AttributeName"].Value = attributeEntity.AttributeName ?? (object)DBNull.Value;
                cmd.Parameters["@TextValue"].Value = attributeEntity.TextValue ?? (object)DBNull.Value;
                cmd.Parameters["@BinaryValue"].Value = attributeEntity.BinaryValue ?? (object)DBNull.Value;

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

        public static int LoadAttribute(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_ATTRIBUTE]"
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
