using BIServices.Common.Managers.E1;
using BIServices.Common.Types.Configuration;
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
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BIServices.Common.Managers.Enterprise
{
    public class AgcOpenOrderManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public AgcOpenOrderManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("AgcOpenOrderManager.GetRelatedRacksBySalesLine", GetRelatedRacksBySalesLine());
            hash.Add("AgcOpenOrderManager.GetRelatedRacksByItemNum", GetRelatedRacksByItemNum());
            hash.Add("AgcOpenOrderManager.GetIntercompanyRelatedOrdersBySalesLine", GetIntercompanyRelatedOrdersBySalesLine());
            hash.Add("AgcOpenOrderManager.GetOutsideRelatedOrdersBySalesLine", GetOutsideRelatedOrdersBySalesLine());
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
				S7Z58L1RG,
				COALESCE(CP1.CPZ58GAL1, CP2.CPZ58GAL1) AS CPZ58GAL1,
				(	SELECT 
						SUM(RTZ58RTWT) AS RTZ58RTWT
					FROM 
						F580061
					WHERE 
						RTZ58RT = ATZ58RT AND
							RTMCU = SDMCU AND
							RTAN8 = 0 and
							RTSHAN = 0
					) AS ATZ58RTWT,
				(SELECT 
					MAX(LHGFLAG1) AS LHGFLAG1 
				FROM 
					F580151 
					INNER JOIN F580150
					ON	LHLDNM = LPLDNM
				WHERE
					LPKCOO = SDKCOO AND
					LPDOCO = SDDOCO AND
					LPDCTO = SDDCTO AND
					LPLNID = SDLNID) AS GFLAG1
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
				INNER JOIN F4101
				ON	IMITM = SDITM
				LEFT OUTER JOIN F4801
				ON WADOCO = ATZ58WODC 
				LEFT OUTER JOIN F580021
				ON S7DOCO = WADOCO
				LEFT OUTER JOIN F580430 CP1
				ON	CP1.CPAN8 = SDAN8 AND
					CP1.CPMCU = SDMCU
				LEFT OUTER JOIN F580430 CP2
				ON	CP2.CPAN8 = SDSHAN AND
					CP2.CPMCU = SDMCU
			WHERE 
                {filterString}
				SDNXTR <= '545' and
				SDDCTO IN ('SO','ST')
            FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetOpenOrders(BusinessSystemSession session, string plants)
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
                        line.WorkOrderNum = reader.Get<decimal>("WADOCO");
                        //line.WorkOrderNum = reader.Get<string>("SDRCTO") == "WO" ? Utilities.Data.ParseStringToDecimal(reader.Get<string>("SDRORN")) : 0;
                        line.WorkOrderStatus = reader.Get<string>("WASRST");
                        line.LastStatus = reader.Get<string>("SDLTTR");
                        line.NextStatus = reader.Get<string>("SDNXTR");
                        line.ItemNum = reader.Get<decimal>("SDITM");
                        line.StockType = CastDbNull.To(reader["IMSTKT"], string.Empty).Trim();
                        line.Construction = reader.Get<string>("SDDSC1");
                        line.Dimension = reader.Get<string>("SDDSC2");
                        line.CustomerPO = reader.Get<string>("SDVR01");
                        line.CustomerPart1 = reader.Get<string>("ATZ58CP2");
                        line.CustomerPart2 = reader.Get<string>("ATZ58CP3");
                        line.CarrierFlag = reader.Get<string>("GFLAG1");
                        line.OrderRouting = reader.Get<string>("S7Z58L1RG");
                        line.OrderQty = reader.Get<decimal>("SDUORG") / 1000;
                        line.LitesPerRack = reader.Get<decimal>("ATZ58LPR");
						line.OrderedRacksQty = reader.Get<int>("ATZ58RKS");
                        line.OrderedRacksGlassWeight = reader.Get<decimal>("ATZ58GWTT") / 100;
                        line.OrderedRacksTareWeight = reader.Get<decimal>("ATZ58RTWT") / 100;
                        lines.Add(line);
                    }
                    reader.Close();

                    foreach (var line in lines)
                    {
                        GetRelatedRacksBySalesLine(session, line);
                        GetRelatedOrdersBySalesLine(session, line);
                        if (line.StockType != "C")
                        {
                            GetRelatedRacksByItemNum(session, line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        /*********************************************************************************
          * Racks by Sales Line
          *******************************************************************************/
        public static iDB2Command GetRelatedRacksBySalesLine()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
			SELECT 
				RHMCU,
				RHZ58RID,
				RHZ58HST,
				RHZ58RT,
				RHZ58GWTT,
				RHZ58RTWT,
				RDITM,
				RDZ58SN,
				RDZ58DST,
				RHPSN,
				RHZ58SLDN,
				RDDOCO,
				RDDCTO,
				RDLNID
			FROM 
				F580056 
				INNER JOIN F580055
				ON	RHZ58RID = RDZ58RID
			WHERE 
				RDDOCO = ? AND
                RDDCTO = ? AND
                RDKCOO = ? AND
                RDLNID = ? 
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

        public static iDB2Command GetRelatedRacksByItemNum()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
			SELECT 
				RHMCU,
				RHZ58RID,
				RHZ58HST,
				RHZ58RT,
				RHZ58GWTT,
				RHZ58RTWT,
                RHLOCN,
				RDITM,
				RDZ58SN,
				RDZ58DST,
				RHPSN,
				RHZ58SLDN,
				RDDOCO,
				RDDCTO,
				RDLNID
			FROM 
				F580056 
				INNER JOIN F580055
				ON	RHZ58RID = RDZ58RID
			WHERE 
                RDPQOH > 0 AND
                RHZ58HST NOT IN ('0','8') AND
				RDITM = ? AND
                RDMCU IN ('          47','          48','          49','          50') AND
                NOT ( RDDOCO = ? AND RDDCTO = ? AND RDLNID = ? )
			FOR READ ONLY";

            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetRelatedRacksBySalesLine (BusinessSystemSession session, SalesOrderLine line)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {            
                cmd = session.DbCommands["AgcOpenOrderManager.GetRelatedRacksBySalesLine"];
                cmd.Parameters["DOCO"].Value = line.OrderNum;
                cmd.Parameters["DCTO"].Value = line.OrderType;
                cmd.Parameters["KCOO"].Value = line.OrderCompany;
                cmd.Parameters["LNID"].Value = line.OrderLineNum * 1000;
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                var ds = new DataSet();
                new iDB2DataAdapter(cmd).Fill(ds);

                var rows = ds.Tables[0].AsEnumerable();
                var anons = rows
                  .Select(r => new {
                      RHMCU = ((string)r["RHMCU"]).Trim(),
                      RHZ58RID = (decimal)r["RHZ58RID"],
                      RHZ58GWTT = (decimal)r["RHZ58GWTT"] / 100,
                      RHZ58RTWT = (decimal)r["RHZ58RTWT"] / 100,
                      RHZ58SLDN = (decimal)r["RHZ58SLDN"],
                      RHZ58HST = (string)r["RHZ58HST"],
                      RDZ58DST = (string)r["RDZ58DST"]
                  })
                  .ToList();

                line.OnHandRacksQty = anons.Count;
                line.OnHandRacksGlassWeight = anons.Sum(x => x.RHZ58GWTT);
                line.OnHandRacksTareWeight = anons.Sum(x => x.RHZ58RTWT);
                line.ReleasedRacksQty = anons.Count(x => x.RHZ58SLDN > 0);
                line.AllocatedRacksQty = anons.Count;
                line.AllocatedRacksGlassWeight = anons.Sum(x => x.RHZ58GWTT);
                line.AllocatedRacksTareWeight = anons.Sum(x => x.RHZ58RTWT);

                var sb = new StringBuilder();
                sb.AppendLine("");
                if (anons.Any())
                {
                    sb.Append($"{line.PlantCode}:[{anons.Count}/{anons.Count(x => x.RHZ58SLDN > 0)}] ");
                }
                line.OrderInventoryString = sb.ToString().Trim();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void GetRelatedRacksByItemNum(BusinessSystemSession session, SalesOrderLine line)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {
                cmd = session.DbCommands["AgcOpenOrderManager.GetRelatedRacksByItemNum"];
                cmd.Parameters["ITM"].Value = line.ItemNum;
                cmd.Parameters["DOCO"].Value = line.OrderNum;
                cmd.Parameters["DCTO"].Value = line.OrderType;
                cmd.Parameters["LNID"].Value = line.OrderLineNum * 1000;
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                var ds = new DataSet();
                new iDB2DataAdapter(cmd).Fill(ds);

                var rows = ds.Tables[0].AsEnumerable(); 
                var anons = rows
                  .Select(r => new { 
                      RHMCU = ((string)r["RHMCU"]).Trim(),
                      RHMCU_CODE = PlantManager.GetPlant(((string)r["RHMCU"]).Trim()).PlantCode,
                      RHZ58RID = (decimal)r["RHZ58RID"], 
                      RHZ58GWTT = (decimal)r["RHZ58GWTT"]/100, 
                      RHZ58SLDN = (decimal)r["RHZ58SLDN"],
                      RHZ58HST = (string)r["RHZ58HST"],
                      RHLOCN = (string)r["RHLOCN"],
                      RDZ58DST = (string)r["RDZ58DST"]
                  })
                  .ToList();

                var abcgInventory = anons.Where(x => x.RHMCU == "50");
                //line.ABCG_OnHandQty = abcgInventory.Count(x => x.RDZ58DST != "1");
                //line.ABCG_OnHandGlassWeight = abcgInventory.Sum(x => x.RHZ58GWTT);
                //line.ABCG_OnHandTareWeight = abcgInventory.Sum(x => x.RHZ58GWTT);
                //line.ABCG_ReleasedQty = abcgInventory.Count(x => x.RHZ58SLDN > 0);
                //line.ABCG_ReleasedGlassWeight = abcgInventory.Where(x => x.RHZ58SLDN > 0).Sum(x => x.RHZ58GWTT);
                //line.ABCG_ReleasedTareWeight = abcgInventory.Where(x => x.RHZ58SLDN > 0).Sum(x => x.RHZ58GWTT);

                var glcgInventory = anons.Where(x => x.RHMCU == "48");
                //line.GLCG_OnHandQty = glcgInventory.Count(x => x.RDZ58DST != "1");
                //line.GLCG_OnHandGlassWeight = glcgInventory.Sum(x => x.RHZ58GWTT);
                //line.GLCG_OnHandTareWeight = glcgInventory.Sum(x => x.RHZ58GWTT);
                //line.GLCG_ReleasedQty = glcgInventory.Count(x => x.RHZ58SLDN > 0);
                //line.GLCG_ReleasedGlassWeight = glcgInventory.Where(x => x.RHZ58SLDN > 0).Sum(x => x.RHZ58GWTT);
                //line.GLCG_ReleasedTareWeight = glcgInventory.Where(x => x.RHZ58SLDN > 0).Sum(x => x.RHZ58GWTT);

                var sellerInventory = anons.Where(x => x.RHMCU == line.PlantNum);
                //line.XXXX_OnHandQty = salePlantInventory.Count(x => x.RDZ58DST != "1");
                //line.XXXX_OnHandGlassWeight = salePlantInventory.Sum(x => x.RHZ58GWTT);
                //line.XXXX_OnHandTareWeight = salePlantInventory.Sum(x => x.RHZ58GWTT);
                //line.XXXX_ReleasedQty = salePlantInventory.Count(x => x.RHZ58SLDN > 0);
                //line.XXXX_ReleasedGlassWeight = salePlantInventory.Where(x => x.RHZ58SLDN > 0).Sum(x => x.RHZ58GWTT);
                //line.XXXX_ReleasedTareWeight = salePlantInventory.Where(x => x.RHZ58SLDN > 0).Sum(x => x.RHZ58GWTT);

                var sb = new StringBuilder();
                sb.AppendLine("");
                if (abcgInventory.Any())
                {
                    sb.Append($"ABCG [{abcgInventory.Count(x => x.RHZ58HST == "4")}/{abcgInventory.Count(x => x.RHZ58SLDN > 0)}] ");
                    //foreach (var group in abcgInventory.GroupBy(x => x.RHLOCN))
                    //{
                    //    sb.Append($"{group.Key.Trim()}[{group.Count(x => x.RHZ58HST == "4")}/{group.Count(x => x.RHZ58SLDN > 0)}] ");
                    //}
                }
                if (glcgInventory.Any())
                {
                    sb.Append($"GLCG [{glcgInventory.Count(x => x.RHZ58HST == "4")}/{glcgInventory.Count(x => x.RHZ58SLDN > 0)}] ");
                    //foreach (var group in glcgInventory.GroupBy(x => x.RHLOCN))
                    //{
                    //    sb.Append($"{group.Key.Trim()}[{group.Count(x => x.RHZ58HST == "4")}/{group.Count(x => x.RHZ58SLDN > 0)}] ");
                    //}
                }
                if (sellerInventory.Any())
                {
                    sb.Append($"{line.PlantCode} [{sellerInventory.Count(x => x.RHZ58HST == "4")}/{sellerInventory.Count(x => x.RHZ58SLDN > 0)}] ");
                    //foreach (var group in sellerInventory.GroupBy(x => x.RHLOCN))
                    //{
                    //    sb.Append($"{group.Key.Trim()}[{group.Count(x => x.RHZ58HST == "4")}/{group.Count(x => x.RHZ58SLDN > 0)}] ");
                    //}
                }
                line.ItemInventoryString = sb.ToString().Trim();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
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
	            INNER  JOIN F580032
	            ON	ATKCOO = SDKCOO AND
		            ATDOCO = SDDOCO AND
		            ATDCTO = SDDCTO AND
		            ATLNID = SDLNID
				INNER  JOIN F580400
	            ON	ROKCOO = SDKCOO  AND
		            RODOCO = SDDOCO AND
		            RODCTO = SDDCTO AND
		            ROLNID = SDLNID
	            INNER  JOIN F4311
	            ON	PDKCOO = RORKCO  AND
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

        public void GetRelatedOrdersBySalesLine(BusinessSystemSession session, SalesOrderLine line)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {
                var cmdName = line.OrderType == "ST" ? "AgcOpenOrderManager.GetIntercompanyRelatedOrdersBySalesLine" : "AgcOpenOrderManager.GetOutsideRelatedOrdersBySalesLine";
                cmd = session.DbCommands[cmdName];
                cmd.Parameters["DOCO"].Value = line.OrderNum;
                cmd.Parameters["DCTO"].Value = line.OrderType;
                cmd.Parameters["KCOO"].Value = line.OrderCompany;
                cmd.Parameters["LNID"].Value = line.OrderLineNum * 1000;
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                var ds = new DataSet();
                new iDB2DataAdapter(cmd).Fill(ds);

                var rows = ds.Tables[0].AsEnumerable();
                var anons = rows
                  .Select(r => new
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
                  })
                  .ToList();

                var sb = new StringBuilder();
                sb.AppendLine("");
                if (anons.Count > 0)
                {
                    //sb.Append($"{anons[0].ROMCU_CODE}:{anons[0].RODOC1}-{anons[0].RORCTO}-{anons[0].RORLLN} [{anons[0].PDLTTR}/{anons[0].PDNXTR}] / {anons[0].ROMCU2_CODE}:{anons[0].ATDOCO}-{anons[0].ATDCTO}-{anons[0].ATLNID} [{anons[0].SDLTTR}/{anons[0].SDNXTR}] / {anons[0].ROMCU2_CODE}:{anons[0].ATDOCO}-WO [{anons[0].WASRST}] ");
                    sb.Append($"{anons[0].PDMCU_CODE} {anons[0].PDDOCO}-{anons[0].PDDCTO}-{anons[0].PDLNID}[{(anons[0].PDLTTR == "980" ? anons[0].PDLTTR : anons[0].PDNXTR)}] / {anons[0].SDMCU_CODE} {anons[0].SDDOCO}-{anons[0].SDDCTO}-{anons[0].SDLNID}[{(anons[0].SDLTTR == "980" ? anons[0].SDLTTR : anons[0].SDNXTR)}] / {anons[0].WAMCU_CODE} {anons[0].WADOCO}-WO[{anons[0].WASRST}] ");
                }
                else if (line.WorkOrderNum > 0)
                {
                    sb.Append($"{line.PlantCode} {line.WorkOrderNum}-WO[{line.WorkOrderStatus}]");
                }
                line.RelatedOrderString = sb.ToString().Trim();


            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Stock Orders - DEPRECATED
          *******************************************************************************/
        public static iDB2Command GetOpenOrders_VER1(List<string> filters)
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
				COALESCE(CP1.CPZ58GAL1, CP2.CPZ58GAL1) AS CPZ58GAL1,
				SDMCU,
				SDAN8,
				SDSHAN,
				SOLD.ABALPH,
				SHIP.ABALPH AS ABALPHSH,
				SDTRDJ,
				SDDRQJ,
				SDLTTR,
				SDNXTR, 
				SDDOCO,
				SDDCTO,
				SDLNID, 
				SDITM,
				SDLNTY,
				IMSTKT,
				SDDSC1,
				SDDSC2,
				SDVR01, 
				ATZ58CP2,
				ATZ58CP3,
				SDUORG,

				ATZ58LPR,
				ATZ58RT,
				ATZ58RKS,
				ATZ58GWTT,
				(	SELECT 
						SUM(RTZ58RTWT) AS RTZ58RTWT
					FROM 
						F580061
					WHERE 
						RTZ58RT = ATZ58RT AND
							RTMCU = SDMCU AND
							RTAN8 = 0 and
							RTSHAN = 0
					) AS ATZ58RTWT,

				(	SELECT 
						COUNT(DISTINCT RHZ58RID) AS RHZ58RKS
					FROM 
						F580056
						INNER JOIN F580055
						ON RHZ58RID = RDZ58RID
					WHERE 
						RDDOCO = SDDOCO AND
						RDDCTO = SDDCTO AND
						RDLNID = SDLNID AND
						RDMCU = SDMCU
					) AS RHZ58RKS,
				(	SELECT 
						SUM(RHZ58GWTT) AS RHZ58GWTT
					FROM 
						F580056
						INNER JOIN F580055
						ON RHZ58RID = RDZ58RID
					WHERE 
						RDDOCO = SDDOCO AND
						RDDCTO = SDDCTO AND
						RDLNID = SDLNID AND
						RDMCU = SDMCU
					) AS RHZ58GWTT,
				(	SELECT 
						SUM(RHZ58RTWT) AS RHZ58RTWT
					FROM 
						F580056
						INNER JOIN F580055
						ON RHZ58RID = RDZ58RID
					WHERE 
						RDDOCO = SDDOCO AND
						RDDCTO = SDDCTO AND
						RDLNID = SDLNID AND
						RDMCU = SDMCU
					) AS RHZ58RTWT,

				(	SELECT 
						COUNT(*) 
					FROM 
						F580056 
					WHERE 
						RDLOTN = ' AND
						RDITM = SDITM AND 
						RDMCU = SDMCU AND 
						RDZ58DST IN ('2','4','5')
					) AS XXSRKQOH,
				(	SELECT 
						COUNT(*) 
					FROM 
						F580056 
					WHERE 
						RDLOTN <> ' AND
						RDDOCO = SDDOCO AND 
						RDDCTO = SDDCTO AND
						RDLNID = SDLNID AND
						RDMCU = SDMCU AND 
						RDZ58DST IN ('2','4','5')
					) AS XXCRKQOH,
				(	SELECT 
						COUNT(*) 
					FROM 
						F580056 
						INNER JOIN F580055 
						ON	RHZ58RID = RDZ58RID 
						INNER JOIN F580151 
						ON	LPLDNM = RHZ58SLDN AND 
							LPITM = RDITM 
					WHERE 
						RDLOTN = ' AND
						RDZ58SLDN > 0 AND 
						RDITM = SDITM AND 
						RDMCU = SDMCU AND 
						RDZ58DST IN ('2','4','5')
					) AS XXSRKREL,
				(	SELECT 
						COUNT(*) 
					FROM 
						F580056 
						INNER JOIN F580055 
						ON	RHZ58RID = RDZ58RID 
						INNER JOIN F580151 
						ON	LPLDNM = RHZ58SLDN AND 
							LPITM = RDITM 
					WHERE 
						RHZ58SLDN > 0 AND
						RDLOTN <> ' AND
						RDMCU =  SDMCU AND
						RDDOCO = SDDOCO AND 
						RDDCTO = SDDCTO AND
						RDLNID = SDLNID AND
						RDZ58DST IN ('2','4','5')
					) AS XXCRKREL,
				(	SELECT 
						SUM(ATZ58RKS) 
					FROM 
						F4211 SD1
						INNER JOIN F580032 AT1
						ON	AT1.ATKCOO = SD1.SDKCOO AND
							AT1.ATDOCO = SD1.SDDOCO AND
							AT1.ATDCTO = SD1.SDDCTO AND
							AT1.ATLNID = SD1.SDLNID
					WHERE 
						SD1.SDLNTY = 'S' AND
						SD1.SDITM = F4211.SDITM AND 
						SD1.SDMCU = F4211.SDMCU AND 
						SD1.SDNXTR <= '545'
					) AS XXSRKQOR,
					(SELECT 
						MAX(LHGFLAG1) AS LHGFLAG1 
					FROM 
						F580151 
						INNER JOIN F580150
						ON	LHLDNM = LPLDNM
					WHERE
						LPKCOO = SDKCOO AND
						LPDOCO = SDDOCO AND
						LPDCTO = SDDCTO AND
						LPLNID = SDLNID) AS GFLAG1
			from 
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
				INNER JOIN F4101
				ON	IMITM = SDITM
				LEFT OUTER JOIN F580430 CP1
				ON	CP1.CPAN8 = SDAN8 AND
					CP1.CPMCU = SDMCU
				LEFT OUTER JOIN F580430 CP2
				ON	CP2.CPAN8 = SDSHAN AND
					CP2.CPMCU = SDMCU
			where 
                {filterString}
				SDNXTR <= '545' and
				SDDCTO IN ('SO','ST')
            FOR READ ONLY";
            return cmd;
        }

        public static SalesOrderLines GetOpenOrders_VER1(BusinessSystemSession session, string plants)
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
                cmd = GetOpenOrders_VER1(filters);
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
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["ABALPHSH"], string.Empty).Trim();
                        line.OrderDate = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDTRDJ"));
                        line.RequestedDate = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDDRQJ"));
                        line.LineType = reader.Get<string>("SDLNTY");
                        line.OrderNum = reader.Get<decimal>("SDDOCO");
                        line.OrderType = reader.Get<string>("SDDCTO");
                        line.OrderLineNum = reader.Get<decimal>("SDLNID") / 1000;
                        line.LastStatus = reader.Get<string>("SDLTTR");
                        line.NextStatus = reader.Get<string>("SDNXTR");
                        line.ItemNum = reader.Get<decimal>("SDITM");
                        line.StockType = CastDbNull.To(reader["IMSTKT"], string.Empty).Trim();
                        line.Construction = reader.Get<string>("SDDSC1");
                        line.Dimension = reader.Get<string>("SDDSC2");
                        line.CustomerPO = reader.Get<string>("SDVR01");
                        line.CustomerPart1 = reader.Get<string>("ATZ58CP2");
                        line.CustomerPart2 = reader.Get<string>("ATZ58CP3");
                        line.CarrierFlag = reader.Get<string>("GFLAG1");

                        line.OrderQty = reader.Get<decimal>("SDUORG") / 1000;
                        line.LitesPerRack = reader.Get<decimal>("ATZ58LPR");
                        line.OrderedRacksQty = reader.Get<int>("ATZ58RKS");
                        line.OrderedRacksGlassWeight = reader.Get<decimal>("ATZ58GWTT") / 100;
                        line.OrderedRacksTareWeight = reader.Get<decimal>("ATZ58RTWT") / 100;

                        line.AllocatedRacksQty = reader.Get<int>("RHZ58RKS");
                        line.AllocatedRacksGlassWeight = reader.Get<decimal>("RHZ58GWTT") / 100;
                        line.AllocatedRacksTareWeight = reader.Get<decimal>("RHZ58RTWT") / 100;

                        var onHandItemQty = reader.Get<int>("XXSRKQOH");
                        var releasedItemQty = reader.Get<int>("XXSRKREL");
                        line.OnHandRacksQty = line.StockType == "C" ? reader.Get<int>("XXCRKQOH") : reader.Get<int>("XXSRKQOH");
                        line.ReleasedRacksQty = line.StockType == "C" ? reader.Get<int>("XXCRKREL") : reader.Get<int>("XXSRKREL");

                        lines.Add(line);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }
    }
}


