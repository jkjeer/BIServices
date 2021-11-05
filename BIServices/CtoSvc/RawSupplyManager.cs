using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.CtoSvc
{
    public class RawSupplyManager
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public RawSupplyManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("RawSupplyManager.GetItemBalance", GetItemBalance());
            hash.Add("RawSupplyManager.GetPurchasedLites", GetPurchasedLites());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
         * Master Process
         *******************************************************************************/
        public RawItems GetRawItems(BusinessSystemSession session, SqlConnection cn, Plant plant, List<string> itemNumList)
        {
            RawItems items;
            try
            {
                items = GetRawItemsByBranch(session, cn, plant, itemNumList);

                // Item Balance
                GetItemAttributes(session, items);

                // Item Balance
                GetItemBalance(session, items);

                // On Order
                GetPurchasedLites(session, plant, items);

                // Get E1 Part Lists
                var rawOrders = GetWorkOrderSqft(session, plant, items);

                // Get CP Cut
                CreateTempTable();
                InsertTempTable(cn, rawOrders);
                var sfOrders = GetWorkOrderCutSqft(cn);

                foreach (RawItemOrder order in rawOrders)
                {
                    var z = sfOrders.FirstOrDefault(x => x.WorkOrderNum == order.WorkOrderNum &&
                                                         x.LiteNum == order.LiteNum);
                    if (z != null)
                    {
                        order.WorkOrderCutSqft = z.WorkOrderCutSqft * (decimal) 1.0175;
                    }
                }

                foreach (var item in items)
                {
                    decimal itemRemainingQty = item.QtyOnHand;
                    item.RawItemOrders = rawOrders.Where(x => x.ItemNum == item.ItemNum).ToList();
                    foreach (var order in item.RawItemOrders)
                    {
                        order.WorkOrderCutSheets = Math.Round(order.WorkOrderCutSqft / item.ActualSqft, 3);
                        order.WorkOrderSheets = Math.Round(order.WorkOrderSqft / item.ActualSqft, 3);
                        order.OrderRemainingSheets = (order.WorkOrderSheets - order.WorkOrderCutSheets > 0)
                            ? order.WorkOrderSheets - order.WorkOrderCutSheets
                            : 0;
                        order.ItemRemainingSheets = itemRemainingQty - order.OrderRemainingSheets;
                        itemRemainingQty = order.ItemRemainingSheets;
                    }
                    item.OrderRemainingSheets = item.RawItemOrders.Sum(x => x.OrderRemainingSheets);
                    item.ItemRemainingSheets = item.QtyOnHand - item.OrderRemainingSheets;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
            return items;
        }

        /*********************************************************************************
        * Get Items by Q Table
        *******************************************************************************/
        public RawItems GetRawItemsbyBrGlass(BusinessSystemSession session, SqlConnection cn, Plant plant, List<string> itemNumList)
        {
            var items = new RawItems();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant Filter
                filters.Add("RDMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Unit Type
                if (itemNumList.Any())
                {
                    string strInClause = "";
                    foreach (string itemNum in itemNumList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = Convert.ToDecimal(itemNum);
                    }

                    filters.Add("RDITM IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                cmd = GetRawItemsbyBrGlass(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var item = new RawItem();
                        item.PlantNum = plant.PlantNum;
                        item.PlantCode = plant.PlantCode;
                        item.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
                        item.ColorCode = CastDbNull.To<String>(reader["Z58L1CL"], String.Empty).Trim();
                        item.PatternCode = CastDbNull.To<String>(reader["Z58L1PX"], String.Empty).Trim();
                        item.ActualSqft = Common.Utilities.Common.CastStringToDecimal(CastDbNull.To<String>(reader["Z58USF"], String.Empty).Trim());
                        items.Add(item);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }

            return items;
        }

        public iDB2Command GetRawItemsbyBrGlass(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
                RDITM,
                COALESCE(IA1.IADVAL, '') AS Z58L1CL,
                COALESCE(IA2.IADVAL, '') AS Z58L1PX,
                COALESCE(IA3.IADVAL, '') AS Z58USF
            FROM
                F3283
                LEFT OUTER JOIN F580002 IA1
                ON	IA1.IAITM = RDITM AND
		            IA1.IAZ58ATNM = 'Z58L1CL'
                LEFT OUTER JOIN F580002 IA2
                ON	IA2.IAITM = RDITM AND
		            IA2.IAZ58ATNM = 'Z58L1PX'
                LEFT OUTER JOIN F580002 IA3
                ON	IA3.IAITM = RDITM AND
		            IA3.IAZ58ATNM = 'Z58USF'
            WHERE
                {0}
                RDTBLC = 'BRGLASS' AND
                COALESCE(IA1.IADVAL, '') = '' AND
	            COALESCE(IA2.IADVAL, '') = ''
            GROUP BY
	            RDITM,
                COALESCE(IA1.IADVAL, ''),
                COALESCE(IA2.IADVAL, ''),
                COALESCE(IA3.IADVAL, '')
            ORDER BY 1
            FOR READ ONLY", filterString);
            return cmd;
        }

        /*********************************************************************************
        * Get Items by Item Branch
        *******************************************************************************/
        public RawItems GetRawItemsByBranch(BusinessSystemSession session, SqlConnection cn, Plant plant, List<string> itemNumList)
        {
            var items = new RawItems();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant Filter
                filters.Add("IBMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Unit Type
                if (itemNumList.Any())
                {
                    string strInClause = "";
                    foreach (string itemNum in itemNumList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = Convert.ToDecimal(itemNum);
                    }

                    filters.Add("IBITM IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                cmd = GetRawItemsByBranch(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var item = new RawItem();
                        item.PlantNum = plant.PlantNum;
                        item.PlantCode = plant.PlantCode;
                        item.ItemNum = CastDbNull.To<Decimal>(reader["IBITM"], 0);
                        items.Add(item);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }

            return items;
        }

        public iDB2Command GetRawItemsByBranch(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
                IBITM
            FROM
                F4102
            WHERE
                {0}
                IBPRP4 IN ('PR', 'SR', 'SF')
            GROUP BY
	            IBITM
            ORDER BY 1
            FOR READ ONLY", filterString);
            return cmd;
        }

        /*********************************************************************************
         * Item Attributes
         *******************************************************************************/
        public void GetItemAttributes(BusinessSystemSession session, RawItems items)
        {
            try
            {
                var itemAttributeMgr = new ItemAttributeManager(session);
                foreach (RawItem item in items)
                {
                    var attributes = itemAttributeMgr.GetItemAttributes(session, item.ItemNum, 0);
                    item.Thickness = attributes.GetDecimalAttributeValue("Z58L1TK");
                    item.HeatTreatmentCode = attributes.GetStringAttributeValue("Z58L1HT");
                    item.ColorCode = attributes.GetStringAttributeDesc3("Z58L1CL");
                    item.Surface1Coating = attributes.GetStringAttributeValue("Z58L11C");
                    item.Surface2Coating = attributes.GetStringAttributeValue("Z58L12C");
                    item.PatternCode = attributes.GetStringAttributeValue("Z58L1PX");
                    item.LamiTypeCode = attributes.GetStringAttributeDesc1("Z58L1LPT");
                    item.ActualSqft = attributes.GetDecimalAttributeValue("Z58USF");
                    item.MakeupCode =
                        $"{item.Thickness}:{item.ColorCode}:{item.Surface1Coating}:{item.HeatTreatmentCode}:{item.PatternCode}:{item.LamiTypeCode}:{item.Surface2Coating}";
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
         * Item Balances
         *******************************************************************************/
        public void GetItemBalance(BusinessSystemSession session, RawItems items)
        {
            try
            {
                var cmd = session.DbCommands["RawSupplyManager.GetItemBalance"];
                cmd.Connection = session.DbConnection;
                foreach (RawItem item in items)
                {
                    cmd.Parameters[0].Value = item.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = item.ItemNum;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            item.QtyOnHand = CastDbNull.To<Decimal>(reader["LIPQOH"], 0) / 1000;
                            item.SafetyStock = CastDbNull.To<Decimal>(reader["IBSAFE"], 0) / 1000;
                            item.Description = CastDbNull.To<String>(reader["IMDSC1"], String.Empty).Trim();
                            item.Description2 = CastDbNull.To<String>(reader["IMDSC2"], String.Empty).Trim();
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public iDB2Command GetItemBalance()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                IBITM,
	            IBSAFE,
	            IMDSC1,
                IMDSC2,
	            SUM(COALESCE(LIPQOH, 0)) AS LIPQOH
            FROM
                F4102
	            INNER JOIN F4101 
	            ON	IMITM = IBITM
	            LEFT OUTER JOIN F41021
	            ON	LIMCU = IBMCU AND
		            LIITM = IBITM
            WHERE
                IBMCU = ? AND
	            IBITM = ?
            GROUP BY
	            IBITM,
	            IBSAFE,
	            IMDSC1,
                IMDSC2
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            return cmd;
        }

        /*********************************************************************************
         * Work Orders
         *******************************************************************************/
        public RawItemOrders GetWorkOrderSqft(BusinessSystemSession session, Plant plant, RawItems items)
        {
            var orders = new RawItemOrders();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant Filter
                filters.Add("WAMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Item List
                if (items.Count < 1000)
                {
                    string strInClause = "";
                    foreach (var item in items)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = item.ItemNum;
                    }

                    filters.Add("WMCPIT IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                cmd = GetWorkOrderSqft(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var order = new RawItemOrder();
                        order.WorkOrderNum = CastDbNull.To<Decimal>(reader["WADOCO"], 0);
                        order.ItemNum = CastDbNull.To<Decimal>(reader["WMCPIT"], 0);
                        order.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["WADRQJ"], 0));
                        order.WorkOrderQty = CastDbNull.To<Decimal>(reader["WAUORG"], 0) / 1000;
                        order.WorkOrderSqft = order.WorkOrderQty * Common.Utilities.Common.CastStringToDecimal(CastDbNull.To<String>(reader["KZSGVL"], String.Empty).Trim()) * (decimal) 1.0175;
                        var opsq = CastDbNull.To<Decimal>(reader["WMOPSQ"], 0);
                        if (opsq > 0 && opsq < 10000)
                        {
                            order.LiteNum = 1;
                        }
                        else
                        {
                            if (opsq >= 10000 && opsq < 20000)
                            {
                                order.LiteNum = 2;
                            }
                            else
                            {
                                if (opsq >= 20000 && opsq < 30000)
                                {
                                    order.LiteNum = 3;
                                }
                                else
                                {
                                    order.LiteNum = 0;
                                }
                            }
                        }

                        orders.Add(order);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }

            return orders;
        }

        public iDB2Command GetWorkOrderSqft(List<string> filters)
        {

            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
                WADOCO,
                WADRQJ,
	            WASRST,
	            WAAN8,
                WAUORG,
	            WMCPIT,
	            WMOPSQ,
	            IMGLPT,
	            KZSGVL
            FROM
                F4801
                INNER JOIN F3111
                ON	WMDOCO = WADOCO
	            INNER JOIN F3201
	            ON	CMDOCO = WADOCO AND
		            CMKCOO = '*' AND
		            CMDCTO = 'WO'
	            INNER JOIN F3211
	            ON	KZCFGID = CMCFGID AND
		            KZPEL = 50000
	            INNER JOIN F4101 
	            ON IMITM = WMCPIT
            WHERE
                {0}
	            WASRST IN ('10','40') AND
	            IMGLPT IN ('GLAS','GLAF')
            GROUP BY
                WADOCO,
                WADRQJ,
	            WASRST,
	            WAAN8,
                WAUORG,
	            WMCPIT,
	            WMOPSQ,
	            IMGLPT,
	            KZSGVL
            ORDER BY WADRQJ, WADOCO
            FOR READ ONLY", filterString);
            return cmd;
        }


        /*********************************************************************************
          * Purchase Orders
          *******************************************************************************/
        public void GetPurchasedLites(BusinessSystemSession session, Plant plant, RawItems rawItems)
        {
            try
            {
                var cmd = session.DbCommands["RawSupplyManager.GetPurchasedLites"];
                cmd.Connection = session.DbConnection;
                foreach (RawItem item in rawItems)
                {
                    cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = item.ItemNum;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            item.QtyOnOrder = CastDbNull.To<Decimal>(reader["PDUOPN"], 0) / 1000;
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public iDB2Command GetPurchasedLites()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                SUM(PDUOPN) AS PDUOPN
            FROM
                F4311
            WHERE
                PDMCU = ? AND
                PDITM = ? AND
                PDLTTR NOT IN ('980')
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            return cmd;
        }

        /*********************************************************************************
         * Cut Sqft
         *******************************************************************************/
        public SqlCommand CreateTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"CREATE TABLE #TempWoList (WorkOrderNum int)";
            return cmd;
        }

        public void InsertTempTable(SqlConnection cn, RawItemOrders orders)
        {
            try
            {
                var cmd1 = CreateTempTable();
                cmd1.Connection = cn;
                int rows = cmd1.ExecuteNonQuery();

                DataTable dt = new DataTable("WorkOrderNums");
                dt.Columns.Add("WorkOrderNum", typeof(int));
                var workOrderNums = orders.Select(x => x.WorkOrderNum).Distinct();
                foreach (var orderNum in workOrderNums)
                {
                    DataRow row = dt.NewRow();
                    row["WorkOrderNum"] = (int)orderNum;
                    dt.Rows.Add(row);
                }
                var bulk = new SqlBulkCopy(cn)
                {
                    DestinationTableName = "#TempWoList",
                    BulkCopyTimeout = 300,
                    BatchSize = 25000
                };
                bulk.WriteToServer(dt);
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        public RawItemOrders GetWorkOrderCutSqft(SqlConnection cn)
        {
            var orders = new RawItemOrders();
            try
            {
                var cmd = GetWorkOrderCutSqft();
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        RawItemOrder order = new RawItemOrder();
                        order.WorkOrderNum = CastDbNull.To<decimal>(reader["WorkOrderNum"], 0);
                        order.LiteNum = CastDbNull.To<int>(reader["UnitLitePosition"], 0) + 1;
                        order.WorkOrderCutSqft = CastDbNull.To<decimal>(reader["AreaActual"], 0);
                        orders.Add(order);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }

            return orders;
        }

        public SqlCommand GetWorkOrderCutSqft()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                CAST(pl.WorkOrderNum AS DECIMAL(8,0)) AS WorkOrderNum,
                CAST(pl.UnitLitePosition AS INT) AS UnitLitePosition,
                SUM(CAST(pl.AreaActual AS DECIMAL(15,4))) AS AreaActual
              FROM
                ClearPathReporting.ProdLites pl WITH (NOLOCK)
                INNER JOIN #TempWoList B
                ON B.WorkOrderNum = pl.WorkOrderNum
              WHERE
                pl.LineTypeCode = 'CUTLV' AND
                pl.ProdRejectUnitLiteID = 0
              GROUP BY
                pl.WorkOrderNum,
                pl.UnitLitePosition";
            return cmd;
        }
    }
}
