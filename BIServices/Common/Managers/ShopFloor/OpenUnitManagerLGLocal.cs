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
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public class OpenUnitManagerLGLocal
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OpenUnitManagerLGLocal(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("OpenUnitManagerLGLocal.GetScheduledRack", GetScheduledRack());
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
        public SqlCommand GetOpenUnits(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT * FROM (
            SELECT
                rk.RackId,
                rk.RackType,
                rk.ScheduleNum,
                rk.BoxNum,
                rk.StatusCode AS RackStatusCode,
                rk.GlassWeight AS RackGlassWeight,
                rk.TareWeight AS RackTareWeight,
                rk.AssetId,
                ri.LotNum,
                ri.SerialNum,
                ri.ItemNum,
                ri.StatusCode AS RackItemStatusCode,
                ri.QtyOnHand,
                ri.Slot,
                ri.Position,
                ri.Side,
                ri.WorkOrderNum,
                ri.TransactionDate as RackItemDate,                
                ord.SystemName,
                ord.OrderDate,
                ord.SalesOrderNum,
                ord.SalesOrderType,
                ord.SalesOrderLineNum,
                --ord.RequestedDate,
	            sl.RequestedDate,
                sl.ShipDate,
                sl.[Priority],
                ord.SoldToCustNum,
                ord.SoldToCustName,
                ord.ShipToCustNum,
                ord.ShipToCustName,
                ord.PriorityCode,
                ord.CustomerPart2,
                ord.CustomerPart3,
                ord.CustomerPart4,
                ord.CustomerPO,
                ord.CustomerSeqNum,
                ord.GenericSeqNum,
                ord.ProductType,
                ord.CardinalProductType,
                ord.ProductCode,
                ord.ShapeNum,
                ord.Dimension,
                ord.UnitTypeCode,
                wo.StatusCode AS WorkOrderStatusCode,
                pu.OriginalScheduleNum,
                pu.OriginalScheduleDate,
                pu.CurrentScheduleNum,
                pu.CurrentScheduleDate,
                pu.Schedules,
                tsu.MfgSource
            FROM
                EBS.ScheduledOrders1 ord WITH (NOLOCK)
                INNER JOIN EBS.RackItem ri WITH (NOLOCK)
                ON  ri.WorkOrderNum = ord.WorkOrderNum
                INNER JOIN EBS.Rack rk WITH (NOLOCK)
                ON  rk.RackId = ri.RackId
                LEFT OUTER JOIN EBS.SalesLine sl WITH (NOLOCK)
                ON  sl.OrderNum = ri.DocumentNum AND
                    sl.OrderType = ri.DocumentType AND
                    sl.OrderLineNum = ri.DocumentLineNum
                LEFT OUTER JOIN EBS.WorkOrder wo WITH (NOLOCK)
                ON  wo.OrderNum = ri.WorkOrderNum
                LEFT OUTER JOIN EBS.vw_ProdUnit pu WITH (NOLOCK)
                ON  pu.WorkOrderNumber = ri.WorkOrderNum AND
                    pu.SerialNumber = TRY_CAST(ri.SerialNum AS BIGINT)
                LEFT OUTER JOIN ProductionLG.dbo.vw_CurrentUnits            CU
	            ON  CU.UnitId = pu.SerialNumber
                LEFT OUTER JOIN  ProductionLG.dbo.t_ScheduleUnits tsu
                ON  CU.ProductionUnitId = tsu.ProductionUnitId AND
                    tsu.OriginalScheduleNumber = pu.OriginalScheduleNum
            ) A
            WHERE
                {filterString}
                NOT(A.RackStatusCode = '0' AND A.RackId > 100)
            ORDER BY 
                SerialNum;";
            return cmd;
        }

        public SqlCommand GetOpenUnitsOCLG(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT * FROM (
            SELECT
                rk.RackId,
                rk.RackType,
                rk.ScheduleNum,
                rk.BoxNum,
                rk.StatusCode AS RackStatusCode,
                rk.GlassWeight AS RackGlassWeight,
                rk.TareWeight AS RackTareWeight,
                rk.AssetId,
                ri.LotNum,
                ri.SerialNum,
                ri.ItemNum,
                ri.StatusCode AS RackItemStatusCode,
                ri.QtyOnHand,
                ri.Slot,
                ri.Position,
                ri.Side,
                ri.WorkOrderNum,
                ri.TransactionDate as RackItemDate,                
                ord.SystemName,
                ord.OrderDate,
                ord.SalesOrderNum,
                ord.SalesOrderType,
                ord.SalesOrderLineNum,
                --ord.RequestedDate,
	            sl.RequestedDate,
                sl.ShipDate,
                sl.[Priority],
                ord.SoldToCustNum,
                ord.SoldToCustName,
                ord.ShipToCustNum,
                ord.ShipToCustName,
                ord.PriorityCode,
                ord.CustomerPart2,
                ord.CustomerPart3,
                ord.CustomerPart4,
                ord.CustomerPO,
                ord.CustomerSeqNum,
                ord.GenericSeqNum,
                ord.ProductType,
                ord.CardinalProductType,
                ord.ProductCode,
                ord.ShapeNum,
                ord.Dimension,
                ord.UnitTypeCode,
                wo.StatusCode AS WorkOrderStatusCode,
                pu.OriginalScheduleNum,
                pu.OriginalScheduleDate,
                pu.CurrentScheduleNum,
                pu.CurrentScheduleDate,
                pu.Schedules,
                tsu.MfgSource
            FROM
                EBS.ScheduledOrders1 ord WITH (NOLOCK)
                INNER JOIN EBS.RackItem ri WITH (NOLOCK)
                ON  ri.WorkOrderNum = ord.WorkOrderNum
                INNER JOIN EBS.Rack rk WITH (NOLOCK)
                ON  rk.RackId = ri.RackId
                LEFT OUTER JOIN EBS.SalesLine sl WITH (NOLOCK)
                ON  sl.OrderNum = ri.DocumentNum AND
                    sl.OrderType = ri.DocumentType AND
                    sl.OrderLineNum = ri.DocumentLineNum
                LEFT OUTER JOIN EBS.WorkOrder wo WITH (NOLOCK)
                ON  wo.OrderNum = ri.WorkOrderNum
                LEFT OUTER JOIN EBS.vw_ProdUnit pu WITH (NOLOCK)
                ON  pu.WorkOrderNumber = ri.WorkOrderNum AND
                    pu.SerialNumber = TRY_CAST(ri.SerialNum AS BIGINT)
                LEFT OUTER JOIN ProductionLG.dbo.vw_CurrentUnits            CU
	            ON  CU.UnitId = pu.SerialNumber
                LEFT OUTER JOIN  ProductionLG.dbo.t_ScheduleUnits tsu
                ON  CU.ProductionUnitId = tsu.ProductionUnitId AND
                    tsu.OriginalScheduleNumber = pu.OriginalScheduleNum
            ) A
            WHERE
                {filterString}
                NOT(A.RackStatusCode = '0' AND A.RackId > 100)
            ORDER BY 
                SerialNum;";
            return cmd;
        }

        public SqlCommand GetOpenUnitsNELG(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT * FROM (
            SELECT
                rk.RackId,
                rk.RackType,
                rk.ScheduleNum,
                rk.BoxNum,
                rk.StatusCode      AS RackStatusCode,
                rk.GlassWeight     AS RackGlassWeight,
                rk.TareWeight      AS RackTareWeight,
                rk.AssetId,
                ri.LotNum,
                ri.SerialNum,
                ri.ItemNum,
                ri.StatusCode      AS RackItemStatusCode,
                ri.QtyOnHand,
                ri.Slot,
                ri.Position,
                ri.Side,
                ri.WOrderNum       AS WorkOrderNum,
                ri.TransactionDate AS RackItemDate,
                ord.SystemName,
                ord.OrderDate,
                ord.SalesOrderNum,
                ord.SalesOrderType,
                ord.SalesOrderLineNum,
                sl.RequestedDate,
                sl.ShipDate,
                sl.Priority,
                ord.SoldToCustNum,
                ord.SoldToCustName,
                ord.ShipToCustNum,
                ord.ShipToCustName,
                ord.PriorityCode,
                ord.CustomerPart2,
                ord.CustomerPart3,
                ord.CustomerPart4,
                ord.CustomerPO,
                ord.CustomerSeqNum,
                ord.GenericSeqNum,
                ord.ProductType,
                ord.CardinalProductType,
                ord.ProductCode,
                ord.ShapeNum,
                ord.Dimension,
                ord.UnitTypeCode,
                wo.StatusCode      AS WorkOrderStatusCode,
                pu.OriginalScheduleNum,
                pu.OriginalScheduleDate,
                pu.CurrentScheduleNum,
                pu.CurrentScheduleDate,
                pu.Schedules,
                tsu.MfgSource
            FROM
                EBS.ScheduledOrders                                   ord WITH (NOLOCK)
                INNER JOIN EBS.RackDetail                             ri WITH (NOLOCK)
                ON  ri.LotNum = CAST(ord.WorkOrderNum AS VARCHAR(20))
                INNER JOIN EBS.Rack                                   rk WITH (NOLOCK)
                ON  rk.RackId = ri.RackId
                INNER JOIN EBS.WorkOrder                              wo WITH (NOLOCK)
                ON  wo.OrderNum = ri.WOrderNum
                INNER JOIN EBS.vw_ProdUnit                            pu WITH (NOLOCK)
                ON  pu.WorkOrderNumber = ri.WOrderNum AND
                    pu.SerialNumber = TRY_CAST(ri.SerialNum AS BIGINT)
                INNER JOIN EBS.SalesLine                         sl WITH (NOLOCK)
                ON  sl.OrderNum = ri.SOrderNum AND
                    sl.OrderType = ri.SOrderType AND
                    sl.OrderLineNum = ri.SOrderLineNum
                LEFT OUTER JOIN ProductionLG.dbo.t_Schedule           sch WITH (NOLOCK)
                ON  sch.ScheduleNumber = pu.CurrentScheduleNum AND
                    CAST(sch.ScheduleDate AS DATE) = CAST(pu.CurrentScheduleDate AS DATE)
                LEFT OUTER JOIN ProductionLG.dbo.t_ScheduleOrders     sord WITH (NOLOCK)
                ON  sord.ScheduleId = sch.ScheduleId AND
                    sord.OrderNumber = ord.SalesOrderNum AND
                    sord.OrderType = ord.SalesOrderType
                LEFT OUTER JOIN ProductionLG.dbo.t_ScheduleOrderLines sol WITH (NOLOCK)
                ON  sol.WorkOrderNumber = ri.WOrderNum AND
                    sol.ScheduleOrderId = sord.ScheduleOrderId
                LEFT OUTER JOIN ProductionLG.dbo.t_ScheduleUnits      tsu WITH (NOLOCK)
                ON  tsu.ScheduleOrderLineId = sol.ScheduleOrderLineId AND
                    tsu.UnitId = pu.SerialNumber
			WHERE
				ord.SystemName <> 'IG MTS' AND
				NOT(ri.StatusCode = '0' AND rk.RackId > 100)
            ) A
            WHERE
                {filterString}
                1=1
            ORDER BY 
                SerialNum;";
            return cmd;
        }

        public SqlCommand GetOpenUnitsAMLG(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT * FROM (
            SELECT
                rk.RackId,
                rk.RackType,
                rk.ScheduleNum,
                rk.BoxNum,
                rk.StatusCode      AS RackStatusCode,
                rk.GlassWeight     AS RackGlassWeight,
                rk.TareWeight      AS RackTareWeight,
                rk.AssetId,
                ri.LotNum,
                ri.SerialNum,
                ri.ItemNum,
                ri.StatusCode      AS RackItemStatusCode,
                ri.QtyOnHand,
                ri.Slot,
                ri.Position,
                ri.Side,
                ri.WOrderNum       AS WorkOrderNum,
                ri.TransactionDate AS RackItemDate,
                ord.SystemName,
                ord.OrderDate,
                ord.SalesOrderNum,
                ord.SalesOrderType,
                ord.SalesOrderLineNum,
                sl.RequestedDate,
                sl.ShipDate,
                sl.Priority,
                ord.SoldToCustNum,
                ord.SoldToCustName,
                ord.ShipToCustNum,
                ord.ShipToCustName,
                ord.PriorityCode,
                ord.CustomerPart2,
                ord.CustomerPart3,
                ord.CustomerPart4,
                ord.CustomerPO,
                ord.CustomerSeqNum,
                ord.GenericSeqNum,
                ord.ProductType,
                ord.CardinalProductType,
                ord.ProductCode,
                ord.ShapeNum,
                ord.Dimension,
                ord.UnitTypeCode,
                wo.StatusCode      AS WorkOrderStatusCode,
                pu.OriginalScheduleNum,
                pu.OriginalScheduleDate,
                pu.CurrentScheduleNum,
                pu.CurrentScheduleDate,
                pu.Schedules,
                pu.StatusPointName AS MfgSource
            FROM
                EBS.ScheduledOrders                                   ord WITH (NOLOCK)
                INNER JOIN EBS.RackDetail                             ri WITH (NOLOCK)
                ON  ri.LotNum = CAST(ord.WorkOrderNum AS VARCHAR(20))
                INNER JOIN EBS.Rack                                   rk WITH (NOLOCK)
                ON  rk.RackId = ri.RackId
                INNER JOIN EBS.WorkOrder                              wo WITH (NOLOCK)
                ON  wo.OrderNum = ri.WOrderNum
                INNER JOIN EBS.vw_ProdUnit                            pu WITH (NOLOCK)
                ON  pu.WorkOrderNumber = ri.WOrderNum AND
                    pu.SerialNumber = TRY_CAST(ri.SerialNum AS BIGINT)
                INNER JOIN EBS.SalesLine                         sl WITH (NOLOCK)
                ON  sl.OrderNum = ri.SOrderNum AND
                    sl.OrderType = ri.SOrderType AND
                    sl.OrderLineNum = ri.SOrderLineNum
			WHERE
				ord.SystemName <> 'IG MTS' AND
				NOT(ri.StatusCode = '0' AND rk.RackId > 100)
            ) A
            WHERE
                {filterString}
                1=1
            ORDER BY 
                SerialNum;";
            return cmd;
        }

 

        public GlassUnits GetOpenUnits(BusinessSystemSession session,  SqlConnection cnShopFloor, SqlConnection cnDw, Plant plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null,
            List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, List<decimal> rackIdsList = null)
        {
            var units = new GlassUnits();
            var cmd = new SqlCommand();
            List<String> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {
                // Date Range
                DateTime startDateDt = new DateTime(1900, 1, 1);
                if (!(startDate == null || startDate == DateTime.MinValue))
                {
                    startDateDt = (DateTime)startDate;
                }
                DateTime endDateDt = DateTime.Now.Date;
                if (!String.IsNullOrEmpty(maxDateCode))
                {
                    maxDateCode = maxDateCode.Replace(",", "").Replace("'", "");
                    try
                    {
                        endDateDt = DateTime.Parse(maxDateCode);
                    }
                    catch (FormatException)
                    {
                        endDateDt = GetRequestedDate(maxDateCode, cnDw);
                    }
                    if (maxDateCode.ToUpper() == "NBDAY")
                    {
                        endDateDt = CalendarManager.GetNextBusinessDate(session, plant, "", plant.OrderCalendarName, DateTime.Now.Date);
                    }
                }
                else
                {
                    if (!(endDate == null || endDate == DateTime.MaxValue))
                    {
                        endDateDt = (DateTime)endDate;
                    }
                }

                // Schedules
                if (scheduleNumList.Count > 0)
                {
                    filters.Add("RackItemStatusCode IN ('0', '1', '2', '3', '4', '5', '9') AND\r\n");
                    string strInClause = "";
                    if (scheduleNumList[0] < 2000)
                    {
                        foreach (decimal scheduleNum in scheduleNumList)
                        {
                            strInClause = strInClause + $"{scheduleNum.ToString()},";
                        }
                        filters.Add("CurrentScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                    }
                    else
                    {
                        foreach (decimal scheduleNum in scheduleNumList)
                        {
                            strInClause = strInClause + $"{scheduleNum.ToString()},";
                        }
                        filters.Add("OriginalScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                    }
                }

                // Rack Ids
                if (rackIdsList.Count > 0)
                {
                    filters.Add("RackItemStatusCode IN ('0', '1', '2', '3', '4', '5', '9') AND\r\n");
                    string strInClause = "";
                    foreach (decimal rackId in rackIdsList)
                    {
                        strInClause = strInClause + $"{rackId.ToString()},";
                    }
                    filters.Add("RackId IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }


                if (rackIdsList.Count == 0 && scheduleNumList.Count == 0)
                {
                    // Base Query
                    filters.Add("RackItemStatusCode IN ('0', '1', '2', '3', '4', '5', '9') AND RequestedDate >= @RequestedDateStart AND RequestedDate <= @RequestedDateEnd AND\r\n");
                    parameters.Add(new SqlParameter("@RequestedDateStart", SqlDbType.DateTime));
                    parameters.Last().Value = startDateDt;
                    parameters.Add(new SqlParameter("@RequestedDateEnd", SqlDbType.DateTime));
                    parameters.Last().Value = endDateDt;

                    // Order Date
                    if (!String.IsNullOrEmpty(orderDateCode))
                    {
                        var orderDate = new DateTime(1900, 1, 1);
                        try
                        {
                            orderDate = DateTime.Parse(orderDateCode);
                        }
                        catch (FormatException)
                        {
                            orderDate = GetRequestedDate(orderDateCode, cnDw);
                        }

                        filters.Add("OrderDate <= ? AND\r\n");
                        parameters.Add(new SqlParameter("@OrderDate", SqlDbType.DateTime));
                        parameters.Last().Value = orderDate;
                    }

                    // Customer
                    if (custNum > 0)
                    {
                        filters.Add("SoldToCustNum = ? AND\r\n");
                        parameters.Add(new SqlParameter("@SoldToCustNum", SqlDbType.Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = custNum;
                    }

                    // Customer Ship Num
                    if (!String.IsNullOrEmpty(customerShipNum))
                    {
                        var exclude = false;
                        List<decimal> customerShipNumList;
                        if (customerShipNum.Substring(0, 1) == "!")
                        {
                            exclude = true;
                            customerShipNumList = Array
                                .ConvertAll<string, decimal>(
                                    customerShipNum.Substring(1, customerShipNum.Length - 1).Split(','),
                                    Convert.ToDecimal)
                                .ToList();
                        }
                        else
                        {
                            customerShipNumList =
                                Array.ConvertAll<string, decimal>(customerShipNum.Split(','), Convert.ToDecimal)
                                    .ToList();
                        }

                        string strInClause = "";
                        foreach (decimal customerShip in customerShipNumList)
                        {
                            strInClause = strInClause + $"{customerShip.ToString()},";
                        }

                        filters.Add(
                            $"ShipToCustNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                    }

                    // Customer Sold Num
                    if (!String.IsNullOrEmpty(customerSoldNum))
                    {
                        var exclude = false;
                        List<decimal> customerSoldNumList;
                        if (customerSoldNum.Substring(0, 1) == "!")
                        {
                            exclude = true;
                            customerSoldNumList = Array
                                .ConvertAll<string, decimal>(
                                    customerShipNum.Substring(1, customerSoldNum.Length - 1).Split(','),
                                    Convert.ToDecimal)
                                .ToList();
                        }
                        else
                        {
                            customerSoldNumList =
                                Array.ConvertAll<string, decimal>(customerSoldNum.Split(','), Convert.ToDecimal)
                                    .ToList();
                        }

                        string strInClause = "";
                        foreach (decimal customerSold in customerSoldNumList)
                        {
                            strInClause = strInClause + $"{customerSold.ToString()},";
                        }

                        filters.Add(
                            $"SoldToCustNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                    }

                    // Box Num
                    if (boxNum > 0)
                    {
                        filters.Add("BoxNum = ? AND\r\n");
                        parameters.Add(new SqlParameter("@BoxNum", SqlDbType.Decimal));
                        parameters.Last().Precision = 5;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = boxNum;
                    }

                    // Rack Type
                    if (!String.IsNullOrEmpty(rackType))
                    {
                        filters.Add("RackType = ? AND\r\n");
                        parameters.Add(new SqlParameter("@RackType", SqlDbType.VarChar, 10));
                        parameters.Last().Value = rackType;
                    }

                    // Schedule Type
                    //if (!String.IsNullOrEmpty(scheduleType))
                    //{
                    //    filters.Add("ScheduleType = ? AND\r\n");
                    //    parameters.Add(new SqlParameter("@ScheduleType", SqlDbType.VarChar, 10));
                    //    parameters.Last().Value = scheduleType;
                    //}

                    // Unit Type
                    if (unitTypeList.Count() > 0)
                    {
                        string strInClause = "";
                        foreach (string unitType in unitTypeList)
                        {
                            if (unitType == "MONO")
                            {
                                strInClause = strInClause + $"'MONO GLASS',";
                            }

                            if (unitType == "LAMI")
                            {
                                strInClause = strInClause + $"'MONO LAMI',";
                            }
                        }

                        filters.Add(
                            "UnitTypeCode IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                    }

                    // Rack Status
                    if (rackStatusList.Count > 0)
                    {
                        string strInClause = "";
                        foreach (string rackStatus in rackStatusList)
                        {
                            strInClause = strInClause + $"'{rackStatus}',";
                        }

                        filters.Add("RackItemStatusCode IN (" + strInClause.Substring(0, strInClause.Length - 1) +
                                    ") AND\r\n");
                    }
                }

                switch (plant.PlantCode)
                {
                    case "AMLG":
                    {
                        cmd = GetOpenUnitsAMLG(filters);
                        break;
                    }
                    case "NELG":
                    {
                        cmd = GetOpenUnitsNELG(filters);
                        break;
                    }
                    case "OCLG":
                    {
                        cmd = GetOpenUnitsOCLG(filters);
                        break;
                    }
                    default:
                    {
                        cmd = GetOpenUnits(filters);
                        break;
                    }
                }

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
                        var unit = new GlassUnit();
                        unit.Plant = plant.PlantNum;
                        unit.UnitIdChar = CastDbNull.To<String>(reader["SerialNum"], String.Empty).Trim();
                        unit.UnitId = Utilities.Data.CastStringToDecimal(unit.UnitIdChar);
                        unit.RackId = CastDbNull.To<Decimal>(reader["RackId"], 0);
                        unit.AssetId = CastDbNull.To<String>(reader["AssetId"], String.Empty).Trim();
                        unit.RackType = CastDbNull.To<String>(reader["RackType"], String.Empty).Trim();
                        unit.RackSide = CastDbNull.To<String>(reader["Side"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<Decimal>(reader["BoxNum"], 0);
                        unit.SlotNum = Convert.ToDecimal(CastDbNull.To<Int16>(reader["Slot"], 0));
                        if (unit.SlotNum == 0)
                        {
                            unit.SlotNum = Convert.ToDecimal(CastDbNull.To<Int16>(reader["Position"], 0));
                        }

                        unit.RackTareWeight = CastDbNull.To<Decimal>(reader["RackTareWeight"], 0) / 100;
                        unit.RackStatus = CastDbNull.To<String>(reader["RackStatusCode"], String.Empty).Trim();
                        unit.Status = CastDbNull.To<String>(reader["RackItemStatusCode"], String.Empty).Trim();

                        unit.ScheduleNum = CastDbNull.To<Decimal>(reader["OriginalScheduleNum"], 0);
                        unit.ScheduleDate = CastDbNull.To<DateTime>(reader["OriginalScheduleDate"], DateTime.MinValue);
                        var currentScheduleNum = CastDbNull.To<Decimal>(reader["CurrentScheduleNum"], 0);
                        if (currentScheduleNum < 3000)
                        {
                            unit.RemakeScheduleNum = CastDbNull.To<Decimal>(reader["CurrentScheduleNum"], 0);
                            unit.RemakeInitiateDate = CastDbNull.To<DateTime>(reader["CurrentScheduleDate"], DateTime.MinValue);
                        }

                        unit.OrderNum = CastDbNull.To<Decimal>(reader["SalesOrderNum"], 0);
                        unit.OrderType = CastDbNull.To<String>(reader["SalesOrderType"], String.Empty).Trim();
                        unit.OrderLineNum = Math.Round(CastDbNull.To<Decimal>(reader["SalesOrderLineNum"], 0), 3);
                        unit.WorkOrderNum = CastDbNull.To<Decimal>(reader["WorkOrderNum"], 0);
                        unit.WorkOrderStatus = CastDbNull.To<String>(reader["WorkOrderStatusCode"], String.Empty).Trim();
                        unit.ItemNum = CastDbNull.To<Decimal>(reader["ItemNum"], 0);
                        unit.ProductType = CastDbNull.To<String>(reader["ProductType"], String.Empty).Trim();
                        unit.OrderDate = CastDbNull.To<DateTime>(reader["OrderDate"], DateTime.MinValue);
                        unit.RackItemDate = CastDbNull.To<DateTime>(reader["RackItemDate"], DateTime.MinValue).AddHours(plant.TimeOffset);
                        unit.RequestedDate = CastDbNull.To<DateTime>(reader["RequestedDate"], DateTime.MinValue);
                        unit.CustomerNum = CastDbNull.To<Decimal>(reader["SoldToCustNum"], 0);
                        unit.CustomerName = CastDbNull.To<String>(reader["SoldToCustName"], String.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["ShipToCustNum"], 0);
                        unit.CustomerShipName = CastDbNull.To<String>(reader["ShipToCustName"], String.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<String>(reader["CustomerPart2"], String.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<String>(reader["CustomerPart3"], String.Empty).Trim();
                        unit.CustomerPart3 = CastDbNull.To<String>(reader["CustomerPart4"], String.Empty).Trim();
                        unit.PriorityCode = CastDbNull.To<String>(reader["PriorityCode"], String.Empty).Trim();
                        unit.CustomerSeqNum = CastDbNull.To<String>(reader["CustomerSeqNum"], String.Empty).Trim();
                        unit.GenericSeqNum = CastDbNull.To<String>(reader["GenericSeqNum"], String.Empty).Trim();
                        unit.PriorityCode = CastDbNull.To<String>(reader["Priority"], String.Empty).Trim();
                        // Customer PO Parsing
                        unit.CustomerPO = CastDbNull.To<String>(reader["CustomerPO"], String.Empty).Trim();
                        var tokens = unit.CustomerPO.Split('-');
                        if (tokens.Length > 0)
                        {
                            unit.CustomerPONum = Common.Utilities.Data.ParseStringToDecimal(tokens[0]);
                        }

                        if (tokens.Length > 1)
                        {
                            unit.CustomerPOLineNum = Common.Utilities.Data.ParseStringToDecimal(tokens[tokens.Length - 1]);
                        }

                        unit.Construction = CastDbNull.To<String>(reader["ProductCode"], String.Empty).Trim();
                        unit.DimMakeup = CastDbNull.To<String>(reader["Dimension"], String.Empty).Trim();
                        unit.ShapeNum = CastDbNull.To<int>(reader["ShapeNum"], 0);
                        unit.IsMissing = "N";
                        unit.OnHand = Convert.ToInt16(CastDbNull.To<Decimal>(reader["QtyOnHand"], 0));
                        unit.Units = 1;
                        unit.Passes = CastDbNull.To<int>(reader["Schedules"], 0);

                        var unitType = CastDbNull.To<String>(reader["UnitTypeCode"], String.Empty).Trim();
                        switch (unitType)
                        {
                            case "MONO GLASS":
                                unit.UnitType = "MONO";
                                break;
                            case "MONO LAMI":
                                unit.UnitType = "LAMI";
                                break;
                            default:
                                unit.UnitType = "";
                                break;
                        }

                        unit.MfgSource = CastDbNull.To<String>(reader["MfgSource"], String.Empty).Trim();
                        units.Add(unit);
                    }

                    reader.Close();
                }

                _logger.Info("Units Returned: " + units.Count);
                if (units.Count > 0)
                {
                    GetScheduledRack(units, session, plant);
                    _logger.Info("Units Returned: " + units.Count);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            _logger.Info("Return Units");
            return units;
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
                throw new FaultException(e.Message);
            }

            return dt;
        }

        /*********************************************************************************
         * ScheduledRack
         *******************************************************************************/
        public iDB2Command GetScheduledRack()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        DLZ58RID,
		        RHZ58HST
	        FROM 
		        F580569 
		        INNER JOIN F580055
		        ON RHZ58RID = DLZ58RID
	        WHERE 
		        DLMCU = ? AND
		        DLZ58SN = ? and 
		        DLLOTN = ? and
		        DLZ58DST = '1' AND
		        DLIOPR = 'Insert'";
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SerialNum", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("LotNum", iDB2DbType.iDB2Graphic, 30));
            return cmd;
        }

        public void GetScheduledRack(GlassUnits units, BusinessSystemSession session, Plant plant)
        {
            try
            {
                var cmd = session.DbCommands["OpenUnitManagerLGLocal.GetScheduledRack"];
                cmd.Connection = session.DbConnection;
                foreach (GlassUnit unit in units)
                {
                    if (unit.OrderNum > 0)
                    {
                        cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                        cmd.Parameters[1].Value = unit.UnitId;
                        cmd.Parameters[2].Value = unit.WorkOrderNum.ToString();
                        //_logger.Info(Logging.DbCmdString(cmd));
                        using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            while (reader.Read())
                            {
                                var rackId = CastDbNull.To<Decimal>(reader["DLZ58RID"], 0);
                                var statusCode = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                                unit.ScheduledRackId = rackId;
                                if (unit.RackId != rackId && (statusCode == "7" || statusCode == "8"))
                                {
                                    unit.IsMissing = "Y";
                                }
                            }
                            reader.Close();
                        }
                    }
                    else
                    {
                        unit.ScheduledRackId = unit.RackId;
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



