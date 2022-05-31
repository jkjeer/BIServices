using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.RackSvc
{
    public class ConfiguredRackManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;

        public ConfiguredRackManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("ConfiguredRackManager.GetManufacturedCost", GetManufacturedCost());
            hash.Add("ConfiguredRackManager.GetReceivedCost", GetReceivedCost());
            hash.Add("ConfiguredRackManager.GetReceivedDate", GetReceivedDate());
            hash.Add("ConfiguredRackManager.GetItemProperties", GetItemProperties());
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
          * HV Racks - Single Detail with Qty > 1
          *******************************************************************************/
        public iDB2Command GetHVManufacturedConfiguredRacks(List<string> filters)
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
		        A.RHZ58RID,
		        A.RHMCU,
		        A.MCDL01,
		        A.RHZ58RT,
		        A.RHAN8,
		        A.RHSHAN,
		        A.RHZ58HST,
		        A.RHZ58RSDT,
		        A.RHLOCN,
		        A.RHZ58RTWT,
		        A.RHZ58MTRW,
		        A.RHZ58GWTT,
		        A.RHZ58BOX,
		        A.RHZ58SCN,
		        MIN(A.RDDOC1) AS RDDOC1,
		        MIN(A.RDLOTN) AS RDLOTN,
		        MIN(A.RDZ58SN) AS RDZ58SN,
		        MIN(A.RDITM) AS RDITM,
		        MAX(A.RDZ58MDT) AS RDZ58MDT,
		        MAX(A.RDZ58PDT) AS RDZ58PDT,
		        SUM(A.RDPQOH) AS RDPQOH,
		        SUM(A.RDZ58CQTY) AS RDZ58CQTY,
		        MIN(A.CMCFGID) AS CMCFGID
	        FROM (
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
				        RDLOTN,
                        RDDOC1,
				        RDZ58SN,
				        RDITM,
				        RDZ58MDT,
				        RDZ58PDT,
				        RDPQOH,
                        RDZ58CQTY,
                        COALESCE(CMCFGID, 0) AS CMCFGID,
				        ROW_NUMBER() OVER (PARTITION BY RHZ58RID ORDER BY CMCFGID ASC) AS RN
                    FROM 
	                    F580055
	                    INNER JOIN F580056
	                    ON  RDZ58RID = RHZ58RID
                        INNER JOIN F0006
                        ON  MCMCU = RHMCU
                        LEFT OUTER JOIN F3201
                        ON  CMDOCO = RDDOC1 AND
                            CMDCTO = 'WO' AND
                            CMKCOO = '*' 
                    WHERE
                        {0}
				        RHZ58HST NOT IN ('0', '8') AND
                        RDDOC1 > 0 AND
                        RDLOTN != ''
				        ) A
	        WHERE 
		        A.RN = 1
            GROUP BY
		        A.RHZ58RID,
		        A.RHMCU,
		        A.MCDL01,
		        A.RHZ58RT,
		        A.RHAN8,
		        A.RHSHAN,
		        A.RHZ58HST,
		        A.RHZ58RSDT,
		        A.RHLOCN,
		        A.RHZ58RTWT,
		        A.RHZ58MTRW,
		        A.RHZ58GWTT,
		        A.RHZ58BOX,
		        A.RHZ58SCN
            HAVING
                SUM(A.RDPQOH) > 1000 AND
                COUNT(*) = 1
            ORDER BY 1,2,3	
            FOR READ ONLY", filterString);
            return cmd;
        }

        public Racks GetHVManufacturedConfiguredRacks(BusinessSystemSession session, List<string> plantList, int agingDays)
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

                // Status
                filters.Add("RHZ58HST NOT IN ('0','8') AND\r\n");

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

                cmd = GetHVManufacturedConfiguredRacks(filters);
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
                        rack.StockSerialNum = CastDbNull.To(reader["RDZ58SN"], String.Empty).Trim();
                        rack.CostCenter = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.PlantNum = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.PlantCode = CastDbNull.To(reader["MCDL01"], String.Empty).Trim();
                        rack.RackType = CastDbNull.To(reader["RHZ58RT"], String.Empty).Trim();
                        rack.Location = CastDbNull.To(reader["RHLOCN"], String.Empty).Trim();
                        rack.HeaderStatus = CastDbNull.To(reader["RHZ58HST"], String.Empty).Trim();
                        rack.HeaderStatusDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RHZ58RSDT"], 0));
                        rack.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
                        rack.LotNum = CastDbNull.To(reader["RDLOTN"], String.Empty).Trim();
                        rack.CustomerNum = CastDbNull.To<Decimal>(reader["RHAN8"], 0);
                        rack.ShipToCustomerNum = CastDbNull.To<Decimal>(reader["RHSHAN"], 0);
                        rack.WOrderNum = CastDbNull.To<Decimal>(reader["RDDOC1"], 0);
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
                        var configId = CastDbNull.To<Decimal>(reader["CMCFGID"], 0);

                        ItemAttributes itemAtts;
                        itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, configId);
                        if (itemAtts.Count == 0)
                        {
                            itemAtts = _itemAttMgr.GetConfiguredItemArchiveAttributes(session, configId);
                        }

                        rack.ItemAttributes = itemAtts;
                        rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
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
                        racks.Add(rack);
                    }

                    reader.Close();
                }
                GetItemProperties(session, racks);
                GetManufacturedCost(session, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }


        /*********************************************************************************
          * Purchased MTO Racks
          *******************************************************************************/
        public iDB2Command GetHVPurchasedConfiguredRacks(List<string> filters)
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
				        RDLOTN,
                        RDDOC2,
                        RDXCTO,
                        RDLNIC,
                        RDPKCO,
				        RDZ58SN,
				        RDITM,
				        RDZ58MDT,
				        RDZ58PDT,
				        RDPQOH,
                        RDZ58CQTY,
				        COALESCE(CMCFGID, 0) AS CMCFGID,
				        ROW_NUMBER() OVER (PARTITION BY RHZ58RID, RDLOTN, RDITM ORDER BY CMCFGID ASC) AS CMRN
                    FROM 
	                    F580055
	                    INNER JOIN F580056
	                    ON  RDZ58RID = RHZ58RID
                        INNER JOIN F0006
                        ON  MCMCU = RHMCU
                        LEFT OUTER JOIN F3201
				        ON  CMDOCO = RDDOC2 AND
					        CMDCTO = RDXCTO AND
					        CMKCOO = RDPKCO AND
					        CMLNID = FLOOR(RDLNIC / 1000) * 1000
                    WHERE
	                    {0}
				        RHZ58HST NOT IN ('0', '8') AND
				        RDDOC2 > 0 AND
                        RDLOTN != ''
				        ) A
	        WHERE 
                A.CMRN = 1
            ORDER BY 1	
            FOR READ ONLY", filterString);
            return cmd;
        }

        public Racks GetHVPurchasedConfiguredRacks(BusinessSystemSession session, List<string> plantList, int agingDays)
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

                cmd = GetHVPurchasedConfiguredRacks(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                var ds = new DataSet();
                var da = new iDB2DataAdapter(cmd).Fill(ds);
                var rows = ds.Tables[0].AsEnumerable();

                var rackItems = from p in rows
                    select new
                    {
                        RHZ58RID = p.Field<decimal>("RHZ58RID"),
                        RHMCU = p.Field<string>("RHMCU"),
                        MCDL01 = p.Field<string>("MCDL01"),
                        RHZ58RT = p.Field<string>("RHZ58RT"),
                        RHAN8 = p.Field<decimal>("RHAN8"),
                        RHSHAN = p.Field<decimal>("RHSHAN"),
                        RHZ58HST = p.Field<string>("RHZ58HST"),
                        RHZ58RSDT = E1TimeManager.CastToDateTime(p.Field<decimal>("RHZ58RSDT")),
                        RHLOCN = p.Field<string>("RHLOCN"),
                        RHZ58RTWT = p.Field<decimal>("RHZ58RTWT") / 100,
                        RHZ58MTRW = p.Field<decimal>("RHZ58MTRW") / 100,
                        RHZ58GWTT = p.Field<decimal>("RHZ58GWTT") / 100,
                        RHZ58BOX = p.Field<decimal>("RHZ58BOX"),
                        RHZ58SCN = p.Field<decimal>("RHZ58SCN"),
                        RDLOTN = p.Field<string>("RDLOTN"),
                        RDDOC2 = p.Field<decimal>("RDDOC2"),
                        RDXCTO = p.Field<string>("RDXCTO"),
                        RDLNIC = p.Field<decimal>("RDLNIC"),
                        RDPKCO = p.Field<string>("RDPKCO"),
                        RDZ58SN = p.Field<string>("RDZ58SN"),
                        RDITM = p.Field<decimal>("RDITM"),
                        RDZ58MDT = E1TimeManager.CastToDateTime(p.Field<decimal>("RDZ58MDT")),
                        RDZ58PDT = E1TimeManager.CastToDateTime(p.Field<decimal>("RDZ58PDT")),
                        RDPQOH = p.Field<decimal>("RDPQOH") / 1000,
                        RDZ58CQTY = p.Field<decimal>("RDZ58CQTY") / 1000,
                        CMCFGID = p.Field<decimal>("CMCFGID")
                    };

                var rackGroup = from p in rackItems
                    orderby p.RDZ58SN
                    group p by p.RHZ58RID
                    into gr
                    where gr.Count() == 1 && gr.Sum(x => x.RDPQOH) > 1
                                select gr;

                var f4102Mgr = new F4102Manager(session);
                foreach (var g in rackGroup)
                {
                    var configId = g.First().CMCFGID;
                    ItemAttributes itemAtts;
                    itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, configId);
                    if (itemAtts.Count == 0)
                    {
                        itemAtts = _itemAttMgr.GetConfiguredItemArchiveAttributes(session, configId);
                    }
                    var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);

                    var rack = new Rack();
                    rack.RackId = g.Key;
                    rack.StockSerialNum = g.First().RDZ58SN;
                    rack.CostCenter = g.First().RHMCU;
                    rack.PlantNum = g.First().RHMCU;
                    rack.PlantCode = g.First().MCDL01;
                    rack.RackType = g.First().RHZ58RT;
                    rack.Location = g.First().RHLOCN;
                    rack.HeaderStatus = g.First().RHZ58HST;
                    rack.HeaderStatusDate = g.First().RHZ58RSDT;
                    rack.ItemNum = g.First().RDITM;
                    rack.LotNum = g.First().RDLOTN;
                    rack.POrderCompany = g.First().RDPKCO;
                    rack.POrderNum = g.First().RDDOC2;
                    rack.POrderType = g.First().RDXCTO;
                    rack.POrderLineNum = g.First().RDLNIC / 1000;
                    rack.CustomerNum = g.First().RHAN8;
                    rack.ShipToCustomerNum = g.First().RHSHAN;
                    rack.ManufacturedDate = g.First().RDZ58MDT;
                    rack.ProductionDate = g.First().RDZ58PDT;
                    rack.ManufacturedAgingDays = (DateTime.Now - rack.ManufacturedDate.Value).Days;
                    rack.ReceivedQty = g.First().RDZ58CQTY;
                    rack.TareWeight = g.First().RHZ58RTWT;
                    rack.GlassWeight = g.First().RHZ58GWTT;
                    rack.GlassWeightTons = g.First().RHZ58GWTT / 2000;
                    rack.GrossWeight = g.First().RHZ58MTRW;
                    rack.GrossWeightTons = g.First().RHZ58MTRW / 2000;
                    rack.Quantity = g.First().RDPQOH;
                    rack.OriginalQuantity = g.First().RDZ58CQTY;
                    rack.ConfigId = g.First().CMCFGID;
                    rack.ItemAttributes = itemAtts;
                    rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
                    rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                    rack.Construction = flexDescs[0];
                    rack.Dimension = flexDescs[1];
                    rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                    rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
                    var itemBranchAttributes = f4102Mgr.GetPlannerNum(session, rack.PlantNum, rack.ItemNum);
                    rack.PlannerNum = (decimal)itemBranchAttributes["PlannerNum"];
                    rack.PlanningFamily = (string)itemBranchAttributes["IBPRP4"];
                    racks.Add(rack);
                }
                GetItemProperties(session, racks);
                GetReceivedCost(session, racks);
                GetReceivedDate(session, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }

        /*********************************************************************************
          * Purchased MTO Racks
          *******************************************************************************/
        //public iDB2Command GetHVPurchasedConfiguredRacks(List<string> filters)
        //{

        //    var filterString = new StringBuilder();
        //    foreach (string filter in filters)
        //    {
        //        filterString.Append(filter);
        //    }

        //    var cmd = new iDB2Command();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = String.Format(@"
	       // SELECT 
		      //  A.RHZ58RID,
		      //  A.RHMCU,
		      //  A.MCDL01,
		      //  A.RHZ58RT,
		      //  A.RHAN8,
		      //  A.RHSHAN,
		      //  A.RHZ58HST,
		      //  A.RHZ58RSDT,
		      //  A.RHLOCN,
		      //  A.RHZ58RTWT,
		      //  A.RHZ58MTRW,
		      //  A.RHZ58GWTT,
		      //  A.RHZ58BOX,
		      //  A.RHZ58SCN,
		      //  MIN(A.RDLOTN) AS RDLOTN,
		      //  MIN(A.RDZ58SN) AS RDZ58SN,
		      //  MIN(A.RDITM) AS RDITM,
		      //  MAX(A.RDZ58MDT) AS RDZ58MDT,
		      //  MAX(A.RDZ58PDT) AS RDZ58PDT,
		      //  SUM(A.RDPQOH) AS RDPQOH,
		      //  SUM(A.RDZ58CQTY) AS RDZ58CQTY,
		      //  MIN(A.CMCFGID) AS CMCFGID
	       // FROM (
        //            SELECT 
	       //             RHZ58RID,
        //                RHMCU,
        //                MCDL01,
	       //             RHZ58RT,
	       //             RHAN8,
	       //             RHSHAN,
	       //             RHZ58HST,
        //                RHZ58RSDT,
	       //             RHLOCN,
	       //             RHZ58RTWT,
	       //             RHZ58MTRW,
        //                RHZ58GWTT,
				    //    RHZ58BOX,
				    //    RHZ58SCN,
				    //    RDLOTN,
        //                RDDOC2,
        //                RDXCTO,
        //                RDLNIC,
        //                RDPKCO,
				    //    RDZ58SN,
				    //    RDITM,
				    //    RDZ58MDT,
				    //    RDZ58PDT,
				    //    RDPQOH,
        //                RDZ58CQTY,
				    //    COALESCE(CMCFGID, 0) AS CMCFGID,
				    //    ROW_NUMBER() OVER (PARTITION BY RHZ58RID ORDER BY CMCFGID ASC) AS RN
        //            FROM 
	       //             F580055
	       //             INNER JOIN F580056
	       //             ON  RDZ58RID = RHZ58RID
        //                INNER JOIN F0006
        //                ON  MCMCU = RHMCU
        //                LEFT OUTER JOIN F3201
				    //    ON  CMDOCO = RDDOC2 AND
					   //     CMDCTO = RDXCTO AND
					   //     CMKCOO = RDPKCO AND
					   //     CMLNID = RDLNIC
        //            WHERE
	       //             {0}
				    //    RHZ58HST NOT IN ('0', '8') AND
				    //    RDDOC2 > 0 AND
        //                RDLOTN != ''
				    //    ) A
	       // WHERE 
		      //  A.RN = 1
        //    GROUP BY
		      //  A.RHZ58RID,
		      //  A.RHMCU,
		      //  A.MCDL01,
		      //  A.RHZ58RT,
		      //  A.RHAN8,
		      //  A.RHSHAN,
		      //  A.RHZ58HST,
		      //  A.RHZ58RSDT,
		      //  A.RHLOCN,
		      //  A.RHZ58RTWT,
		      //  A.RHZ58MTRW,
		      //  A.RHZ58GWTT,
		      //  A.RHZ58BOX,
		      //  A.RHZ58SCN
        //    HAVING
        //        SUM(A.RDPQOH) > 1000 AND
        //        COUNT(*) = 1
        //    ORDER BY 1,2,3	
        //    FOR READ ONLY", filterString);
        //    return cmd;
        //}

        //public Racks GetHVPurchasedConfiguredRacks(BusinessSystemSession session, List<string> plantList, int agingDays)
        //{
        //    var racks = new Racks();
        //    var cmd = new iDB2Command();
        //    List<String> filters = new List<string>();
        //    List<iDB2Parameter> parameters = new List<iDB2Parameter>();
        //    try
        //    {

        //        // Plant List
        //        if (plantList.Count() > 0)
        //        {
        //            string strInClause = "";
        //            foreach (string plantItem in plantList)
        //            {
        //                var plant = PlantManager.GetPlant(plantItem);
        //                strInClause = strInClause + "?,";
        //                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
        //                parameters.Last().Value = plant.PlantNum.PadLeft(12);
        //            }
        //            filters.Add("RHMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
        //        }

        //        // Status
        //        filters.Add("RHZ58HST NOT IN ('8') AND\r\n");

        //        // Aging Days
        //        if (agingDays > 0)
        //        {
        //            var dt = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(agingDays * -1));
        //            filters.Add("(RDZ58MDT <= ? OR RDZ58PDT <= ?) AND\r\n");
        //            parameters.Add(new iDB2Parameter("DT1", iDB2DbType.iDB2Decimal));
        //            parameters.Last().Precision = 6;
        //            parameters.Last().Scale = 0;
        //            parameters.Last().Value = dt;
        //            parameters.Add(new iDB2Parameter("DT2", iDB2DbType.iDB2Decimal));
        //            parameters.Last().Precision = 6;
        //            parameters.Last().Scale = 0;
        //            parameters.Last().Value = dt;
        //        }

        //        cmd = GetHVPurchasedConfiguredRacks(filters);
        //        foreach (iDB2Parameter parameter in parameters)
        //        {
        //            cmd.Parameters.Add(parameter);
        //        }
        //        cmd.Connection = session.DbConnection;
        //        cmd.CommandTimeout = 0;
        //        _logger.Info(Logging.DbCmdString(cmd));

        //        using( IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
        //        while (reader.Read())
        //        {
        //            var rack = new Rack();
        //            rack.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
        //            rack.StockSerialNum = CastDbNull.To<String>(reader["RDZ58SN"], String.Empty).Trim();
        //            rack.CostCenter = CastDbNull.To<String>(reader["RHMCU"], String.Empty).Trim();
        //            rack.PlantNum = CastDbNull.To<String>(reader["RHMCU"], String.Empty).Trim();
        //            rack.PlantCode = CastDbNull.To<String>(reader["MCDL01"], String.Empty).Trim();
        //            rack.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
        //            rack.Location = CastDbNull.To<String>(reader["RHLOCN"], String.Empty).Trim();
        //            rack.HeaderStatus = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
        //            rack.HeaderStatusDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RHZ58RSDT"], 0));
        //            rack.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
        //            rack.LotNum = CastDbNull.To<String>(reader["RDLOTN"], String.Empty).Trim();
        //            rack.CustomerNum = CastDbNull.To<Decimal>(reader["RHAN8"], 0);
        //            rack.ShipToCustomerNum = CastDbNull.To<Decimal>(reader["RHSHAN"], 0);
        //            rack.ManufacturedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RDZ58MDT"], 0));
        //            rack.ProductionDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RDZ58PDT"], 0));
        //            rack.ManufacturedAgingDays = (DateTime.Now - rack.ManufacturedDate.Value).Days;
        //            rack.ReceivedQty = CastDbNull.To<Decimal>(reader["RDZ58CQTY"], 0) / 1000;
        //            rack.TareWeight = CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0) / 100;
        //            rack.GlassWeight = CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0) / 100;
        //            rack.GlassWeightTons = CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0) / 100 / 2000;
        //            rack.GrossWeight = CastDbNull.To<Decimal>(reader["RHZ58MTRW"], 0) / 100;
        //            rack.GrossWeightTons = CastDbNull.To<Decimal>(reader["RHZ58MTRW"], 0) / 100 / 2000;
        //            rack.Quantity = CastDbNull.To<Decimal>(reader["RDPQOH"], 0) / 1000;
        //            rack.OriginalQuantity = CastDbNull.To<Decimal>(reader["RDZ58CQTY"], 0) / 1000;
        //            var configId = CastDbNull.To<Decimal>(reader["CMCFGID"], 0);
        //            ItemAttributes itemAtts;
        //            itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, configId);
        //            if (itemAtts.Count == 0)
        //            {
        //                itemAtts = _itemAttMgr.GetConfiguredItemArchiveAttributes(session, configId);
        //            }
        //            rack.ItemAttributes = itemAtts;
        //            rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
        //            rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
        //            var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
        //            rack.Construction = flexDescs[0];
        //            rack.Dimension = flexDescs[1];
        //            rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
        //            rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
        //            racks.Add(rack);
        //        }
        //        reader.Close();
        //        GetItemProperties(session, racks);
        //        //GetReceivedCost(session, racks);
        //        GetReceivedDate(session, racks);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //    return racks;
        //}

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
                var cmd = session.DbCommands["ConfiguredRackManager.GetItemProperties"];
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
          * Manufactured Cost
          *******************************************************************************/
        public iDB2Command GetManufacturedCost()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(IGOPAT) AS IGOPAT
            FROM
	            F3102
            WHERE
                IGDOCO = ?
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            return cmd;
        }

        public void GetManufacturedCost(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["ConfiguredRackManager.GetManufacturedCost"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.WOrderNum;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.UnitCost = CastDbNull.To<Decimal>(reader["IGOPAT"], 0) / 10000;
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
          * Received Cost
          *******************************************************************************/
        public iDB2Command GetReceivedCost()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                PDPRRC,
                PDAEXP
            FROM
	            F4311
            WHERE
                PDKCOO = ? AND
                PDDOCO = ? AND
                PDDCTO = ? AND
                PDLNID = ?
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("KCOO", iDB2DbType.iDB2Graphic, 5));
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetReceivedCost(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["ConfiguredRackManager.GetReceivedCost"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.POrderCompany;
                    cmd.Parameters[1].Value = rack.POrderNum;
                    cmd.Parameters[2].Value = rack.POrderType;
                    cmd.Parameters[3].Value = rack.POrderLineNum * 1000;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            rack.UnitCost = CastDbNull.To<Decimal>(reader["PDPRRC"], 0) / 10000;
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
                var cmd = session.DbCommands["ConfiguredRackManager.GetReceivedDate"];
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
