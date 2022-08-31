using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BIServices.Common.Managers.Enterprise
{
    public class AgcOpenOrderManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public AgcOpenOrderManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("AgcOpenOrderManager.GetIntercompanyRelatedOrdersBySalesLine", GetIntercompanyRelatedOrdersBySalesLine());
            hash.Add("AgcOpenOrderManager.GetOutsideRelatedOrdersBySalesLine", GetOutsideRelatedOrdersBySalesLine());
            hash.Add("AgcOpenOrderManager.GetItemInfo", GetItemInfo());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
          * Stock Orders
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
            cmd.CommandText = $@"
			SELECT 
				SDMCU,
				SDAN8,
				SDSHAN,
				SOLD.ABALPH AS SDAN8_NAME,
				SHIP.ABALPH AS SDSHAN_NAME,
				SDTRDJ,
				SDDRQJ,
				SDLTTR,
				SDNXTR, 
                SDKCOO,
				SDDOCO,
				SDDCTO,
				SDLNID, 
				SDRORN,
				SDRCTO,
                WADOCO,
				WASRST,
				SDLNTY,
				IMSTKT,
                IBANPL,
				SDITM,
				SDDSC1,
				SDDSC2,
				SDVR01, 
				ATZ58CP2,
				ATZ58CP3,
				ATZ58LPR,
				ATZ58RT,
				SDUORG,
				ATZ58RKS,
				ATZ58GWTT,
                RTZ58RTWT,
				COALESCE(CP1.CPZ58GAL1, CP2.CPZ58GAL1) AS CPZ58GAL1,
				RD.RDZ58RIDS,
                RD.RHZ58GWTT,
                RD.RHZ58RTWT,
				RD.RHZ58SLDNS,
                RD.RHZ58GWTT_SLDN,
                RD.RHZ58RTWT_SLDN,
				LP.LPZ58RKN,
                LP.LPZ58GWTT,
                LP.LPTOWT,
				LP.LHGFLAG1
			FROM 
				F4211 
				INNER JOIN F580032
				ON	ATKCOO = SDKCOO AND
					ATDOCO = SDDOCO AND
					ATDCTO = SDDCTO AND
					ATLNID = SDLNID
				INNER JOIN F0101 SOLD
				ON	SOLD.ABAN8 = SDAN8
				INNER JOIN F0101 SHIP
				ON	SHIP.ABAN8 = SDSHAN
                INNER JOIN F4102
                ON  IBITM = SDITM AND
                    IBMCU = SDMCU
				INNER JOIN F4101
				ON	IMITM = SDITM
				LEFT OUTER JOIN F4801
				ON WADOCO = ATZ58WODC 
				LEFT OUTER JOIN F580061
				ON	RTMCU = SDMCU AND
					RTZ58RT = ATZ58RT AND
					RTAN8 = 0
				LEFT OUTER JOIN F580430 CP1
				ON	CP1.CPAN8 = SDAN8 AND
					CP1.CPMCU = SDMCU
				LEFT OUTER JOIN F580430 CP2
				ON	CP2.CPAN8 = SDSHAN AND
					CP2.CPMCU = SDMCU
				LEFT OUTER JOIN (
					SELECT 
						LPMCU,
						LPKCOO,
						LPDOCO,
						LPDCTO,
						LPLNID,
						SUM(LPZ58RKN) AS LPZ58RKN,
                        SUM(LPZ58GWTT) AS LPZ58GWTT,
                        SUM(LPTOWT) AS LPTOWT,
						MAX(LHGFLAG1) AS LHGFLAG1
					FROM 
						F580151 
						INNER JOIN F580150
						ON	LHLDNM = LPLDNM
					WHERE
						LHZ58LDS IN ('1','2','3','4','5') 
					GROUP BY
						LPMCU,
						LPKCOO,
						LPDOCO,
						LPDCTO,
						LPLNID						
				) LP
				ON	LP.LPMCU = SDMCU AND
					LP.LPKCOO = SDKCOO AND
					LP.LPDOCO = SDDOCO AND
					LP.LPDCTO = SDDCTO AND
					LP.LPLNID = SDLNID
				LEFT OUTER JOIN (
					SELECT 
						RDMCU,
						RDKCOO,
						RDDOCO,
						RDDCTO,
						RDLNID,
						COUNT(*) AS RDZ58RIDS,
						SUM(CASE WHEN RHZ58SLDN > 0 THEN 1 ELSE 0 END) AS RHZ58SLDNS,
                        SUM(RHZ58GWTT) AS RHZ58GWTT,
                        SUM(RHZ58RTWT) AS RHZ58RTWT,
						SUM(CASE WHEN RHZ58SLDN > 0 THEN RHZ58GWTT ELSE 0 END) AS RHZ58GWTT_SLDN,
						SUM(CASE WHEN RHZ58SLDN > 0 THEN RHZ58RTWT ELSE 0 END) AS RHZ58RTWT_SLDN
					FROM 
						F580056 
						INNER JOIN F580055
						ON	RHZ58RID = RDZ58RID
					WHERE
						RDZ58DST IN ('2','4','5','7') 
					GROUP BY
						RDMCU,
						RDKCOO,
						RDDOCO,
						RDDCTO,
						RDLNID						
				) RD
				ON	RD.RDMCU = SDMCU AND
					RD.RDKCOO = SDKCOO AND
					RD.RDDOCO = SDDOCO AND
					RD.RDDCTO = SDDCTO AND
					RD.RDLNID = SDLNID 
			WHERE 
                {filterString}
				SDNXTR <= '545' and
				SDDCTO IN ('SO','ST')
            FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetOpenOrders(BusinessSystemSession session, Dictionary<string, SqlConnection> sqlCns, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Command
                cmd = GetOpenOrders(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = reader.Get<string>("SDMCU");
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.AccountManager = reader.Get<string>("CPZ58GAL1");
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["SDAN8_NAME"], string.Empty).Trim();
						line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
						line.CustomerShipName = CastDbNull.To(reader["SDSHAN_NAME"], string.Empty).Trim();
						line.OrderDate = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDTRDJ"));
                        line.RequestedDate = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDDRQJ"));
                        line.LineType = reader.Get<string>("SDLNTY");
                        line.OrderCompany = reader.Get<string>("SDKCOO");
                        line.OrderNum = reader.Get<decimal>("SDDOCO");
                        line.OrderType = reader.Get<string>("SDDCTO");
                        line.OrderLineNum = reader.Get<decimal>("SDLNID") / 1000;
                        line.SOrderDesc = $"{line.OrderNum}-{line.OrderType}-{line.OrderLineNum:0.000}";
                        line.WorkOrderNum = reader.Get<decimal>("WADOCO");
                        line.WorkOrderStatus = reader.Get<string>("WASRST");
                        line.WOrderDesc = line.WorkOrderNum > 0 ? $"{line.WorkOrderNum}-WO" : "";
                        line.LastStatus = reader.Get<string>("SDLTTR");
                        line.NextStatus = reader.Get<string>("SDNXTR");
                        line.ItemNum = reader.Get<decimal>("SDITM");
                        line.StockType = CastDbNull.To(reader["IMSTKT"], string.Empty).Trim();
                        line.Construction = reader.Get<string>("SDDSC1");
                        line.Dimension = reader.Get<string>("SDDSC2");
                        line.CustomerPO = reader.Get<string>("SDVR01");
                        line.CustomerPart1 = reader.Get<string>("ATZ58CP2");
                        line.CustomerPart2 = reader.Get<string>("ATZ58CP3");
                        line.CarrierFlag = reader.Get<string>("LHGFLAG1");
                        line.OrderQty = reader.Get<decimal>("SDUORG") / 1000;
                        line.LitesPerRack = reader.Get<decimal>("ATZ58LPR");

						line.OrderedRacksQty = reader.Get<int>("ATZ58RKS");
                        line.OrderedGlassWeight = reader.Get<decimal>("ATZ58GWTT") / 100;
                        line.OrderedTareWeight = reader.Get<decimal>("RTZ58RTWT") / 100;

                        line.ScheduledRacksQty = 0;
                        line.ScheduledGlassWeight = 0;
                        line.ScheduledTareWeight = 0;

                        line.OnHandToOrderRacksQty = reader.Get<int>("RDZ58RIDS");
                        line.OnHandToOrderGlassWeight = reader.Get<decimal>("RHZ58GWTT") / 100;
                        line.OnHandToOrderTareWeight = reader.Get<decimal>("RHZ58RTWT") / 100;

                        line.FGReleasedToOrderRacksQty = 0;
                        line.FGReleasedToOrderGlassWeight = 0;
                        line.FGReleasedToOrderTareWeight = 0;

                        line.LoadReleasedToOrderRacksQty = (int)reader.Get<decimal>("LPZ58RKN");
                        line.LoadReleasedToOrderGlassWeight = reader.Get<decimal>("LPZ58GWTT") / 100;
                        line.LoadReleasedToOrderTareWeight = (reader.Get<decimal>("LPTOWT") / 10000)  - (reader.Get<decimal>("LPZ58GWTT") / 100);

                        line.OnHandToItemRacksQty = 0;
                        line.OnHandToItemGlassWeight = 0;
                        line.OnHandToItemTareWeight = 0;

                        line.FGReleasedToItemRacksQty = 0;
                        line.FGReleasedToItemGlassWeight = 0;
                        line.FGReleasedToItemTareWeight = 0;

                        line.LoadReleasedToItemRacksQty = 0;
                        line.LoadReleasedToItemGlassWeight = 0;
                        line.LoadReleasedToItemTareWeight = 0;

                        line.PlannerNum = reader.Get<decimal>("IBANPL");
                        lines.Add(line);
                    }
                    reader.Close();

                    GetItemInfo(session, lines.Where(x => x.StockType == "S" && x.PlannerNum != 10501718).ToList());
                    GetIntercompanyRelatedOrders(session, lines.Where(x => x.StockType == "C" && x.OrderType == "ST").ToList());
                    GetOutsideRelatedOrders(session, lines.Where(x => x.StockType == "C" && x.OrderType == "SO").ToList());
                    GetFGWebData(sqlCns["PLANT.FGWEB"], lines);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        public static iDB2Command GetIntercompanyRelatedOrdersBySalesLine()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
           SELECT 
	            SDMCU,
				SDKCOO,
	            SDDOCO,
	            SDDCTO,
	            SDLNID,
	            SDLTTR,
	            SDNXTR,
				PDKCOO,
				PDMCU,
				PDDOCO,
				PDDCTO,
				PDLNID,
	            PDLTTR,
	            PDNXTR,
	            ROEDTR,
	            ROZ58L1CLN,
				WAMCU,
	            WADOCO,
	            WASRST
            from 
	            F4211 
	            INNER JOIN F580032
	            ON	SDKCOO = ATKCOO AND
		            SDDOCO = ATDOCO AND
		            SDDCTO = ATDCTO AND
		            SDLNID = ATLNID
				INNER JOIN F580400
	            ON	RORKCO = ATZ58POCO AND
		            RODOC1 = ATZ58PODC AND
		            RORCTO = ATZ58PODT AND
		            RORLLN = ATZ58POLI
	            INNER JOIN F4311
	            ON	PDKCOO = RORKCO AND
		            PDDOCO = RODOC1 AND
		            PDDCTO = RORCTO AND
		            PDLNID = RORLLN
	            INNER JOIN F4801
	            ON	WADOCO = ATZ58WODC AND
                    ATZ58WODC > 0
            WHERE 
				SDDOCO = ? AND
                SDDCTO = ? AND
                SDKCOO = ? AND
                SDLNID = ? 
			FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("KCOO", iDB2DbType.iDB2Graphic, 5));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public static iDB2Command GetOutsideRelatedOrdersBySalesLine()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
           SELECT 
				SDT.SDMCU,
                SDT.SDKCOO,
				SDT.SDDOCO,
				SDT.SDDCTO,
				SDT.SDLNID,
				SDT.SDLTTR,
				SDT.SDNXTR,
				PDKCOO,
				PDMCU,
				PDDOCO,
				PDDCTO,
				PDLNID,
	            PDLTTR,
	            PDNXTR,
	            ROEDTR,
	            ROZ58L1CLN,
				WAMCU,
				WADOCO,
				WASRST
            from 
	            F4211 SDO
				INNER JOIN F580400
	            ON	RODOCO = SDO.SDDOCO AND
		            RODCTO = SDO.SDDCTO AND
		            ROLNID = SDO.SDLNID 
	            INNER JOIN F4311
	            ON	PDKCOO = RORKCO AND
		            PDDOCO = RODOC1 AND
		            PDDCTO = RORCTO AND
		            PDLNID = RORLLN
				INNER JOIN F580032
				ON	ATZ58PODC = RODOC1 AND
					ATZ58PODT = RORCTO AND
					ATZ58POLI = RORLLN AND
                    ATZ58PODC > 0
				INNER JOIN F4211 SDT
				ON  SDT.SDDOCO = ATDOCO AND
					SDT.SDDCTO = ATDCTO AND
					SDT.SDLNID = ATLNID
				INNER JOIN F4801
				ON	WADOCO = ATZ58WODC AND
                    ATZ58WODC > 0
            WHERE 
				SDO.SDDOCO = ? AND
                SDO.SDDCTO = ? AND
                SDO.SDKCOO = ? AND
                SDO.SDLNID = ? 
			FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("KCOO", iDB2DbType.iDB2Graphic, 5));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetIntercompanyRelatedOrders(BusinessSystemSession session, List<SalesOrderLine> lines)
        {
            try
            {
                var cmd = session.DbCommands["AgcOpenOrderManager.GetIntercompanyRelatedOrdersBySalesLine"];
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                foreach (var line in lines)
                {
                    cmd.Parameters["DOCO"].Value = line.OrderNum;
                    cmd.Parameters["DCTO"].Value = line.OrderType;
                    cmd.Parameters["KCOO"].Value = line.OrderCompany;
                    cmd.Parameters["LNID"].Value = line.OrderLineNum * 1000;
                    _logger.Info(Logging.DbCmdString(cmd));

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        var anons = reader.Cast<IDataRecord>().Select(r => new
                        {
                            SDMCU = ((string)r["SDMCU"]).Trim(),
                            SDMCU_CODE = PlantManager.GetPlant(((string)r["SDMCU"]).Trim()).PlantCode,
                            SDDOCO = (decimal)r["SDDOCO"],
                            SDDCTO = (string)r["SDDCTO"],
                            SDLNID = ((decimal)r["SDLNID"]) / 1000,
                            SDLTTR = (string)r["SDLTTR"],
                            SDNXTR = (string)r["SDNXTR"],
                            PDMCU = ((string)r["PDMCU"]).Trim(),
                            PDMCU_CODE = PlantManager.GetPlant(((string)r["PDMCU"]).Trim()).PlantCode,
                            PDDOCO = (decimal)r["PDDOCO"],
                            PDDCTO = (string)r["PDDCTO"],
                            PDLNID = ((decimal)r["PDLNID"]) / 1000,
                            PDLTTR = (string)r["PDLTTR"],
                            PDNXTR = (string)r["PDNXTR"],
                            ROEDTR = (string)r["ROEDTR"],
                            WAMCU = (string)r["WAMCU"],
                            WAMCU_CODE = PlantManager.GetPlant(((string)r["WAMCU"]).Trim()).PlantCode,
                            WADOCO = (decimal)r["WADOCO"],
                            WASRST = (string)r["WASRST"]
                        }).ToList();
                        reader.Close();

                        var sb = new StringBuilder();
                        if (anons.Count > 0)
                        {
                            sb.Append($"{anons[0].PDMCU_CODE} {anons[0].PDDOCO}-{anons[0].PDDCTO}-{anons[0].PDLNID}[{(anons[0].PDLTTR == "980" ? anons[0].PDLTTR : anons[0].PDNXTR)}] / {anons[0].SDMCU_CODE} {anons[0].SDDOCO}-{anons[0].SDDCTO}-{anons[0].SDLNID}[{(anons[0].SDLTTR == "980" ? anons[0].SDLTTR : anons[0].SDNXTR)}] {anons[0].WADOCO}-WO[{anons[0].WASRST}] ");
                        }
                        else if (line.WorkOrderNum > 0)
                        {
                            sb.Append($"{line.PlantCode} {line.WorkOrderNum}-WO[{line.WorkOrderStatus}]");
                        }
                        line.RelatedOrderDesc = sb.ToString().Trim();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void GetOutsideRelatedOrders(BusinessSystemSession session, List<SalesOrderLine> lines)
        {
            try
            {
                var cmd = session.DbCommands["AgcOpenOrderManager.GetOutsideRelatedOrdersBySalesLine"];
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                foreach (var line in lines)
                {
                    cmd.Parameters["DOCO"].Value = line.OrderNum;
                    cmd.Parameters["DCTO"].Value = line.OrderType;
                    cmd.Parameters["KCOO"].Value = line.OrderCompany;
                    cmd.Parameters["LNID"].Value = line.OrderLineNum * 1000;
                    _logger.Info(Logging.DbCmdString(cmd));

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        var anons = reader.Cast<IDataRecord>().Select(r => new
                        {
                            SDMCU = ((string)r["SDMCU"]).Trim(),
                            SDMCU_CODE = PlantManager.GetPlant(((string)r["SDMCU"]).Trim()).PlantCode,
                            SDDOCO = (decimal)r["SDDOCO"],
                            SDDCTO = (string)r["SDDCTO"],
                            SDLNID = ((decimal)r["SDLNID"]) / 1000,
                            SDLTTR = (string)r["SDLTTR"],
                            SDNXTR = (string)r["SDNXTR"],
                            PDMCU = ((string)r["PDMCU"]).Trim(),
                            PDMCU_CODE = PlantManager.GetPlant(((string)r["PDMCU"]).Trim()).PlantCode,
                            PDDOCO = (decimal)r["PDDOCO"],
                            PDDCTO = (string)r["PDDCTO"],
                            PDLNID = ((decimal)r["PDLNID"]) / 1000,
                            PDLTTR = (string)r["PDLTTR"],
                            PDNXTR = (string)r["PDNXTR"],
                            ROEDTR = (string)r["ROEDTR"],
                            WAMCU = (string)r["WAMCU"],
                            WAMCU_CODE = PlantManager.GetPlant(((string)r["WAMCU"]).Trim()).PlantCode,
                            WADOCO = (decimal)r["WADOCO"],
                            WASRST = (string)r["WASRST"]
                        }).ToList();
                        reader.Close();

                        var sb = new StringBuilder();
                        if (anons.Count > 0)
                        {
                            sb.Append($"{anons[0].PDMCU_CODE} {anons[0].PDDOCO}-{anons[0].PDDCTO}-{anons[0].PDLNID}[{(anons[0].PDLTTR == "980" ? anons[0].PDLTTR : anons[0].PDNXTR)}] / {anons[0].SDMCU_CODE} {anons[0].SDDOCO}-{anons[0].SDDCTO}-{anons[0].SDLNID}[{(anons[0].SDLTTR == "980" ? anons[0].SDLTTR : anons[0].SDNXTR)}] {anons[0].WADOCO}-WO[{anons[0].WASRST}] ");
                        }
                        else if (line.WorkOrderNum > 0)
                        {
                            sb.Append($"{line.PlantCode} {line.WorkOrderNum}-WO[{line.WorkOrderStatus}]");
                        }
                        line.RelatedOrderDesc = sb.ToString().Trim();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static iDB2Command GetItemInfo()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            IBMCU,
	            IBITM, 
	            SUM(CASE WHEN RDZ58RID > 0 THEN 1 ELSE 0 END) AS PRODUCED,
	            SUM(CASE WHEN RHZ58SLDN > 0 THEN 1 ELSE 0 END) AS PICKED,
	            COALESCE(LP.LPZ58RKN, 0) AS LOADED
            FROM 
	            F4102
	            LEFT OUTER JOIN F580056 	
		            INNER JOIN F580055
		            ON	RHZ58RID = RDZ58RID
	            ON	RDMCU = IBMCU AND
		            RDITM = IBITM AND
		            RDLOTN = '' AND
		            RDZ58DST IN ('2','4','5','7')
	            LEFT OUTER JOIN (
		            SELECT 
			            LPMCU, 
			            LPITM, 
			            SUM(LPZ58RKN) AS LPZ58RKN 
		            FROM 
			            F580151 
			            INNER JOIN F580150 
			            ON	LHLDNM = LPLDNM 
		            WHERE 
			            LHZ58LDS NOT IN ('5') 
		            GROUP BY 
			            LPMCU, 
			            LPITM) AS LP
	            ON	LP.LPMCU = RDMCU AND 
		            LP.LPITM = RDITM
            WHERE 
	            IBMCU IN ('          47','          48','          49','          50')  AND
	            IBSTKT IN ('S','U','P','O')
            GROUP BY 
	            IBMCU,
	            IBITM,
	            LP.LPZ58RKN
            HAVING
	            SUM(CASE WHEN RDZ58RID > 0 THEN 1 ELSE 0 END) > 0 OR
	            LP.LPZ58RKN > 0
            FOR READ ONLY";
            return cmd;
        }

        public void GetItemInfo(BusinessSystemSession session, List<SalesOrderLine> lines)
        {
            var dict = new Dictionary<decimal, object>();
            iDB2Command cmd;
            try
            {
                cmd = session.DbCommands["AgcOpenOrderManager.GetItemInfo"];
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));
                _logger.Info("Reader Opened");
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var anons = reader.Cast<IDataRecord>().Select(r => new
                    {
                        MCU = ((String)r["IBMCU"]).Trim(),
                        MCU_CODE = PlantManager.GetPlant(((String)r["IBMCU"]).Trim()).PlantCode,
                        ITM = (Decimal)r["IBITM"],
                        PRODUCED = (Int32)r["PRODUCED"],
                        PICKED = (Int32)r["PICKED"],
                        LOADED = (Decimal)r["LOADED"]
                    }).ToList();
                    reader.Close();
                    _logger.Info("Reader Closed");

                    foreach (var line in lines)
                    {
                        var items = anons.Where(x => x.ITM == line.ItemNum);
                        line.OnHandToItemRacksQty = items.Sum(x => x.PRODUCED);
                        line.FGReleasedToItemRacksQty = 0;
                        line.LoadReleasedToItemRacksQty = (int)items.Sum(x => x.LOADED);
                        var sb = new StringBuilder();
                        //sb.AppendLine("");
                        foreach (var item in items)
                        {
                            sb.Append($"{item.MCU_CODE} [{item.PRODUCED}/{item.LOADED}] ");
                        }
                        line.ItemInventoryDesc = sb.ToString().Trim();
                    }
                    _logger.Info("Collection Closed");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public static SqlCommand GetFGWebData()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                salesOrder,
                salesOrderLine,
                ISNULL(item, 0) AS item,
                ISNULL(isStock, 0) AS isStock,
                ISNULL(racksOnOrder, 0) AS racksOnOrder,
                ISNULL(racksReleased, 0) AS racksReleased,
                ISNULL(racksOnLoads, 0) AS racksOnLoads,
                ISNULL(racksProduced, 0) AS racksProduced,
                ISNULL(racksScheduled, 0) AS racksScheduled
            FROM
                tblSomgrOrders WITH (NOLOCK)
            WHERE
                plant IN ( 47, 48, 49, 50 )";
            return cmd;
        }

        public void GetFGWebData(SqlConnection cn, List<SalesOrderLine> lines)
        {
            var dict = new Dictionary<decimal, object>();
            SqlCommand cmd;
            try
            {
                cmd = GetFGWebData();
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var anons = reader.Cast<IDataRecord>().Select(r => new
                    {
                        salesOrder = (int)r["salesOrder"],
                        salesOrderLine = (decimal)r["salesOrderLine"],
                        item = (int)r["item"],
                        isStock = (int)r["isStock"],
                        racksOnOrder = (int)r["racksOnOrder"],
                        racksReleased = (int)r["racksReleased"],
                        racksOnLoads = (int)r["racksOnLoads"],
                        racksScheduled = (int)r["racksScheduled"]
                    }).ToList();
                    reader.Close();
                    _logger.Info("Reader Closed");

                    foreach (var line in lines)
                    {
                        var salesLines = anons.Where(x => x.salesOrder == line.OrderNum && x.salesOrderLine == line.OrderLineNum).ToList();
                        if (salesLines.Any()) {
                            line.FGReleasedToOrderRacksQty = salesLines.Sum(x => x.racksReleased);
                            line.ScheduledRacksQty = salesLines.Max(x => x.racksScheduled);
                        }

                        var items = anons.Where(x => x.item == line.ItemNum).ToList();
                        if (items.Any())
                        {
                            line.FGReleasedToItemRacksQty = items.Sum(x => x.racksReleased);
                        }
                    }
                    _logger.Info("Collection Closed");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}


