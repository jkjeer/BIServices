using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class OrderManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;
        private RackManager _rackMgr;

        public OrderManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("OrderManager.GetStockRacks", GetStockRacks());
            hash.Add("OrderManager.GetCustomRacks", GetCustomRacks());
            hash.Add("OrderManager.GetRelatedOrders", GetRelatedOrders());
            hash.Add("OrderManager.GetScheduledUnits", GetScheduledUnits());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            _rackMgr = new RackManager(session);
            _flexItemMgr = new FlexItemManager(session, "P5842300");
        }

        /*********************************************************************************
          * Stock Orders
          *******************************************************************************/
        public iDB2Command GetStockOrders(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT * FROM (
            SELECT
	            SDMCU,
	            SDAN8,
	            AB1.ABALPH AS SDAN8ALPH,
	            SDSHAN,
	            AB2.ABALPH AS SDSHANALPH,
				SDITWT,
                SDPPDJ,	            
                SDDRQJ,
                SDTRDJ,
	            SDIVD,
	            SDLTTR,
	            SDNXTR,
                SDKCOO,
	            SDDOCO,
	            SDDCTO,
	            SDLNID,
                SDRORN,
                SHVR01,
	            SDVR01,
	            SDURCD,
	            SDITM,
	            SDUORG,
                SDUNCS,
                SDECST,
                SDUPRC,
                SDAEXP,
                SDRCD,
                SDIR04,
	            ATZ58PT,
	            ATZ58CP2,
	            ATZ58CP3,
	            ATZ58CP4,
                ATZ58CSQNO,
	            ATZ58LPR,
	            ATZ58RT,
                ATZ5840460,
	            IBSTKT,
	            IBGLPT,
                IBSRP4,
	            IMDSC1,
	            IMDSC2,
                DRDL01,
                SLUPMJ,
                SLTDAY,
				UMCONV,
                CP1.CPZ58CCV05,
                CP2.CPZ58GAL1,
                LHLDNM,
                LHZ58LDS,
			    ROW_NUMBER() OVER (PARTITION BY SDDOCO, SDDCTO, SDLNID ORDER BY SLUPMJ ASC, SLTDAY ASC) AS RN
            FROM
	            F4211
                INNER JOIN F4201
                ON  SHKCOO = SDKCOO AND
                    SHDOCO = SDDOCO AND
                    SHDCTO = SDDCTO
	            INNER JOIN F580032
	            ON	ATKCOO = SDKCOO AND
		            ATDOCO = SDDOCO AND
		            ATDCTO = SDDCTO AND
		            ATLNID = SDLNID 
	            INNER JOIN F4102 
	            INNER JOIN F4101
	            ON  IMITM = IBITM
	            ON	IBMCU = SDMCU AND
		            IBITM = SDITM
	            INNER JOIN F0101 AB1
	            ON	AB1.ABAN8 = SDAN8
	            INNER JOIN F0101 AB2
	            ON	AB2.ABAN8 = SDSHAN
                LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005
                    ON  DRSY = '42' 
                    AND DRRT = 'RC' 
                    AND TRIM(DRKY) = SDRCD
                LEFT OUTER JOIN F42199
	            ON	SLKCOO = SDKCOO AND
		            SLDOCO = SDDOCO AND
		            SLDCTO = SDDCTO AND
		            SLLNID = SDLNID
				LEFT OUTER JOIN F41002
				ON	UMMCU = SDMCU AND
					UMITM = SDITM AND
					UMUM = 'EA' AND
					UMRUM = 'LB'
                LEFT OUTER JOIN F580375 CP1
                ON  CP1.CPMCU = SDMCU AND
                    CP1.CPSHAN = SDSHAN
				LEFT OUTER JOIN F580430 CP2
				ON	CP2.CPAN8 = SDSHAN AND
                    CP2.CPMCU = SDMCU
                LEFT OUTER JOIN F580056 
					INNER JOIN F580055 
					ON	RHZ58RID = RDZ58RID
					INNER JOIN F580150
					ON	LHLDNM = RHPSN
                ON  RDDOCO = SDDOCO and 
					RDDCTO = SDDCTO AND 
					RDLNID = SDLNID  AND
					RDKCOO = SDKCOO
            WHERE
                {filterString}
                SDLNTY != 'W' AND
                SDDCTO NOT IN ('SQ') AND
                SDGLC IN ('GLAS','GLAF','GLAX','GLAZ')) A
            WHERE
                A.RN = 1
            ORDER BY
	            1,2
            FOR READ ONLY";
            return cmd;
        }

        /*********************************************************************************
          * Custom Orders
          *******************************************************************************/
        public iDB2Command GetCustomOrders(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
                    SELECT * FROM (
                    SELECT
	                    SDMCU,
	                    SDAN8,
	                    AB1.ABALPH AS SDAN8ALPH,
	                    SDSHAN,
	                    AB2.ABALPH AS SDSHANALPH,
                        SDITWT,
                        SDPPDJ,
	                    SDDRQJ,
                        SDTRDJ,
	                    SDIVD,
	                    SDLTTR,
	                    SDNXTR,
                        SDKCOO,
	                    SDDOCO,
	                    SDDCTO,
	                    SDLNID,
                        SDRORN,
                        SHVR01,
	                    SDVR01,
	                    SDURCD,
	                    SDITM,
	                    SDUORG,
                        SDUNCS,
                        SDECST,
                        SDUPRC,
                        SDAEXP,
                        SDRCD,
                        SDIR04,
                        SDDSC1,
	                    ATZ58PT,
	                    ATZ58CP2,
	                    ATZ58CP3,
	                    ATZ58CP4,
                        ATZ58CSQNO,
	                    ATZ58LPR,
	                    ATZ58RT,
                        ATZ5840460,
                        ATZ58WODC,
	                    COALESCE(IBSTKT, IMSTKT) AS IBSTKT,
	                    COALESCE(IBGLPT, IMGLPT) AS IBGLPT,
	                    COALESCE(IBSRP4, IMSRP4) AS IBSRP4,
	                    IMDSC1,
	                    IMDSC2,
                        DRDL01,
                        SDUPMJ AS SLUPMJ,
                        SDTDAY AS SLTDAY,
                        CP1.CPZ58CCV05,
                        CP2.CPZ58GAL1,
                        LHLDNM,
                        LHZ58LDS,
						(select CMCFGID from F3201 where CMDOCO = ATZ58WODC and CMDCTO = 'WO' order by CMUPMJ DESC FETCH FIRST 1 ROWS ONLY) AS CMCFGID,
			            ROW_NUMBER() OVER (PARTITION BY SDDOCO, SDDCTO, SDLNID ORDER BY SDUPMJ ASC, SDTDAY ASC) AS RN
                    FROM
	                    F4211
                        INNER JOIN F4201
                        ON  SHKCOO = SDKCOO AND
                            SHDOCO = SDDOCO AND
                            SHDCTO = SDDCTO
	                    LEFT OUTER JOIN F4102 
	                    INNER JOIN F4101
	                    ON  IMITM = IBITM
	                    ON	IBMCU = SDMCU AND
		                    IBITM = SDITM
	                    INNER JOIN F0101 AB1
	                    ON	AB1.ABAN8 = SDAN8
	                    INNER JOIN F0101 AB2
	                    ON	AB2.ABAN8 = SDSHAN
	                    INNER JOIN F580032
	                    ON	ATKCOO = SDKCOO AND
		                    ATDOCO = SDDOCO AND
		                    ATDCTO = SDDCTO AND
		                    ATLNID = SDLNID 
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005
                            ON  DRSY = '42' 
                            AND DRRT = 'RC' 
                            AND TRIM(DRKY) = SDRCD                  
                        LEFT OUTER JOIN F580375 CP1
                        ON  CP1.CPMCU = SDMCU AND
                            CP1.CPSHAN = SDSHAN
				        LEFT OUTER JOIN F580430 CP2
				        ON	CP2.CPAN8 = SDSHAN AND
                            CP2.CPMCU = SDMCU

                        LEFT OUTER JOIN F580056 
					        INNER JOIN F580055 
					        ON	RHZ58RID = RDZ58RID
					        INNER JOIN F580150
					        ON	LHLDNM = RHPSN
                        ON  RDDOCO = SDDOCO and 
					        RDDCTO = SDDCTO AND 
					        RDLNID = SDLNID  AND
					        RDKCOO = SDKCOO
                    WHERE
                        {filterString}
                        SDLNTY IN ('W') AND
                        SDDCTO NOT IN ('SQ') AND
                        SDGLC IN ('GLAS','GLAF','GLAX','GLAZ','LUWT','MSAL','MSCR')) A
                    WHERE A.RN = 1                       
            ORDER BY
	            1,2
            FOR READ ONLY";
            return cmd;
        }




        public SalesOrderLines GetStockOrders(BusinessSystemSession session, string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
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

                // Dates
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) || !string.IsNullOrEmpty(dateProperty) || !string.IsNullOrEmpty(dateCode))
                {
                    DateTime startDateValue;
                    DateTime endDateValue;

                    if (!string.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        startDateValue = dateValues[0];
                        endDateValue = dateValues[1];
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(startDate))
                        {
                            startDateValue = DateCodeManager.IsDate(startDate)
                                ? Convert.ToDateTime(startDate)
                                : DateCodeManager.GetDateCodeValue(startDate);
                        }
                        else
                        {
                            startDateValue = new DateTime(1900, 1, 1);
                        }


                        if (!string.IsNullOrEmpty(endDate))
                        {
                            endDateValue = DateCodeManager.IsDate(endDate)
                                ? Convert.ToDateTime(endDate)
                                : DateCodeManager.GetDateCodeValue(endDate);
                        }
                        else
                        {
                            endDateValue = new DateTime(2050, 12, 31);
                        }
                    }

                    // Date Field
                    if (!string.IsNullOrEmpty(dateProperty))
                    {
                        switch (dateProperty.ToUpper())
                        {
                            case "DUE":
                            case "REQ":
                            case "REQUEST":
                            case "REQUESTED":
                                filters.Add("SDDRQJ >= ? AND\r\n");
                                filters.Add("SDDRQJ <= ? AND\r\n");
                                break;
                            case "ORD":
                            case "ORDER":
                            case "ORDERED":
                                filters.Add("SDTRDJ >= ? AND\r\n");
                                filters.Add("SDTRDJ <= ? AND\r\n");
                                break;
                            default:
                                filters.Add("SDTRDJ >= ? AND\r\n");
                                filters.Add("SDTRDJ <= ? AND\r\n");
                                break;
                        }
                    }
                    else
                    {
                        filters.Add("SDDRQJ >= ? AND\r\n");
                        filters.Add("SDDRQJ <= ? AND\r\n");
                    }

                    parameters.Add(new iDB2Parameter("StartDate", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Utilities.Common.CastToE1Date(startDateValue);

                    parameters.Add(new iDB2Parameter("EndDate", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Utilities.Common.CastToE1Date(endDateValue);
                }

                // Last Status
                if (!string.IsNullOrEmpty(lastStatusCodes))
                {
                    var statusCodesList = lastStatusCodes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string statusCode in statusCodesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("LTTR", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = statusCode;
                    }
                    filters.Add("SDLTTR IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Next Status
                if (!string.IsNullOrEmpty(nextStatusCodes))
                {
                    var statusCodesList = nextStatusCodes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string statusCode in statusCodesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("NXTR", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = statusCode;
                    }
                    filters.Add("SDNXTR IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Customer Sold Num
                if (!string.IsNullOrEmpty(customerSoldNums))
                {
                    var customerSoldNumsList = Array.ConvertAll(customerSoldNums.Split(','), Convert.ToDecimal).ToList();
                    string strInClause = "";
                    foreach (decimal customerSoldNum in customerSoldNumsList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerSoldNum;
                    }
                    filters.Add("SDAN8 IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Customer Ship Num
                if (!string.IsNullOrEmpty(customerShipNums))
                {
                    var customerShipNumsList = Array.ConvertAll(customerShipNums.Split(','), Convert.ToDecimal).ToList();
                    string strInClause = "";
                    foreach (decimal customerShipNum in customerShipNumsList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("SHAN", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerShipNum;
                    }
                    filters.Add("SDSHAN IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Stock Types
                if (!string.IsNullOrEmpty(stockTypes))
                {
                    var stockTypesList = stockTypes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string stockType in stockTypesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("STKT", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = stockType;
                    }
                    filters.Add("IBSTKT IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Document Types
                if (!string.IsNullOrEmpty(documentTypes))
                {
                    var documentTypesList = documentTypes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string documentType in documentTypesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = documentType;
                    }
                    filters.Add("SDDCTO IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Options
                if (!string.IsNullOrEmpty(options))
                {
                    var optionsList = options.ToUpper().Split(',').ToList();
                    if (optionsList.Contains("OPEN"))
                    {
                        filters.Add("SDLTTR <= '545' AND\r\n");
                    }
                }

                // Command
                cmd = GetStockOrders(filters);
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
                        try
                        {
                            var line = new SalesOrderLine();
                            line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                            line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                            line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                            line.CreatedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SLUPMJ"], 0), CastDbNull.To<decimal>(reader["SLTDAY"], 0));
                            line.PromiseDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDPPDJ"], 0));
                            line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                            line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                            line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                            line.ReasonCodeDescription = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                            //line.PurchaseDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["ROUPMJ"], 0));
                            line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                            line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                            line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                            line.LoadNum = CastDbNull.To<decimal>(reader["LHLDNM"], 0);
                            line.LoadStatus = CastDbNull.To(reader["LHZ58LDS"], string.Empty).Trim();
                            line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                            line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                            line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                            line.CustomerName = CastDbNull.To(reader["SDAN8ALPH"], string.Empty).Trim();
                            line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
                            line.CustomerShipName = CastDbNull.To(reader["SDSHANALPH"], string.Empty).Trim();
                            line.CustomerPart1 = CastDbNull.To(reader["ATZ58CP2"], string.Empty).Trim();
                            line.CustomerPart2 = CastDbNull.To(reader["ATZ58CP3"], string.Empty).Trim();
                            line.CustomerPart3 = CastDbNull.To(reader["ATZ58CP4"], string.Empty).Trim();
                            line.CustomerPOHdr = CastDbNull.To(reader["SHVR01"], string.Empty).Trim();
                            line.CustomerPO = CastDbNull.To(reader["SDVR01"], string.Empty).Trim();
                            line.CustomerTier = CastDbNull.To(reader["CPZ58CCV05"], string.Empty).Trim();
                            line.ProductType = CastDbNull.To(reader["ATZ58PT"], string.Empty).Trim();
                            line.AccountManager = CastDbNull.To(reader["CPZ58GAL1"], string.Empty).Trim();

                            line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                            line.UnitCost = CastDbNull.To<decimal>(reader["SDUNCS"], 0) / 10000;
                            line.TotalCost = CastDbNull.To<decimal>(reader["SDECST"], 0) / 100;
                            line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                            line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                            line.PriorityCode = CastDbNull.To(reader["SDURCD"], string.Empty).Trim();
                            line.LitesPerRack = CastDbNull.To<decimal>(reader["ATZ58LPR"], 0);

                            line.TotalGlassPounds = CastDbNull.To<decimal>(reader["SDITWT"], 0) / 10000;
                            line.TotalGlassTons = line.TotalGlassPounds / 2000;
                            line.TotalGlassWeight = line.OrderQty * CastDbNull.To<decimal>(reader["UMCONV"], 0) / 10000000;

                            line.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                            line.ItemNum = CastDbNull.To<decimal>(reader["SDITM"], 0);
                            line.ItemDesc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                            line.ItemDesc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                            line.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                            line.GlCategory = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                            line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int)Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
                            line.HvLv = CastDbNull.To(reader["ATZ5840460"], string.Empty).Trim();
                            lines.Add(line);
                        }
                        catch (Exception e)
                        { 
                            throw new Exception(e.Message); 
                        }    
                    }

                    reader.Close();
                    _logger.Info($"{i} Rows");
                    _logger.Info("End Reader");
                }

                try
                {
                    GetScheduledUnits(session, lines);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                foreach (var line in lines)
                {
                    try
                    {
                        var itemAtts = _itemAttMgr.GetItemAttributes(session, line.ItemNum, 0);
                        line.ItemAttributes = itemAtts;
                        line.QCInspectFlag = itemAtts.GetStringAttributeValue("Z5801840");
                        line.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                        line.Lite1HeatTreatment = itemAtts.GetStringAttributeValue("Z58L1HT");
                        line.Lite2HeatTreatment = itemAtts.GetStringAttributeValue("Z58L2HT");
                        line.Lite3HeatTreatment = itemAtts.GetStringAttributeValue("Z58L3HT");

                        line.ExtPrsvScoring = itemAtts.GetDecimalAttributeValue("Z587569");
                        line.IntPrsvScoring = itemAtts.GetDecimalAttributeValue("Z587581");
                        line.ExtPrsvCB = itemAtts.GetStringAttributeValue("Z587563");
                        line.IntPrsvCB = itemAtts.GetStringAttributeValue("Z587575");

                        line.TotalActualSqft = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z58USF");
                        line.TotalBillableSqft = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z58BSF");
                        line.BarsQty =
                            (itemAtts.GetDecimalAttributeValue("Z58GHA1") + itemAtts.GetDecimalAttributeValue("Z58GVA1") +
                             itemAtts.GetDecimalAttributeValue("Z58GHA2") + itemAtts.GetDecimalAttributeValue("Z58GVA2") >
                             0 && itemAtts.GetStringAttributeValue("Z58M1A1") != "989")
                                ? line.OrderQty
                                : 0;
                        line.ShapeNum = itemAtts.GetDecimalAttributeValue("Z58SHAPE");
                        if (line.BarsQty > 0)
                        {
                            line.BarOpenings = (line.ShapeNum == 0 && line.BarsQty > 0)
                                ? itemAtts.GetDecimalAttributeValue("Z58OHA1") * itemAtts.GetDecimalAttributeValue("Z58OWA1") +
                                   itemAtts.GetDecimalAttributeValue("Z58OHA2") * itemAtts.GetDecimalAttributeValue("Z58OWA2") : (itemAtts.GetDecimalAttributeValue("Z58OOA1") + itemAtts.GetDecimalAttributeValue("Z58OOA2"));
                        }
                        line.Construction = _flexItemMgr.GetUnitConstruction(itemAtts);
                        line.Dimension = _flexItemMgr.GetDimension(itemAtts);
                        line.Dimensions = _flexItemMgr.GetDimension(itemAtts);
                        line.Lite1GlassType = _flexItemMgr.GetLite1GlassType(itemAtts);
                        line.Lite2GlassType = _flexItemMgr.GetLite2GlassType(itemAtts);
                        line.Lite3GlassType = _flexItemMgr.GetLite3GlassType(itemAtts);
                        line.LiteRouting = _flexItemMgr.GetLiteRouting(itemAtts);
                        line.Airspace = _flexItemMgr.GetAirspace(itemAtts, true);
                        line.SpacerType = _flexItemMgr.GetSpacerType(itemAtts, true);
                        line.BarAssembly = _flexItemMgr.GetBarAssembly(itemAtts, true);
                        line.Grid = _flexItemMgr.GetGrid(itemAtts, true);
                        line.GridIndexes = _flexItemMgr.GetGridIndexes(itemAtts);
                        line.Logo = _flexItemMgr.GetLogo(itemAtts);
                        line.PreserveProcessing = _flexItemMgr.GetPreserveProcessing(itemAtts, true);
                        line.EdgeProcessing = _flexItemMgr.GetEdgeProcessing(itemAtts, true);
                        line.HoleProcessing = _flexItemMgr.GetHoleProcessing(itemAtts, true);

                        int lite1Routing;
                        int lite2Routing;
                        int lite3Routing;
                        int.TryParse(line.Lite1Routing, out lite1Routing);
                        int.TryParse(line.Lite2Routing, out lite2Routing);
                        int.TryParse(line.Lite3Routing, out lite3Routing);
                        line.MonoQty = 0;
                        line.MonoSqft = 0;
                        line.DoubleQty = 0;
                        line.DoubleSqft = 0;
                        line.TripleQty = 0;
                        line.TripleSqft = 0;
                        line.TemperedSqft = 0;
                        line.TemperedPurchasedSqft = 0;
                        line.TemperedInternalSqft = 0;
                        line.CutSqft = 0;
                        switch (line.UnitType)
                        {
                            case "1":
                            case "L":
                                line.MonoQty = line.OrderQty;
                                line.MonoSqft = line.TotalActualSqft;
                                if (line.Lite1GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TotalActualSqft;
                                if (lite1Routing >= 10 && lite1Routing <= 20) line.CutSqft = line.TotalActualSqft;
                                break;
                            case "2":
                                line.DoubleQty = line.OrderQty;
                                line.DoubleSqft = line.TotalActualSqft;
                                if (line.Lite1GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TotalActualSqft;
                                if (line.Lite2GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TemperedSqft + line.TotalActualSqft;
                                if (lite1Routing >= 10 && lite1Routing <= 20) line.CutSqft = line.TotalActualSqft;
                                if (lite2Routing >= 10 && lite2Routing <= 20) line.CutSqft = line.CutSqft + line.TotalActualSqft;
                                break;
                            case "3":
                                line.TripleQty = line.OrderQty;
                                line.TripleSqft = line.TotalActualSqft;
                                if (line.Lite1GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TotalActualSqft;
                                if (line.Lite2GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TemperedSqft + line.TotalActualSqft;
                                if (line.Lite3GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TemperedSqft + line.TotalActualSqft;
                                if (lite1Routing >= 10 && lite1Routing <= 20) line.CutSqft = line.TotalActualSqft;
                                if (lite2Routing >= 10 && lite2Routing <= 20) line.CutSqft = line.CutSqft + line.TotalActualSqft;
                                if (lite3Routing >= 10 && lite3Routing <= 20) line.CutSqft = line.CutSqft + line.TotalActualSqft;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }

                _logger.Info("End Attributes");

                if (!string.IsNullOrEmpty(racks) && racks.ToUpper() == "Y")
                {
                    try
                    {
                        GetStockRacks(session, lines);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
                _logger.Info("End Racks");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        public SalesOrderLines GetCustomOrders(BusinessSystemSession session, string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
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

                // Dates
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) || !string.IsNullOrEmpty(dateProperty) || !string.IsNullOrEmpty(dateCode))
                {
                    DateTime startDateValue;
                    DateTime endDateValue;

                    if (!string.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        startDateValue = dateValues[0];
                        endDateValue = dateValues[1];
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(startDate))
                        {
                            startDateValue = DateCodeManager.IsDate(startDate)
                                ? Convert.ToDateTime(startDate)
                                : DateCodeManager.GetDateCodeValue(startDate);
                        }
                        else
                        {
                            startDateValue = new DateTime(1900, 1, 1);
                        }


                        if (!string.IsNullOrEmpty(endDate))
                        {
                            endDateValue = DateCodeManager.IsDate(endDate)
                                ? Convert.ToDateTime(endDate)
                                : DateCodeManager.GetDateCodeValue(endDate);
                        }
                        else
                        {
                            endDateValue = new DateTime(2050, 12, 31);
                        }
                    }

                    // Date Field
                    if (!string.IsNullOrEmpty(dateProperty))
                    {
                        switch (dateProperty.ToUpper())
                        {
                            case "DUE":
                            case "REQ":
                            case "REQUEST":
                            case "REQUESTED":
                                filters.Add("SDDRQJ >= ? AND\r\n");
                                filters.Add("SDDRQJ <= ? AND\r\n");
                                break;
                            case "ORD":
                            case "ORDER":
                            case "ORDERED":
                                filters.Add("SDTRDJ >= ? AND\r\n");
                                filters.Add("SDTRDJ <= ? AND\r\n");
                                break;
                            default:
                                filters.Add("SDTRDJ >= ? AND\r\n");
                                filters.Add("SDTRDJ <= ? AND\r\n");
                                break;
                        }
                    }
                    else
                    {
                        filters.Add("SDDRQJ >= ? AND\r\n");
                        filters.Add("SDDRQJ <= ? AND\r\n");
                    }

                    parameters.Add(new iDB2Parameter("StartDate", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Utilities.Common.CastToE1Date(startDateValue);

                    parameters.Add(new iDB2Parameter("EndDate", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Utilities.Common.CastToE1Date(endDateValue);
                }

                // Last Status
                if (!string.IsNullOrEmpty(lastStatusCodes))
                {
                    var statusCodesList = lastStatusCodes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string statusCode in statusCodesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("LTTR", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = statusCode;
                    }
                    filters.Add("SDLTTR IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Next Status
                if (!string.IsNullOrEmpty(nextStatusCodes))
                {
                    var statusCodesList = nextStatusCodes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string statusCode in statusCodesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("NXTR", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = statusCode;
                    }
                    filters.Add("SDNXTR IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Customer Sold Num
                if (!string.IsNullOrEmpty(customerSoldNums))
                {
                    var customerSoldNumsList = Array.ConvertAll(customerSoldNums.Split(','), Convert.ToDecimal).ToList();
                    string strInClause = "";
                    foreach (decimal customerSoldNum in customerSoldNumsList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerSoldNum;
                    }
                    filters.Add("SDAN8 IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Customer Ship Num
                if (!string.IsNullOrEmpty(customerShipNums))
                {
                    var customerShipNumsList = Array.ConvertAll(customerShipNums.Split(','), Convert.ToDecimal).ToList();
                    string strInClause = "";
                    foreach (decimal customerShipNum in customerShipNumsList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("SHAN", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerShipNum;
                    }
                    filters.Add("SDSHAN IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Status
                if (!string.IsNullOrEmpty(stockTypes))
                {
                    var stockTypesList = stockTypes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string stockType in stockTypesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("STKT", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = stockType;
                    }
                    filters.Add("IBSTKT IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Document Types
                if (!string.IsNullOrEmpty(documentTypes))
                {
                    var documentTypesList = documentTypes.Split(',').ToList();
                    string strInClause = "";
                    foreach (string documentType in documentTypesList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = documentType;
                    }
                    filters.Add("SDDCTO IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Options
                if (!string.IsNullOrEmpty(options))
                {
                    var optionsList = options.ToUpper().Split(',').ToList();
                    if (optionsList.Contains("OPEN"))
                    {
                        filters.Add("SDLTTR <= '545' AND\r\n");
                    }
                }

                // Orders Command
                cmd = GetCustomOrders(filters);
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
                    _logger.Info("reader start");
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        //line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.WorkOrderNum = CastDbNull.To<decimal>(reader["ATZ58WODC"], 0);
                        line.LotNum = line.WorkOrderNum.ToString(CultureInfo.InvariantCulture);
                        line.CreatedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SLUPMJ"], 0), CastDbNull.To<decimal>(reader["SLTDAY"], 0));
                        line.PromiseDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDPPDJ"], 0));
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.ReasonCodeDescription = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                        //line.PurchaseDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["ROUPMJ"], 0));
                        line.OrderCompany = CastDbNull.To(reader["SDKCOO"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LoadNum = CastDbNull.To<decimal>(reader["LHLDNM"], 0);
                        line.LoadStatus = CastDbNull.To(reader["LHZ58LDS"], string.Empty).Trim();
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["SDAN8ALPH"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["SDSHANALPH"], string.Empty).Trim();
                        line.CustomerPart1 = CastDbNull.To(reader["ATZ58CP2"], string.Empty).Trim();
                        line.CustomerPart2 = CastDbNull.To(reader["ATZ58CP3"], string.Empty).Trim();
                        line.CustomerPart3 = CastDbNull.To(reader["ATZ58CP4"], string.Empty).Trim();
                        line.CustomerSequenceNum = CastDbNull.To(reader["ATZ58CSQNO"], string.Empty).Trim();
                        line.CustomerPOHdr = CastDbNull.To(reader["SHVR01"], string.Empty).Trim();
                        line.CustomerPO = CastDbNull.To(reader["SDVR01"], string.Empty).Trim();
                        line.CustomerTier = CastDbNull.To(reader["CPZ58CCV05"], string.Empty).Trim();
                        line.ProductType = CastDbNull.To(reader["ATZ58PT"], string.Empty).Trim();
                        line.AccountManager = CastDbNull.To(reader["CPZ58GAL1"], string.Empty).Trim();
                        line.Construction = CastDbNull.To(reader["SDDSC1"], string.Empty).Trim();

                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitCost = CastDbNull.To<decimal>(reader["SDUNCS"], 0) / 10000;
                        line.TotalCost = CastDbNull.To<decimal>(reader["SDECST"], 0) / 100;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.PriorityCode = CastDbNull.To(reader["SDURCD"], string.Empty).Trim();
                        line.LitesPerRack = CastDbNull.To<decimal>(reader["ATZ58LPR"], 0);
                        line.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                        line.ItemNum = CastDbNull.To<decimal>(reader["SDITM"], 0);
                        line.ItemDesc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        line.ItemDesc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        line.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                        line.GlCategory = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                        line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int) Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.HvLv = CastDbNull.To(reader["ATZ5840460"], string.Empty).Trim();

                        var attDict = new Dictionary<string, string>();
                        //attDict.Add("Z58L1TK", (CastDbNull.To<Decimal>(reader["SAZ58L1TK"], 0) / 10).ToString());
                        //attDict.Add("Z58L11C", CastDbNull.To<String>(reader["SAZ58L11C"], String.Empty).Trim());
                        //attDict.Add("Z58L1CL", CastDbNull.To<String>(reader["SAZ58L1CL"], String.Empty).Trim());
                        //attDict.Add("Z58L1HT", CastDbNull.To<String>(reader["SAZ58L1HT"], String.Empty).Trim());
                        //attDict.Add("Z58L12C", CastDbNull.To<String>(reader["SAZ58L12C"], String.Empty).Trim());
                        //attDict.Add("Z58L1PX", CastDbNull.To<String>(reader["SAZ58L1PX"], String.Empty).Trim());
                        //attDict.Add("Z5800495", CastDbNull.To<String>(reader["SAZ5800495"], String.Empty).Trim());
                        //attDict.Add("Z58L1LPT", CastDbNull.To<String>(reader["SAZ58L1LPT"], String.Empty).Trim());
                        //attDict.Add("Z5800522", CastDbNull.To<String>(reader["SAZ5800522"], String.Empty).Trim());
                        //attDict.Add("Z5800524", CastDbNull.To<String>(reader["SAZ5800524"], String.Empty).Trim());

                        //attDict.Add("Z5800536", CastDbNull.To<Decimal>(reader["SAZ58L2TK"], 0).ToString());
                        //attDict.Add("Z5800537", CastDbNull.To<Decimal>(reader["SAZ5800537"], 0).ToString());
                        //attDict.Add("Z5800538", CastDbNull.To<Decimal>(reader["SAZ5800538"], 0).ToString());
                        //attDict.Add("Z5800539", CastDbNull.To<Decimal>(reader["SAZ5800539"], 0).ToString());

                        //attDict.Add("Z58SHAPE", CastDbNull.To<String>(reader["SAZ58SHAPE"], String.Empty).Trim());
                        //attDict.Add("Z58L1BDM", CastDbNull.To<Decimal>(reader["SAZ58L1BDM"], 0).ToString());
                        //attDict.Add("Z58L1BNM", CastDbNull.To<Decimal>(reader["SAZ58L1BNM"], 0).ToString());
                        //attDict.Add("Z58L1BDN", CastDbNull.To<Decimal>(reader["SAZ58L1BDN"], 0).ToString());
                        //attDict.Add("Z58L1LDM", CastDbNull.To<Decimal>(reader["SAZ58L1LDM"], 0).ToString());
                        //attDict.Add("Z58L1LNM", CastDbNull.To<Decimal>(reader["SAZ58L1LNM"], 0).ToString());
                        //attDict.Add("Z58L1LDN", CastDbNull.To<Decimal>(reader["SAZ58L1LDN"], 0).ToString());
                        //attDict.Add("Z58L1RDM", CastDbNull.To<Decimal>(reader["SAZ58L1RDM"], 0).ToString());
                        //attDict.Add("Z58L1RNM", CastDbNull.To<Decimal>(reader["SAZ58L1RNM"], 0).ToString());
                        //attDict.Add("Z58L1RDN", CastDbNull.To<Decimal>(reader["SAZ58L1RDN"], 0).ToString());

                        //attDict.Add("Z58L1VG", CastDbNull.To<String>(reader["SAZ58L1VG"], String.Empty).Trim());
                        //attDict.Add("Z58L1HL", CastDbNull.To<String>(reader["SAZ58L1HL"], String.Empty).Trim());
                        //attDict.Add("Z58L1EW", CastDbNull.To<String>(reader["SAZ58L1EW"], String.Empty).Trim());
                        //attDict.Add("Z58L1PQL", CastDbNull.To<String>(reader["SAZ58L1PQL"], String.Empty).Trim());
                        //attDict.Add("Z58L1SSZ", CastDbNull.To<Decimal>(reader["SAZ58L1SSZ"], 0).ToString());

                        //attDict.Add("Z58L1STP", CastDbNull.To<String>(reader["SAZ58L1STP"], String.Empty).Trim());
                        //attDict.Add("Z58L2TK", (CastDbNull.To<Decimal>(reader["SAZ58L2TK"], 0) / 10).ToString());
                        //attDict.Add("Z58L23C", CastDbNull.To<String>(reader["SAZ58L23C"], String.Empty).Trim());
                        //attDict.Add("Z58L2CL", CastDbNull.To<String>(reader["SAZ58L2CL"], String.Empty).Trim());
                        //attDict.Add("Z58L2HT", CastDbNull.To<String>(reader["SAZ58L2HT"], String.Empty).Trim());
                        //attDict.Add("Z58L24C", CastDbNull.To<String>(reader["SAZ58L24C"], String.Empty).Trim());
                        //attDict.Add("Z58L2PX", CastDbNull.To<String>(reader["SAZ58L2PX"], String.Empty).Trim());
                        //attDict.Add("Z58L2LPT", CastDbNull.To<String>(reader["SAZ58L2LPT"], String.Empty).Trim());
                        //attDict.Add("Z58L2VG", CastDbNull.To<String>(reader["SAZ58L2VG"], String.Empty).Trim());
                        //attDict.Add("Z58L2HL", CastDbNull.To<String>(reader["SAZ58L2HL"], String.Empty).Trim());
                        //attDict.Add("Z58L3TK", (CastDbNull.To<Decimal>(reader["SAZ58L3TK"], 0) / 10).ToString());
                        //attDict.Add("Z58L35C", CastDbNull.To<String>(reader["SAZ58L35C"], String.Empty).Trim());
                        //attDict.Add("Z58L3CL", CastDbNull.To<String>(reader["SAZ58L3CL"], String.Empty).Trim());
                        //attDict.Add("Z58L3HT", CastDbNull.To<String>(reader["SAZ58L3HT"], String.Empty).Trim());
                        //attDict.Add("Z58L36C", CastDbNull.To<String>(reader["SAZ58L36C"], String.Empty).Trim());
                        //attDict.Add("Z58L3PX", CastDbNull.To<String>(reader["SAZ58L3PX"], String.Empty).Trim());
                        //attDict.Add("Z58L3LPT", CastDbNull.To<String>(reader["SAZ58L3LPT"], String.Empty).Trim());
                        //attDict.Add("Z58L3VG", CastDbNull.To<String>(reader["SAZ58L3VG"], String.Empty).Trim());
                        //attDict.Add("Z58L3HL", CastDbNull.To<String>(reader["SAZ58L3HL"], String.Empty).Trim());
                        //attDict.Add("Z58L3EW", CastDbNull.To<String>(reader["SAZ58L3EW"], String.Empty).Trim());

                        //attDict.Add("Z58M1A1", CastDbNull.To<Decimal>(reader["SAZ58M1A1"], 0).ToString());
                        //attDict.Add("Z58OOA1", CastDbNull.To<Decimal>(reader["SAZ58OOA1"], 0).ToString());
                        ////attDict.Add("Z58OOA2", CastDbNull.To<Decimal>(reader["SAZ58OOA2"], 0).ToString());
                        ////attDict.Add("Z58OWA1", CastDbNull.To<Decimal>(reader["SAZ58OWA1"], 0).ToString());
                        ////attDict.Add("Z58OWA2", CastDbNull.To<Decimal>(reader["SAZ58OWA2"], 0).ToString());
                        ////attDict.Add("Z58OHA1", CastDbNull.To<Decimal>(reader["SAZ58OHA1"], 0).ToString());
                        ////attDict.Add("Z58OHA2", CastDbNull.To<Decimal>(reader["SAZ58OHA2"], 0).ToString());
                        ////attDict.Add("Z58CCA1", CastDbNull.To<Decimal>(reader["SAZ58CCA1"], 0).ToString());
                        ////attDict.Add("Z58GHA1", CastDbNull.To<Decimal>(reader["SAZ58GHA1"], 0).ToString());
                        ////attDict.Add("Z58GVA1", CastDbNull.To<Decimal>(reader["SAZ58GVA1"], 0).ToString());

                        //attDict.Add("Z58GSS1", CastDbNull.To<String>(reader["SAZ58GSS1"], String.Empty).Trim());
                        //attDict.Add("Z58RT", CastDbNull.To<String>(reader["SAZ58RT"], String.Empty).Trim());
                        //attDict.Add("Z587563", CastDbNull.To<String>(reader["SAZ587563"], String.Empty).Trim());
                        //attDict.Add("Z587569", CastDbNull.To<Decimal>(reader["SAZ587569"], 0).ToString());
                        //attDict.Add("Z587575", CastDbNull.To<String>(reader["SAZ587575"], String.Empty).Trim());
                        //attDict.Add("Z587581", CastDbNull.To<Decimal>(reader["SAZ587581"], 0).ToString());

                        //attDict.Add("Z5840275", CastDbNull.To<String>(reader["SAZ5840275"], String.Empty).Trim());
                        //attDict.Add("Z58PT", CastDbNull.To<String>(reader["SAZ58PT"], String.Empty).Trim());
                        //attDict.Add("Z5840460", CastDbNull.To<String>(reader["SAZ5840460"], String.Empty).Trim());
                        //attDict.Add("Z58USF", CastDbNull.To<Decimal>(reader["SAZ58USF"], 0).ToString());
                        //attDict.Add("Z58BSF", CastDbNull.To<Decimal>(reader["SAZ58USF"], 0).ToString());

                        //attDict.Add("Z58L1RTG", CastDbNull.To<Decimal>(reader["SAZ58L1RTG"], 0).ToString());
                        //attDict.Add("Z58L2RTG", CastDbNull.To<Decimal>(reader["SAZ58L2RTG"], 0).ToString());
                        //attDict.Add("Z58L3RTG", CastDbNull.To<Decimal>(reader["SAZ58L3RTG"], 0).ToString());
                        //attDict.Add("AN8DR", CastDbNull.To<Decimal>(reader["SAAN8DR"], 0).ToString());
                        //attDict.Add("Z5852020", CastDbNull.To<String>(reader["SAZ5852020"], String.Empty).Trim());
                        //attDict.Add("Z5852021", CastDbNull.To<String>(reader["SAZ5852021"], String.Empty).Trim());
                        //attDict.Add("Z5852022", CastDbNull.To<String>(reader["SAZ5852022"], String.Empty).Trim());
                        //attDict.Add("Z5852060", CastDbNull.To<String>(reader["SAZ5852060"], String.Empty).Trim());
                        //attDict.Add("Z5852061", CastDbNull.To<String>(reader["SAZ5852061"], String.Empty).Trim());
                        //attDict.Add("Z5852062", CastDbNull.To<String>(reader["SAZ5852062"], String.Empty).Trim());

                        //attDict.Add("Z5852075", CastDbNull.To<Decimal>(reader["SAZ5852075"], 0).ToString());
                        //attDict.Add("Z5852076", CastDbNull.To<Decimal>(reader["SAZ5852076"], 0).ToString());
                        //attDict.Add("Z5852077", CastDbNull.To<Decimal>(reader["SAZ5852077"], 0).ToString());

                        line.AttributesDict = attDict;
                        lines.Add(line);
                        i++;
                        if (i % 10000 == 0)
                        {
                            _logger.Info($"Reader {i}");
                        }
                    }

                    reader.Close();
                    _logger.Info($"{i} Rows");
                    _logger.Info("End Reader");
                }

                int j = 0;
                foreach (var line in lines)
                {
                    _logger.Info($"Line Lot Num {line.LotNum}");
                    j++;
                    if (j % 1000 == 0)
                    {
                        _logger.Info($"Attributes {j}");
                    }

                    var itemAtts = _itemAttMgr.GetItemAttributes(session, 0, line.ConfiguratorId);

                    //var f5834A15mgr = new F5834A15Manager(session);
                    //var itemAtts = f5834A15mgr.GetItemAttributes(session, line.AttributesDict);

                    line.QCInspectFlag = itemAtts.GetStringAttributeValue("Z5801840");
                    line.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                    line.Lite1HeatTreatment = itemAtts.GetStringAttributeValue("Z58L1HT");
                    line.Lite2HeatTreatment = itemAtts.GetStringAttributeValue("Z58L2HT");
                    line.Lite3HeatTreatment = itemAtts.GetStringAttributeValue("Z58L3HT");
                    line.ExtPrsvScoring = itemAtts.GetDecimalAttributeValue("Z587569");
                    line.IntPrsvScoring = itemAtts.GetDecimalAttributeValue("Z587581");
                    line.ExtPrsvCB = itemAtts.GetStringAttributeValue("Z587563");
                    line.IntPrsvCB = itemAtts.GetStringAttributeValue("Z587575");
                    line.Lite1Routing = itemAtts.GetStringAttributeValue("Z58L1RTG");
                    line.Lite2Routing = itemAtts.GetStringAttributeValue("Z58L2RTG");
                    line.Lite3Routing = itemAtts.GetStringAttributeValue("Z58L3RTG");
                    line.TotalActualSqft = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z58USF");
                    line.TotalBillableSqft = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z58BSF");
                    line.TotalGlassWeight = itemAtts.GetDecimalAttributeValue("Z58GLWT");
                    line.Lite1TotalWeight = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z5887004");
                    line.Lite2TotalWeight = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z5887005");
                    line.Lite3TotalWeight = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z5887006");
                    line.EndItemPickNum = itemAtts.GetDecimalAttributeValue("Z5880230");
                    line.Lite1ItemNum = itemAtts.GetDecimalAttributeValue("Z5851551");
                    line.BarsQty =
                        (itemAtts.GetDecimalAttributeValue("Z58GHA1") + itemAtts.GetDecimalAttributeValue("Z58GVA1") +
                         itemAtts.GetDecimalAttributeValue("Z58GHA2") + itemAtts.GetDecimalAttributeValue("Z58GVA2") >
                         0  && itemAtts.GetStringAttributeValue("Z58M1A1") != "989")
                            ? line.OrderQty
                            : 0;
                    line.ShapeNum = itemAtts.GetDecimalAttributeValue("Z58SHAPE");
                    if (line.BarsQty > 0)
                    {
                        line.BarOpenings = (line.ShapeNum == 0 && line.BarsQty > 0)
                            ? itemAtts.GetDecimalAttributeValue("Z58OHA1") * itemAtts.GetDecimalAttributeValue("Z58OWA1") +
                              itemAtts.GetDecimalAttributeValue("Z58OHA2") * itemAtts.GetDecimalAttributeValue("Z58OWA2") : (itemAtts.GetDecimalAttributeValue("Z58OOA1") + itemAtts.GetDecimalAttributeValue("Z58OOA2"));
                    }
                    line.Dimension = _flexItemMgr.GetDimension(itemAtts);
                    line.Lite1GlassType = _flexItemMgr.GetLite1GlassType(itemAtts);
                    line.Lite2GlassType = _flexItemMgr.GetLite2GlassType(itemAtts);
                    line.Lite3GlassType = _flexItemMgr.GetLite3GlassType(itemAtts);
                    line.LiteRouting = _flexItemMgr.GetLiteRouting(itemAtts);
                    line.Airspace = _flexItemMgr.GetAirspace(itemAtts, true);
                    line.SpacerType = _flexItemMgr.GetSpacerType(itemAtts, true);
                    line.BarAssembly = _flexItemMgr.GetBarAssembly(itemAtts, true);
                    line.Grid = _flexItemMgr.GetGrid(itemAtts, true);
                    line.GridIndexes = _flexItemMgr.GetGridIndexes(itemAtts);
                    line.Dimensions = _flexItemMgr.GetDimension(itemAtts);
                    line.Logo = _flexItemMgr.GetLogo(itemAtts);
                    line.PreserveProcessing = _flexItemMgr.GetPreserveProcessing(itemAtts, true);
                    line.EdgeProcessing = _flexItemMgr.GetEdgeProcessing(itemAtts, true);
                    line.HoleProcessing = _flexItemMgr.GetHoleProcessing(itemAtts, true);
                    //line.ConfigHash = _flexItemMgr.GetHashValue(itemAtts);

                    // New Metrics
                    int lite1Routing;
                    int lite2Routing;
                    int lite3Routing;
                    int.TryParse(line.Lite1Routing, out lite1Routing);
                    int.TryParse(line.Lite2Routing, out lite2Routing);
                    int.TryParse(line.Lite3Routing, out lite3Routing);
                    line.MonoQty = 0;
                    line.MonoSqft = 0;
                    line.DoubleQty = 0;
                    line.DoubleSqft = 0;
                    line.TripleQty = 0;
                    line.TripleSqft = 0;
                    line.TemperedSqft = 0;
                    line.TemperedPurchasedSqft = 0;
                    line.TemperedInternalSqft = 0;
                    line.LaminatedSqft = 0;
                    line.CutSqft = 0;

                    List<string> temperedCodes = new List<string> { "H", "T" };
                    switch (line.UnitType)
                    {
                        case "1":
                            line.MonoQty = line.OrderQty;
                            line.MonoSqft = line.TotalActualSqft;
                            // Laminated
                            line.LaminatedSqft = 0;
                            // Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")))
                                line.TemperedSqft = line.TotalActualSqft;
                            // Annealed
                            if (itemAtts.GetStringAttributeValue("Z58L1HT") == "A")
                                line.AnnealedSqft = line.TotalActualSqft;
                            // Cut
                            if (lite1Routing >= 10 && lite1Routing <= 20)
                                line.CutSqft = line.TotalActualSqft;
                            break;
                        case "L":
                            line.MonoQty = line.OrderQty;
                            line.MonoSqft = line.TotalActualSqft;
                            // Laminated
                            line.LaminatedSqft = line.TotalActualSqft;
                            // Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")))
                                line.TemperedSqft = line.TotalActualSqft;
                            // Annealed
                            if (itemAtts.GetStringAttributeValue("Z58L1HT") == "A")
                                line.AnnealedSqft = line.TotalActualSqft;
                            // Cut
                            if (lite1Routing >= 10 && lite1Routing <= 20)
                                line.CutSqft = line.TotalActualSqft;
                            break;
                        case "2":
                            line.DoubleQty = line.OrderQty;
                            line.DoubleSqft = line.TotalActualSqft;
                            // Laminated
                            if (!String.IsNullOrEmpty(itemAtts.GetStringAttributeValue("Z58L1LPT")))
                                line.LaminatedSqft = line.TotalActualSqft;
                            if (!String.IsNullOrEmpty(itemAtts.GetStringAttributeValue("Z58L2LPT")))
                                line.LaminatedSqft += line.TotalActualSqft;
                            // Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")))
                                line.TemperedSqft = line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L2HT")))
                                line.TemperedSqft += line.TotalActualSqft;
                            // Purchased Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")) && lite1Routing >= 30 && lite1Routing < 50)
                                line.TemperedPurchasedSqft = line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L2HT")) && lite2Routing >= 30 && lite2Routing < 50)
                                line.TemperedPurchasedSqft += line.TotalActualSqft;
                            // Internal Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")) && lite1Routing == 12)
                                line.TemperedInternalSqft = line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L2HT")) && lite2Routing == 12)
                                line.TemperedInternalSqft += line.TotalActualSqft;
                            // Annealed
                            if (itemAtts.GetStringAttributeValue("Z58L1HT") == "A")
                                line.AnnealedSqft = line.TotalActualSqft;
                            if (itemAtts.GetStringAttributeValue("Z58L2HT") == "A")
                                line.AnnealedSqft += line.TotalActualSqft;
                            // Cut
                            if (lite1Routing >= 10 && lite1Routing <= 20)
                                line.CutSqft = line.TotalActualSqft;
                            if (lite2Routing >= 10 && lite2Routing <= 20)
                                line.CutSqft = line.CutSqft + line.TotalActualSqft;
                            break;
                        case "3":
                            line.TripleQty = line.OrderQty;
                            line.TripleSqft = line.TotalActualSqft;
                            // Laminated
                            if (!String.IsNullOrEmpty(itemAtts.GetStringAttributeValue("Z58L1LPT")))
                                line.LaminatedSqft = line.TotalActualSqft;
                            if (!String.IsNullOrEmpty(itemAtts.GetStringAttributeValue("Z58L2LPT")))
                                line.LaminatedSqft += line.TotalActualSqft;
                            if (!String.IsNullOrEmpty(itemAtts.GetStringAttributeValue("Z58L3LPT")))
                                line.LaminatedSqft = line.TotalActualSqft;
                            // Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")))
                                line.TemperedSqft = line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L2HT")))
                                line.TemperedSqft += line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L3HT")))
                                line.TemperedSqft += line.TotalActualSqft;
                            // Purchased Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")) && lite1Routing >= 30 && lite1Routing < 50)
                                line.TemperedPurchasedSqft = line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L2HT")) && lite2Routing >= 30 && lite2Routing < 50)
                                line.TemperedPurchasedSqft += line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L3HT")) && lite3Routing >= 30 && lite3Routing < 50)
                                line.TemperedPurchasedSqft += line.TotalActualSqft;
                            // Internal Tempered
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L1HT")) && lite1Routing == 12)
                                line.TemperedInternalSqft = line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L2HT")) && lite2Routing == 12)
                                line.TemperedInternalSqft += line.TotalActualSqft;
                            if (temperedCodes.Contains(itemAtts.GetStringAttributeValue("Z58L3HT")) && lite3Routing == 12)
                                line.TemperedInternalSqft += line.TotalActualSqft;
                            // Annealed
                            if (itemAtts.GetStringAttributeValue("Z58L1HT") == "A")
                                line.AnnealedSqft = line.TotalActualSqft;
                            if (itemAtts.GetStringAttributeValue("Z58L2HT") == "A")
                                line.AnnealedSqft += line.TotalActualSqft;
                            if (itemAtts.GetStringAttributeValue("Z58L3HT") == "A")
                                line.AnnealedSqft += line.TotalActualSqft;
                            // Cut
                            if (lite1Routing >= 10 && lite1Routing <= 20)
                                line.CutSqft = line.TotalActualSqft;
                            if (lite2Routing >= 10 && lite2Routing <= 20)
                                line.CutSqft += line.TotalActualSqft;
                            if (lite3Routing >= 10 && lite3Routing <= 20)
                                line.CutSqft += line.TotalActualSqft;
                            break;
                    }
                }

                _logger.Info("End Attributes");

                //GetRelatedOrders(session, lines);
                if (!string.IsNullOrEmpty(racks) && racks.ToUpper() == "Y")
                {
                    GetCustomRacks(session, lines);
                }
                _logger.Info("End Racks");

                //foreach (var line in lines)
                //{
                //    var configLines = lines.Where(x => x.ConfigHash == line.ConfigHash);
                //    var configLinesList = configLines as IList<SalesOrderLine> ?? configLines.ToList();
                //    var cfgAgg = new ConfigAggregate();
                //    cfgAgg.ConfigHashCount = configLinesList.Count();
                //    cfgAgg.Lite1TotalWeightAggregate = configLinesList.Sum(x => x.Lite1TotalWeight);
                //    cfgAgg.Lite2TotalWeightAggregate = configLinesList.Sum(x => x.Lite2TotalWeight);
                //    cfgAgg.Lite3TotalWeightAggregate = configLinesList.Sum(x => x.Lite3TotalWeight);
                //    cfgAgg.TotalGlassWeightAggregate = configLinesList.Sum(x => x.TotalGlassWeight);
                //    cfgAgg.AllLitesPurchased = configLinesList.Min(x => x.LitesPurchased);
                //    decimal[] liteWeights =
                //    {
                //        cfgAgg.Lite1TotalWeightAggregate, cfgAgg.Lite2TotalWeightAggregate,
                //        cfgAgg.Lite3TotalWeightAggregate
                //    };
                //    cfgAgg.MaximumLiteWeightAggregate = liteWeights.Max();
                //    line.ConfigAggregate = cfgAgg;
                //    if (line.TotalGlassPounds <= 0)
                //    {
                //        line.TotalGlassPounds = cfgAgg.TotalGlassWeightAggregate;
                //        line.TotalGlassTons = cfgAgg.TotalGlassWeightAggregate / 2000;
                //    }
                //}
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetStockRacks()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            RDZ58RID
            FROM
	            F580056
                INNER JOIN F580055
                ON RHZ58RID = RDZ58RID
            WHERE
	            RDITM = ? AND
                RDMCU = ? AND
                RDZ58DST IN ('4','5') and
                RHZ58RT NOT IN ('STKIP')
            GROUP BY
                RDZ58RID
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetStockRacks(BusinessSystemSession session, SalesOrderLines lines)
        {
            try
            {
                var cmd = session.DbCommands["OrderManager.GetStockRacks"];
                cmd.Connection = session.DbConnection;
                foreach (SalesOrderLine line in lines)
                {
                    var racks = new Racks();
                    cmd.Parameters[0].Value = line.ItemNum;
                    cmd.Parameters[1].Value = line.PlantNum.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var rackId = CastDbNull.To<decimal>(reader["RDZ58RID"], 0);
                            racks.Add(_rackMgr.GetRack(session, rackId));
                        }

                        reader.Close();
                    }

                    line.Racks = racks;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Custom Racks
          *******************************************************************************/
        public iDB2Command GetCustomRacks()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            RDZ58RID
            FROM
	            F580056
            WHERE
	            RDITM = ? AND
                RDLOTN = ? AND
                RDMCU = ? AND
                RDZ58DST IN ('4','5')
            GROUP BY
                RDZ58RID
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("LOTN", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetCustomRacks(BusinessSystemSession session, SalesOrderLines lines)
        {
            try
            {
                var cmd = session.DbCommands["OrderManager.GetCustomRacks"];
                cmd.Connection = session.DbConnection;
                foreach (SalesOrderLine line in lines)
                {
                    var racks = new Racks();
                    cmd.Parameters[0].Value = line.ItemNum;
                    cmd.Parameters[1].Value = line.LotNum;
                    cmd.Parameters[2].Value = line.PlantNum.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    { 
                        while (reader.Read())
                        {
                            var rackId = CastDbNull.To<decimal>(reader["RDZ58RID"], 0);
                            racks.Add(_rackMgr.GetRack(session, rackId));
                        }

                        reader.Close();
                    }

                    line.Racks = racks;
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
        public iDB2Command GetRelatedOrders()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                ROEDTR,
	            ROZ58L1CLN,
                ROUPMJ,
                ROMCU2
            FROM
	            F580400
            WHERE
                ROMCU = ? AND
                RODOCO = ? AND
	            RODCTO = ? AND
                ROLNID = ? AND
                ROZ58JBSTS = '30' AND
                ROEDTR = 'S'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetRelatedOrders(BusinessSystemSession session, SalesOrderLines lines)
        {
            try
            {
                var cmd = session.DbCommands["OrderManager.GetRelatedOrders"];
                cmd.Connection = session.DbConnection;
                foreach (SalesOrderLine line in lines)
                {
                    cmd.Parameters[0].Value = line.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = line.OrderNum;
                    cmd.Parameters[2].Value = line.OrderType;
                    cmd.Parameters[3].Value = line.OrderLineNum * 1000;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        line.LitesPurchased = "N";
                        while (reader.Read())
                        {
                            var liteNum = CastDbNull.To<decimal>(reader["ROZ58L1CLN"], 0);
                            var upmj = CastDbNull.To<decimal>(reader["ROUPMJ"], 0);
                            var mcu2 = CastDbNull.To(reader["ROMCU2"], string.Empty).Trim();
                            try
                            {
                                switch (liteNum)
                                {

                                    case 1:
                                        line.Lite1PurDate = E1TimeManager.CastToDateTime(upmj);
                                        line.Lite1Supplier = PlantManager.GetPlant(mcu2).PlantCode;
                                        break;
                                    case 2:
                                        line.Lite2PurDate = E1TimeManager.CastToDateTime(upmj);
                                        line.Lite2Supplier = PlantManager.GetPlant(mcu2).PlantCode;
                                        break;
                                    case 3:
                                        line.Lite3PurDate = E1TimeManager.CastToDateTime(upmj);
                                        line.Lite3Supplier = PlantManager.GetPlant(mcu2).PlantCode;
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                            }

                            if (line.UnitType == "3" && line.Lite1PurDate != null && line.Lite2PurDate != null &&
                                line.Lite3PurDate != null)
                            {
                                line.LitesPurchased = "Y";
                            }
                            else if (line.UnitType == "2" && line.Lite1PurDate != null)
                            {
                                line.LitesPurchased = "Y";
                            }
                            else if (line.Lite1PurDate != null)
                            {
                                line.LitesPurchased = "Y";
                            }
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
          * Scheduled Stock Units
          *******************************************************************************/
        public iDB2Command GetScheduledUnits()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            CAST(SUM(WAUORG/1000) AS INT) AS UORG
            FROM
	            F4801
            WHERE
                WAMCU = ? AND
                WAITM = ? AND
                WASRST = '40'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;

            return cmd;
        }

        public void GetScheduledUnits(BusinessSystemSession session, SalesOrderLines lines)
        {
            try
            {
                var cmd = session.DbCommands["OrderManager.GetScheduledUnits"];
                cmd.Connection = session.DbConnection;
                foreach (SalesOrderLine line in lines)
                {
                    cmd.Parameters[0].Value = line.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = line.ItemNum;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            line.ScheduledUnits = CastDbNull.To(reader["UORG"], 0);
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

        public iDB2Command GetCustomOrders1(List<string> filters)
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
	                    AB1.ABALPH AS SDAN8ALPH,
	                    SDSHAN,
	                    AB2.ABALPH AS SDSHANALPH,
                        SDITWT,
                        SDPPDJ,
	                    SDDRQJ,
                        SDTRDJ,
	                    SDIVD,
	                    SDLTTR,
	                    SDNXTR,
                        SDKCOO,
	                    SDDOCO,
	                    SDDCTO,
	                    SDLNID,
                        SDRORN,
                        SHVR01,
	                    SDVR01,
	                    SDURCD,
	                    SDITM,
	                    SDUORG,
                        SDUNCS,
                        SDECST,
                        SDUPRC,
                        SDAEXP,
                        SDRCD,
                        SDIR04,
                        SDDSC1,
                        SDPSN,
	                    ATZ58PT,
	                    ATZ58CP2,
	                    ATZ58CP3,
	                    ATZ58CP4,
                        ATZ58CSQNO,
	                    ATZ58LPR,
	                    ATZ58RT,
                        ATZ5840460,
                        ATZ58WODC,
	                    COALESCE(IBSTKT, IMSTKT) AS IBSTKT,
	                    COALESCE(IBGLPT, IMGLPT) AS IBGLPT,
	                    COALESCE(IBSRP4, IMSRP4) AS IBSRP4,
	                    IMDSC1,
	                    IMDSC2,
                        DRDL01,
                        SAZ58L1TK,
                        SAZ58L11C,
                        SAZ58L1CL,
                        SAZ58L1HT,
                        SAZ58L12C,
                        SAZ58L1PX,
                        SAZ5800495,
                        SAZ58L1LPT,
                        SAZ5800522,
                        SAZ5800524,
                        SAZ5800536,
                        SAZ5800537,
                        SAZ5800538,
                        SAZ5800539,
                        SAZ58SHAPE,
                        SAZ58L1BDM,
                        SAZ58L1BNM,
                        SAZ58L1BDN,
                        SAZ58L1LDM,
                        SAZ58L1LNM,
                        SAZ58L1LDN,
                        SAZ58L1RDM,
                        SAZ58L1RNM,
                        SAZ58L1RDN,
                        SAZ58L1VG,
                        SAZ58L1HL,
                        SAZ58L1EW,
                        SAZ58L1PQL,
                        SADRQJ,
                        SAZ58L1SSZ,
                        SAZ58L1STP,
                        SAZ58L2TK,
                        SAZ58L23C,
                        SAZ58L2CL,
                        SAZ58L2HT,
                        SAZ58L24C,
                        SAZ58L2PX,
                        SAZ58L2LPT,
                        SAZ58L2VG,
                        SAZ58L2HL,
                        SAZ58L3TK,
                        SAZ58L35C,
                        SAZ58L3CL,
                        SAZ58L3HT,
                        SAZ58L36C,
                        SAZ58L3PX,
                        SAZ58L3LPT,
                        SAZ58L3VG,
                        SAZ58L3HL,
                        SAZ58L3EW,
                        SAZ58M1A1,
                        SAZ58OOA1,
                        SAZ58CCA1,
                        SAZ58GSS1,
                        SAZ58RT,
                        SAZ587563,
                        SAZ587569,
                        SAZ587575,
                        SAZ587581,
                        SAZ5840275,
                        SAZ58PT,
                        SAZ5840460,
                        SAZ58USF / 10000 AS SAZ58USF,
                        SAZ58L1RTG,
                        SAZ58L2RTG,
                        SAZ58L3RTG,
                        SAAN8DR,
                        SAZ5852020,
                        SAZ5852021,
                        SAZ5852022,
                        SAZ5852060,
                        SAZ5852061,
                        SAZ5852062,
                        SAZ5852075,
                        SAZ5852076,
                        SAZ5852077,
                        SDUPMJ AS SLUPMJ,
                        SDTDAY AS SLTDAY,
                        CP1.CPZ58CCV05,
                        CP2.CPZ58GAL1,
                        LHZ58LDS
						(select CMCFGID from F3201 where CMDOCO = ATZ58WODC and CMDCTO = 'WO' order by CMUPMJ DESC FETCH FIRST 1 ROWS ONLY) AS CMCFGID
                    FROM
	                    F4211
                        INNER JOIN F4201
                        ON  SHKCOO = SDKCOO AND
                            SHDOCO = SDDOCO AND
                            SHDCTO = SDDCTO
	                    LEFT OUTER JOIN F4102 
	                    INNER JOIN F4101
	                    ON  IMITM = IBITM
	                    ON	IBMCU = SDMCU AND
		                    IBITM = SDITM
	                    INNER JOIN F0101 AB1
	                    ON	AB1.ABAN8 = SDAN8
	                    INNER JOIN F0101 AB2
	                    ON	AB2.ABAN8 = SDSHAN
	                    INNER JOIN F580032
	                    ON	ATKCOO = SDKCOO AND
		                    ATDOCO = SDDOCO AND
		                    ATDCTO = SDDCTO AND
		                    ATLNID = SDLNID 
						INNER JOIN F5834A15
							ON SADOCO = SDDOCO AND
							SADCTO = SDDCTO AND
							SALNID = SDLNID AND
							SAKCOO = SDKCOO
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005
                            ON  DRSY = '42' 
                            AND DRRT = 'RC' 
                            AND TRIM(DRKY) = SDRCD                  
                        LEFT OUTER JOIN F580375 CP1
                        ON  CP1.CPMCU = SDMCU AND
                            CP1.CPSHAN = SDSHAN
				        LEFT OUTER JOIN F580430 CP2
				        ON	CP2.CPAN8 = SDSHAN AND
                            CP2.CPMCU = SDMCU
                        LEFT OUTER JOIN F580150
                        ON  LHLDNM = SDPSN
                    WHERE
                        {filterString}
                        SDLNTY IN ('W') AND
                        SDDCTO NOT IN ('SQ') AND
                        SDGLC IN ('GLAS','GLAF','GLAX','GLAZ','LUWT','MSAL','MSCR')
                        
            ORDER BY
	            1,2
            FOR READ ONLY";
            return cmd;
        }

        public iDB2Command GetCustomOrders0(List<string> filters)
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
	            *
            FROM (
                    SELECT
	                    SDMCU,
	                    SDAN8,
	                    AB1.ABALPH AS SDAN8ALPH,
	                    SDSHAN,
	                    AB2.ABALPH AS SDSHANALPH,
                        SDITWT,
                        SDPPDJ,
	                    SDDRQJ,
                        SDTRDJ,
	                    SDIVD,
	                    SDLTTR,
	                    SDNXTR,
                        SDKCOO,
	                    SDDOCO,
	                    SDDCTO,
	                    SDLNID,
                        SDRORN,
                        SHVR01,
	                    SDVR01,
	                    SDURCD,
	                    SDITM,
	                    SDUORG,
                        SDUNCS,
                        SDECST,
                        SDUPRC,
                        SDAEXP,
                        SDRCD,
                        SDIR04,
                        SDDSC1,
                        SDPSN,
	                    ATZ58PT,
	                    ATZ58CP2,
	                    ATZ58CP3,
	                    ATZ58CP4,
                        ATZ58CSQNO,
	                    ATZ58LPR,
	                    ATZ58RT,
                        ATZ5840460,
                        ATZ58WODC,
	                    IBSTKT,
	                    IBGLPT,
                        IBSRP4,
	                    IMDSC1,
	                    IMDSC2,
                        DRDL01,
                        SAZ58L1TK,
                        SAZ58L11C,
                        SAZ58L1CL,
                        SAZ58L1HT,
                        SAZ58L12C,
                        SAZ58L1PX,
                        SAZ5800495,
                        SAZ58L1LPT,
                        SAZ5800522,
                        SAZ5800524,
                        SAZ5800536,
                        SAZ5800537,
                        SAZ5800538,
                        SAZ5800539,
                        SAZ58SHAPE,
                        SAZ58L1BDM,
                        SAZ58L1BNM,
                        SAZ58L1BDN,
                        SAZ58L1LDM,
                        SAZ58L1LNM,
                        SAZ58L1LDN,
                        SAZ58L1RDM,
                        SAZ58L1RNM,
                        SAZ58L1RDN,
                        SAZ58L1VG,
                        SAZ58L1HL,
                        SAZ58L1EW,
                        SAZ58L1PQL,
                        SADRQJ,
                        SAZ58L1SSZ,
                        SAZ58L1STP,
                        SAZ58L2TK,
                        SAZ58L23C,
                        SAZ58L2CL,
                        SAZ58L2HT,
                        SAZ58L24C,
                        SAZ58L2PX,
                        SAZ58L2LPT,
                        SAZ58L2VG,
                        SAZ58L2HL,
                        SAZ58L3TK,
                        SAZ58L35C,
                        SAZ58L3CL,
                        SAZ58L3HT,
                        SAZ58L36C,
                        SAZ58L3PX,
                        SAZ58L3LPT,
                        SAZ58L3VG,
                        SAZ58L3HL,
                        SAZ58L3EW,
                        SAZ58M1A1,
                        SAZ58OOA1,
                        SAZ58CCA1,
                        SAZ58GSS1,
                        SAZ58RT,
                        SAZ587563,
                        SAZ587569,
                        SAZ587575,
                        SAZ587581,
                        SAZ5840275,
                        SAZ58PT,
                        SAZ5840460,
                        SAZ58USF / 10000 AS SAZ58USF,
                        SAZ58L1RTG,
                        SAZ58L2RTG,
                        SAZ58L3RTG,
                        SAAN8DR,
                        SAZ5852020,
                        SAZ5852021,
                        SAZ5852022,
                        SAZ5852060,
                        SAZ5852061,
                        SAZ5852062,
                        SAZ5852075,
                        SAZ5852076,
                        SAZ5852077,
                        SDUPMJ AS SLUPMJ,
                        SDTDAY AS SLTDAY,
                        CP1.CPZ58CCV05,
                        CP2.CPZ58GAL1,
                        COALESCE(CMCFGID, 0) AS CMCFGID,
                        CAST(0 AS DECIMAL(6,0)) AS ROUPMJ,
			            --(	SELECT 
					    --        MIN(ROUPMJ) 
				        --    FROM 
					    --        F580400 where ROKCOO = SDKCOO AND
					    --        RODOCO = SDDOCO AND
					    --        RODCTO = SDDCTO AND
					    --        ROLNID = SDLNID AND
					    --        ROEDTR = 'S') AS ROUPMJ,
			            ROW_NUMBER() OVER (PARTITION BY SDDOCO, SDDCTO, SDLNID ORDER BY CMUPMJ DESC, SDUPMJ ASC, SDTDAY ASC) AS RN
                    FROM
	                    F4211
                        INNER JOIN F4201
                        ON  SHKCOO = SDKCOO AND
                            SHDOCO = SDDOCO AND
                            SHDCTO = SDDCTO
	                    INNER JOIN F4102 
	                    INNER JOIN F4101
	                    ON  IMITM = IBITM
	                    ON	IBMCU = SDMCU AND
		                    IBITM = SDITM
	                    INNER JOIN F0101 AB1
	                    ON	AB1.ABAN8 = SDAN8
	                    INNER JOIN F0101 AB2
	                    ON	AB2.ABAN8 = SDSHAN
	                    LEFT OUTER JOIN F580032
	                    ON	ATKCOO = SDKCOO AND
		                    ATDOCO = SDDOCO AND
		                    ATDCTO = SDDCTO AND
		                    ATLNID = SDLNID 
                        LEFT OUTER JOIN F3201
	                    ON	CMKCOO = SDKCOO AND
		                    CMDOCO = SDDOCO AND
		                    CMDCTO = SDDCTO AND
		                    CMLNID = SDLNID
						LEFT OUTER JOIN F5834A15
							ON SADOCO = SDDOCO AND
							SADCTO = SDDCTO AND
							SALNID = SDLNID AND
							SAKCOO = SDKCOO
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005
                            ON  DRSY = '42' 
                            AND DRRT = 'RC' 
                            AND TRIM(DRKY) = SDRCD
                        --LEFT OUTER JOIN F42199
	                    --ON	SLKCOO = SDKCOO AND
		                --    SLDOCO = SDDOCO AND
		                --    SLDCTO = SDDCTO AND
		                --    SLLNID = SDLNID                    
                        LEFT OUTER JOIN F580375 CP1
                        ON  CP1.CPMCU = SDMCU AND
                            CP1.CPSHAN = SDSHAN
				        LEFT OUTER JOIN F580430 CP2
				        ON	CP2.CPAN8 = SDSHAN AND
                            CP2.CPMCU = SDMCU
                    WHERE
                        {filterString}
                        SDLNTY = 'W' AND
                        SDDCTO NOT IN ('SQ') AND
                        SDGLC IN ('GLAS','GLAF','GLAX','GLAZ')
                        ) A
            WHERE 
	            A.RN = 1
            ORDER BY
	            1,2
            FOR READ ONLY";
            return cmd;
        }


    }
}
