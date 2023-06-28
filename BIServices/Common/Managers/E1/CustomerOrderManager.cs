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
    public class CustomerOrderManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public CustomerOrderManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("CustomerOrderManager.GetRelatedOrders", GetRelatedOrders());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
          * Current Orders
          *******************************************************************************/
        public iDB2Command GetCurrentOrders(List<string> filters)
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
	            SDDOCO,
	            SDDCTO,
	            SDLNID,                
                SDRORN,
                SDITM,
	            SDLNTY,
	            SDUORG,
	            SDLTTR,
	            SDNXTR,
	            SDTRDJ,
	            SDDRQJ,
	            SDADDJ,
                SDPPDJ,
	            SDIVD,
	            SDPSN,
	            SDECST,
	            SDAEXP,
	            IBITM,
	            IBSTKT,
	            IBGLPT,
                IBSRP4,
	            IMDSC1,
	            IMDSC2,
                ATZ58LPR,
                ATZ5840460,
	            ATZ58USF,
	            ATZ58BSF,
	            ATZ58GWTT,
	            ATZ58WTMX,
	            ATZ58RT,
	            ATZ58RKS,
	            RTZ58RTWT,
	            SAZ58L1TK,
	            UMCONV,
	            (SELECT SUM(RDZ58GWT) FROM F580056 where RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS RDZ58GWT
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
	            INNER JOIN F580032
	            ON	ATKCOO = SDKCOO AND
		            ATDOCO = SDDOCO AND
		            ATDCTO = SDDCTO AND
		            ATLNID = SDLNID 
	            INNER JOIN F580061
		            ON RTZ58RT = ATZ58RT and
		            RTMCU = SDMCU and
		            RTAN8 = 0
	            INNER JOIN F5834A15
		            ON SADOCO = SDDOCO AND
		            SADCTO = SDDCTO AND
		            SALNID = SDLNID AND
		            SAKCOO = SDKCOO
	            LEFT OUTER JOIN F41002
	            ON	UMMCU = SDMCU AND
		            UMITM = SDITM AND
		            UMUM = 'EA' and
		            UMRUM = 'LB'
            WHERE
                {filterString}
	            SDLTTR NOT IN ('980') AND
	            IBGLPT LIKE 'GL%'
            ORDER BY
	            1,2,3
            FOR READ ONLY";
            return cmd;
        }

        /*********************************************************************************
          * Archived Orders
          *******************************************************************************/
        public iDB2Command GetArchivedOrders(List<string> filters)
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
	            SDDOCO,
	            SDDCTO,
	            SDLNID,
                SDRORN,
                SDITM,
	            SDLNTY,
	            SDUORG,
	            SDLTTR,
	            SDNXTR,
	            SDTRDJ,
	            SDDRQJ,
	            SDADDJ,
                SDPPDJ,
	            SDIVD,
	            SDPSN,
	            SDECST,
	            SDAEXP,
	            IBITM,
	            IBSTKT,
	            IBGLPT,
                IBSRP4,
	            IMDSC1,
	            IMDSC2,
                ATZ58LPR,
                ATZ5840460,
	            ATZ58USF,
	            ATZ58BSF,
	            ATZ58GWTT,
	            ATZ58WTMX,
	            ATZ58RT,
	            ATZ58RKS,
	            RTZ58RTWT,
	            SAZ58L1TK,
	            UMCONV,
	            (SELECT SUM(RDZ58GWT) FROM F580056 where RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS RDZ58GWT
            FROM
	            F42119
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
	            INNER JOIN F580032
	            ON	ATKCOO = SDKCOO AND
		            ATDOCO = SDDOCO AND
		            ATDCTO = SDDCTO AND
		            ATLNID = SDLNID 
	            INNER JOIN F580061
		            ON RTZ58RT = ATZ58RT and
		            RTMCU = SDMCU and
		            RTAN8 = 0
	            INNER JOIN F5834A15
		            ON SADOCO = SDDOCO AND
		            SADCTO = SDDCTO AND
		            SALNID = SDLNID AND
		            SAKCOO = SDKCOO
	            LEFT OUTER JOIN F41002
	            ON	UMMCU = SDMCU AND
		            UMITM = SDITM AND
		            UMUM = 'EA' and
		            UMRUM = 'LB'
            WHERE
                {filterString}
	            SDLTTR NOT IN ('980') AND
	            IBGLPT LIKE 'GL%'
            ORDER BY
	            1,2,3
            FOR READ ONLY";
            return cmd;
        }


        public SalesOrderLines GetCurrentOrders(BusinessSystemSession session, string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options)
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
                cmd = GetCurrentOrders(filters);
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
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        //line.CreatedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDUPMJ"], 0), CastDbNull.To<decimal>(reader["SDTDAY"], 0));
                        line.PromiseDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDPPDJ"], 0));
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        //line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        //line.ReasonCodeDescription = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["SDAN8ALPH"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["SDSHANALPH"], string.Empty).Trim();
                        line.LitesPerRack = CastDbNull.To<decimal>(reader["ATZ58LPR"], 0);
                        line.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                        line.ItemNum = CastDbNull.To<decimal>(reader["SDITM"], 0);
                        line.ItemDesc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        line.ItemDesc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        line.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                        line.GlCategory = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                        line.HvLv = CastDbNull.To(reader["ATZ5840460"], string.Empty).Trim();
                        // Metrics
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int)Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["ATZ58USF"], 0);
                        line.TotalBillableSqft = CastDbNull.To<decimal>(reader["ATZ58BSF"], 0);
                        line.TotalGlassWeight = CastDbNull.To<decimal>(reader["RDZ58GWT"], 0) / 100;
                        line.TotalGlassPounds = CastDbNull.To<decimal>(reader["RDZ58GWT"], 0) / 100;
                        line.TotalGlassTons = line.TotalGlassPounds / 2000;
                        line.TotalCost = CastDbNull.To<decimal>(reader["SDECST"], 0) / 100;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        lines.Add(line);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.StackTrace);
            }
            return lines;
        }


        public SalesOrderLines GetArchivedOrders(BusinessSystemSession session, string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options)
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
                cmd = GetArchivedOrders(filters);
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
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        //line.CreatedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDUPMJ"], 0), CastDbNull.To<decimal>(reader["SDTDAY"], 0));
                        line.PromiseDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDPPDJ"], 0));
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        //line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        //line.ReasonCodeDescription = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["SDAN8ALPH"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["SDSHANALPH"], string.Empty).Trim();
                        //line.CustomerPart1 = CastDbNull.To(reader["ATZ58CP2"], string.Empty).Trim();
                        //line.CustomerPart2 = CastDbNull.To(reader["ATZ58CP3"], string.Empty).Trim();
                        //line.CustomerPart3 = CastDbNull.To(reader["ATZ58CP4"], string.Empty).Trim();
                        //line.CustomerPOHdr = CastDbNull.To(reader["SHVR01"], string.Empty).Trim();
                        //line.CustomerPO = CastDbNull.To(reader["SDVR01"], string.Empty).Trim();
                        //line.CustomerTier = CastDbNull.To(reader["CPZ58CCV05"], string.Empty).Trim();
                        //line.ProductType = CastDbNull.To(reader["ATZ58PT"], string.Empty).Trim();
                        //line.PriorityCode = CastDbNull.To(reader["SDURCD"], string.Empty).Trim();
                        line.LitesPerRack = CastDbNull.To<decimal>(reader["ATZ58LPR"], 0);
                        line.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                        line.ItemNum = CastDbNull.To<decimal>(reader["SDITM"], 0);
                        line.ItemDesc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        line.ItemDesc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        line.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                        line.GlCategory = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                        line.HvLv = CastDbNull.To(reader["ATZ5840460"], string.Empty).Trim();
                        // Metrics
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int)Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["ATZ58USF"], 0);
                        line.TotalBillableSqft = CastDbNull.To<decimal>(reader["ATZ58BSF"], 0);
                        line.TotalGlassWeight = CastDbNull.To<decimal>(reader["RDZ58GWT"], 0) / 100;
                        line.TotalGlassPounds = CastDbNull.To<decimal>(reader["RDZ58GWT"], 0) / 100;
                        line.TotalGlassTons = line.TotalGlassPounds / 2000;
                        //line.UnitCost = CastDbNull.To<decimal>(reader["SDUNCS"], 0) / 10000;
                        line.TotalCost = CastDbNull.To<decimal>(reader["SDECST"], 0) / 100;
                        //line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
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
