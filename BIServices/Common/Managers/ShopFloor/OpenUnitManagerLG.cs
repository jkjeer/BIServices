using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public class OpenUnitManagerLG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OpenUnitManagerLG(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
         * Open Units
         *******************************************************************************/
        public GlassUnits GetOpenUnits(BusinessSystemSession session, SqlConnection cnShopFloor, SqlConnection cnDw, Plant plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null, List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, List<decimal> rackIdsList = null, List<string> productionTypeList = null)
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
                    string strInClause = "";
                    if (scheduleNumList[0] < 2000)
                    {
                        foreach (decimal scheduleNum in scheduleNumList)
                        {
                            strInClause = strInClause + $"{scheduleNum.ToString()},";
                        }
                        filters.Add("ScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                    }
                    else
                    {
                        foreach (decimal scheduleNum in scheduleNumList)
                        {
                            strInClause = strInClause + $"{scheduleNum.ToString()},";
                        }
                        filters.Add("OrigScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                    }
                }

                // Rack Ids
                if (rackIdsList.Count > 0)
                {
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
                    filters.Add("RequestedDate >= @RequestedDateStart AND RequestedDate <= @RequestedDateEnd AND\r\n");
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
                        filters.Add("RackBoxNum = ? AND\r\n");
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


                    // Production Type
                    if (productionTypeList.Count > 0)
                    {
                        bool lv = false;
                        bool hv = false;
                        bool mto = false;
                        bool mts = false;
                        foreach (string productionType in productionTypeList)
                        {
                            switch (productionType)
                            {
                                case "LV MTO":
                                    lv = true;
                                    mto = true;
                                    break;
                                case "HV MTO":
                                    hv = true;
                                    mto = true;
                                    break;
                                case "HV MTS":
                                    hv = true;
                                    mts = true;
                                    break;
                            }
                        }

                        if (lv && hv)
                        {
                            filters.Add("VolumeCode IN ('LV','HV') AND\r\n");
                        }
                        else
                        {
                            if (lv)
                            {
                                filters.Add("VolumeCode IN ('LV') AND\r\n");
                            }
                            if (hv)
                            {
                                filters.Add("VolumeCode IN ('HV') AND\r\n");
                            }
                        }

                        if (mto && mts)
                        {
                            filters.Add("StockingType IN ('C','S') AND\r\n");
                        }
                        else
                        {
                            if (mto)
                            {
                                filters.Add("StockingType IN ('C') AND\r\n");
                            }
                            if (mts)
                            {
                                filters.Add("StockingType IN ('S') AND\r\n");
                            }
                        }

                    }

                    // Rack Status
                    if (rackStatusList.Count > 0)
                    {
                        string strInClause = "";
                        foreach (string rackStatus in rackStatusList)
                        {
                            strInClause = strInClause + $"'{rackStatus}',";
                        }

                        filters.Add("RackDetailStatusCode IN (" + strInClause.Substring(0, strInClause.Length - 1) +
                                    ") AND\r\n");
                    }
                }

                cmd = GetOpenUnits(filters);
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
                        unit.PlantCode = plant.PlantCode;

                        unit.WorkOrderNum = CastDbNull.To<Decimal>(reader["WOrderNum"], 0);
                        unit.UnitId = (decimal)CastDbNull.To<Int64>(reader["SerialNum"], 0);

                        unit.WorkOrderStatus = CastDbNull.To<String>(reader["WOrderStatusCode"], String.Empty).Trim();
                        unit.NextStatus = CastDbNull.To<String>(reader["SOrderNextStatusCode"], String.Empty).Trim();
                        unit.VolumeCode = CastDbNull.To<String>(reader["VolumeCode"], String.Empty).Trim();
                        unit.ItemStockType = CastDbNull.To<String>(reader["StockingType"], String.Empty).Trim();
                        unit.ItemNum = CastDbNull.To<Decimal>(reader["ItemNum"], 0);
                        unit.UnitOriginCode = CastDbNull.To<String>(reader["UnitOriginCode"], String.Empty).Trim();
                        switch (CastDbNull.To<String>(reader["UnitTypeCode"], String.Empty).Trim())
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
                        unit.MfgSource = CastDbNull.To<String>(reader["UnitOriginCode"], String.Empty).Trim();
                        unit.Units = 1;

                        unit.Construction = CastDbNull.To<String>(reader["ProductCode"], String.Empty).Trim();
                        unit.ProductCode = CastDbNull.To<String>(reader["ProductCode"], String.Empty).Trim();
                        unit.Glass1Code = CastDbNull.To<String>(reader["Glass1Code"], String.Empty).Trim();
                        unit.Glass2Code = CastDbNull.To<String>(reader["Glass2Code"], String.Empty).Trim();
                        unit.Glass3Code = CastDbNull.To<String>(reader["Glass3Code"], String.Empty).Trim();
                        unit.Glass4Code = CastDbNull.To<String>(reader["Glass4Code"], String.Empty).Trim();
                        unit.Interlayer1Code = CastDbNull.To<String>(reader["Interlayer1Code"], String.Empty).Trim();
                        unit.Interlayer2Code = CastDbNull.To<String>(reader["Interlayer2Code"], String.Empty).Trim();
                        unit.Interlayer3Code = CastDbNull.To<String>(reader["Interlayer3Code"], String.Empty).Trim();

                        unit.ShapeNum = CastDbNull.To<int>(reader["ShapeNum"], 0);
                        unit.DimMakeup = CastDbNull.To<String>(reader["Dimension"], String.Empty).Trim();
                        unit.UnitActualSqft = CastDbNull.To<decimal>(reader["UnitActualSqft"], 0);
                        unit.GlassWeight = CastDbNull.To<decimal>(reader["UnitGlassWeightLbs"], 0);
                        unit.IsFabrication = CastDbNull.To<bool>(reader["IsFabrication"], false) ? "Y" : "N";

                        unit.Passes = CastDbNull.To<int>(reader["Schedules"], 0);
                        unit.ScheduleNum = CastDbNull.To<Decimal>(reader["OrigScheduleNum"], 0);
                        unit.ScheduleDate = CastDbNull.To<DateTime>(reader["OrigScheduleDate"], DateTime.MinValue);
                        unit.ScheduleType = CastDbNull.To<String>(reader["OrigScheduleType"], String.Empty).Trim();
                        unit.ScheduleName = CastDbNull.To<String>(reader["OrigScheduleName"], String.Empty).Trim();
                        var currentScheduleNum = CastDbNull.To<Decimal>(reader["ScheduleNum"], 0);
                        if (currentScheduleNum < 3000)
                        {
                            unit.RemakeScheduleNum = CastDbNull.To<Decimal>(reader["ScheduleNum"], 0);
                            unit.RemakeInitiateDate = CastDbNull.To<DateTime>(reader["ScheduleDate"], DateTime.MinValue);
                        }
                        unit.ScheduledRackId = CastDbNull.To<Decimal>(reader["OrigScheduleRackId"], 0);
                        unit.ScheduledRackStatusCode = CastDbNull.To<String>(reader["OrigScheduleRackStatusCode"], String.Empty).Trim();

                        unit.RackId = CastDbNull.To<Decimal>(reader["RackId"], 0);
                        unit.RackStatusCode = CastDbNull.To<String>(reader["RackStatusCode"], String.Empty).Trim();
                        unit.RackType = CastDbNull.To<String>(reader["RackType"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<Decimal>(reader["RackBoxNum"], 0);
                        unit.RackScheduleNum = CastDbNull.To<Decimal>(reader["RackScheduleNum"], 0);
                        unit.AssetId = CastDbNull.To<String>(reader["RackAssetId"], String.Empty).Trim();
                        unit.RackLocation = CastDbNull.To<String>(reader["RackLocation"], String.Empty).Trim();
                        unit.RackTransactionDate = CastDbNull.To<DateTime>(reader["RackTransactionDate"], DateTime.MinValue);

                        unit.Status = CastDbNull.To<String>(reader["RackDetailStatusCode"], String.Empty).Trim();
                        unit.RackItemStatusCode = CastDbNull.To<String>(reader["RackDetailStatusCode"], String.Empty).Trim();
                        unit.RackDetailStatusCode = CastDbNull.To<String>(reader["RackDetailStatusCode"], String.Empty).Trim();
                        unit.RackRow = CastDbNull.To<Int16>(reader["RackDetailRow"], 0).ToString();
                        unit.RackSide = CastDbNull.To<String>(reader["RackDetailSide"], String.Empty).Trim();
                        unit.RackStack = CastDbNull.To<Int16>(reader["RackDetailStack"], 0).ToString();
                        unit.SlotNum = Convert.ToDecimal(CastDbNull.To<Int16>(reader["RackDetailPosition"], 0));
                        unit.RackItemDate = CastDbNull.To<DateTime>(reader["RackDetailTransactionDate"], DateTime.MinValue);
                        unit.VirtualRackName = CastDbNull.To<String>(reader["VirtualRackName"], String.Empty).Trim();
                        unit.VirtualSideNum = CastDbNull.To<int>(reader["VirtualSideNumber"], 0);
                        unit.VirtualStackNum = CastDbNull.To<int>(reader["VirtualStackNumber"], 0);
                        unit.VirtualSlotNum = CastDbNull.To<int>(reader["VirtualSlotNumber"], 0);

                        unit.ProductionStatus = CastDbNull.To<String>(reader["ProductionStatus"], String.Empty).Trim();
                        unit.LastLineDate = CastDbNull.To<DateTime>(reader["LastProductionDate"], DateTime.MinValue);
                        unit.LastProductionLine = CastDbNull.To<String>(reader["LastProductionLineCode"], String.Empty).Trim();
                        unit.LastLineOperation = CastDbNull.To<String>(reader["LastOperationCode"], String.Empty).Trim();
                        unit.LastTransactionCode = CastDbNull.To<String>(reader["LastTransactionCode"], String.Empty).Trim();
                        unit.LastStationDesc = CastDbNull.To<String>(reader["LastTransactionRejectCode"], String.Empty).Trim();

                        unit.CustomerSoldType = CastDbNull.To<String>(reader["SoldToCustType"], String.Empty).Trim();
                        unit.CustomerNum = CastDbNull.To<Decimal>(reader["SoldToCustNum"], 0);
                        unit.CustomerName = CastDbNull.To<String>(reader["SoldToCustName"], String.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["ShipToCustNum"], 0);
                        unit.CustomerShipName = CastDbNull.To<String>(reader["ShipToCustName"], String.Empty).Trim();
                        unit.OrigCustomerNum = CastDbNull.To<Decimal>(reader["OrigSoldToCustNum"], 0);
                        unit.OrigCustomerName = CastDbNull.To<String>(reader["OrigSoldToCustName"], String.Empty).Trim();
                        unit.OrigWOrderNum = CastDbNull.To<Decimal>(reader["OrigWOrderNum"], 0);
                        unit.OrderDate = CastDbNull.To<DateTime>(reader["OrderDate"], DateTime.MinValue);
                        unit.RequestedDate = CastDbNull.To<DateTime>(reader["RequestedDate"], DateTime.MinValue);
                        unit.OrigRequestedDate = CastDbNull.To<DateTime>(reader["OrigRequestedDate"], DateTime.MinValue);
                        unit.ShipDateTime = CastDbNull.To<DateTime>(reader["ShipDate"], DateTime.MinValue);
                        unit.OrderNum = CastDbNull.To<Decimal>(reader["SOrderNum"], 0);
                        unit.OrderType = CastDbNull.To<String>(reader["SOrderType"], String.Empty).Trim();
                        unit.OrderLineNum = Math.Round(CastDbNull.To<Decimal>(reader["SOrderLineNum"], 0), 3);
                        unit.PriorityCode = CastDbNull.To<String>(reader["PriorityCode"], String.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<String>(reader["CustomerPart1"], String.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<String>(reader["CustomerPart2"], String.Empty).Trim();
                        unit.CustomerPart3 = CastDbNull.To<String>(reader["CustomerPart3"], String.Empty).Trim();
                        unit.CustomerPO = CastDbNull.To<String>(reader["CustomerPO"], String.Empty).Trim();
                        var tokens = unit.CustomerPO.Split('-', '_');
                        if (tokens.Length > 0)
                        {
                            unit.CustomerPONum = Data.ParseStringToDecimal(tokens[0]);
                        }
                        if (tokens.Length > 1)
                        {
                            unit.CustomerPOLineNum = Data.ParseStringToDecimal(tokens[tokens.Length - 1]);
                        }
                        unit.CustomerSeqNum = CastDbNull.To<String>(reader["CustomerSeqNum"], String.Empty).Trim();
                        unit.GenericSeqNum = CastDbNull.To<String>(reader["GenericSeqNum"], String.Empty).Trim();
                        unit.ProductType = CastDbNull.To<String>(reader["ProductType"], String.Empty).Trim();
                        unit.CardinalProductType = CastDbNull.To<String>(reader["CardinalProductType"], String.Empty).Trim();
                        unit.ProjectName = CastDbNull.To<String>(reader["ProjectName"], String.Empty).Trim();
                        unit.PriceCode1 = CastDbNull.To<String>(reader["PriceCode1"], String.Empty).Trim();
                        unit.PriceCode2 = CastDbNull.To<String>(reader["PriceCode2"], String.Empty).Trim();
                        unit.PriceCode3 = CastDbNull.To<String>(reader["PriceCode3"], String.Empty).Trim();
                        unit.Originator = CastDbNull.To<String>(reader["Originator"], String.Empty).Trim();
                        unit.IsMissing = (unit.RackId != unit.ScheduledRackId && unit.ScheduledRackStatusCode == "8") ? "Y" : "N";
                        unit.IsInternalOrder = unit.CustomerSoldType == "F";
                        unit.CustomerTier = CastDbNull.To<String>(reader["CustomerTier"], String.Empty).Trim();
                        units.Add(unit);
                    }
                    reader.Close();
                }
                _logger.Info("Units Returned: " + units.Count);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            _logger.Info("Return Units");
            return units;
        }

        /*********************************************************************************
         * Open Units SQL
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
            SELECT
                ProdUnitId,
                WOrderNum,
                SerialNum,
                WOrderStatusCode,
                SOrderNextStatusCode,
                WOrderQty,
                VolumeCode,
                StockingType,
                ItemNum,
                UnitTypeCode,
                UnitOriginCode,
                Quantity,
                ProductCode,

                Glass1Code,
                Glass2Code,
                Glass3Code,
                Glass4Code,
                Interlayer1Code,
                Interlayer2Code,
                Interlayer3Code,

                ShapeNum,
                Dimension,
                UnitActualSqft,
                UnitGlassWeightLbs,
                IsFabrication,
                Schedules,
                OrigScheduleNum,
                OrigScheduleDate,
                OrigScheduleType,
                OrigScheduleName,
                OrigScheduleRackId,
                OrigScheduleRackStatusCode,
                OrigWOrderNum,
                ScheduleNum,
                ScheduleDate,
                RackId,
                RackStatusCode,
                RackDetailStatusCode,
                RackType,
                RackScheduleNum,
                RackBoxNum,
                RackLocation,
                RackAssetId,
                RackDetailRow,
                RackDetailSide,
                RackDetailStack,
                RackDetailPosition,
                RackTransactionDate,
                RackDetailTransactionDate,
                VirtualRackName,
                VirtualSlotNumber,
                VirtualSideNumber,
                VirtualStackNumber,
                ProductionStatus,
                LastProductionDate,
                LastProductionLineCode,
                LastOperationCode,
                LastTransactionCode,
                LastTransactionRejectCode,
                SoldToCustNum,
                SoldToCustName,
                SoldToCustType,
                ShipToCustNum,
                ShipToCustName,                
                OrigSoldToCustNum,
                OrigSoldToCustName,
                OrderDate,
                OrigRequestedDate,
                RequestedDate,
                SystemName,
                SOrderNum,
                SOrderType,
                SOrderLineNum,
                PriorityCode,
                CustomerPart1,
                CustomerPart2,
                CustomerPart3,
                CustomerPO,
                CustomerSeqNum,
                GenericSeqNum,
                ProductType,
                CardinalProductType,
                ShipDate,
                Originator,
                ProjectName,
                PriceCode1,
                PriceCode2,
                PriceCode3,
                CustomerTier
            FROM
                PlantReporting.rpclient.OpenUnits
            WHERE
                {filterString}
                WOrderStatusCode IN ( '40', '92' ) AND
                RackDetailStatusCode IN ('0', '1', '2', '3', '4', '5', '9')";
            return cmd;
        }

        public SqlCommand GetOpenUnitsView(List<string> filters)
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
                ProdUnitId,
                WOrderNum,
                SerialNum,
                WOrderStatusCode,
                SOrderNextStatusCode,
                WOrderQty,
                VolumeCode,
                StockingType,
                ItemNum,
                UnitTypeCode,
                UnitOriginCode,
                Quantity,
                ProductCode,
                ShapeNum,
                Dimension,
                UnitActualSqft,
                UnitGlassWeightLbs,
                IsFabrication,
                Schedules,
                OrigScheduleNum,
                OrigScheduleDate,
                OrigScheduleType,
                OrigScheduleName,
                OrigScheduleRackId,
                OrigScheduleRackStatusCode,
                OrigWOrderNum,
                ScheduleNum,
                ScheduleDate,
                RackId,
                RackStatusCode,
                RackDetailStatusCode,
                RackType,
                RackScheduleNum,
                RackBoxNum,
                RackLocation,
                RackAssetId,
                RackDetailRow,
                RackDetailSide,
                RackDetailStack,
                RackDetailPosition,
                RackTransactionDate,
                RackDetailTransactionDate,
                VirtualRackName,
                VirtualSlotNumber,
                VirtualSideNumber,
                VirtualStackNumber,
                ProductionStatus,
                LastProductionDate,
                LastProductionLineCode,
                LastOperationCode,
                LastTransactionCode,
                LastTransactionRejectCode,
                SoldToCustNum,
                SoldToCustName,
                SoldToCustType,
                ShipToCustNum,
                ShipToCustName,
                OrigSoldToCustNum,
                OrigSoldToCustName,
                OrderDate,
                OrigRequestedDate,
                RequestedDate,
                SystemName,
                SOrderNum,
                SOrderType,
                SOrderLineNum,
                PriorityCode,
                CustomerPart1,
                CustomerPart2,
                CustomerPart3,
                CustomerPO,
                CustomerSeqNum,
                GenericSeqNum,
                ProductType,
                CardinalProductType,
                ShipDate,
                Originator,
                ProjectName,
                PriceCode1,
                PriceCode2,
                PriceCode3,
                CustomerTier
            FROM
                EBS.vw_OpenUnits
            WHERE
                {filterString}
                WOrderStatusCode IN ( '40', '92' ) AND
                RackDetailStatusCode IN ('0', '1', '2', '3', '4', '5', '9')";
            return cmd;
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

