using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
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
    public class OpenUnitManagerIG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OpenUnitManagerIG(BusinessSystemSession session)
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
                WOrderNum,
                UnitId,
                SOrderNum,
                SOrderType,
                SOrderLineNum,
                WOrderStatusCode,
                WOrderDate,
                WOrderDueDate,
                ItemNum,
                GLCategoryCode,
                VolumeCode,
                StockTypeCode,
                UnitTypeCode,
                LitesNum,
                SOrderLastStatusCode,
                SOrderNextStatusCode,
                OrderDate,
                DueDate,
                OrigDueDate,
                DeliveryDateTime,
                ShipDate,
                CustomerSoldNum,
                CustomerSoldName,
                CustomerSoldType,
                CustomerShipNum,
                CustomerShipName,
                CustomerOrderNum,
                CustomerPart1,
                CustomerPart2,
                CustomerPart3,
                CustomerSeqNum,
                CustomerShipPref04,
                CustomerShipPref05,
                CustomerProductionDate,
                CustomerOrigProductionDate,
                GenericSeqNum,
                ProductType,
                PriorityCode,
                ReasonCode,
                CardinalProductType,
                SchedulerMsg1,
                PickingNotes,
                PackingNotes,
                LoadNum,
                UnitCostAmt,
                UnitActualPriceAmt,
                UnitListPriceAmt,
                Attempts,
                OrigScheduleNum,
                OrigScheduleDate,
                OrigScheduleType,
                OrigScheduleRackId,
                OrigScheduleRackStatusCode,
                OrigScheduleProductionLine,
                CurrScheduleNum,
                CurrScheduleDate,
                RemakeScheduleNum,
                RemakeScheduleDate,
                RemakeScheduleReason,
                RemakeScheduleComments,
                IGLineRun,
                BoxingBoxNum,
                BoxingPosition,
                BoxingRunSeq,
                TargetLineId,
                TargetShift,
                TargetShiftDate,
                ProductCode,
                Construction,
                Lite1Glass,
                Lite2Glass,
                Lite3Glass,
                Lite1Furnace,
                Lite2Furnace,
                Lite3Furnace,
                IsPick,
                Lite1Routing,
                Lite2Routing,
                Lite3Routing,
                Lite1RecdStatus,
                Lite2RecdStatus,
                Lite3RecdStatus,
                Lite1Lami,
                Lite2Lami,
                Lite3Lami,
                Lite1Holes,
                Lite2Holes,
                Lite3Holes,
                Spacer1,
                Spacer2,
                Air1MCode1,
                SpacerLength,
                BarDescription,
                GridStyleCode,
                GridStyleGroupName,
                ShapeNum,
                ShapeGroupName,
                BaseDim,
                LeftDim,
                RightDim,
                TopDim,
                Side1Dim,
                Side2Dim,
                BaseDimDesc,
                LeftDimDesc,
                RightDimDesc,
                TopDimDesc,
                Side1DimDesc,
                Side2DimDesc,
                DimensionDesc,
                UnitWeight,
                Lite1Weight,
                Lite2Weight,
                Lite3Weight,
                UnitBillableSqft,
                UnitActualSqft,
                RackId,
                RackStatusCode,
                RackType,
                RackLocation,
                RackAssetId,
                RackScheduleNum,
                RackBoxNum,
                RackTransactionDate,
                RackLockDate,
                LotNum,
                SerialNum,
                RackDetailStatusCode,
                Side,
                Stack,
                Row,
                Slot,
                Position,
                RackDetailTransactionDate,
                LastMachineName,
                LastMachineLine,
                LastMachineDate,
                LastProductionLine,
                LastOperationCode,
                LastProductionDate,
                LastProductionDesc,
                Units,
                MonoUnits,
                DoubleUnits,
                TripleUnits,
                PickUnits,
                PickUnitsAvl,
                TubeUnits,
                ShapeUnits,
                PreserveUnits,
                ScoringUnits,
                BarUnits,
                PurLiteUnits,
                UnrackedUnits,
                UncompletedUnits,
                CompletedUnits,
                ShippedUnits,
                CutDemandLites,
                CutDemandSqft,
                CutCompletedLites,
                CutCompletedSqft,
                Temper1DemandLites,
                Temper1DemandSqft,
                Temper1DemandTons,
                Temper1CompletedLites,
                Temper1CompletedSqft,
                Temper1CompletedTons,
                Temper2DemandLites,
                Temper2DemandSqft,
                Temper2DemandTons,
                Temper2CompletedLites,
                Temper2CompletedSqft,
                Temper2CompletedTons,
                PickDemandLites,
                PickDemandSqft,
                PurchDemandLites,
                PurchDemandSqft,
                PurchReceivedLites,
                PurchReceivedSqft,
                HolesDemandLites,
                HolesDemandSqft,
                HolesCompletedLites,
                HolesCompletedSqft,
                ChannelDemandAirspaces,
                ChannelDemandInches,
                ChannelCompletedAirspaces,
                ChannelCompletedInches,
                CategoryCode,
                WrittenDate
            FROM
                PlantReporting.rpclient.OpenUnits
            WHERE
                {filterString} 1=1";
            return cmd;
        }

        /*********************************************************************************
         * Open Units
         *******************************************************************************/
        public ProdUnits GetOpenUnits(BusinessSystemSession session, SqlConnection cnShopFloor, SqlConnection cnDw, Plant plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null, 
            List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, List<decimal> rackIdsList = null, List<string> productionTypeList = null)
        {
            var units = new ProdUnits();
            var cmd = new SqlCommand();
            List<String> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {
                // Time Span
                DateTime startDateDt = new DateTime(1900, 1, 1);
                DateTime endDateDt = new DateTime(2050, 12, 31);

                if (!(startDate == null || startDate == DateTime.MinValue))
                {
                    startDateDt = (DateTime) startDate;
                }

                if (!(endDate == null || endDate == DateTime.MaxValue))
                {
                    endDateDt = (DateTime)endDate;
                }

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

                if (rackIdsList.Count == 0 && scheduleNumList.Count == 0)
                {
                    filters.Add("DueDate >= @DueDateStart AND DueDate <= @DueDateEnd AND\r\n");
                    parameters.Add(new SqlParameter("@DueDateStart", SqlDbType.DateTime));
                    parameters.Last().Value = startDateDt;
                    parameters.Add(new SqlParameter("@DueDateEnd", SqlDbType.DateTime));
                    parameters.Last().Value = endDateDt;
                }

                // Schedules
                if (scheduleNumList.Count > 0)
                {
                    string strInClause = "";
                    if (scheduleNumList[0] < 200)
                    {
                        foreach (decimal scheduleNum in scheduleNumList)
                        {
                            strInClause = strInClause + $"{scheduleNum.ToString()},";
                        }
                        filters.Add("RemakeScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
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

                    filters.Add("WOrderDate <= ? AND\r\n");
                    parameters.Add(new SqlParameter("@WOrderDate", SqlDbType.DateTime));
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
                        filters.Add("StockTypeCode IN ('C','S') AND\r\n");
                    }
                    else
                    {
                        if (mto)
                        {
                            filters.Add("StockTypeCode IN ('C') AND\r\n");
                        }
                        if (mts)
                        {
                            filters.Add("StockTypeCode IN ('S') AND\r\n");
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

                //cmd = GetOpenUnitsProcedure();
                cmd.Connection = cnShopFloor;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var unit = new ProdUnit();
                        unit.Plant = plant.PlantNum;
                        unit.PlantCode = plant.PlantCode;

                        unit.WOrderNum = CastDbNull.To<decimal>(reader["WOrderNum"], 0);
                        unit.UnitId = CastDbNull.To<int>(reader["UnitId"], 0);
                        unit.SerialNum = CastDbNull.AsString(reader["SerialNum"]);

                        unit.SOrderNum = CastDbNull.To<decimal>(reader["SOrderNum"], 0);
                        unit.SOrderType = CastDbNull.AsString(reader["SOrderType"]);
                        unit.SOrderLineNum = CastDbNull.To<decimal>(reader["SOrderLineNum"], 0);

                        unit.WOrderStatusCode = CastDbNull.AsString(reader["WOrderStatusCode"]);
                        unit.SOrderLastStatusCode = CastDbNull.AsString(reader["SOrderLastStatusCode"]);
                        unit.SOrderNextStatusCode = CastDbNull.AsString(reader["SOrderNextStatusCode"]);
                        unit.DueDate = CastDbNull.To<DateTime>(reader["DueDate"], DateTime.MinValue);
                        unit.OrigDueDate = CastDbNull.To<DateTime>(reader["OrigDueDate"], DateTime.MinValue);
                        unit.OrderDate = CastDbNull.To<DateTime>(reader["WOrderDate"], DateTime.MinValue);
                        unit.DeliveryDateTime = CastDbNull.To<DateTime>(reader["DeliveryDateTime"], DateTime.MinValue);

                        unit.CustomerSoldType = CastDbNull.AsString(reader["CustomerSoldType"]);
                        unit.CustomerSoldNum = CastDbNull.To<Decimal>(reader["CustomerSoldNum"], 0);
                        unit.CustomerSoldName = CastDbNull.AsString(reader["CustomerSoldName"]);
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["CustomerShipNum"], 0);
                        unit.CustomerShipName = CastDbNull.AsString(reader["CustomerShipName"]);
                        unit.CustomerOrderNum = CastDbNull.AsString(reader["CustomerOrderNum"]);
                        unit.IsInternalOrder = unit.CustomerSoldType == "F";
                        unit.CustomerPart1 = CastDbNull.AsString(reader["CustomerPart1"]);
                        unit.CustomerPart2 = CastDbNull.AsString(reader["CustomerPart2"]);
                        unit.CustomerPart3 = CastDbNull.AsString(reader["CustomerPart3"]);
                        unit.CustomerSeqNum = CastDbNull.AsString(reader["CustomerSeqNum"]);
                        unit.CustomerProductionDate = CastDbNull.To<DateTime>(reader["CustomerProductionDate"], DateTime.MinValue);
                        unit.CustomerOrigProductionDate = CastDbNull.To<DateTime>(reader["CustomerOrigProductionDate"], DateTime.MinValue);
                        unit.ShipRoute = CastDbNull.AsString(reader["CustomerShipPref04"]);
                        unit.CustomerTier = CastDbNull.AsString(reader["CustomerShipPref05"]);
                        unit.GenericSeqNum = CastDbNull.AsString(reader["GenericSeqNum"]);
                        unit.ProductType = CastDbNull.AsString(reader["ProductType"]);
                        unit.PriorityCode = CastDbNull.AsString(reader["PriorityCode"]);
                        unit.ReasonCode = CastDbNull.AsString(reader["ReasonCode"]);
                        unit.CardinalProductType = CastDbNull.AsString(reader["CardinalProductType"]);
                        unit.PickNotes = CastDbNull.AsString(reader["PickingNotes"]);
                        unit.PackNotes = CastDbNull.AsString(reader["PackingNotes"]);
                        unit.IsPick = CastDbNull.To<bool>(reader["IsPick"], false);
                        unit.SchedulingMessage1 = CastDbNull.AsString(reader["SchedulerMsg1"]);
                        unit.UnitCostAmt = CastDbNull.To<Decimal>(reader["UnitCostAmt"], 0);
                        unit.UnitActualPriceAmt = CastDbNull.To<Decimal>(reader["UnitActualPriceAmt"], 0);
                        unit.UnitListPriceAmt = CastDbNull.To<Decimal>(reader["UnitListPriceAmt"], 0);

                        unit.ItemNum = CastDbNull.To<Decimal>(reader["ItemNum"], 0);
                        unit.VolumeCode = CastDbNull.AsString(reader["VolumeCode"]);
                        unit.StockTypeCode = CastDbNull.AsString(reader["StockTypeCode"]);
                        unit.UnitTypeCode = CastDbNull.AsString(reader["UnitTypeCode"]);
                        unit.LitesNum = CastDbNull.To<int>(reader["LitesNum"], 0);

                        unit.Attempts = CastDbNull.To<int>(reader["Attempts"], 0);                      
                        unit.OrigScheduleNum = CastDbNull.To<Decimal>(reader["OrigScheduleNum"], 0);
                        unit.OrigScheduleDate = CastDbNull.To<DateTime>(reader["OrigScheduleDate"], DateTime.MinValue);
                        unit.OrigScheduleType = CastDbNull.AsString(reader["OrigScheduleType"]);
                        unit.RemakeScheduleNum = CastDbNull.To<Decimal>(reader["RemakeScheduleNum"], 0);
                        unit.RemakeScheduleDate = CastDbNull.To<DateTime?>(reader["RemakeScheduleDate"], null);
                        unit.RemakeScheduleReason = CastDbNull.AsString(reader["RemakeScheduleReason"]);
                        unit.RemakeScheduleComment = CastDbNull.AsString(reader["RemakeScheduleComments"]);
                        unit.OrigScheduleRackId = CastDbNull.To<Decimal>(reader["OrigScheduleRackId"], 0);
                        unit.OrigScheduleRackStatusCode = CastDbNull.AsString(reader["OrigScheduleRackStatusCode"]);
                        unit.OrigScheduleProductionLine = CastDbNull.To<byte>(reader["OrigScheduleProductionLine"], 0).ToString();
                        unit.CurrScheduleNum = (unit.RemakeScheduleNum > 0) ? unit.RemakeScheduleNum : unit.OrigScheduleNum;
                        unit.CurrScheduleDate = unit.RemakeScheduleDate ?? unit.OrigScheduleDate;
                        unit.IsRackOrphan = (unit.RackId != unit.OrigScheduleRackId && unit.OrigScheduleRackStatusCode == "8") ? "Y" : "N";
                        unit.TargetLineId = CastDbNull.To<int>(reader["TargetLineId"], 0);
                        unit.TargetShift = CastDbNull.To<int>(reader["TargetShift"], 0);
                        unit.TargetShiftDate = CastDbNull.To<DateTime>(reader["TargetShiftDate"], DateTime.MinValue);

                        unit.ProductCode = CastDbNull.AsString(reader["ProductCode"]);
                        unit.ProductCodeDesc = CastDbNull.AsString(reader["Construction"]);
                        unit.Glass1 = CastDbNull.AsString(reader["Lite1Glass"]);
                        unit.Glass2 = CastDbNull.AsString(reader["Lite2Glass"]);
                        unit.Glass3 = CastDbNull.AsString(reader["Lite3Glass"]);
                        unit.Spacer1 = CastDbNull.AsString(reader["Spacer1"]);
                        unit.Spacer2 = CastDbNull.AsString(reader["Spacer2"]);
                        unit.Air1MCode1 = CastDbNull.AsString(reader["Air1MCode1"]);
                        unit.BarDescription = CastDbNull.AsString(reader["BarDescription"]);
                        unit.GridStyleCode = CastDbNull.AsString(reader["GridStyleCode"]);
                        unit.GridStyleGroupName = CastDbNull.AsString(reader["GridStyleGroupName"]);
                        unit.Lite1Routing = CastDbNull.AsString(reader["Lite1Routing"]);
                        unit.Lite2Routing = CastDbNull.AsString(reader["Lite2Routing"]);
                        unit.Lite3Routing = CastDbNull.AsString(reader["Lite3Routing"]);

                        unit.ShapeNum = CastDbNull.To<int>(reader["ShapeNum"], 0);
                        unit.ShapeGroupName = CastDbNull.AsString(reader["ShapeGroupName"]);
                        unit.BaseDimension = CastDbNull.To<decimal>(reader["BaseDim"], 0);
                        unit.BaseDimensionDesc = CastDbNull.AsString(reader["BaseDimDesc"]);
                        unit.LeftDimension = CastDbNull.To<decimal>(reader["LeftDim"], 0);
                        unit.LeftDimensionDesc = CastDbNull.AsString(reader["LeftDimDesc"]);
                        unit.RightDimension = CastDbNull.To<decimal>(reader["RightDim"], 0);
                        unit.RightDimensionDesc = CastDbNull.AsString(reader["RightDimDesc"]);
                        unit.TopDimension = CastDbNull.To<decimal>(reader["TopDim"], 0);
                        unit.TopDimensionDesc = CastDbNull.AsString(reader["TopDimDesc"]);
                        unit.Side1Dimension = CastDbNull.To<decimal>(reader["Side1Dim"], 0);
                        unit.Side1DimensionDesc = CastDbNull.AsString(reader["Side1DimDesc"]);
                        unit.Side2Dimension = CastDbNull.To<decimal>(reader["Side2Dim"], 0);
                        unit.Side2DimensionDesc = CastDbNull.AsString(reader["Side2DimDesc"]);
                        unit.DimensionDesc = CastDbNull.AsString(reader["DimensionDesc"]);
                        unit.UnitActualSqft = CastDbNull.To<decimal>(reader["UnitActualSqft"], 0);
                        unit.UnitBillableSqft = CastDbNull.To<decimal>(reader["UnitBillableSqft"], 0);
                        unit.UnitWeight = CastDbNull.To<decimal>(reader["UnitWeight"], 0);

                        unit.RackId = CastDbNull.To<Decimal>(reader["RackId"], 0);
                        unit.RackStatusCode = CastDbNull.AsString(reader["RackStatusCode"]);
                        unit.RackType = CastDbNull.AsString(reader["RackType"]);
                        unit.RackBoxNum = CastDbNull.To<Decimal>(reader["RackBoxNum"], 0);
                        unit.RackScheduleNum = CastDbNull.To<Decimal>(reader["RackScheduleNum"], 0);
                        unit.RackAssetId = CastDbNull.AsString(reader["RackAssetId"]);
                        unit.RackLocation = CastDbNull.AsString(reader["RackLocation"]);
                        unit.RackTransactionDate = CastDbNull.To<DateTime>(reader["RackTransactionDate"], DateTime.MinValue);
                        if (reader["RackLockDate"] != DBNull.Value)
                        {
                            unit.RackLockDate = CastDbNull.To<DateTime>(reader["RackLockDate"], DateTime.MinValue);
                        }
                        unit.RackDetailStatusCode = CastDbNull.AsString(reader["RackDetailStatusCode"]);
                        unit.RackRow = CastDbNull.To<Int16>(reader["Row"], 0).ToString();
                        unit.RackSide = CastDbNull.AsString(reader["Side"]);
                        unit.RackStack = CastDbNull.To<Int16>(reader["Stack"], 0).ToString();
                        unit.SlotNum = Convert.ToDecimal(Convert.ToDecimal(CastDbNull.To<Int16>(reader["Position"], 0)) > 0 ? CastDbNull.To<Int16>(reader["Position"], 0) : CastDbNull.To<Int16>(reader["Slot"], 0));
                        unit.RackDetailTransactionDate = CastDbNull.To<DateTime>(reader["RackDetailTransactionDate"], DateTime.MinValue);

                        unit.LastProductionDate = CastDbNull.To<DateTime?>(reader["LastProductionDate"], null);
                        unit.LastProductionLineCode = CastDbNull.AsString(reader["LastProductionLine"]);
                        unit.LastLineOperationCode = CastDbNull.AsString(reader["LastOperationCode"]);
                        unit.LastTransactionCode = CastDbNull.AsString(reader["LastProductionDesc"]);
                        unit.LastMachineDate = CastDbNull.To<DateTime?>(reader["LastMachineDate"], null);
                        unit.LastMachineLineCode = CastDbNull.AsString(reader["LastMachineLine"]);
                        unit.LastMachineName = CastDbNull.AsString(reader["LastMachineName"]);
                        unit.Lite1RecdStatus = CastDbNull.AsString(reader["Lite1RecdStatus"]);
                        unit.Lite2RecdStatus = CastDbNull.AsString(reader["Lite2RecdStatus"]);
                        unit.Lite3RecdStatus = CastDbNull.AsString(reader["Lite3RecdStatus"]);
                        unit.HasPurchasedLite = (unit.Lite1RecdStatus + unit.Lite2RecdStatus + unit.Lite3RecdStatus).Length > 0 ? "Y" : "N";

                        unit.Units = CastDbNull.To<int>(reader["Units"], 0); ;
                        unit.MonoUnits = CastDbNull.To<int>(reader["PreserveUnits"], 0);
                        unit.DoublePaneUnits = CastDbNull.To<int>(reader["DoubleUnits"], 0);
                        unit.TriplePaneUnits = CastDbNull.To<int>(reader["TripleUnits"], 0);
                        unit.PickUnits = CastDbNull.To<int>(reader["PickUnits"], 0);
                        unit.PickUnitsAvl = CastDbNull.To<int>(reader["PickUnitsAvl"], 0);
                        unit.OnHand = CastDbNull.To<int>(reader["PickUnitsAvl"], 0);
                        unit.TubeUnits = CastDbNull.To<int>(reader["TubeUnits"], 0);
                        unit.ShapeUnits = CastDbNull.To<int>(reader["ShapeUnits"], 0);
                        unit.PreserveUnits = CastDbNull.To<int>(reader["PreserveUnits"], 0);
                        unit.ScoringUnits = CastDbNull.To<int>(reader["ScoringUnits"], 0);
                        unit.BarUnits = CastDbNull.To<int>(reader["BarUnits"], 0);
                        unit.PurchasedLiteUnits = CastDbNull.To<int>(reader["PurLiteUnits"], 0);
                        unit.UnrackedUnits = CastDbNull.To<int>(reader["UnrackedUnits"], 0);
                        unit.UncompletedUnits = CastDbNull.To<int>(reader["UncompletedUnits"], 0);
                        unit.CompletedUnits = CastDbNull.To<int>(reader["CompletedUnits"], 0);
                        unit.ShippedUnits = CastDbNull.To<int>(reader["ShippedUnits"], 0);

                        unit.CutDemandLites = CastDbNull.To<int>(reader["CutDemandLites"], 0);
                        unit.CutCompletedLites = CastDbNull.To<int>(reader["CutCompletedLites"], 0);
                        unit.Temper1DemandLites = CastDbNull.To<int>(reader["Temper1DemandLites"], 0);
                        unit.Temper1CompletedLites = CastDbNull.To<int>(reader["Temper1CompletedLites"], 0);
                        unit.Temper2DemandLites = CastDbNull.To<int>(reader["Temper2DemandLites"], 0);
                        unit.Temper2CompletedLites = CastDbNull.To<int>(reader["Temper2CompletedLites"], 0);
                        unit.PickDemandLites = CastDbNull.To<int>(reader["PickDemandLites"], 0);
                        unit.PurchDemandLites = CastDbNull.To<int>(reader["PurchDemandLites"], 0);
                        unit.PurchReceivedLites = CastDbNull.To<int>(reader["PurchReceivedLites"], 0);
                        unit.HolesDemandLites = CastDbNull.To<int>(reader["HolesDemandLites"], 0);
                        unit.HolesCompletedLites = CastDbNull.To<int>(reader["HolesCompletedLites"], 0);
                        unit.ChannelDemandAirspaces = CastDbNull.To<int>(reader["ChannelDemandAirspaces"], 0);
                        unit.ChannelCompletedAirspaces = CastDbNull.To<int>(reader["ChannelCompletedAirspaces"], 0);

                        unit.CutDemandSqft = CastDbNull.To<decimal>(reader["CutDemandSqft"], 0);
                        unit.CutCompletedSqft = CastDbNull.To<decimal>(reader["CutCompletedSqft"], 0);
                        unit.Temper1DemandSqft = CastDbNull.To<decimal>(reader["Temper1DemandSqft"], 0);
                        unit.Temper1CompletedSqft = CastDbNull.To<decimal>(reader["Temper1CompletedSqft"], 0);
                        unit.Temper2DemandSqft = CastDbNull.To<decimal>(reader["Temper2DemandSqft"], 0);
                        unit.Temper2CompletedSqft = CastDbNull.To<decimal>(reader["Temper2CompletedSqft"], 0);
                        unit.PickDemandSqft = CastDbNull.To<decimal>(reader["PickDemandSqft"], 0);
                        unit.PurchDemandSqft = CastDbNull.To<decimal>(reader["PurchDemandSqft"], 0);
                        unit.PurchReceivedSqft = CastDbNull.To<decimal>(reader["PurchReceivedSqft"], 0);
                        unit.HolesDemandSqft = CastDbNull.To<decimal>(reader["HolesDemandSqft"], 0);
                        unit.HolesCompletedSqft = CastDbNull.To<decimal>(reader["HolesCompletedSqft"], 0);
                        unit.ChannelDemandInches = CastDbNull.To<decimal>(reader["ChannelDemandInches"], 0);
                        unit.ChannelCompletedInches= CastDbNull.To<decimal>(reader["ChannelCompletedInches"], 0);

                        unit.Temper1DemandTons = CastDbNull.To<decimal>(reader["Temper1DemandTons"], 0);
                        unit.Temper1CompletedTons = CastDbNull.To<decimal>(reader["Temper1CompletedTons"], 0);
                        unit.Temper2DemandTons = CastDbNull.To<decimal>(reader["Temper2DemandTons"], 0);
                        unit.Temper2CompletedTons = CastDbNull.To<decimal>(reader["Temper2CompletedTons"], 0);

                        unit.UpdateDate = CastDbNull.To<DateTime>(reader["WrittenDate"], new DateTime(1900,1,1));
                        unit.CategoryCode = CastDbNull.AsString(reader["CategoryCode"]);

                        units.Add(unit);

                        // Options
                        if (optionsList.Contains("MU"))
                        {
                            _logger.Info("Local IsMissing Elimination");
                            var x = units.RemoveAll(a => a.IsRackOrphan == "N");
                        }
                        if (optionsList.Contains("XHVMTO"))
                        {
                            var x = units.RemoveAll(a => a.OrigScheduleType == "HV MTO");
                        }
                        _logger.Debug("Option MP");
                        if (optionsList.Contains("MP"))
                        {
                            var x = units.RemoveAll(a => a.Attempts < 2);
                        }
                        _logger.Debug("Option SP");
                        if (optionsList.Contains("SP"))
                        {
                            var x = units.RemoveAll(a => a.Attempts < 1);
                        }
                        _logger.Debug("Option RNC/PRNC");
                        if (optionsList.Contains("RNC") || optionsList.Contains("PRNC"))
                        {
                            var x = units.RemoveAll(a => a.Attempts < 1);
                        }
                        if (optionsList.Contains("PRNC"))
                        {
                            var x = units.RemoveAll(a => a.PurchasedLiteUnits == 0);
                        }
                        if (optionsList.Contains("QC"))
                        {
                            var x = units.RemoveAll(a => a.QcInspect == "N");
                        }
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

 



        public GlassUnits GetOpenUnitsGlassUnit (BusinessSystemSession session, SqlConnection cnShopFloorRpt, SqlConnection cnShopFloor, SqlConnection cnDw, Plant plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null,
            List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, List<decimal> rackIdsList = null, List<string> productionTypeList = null)
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

                // Include Shipped Units
                if (optionsList.Contains("ADD_SHIP_TODAY"))
                {
                    filters.Add("((RackDetailStatusCode IN ('0', '1', '2', '3', '4', '5', '9', 'W') AND DueDate >= @DueDate1 AND DueDate <= @DueDate2) OR (RackDetailStatusCode IN ('7', '8') AND RackTransactionDate = @RackTransactionDate)) AND\r\n");
                    parameters.Add(new SqlParameter("@DueDate1", SqlDbType.DateTime));
                    parameters.Last().Value = startDateDt;
                    parameters.Add(new SqlParameter("@DueDate2", SqlDbType.DateTime));
                    parameters.Last().Value = endDateDt;
                    parameters.Add(new SqlParameter("@RackTransactionDate", SqlDbType.DateTime));
                    parameters.Last().Value = DateTime.Now.Date;
                }
                else
                {
                    filters.Add("RackDetailStatusCode IN ('0', '1', '2', '3', '4', '5', '9', 'W') AND DueDate >= @DueDate1 AND DueDate <= @DueDate2 AND\r\n");
                    parameters.Add(new SqlParameter("@DueDate1", SqlDbType.DateTime));
                    parameters.Last().Value = startDateDt;
                    parameters.Add(new SqlParameter("@DueDate2", SqlDbType.DateTime));
                    parameters.Last().Value = endDateDt;
                }

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
                    filters.Add("OrderDate <= @OrderDate AND\r\n");
                    parameters.Add(new SqlParameter("@OrderDate", SqlDbType.DateTime));
                    parameters.Last().Value = orderDate;
                }

                // Customer
                if (custNum > 0)
                {
                    filters.Add("CustomerSoldNum = @CustomerSoldNum AND\r\n");
                    parameters.Add(new SqlParameter("@CustomerSoldNum", SqlDbType.Decimal));
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
                        $"CustomerShipNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
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
                        $"CustomerSoldNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                // Box Num
                if (boxNum > 0)
                {
                    filters.Add("RackBoxNum = @RackBoxNum AND\r\n");
                    parameters.Add(new SqlParameter("@RackBoxNum", SqlDbType.Decimal));
                    parameters.Last().Precision = 5;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = boxNum;
                }

                // Rack Type
                if (!String.IsNullOrEmpty(rackType))
                {
                    filters.Add("RackType = @RackType AND\r\n");
                    parameters.Add(new SqlParameter("@RackType", SqlDbType.VarChar, 10));
                    parameters.Last().Value = rackType;
                }

                // Schedule Type
                if (!String.IsNullOrEmpty(scheduleType))
                {
                    filters.Add("OrigScheduleType = @OrigScheduleType AND\r\n");
                    parameters.Add(new SqlParameter("@OrigScheduleType", SqlDbType.VarChar, 10));
                    parameters.Last().Value = scheduleType;
                }

                //  Unit Type
                if (unitTypeList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string unitType in unitTypeList)
                    {
                        strInClause = strInClause + $"'{unitType}',";
                    }
                    filters.Add("UnitTypeCode IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Schedule Nums
                if (rackIdsList.Count > 0)
                {
                    string strInClause = "";
                    foreach (decimal scheduleNum in scheduleNumList)
                    {
                        strInClause = strInClause + $"{scheduleNum.ToString()},";
                    }
                    filters.Add("CurrScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
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

                // Rejects Not Completed
                if (optionsList.Contains("RNC") || optionsList.Contains("PRNC"))
                {
                    rackStatusList.Add("1");
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
                        unit.UnitIdChar = reader.Get<int>("UnitId").ToString();
                        unit.UnitId = Convert.ToDecimal(reader.Get<int>("UnitId"));
                        unit.SerialNum = reader.Get<string>("SerialNum");
                        unit.LotNum = reader.Get<string>("LotNum");
                        unit.RackId = reader.Get<decimal>("RackId");
                        unit.AssetId = reader.Get<string>("RackAssetId");
                        unit.RackType = reader.Get<string>("RackType");
                        unit.RackLocation = reader.Get<string>("RackLocation");
                        unit.RackSide = reader.Get<string>("Side");
                        unit.BoxNum = reader.Get<decimal>("RackBoxNum");
                        unit.RackStatus = reader.Get<string>("RackStatusCode");
                        unit.RackStatusCode = reader.Get<string>("RackStatusCode");
                        unit.RackTransactionDate = reader.Get<DateTime>("RackTransactionDate");
                        unit.Status = reader.Get<string>("RackDetailStatusCode");
                        unit.RackDetailStatusCode = reader.Get<string>("RackDetailStatusCode");

                        unit.ScheduleNum = reader.Get<decimal>("OrigScheduleNum");
                        unit.ScheduleDate = reader.Get<DateTime>("OrigScheduleDate");
                        unit.OriginalScheduleNum = reader.Get<Decimal>("OrigScheduleNum");
                        unit.OriginalScheduleDate = reader.Get<DateTime>("OrigScheduleDate");
                        unit.RemakeScheduleNum = reader.Get<Decimal>("RemakeScheduleNum");
                        unit.RemakeInitiateDate = reader.Get<DateTime>("RemakeScheduleDate");
                        unit.Passes = reader.Get<int>("Attempts");
                        unit.ScheduledRackId = reader.Get<Decimal>("OrigScheduleRackId");
                        unit.ScheduledRackStatusCode = reader.Get<String>("OrigScheduleRackStatusCode");
                        unit.ScheduledLineId = reader.Get<byte>("OrigScheduleProductionLine").ToString();
                        unit.ScheduledLineNum = reader.Get<byte>("OrigScheduleProductionLine");
                        unit.IGLineScheduled = reader.Get<byte>("OrigScheduleProductionLine").ToString();
                        if (unit.RackId != unit.ScheduledRackId)
                        {
                            unit.IsMissing = "Y";
                        }
                        else 
                        { 
                            unit.IsMissing = "N"; 
                        }
                        unit.OrderNum = reader.Get<Decimal>("SOrderNum");
                        unit.OrderType = reader.Get<String>("SOrderType");
                        unit.OrderLineNum = Math.Round(reader.Get<Decimal>("SorderLineNum"), 3);
                        unit.SOrderNum = reader.Get<Decimal>("SOrderNum");
                        unit.SOrderType = reader.Get<String>("SOrderType");
                        unit.SOrderLineNum = Math.Round(reader.Get<Decimal>("SorderLineNum"), 3);
                        unit.SOrderNextStatus = reader.Get<String>("SOrderNextStatusCode");
                        unit.WorkOrderNum = reader.Get<Decimal>("WOrderNum");
                        unit.WorkOrderStatus = reader.Get<String>("WOrderStatusCode");
                        unit.ItemNum = reader.Get<Decimal>("ItemNum");
                        unit.ItemStockType = reader.Get<String>("StockTypeCode");
                        unit.ScheduleType = reader.Get<String>("OrigScheduleType");
                        unit.ProductType = reader.Get<String>("ProductType");
                        unit.OrderDate = reader.Get<DateTime>("OrderDate");
                        unit.RackItemDate = reader.Get<DateTime>("RackDetailTransactionDate");
                        unit.RequestedDate = reader.Get<DateTime>("DueDate");
                        unit.PromisedDate = reader.Get<DateTime>("OrigDueDate");
                        unit.NextStatus = reader.Get<String>("SOrderNextStatusCode");
                        unit.DeliveryDateTime = reader.Get<DateTime>("DeliveryDateTime");
                        TimeSpan ts = unit.DeliveryDateTime - DateTime.Now;
                        unit.DeliveryDateTimeHours = Math.Round(ts.TotalMinutes / 60.00, 3);
                        unit.CustomerTier = reader.Get<String>("CustomerShipPref05");
                        unit.CustomerNum = reader.Get<Decimal>("CustomerSoldNum");
                        unit.CustomerName = reader.Get<String>("CustomerSoldName");
                        unit.CustomerSoldNum = reader.Get<Decimal>("CustomerSoldNum");
                        unit.CustomerSoldName = reader.Get<String>("CustomerSoldName");
                        unit.CustomerSoldType = reader.Get<String>("CustomerSoldType");
                        unit.CustomerShipNum = reader.Get<Decimal>("CustomerShipNum");
                        unit.CustomerShipName = reader.Get<String>("CustomerShipName");
                        unit.CustomerPO = reader.Get<String>("CustomerOrderNum");
                        var poArray = unit.CustomerPO.Split('-');
                        unit.CustomerPONum = poArray.Length > 1 ? Utilities.Data.CastStringToDecimal(poArray.First()) : 0;
                        unit.CustomerPOLineNum = poArray.Length > 1 ? Utilities.Data.CastStringToDecimal(poArray.Last()) : 0;
                        unit.CustomerPart1 = reader.Get<String>("CustomerPart1");
                        unit.CustomerPart2 = reader.Get<String>("CustomerPart2");
                        unit.CustomerPart3 = reader.Get<String>("CustomerPart3");
                        unit.PriorityCode = reader.Get<String>("PriorityCode");
                        unit.CustomerSeqNum = reader.Get<String>("CustomerSeqNum");
                        unit.CustomerProductionDate = reader.Get<DateTime>("CustomerProductionDate");
                        unit.CustomerOrigProductionDate = CastDbNull.To<DateTime>(reader["CustomerOrigProductionDate"], DateTime.MinValue);
                        unit.CustomerSeqPart1 = unit.CustomerSeqNum.Split('/').Length > 0 ? unit.CustomerSeqNum.Split('/')[0] : "";
                        unit.SchedulingMessage = reader.Get<string>("SchedulerMsg1");
                        unit.GenericSeqNum = reader.Get<String>("GenericSeqNum");
                        unit.PickNotes = reader.Get<String>("PickingNotes");
                        unit.PackNotes = reader.Get<String>("PackingNotes");
                        unit.UnitActualSqft = reader.Get<Decimal>("UnitActualSqft");
                        unit.UnitBillableSqft = reader.Get<Decimal>("UnitBillableSqft");
                        unit.UnitCostAmt = reader.Get<Decimal>("UnitCostAmt");
                        unit.UnitActualPriceAmt = reader.Get<Decimal>("UnitActualPriceAmt");
                        unit.UnitListPriceAmt = reader.Get<Decimal>("UnitListPriceAmt");
                        unit.ShipDateTime = reader.Get<DateTime>("ShipDate");
                        unit.RemakeReason = reader.Get<String>("RemakeScheduleReason");
                        unit.RemakeComment = reader.Get<String>("RemakeScheduleComments");
                        unit.LastMachineName = reader.Get<String>("LastMachineName");
                        unit.LastMachineLine = reader.Get<String>("LastMachineLine");
                        unit.LastMachineDate = reader.Get<DateTime>("LastMachineDate");
                        unit.LastStationName = reader.Get<String>("LastProductionLine");
                        unit.LastStationLine = reader.Get<String>("LastOperationCode");
                        unit.LastStationDate = reader.Get<DateTime>("LastProductionDate");
                        unit.SealerStatus = "N";
                        unit.LastProductionLine = reader.Get<String>("LastProductionLine");
                        unit.LastLineOperation = reader.Get<String>("LastOperationCode");
                        unit.LastLineDate = reader.Get<DateTime>("LastProductionDate");
                        //unit.CustomerNameUnitBox = reader.Get<String>("CustomerNameUnitBox"], String.Empty).Trim();
                        unit.BoxNum = reader.Get<decimal>("RackBoxNum");
                        unit.SlotNum = Convert.ToDecimal(reader.Get<Int16>("Slot"));
                        //unit.RunSequence = reader.Get<int>("RunSequence"], 0);
                        unit.Construction = reader.Get<String>("ProductCode");
                        unit.IsLami = reader.Get<String>("Lite1Lami").Length + reader.Get<String>("Lite2Lami").Length + reader.Get<String>("Lite3Lami").Length > 0 ? "Y" : "N";
                        unit.BarDescription = reader.Get<String>("BarDescription");
                        unit.BaseDim = (double)reader.Get<decimal>("BaseDim");
                        unit.LeftDim = (double)reader.Get<decimal>("LeftDim");
                        unit.ShapeNum = reader.Get<int>("ShapeNum");
                        //unit.QcInspect = reader.Get<String>("QcInspect"], String.Empty).Trim();
                        unit.GlassWeight = reader.Get<Decimal>("UnitWeight");

                        unit.Tubes = reader.Get<int>("TubeUnits");
                        unit.Shapes = reader.Get<int>("ShapeUnits");
                        unit.Preserve = reader.Get<int>("PreserveUnits");
                        unit.Scoring = reader.Get<int>("ScoringUnits");
                        unit.Bars = reader.Get<int>("BarUnits");
                        unit.Picks = reader.Get<int>("PickUnits");
                        unit.Purchases = reader.Get<int>("PurLiteUnits");
                        unit.IGLineRun = reader.Get<int>("IGLineRun");
                        unit.LastLine = reader.Get<String>("LastProductionLine");
                        unit.Lite1Routing = reader.Get<String>("Lite1Routing");
                        unit.Lite2Routing = reader.Get<String>("Lite2Routing");
                        unit.Lite3Routing = reader.Get<String>("Lite3Routing");
                        unit.UnitType = reader.Get<String>("UnitTypeCode");
                        unit.BaseDimChar = reader.Get<String>("BaseDimDesc");
                        unit.LeftDimChar = reader.Get<String>("LeftDimDesc");
                        //unit.EndPickItemNum = reader.Get<Decimal>("EndPickItemNum");
                        unit.CardinalProductType = reader.Get<String>("CardinalProductType");
                        unit.DimMakeup = reader.Get<String>("DimensionDesc");
                        unit.IsPurchasedLite = "N";
                        if (reader.Get<String>("Lite1RecdStatus") != String.Empty)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + reader.Get<String>("Lite1RecdStatus");
                        }
                        if (reader.Get<String>("Lite2RecdStatus") != String.Empty)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + reader.Get<String>("Lite2RecdStatus");
                        }
                        if (reader.Get<String>("Lite3RecdStatus") != String.Empty)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + reader.Get<String>("Lite3RecdStatus");
                        }
                        unit.RackCloseDate = reader.Get<DateTime>("RackLockDate");
                        unit.RackLockDate = reader.Get<DateTime>("RackLockDate");
                       // unit.RackLockLoadDate = reader.Get<DateTime>("RackLockLoadDate");

                        // HRIG Delivery Date Time
                        if (plant.PlantCode == "HRIG")
                        {
                            //unit.ShipRoute = reader.Get<String>("CustomerShipPref04");
                            //unit.DeliveryDateTime = unit.RequestedDate.AddHours((double)reader.Get<Decimal>("CustomerShippingOffset"));
                            //TimeSpan ts1 = unit.DeliveryDateTime - DateTime.Now.AddHours(plant.TimeOffset);
                            //unit.DeliveryDateTimeHours = Math.Round(ts1.TotalMinutes / 60.00, 3);
                        }
                        // GFIG PELLA BUCKET SLOT
                        if (unit.CustomerNum == 10065870)
                        {
                            var arrPickNotes = unit.PickNotes.Split('.');
                            if (arrPickNotes.Length > 1)
                            {
                                unit.PickNotes1 = arrPickNotes[0].Trim();
                                unit.PickNotes2 = arrPickNotes[1].Trim();
                            }
                        }
                        unit.Units = 1;
                        units.Add(unit);
                    }
                    reader.Close();
                    // Options
                    if (optionsList.Contains("MU"))
                    {
                        _logger.Info("Local IsMissing Elimination");
                        var x = units.RemoveAll(a => a.IsMissing == "N");
                    }
                    if (optionsList.Contains("XHVMTO"))
                    {
                        var x = units.RemoveAll(a => a.ScheduleType == "HV MTO");
                    }
                    _logger.Debug("Option MP");
                    if (optionsList.Contains("MP"))
                    {
                        var x = units.RemoveAll(a => a.Passes < 2);
                    }
                    _logger.Debug("Option SP");
                    if (optionsList.Contains("SP"))
                    {
                        var x = units.RemoveAll(a => a.Passes < 1);
                    }
                    _logger.Debug("Option RNC/PRNC");
                    if (optionsList.Contains("RNC") || optionsList.Contains("PRNC"))
                    {
                        var x = units.RemoveAll(a => a.Passes < 1);
                    }
                    if (optionsList.Contains("PRNC"))
                    {
                        var x = units.RemoveAll(a => a.IsPurchasedLite != "Y");
                    }
                    if (optionsList.Contains("QC"))
                    {
                        var x = units.RemoveAll(a => a.QcInspect == "N");
                    }
                    if (igLinesList.Any())
                    {
                        var x = units.RemoveAll(a => !igLinesList.Contains(a.ScheduledLineNum));
                    }
                }
                _logger.Info("Units Returned: " + units.Count);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
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
                throw new Exception(e.Message);
            }

            return dt;
        }
    }
}

