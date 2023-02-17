using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public class OpenOrderManagerIG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OpenOrderManagerIG(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("OpenOrderManager.GetScheduledRack", GetScheduledRack());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
         * Get Open Units Dynamic
         *******************************************************************************/
        public SqlCommand GetOpenOrders(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT
                sl.CustomerSoldNum,
                cs1.Name                        AS CustomerSoldName,
                sl.CustomerShipNum,
                cs2.Name                        AS CustomerShipName,
                sl.OrderNum                     AS SOrderNum,
                sl.OrderType                    AS SOrderType,
                sl.OrderLineNum                 AS SOrderLineNum,
                sl.WOrderNum,
                wo.StatusCode,
                CASE WHEN wo.StatusCode = '10' THEN NULL ELSE ord.ScheduleNum END AS ScheduleNum,
                CASE WHEN wo.StatusCode = '10' THEN NULL ELSE ord.ScheduleDate END AS ScheduleDate,
                ord.ScheduleNum,
                ord.ScheduleDate,
                sl.CustomerOrderNum AS CustomerPO,
                sl.CustomerPart1,
                sl.CustomerPart2,
                sl.CustomerPart3,
                sl.CustomerSeqNum,
                sl.Description1,
                sl.Description2,
                CAST(ord.ShapeNum AS INT) AS ShapeNum,
                ord.CardinalProductType,
                ord.MCode1_Air1,
                ord.ExtPrsvCB, 
                ord.IntPrsvCB, 
                ord.ExtPrsvScoring, 
                ord.IntPrsvScoring,
                sl.ProductType,
                sl.ReasonCode,
                sl.Priority                     AS PriorityCode,
                sl.UnitCostAmt,
                sl.UnitCostAmt * sl.Quantity    AS TotalCostAmt,
                sl.UnitPriceAmt,
                sl.UnitPriceAmt * sl.Quantity AS TotalPriceAmt,
                CAST(sl.OrderDate AS DATE)      AS OrderDate,
                CAST(sl.OrigRequestedDate AS DATE)   AS InitialRequestedDate,
                CAST(sl.RequestedDate AS DATE)  AS CurrentRequestedDate,
                sl.Quantity,
                sl.ItemNum,
                '' AS UnitTypeCode,
                sl.LastStatus,
                sl.NextStatus,
                sl.UnitActualSqft,
                gi.UnitType,
                sl.UnitActualSqft * sl.Quantity AS TotalActualSqft,
	            ScheduledUnits = CAST(ISNULL((SELECT SUM(rd.QtyRecd) FROM EBS.RackDetail rd WHERE rd.SOrderNum = sl.OrderNum AND rd.SOrderType = sl.OrderType AND rd.SOrderLineNum = sl.OrderLineNum AND rd.StatusCode = '1'), 0) AS INT), 
	            CompletedUnits = CAST(ISNULL((SELECT SUM(rd.QtyOnHand) FROM EBS.RackDetail rd WHERE rd.SOrderNum = sl.OrderNum AND rd.SOrderType = sl.OrderType AND rd.SOrderLineNum = sl.OrderLineNum AND rd.StatusCode = '4'), 0) AS INT), 
	            ClosedUnits = CAST(ISNULL((SELECT SUM(rd.QtyOnHand) FROM EBS.RackDetail rd WHERE rd.SOrderNum = sl.OrderNum AND rd.SOrderType = sl.OrderType AND rd.SOrderLineNum = sl.OrderLineNum AND rd.StatusCode = '5'), 0) AS INT),
				MinUnitId = (SELECT MIN(SerialNum) FROM EBS.RackDetail WHERE WOrderNum = sl.WOrderNum and Sl.WOrderNum > 0),
				MaxUnitId = (SELECT MAX(SerialNum) FROM EBS.RackDetail WHERE WOrderNum = sl.WOrderNum and Sl.WOrderNum > 0),
				MinUnitStatus = (SELECT MIN(StatusCode) FROM EBS.RackDetail WHERE WOrderNum = sl.WOrderNum and sl.WOrderNum > 0 ),
                MaxUnitStatus = (SELECT MAX(StatusCode) FROM EBS.RackDetail WHERE WOrderNum = sl.WOrderNum and sl.WOrderNum > 0 )
            FROM
                EBS.SalesLine                        sl
                LEFT OUTER JOIN EBS.AddressBook         cs1
                ON  cs1.AddressNum = sl.CustomerSoldNum
                LEFT OUTER JOIN EBS.AddressBook         cs2
                ON  cs2.AddressNum = sl.CustomerShipNum
                LEFT OUTER JOIN dbo.IncomingOrders ord
                ON  ord.SOrderNum = sl.OrderNum AND
                    ord.SOrderType = sl.OrderType AND
                    ord.SOrderLine = sl.OrderLineNum 
		        LEFT OUTER JOIN EBS.WorkOrder wo
		        ON wo.OrderNum = sl.WOrderNum
                LEFT OUTER JOIN EBS.GlassItem gi
                oN gi.ItemNum = sl.ItemNum
            WHERE
               {filterString}
                sl.NextStatus <= '545'
            ORDER BY
                cs1.Name,
                sl.RequestedDate,
                sl.OrderNum,
                sl.OrderLineNum";
            return cmd;
        }


        public SalesOrderLines GetOpenOrders(BusinessSystemSession session, SqlConnection cnShopFloor, Plant plant, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
        {
            var lines = new SalesOrderLines();
            var cmd = new SqlCommand();
            List<String> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {
                // Dates
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) ||
                    !string.IsNullOrEmpty(dateProperty) || !string.IsNullOrEmpty(dateCode))
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
                                filters.Add("sl.CurrentRequestedDate >= ? AND\r\n");
                                filters.Add("sl.CurrentRequestedDate <= ? AND\r\n");
                                break;
                            case "ORD":
                            case "ORDER":
                            case "ORDERED":
                                filters.Add("sl.OrderDate >= ? AND\r\n");
                                filters.Add("sl.OrderDate <= ? AND\r\n");
                                break;
                            default:
                                filters.Add("sl.OrderDate >= ? AND\r\n");
                                filters.Add("sl.OrderDate <= ? AND\r\n");
                                break;
                        }
                    }
                    else
                    {
                        filters.Add("sl.CurrentRequestedDate >= ? AND\r\n");
                        filters.Add("sl.CurrentRequestedDate <= ? AND\r\n");
                    }

                    parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date));
                    parameters.Last().Value = startDateValue;

                    parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date));
                    parameters.Last().Value = endDateValue;

                }

                // Customer Ship Num
                if (!String.IsNullOrEmpty(customerShipNums))
                {
                    var exclude = false;
                    List<decimal> customerShipNumList;
                    if (customerShipNums.Substring(0, 1) == "!")
                    {
                        exclude = true;
                        customerShipNumList = Array
                            .ConvertAll<string, decimal>(
                                customerShipNums.Substring(1, customerShipNums.Length - 1).Split(','),
                                Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerShipNumList =
                            Array.ConvertAll<string, decimal>(customerShipNums.Split(','), Convert.ToDecimal)
                                .ToList();
                    }

                    string strInClause = "";
                    foreach (decimal customerShip in customerShipNumList)
                    {
                        strInClause = strInClause + $"{customerShip.ToString()},";
                    }

                    filters.Add(
                        $"sl.CustomerShipNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                // Customer Sold Num
                if (!String.IsNullOrEmpty(customerSoldNums))
                {
                    var exclude = false;
                    List<decimal> customerSoldNumList;
                    if (customerSoldNums.Substring(0, 1) == "!")
                    {
                        exclude = true;
                        customerSoldNumList = Array
                            .ConvertAll<string, decimal>(
                                customerSoldNums.Substring(1, customerSoldNums.Length - 1).Split(','),
                                Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerSoldNumList =
                            Array.ConvertAll<string, decimal>(customerSoldNums.Split(','), Convert.ToDecimal)
                                .ToList();
                    }

                    string strInClause = "";
                    foreach (decimal customerSold in customerSoldNumList)
                    {
                        strInClause = strInClause + $"{customerSold.ToString()},";
                    }

                    filters.Add(
                        $"sl.CustomerSoldNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                cmd = GetOpenOrders(filters);

                foreach (SqlParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = cnShopFloor;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = plant.PlantNum;
                        line.PlantCode = plant.PlantCode;
                        line.WorkOrderNum = CastDbNull.To<decimal>(reader["WOrderNum"], 0);
                        line.MinUnitId = CastDbNull.To(reader["MinUnitId"], string.Empty).Trim();
                        line.MaxUnitId = CastDbNull.To(reader["MaxUnitId"], string.Empty).Trim();
                        line.MinUnitStatus = CastDbNull.To(reader["MinUnitStatus"], string.Empty).Trim();
                        line.MaxUnitStatus = CastDbNull.To(reader["MaxUnitStatus"], string.Empty).Trim();
                        //line.LotNum = line.WorkOrderNum.ToString(CultureInfo.InvariantCulture);
                        //line.CreatedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SLUPMJ"], 0), CastDbNull.To<decimal>(reader["SLTDAY"], 0));
                        line.OrderDate = CastDbNull.To<DateTime>(reader["OrderDate"], new DateTime(1900, 1, 1));
                        line.InitialRequestedDate = CastDbNull.To<DateTime>(reader["InitialRequestedDate"], new DateTime(1900, 1, 1));
                        line.RequestedDate =
                            CastDbNull.To<DateTime>(reader["CurrentRequestedDate"], new DateTime(1900, 1, 1));
                        line.ScheduleDate = CastDbNull.To<DateTime>(reader["ScheduleDate"], new DateTime(1900, 1, 1));
                        line.ReasonCode = CastDbNull.To(reader["ReasonCode"], string.Empty).Trim();
                        //line.ReasonCodeDescription = CastDbNull.To(reader["DRDL01"], string.Empty).Trim();
                        //line.PurchaseDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["ROUPMJ"], 0));
                        //line.OrderCompany = CastDbNull.To(reader["SDKCOO"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SOrderNum"], 0);
                        line.OrderType = CastDbNull.To(reader["SOrderType"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SOrderLineNum"], 0);
                        line.LastStatus = CastDbNull.To(reader["LastStatus"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["NextStatus"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["CustomerSoldNum"], 0);
                        line.CustomerName = CastDbNull.To(reader["CustomerSoldName"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["CustomerShipNum"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["CustomerShipName"], string.Empty).Trim();
                        line.CustomerPart1 = CastDbNull.To(reader["CustomerPart1"], string.Empty).Trim();
                        line.CustomerPart2 = CastDbNull.To(reader["CustomerPart2"], string.Empty).Trim();
                        line.CustomerPart3 = CastDbNull.To(reader["CustomerPart3"], string.Empty).Trim();
                        line.CustomerSequenceNum = CastDbNull.To(reader["CustomerSeqNum"], string.Empty).Trim();
                        line.Description1 = CastDbNull.To(reader["Description1"], string.Empty).Trim();
                        line.Description2 = CastDbNull.To(reader["Description2"], string.Empty).Trim();
                        line.ShapeNum = CastDbNull.To<int>(reader["ShapeNum"], 0);
                        line.MCode = CastDbNull.To<decimal>(reader["MCode1_Air1"], 0);

                        line.ExtPrsvCB = CastDbNull.To<string>(reader["ExtPrsvCB"], String.Empty);
                        line.IntPrsvCB = CastDbNull.To<string>(reader["IntPrsvCB"], String.Empty);
                        line.ExtPrsvScoring = CastDbNull.To<decimal>(reader["ExtPrsvScoring"], 0);
                        line.IntPrsvScoring = CastDbNull.To<decimal>(reader["IntPrsvScoring"], 0);

                        //line.CustomerPOHdr = CastDbNull.To(reader["SHVR01"], string.Empty).Trim();
                        line.CustomerPO = CastDbNull.To(reader["CustomerPO"], string.Empty).Trim();
                        line.ProductType = CastDbNull.To(reader["ProductType"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["Quantity"], 0);
                        line.UnitCost = CastDbNull.To<decimal>(reader["UnitCostAmt"], 0);
                        line.TotalCost = CastDbNull.To<decimal>(reader["TotalCostAmt"], 0);
                        line.UnitPrice = CastDbNull.To<decimal>(reader["UnitPriceAmt"], 0);
                        line.TotalPrice = CastDbNull.To<decimal>(reader["TotalPriceAmt"], 0);
                        line.PriorityCode = CastDbNull.To(reader["PriorityCode"], string.Empty).Trim();
                        //line.LitesPerRack = CastDbNull.To<decimal>(reader["ATZ58LPR"], 0);
                        //line.UnitType = CastDbNull.To(reader["UnitType"], string.Empty).Trim();
                        line.ItemNum = CastDbNull.To<decimal>(reader["ItemNum"], 0);
                        line.UnitType = CastDbNull.To(reader["UnitType"], string.Empty).Trim();
                        //line.ItemDesc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        //line.ItemDesc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        //line.StockType = CastDbNull.To(reader["StockType"], string.Empty).Trim();
                        //line.GlCategory = CastDbNull.To(reader["GlCategory"], string.Empty).Trim();
                        //line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int)Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
                        //line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        //line.HvLv = CastDbNull.To(reader["ATZ5840460"], string.Empty).Trim();
                        line.ScheduleNum = CastDbNull.To<decimal>(reader["ScheduleNum"], 0);
                        line.UnitActualSqft = CastDbNull.To<decimal>(reader["UnitActualSqft"], 0);
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["TotalActualSqft"], 0);
                        line.ScheduledUnits = CastDbNull.To<int>(reader["ScheduledUnits"], 0);
                        line.CompletedUnits = CastDbNull.To<int>(reader["CompletedUnits"], 0);
                        line.ClosedUnits = CastDbNull.To<int>(reader["ClosedUnits"], 0);
                        lines.Add(line);
                    }

                    reader.Close();
                }
                _logger.Info("Lines Returned: " + lines.Count);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            _logger.Info("Return Lines");
            return lines;
        }

        /*********************************************************************************
         * Requested Date
         *******************************************************************************/
        public SqlCommand GetRequestedDate()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              MAX(CAL_DATE) AS CAL_DATE
            FROM
              DW.CALENDAR_DATE_TYPE CDT
              INNER JOIN DW.CALENDAR_DATE_ATTRIBUTES CDA
              ON CDA.CAL_DATE_TYPE_ID = CDT.DATE_TYPE_ID
            WHERE
              CDT.DATE_TYPE_CD = @DateCode;";
            cmd.Parameters.Add(new SqlParameter("@DateCode", SqlDbType.VarChar, 30));
            return cmd;
        }

        public DateTime GetRequestedDate(string code, SqlConnection cn)
        {
            var dt = new DateTime(1900, 1, 1);
            try
            {
                var cmd = GetRequestedDate();
                cmd.Connection = cn;
                cmd.Parameters[0].Value = code;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        dt = CastDbNull.To<DateTime>(reader["CAL_DATE"], new DateTime(1900, 1, 1));
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return dt;
        }



    }
}



