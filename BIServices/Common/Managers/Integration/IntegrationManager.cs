using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Integration
{
    public class IntegrationManager
    {
        //public IntegrationManager()
        //{
        //}
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static decimal GetCurrentCorpIntQAUKID(SqlConnection cnCorpIntPY)
        {
            decimal nextValue = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = @"SELECT current_value FROM sys.sequences WHERE name = 'QAUKID'"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        long currentValue = CastDbNull.To<long>(reader["current_value"], 0);
                        nextValue = Convert.ToDecimal(currentValue);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: 1");
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return nextValue;
        }

        public static SqlDmlResponse UpdateCorpIntegrationQAUKID(SqlConnection cnCorpIntPY, decimal maxE1PYQAUKID)
        {
            var response = new SqlDmlResponse();
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = $@"ALTER SEQUENCE E1.QAUKID RESTART WITH {maxE1PYQAUKID}"
                };
                response.Init(cmd);
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return response;
        }

        public static decimal GetCurrentCorpIntBIEDBT(SqlConnection cnCorpIntPY)
        {
            decimal nextValue = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = @"SELECT current_value FROM sys.sequences WHERE name = 'BIEDBT'"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        long currentValue = CastDbNull.To<long>(reader["current_value"], 0);
                        nextValue = Convert.ToDecimal(currentValue);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: 1");
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return nextValue;
        }

        public static decimal GetCurrentCorpIntSFEDBT(SqlConnection cnCorpIntPY)
        {
            decimal nextValue = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = @"SELECT current_value FROM sys.sequences WHERE name = 'SFEDBT'"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        long currentValue = CastDbNull.To<long>(reader["current_value"], 0);
                        nextValue = Convert.ToDecimal(currentValue);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: 1");
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return nextValue;
        }

        public static decimal GetCurrentCorpIntBIUKID(SqlConnection cnCorpIntPY)
        {
            decimal nextValue = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = @"SELECT current_value FROM sys.sequences WHERE name = 'BIUKID'"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        long currentValue = CastDbNull.To<long>(reader["current_value"], 0);
                        nextValue = Convert.ToDecimal(currentValue);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: 1");
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return nextValue;
        }

        public static SqlDmlResponse UpdateCorpIntegrationBIUKID(SqlConnection cnCorpIntPY, decimal maxE1PYBIUKID)
        {
            var response = new SqlDmlResponse();
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = $@"ALTER SEQUENCE E1.BIUKID RESTART WITH {maxE1PYBIUKID}"
                };
                response.Init(cmd);
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return response;
        }

        public static SqlDmlResponse UpdateCorpIntegrationBIEDBT(SqlConnection cnCorpIntPY, decimal maxE1PYBIEDBT)
        {
            var response = new SqlDmlResponse();
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = $@"ALTER SEQUENCE E1.BIEDBT RESTART WITH {maxE1PYBIEDBT}"
                };
                response.Init(cmd);
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return response;
        }

        public static SqlDmlResponse UpdateCorpIntegrationSFEDBT(SqlConnection cnCorpIntPY, decimal maxE1PYSFEDBT)
        {
            var response = new SqlDmlResponse();
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = $@"ALTER SEQUENCE E1.SFEDBT RESTART WITH {maxE1PYSFEDBT}"
                };
                response.Init(cmd);
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return response;
        }

        public static decimal GetCurrentCorpIntSFUKID(SqlConnection cnCorpIntPY)
        {
            decimal nextValue = 0;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = @"SELECT current_value FROM sys.sequences WHERE name = 'SFUKID'"
                };
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        long currentValue = CastDbNull.To<long>(reader["current_value"], 0);
                        nextValue = Convert.ToDecimal(currentValue);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: 1");
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return nextValue;
        }

        public static SqlDmlResponse UpdateCorpIntegrationSFUKID(SqlConnection cnCorpIntPY, decimal maxE1PYSFUKID)
        {
            var response = new SqlDmlResponse();
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnCorpIntPY,
                    CommandType = CommandType.Text,
                    CommandText = $@"ALTER SEQUENCE E1.SFUKID RESTART WITH {maxE1PYSFUKID}"
                };
                response.Init(cmd);
                var rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return response;
        }

        /**********************************************************************
         *  Subscriptions
         *********************************************************************/
        private static SqlCommand GetEnabledSubscriptions()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    SELECT
                      bu.MCU ,
                      tfr.E1TransferTypeName ,
                      sub.E1MappingSubscriptionId ,
                      sub.E1MappingTableId ,
                      tbl.MappingTableDesc ,
                      ds.E1MappingDataSourceId ,
                      ds.ConnectionString ,
                      sub.TableName ,
                      tbl.E1StagingTableName ,
                      tbl.E1StagingTableView ,
                      sub.SelectionCriteria ,
                      sub.Active ,
                      sub.IsEnabled
                    FROM
                      E1MappingSubscription sub
                      LEFT OUTER JOIN E1MappingTable tbl
                      ON tbl.E1MappingTableId = sub.E1MappingTableId
                      LEFT OUTER JOIN E1BusinessUnit bu
                      ON bu.E1BusinessUnitId = sub.E1BusinessUnitId
                      LEFT OUTER JOIN E1MappingDataSource ds
                      ON ds.E1MappingDataSourceId = sub.E1MappingDataSourceId
                      LEFT OUTER JOIN E1MappingDataSource ds1
                      ON ds1.E1MappingDataSourceId = tbl.E1MappingDataSourceId
                      LEFT OUTER JOIN E1TransferType tfr
                      ON tfr.E1TransferTypeId = tbl.E1TransferTypeId
                    WHERE
                      sub.IsEnabled = 1 AND
                      bu.MCU = @Plant
                    ORDER BY
                      tfr.E1TransferTypeName ,
                      tbl.MappingTableDesc"
            };
            cmd.Parameters.Add(new SqlParameter("@Plant", SqlDbType.VarChar, 50));
            return cmd;
        }

        public Types.Integration.Subscriptions GetEnabledSubscriptions(string plantCode)
        {
            var subs = new Types.Integration.Subscriptions();
            var cn = new SqlConnection();
            try
            {
                var map = new PlantMap();
                var plantNum = map.GetAttributesByCode(plantCode).NumericCode;

                var cmd = GetEnabledSubscriptions();
                cmd.Parameters[0].Value = plantNum;
                cn = ConnectionManager.GetSqlConnectionFromKey("INT");
                cn.Open();
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var sub = new Types.Integration.Subscription
                        {
                            Plant = CastDbNull.To(reader["MCU"], string.Empty),
                            TransferType = CastDbNull.To(reader["E1TransferTypeName"], string.Empty),
                            SubscriptionId = CastDbNull.To(reader["E1MappingSubscriptionId"], 0),
                            MappingId = CastDbNull.To(reader["E1MappingTableId"], 0),
                            MappingName = CastDbNull.To(reader["MappingTableDesc"], string.Empty),
                            DataSourceId = CastDbNull.To(reader["E1MappingDataSourceId"], 0),
                            ConnectionString = CastDbNull.To(reader["ConnectionString"], string.Empty),
                            TableName = CastDbNull.To(reader["TableName"], string.Empty),
                            StagingTableName = CastDbNull.To(reader["E1StagingTableName"], string.Empty),
                            StagingViewName = CastDbNull.To(reader["E1StagingTableView"], string.Empty),
                            SelectionCriteria = CastDbNull.To(reader["SelectionCriteria"], string.Empty),
                            IsActive = Convert.ToBoolean(CastDbNull.To(reader["Active"], 0))
                        };
                        subs.Add(sub);
                    }

                    reader.Close();
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                cn.Close();
            }

            return subs;
        }

        private static SqlCommand PlantsWithInactiveSubscriptions()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    SELECT
                      bu.MCU
                    FROM
                      E1MappingSubscription sub
                      LEFT OUTER JOIN E1BusinessUnit bu
                      ON bu.E1BusinessUnitId = sub.E1BusinessUnitId
                    WHERE
                        sub.IsEnabled = 1 AND
			            sub.Active = 0
                    GROUP BY 
                        bu.MCU"
            };
            return cmd;
        }

        public List<string> GetPlantsWithInactiveSubscriptions()
        {
            var plants = new List<string>();
            var cn = new SqlConnection();
            try
            {
                var map = new PlantMap();
                var cmd = PlantsWithInactiveSubscriptions();
                cn = ConnectionManager.GetSqlConnectionFromKey("INT");
                cn.Open();
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var plantNum = CastDbNull.To(reader["MCU"], string.Empty);
                        var plantCode = map.GetAttributesByNum(plantNum).Code;
                        plants.Add(plantCode);
                    }

                    reader.Close();
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                cn.Close();
            }

            return plants;
        }

        /**********************************************************************
         *  Activation
         *********************************************************************/
        private static SqlCommand ActivateSubscriptions()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    UPDATE
                      sub
                    SET
                      sub.Active = 1
                    FROM
                      E1MappingSubscription sub
                      INNER JOIN E1BusinessUnit bu
                      ON bu.E1BusinessUnitId = sub.E1BusinessUnitId
                    WHERE
                      sub.IsEnabled = 1 AND
                      bu.MCU = @Plant"
            };
            cmd.Parameters.Add(new SqlParameter("@Plant", SqlDbType.VarChar, 50));
            return cmd;
        }

        public static SqlDmlResponse ActivateSubscriptions(string plantCode)
        {
            var response = new SqlDmlResponse();
            var cn = new SqlConnection();
            try
            {
                var map = new PlantMap();
                var plantNum = map.GetAttributesByCode(plantCode).NumericCode;

                cn = ConnectionManager.GetSqlConnectionFromKey("INT");
                var cmd = ActivateSubscriptions();
                cmd.Parameters[0].Value = plantNum;
                cmd.Connection = cn;
                response.Init(cmd);
                cn.Open();
                int rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
               cn.Close(); 
            }
            return response;
        }

        private static SqlCommand InactivateSubscriptions()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    UPDATE
                      sub
                    SET
                      sub.Active = 0
                    FROM
                      E1MappingSubscription sub
                      INNER JOIN E1BusinessUnit bu
                      ON bu.E1BusinessUnitId = sub.E1BusinessUnitId
                    WHERE
                      sub.IsEnabled = 1 AND
                      bu.MCU = @Plant"
            };
            cmd.Parameters.Add(new SqlParameter("@Plant", SqlDbType.VarChar, 50));
            return cmd;
        }

        public SqlDmlResponse InactivateSubscriptions(string plantCode)
        {
            var response = new SqlDmlResponse();
            var cn = new SqlConnection();
            try
            {
                var map = new PlantMap();
                var plantNum = map.GetAttributesByCode(plantCode).NumericCode;

                cn = ConnectionManager.GetSqlConnectionFromKey("INT");
                var cmd = InactivateSubscriptions();
                cmd.Parameters[0].Value = plantNum;
                cmd.Connection = cn;
                response.Init(cmd);
                cn.Open();

                int rows = cmd.ExecuteNonQuery();
                response.RowsAffected = rows;
                response.EndTime = DateTime.Now;
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                cn.Close();
            }
            return response;
        }

        /**********************************************************************
         *  Ping Results
         *********************************************************************/
        private static SqlCommand GetPingResults()
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    SELECT
                        A.PLANT_NAME,
                        A.PING_ADDR,
                        A.MIN_DATE AS [START_DATE],
                        A.MAX_DATE AS [END_DATE],
                        A.REQUESTS,
                        ISNULL(PINGERR.REQUESTS, 0) AS ERR,
                        ISNULL(SAT.REQUESTS, 0) AS SAT,
                        ISNULL(OPT.REQUESTS, 0) AS OPT,
                        FORMAT(ISNULL(PINGERR.REQUESTS, 0) / (A.REQUESTS * 1.0), 'p') AS [ERR%],
                        FORMAT(ISNULL(SAT.REQUESTS, 0) / (A.REQUESTS * 1.0), 'p') AS [SAT%],
                        FORMAT(ISNULL(OPT.REQUESTS, 0) / (A.REQUESTS * 1.0), 'p') AS [OPT%],
                        ISNULL(SAT.RESPONSE_TIME_AVG, 0) AS SAT_MS,
                        ISNULL(OPT.RESPONSE_TIME_AVG, 0) AS OPT_MS
                    FROM
                        (
                            SELECT
                                T.PLANT_NAME,
                                MIN(T.PING_ADDR) AS PING_ADDR,
                                COUNT(*) AS REQUESTS,
                                MIN(E.WRITTEN_DATE) AS MIN_DATE,
                                MAX(E.WRITTEN_DATE) AS MAX_DATE
                            FROM
                                DW.PING_EVENT E WITH (NOLOCK)
                                INNER JOIN DW.PING_TARGET T WITH (NOLOCK)
                                ON  T.TARGET_ID = E.TARGET_ID
                            WHERE
                                E.WRITTEN_DATE >= @START_DATE AND
                                E.WRITTEN_DATE < @END_DATE
                            GROUP BY
                                T.PLANT_NAME
                        ) A
                        LEFT OUTER JOIN
                        (
                            SELECT
                                T.PLANT_NAME,
                                COUNT(*) AS REQUESTS
                            FROM
                                DW.PING_EVENT E WITH (NOLOCK)
                                INNER JOIN DW.PING_TARGET T WITH (NOLOCK)
                                ON  T.TARGET_ID = E.TARGET_ID
                            WHERE
                                E.WRITTEN_DATE >= @START_DATE AND
                                E.WRITTEN_DATE < @END_DATE AND
                                E.IS_PING_ERROR = 1
                            GROUP BY
                                T.PLANT_NAME
                        ) PINGERR
                        ON  PINGERR.PLANT_NAME = A.PLANT_NAME
                        LEFT OUTER JOIN
                        (
                            SELECT
                                T.PLANT_NAME,
                                COUNT(*) AS REQUESTS,
                                SUM(E.PING_RESPONSE_TIME) AS RESPONSE_TIME,
                                AVG(E.PING_RESPONSE_TIME) AS RESPONSE_TIME_AVG
                            FROM
                                DW.PING_EVENT E WITH (NOLOCK)
                                INNER JOIN DW.PING_TARGET T WITH (NOLOCK)
                                ON  T.TARGET_ID = E.TARGET_ID
                            WHERE
                                E.WRITTEN_DATE >= @START_DATE AND
                                E.WRITTEN_DATE < @END_DATE AND
                                E.PING_RESPONSE_TIME >= @SAT_MS
                            GROUP BY
                                T.PLANT_NAME
                        ) SAT
                        ON  SAT.PLANT_NAME = A.PLANT_NAME
                        LEFT OUTER JOIN
                        (
                            SELECT
                                T.PLANT_NAME,
                                COUNT(*) AS REQUESTS,
                                SUM(E.PING_RESPONSE_TIME) AS RESPONSE_TIME,
                                AVG(E.PING_RESPONSE_TIME) AS RESPONSE_TIME_AVG
                            FROM
                                DW.PING_EVENT E WITH (NOLOCK)
                                INNER JOIN DW.PING_TARGET T WITH (NOLOCK)
                                ON  T.TARGET_ID = E.TARGET_ID
                            WHERE
                                E.WRITTEN_DATE >= @START_DATE AND
                                E.WRITTEN_DATE < @END_DATE AND
                                E.PING_RESPONSE_TIME < @SAT_MS AND
                                E.IS_PING_ERROR = 0
                            GROUP BY
                                T.PLANT_NAME
                        ) OPT
                        ON  OPT.PLANT_NAME = A.PLANT_NAME
                    WHERE
                        A.PLANT_NAME NOT LIKE 'CORP%'
                    ORDER BY
                        ISNULL(OPT.REQUESTS, 0) / (A.REQUESTS * 1.0) DESC,
                        ISNULL(OPT.RESPONSE_TIME_AVG, 0) ASC;"
            };
            cmd.Parameters.Add(new SqlParameter("@START_DATE", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@END_DATE", SqlDbType.DateTime));
            return cmd;
        }

        public PingResults GetPingResults(string plantCode,string startDate, string endDate)
        {
            var results = new PingResults();
            var cn = new SqlConnection();
            try
            {
                var cmd = GetPingResults();
                cmd.Parameters[0].Value = startDate;
                cmd.Parameters[0].Value = endDate;
                cn = ConnectionManager.GetSqlConnectionFromKey("INT");
                cn.Open();
                cmd.Connection = cn;
                //IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                //while (reader.Read())
                //{
                //    var sub = new Subscription();
                //    sub.Plant = CastDbNull.To<String>(reader["MCU"], String.Empty);
                //    sub.TransferType = CastDbNull.To<String>(reader["E1TransferTypeName"], String.Empty);
                //    sub.SubscriptionId = CastDbNull.To<int>(reader["E1MappingSubscriptionId"], 0);
                //    sub.MappingId = CastDbNull.To<int>(reader["E1MappingTableId"], 0);
                //    sub.MappingName = CastDbNull.To<String>(reader["MappingTableDesc"], String.Empty);
                //    sub.DataSourceId = CastDbNull.To<int>(reader["E1MappingDataSourceId"], 0);
                //    sub.ConnectionString = CastDbNull.To<String>(reader["ConnectionString"], String.Empty);
                //    sub.TableName = CastDbNull.To<String>(reader["TableName"], String.Empty);
                //    sub.StagingTableName = CastDbNull.To<String>(reader["E1StagingTableName"], String.Empty);
                //    sub.StagingViewName = CastDbNull.To<String>(reader["E1StagingTableView"], String.Empty);
                //    sub.SelectionCriteria = CastDbNull.To<String>(reader["SelectionCriteria"], String.Empty);
                //    sub.IsActive = Convert.ToBoolean(CastDbNull.To<int>(reader["Active"], 0));
                //    subs.Add(sub);
                //}
                //reader.Close();
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                cn.Close();
            }
            return results;
        }
    }
}
