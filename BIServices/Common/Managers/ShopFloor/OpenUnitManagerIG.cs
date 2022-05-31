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
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null, List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, List<decimal> rackIdsList = null, List<string> productionTypeList = null)
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
                    filters.Add("((RackStatusCode IN ('0', '1', '2', '3', '4', '5', '9', 'W') AND DueDate >= ? AND DueDate <= ?) OR (RackStatusCode IN ('7', '8') AND RackTransactionDate = ?)) AND\r\n");
                    parameters.Add(new SqlParameter("@DueDate1", SqlDbType.Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(startDateDt);
                    parameters.Add(new SqlParameter("@DueDate2", SqlDbType.Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(endDateDt);
                    parameters.Add(new SqlParameter("@RackTransactionDate", SqlDbType.Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(DateTime.Now);
                }
                else
                {
                    filters.Add("RackStatusCode IN ('0', '1', '2', '3', '4', '5', '9', 'W') AND DueDate >= ? AND DueDate <= ? AND\r\n");
                    parameters.Add(new SqlParameter("@DueDate1", SqlDbType.Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(startDateDt);
                    parameters.Add(new SqlParameter("@DueDate2", SqlDbType.Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(endDateDt);
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
                    filters.Add("OrderDate <= ? AND\r\n");
                    parameters.Add(new SqlParameter("@OrderDate", SqlDbType.Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)orderDate);
                }

                // Customer
                if (custNum > 0)
                {
                    filters.Add("CustomerSoldNum = ? AND\r\n");
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
                                customerShipNum.Substring(1, customerShipNum.Length - 1).Split(','), Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerShipNumList = Array.ConvertAll<string, decimal>(customerShipNum.Split(','), Convert.ToDecimal).ToList();
                    }
                    string strInClause = "";
                    foreach (decimal customerShip in customerShipNumList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new SqlParameter("@CustomerShipNum", SqlDbType.Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerShip;
                    }
                    filters.Add($"CustomerShipNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
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
                                customerSoldNum.Substring(1, customerSoldNum.Length - 1).Split(','), Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerSoldNumList = Array.ConvertAll<string, decimal>(customerSoldNum.Split(','), Convert.ToDecimal).ToList();
                    }
                    string strInClause = "";
                    foreach (decimal customerSold in customerSoldNumList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new SqlParameter("@CustomerSoldNum", SqlDbType.Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerSold;
                    }
                    filters.Add($"CustomerSoldNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                // Box Num
                if (boxNum > 0)
                {
                    filters.Add("RackBoxNum = ? AND\r\n");
                    parameters.Add(new SqlParameter("@RackBoxNum", SqlDbType.Decimal));
                    parameters.Last().Precision = 5;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = boxNum;
                }

                // Rack Type
                if (!String.IsNullOrEmpty(rackType))
                {
                    filters.Add("RackTypeCode = ? AND\r\n");
                    parameters.Add(new SqlParameter("@RackTypeCode", SqlDbType.VarChar, 10));
                    parameters.Last().Value = rackType;
                }

                // Schedule Type
                if (!String.IsNullOrEmpty(scheduleType))
                {
                    filters.Add("OrigScheduleType = ? AND\r\n");
                    parameters.Add(new SqlParameter("@OrigScheduleType", SqlDbType.VarChar, 10));
                    parameters.Last().Value = scheduleType;
                }

                // Unit Type
                if (unitTypeList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string unitType in unitTypeList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new SqlParameter("@UnitTypeCode", SqlDbType.VarChar, 1));
                        parameters.Last().Value = unitType;
                    }
                    filters.Add("UnitTypeCode IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Schedule Num
                if (scheduleNumList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (decimal scheduleNum in scheduleNumList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new SqlParameter("@ScheduleNum", SqlDbType.Decimal));
                        parameters.Last().Precision = 5;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = scheduleNum;
                    }
                    filters.Add("CurrScheduleNum IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Rack Ids
                if (rackIdsList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (decimal rackId in rackIdsList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new SqlParameter("@RackId", SqlDbType.Decimal));
                        parameters.Last().Precision = 15;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = rackId;
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
                        strInClause = strInClause + "?,";
                        parameters.Add(new SqlParameter("rackStatus", SqlDbType.VarChar, 1));
                        parameters.Last().Value = rackStatus;
                    }
                    filters.Add("RDZ58DST IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
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
                        unit.UnitIdChar = CastDbNull.To<int>(reader["UnitId"], 0).ToString().Trim();
                        unit.UnitId = Convert.ToDecimal(unit.UnitIdChar);
                        unit.RackId = CastDbNull.To<Decimal>(reader["RackId"], 0);
                        unit.AssetId = CastDbNull.To<String>(reader["RackAssetId"], String.Empty).Trim();
                        unit.RackType = CastDbNull.To<String>(reader["RackType"], String.Empty).Trim();
                        unit.RackLocation = CastDbNull.To<String>(reader["RackLocation"], String.Empty).Trim();
                        unit.RackSide = CastDbNull.To<String>(reader["Side"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<Decimal>(reader["RackBoxNum"], 0);
                        //unit.RackTareWeight = CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0) / 100;
                        unit.RackStatus = CastDbNull.To<String>(reader["RackStatusCode"], String.Empty).Trim();
                        unit.Status = CastDbNull.To<String>(reader["RackDetailStatusCode"], String.Empty).Trim();
                        unit.ScheduleNum = CastDbNull.To<Decimal>(reader["CurrScheduleNum"], 0);
                        unit.ScheduleDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["CurrScheduleDate"], 0));
                        //unit.OrderCompany = CastDbNull.To<String>(reader["SDKCOO"], String.Empty).Trim();
                        unit.OrderNum = CastDbNull.To<Decimal>(reader["SOrderNum"], 0);
                        unit.OrderType = CastDbNull.To<String>(reader["SOrderType"], String.Empty).Trim();
                        unit.OrderLineNum = Math.Round(CastDbNull.To<Decimal>(reader["SorderLineNum"], 0), 3);
                        unit.WorkOrderNum = CastDbNull.To<Decimal>(reader["WOrderNum"], 0);
                        unit.WorkOrderStatus = CastDbNull.To<String>(reader["WOrderStatusCode"], String.Empty).Trim();
                        unit.ItemNum = CastDbNull.To<Decimal>(reader["ItemNum"], 0);
                        unit.ItemStockType = CastDbNull.To<String>(reader["StockTypeCode"], String.Empty).Trim();

                        unit.ScheduleType = CastDbNull.To<String>(reader["OrigScheduleType"], String.Empty).Trim();
                        unit.ProductType = CastDbNull.To<String>(reader["ProductType"], String.Empty).Trim();
                        unit.OrderDate = CastDbNull.To<DateTime>(reader["OrderDate"], DateTime.MinValue);
                        unit.RackItemDate = CastDbNull.To<DateTime>(reader["RackDetailTransactionDate"], DateTime.MinValue);
                        unit.RequestedDate = CastDbNull.To<DateTime>(reader["DueDate"], DateTime.MinValue);
                        unit.PromisedDate = CastDbNull.To<DateTime>(reader["OrigDueDate"], DateTime.MinValue);
                        unit.NextStatus = CastDbNull.To<String>(reader["SOrderNextStatusCode"], String.Empty).Trim();

                        // Delivery Time per Order Entry
                        unit.DeliveryDateTime = CastDbNull.To<DateTime>(reader["DeliveryDateTime"], DateTime.MinValue);
                        TimeSpan ts = unit.DeliveryDateTime - DateTime.Now;
                        unit.DeliveryDateTimeHours = Math.Round(ts.TotalMinutes / 60.00, 3);
                        unit.CustomerTier = CastDbNull.To<String>(reader["CustomerShipPref05"], String.Empty).Trim();

                        unit.CustomerNum = CastDbNull.To<Decimal>(reader["CustomerSoldNum"], 0);
                        unit.CustomerName = CastDbNull.To<String>(reader["CustomerSoldName"], String.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["CustomerShipNum"], 0);
                        unit.CustomerShipName = CastDbNull.To<String>(reader["CustomerShipName"], String.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<String>(reader["CustomerPart1"], String.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<String>(reader["CustomerPart2"], String.Empty).Trim();
                        unit.CustomerPart3 = CastDbNull.To<String>(reader["CustomerPart3"], String.Empty).Trim();
                        unit.PriorityCode = CastDbNull.To<String>(reader["PriorityCode"], String.Empty).Trim();
                        unit.CustomerSeqNum = CastDbNull.To<String>(reader["CustomerSeqNum"], String.Empty).Trim();
                        unit.SchedulingMessage = CastDbNull.To<string>(reader["SchedulerMsg1"], string.Empty).Trim();
                        //var arr = unit.CustomerSeqNum.Split('/');
                        //if (arr.Length > 0)
                        //{
                        //    unit.CustomerSeqPart1 = arr[0];
                        //}
                        unit.GenericSeqNum = CastDbNull.To<String>(reader["GenericSeqNum"], String.Empty).Trim();
                        unit.PickNotes = CastDbNull.To<String>(reader["PickingNotes"], String.Empty).Trim();
                        unit.PackNotes = CastDbNull.To<String>(reader["PackingNotes"], String.Empty).Trim();
                        unit.UnitActualSqft = CastDbNull.To<Decimal>(reader["UnitActualSqft"], 0);
                        unit.UnitBillableSqft = CastDbNull.To<Decimal>(reader["UnitBillableSqft"], 0);
                        unit.UnitCostAmt = CastDbNull.To<Decimal>(reader["UnitCostAmt"], 0);
                        unit.UnitActualPriceAmt = CastDbNull.To<Decimal>(reader["UnitActualPriceAmt"], 0);
                        unit.UnitListPriceAmt = CastDbNull.To<Decimal>(reader["UnitListPriceAmt"], 0);

                        //// PELLA BUCKET SLOT
                        //if (unit.CustomerNum == 10065870)
                        //{
                        //    var arrPickNotes = unit.PickNotes.Split('.');
                        //    if (arrPickNotes.Length > 1)
                        //    {
                        //        unit.PickNotes1 = arrPickNotes[0].Trim();
                        //        unit.PickNotes2 = arrPickNotes[1].Trim();
                        //    }
                        //}

                        // Customer PO Parsing
                        unit.CustomerPO = CastDbNull.To<String>(reader["CustomerOrderNum"], String.Empty).Trim();
                        unit.CustomerPONum = CastDbNull.To<Decimal>(reader["CustomerPONum"], 0);
                        unit.CustomerPOLineNum = CastDbNull.To<Decimal>(reader["CustomerPOLineNum"], 0);

                        //var shipDateId = CastDbNull.To<Decimal>(reader["LHADDJ"], 0);
                        //var shipTimeId = CastDbNull.To<Decimal>(reader["LHADTM"], 0);
                        //if (shipDateId > 0) unit.ShipDateTime = Common.Utilities.Common.CastToDateTime(shipDateId, shipTimeId).AddHours(plant.TimeOffset);

                        unit.IGLineScheduled = "";
                        unit.IsMissing = "N";
                        unit.OnHand = Convert.ToInt16(CastDbNull.To<Decimal>(reader["RackDetailQtyOnHand"], 0));
                        unit.Units = 1;
                        //unit.ConfigId = CastDbNull.To<Decimal>(reader["CMCFGID"], 0);

                        if (plant.PlantCode == "HRIG")
                        {
                            unit.ShipRoute = CastDbNull.To<String>(reader["CustomerShippingLane"], String.Empty).Trim();
                            unit.DeliveryDateTime = unit.RequestedDate.AddHours((double)CastDbNull.To<Decimal>(reader["CustomerShippingOffset"], 0));
                            TimeSpan ts1 = unit.DeliveryDateTime - DateTime.Now.AddHours(plant.TimeOffset);
                            unit.DeliveryDateTimeHours = Math.Round(ts1.TotalMinutes / 60.00, 3);
                        }

                        var rackId = CastDbNull.To<Decimal>(reader["DLZ58RID"], 0);
                        var rackStatus = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                        var scheduleNum = CastDbNull.To<Decimal>(reader["S1Z58SCN"], 0);
                        var scheduleDate = Timestamp.CastToDateTime(CastDbNull.To<Decimal>(reader["S1Z58SCD"], 0));
                        unit.ScheduledRackId = CastDbNull.To<Decimal>(reader["ScheduledRackId"], 0);
                        unit.OriginalScheduleNum = CastDbNull.To<Decimal>(reader["OrigScheduleNum"], 0);
                        unit.OriginalScheduleDate = CastDbNull.To<DateTime>(reader["OrigScheduleDate"], (DateTime)SqlDateTime.MinValue);
                        if (unit.RackId != unit.ScheduledRackId && (unit.RackStatusCode == "7" || unit.RackStatusCode == "8"))
                        {
                            unit.IsMissing = "Y";
                        }

                        //unit.RemakeId = (int)remakeRec["RemakesId"];
                        unit.RemakeScheduleNum = CastDbNull.To<Decimal>(reader["RemakeScheduleNum"], 0);
                        unit.RemakeInitiateDate = CastDbNull.To<DateTime>(reader["RemakeScheduleDate"], (DateTime)SqlDateTime.MinValue);
                        unit.RemakeReason = CastDbNull.To<String>(reader["RemakeReason"], String.Empty).Trim();
                        unit.RemakeComment = CastDbNull.To<String>(reader["RemakeComments"], String.Empty).Trim();

                        unit.LastMachineName = CastDbNull.To<String>(reader["LastMachineName"], String.Empty).Trim();
                        unit.LastMachineLine = CastDbNull.To<String>(reader["LastMachineLine"], String.Empty).Trim();
                        unit.LastMachineDate = CastDbNull.To<DateTime>(reader["LastMachineDate"], (DateTime)SqlDateTime.MinValue);
                        unit.LastStationName = CastDbNull.To<String>(reader["LastStationName"], String.Empty).Trim();
                        unit.LastStationLine = CastDbNull.To<String>(reader["LastStationLine"], String.Empty).Trim();
                        unit.LastStationDate = CastDbNull.To<DateTime>(reader["LastStationDate"], (DateTime)SqlDateTime.MinValue);
                        unit.SealerStatus = "N";
                        unit.LastProductionLine = CastDbNull.To<String>(reader["LastStationName"], String.Empty).Trim();
                        unit.LastLineOperation = CastDbNull.To<String>(reader["LastStationName"], String.Empty).Trim();
                        unit.LastLineDate = CastDbNull.To<DateTime>(reader["LastStationDate"], (DateTime)SqlDateTime.MinValue);

                        //unit.CustomerNameUnitBox = CastDbNull.To<String>(reader["CustomerNameUnitBox"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<int>(reader["RackBoxNum"], 0);
                        unit.SlotNum = CastDbNull.To<int>(reader["Slot"], 0);
                        //unit.RunSequence = CastDbNull.To<int>(reader["RunSequence"], 0);
                        unit.Construction = CastDbNull.To<String>(reader["Construction"], String.Empty).Trim();
                        unit.IsLami = CastDbNull.To<String>(reader["LastMachineName"], String.Empty).Trim();
                        //unit.BarDescription = CastDbNull.To<String>(reader["LastMachineName"], String.Empty).Trim();
                        unit.BaseDim = CastDbNull.To<double>(reader["BaseDim"], 0);
                        unit.LeftDim = CastDbNull.To<double>(reader["LeftDim"], 0);
                        unit.ShapeNum = CastDbNull.To<int>(reader["ShapeNum"], 0);
                        //unit.QcInspect = CastDbNull.To<String>(reader["QcInspect"], String.Empty).Trim();
                        unit.GlassWeight = CastDbNull.To<Decimal>(reader["GlassWeight"], 0);
                        unit.Tubes = CastDbNull.To<int>(reader["Tubes"], 0);
                        unit.Shapes = CastDbNull.To<int>(reader["Shapes"], 0);
                        unit.Preserve = CastDbNull.To<int>(reader["Preserve"], 0);
                        unit.Scoring = CastDbNull.To<int>(reader["Scoring"], 0);
                        unit.Bars = CastDbNull.To<int>(reader["Bars"], 0);
                        unit.Picks = CastDbNull.To<int>(reader["Picks"], 0);
                        unit.Purchases = CastDbNull.To<int>(reader["Purchases"], 0);
                        unit.IGLineRun = CastDbNull.To<int>(reader["IGLineRun"], 0);
                        unit.LastLine = CastDbNull.To<String>(reader["LastLine"], String.Empty).Trim();
                        unit.Lite1Routing = CastDbNull.To<String>(reader["Lite1Routing"], String.Empty).Trim();
                        unit.Lite2Routing = CastDbNull.To<String>(reader["Lite2Routing"], String.Empty).Trim();
                        unit.Lite3Routing = CastDbNull.To<String>(reader["Lite3Routing"], String.Empty).Trim();
                        unit.UnitType = CastDbNull.To<String>(reader["UnitType"], String.Empty).Trim();
                        unit.BaseDimChar = CastDbNull.To<String>(reader["BaseDimChar"], String.Empty).Trim();
                        unit.LeftDimChar = CastDbNull.To<String>(reader["LeftDimChar"], String.Empty).Trim();
                        unit.EndPickItemNum = CastDbNull.To<Decimal>(reader["EndPickItemNum"], 0);
                        unit.CardinalProductType = CastDbNull.To<String>(reader["CardinalProductType"], String.Empty).Trim();
                        unit.DimMakeup = CastDbNull.To<String>(reader["Dimensions"], String.Empty).Trim();

                        unit.IsPurchasedLite = "N";
                        if (CastDbNull.To<String>(reader["Lite1_RecdStatus"], String.Empty).Trim() != String.Empty)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + CastDbNull.To<String>(reader["Lite1_RecdStatus"], String.Empty).Trim();
                        }
                        if (CastDbNull.To<String>(reader["Lite2_RecdStatus"], String.Empty).Trim() != String.Empty)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + CastDbNull.To<String>(reader["Lite2_RecdStatus"], String.Empty).Trim();
                        }
                        if (CastDbNull.To<String>(reader["Lite3_RecdStatus"], String.Empty).Trim() != String.Empty)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + CastDbNull.To<String>(reader["Lite3_RecdStatus"], String.Empty).Trim();
                        }

                        unit.RackCloseDate = CastDbNull.To<DateTime>(reader["RackLockDate"], (DateTime)SqlDateTime.MinValue);
                        unit.RackLockDate = CastDbNull.To<DateTime>(reader["RackLockDate"], (DateTime)SqlDateTime.MinValue);
                        unit.RackLockLoadDate = CastDbNull.To<DateTime>(reader["RackLockLoadDate"], (DateTime)SqlDateTime.MinValue);

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

        /*********************************************************************************
         * Deprecated
         *******************************************************************************/
        public SqlCommand GetOpenUnitsShopFloor(List<string> filters)
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
                LotNum,
                SerialNum,
                WOrderStatusCode,
                SOrderLastStatusCode,
                SOrderNextStatusCode,
                OrderDate,
                DueDate,
                OrigDueDate,
                WOrderDate,
                WOrderDueDate,
                DeliveryDateTime,
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
                GenericSeqNum,
                ProductType,
                PriorityCode,
                ReasonCode,
                CardinalProductType,
                CustomerShipPref04,
                CustomerShipPref05,
                SchedulerMsg1,
                PickingNotes,
                PackingNotes,
                IsPick,
                UnitCostAmt,
                UnitActualPriceAmt,
                UnitListPriceAmt,
                ItemNum,
                VolumeCode,
                StockTypeCode,
                UnitTypeCode,
                LitesNum,
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
                Construction,
                ProductCode,
                SpacerLength,
                BarDescription,
                Lite1Glass,
                Lite2Glass,
                Lite3Glass,
                Lite1Furnace,
                Lite2Furnace,
                Lite3Furnace,
                Lite1Routing,
                Lite2Routing,
                Lite3Routing,
                Lite1Lami,
                Lite2Lami,
                Lite3Lami,
                Lite1Holes,
                Lite2Holes,
                Lite3Holes,
                Spacer1,
                Spacer2,
                Air1MCode1,
                GridStyleCode,
                GridStyleGroupName,
                RackId,
                RackStatusCode,
                RackType,
                RackLocation,
                RackAssetId,
                RackScheduleNum,
                RackBoxNum,
                RackTransactionDate,
                RackLockDate,
                RackDetailStatusCode,
                Side,
                Stack,
                Row,
                Slot,
                Position,
                RackDetailTransactionDate,
                Lite1RecdStatus,
                Lite2RecdStatus,
                Lite3RecdStatus,
                LastMachineName,
                LastMachineLine,
                LastMachineDate,
                LastProductionLine,
                LastOperationCode,
                LastProductionDate,
                LastProductionDesc,
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
                UnitBillableSqft,
                UnitActualSqft,
                LoadNum,
                ShipDate,
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
                PlantReporting.ShopFloor.OpenUnits
            WHERE
                {filterString} 1=1";
            return cmd;
        }

    }
}

