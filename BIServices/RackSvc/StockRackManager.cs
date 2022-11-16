using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Configuration;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
using NLog.Filters;

namespace BIServices.RackSvc
{
    public class StockRackManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;

        public StockRackManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("StockRackManager.GetCost", GetCost());
            //hash.Add("StockRackManager.GetReceivedDate", GetReceivedDate());
            //hash.Add("StockRackManager.GetItemProperties", GetItemProperties());
            //hash.Add("StockRackManager.GetConsumption", GetConsumption());
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
          * Get MTS Racks
          *******************************************************************************/
        public iDB2Command GetStockRacks(List<string> filters)
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
	            RHZ58RID,
                RHMCU,
                MCDL01,
	            RHZ58RT,
	            RHAN8,
	            RHSHAN,
	            RHZ58HST,
                RHZ58RSDT,
	            RHLOCN,
	            RHZ58RTWT,
	            RHZ58MTRW,
                RHZ58GWTT,
				RHZ58BOX,
				RHZ58SCN,
                RHURRF,
                RHPSN,
                RHZ58SLDN,
				RDLOTN,
				RDZ58SN,
				RDITM,
				RDZ58MDT,
				RDZ58PDT,
				RDPQOH,
                RDZ58CQTY,
                RDDSC1,
                RDDSC2,
				IBSTKT,
				IBGLPT,
				IBANPL,
                IBPRP4,
				AB1.ABALPH AS IBANPLALPH,
				IMDSC1,
				IMDSC2,
				(SELECT
					MIN(HLUPMJ)
				FROM
					F580559
				WHERE
					HLZ58RID = RHZ58RID AND
					HLMCU = RHMCU
					) AS HLUPMJ,
                (SELECT
                    IVCITM
                FROM
                    F4104 
                    INNER JOIN F0101
                    ON ABAN8 = IVAN8
                WHERE
                    IVAN8 = IBANPL AND
                    IVITM = IBITM AND
                    ? BETWEEN IVEFTJ AND IVEXDJ AND
                    IVXRT = 'C' AND
                    ABAT1 = 'C'
				FETCH FIRST 1 ROWS ONLY) AS IVCITM,
				(SELECT
					SUM(ILTRQT) AS ILTRQT
				FROM
					F4111
				WHERE
					ILITM = IBITM AND
					ILMCU = IBMCU AND
					ILTRDJ >= ? AND
					ILDCT IN ('2I','IM','1A')) AS ILTRQT,
				(SELECT
					SUM(IECSL) AS IECSL
				FROM
					F30026
				WHERE
					IEITM = IBITM AND
					IEMMCU = IBMCU AND
					IELEDG = '07') AS IECSL
            FROM 
	            F580055
	            INNER JOIN F580056
	            ON  RDZ58RID = RHZ58RID
				INNER JOIN F4102
				ON	IBMCU = RHMCU AND
					IBITM = RDITM
				INNER JOIN F4101
				ON	IMITM = IBITM
                INNER JOIN F0006
                ON  MCMCU = RHMCU
				LEFT OUTER JOIN F0101 AB1
				ON	AB1.ABAN8 = IBANPL
            WHERE
	            {0}
				RHZ58HST NOT IN ('0', '8') AND
                IBSTKT <> 'C' AND
                RDPQOH <> 0
            ORDER BY 1	
            FOR READ ONLY", filterString);
            cmd.Parameters.Add(new iDB2Parameter("TDAY", iDB2DbType.iDB2Decimal));
            cmd.Parameters["TDAY"].Precision = 6;
            cmd.Parameters["TDAY"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("TRDJ", iDB2DbType.iDB2Decimal));
            cmd.Parameters["TRDJ"].Precision = 6;
            cmd.Parameters["TRDJ"].Scale = 0;
            return cmd;
        }

        public Racks GetStockRacks(BusinessSystemSession session, List<string> plantList, int agingDays, List<string> statusList)
        {
            var racks = new Racks();
            iDB2Command cmd;
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {
                // Plant List
                if (plantList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string plantItem in plantList)
                    {
                        var plant = PlantManager.GetPlant(plantItem.Trim());
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("RHMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Status List
                if (statusList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string statusItem in statusList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("HST", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = statusItem;
                    }
                    filters.Add("RHZ58HST IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Aging Days
                if (agingDays > 0)
                {
                    var dt = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(agingDays * -1));
                    filters.Add("(RDZ58MDT BETWEEN 0 AND ? OR RDZ58PDT BETWEEN 0 AND ?) AND\r\n");
                    parameters.Add(new iDB2Parameter("DT1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                    parameters.Add(new iDB2Parameter("DT2", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                }

                cmd = GetStockRacks(filters);
                cmd.Parameters[0].Value = E1TimeManager.CastToE1Date(DateTime.Now);
                cmd.Parameters[1].Value = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(-90));
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
                        var rack = new Rack();
                        rack.RackId = reader.Get<Decimal>("RHZ58RID", 0);
                        rack.AssetId = reader.Get("RHURRF", String.Empty).Trim();
                        rack.StockSerialNum = reader.Get("RDZ58SN", String.Empty).Trim();
                        rack.CostCenter = reader.Get("RHMCU", String.Empty).Trim();
                        rack.PlantNum = reader.Get("RHMCU", String.Empty).Trim();
                        rack.PlantCode = reader.Get("MCDL01", String.Empty).Trim();
                        rack.RackType = reader.Get("RHZ58RT", String.Empty).Trim();
                        rack.Location = reader.Get("RHLOCN", String.Empty).Trim();
                        rack.HeaderStatus = reader.Get("RHZ58HST", String.Empty).Trim();
                        rack.HeaderStatusDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("RHZ58RSDT", 0));
                        rack.ItemNum = reader.Get<Decimal>("RDITM", 0);
                        rack.CustomerNum = reader.Get<Decimal>("RHAN8", 0);
                        rack.ShipToCustomerNum = reader.Get<Decimal>("RHSHAN", 0);
                        rack.ManufacturedDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("RDZ58MDT", 0));
                        rack.ProductionDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("RDZ58PDT", 0));
                        rack.ManufacturedAgingDays = (DateTime.Now - rack.ManufacturedDate.Value).Days;
                        rack.TareWeight = reader.Get<Decimal>("RHZ58RTWT", 0) / 100;
                        rack.GlassWeight = reader.Get<Decimal>("RHZ58GWTT", 0) / 100;
                        rack.GlassWeightTons = reader.Get<Decimal>("RHZ58GWTT", 0) / 100 / 2000;
                        rack.GrossWeight = reader.Get<Decimal>("RHZ58MTRW", 0) / 100;
                        rack.GrossWeightTons = reader.Get<Decimal>("RHZ58MTRW", 0) / 100 / 2000;
                        rack.Quantity = reader.Get<Decimal>("RDPQOH", 0) / 1000;
                        rack.OriginalQuantity = reader.Get<Decimal>("RDZ58CQTY", 0) / 1000;
                        rack.PlannerNum = reader.Get<Decimal>("IBANPL", 0);
                        rack.PlannerName = reader.Get<string>("IBANPLALPH", String.Empty);
                        rack.PlanningFamily = reader.Get<string>("IBPRP4", String.Empty);
                        rack.ReceivedDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("HLUPMJ"));
                        rack.ReceivedAgingDays = (DateTime.Now - rack.ReceivedDate.Value).Days;
                        rack.ReceivedQty = rack.OriginalQuantity;
                        rack.ItemDesc = reader.Get<string>("IMDSC1", String.Empty);
                        rack.StockType = reader.Get<string>("IBSTKT", String.Empty);
                        rack.GlCategory = reader.Get<string>("IBGLPT", String.Empty);
                        rack.ItemConsumptionDays = 90;
                        rack.LoadNum = reader.Get<Decimal>("RHPSN", 0);
                        rack.SuggestedLoadNum = reader.Get<Decimal>("RHZ58SLDN", 0);
                        rack.CustomerPartNum = reader.Get<string>("IVCITM", String.Empty);
                        rack.UnitCost = reader.Get<Decimal>("IECSL", 0) / 10000;
                        rack.TotalCost = rack.UnitCost * rack.Quantity;
                        rack.ItemConsumptionAmt = reader.Get<Decimal>("ILTRQT", 0) / 1000;
                        rack.Description1 = reader.Get<string>("RDDSC1", String.Empty);
                        rack.Description2 = reader.Get<string>("RDDSC2", String.Empty);
                        racks.Add(rack);
                    }
                    reader.Close();
                }

                // Item Properties
                var items = racks.Select(x => new { x.ItemNum }).Distinct().ToList();

                // Attributes
                var itemAttDict = new Dictionary<decimal, ItemAttributes>();
                HashSet<string> hash = new HashSet<string> {"Z58USF", "Z58LPR", "Z58L1TK", "Z5851705" };
                foreach (var item in items)
                {
                    itemAttDict.Add(item.ItemNum, _itemAttMgr.GetDiscreteItemAttributes(session, item.ItemNum));
                }
                // Assign Attributes
                foreach (var rack in racks)
                {
                    var itemAtts = itemAttDict[rack.ItemNum];
                    rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
                    rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                    rack.LitesPerRack = itemAtts.GetDecimalAttributeValue("Z58LPR");
                    rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                    rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
                    var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
                    rack.Construction = flexDescs[0];
                    rack.Dimension = flexDescs[1];
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }


        /*********************************************************************************
          * Get MTS Racks
          *******************************************************************************/
        public iDB2Command GetStockRacks0(List<string> filters)
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
	            RHZ58RID,
                RHMCU,
                MCDL01,
	            RHZ58RT,
	            RHAN8,
	            RHSHAN,
	            RHZ58HST,
                RHZ58RSDT,
	            RHLOCN,
	            RHZ58RTWT,
	            RHZ58MTRW,
                RHZ58GWTT,
				RHZ58BOX,
				RHZ58SCN,
                RHURRF,
                RHPSN,
                RHZ58SLDN,
				RDLOTN,
				RDZ58SN,
				RDITM,
				RDZ58MDT,
				RDZ58PDT,
				RDPQOH,
                RDZ58CQTY,
				IBSTKT,
				IBGLPT,
				IBANPL,
                IBPRP4,
				AB1.ABALPH AS IBANPLALPH,
				IMDSC1,
				IMDSC2,
				(SELECT
					MIN(HLUPMJ)
				FROM
					F580559
				WHERE
					HLZ58RID = RHZ58RID AND
					HLMCU = RHMCU
					) AS HLUPMJ
            FROM 
	            F580055
	            INNER JOIN F580056
	            ON  RDZ58RID = RHZ58RID
				INNER JOIN F4102
				ON	IBMCU = RHMCU AND
					IBITM = RDITM
				INNER JOIN F4101
				ON	IMITM = IBITM
                INNER JOIN F0006
                ON  MCMCU = RHMCU
				LEFT OUTER JOIN F0101 AB1
				ON	AB1.ABAN8 = IBANPL
            WHERE
	            {0}
				RHZ58HST NOT IN ('0', '8') AND
                RDLOTN = '' AND
                RDPQOH <> 0
            ORDER BY 1	
            FOR READ ONLY", filterString);
            return cmd;
        }

        public Racks GetStockRacks0(BusinessSystemSession session, List<string> plantList, int agingDays)
        {
            var racks = new Racks();
            iDB2Command cmd;
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {
                // EFFECTIVE DATE
                //parameters.Add(new iDB2Parameter("TDAY", iDB2DbType.iDB2Decimal));
                //parameters.Last().Precision = 6;
                //parameters.Last().Scale = 0;
                //parameters.Last().Value = E1TimeManager.CastToE1Date(DateTime.Now);

                // Plant List
                if (plantList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string plantItem in plantList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("RHMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Status
                filters.Add("RHZ58HST NOT IN ('8') AND\r\n");

                // Aging Days
                if (agingDays > 0)
                {
                    var dt = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(agingDays * -1));
                    filters.Add("(RDZ58MDT BETWEEN 0 AND ? OR RDZ58PDT BETWEEN 0 AND ?) AND\r\n");
                    parameters.Add(new iDB2Parameter("DT1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                    parameters.Add(new iDB2Parameter("DT2", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                }

                //// Customer Num
                //if (customerNum > 0)
                //{
                //    filters.Add("RHAN8 = ? AND\r\n");
                //    parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                //    parameters.Last().Precision = 8;
                //    parameters.Last().Scale = 0;
                //    parameters.Last().Value = customerNum;
                //}

                cmd = GetStockRacks0(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));
                var f4102Mgr = new F4102Manager(session);

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rack = new Rack();
                        rack.RackId = reader.Get<Decimal>("RHZ58RID", 0);
                        rack.AssetId = reader.Get("RHURRF", String.Empty).Trim();
                        rack.StockSerialNum = reader.Get("RDZ58SN", String.Empty).Trim();
                        rack.CostCenter = reader.Get("RHMCU", String.Empty).Trim();
                        rack.PlantNum = reader.Get("RHMCU", String.Empty).Trim();
                        rack.PlantCode = reader.Get("MCDL01", String.Empty).Trim();
                        rack.RackType = reader.Get("RHZ58RT", String.Empty).Trim();
                        rack.Location = reader.Get("RHLOCN", String.Empty).Trim();
                        rack.HeaderStatus = reader.Get("RHZ58HST", String.Empty).Trim();
                        rack.HeaderStatusDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("RHZ58RSDT", 0));
                        rack.ItemNum = reader.Get<Decimal>("RDITM", 0);
                        rack.CustomerNum = reader.Get<Decimal>("RHAN8", 0);
                        rack.ShipToCustomerNum = reader.Get<Decimal>("RHSHAN", 0);
                        rack.ManufacturedDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("RDZ58MDT", 0));
                        rack.ProductionDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("RDZ58PDT", 0));
                        rack.ManufacturedAgingDays = (DateTime.Now - rack.ManufacturedDate.Value).Days;
                        rack.TareWeight = reader.Get<Decimal>("RHZ58RTWT", 0) / 100;
                        rack.GlassWeight = reader.Get<Decimal>("RHZ58GWTT", 0) / 100;
                        rack.GlassWeightTons = reader.Get<Decimal>("RHZ58GWTT", 0) / 100 / 2000;
                        rack.GrossWeight = reader.Get<Decimal>("RHZ58MTRW", 0) / 100;
                        rack.GrossWeightTons = reader.Get<Decimal>("RHZ58MTRW", 0) / 100 / 2000;
                        rack.Quantity = reader.Get<Decimal>("RDPQOH", 0) / 1000;
                        rack.OriginalQuantity = reader.Get<Decimal>("RDZ58CQTY", 0) / 1000;
                        rack.PlannerNum = reader.Get<Decimal>("IBANPL", 0);
                        rack.PlannerName = reader.Get<string>("IBANPLALPH", String.Empty);
                        rack.PlanningFamily = reader.Get<string>("IBPRP4", String.Empty);
                        rack.ReceivedDate = E1TimeManager.CastToDateTime(reader.Get<Decimal>("HLUPMJ"));
                        rack.ReceivedAgingDays = (DateTime.Now - rack.ReceivedDate.Value).Days;
                        rack.ReceivedQty = rack.OriginalQuantity;
                        rack.ItemDesc = reader.Get<string>("IMDSC1", String.Empty);
                        rack.StockType = reader.Get<string>("IBSTKT", String.Empty);
                        rack.GlCategory = reader.Get<string>("IBGLPT", String.Empty);
                        rack.ItemConsumptionDays = 90;
                        rack.LoadNum = reader.Get<Decimal>("RHPSN", 0);
                        rack.SuggestedLoadNum = reader.Get<Decimal>("RHZ58SLDN", 0);
                        racks.Add(rack);
                    }
                    reader.Close();
                }

                // Item Properties
                var items = racks.Select(x => new {x.ItemNum}).Distinct().ToList();

                // Attributes
                var itemAttDict = new Dictionary<decimal, ItemAttributes>();
                //var itemCustPartDict = new Dictionary<decimal, string>();
                //var itemCostDict = new Dictionary<decimal, decimal>();
                //var itemConsumptionDict = new Dictionary<decimal, decimal>();
                foreach (var item in items)
                {
                    itemAttDict.Add(item.ItemNum, _itemAttMgr.GetDiscreteItemAttributes(session, item.ItemNum));
                    //itemCustPartDict.Add(item.ItemNum, f4102Mgr.GetCustomerPartNum(session, item.PlannerNum, item.ItemNum));
                    //itemCostDict.Add(item.ItemNum, f4102Mgr.GetCost(session, PlantManager.GetPlant(item.CostCenter), item.ItemNum));
                    //itemConsumptionDict.Add(item.ItemNum, f4102Mgr.GetConsumption(session, PlantManager.GetPlant(item.CostCenter), item.ItemNum, 90));
                }

                // Assign Attributes
                foreach (var rack in racks)
                {
                    var itemAtts = itemAttDict[rack.ItemNum];
                    rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
                    rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                    rack.LitesPerRack = itemAtts.GetDecimalAttributeValue("Z58LPR");
                    rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                    rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
                    //var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
                    //rack.Construction = flexDescs[0];
                    //rack.Dimension = flexDescs[1];
                    //rack.CustomerPartNum = itemCustPartDict[rack.ItemNum];
                    //rack.UnitCost = itemCostDict[rack.ItemNum];
                    //rack.TotalCost = rack.UnitCost * rack.Quantity;
                    //rack.ItemConsumptionAmt = itemConsumptionDict[rack.ItemNum];
                }
                //GetCost(session, racks);
                //GetReceivedDate(session, racks);
                //GetAverageConsumption(session, racks, 90);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }

        /*********************************************************************************
           * Get MTS Racks
           *******************************************************************************/
        public iDB2Command GetStockRacks1(List<string> filters)
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
	            RHZ58RID,
                RHMCU,
                MCDL01,
	            RHZ58RT,
	            RHAN8,
	            RHSHAN,
	            RHZ58HST,
                RHZ58RSDT,
	            RHLOCN,
	            RHZ58RTWT,
	            RHZ58MTRW,
                RHZ58GWTT,
                RHURRF,
                MIN(RDZ58SN) AS RDZ58SN,
	            MIN(RDITM) AS RDITM,
	            MAX(RDZ58MDT) AS RDZ58MDT,
	            MAX(RDZ58PDT) AS RDZ58PDT,
	            SUM(RDPQOH) AS RDPQOH,
                SUM(RDZ58CQTY) AS RDZ58CQTY
            FROM 
	            F580055
	            INNER JOIN F580056
	            ON RDZ58RID = RHZ58RID
                INNER JOIN F0006
                ON MCMCU = RHMCU
            WHERE
                {0}
                RDLOTN = ''
            GROUP BY
	            RHZ58RID,
                RHMCU,
                MCDL01,
	            RHZ58RT,
	            RHAN8,
	            RHSHAN,
	            RHZ58HST,
                RHZ58RSDT,
	            RHLOCN,
	            RHZ58RTWT,
	            RHZ58MTRW,
                RHZ58GWTT,
                RHURRF,
	            RHZ58BOX,
	            RHZ58SCN
            HAVING
                SUM(RDPQOH) > 0
            ORDER BY 1,2,3	
            FOR READ ONLY", filterString);
            return cmd;
        }

        public Racks GetStockRacks1(BusinessSystemSession session, List<string> plantList, int agingDays)
        {
            var racks = new Racks();
            iDB2Command cmd;
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (plantList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string plantItem in plantList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("RHMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Plant
                //filters.Add("RHMCU = ? AND\r\n");
                //parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                //parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Status
                filters.Add("RHZ58HST NOT IN ('8') AND\r\n");

                // Aging Days
                if (agingDays > 0)
                {
                    var dt = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(agingDays * -1));
                    filters.Add("(RDZ58MDT <= ? OR RDZ58PDT <= ?) AND\r\n");
                    parameters.Add(new iDB2Parameter("DT1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                    parameters.Add(new iDB2Parameter("DT2", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                }

                //// Customer Num
                //if (customerNum > 0)
                //{
                //    filters.Add("RHAN8 = ? AND\r\n");
                //    parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                //    parameters.Last().Precision = 8;
                //    parameters.Last().Scale = 0;
                //    parameters.Last().Value = customerNum;
                //}

                cmd = GetStockRacks1(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));
                var f4102Mgr = new F4102Manager(session);

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rack = new Rack();
                        rack.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
                        rack.AssetId = CastDbNull.To(reader["RHURRF"], String.Empty).Trim();
                        rack.StockSerialNum = CastDbNull.To(reader["RDZ58SN"], String.Empty).Trim();
                        rack.CostCenter = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.PlantNum = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.PlantCode = CastDbNull.To(reader["MCDL01"], String.Empty).Trim();
                        rack.RackType = CastDbNull.To(reader["RHZ58RT"], String.Empty).Trim();
                        rack.Location = CastDbNull.To(reader["RHLOCN"], String.Empty).Trim();
                        rack.HeaderStatus = CastDbNull.To(reader["RHZ58HST"], String.Empty).Trim();
                        rack.HeaderStatusDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RHZ58RSDT"], 0));
                        rack.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
                        rack.CustomerNum = CastDbNull.To<Decimal>(reader["RHAN8"], 0);
                        rack.ShipToCustomerNum = CastDbNull.To<Decimal>(reader["RHSHAN"], 0);
                        rack.ManufacturedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RDZ58MDT"], 0));
                        rack.ProductionDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RDZ58PDT"], 0));
                        rack.ManufacturedAgingDays = (DateTime.Now - rack.ManufacturedDate.Value).Days;
                        rack.TareWeight = CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0) / 100;
                        rack.GlassWeight = CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0) / 100;
                        rack.GlassWeightTons = CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0) / 100 / 2000;
                        rack.GrossWeight = CastDbNull.To<Decimal>(reader["RHZ58MTRW"], 0) / 100;
                        rack.GrossWeightTons = CastDbNull.To<Decimal>(reader["RHZ58MTRW"], 0) / 100 / 2000;
                        rack.Quantity = CastDbNull.To<Decimal>(reader["RDPQOH"], 0) / 1000;
                        rack.OriginalQuantity = CastDbNull.To<Decimal>(reader["RDZ58CQTY"], 0) / 1000;
                        var itemAtts = _itemAttMgr.GetDiscreteItemAttributes(session, rack.ItemNum);
                        rack.ItemAttributes = itemAtts;
                        rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
                        rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                        rack.LitesPerRack = itemAtts.GetDecimalAttributeValue("Z58LPR");
                        rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                        var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
                        rack.Construction = flexDescs[0];
                        rack.Dimension = flexDescs[1];
                        rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                        rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
                        var itemBranchAttributes = f4102Mgr.GetPlannerNum(session, rack.PlantNum, rack.ItemNum);
                        rack.PlannerNum = (decimal)itemBranchAttributes["PlannerNum"];
                        rack.PlannerName = (string)itemBranchAttributes["PlannerName"];
                        rack.PlanningFamily = (string)itemBranchAttributes["IBPRP4"];
                        rack.CustomerPartNum = f4102Mgr.GetCustomerPartNum(session, rack.PlannerNum, rack.ItemNum);

                        rack.ReceivedDate = rack.ProductionDate;
                        rack.ReceivedAgingDays = (DateTime.Now - rack.ReceivedDate.Value).Days;
                        rack.ReceivedQty = rack.OriginalQuantity;
                        racks.Add(rack);
                    }
                    reader.Close();
                }
                GetItemProperties(session, racks);
                GetCost(session, racks);
                //GetReceivedDate(session, racks);
                GetAverageConsumption(session, racks, 90);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }

        /*********************************************************************************
          * Discrete Item Attributes
          *******************************************************************************/
        public iDB2Command GetItemProperties()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            IBSTKT,
                IBGLPT,
                IMDSC1,
                IMDSC2
            FROM
	            F4102
                INNER JOIN F4101
                ON IMITM = IBITM
            WHERE
                IBITM = ? AND
                IBMCU = ? 
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetItemProperties(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetItemProperties"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.ItemNum;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.ItemDesc = CastDbNull.To(reader["IMDSC1"], String.Empty).Trim();
                            rack.StockType = CastDbNull.To(reader["IBSTKT"], String.Empty).Trim();
                            rack.GlCategory = CastDbNull.To(reader["IBGLPT"], String.Empty).Trim();
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

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetCost()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(IECSL) AS IECSL
            FROM
	            F30026
            WHERE
                IEITM = ? AND
                IEMMCU = ? AND
                IELEDG = '07'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetCost(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetCost"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.ItemNum;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.UnitCost = CastDbNull.To<Decimal>(reader["IECSL"], 0) / 10000;
                            rack.TotalCost = rack.UnitCost * rack.Quantity;
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

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetReceivedDate()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                HLUPMJ
            FROM (
                SELECT
	                HLUPMJ,
                    ROW_NUMBER() OVER (PARTITION BY HLZ58RID ORDER BY HLUKID ASC) AS ROWNUM
                FROM
	                F580559
                WHERE
                    HLZ58RID = ? AND
                    HLMCU = ?) A
            WHERE
                A.ROWNUM = 1
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("RID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["RID"].Precision = 15;
            cmd.Parameters["RID"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetReceivedDate(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetReceivedDate"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.RackId;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.ReceivedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["HLUPMJ"], 0));
                            rack.ReceivedAgingDays = (DateTime.Now - rack.ReceivedDate.Value).Days;
                            rack.ReceivedQty = rack.OriginalQuantity;
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

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetConsumption()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(ILTRQT) AS ILTRQT
            FROM
	            F4111
            WHERE
                ILITM = ? AND
                ILMCU = ? AND
                ILTRDJ >= ? AND
                ILDCT IN ('2I','IM','1A')
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("TRDJ", iDB2DbType.iDB2Decimal));
            cmd.Parameters["TRDJ"].Precision = 6;
            cmd.Parameters["TRDJ"].Scale = 0;
            return cmd;
        }

        public void GetAverageConsumption(BusinessSystemSession session, Racks racks, int interval)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetConsumption"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.ItemNum;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(DateTime.Now.Date.AddDays(interval * -1));
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.ItemConsumptionDays = interval;
                            rack.ItemConsumptionAmt = CastDbNull.To<Decimal>(reader["ILTRQT"], 0) / -1000;
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

    }
}
