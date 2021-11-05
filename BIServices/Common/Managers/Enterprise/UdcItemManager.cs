using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Enterprise
{
    public class UdcItemManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UdcItemManager(BusinessSystemSession e1Session)
        {
            var hash = new Dictionary<string, iDB2Command>
            {
                {"UdcItemManager.GetUdcItem", GetUdcItem()},
                {"UdcItemManager.GetBarDesc", GetBarDesc()}
            };
            foreach (var keyName in hash.Keys.Where(keyName => !e1Session.DbCommands.ContainsKey(keyName)))
            {
                e1Session.DbCommands.Add(keyName, hash[keyName]);
            }
        }

        private static iDB2Command GetUdcItem()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        DRSY,
                        DRRT,
                        DTDL01,
                        DRKY,
                        DRDL01,
                        DRDL02,
                        DRSPHD,
                        DRUPMJ,
                        DRUPMT
                    FROM
                        {ConfigurationManager.AppSettings["ControlLib"]}.F0005 
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0004
                        ON DRSY = DTSY AND
                           DRRT = DTRT
                    WHERE 
                        DRSY = ? AND
                        DRRT = ? AND
                        TRIM(DRKY) = ?"
            };
            cmd.Parameters.Add(new iDB2Parameter("DRSY", iDB2DbType.iDB2Graphic, 4));
            cmd.Parameters.Add(new iDB2Parameter("DRRT", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("DRKY", iDB2DbType.iDB2Graphic, 10));
            return cmd;
        }

        /*********************************************************************************
         * Get UDC Items Dynamic
         *******************************************************************************/
        private static iDB2Command GetUdcItemsDynamic(List<string> filters)
        {
            var filterString = new StringBuilder();
            if (filters.Any()) filterString.Append("WHERE ");
            foreach (var filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = string.Format(@"
	                SELECT 
                        DRSY,
                        DRRT,
                        DTDL01,
                        DRKY,
                        DRDL01,
                        DRDL02,
                        DRSPHD,
                        DRUPMJ,
                        DRUPMT
                    FROM
                        {0}.F0005 
                        LEFT OUTER JOIN {0}.F0004
                        ON DRSY = DTSY AND
                           DRRT = DTRT
                    {1}",
                    ConfigurationManager.AppSettings["ControlLib"],
                    filterString)
            };
            return cmd;
        }

        /*********************************************************************************
         * Methods
         ********************************************************************************/
        public UdcItems GetUdcItems(BusinessSystemSession e1Session, string[] productCodes = null, string userCode = null, TimeSpan? timespan = null)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var udcItems = new UdcItems();
            var filters = new List<string>();
            var parameters = new List<iDB2Parameter>();
            try
            {
                // Product Codes
                if (productCodes != null && productCodes.Any())
                {
                    var strInClause = "";
                    foreach (var productCode in productCodes)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("ProductCodes", iDB2DbType.iDB2Graphic, 4));
                        parameters.Last().Value = productCode;
                    }
                    filters.Add("LEFT(DRSY,2) IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND \r\n");
                }
                // User Code
                if (!string.IsNullOrEmpty(userCode))
                {
                    filters.Add("DRRT = ? AND \r\n");
                    parameters.Add(new iDB2Parameter("UserCode", iDB2DbType.iDB2Graphic, 2));
                    parameters.Last().Value = userCode;
                }
                // Timespan
                if (timespan != null && timespan.Value.TotalSeconds > 0)
                {
                    var ts = new E1TimeSpan();
                    ts.SetTimeSpan((TimeSpan)timespan);
                    filters.Add("( ( ( DRUPMJ = ? AND DRUPMT >= ? ) AND DRUPMJ <= ? ) OR ( ( DTUPMJ = ? AND DTUPMT >= ? ) AND DTUPMJ <= ? ) ) AND \r\n");
                    parameters.Add(new iDB2Parameter("Today", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = ts.DateComponent1;
                    parameters.Add(new iDB2Parameter("Now", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = ts.TimeComponent1;
                    parameters.Add(new iDB2Parameter("Tomorrow", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = ts.DateComponent2;
                    parameters.Add(new iDB2Parameter("Today1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = ts.DateComponent1;
                    parameters.Add(new iDB2Parameter("Now1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = ts.TimeComponent1;
                    parameters.Add(new iDB2Parameter("Tomorrow1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = ts.DateComponent2;
                }
                iDB2Command cmd;
                if (filters.Any())
                {
                    filters[filters.Count - 1] = filters.Last().ToUpper().Replace(" AND \r\n", "");
                    cmd = GetUdcItemsDynamic(filters);
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }
                }
                else
                {
                    cmd = GetUdcItemsDynamic(filters);
                }
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace("CN: " + e1Session.DbConnection.ConnectionString);
                Logger.Trace("SQL: " + Environment.NewLine + cmd.CommandText);
                Logger.Trace("PARMS: " + string.Join(", ", cmd.Parameters.Cast<iDB2Parameter>().AsQueryable().Select(parm => parm.Value)));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var udcItem = new UdcItem
                            {
                                ProductCode = CastDbNull.To(reader["DRSY"], string.Empty).Trim(),
                                UserCode = CastDbNull.To(reader["DRRT"], string.Empty).Trim(),
                                UserCodeType = CastDbNull.To(reader["DTDL01"], string.Empty).Trim(),
                                UserValue = CastDbNull.To(reader["DRKY"], string.Empty).Trim(),
                                Description1 = CastDbNull.To(reader["DRDL01"], string.Empty).Trim(),
                                Description2 = CastDbNull.To(reader["DRDL02"], string.Empty).Trim(),
                                SpecialHandlingCode = CastDbNull.To(reader["DRSPHD"], string.Empty).Trim()
                            };
                            var updateDateId = CastDbNull.To<decimal>(reader["DRUPMJ"], 0);
                            var updateTimeId = CastDbNull.To<decimal>(reader["DRUPMT"], 0);
                            udcItem.UpdateDateTime = Utilities.Common.CastToDateTime(updateDateId, updateTimeId);
                            udcItems.Add(udcItem);
                        }

                    reader.Close();
                }
                Logger.Trace("ROWS: " + udcItems.Count);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + e1Session.DbConnection.ConnectionString + " JobName: " + e1Session.DbConnection.JobName + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            return udcItems;
        }

        public UdcItem GetUdcItem(BusinessSystemSession e1Session, string productCode, string userCode, string userKey)
        {
            var item = new UdcItem();
            try
            {
                var cmd = (e1Session.DbCommands.ContainsKey("UdcItemManager.GetUdcItem")) ? e1Session.DbCommands["UdcItemManager.GetUdcItem"] : GetUdcItem();
                cmd.Parameters["DRSY"].Value = productCode;
                cmd.Parameters["DRRT"].Value = userCode;
                cmd.Parameters["DRKY"].Value = userKey;
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        item.ProductCode = CastDbNull.To(reader["DRSY"], string.Empty).Trim();
                        item.UserCode = CastDbNull.To(reader["DRRT"], string.Empty).Trim();
                        item.UserCodeType = CastDbNull.To(reader["DTDL01"], string.Empty).Trim();
                        item.UserValue = CastDbNull.To(reader["DRKY"], string.Empty).Trim();
                        item.Description1 = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                        item.Description2 = CastDbNull.To(reader["DRDL02"], string.Empty).Trim();
                        item.SpecialHandlingCode = CastDbNull.To(reader["DRSPHD"], string.Empty).Trim();
                        var updateDateId = CastDbNull.To<decimal>(reader["DRUPMJ"], 0);
                        var updateTimeId = CastDbNull.To<decimal>(reader["DRUPMT"], 0);
                        item.UpdateDateTime = Utilities.Common.CastToDateTime(updateDateId, updateTimeId);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return item;
        }

        public string GetBarDesc(BusinessSystemSession e1Session, decimal mCode)
        {
            var desc = "";
            try
            {
                var cmd = (e1Session.DbCommands.ContainsKey("UdcItemManager.GetBarDesc")) ? e1Session.DbCommands["UdcItemManager.GetBarDesc"] : GetBarDesc();
                cmd.Parameters["DRKY"].Value = mCode;
                cmd.Connection = e1Session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        desc = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return desc;
        }

        private static iDB2Command GetBarDesc()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
	                SELECT 
                        DRDL01
                    FROM
                        {ConfigurationManager.AppSettings["ControlLib"]}.F0005 
                    WHERE 
                        DRSY = '57' AND
                        DRRT = 'BR' AND
                        CAST(TRIM(DRKY) AS INT) = ?"
            };
            cmd.Parameters.Add(new iDB2Parameter("DRKY", iDB2DbType.iDB2Integer));
            return cmd;
        }
    }
}
