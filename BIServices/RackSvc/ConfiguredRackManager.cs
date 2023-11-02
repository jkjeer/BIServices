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
            hash.Add("ConfiguredRackManager.GetCustomerName", GetCustomerName());
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
        public iDB2Command GetHVMTORacks(List<string> filters)
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
                RHZ58RCD,
	            RDLOTN,
	            RDZ58SN,
	            RDITM,
	            RDZ58MDT,
	            RDZ58PDT,
	            RDPQOH,
                RDZ58CQTY,
	            RDDSC1,
	            RDDSC2,
	            RDDOCO,
	            RDDCTO,
	            RDLNID,
	            RDDOC1,
	            RDXDCTO,
                RDPKCO,
	            RDDOC2,
	            RDXCTO,
	            RDLNIC,
                RDDSC1,
                RDDSC2,
                IMDSC1,
                IBSTKT,
                IBGLPT,
                IBPRP4,
                IBANPL,
	            (select 
		            CMCFGID
	            from 
		            F3201
	            where
		            CMKCOO = RDKCOO AND
		            CMDOCO = RDDOCO AND
		            CMDCTO = RDDCTO AND
		            CMLNID = RDLNID AND
		            CMDOCO > 0
		            FETCH FIRST 1 ROWS ONLY) AS CMCFGIDS,
	            (select 
		            CMCFGID
	            from 
		            F3201
	            where
		            CMDOCO = RDDOC1 AND
		            CMDCTO = RDXDCTO AND
		            CMDOCO > 0
		            FETCH FIRST 1 ROWS ONLY) AS CMCFGIDW,
	            (select 
		            CMCFGID
	            from 
		            F3201
	            where
		            CMKCOO = RDPKCO AND
		            CMDOCO = RDDOC2 AND
		            CMDCTO = RDXCTO AND
		            CMLNID = RDLNIC AND
		            CMDOCO > 0
		            FETCH FIRST 1 ROWS ONLY) AS CMCFGIDP
            FROM 
	            F580055
	            INNER JOIN F580056
	            ON  RDZ58RID = RHZ58RID
                INNER JOIN F0006
                ON  MCMCU = RHMCU
                INNER JOIN F4102
                ON  IBITM = RDITM AND
                    IBMCU = RDMCU
	            INNER JOIN F4101
	            ON	IMITM = RDITM
            WHERE
	            {0}
	            RHZ58HST NOT IN ('0', '8') AND
                IMSTKT = 'C' AND
	            (SELECT COUNT(*) FROM F580056 WHERE F580056.RDZ58RID = RHZ58RID) = 1	
            FOR READ ONLY", filterString);
            return cmd;
        }

        public Racks GetHVMTORacks(BusinessSystemSession session, List<string> plantList, int agingDays, List<string> statusList)
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

                cmd = GetHVMTORacks(filters);
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
                                    RHMCU = p.Field<string>("RHMCU").Trim(),
                                    MCDL01 = p.Field<string>("MCDL01").Trim(),
                                    RHZ58RT = p.Field<string>("RHZ58RT").Trim(),
                                    RHAN8 = p.Field<decimal>("RHAN8"),
                                    RHSHAN = p.Field<decimal>("RHSHAN"),
                                    RHZ58HST = p.Field<string>("RHZ58HST").Trim(),
                                    RHZ58RSDT = E1TimeManager.CastToDateTime(p.Field<decimal>("RHZ58RSDT")),
                                    RHLOCN = p.Field<string>("RHLOCN").Trim(),
                                    RHZ58RTWT = p.Field<decimal>("RHZ58RTWT") / 100,
                                    RHZ58MTRW = p.Field<decimal>("RHZ58MTRW") / 100,
                                    RHZ58GWTT = p.Field<decimal>("RHZ58GWTT") / 100,
                                    RHZ58BOX = p.Field<decimal>("RHZ58BOX"),
                                    RHZ58SCN = p.Field<decimal>("RHZ58SCN"),
                                    RHZ58RCD = p.Field<string>("RHZ58RCD").Trim(),
                                    RDLOTN = p.Field<string>("RDLOTN").Trim(),
                                    RDDOC2 = p.Field<decimal>("RDDOC2"),
                                    RDXCTO = p.Field<string>("RDXCTO").Trim(),
                                    RDLNIC = p.Field<decimal>("RDLNIC"),
                                    RDPKCO = p.Field<string>("RDPKCO").Trim(),
                                    RDZ58SN = p.Field<string>("RDZ58SN").Trim(),
                                    RDITM = p.Field<decimal>("RDITM"),
                                    RDZ58MDT = E1TimeManager.CastToDateTime(p.Field<decimal>("RDZ58MDT")),
                                    RDZ58PDT = E1TimeManager.CastToDateTime(p.Field<decimal>("RDZ58PDT")),
                                    RDPQOH = p.Field<decimal>("RDPQOH") / 1000,
                                    RDZ58CQTY = p.Field<decimal>("RDZ58CQTY") / 1000,
                                    RDDSC1 = p.Field<string>("RDDSC1").Trim(),
                                    RDDSC2 = p.Field<string>("RDDSC2").Trim(),
                                    CMCFGIDS = p.Field<decimal?>("CMCFGIDS"),
                                    CMCFGIDW = p.Field<decimal?>("CMCFGIDW"),
                                    CMCFGIDP = p.Field<decimal?>("CMCFGIDP"),
                                    IMDSC1 = p.Field<string>("IMDSC1").Trim(),
                                    IBSTKT = p.Field<string>("IBSTKT").Trim(),
                                    IBGLPT = p.Field<string>("IBGLPT").Trim(),
                                    IBPRP4 = p.Field<string>("IBPRP4").Trim(),
                                    IBANPL = p.Field<decimal>("IBANPL")
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
                    var rack = new Rack();
                    rack.RackId = g.Key;
                    rack.ConfigId = g.First().CMCFGIDS ?? g.First().CMCFGIDW ?? g.First().CMCFGIDP ?? 0;

                    ItemAttributes itemAtts;
                    itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, rack.ConfigId);
                    if (itemAtts.Count == 0)
                    {
                        itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, rack.ConfigId);
                    }
                    var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);

                    rack.StockSerialNum = g.First().RDZ58SN;
                    rack.CostCenter = g.First().RHMCU;
                    rack.PlantNum = g.First().RHMCU;
                    rack.PlantCode = g.First().MCDL01;
                    rack.RackType = g.First().RHZ58RT;
                    rack.Location = g.First().RHLOCN;
                    rack.HeaderStatus = g.First().RHZ58HST;
                    rack.HeaderStatusDate = g.First().RHZ58RSDT;
                    rack.ReasonCode = g.First().RHZ58RCD;
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
                    rack.ItemAttributes = itemAtts;
                    rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
                    rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                    rack.Construction = flexDescs[0];
                    rack.Dimension = flexDescs[1];
                    rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                    rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
                    var itemBranchAttributes = f4102Mgr.GetPlannerNum(session, rack.PlantNum, rack.ItemNum);
                    rack.PlannerNum = g.First().IBANPL;
                    rack.PlanningFamily = g.First().IBPRP4;
                    rack.ItemDesc = g.First().IMDSC1;
                    rack.StockType = g.First().IBSTKT;
                    rack.GlCategory = g.First().IBGLPT;
                    rack.Description1 = g.First().RDDSC1;
                    rack.Description2 = g.First().RDDSC2;
                    racks.Add(rack);
                }
                //GetItemProperties(session, racks);
                GetCustomerNames(session, racks.Where(x => x.CustomerNum > 0).ToList());
                GetReceivedCost(session, racks.Where(x => x.POrderNum > 0).ToList());
                GetReceivedDate(session, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
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
                A.RHZ58RCD,
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
                        RHZ58RCD,
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
		        A.RHZ58SCN,
                A.RHZ58RCD
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
                        rack.ReasonCode = CastDbNull.To(reader["RHZ58RCD"], String.Empty).Trim();
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
                            itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, configId);
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
                        RHZ58RCD,
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
                        RDDSC1,
                        RDDSC2,
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
                        RHZ58RCD = p.Field<string>("RHZ58RCD"),
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
                        RDDSC1 = p.Field<string>("RDDSC1"),
                        RDDSC2 = p.Field<string>("RDDSC2"),
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
                        itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, configId);
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
                    rack.ReasonCode = g.First().RHZ58RCD;
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
                    rack.Description1 = g.First().RDDSC1;
                    rack.Description2 = g.First().RDDSC2;
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

        public void GetReceivedCost(BusinessSystemSession session, List<Rack> racks)
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

        public iDB2Command GetCustomerName()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            ABALPH
            FROM
	            F0101
            WHERE
                ABAN8 = ?
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
            cmd.Parameters["AN8"].Precision = 8;
            cmd.Parameters["AN8"].Scale = 0;
            return cmd;
        }

        public void GetCustomerNames(BusinessSystemSession session, List<Rack> racks)
        {
            try
            {
                var cmd = session.DbCommands["ConfiguredRackManager.GetCustomerName"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.CustomerNum;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            rack.SoldToCustomerName = CastDbNull.To<string>(reader["ABALPH"], string.Empty).Trim();
                        }

                        reader.Close();
                    }
                    cmd.Parameters[0].Value = rack.ShipToCustomerNum;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            rack.ShipToCustomerName = CastDbNull.To<string>(reader["ABALPH"], string.Empty).Trim();
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
