using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIServices.Common.Managers.Enterprise
{
    public class AgcOpenOrderManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public AgcOpenOrderManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("OpenRackManager.GetLockLoad", GetLockLoad());
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
        public static iDB2Command GetOpenOrders(List<string> filters)
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
				ATZ58RKS,
				(	SELECT 
						COUNT(*) 
					FROM 
						F580056 
					WHERE 
						RDLOTN = '' AND
						RDITM = SDITM AND 
						RDMCU = SDMCU AND 
						RDZ58DST IN ('2','4','5')
					) AS XXSRKQOH,
				(	SELECT 
						COUNT(*) 
					FROM 
						F580056 
					WHERE 
						RDLOTN <> '' AND
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
						RDLOTN = '' AND
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
						RDLOTN <> '' AND
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

        public static SalesOrderLines GetOpenOrders(BusinessSystemSession session, string plants)
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
						line.LitesPerRack = reader.Get<decimal>("ATZ58LPR");
                        line.OrderQty = reader.Get<decimal>("SDUORG") / 1000;
                        line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int)Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
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


