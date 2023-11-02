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

namespace BIServices.Common.Managers.ShopFloor
{
    public class HvMtoManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private E1.ItemAttributeManager _itemAttMgr;

        public HvMtoManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("HvMtoManager.GetShippedUnits", GetShippedUnits());
            hash.Add("HvMtoManager.GetShippedLines", GetShippedLines());
            hash.Add("HvMtoManager.GetPurchasedLites", GetPurchasedLites());
            hash.Add("HvMtoManager.GetReceivedRacks", GetReceivedRacks());
            hash.Add("HvMtoManager.GetReceivedEvent", GetReceivedEvent());
            hash.Add("HvMtoManager.GetProducedEvent", GetProducedEvent());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            _flexItemMgr = new FlexItemManager(session, "P5842300");
        }

        /*********************************************************************************
          * Get Requested Date Facts
          *******************************************************************************/
        public iDB2Command GetOpenOrders(List<string> filters)
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
	            *
            FROM (
	            SELECT 
		            WAMCU,
		            WADOCO,
		            WASRST,
		            WATRDJ,
		            WADRQJ,
		            WARORN,
		            WARCTO,
		            WALNID,
                    WAITM,
                    WALOTN,
		            WAUORG,
		            WAAN8,
		            ABALPH,
		            S1Z58SCN,
		            S1Z58SCTYP,
		            CMCFGID,
                    IBSRP4,
                    IBSTKT,
		            ROW_NUMBER() OVER (PARTITION BY WADOCO ORDER BY S1UPMJ DESC, S1TDAY DESC) AS S1ROW
	            FROM 
		            F4801
		            INNER JOIN F4102
		            ON	IBMCU = WAMCU AND
			            IBITM = WAITM
		            INNER JOIN F0101
		            ON ABAN8 = WAAN8
		            INNER JOIN F3201 
		            ON CMDOCO = WADOCO AND
			            CMDCTO = 'WO' AND
			            CMKCOO = '*' AND
			            CMPID != 'P580150'		            
                    LEFT OUTER JOIN F5834A11 		            
                        INNER JOIN F5834A01
		                ON	S1MCU = S5MCU AND
			                S1Z58SCN = S5Z58SCN AND
			                S1Z58SCD = S5Z58SCD
		            ON S5DOCO = WADOCO 
	            WHERE 
                    {0}
		            WASRST IN ('40','92')
		            ) A
            WHERE 
	            A.S1ROW = 1
            ORDER BY
	            1,2
            FOR READ ONLY", filterString);
            //cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public HvMtoOrders GetOpenOrders(BusinessSystemSession session, SqlConnection cnDt, Plant plant, bool isHvMto, decimal scheduleNum)
        {
            var orders = new HvMtoOrders();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant
                filters.Add("WAMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // HV MTO
                //if (isHvMto)
                //{
                filters.Add("(S1Z58SCTYP = 'HV MTO' OR (WAUORG >= 10000)) AND\r\n");
                filters.Add("EXISTS(SELECT 1 FROM F580400 WHERE RODOCO = S5ODOC AND RODCTO = S5RCTO AND ROLNID = S5LNID) AND\r\n");
                //}

                // Schedule Num
                if (scheduleNum > 0)
                {
                    filters.Add("S1Z58SCN = ? AND EXISTS(SELECT 1 FROM F580400 WHERE RODOCO = S5ODOC AND RODCTO = S5RCTO AND ROLNID = S5LNID) AND\r\n");
                    parameters.Add(new iDB2Parameter("SCN", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 5;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = scheduleNum;
                }

                cmd = GetOpenOrders(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                //var cmd = GetOpenOrders();
                //cmd.Connection = session.DbConnection;
                //cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var order = new HvMtoOrder();
                        order.Plant = CastDbNull.To<String>(reader["WAMCU"], String.Empty).Trim();
                        order.WorkOrderNum = CastDbNull.To<Decimal>(reader["WADOCO"], 0);
                        order.WorkOrderStatus = CastDbNull.To<String>(reader["WASRST"], String.Empty).Trim();
                        order.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["WATRDJ"], 0));
                        order.DueDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["WADRQJ"], 0));
                        order.SalesOrderNum = Data.CastStringToDecimal(CastDbNull.To<String>(reader["WARORN"], String.Empty).Trim());
                        order.SalesOrderType = CastDbNull.To<String>(reader["WARCTO"], String.Empty).Trim();
                        order.SalesOrderLineNum = CastDbNull.To<Decimal>(reader["WALNID"], 0) / 1000;
                        order.CustomerSoldNum = CastDbNull.To<Decimal>(reader["WAAN8"], 0);
                        order.CustomerSoldName = CastDbNull.To<String>(reader["ABALPH"], String.Empty).Trim();
                        order.ConfigId = CastDbNull.To<Decimal>(reader["CMCFGID"], 0);
                        order.ScheduleNum = CastDbNull.To<Decimal>(reader["S1Z58SCN"], 0);
                        order.ScheduleType = CastDbNull.To<String>(reader["S1Z58SCTYP"], String.Empty).Trim();
                        order.OrderedUnitsQty = CastDbNull.To<Decimal>(reader["WAUORG"], 0) / 1000;
                        order.UnitType = CastDbNull.To<String>(reader["IBSRP4"], String.Empty).Trim();
                        order.ItemNum = CastDbNull.To<Decimal>(reader["WAITM"], 0);
                        order.StockType = CastDbNull.To<String>(reader["IBSTKT"], String.Empty).Trim();
                        order.LotNum = CastDbNull.To<String>(reader["WALOTN"], String.Empty).Trim();

                        var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, order.ConfigId);
                        var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
                        order.Construction = flexDescs[0].Trim();
                        order.Dimension = flexDescs[1].Trim();
                        orders.Add(order);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            GetShippedUnits(session, orders);
            GetShippedLines(session, orders);
            GetProducedEvent(session, orders);
            GetPurchasedLites(session, orders);
            return orders;
        }

        /*********************************************************************************
          * Shipped Units
          *******************************************************************************/
        public iDB2Command GetShippedUnits()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(CASE WHEN SDPSN > 0 THEN SDUORG ELSE 0 END) AS SHIPPED,	            
                SUM(CASE WHEN SDPSN = 0 THEN SDUORG ELSE 0 END) AS OPEN,
                MAX(SDADDJ) AS SDADDJ
            FROM
	            F4801
	            INNER JOIN F4211 
	            ON SDDOCO = CAST(WARORN AS DECIMAL(8,0)) AND
		            SDDCTO = WARCTO AND
		            SDLNID >= WALNID AND 
		            SDLNID < WALNID + 1000
            WHERE
	            WADOCO = ? AND
                SDLTTR != '980'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            return cmd;
        }

        public void GetShippedUnits(BusinessSystemSession session, HvMtoOrders orders)
        {

            try
            {
                var cmd = session.DbCommands["HvMtoManager.GetShippedUnits"];
                cmd.Connection = session.DbConnection;
                foreach (HvMtoOrder order in orders)
                {
                    cmd.Parameters[0].Value = order.WorkOrderNum;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            order.ShippedUnitsQty = CastDbNull.To<Decimal>(reader["SHIPPED"], 0) / 1000;
                            order.UnshippedUnitsQty = CastDbNull.To<Decimal>(reader["OPEN"], 0) / 1000;
                            order.LastShippedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["SDADDJ"], 0));
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
          * Shipped Lines
          *******************************************************************************/
        public iDB2Command GetShippedLines()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SDDOCO,
                SDDCTO,
                SDLNID,
                SDUORG,
                SDADDJ,
                SDPSN
            FROM
	            F4801
	            INNER JOIN F4211 
	            ON SDDOCO = CAST(WARORN AS DECIMAL(8,0)) AND
		            SDDCTO = WARCTO AND
		            SDLNID >= WALNID AND 
		            SDLNID < WALNID + 1000
            WHERE
	            WADOCO = ? AND
                SDLTTR != '980' AND
                SDPSN > 0
            ORDER BY
                SDADDJ
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            return cmd;
        }

        public void GetShippedLines(BusinessSystemSession session, HvMtoOrders orders)
        {

            try
            {
                var cmd = session.DbCommands["HvMtoManager.GetShippedLines"];
                cmd.Connection = session.DbConnection;
                foreach (HvMtoOrder order in orders)
                {
                    var lines = new ShippedLines();
                    cmd.Parameters[0].Value = order.WorkOrderNum;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var line = new ShippedLine();
                            line.LineNum = CastDbNull.To<Decimal>(reader["SDLNID"], 0) / 1000;
                            line.ShippedQty = CastDbNull.To<Decimal>(reader["SDUORG"], 0) / 1000;
                            line.ShippedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["SDADDJ"], 0));
                            line.LoadNum = CastDbNull.To<Decimal>(reader["SDPSN"], 0);
                            lines.Add(line);
                        }

                        reader.Close();
                    }

                    order.ShippedLines = lines;
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
          * Purchased Lites
          *******************************************************************************/
        public iDB2Command GetPurchasedLites()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            ROZ58L1CLN,
	            ROEDTR,
                ROMCU,
                ROMCU2,
                PDTRDJ,
                PDKCOO,
	            PDDOCO,
	            PDDCTO,
	            PDLNID,
	            PDITM,
	            PDLOTN,
                PDAN8,
                PDLTTR,
                PDNXTR,
                ABALPH,
                IMSTKT,
                COALESCE(CMCFGID,0) AS CMCFGID,
	            SUM(PDUORG) AS PDUORG,
	            SUM(PDUREC) AS PDUREC,
	            SUM(PDUOPN) AS PDUOPN
            FROM
	            F580400
	            INNER JOIN F4311
	            ON PDMCU = ROMCU AND
		            PDDOCO = RODOC1 AND
		            PDDCTO = RORCTO AND
		            PDLNID >= RORLLN AND 
		            PDLNID < RORLLN - MOD(RORLLN, 1000) + 1000
		            --PDLTTR != '980'
                INNER JOIN F0101
                ON ABAN8 = PDAN8
		        INNER JOIN F4101
                ON IMITM = PDITM
		        LEFT OUTER JOIN F3201 
		        ON CMDOCO = PDDOCO AND
			        CMDCTO = PDDCTO AND
			        CMKCOO = PDKCOO AND
                    CMLNID = PDLNID 
            WHERE 
	            RODOCO = ? AND
	            RODCTO = ? AND
	            ROLNID = ?
            GROUP By
	            ROZ58L1CLN,
	            ROEDTR,
                ROMCU,
                ROMCU2,
                PDTRDJ,
                PDKCOO,
	            PDDOCO,
	            PDDCTO,
	            PDLNID,
	            PDITM,
	            PDLOTN,
                PDAN8,
                PDLTTR,
                PDNXTR,
                ABALPH,
                IMSTKT,
                CMCFGID
            ORDER BY ROZ58L1CLN, PDTRDJ
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetPurchasedLites(BusinessSystemSession session, HvMtoOrders orders)
        {
            try
            {
                var cmd = session.DbCommands["HvMtoManager.GetPurchasedLites"];
                cmd.Connection = session.DbConnection;
                foreach (HvMtoOrder order in orders)
                {
                    var lites = new PurchasedLites();
                    cmd.Parameters[0].Value = order.SalesOrderNum;
                    cmd.Parameters[1].Value = order.SalesOrderType;
                    cmd.Parameters[2].Value = order.SalesOrderLineNum*1000;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var lite = new PurchasedLite();
                            lite.LiteNum = CastDbNull.To<Decimal>(reader["ROZ58L1CLN"], 0);
                            if (lite.LiteNum == 1)
                            {
                                lite.LiteName = "EXT";
                            }
                            else if (lite.LiteNum == 2 && order.UnitType == "3")
                            {
                                lite.LiteName = "MID";
                            }
                            else
                            {
                                lite.LiteName = "INT";
                            }

                            lite.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["PDTRDJ"], 0));
                            lite.Plant = CastDbNull.To<String>(reader["ROMCU"], String.Empty).Trim();
                            lite.TransactionType = CastDbNull.To<String>(reader["ROEDTR"], String.Empty).Trim();
                            lite.PurchaseOrderNum = CastDbNull.To<Decimal>(reader["PDDOCO"], 0);
                            lite.PurchaseOrderType = CastDbNull.To<String>(reader["PDDCTO"], String.Empty).Trim();
                            lite.PurchaseOrderLineNum = CastDbNull.To<Decimal>(reader["PDLNID"], 0) / 1000;
                            lite.PurchaseOrderStatus = CastDbNull.To<String>(reader["PDLTTR"], String.Empty).Trim() + "/" + CastDbNull.To<String>(reader["PDNXTR"], String.Empty).Trim();
                            lite.SupplierNum = CastDbNull.To<Decimal>(reader["PDAN8"], 0);
                            lite.SupplierName = CastDbNull.To<String>(reader["ABALPH"], String.Empty).Trim();
                            lite.SupplierPlant = CastDbNull.To<String>(reader["ROMCU2"], String.Empty).Trim();
                            lite.ItemNum = CastDbNull.To<Decimal>(reader["PDITM"], 0);
                            lite.LotNum = CastDbNull.To<String>(reader["PDLOTN"], String.Empty).Trim();
                            lite.PurchasedQty = CastDbNull.To<Decimal>(reader["PDUORG"], 0) / 1000;
                            lite.ReceivedQty = CastDbNull.To<Decimal>(reader["PDUREC"], 0) / 1000;
                            lite.StockType = CastDbNull.To<String>(reader["IMSTKT"], String.Empty).Trim();
                            lite.ConfigId = CastDbNull.To<Decimal>(reader["CMCFGID"], 0);
                            ItemAttributes itemAtts = _itemAttMgr.GetItemAttributes(session, lite.ItemNum, lite.ConfigId);
                            var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
                            lite.Construction = flexDescs[0].Trim();
                            GetReceivedEvent(session, lite);
                            GetReceivedRacks(session, lite);
                            lites.Add(lite);
                        }

                        reader.Close();
                    }

                    order.PurchasedLites = lites;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Purchased Lites
          *******************************************************************************/
        public iDB2Command GetReceivedRacks()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            PDITM,
	            PDLOTN,
	            RDMCU,
	            RHLOCN,
	            RDZ58RID,
	            MIN(RDZ58SN) AS RDZ58SN1,
	            MAX(RDZ58SN) AS RDZ58SN2,
	            SUM(RDPQOH) AS RDPQOH
            FROM
	            F4311
	            INNER JOIN F580056 INNER JOIN F580055 ON RHZ58RID = RDZ58RID
	            ON	RDLOTN = PDLOTN AND
		            RDITM = PDITM 
            WHERE
	            PDDOCO =  ? AND
	            PDDCTO = ? AND
	            PDLOTN = ?             
            GROUP BY
	            PDITM,
	            PDLOTN,
	            RDMCU,
	            RHLOCN,
	            RDZ58RID
            ORDER BY RDZ58RID
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LOTN", iDB2DbType.iDB2Graphic, 20));
            return cmd;
        }

        public void GetReceivedRacks(BusinessSystemSession session, PurchasedLite lite)
        {
            try
            {
                var cmd = session.DbCommands["HvMtoManager.GetReceivedRacks"];
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = lite.PurchaseOrderNum;
                cmd.Parameters[1].Value = lite.PurchaseOrderType;
                cmd.Parameters[2].Value = lite.LotNum;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var racks = new ReceivedRacks();
                    while (reader.Read())
                    {
                        var rack = new ReceivedRack();
                        rack.RackId = CastDbNull.To<Decimal>(reader["RDZ58RID"], 0);
                        rack.SerialNumMin = CastDbNull.To<String>(reader["RDZ58SN1"], String.Empty).Trim();
                        rack.SerialNumMax = CastDbNull.To<String>(reader["RDZ58SN2"], String.Empty).Trim();
                        rack.Plant = CastDbNull.To<String>(reader["RDMCU"], String.Empty).Trim();
                        rack.Location = CastDbNull.To<String>(reader["RHLOCN"], String.Empty).Trim();
                        rack.ItemNum = CastDbNull.To<Decimal>(reader["PDITM"], 0);
                        rack.LotNum = CastDbNull.To<String>(reader["PDLOTN"], String.Empty).Trim();
                        rack.OnHandQty = CastDbNull.To<Decimal>(reader["RDPQOH"], 0) / 1000;
                        racks.Add(rack);
                    }

                    reader.Close();
                    lite.Racks = racks;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Purchased Lites
          *******************************************************************************/
        public iDB2Command GetReceivedEvent()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                ILTRQT,
                ILCRDJ,
                ILLOCN
            FROM 
                F4111 
            WHERE 
                ILMCU = ? AND
                ILLOTN = ? AND 
                ILDCT = 'OV'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("LOTN", iDB2DbType.iDB2Graphic, 20));
            return cmd;
        }

        public void GetReceivedEvent(BusinessSystemSession session, PurchasedLite lite)
        {
            try
            {
                var cmd = session.DbCommands["HvMtoManager.GetReceivedEvent"];
                cmd.Connection = session.DbConnection;
                if (!String.IsNullOrEmpty(lite.LotNum))
                {
                    cmd.Parameters[0].Value = lite.Plant.PadLeft(12);
                    cmd.Parameters[1].Value = lite.LotNum;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            lite.ReceivedQty = CastDbNull.To<Decimal>(reader["ILTRQT"], 0) / 1000;
                            lite.ReceivedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["ILCRDJ"], 0));
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
          * Purchased Lites
          *******************************************************************************/
        public iDB2Command GetProducedEvent()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                SUM(ILTRQT) AS ILTRQT
            FROM 
                F4111 
            WHERE 
                ILMCU = ? AND
                ILLOTN = ? AND 
                ILDCT = 'IC'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("LOTN", iDB2DbType.iDB2Graphic, 20));
            return cmd;
        }

        public void GetProducedEvent(BusinessSystemSession session, HvMtoOrders orders)
        {
            try
            {
                var cmd = session.DbCommands["HvMtoManager.GetProducedEvent"];
                cmd.Connection = session.DbConnection;
                foreach (HvMtoOrder order in orders)
                {
                    if (!String.IsNullOrEmpty(order.LotNum))
                    {
                        cmd.Parameters[0].Value = order.Plant.PadLeft(12);
                        cmd.Parameters[1].Value = order.LotNum;
                        using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            while (reader.Read())
                            {
                                order.ProducedUnitsQty = CastDbNull.To<Decimal>(reader["ILTRQT"], 0) / 1000;
                            }

                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
          * Purchased Glass Summary
          *******************************************************************************/
        public PurchasedGlassItems GetPurchasedGlassSummary(BusinessSystemSession session, HvMtoOrders orders)
        {
            var items = new PurchasedGlassItems();
            try
            {
                foreach (HvMtoOrder order in orders)
                {
                    // Lite 1
                    var item1 = new PurchasedGlassItem();
                    item1.WorkOrderNum = order.WorkOrderNum;
                    item1.WorkOrderStatus = order.WorkOrderStatus;
                    item1.UnitType = order.UnitType;
                    item1.Units = order.OrderedUnitsQty;
                    item1.OrderDate = order.OrderDate;
                    item1.DueDate = order.DueDate;
                    item1.ScheduleNum = order.ScheduleNum;
                    item1.CustomerName = order.CustomerSoldName;
                    item1.Dimension = order.Dimension;
                    item1.LiteNum = 1;
                    item1.LiteName = "EXT";
                    var lites1 =
                        order.PurchasedLites.Where(
                            t => t.LiteNum == 1 && t.TransactionType != "R" && !t.PurchaseOrderStatus.StartsWith("980") && t.TransactionType != "V");
                    var purchasedLites1 = lites1 as PurchasedLite[] ?? lites1.ToArray();
                    item1.Supplier = purchasedLites1.FirstOrDefault()?.SupplierName;
                    item1.Construction = purchasedLites1.FirstOrDefault()?.Construction;
                    item1.PurchasedLites = purchasedLites1.Sum(t => t.PurchasedQty);
                    item1.ReceivedLites = purchasedLites1.Sum(t => t.ReceivedQty);
                    item1.OnHandLites = purchasedLites1.Sum(t => t.Racks.Sum(w => w.OnHandQty));
                    var rackString1 = "";
                    var rackPlant1 = "";
                    int i1 = 0;
                    foreach (PurchasedLite lite in purchasedLites1)
                    {
                        var sb = new StringBuilder();
                        foreach (ReceivedRack rack in lite.Racks.Where(t => t.OnHandQty > 0))
                        {
                            rackPlant1 = rack.Plant;
                            //if (i1 > 0)
                            //{
                            //    sb.Append(System.Environment.NewLine);
                            //}
                            sb.Append("[");
                            sb.Append(rack.RackId);
                            sb.Append("] ");
                            sb.Append(rack.Location);
                            sb.Append(" ");
                            sb.Append(rack.OnHandQty);
                            sb.Append(" ");
                            i1++;
                        }
                        rackString1 = sb.ToString();
                    }
                    item1.RacksDesc = rackString1;
                    item1.RackPlant = rackPlant1;
                    items.Add(item1);

                    // Lite 2
                    if (order.UnitType == "2" || order.UnitType == "3")
                    {
                        var item2 = new PurchasedGlassItem();
                        item2.WorkOrderNum = order.WorkOrderNum;
                        item2.WorkOrderStatus = order.WorkOrderStatus;
                        item2.OrderDate = order.OrderDate;
                        item2.UnitType = order.UnitType;
                        item2.Units = order.OrderedUnitsQty;
                        item2.DueDate = order.DueDate;
                        item2.ScheduleNum = order.ScheduleNum;
                        item2.CustomerName = order.CustomerSoldName;
                        item2.Dimension = order.Dimension;
                        item2.LiteNum = 2;
                        item2.LiteName = (order.UnitType == "3") ? "MID" : "INT";
                        var lites2 =
                            order.PurchasedLites.Where(
                                t => t.LiteNum == 2 && t.TransactionType != "R" && !t.PurchaseOrderStatus.StartsWith("980") && t.TransactionType != "V");
                        var purchasedLites2 = lites2 as PurchasedLite[] ?? lites2.ToArray();
                        item2.Supplier = purchasedLites2.FirstOrDefault()?.SupplierName;
                        item2.Construction = purchasedLites2.FirstOrDefault()?.Construction;
                        item2.PurchasedLites = purchasedLites2.Sum(t => t.PurchasedQty);
                        item2.ReceivedLites = purchasedLites2.Sum(t => t.ReceivedQty);
                        item2.OnHandLites = purchasedLites2.Sum(t => t.Racks.Sum(w => w.OnHandQty));
                        var rackString2 = "";
                        var rackPlant2 = "";
                        int i2 = 0;
                        foreach (PurchasedLite lite in purchasedLites2)
                        {
                            var sb = new StringBuilder();
                            foreach (ReceivedRack rack in lite.Racks.Where(t => t.OnHandQty > 0))
                            {
                                rackPlant2 = rack.Plant;
                                //if (i2 > 0)
                                //{
                                //    sb.Append(System.Environment.NewLine);
                                //}
                                sb.Append("[");
                                sb.Append(rack.RackId);
                                sb.Append("] ");
                                sb.Append(rack.Location);
                                sb.Append(" ");
                                sb.Append(rack.OnHandQty);
                                sb.Append(" ");
                                i2++;
                            }
                            rackString2 = sb.ToString();
                        }
                        item2.RacksDesc = rackString2;
                        item2.RackPlant = rackPlant2;
                        items.Add(item2);
                    }

                    // Lite 3 Rack
                    if (order.UnitType == "3") {
                        var item3 = new PurchasedGlassItem();
                        item3.WorkOrderNum = order.WorkOrderNum;
                        item3.WorkOrderStatus = order.WorkOrderStatus;
                        item3.OrderDate = order.OrderDate;
                        item3.UnitType = order.UnitType;
                        item3.Units = order.OrderedUnitsQty;
                        item3.DueDate = order.DueDate;
                        item3.ScheduleNum = order.ScheduleNum;
                        item3.CustomerName = order.CustomerSoldName;
                        item3.Dimension = order.Dimension;
                        item3.LiteNum = 3;
                        item3.LiteName = "INT";
                        var lites3 =
                            order.PurchasedLites.Where(
                                t => t.LiteNum == 3 && t.TransactionType != "R" && !t.PurchaseOrderStatus.StartsWith("980") && t.TransactionType != "V");
                        var purchasedLites3 = lites3 as PurchasedLite[] ?? lites3.ToArray();
                        item3.Supplier = purchasedLites3.FirstOrDefault()?.SupplierName;
                        item3.Construction = purchasedLites3.FirstOrDefault()?.Construction;
                        item3.PurchasedLites = purchasedLites3.Sum(t => t.PurchasedQty);
                        item3.ReceivedLites = purchasedLites3.Sum(t => t.ReceivedQty);
                        item3.OnHandLites = purchasedLites3.Sum(t => t.Racks.Sum(w => w.OnHandQty));
                        var rackString3 = "";
                        var rackPlant3 = "";
                        int i3 = 0;
                        foreach (PurchasedLite lite in purchasedLites3)
                        {
                            var sb = new StringBuilder();
                            foreach (ReceivedRack rack in lite.Racks.Where(t => t.OnHandQty > 0))
                            {
                                rackPlant3 = rack.Plant;
                                //if (i3 > 0)
                                //{
                                //    sb.Append(System.Environment.NewLine);
                                //}
                                sb.Append("[");
                                sb.Append(rack.RackId);
                                sb.Append("] ");
                                sb.Append(rack.Location);
                                sb.Append(" ");
                                sb.Append(rack.OnHandQty);
                                sb.Append(" ");
                                i3++;
                            }
                            rackString3 = sb.ToString();
                        }
                        item3.RacksDesc = rackString3;
                        item3.RackPlant = rackPlant3;
                        items.Add(item3);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return items;
        }
    }
}
