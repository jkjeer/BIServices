using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class OpenUnitManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;

        public OpenUnitManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("OpenUnitsManager.GetLockLoad", GetLockLoad());
            hash.Add("OpenUnitsManager.GetOriginalSchedule", GetOriginalSchedule());
            hash.Add("OpenUnitsManager.GetOriginalUKID", GetOriginalUKID());
            hash.Add("OpenUnitsManager.GetOriginalRack", GetOriginalRack());
            hash.Add("OpenUnitsManager.GetOriginalRackId", GetOriginalRackId());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            _flexItemMgr = new FlexItemManager(session, "P5842300");
        }

        /*********************************************************************************
         * Get Open Units Dynamic
         *******************************************************************************/
        public iDB2Command GetOpenUnits(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
	        SELECT * FROM (
                SELECT 
                    RDMCU,
                    RDZ58RID, 
                    RDZ58SN,
                    RHZ58RT,
                    RHZ58BOX,
                    RHZ58HST,
                    RHZ58GWTT,
                    RHZ58RTWT,
                    RHURRF,
                    RHLOCN,
                    RDUPMJ,
                    RDTDAY,
                    RDZ58DST,
                    RDPQOH/1000 AS RDPQOH,
                    RDITM,
                    RDZ58SD,
                    IMSTKT,
                    S1Z58SCN ,
                    S1Z58SCD,
                    S1Z58SCTYP,
                    S5DRQJ,
                    S5SHAN,
                    S5Z58PT,
                    WADOCO,
                    WASRST,
                    WAAN8,
                    SDNXTR,
                    SDKCOO,
                    SDDOCO,
                    SDDCTO,
                    SDLNID/1000 AS SDLNID,
                    SDDRQJ,
                    SDOPDJ,
                    SDPDDJ,
                    SDDRQT,
                    SDAN8,
                    SDSHAN,
                    SDTRDJ,
                    SDVR01,
                    LHADDJ,
                    LHADTM,
                    ATZ58CP2,
                    ATZ58CP3,
                    ATZ58CP4,
                    ATZ58CSQNO,
                    ATZ58GSQNO,
                    ATZ5899010,
                    ATZ5899011,
                    ATZ5899012,
                    SDURCD,
                    AB1.ABALPH AS ABALPH1,
                    AB2.ABALPH AS ABALPH2,
                    CASE WHEN IMSRP4 = 'L' THEN '1' ELSE IMSRP4 END AS IMSRP4,
                    CMCFGID,
                    CPZ58CCV05,
                    ROW_NUMBER() OVER (PARTITION BY WADOCO, RDZ58SN ORDER BY S1UPMJ DESC, S1TDAY DESC) AS RN
	            FROM 
                    F580055
                    INNER JOIN F580056
                    ON	RDZ58RID = RHZ58RID 
                    INNER JOIN F5834A11 
                    INNER JOIN F5834A01
                    ON	S1MCU = S5MCU AND
	                    S1Z58SCN = S5Z58SCN AND
	                    S1Z58SCD = S5Z58SCD
                    ON	S5DOCO = RDDOC1 AND
	                    RDDOC1 > 0
                    INNER JOIN F4801
                    ON  WADOCO = S5DOCO
                    INNER JOIN F4101
                    ON IMITM = RDITM
                    INNER JOIN F4211
                    ON  SDDOCO = RDDOCO AND
                        SDDCTO = RDDCTO AND
                        SDKCOO = RDKCOO AND
                        SDLNID = RDLNID
                    INNER JOIN F580032
                    ON  ATDOCO = SDDOCO AND
                        ATDCTO = SDDCTO AND
                        ATKCOO = SDKCOO AND
                        ATLNID = SDLNID
                    INNER JOIN F0101 AB1
                    ON AB1.ABAN8 = WAAN8
                    INNER JOIN F0101 AB2
                    ON AB2.ABAN8 = SDSHAN
                    INNER JOIN F3201
                    ON  CMDOCO = WADOCO AND
                        CMDCTO = 'WO' AND
                        CMKCOO = '*' AND
                        CMPID != 'P580150' 
                    LEFT OUTER JOIN F580150
                    ON  LHLDNM = SDPSN AND
                        SDPSN > 0
                    LEFT OUTER JOIN F580375
                    ON CPMCU = SDMCU AND
                        CPSHAN = SDSHAN
	            WHERE 
                    {0}
                    WASRST NOT IN ('98','99') ) A
            WHERE A.RN = 1
            ORDER BY RDZ58SN
            FOR READ ONLY", filterString);
            return cmd;
        }

        public GlassUnits GetOpenUnits(BusinessSystemSession session, SqlConnection cnShopFloorRpt, SqlConnection cnShopFloor, SqlConnection cnDw, Plant plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null,
            List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, List<decimal> rackIdsList = null)
        {
            var units = new GlassUnits();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant
                filters.Add("RHMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("PlantNum1", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);
                filters.Add("RDMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("PlantNum2", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);
                filters.Add("SDMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("PlantNum3", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Date Range
                DateTime startDateDt = new DateTime(1900,1,1);
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
                    filters.Add("((RHZ58HST IN ('0', '1', '2', '3', '4', '5', '9') AND SDDRQJ >= ? AND SDDRQJ <= ?) OR (RHZ58HST IN ('7', '8') AND SDUPMJ = ?)) AND\r\n");
                    parameters.Add(new iDB2Parameter("DRQJ1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(startDateDt);
                    parameters.Add(new iDB2Parameter("DRQJ2", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(endDateDt);
                    parameters.Add(new iDB2Parameter("UPMJ", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(DateTime.Now);
                }
                else
                {
                    filters.Add("RHZ58HST IN ('0', '1', '2', '3', '4', '5', '9') AND SDDRQJ >= ? AND SDDRQJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("DRQJ1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(startDateDt);
                    parameters.Add(new iDB2Parameter("DRQJ2", iDB2DbType.iDB2Decimal));
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
                    filters.Add("SDTRDJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("TRDJ", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)orderDate);
                }

                // Customer
                if (custNum > 0)
                {
                    filters.Add("WAAN8 = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("WAAN8", iDB2DbType.iDB2Decimal));
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
                        parameters.Add(new iDB2Parameter("SHAN", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerShip;
                    }
                    filters.Add($"SDSHAN {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
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
                        parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 8;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = customerSold;
                    }
                    filters.Add($"SDAN8 {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                // Box Num
                if (boxNum > 0)
                {
                    filters.Add("RHZ58BOX = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("boxNum", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 5;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = boxNum;
                }

                // Rack Type
                if (!String.IsNullOrEmpty(rackType))
                {
                    filters.Add("RHZ58RT = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("rackType", iDB2DbType.iDB2Graphic, 10));
                    parameters.Last().Value = rackType;
                }

                // Schedule Type
                if (!String.IsNullOrEmpty(scheduleType))
                {
                    filters.Add("S1Z58SCTYP = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("scheduleType", iDB2DbType.iDB2Graphic, 10));
                    parameters.Last().Value = scheduleType;
                }

                // Unit Type
                if (unitTypeList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string unitType in unitTypeList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("unitType", iDB2DbType.iDB2Graphic, 1));
                        parameters.Last().Value = unitType;
                    }
                    filters.Add("IMSRP4 IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Schedule Num
                if (scheduleNumList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (decimal scheduleNum in scheduleNumList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("ScheduleNums", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 5;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = scheduleNum;
                    }
                    filters.Add("S5Z58SCN IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Rack Ids
                if (rackIdsList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (decimal rackId in rackIdsList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("RackIds", iDB2DbType.iDB2Decimal));
                        parameters.Last().Precision = 15;
                        parameters.Last().Scale = 0;
                        parameters.Last().Value = rackId;
                    }
                    filters.Add("RDZ58RID IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Rejects Not Completed
                if (optionsList.Contains("RNC") || optionsList.Contains("PRNC"))
                {
                    rackStatusList.Add("1");
                    //filters.Clear();
                    //parameters.Clear();

                    //filters.Add("RHMCU = ? AND\r\n");
                    //parameters.Add(new iDB2Parameter("PlantNum1", iDB2DbType.iDB2Graphic, 12));
                    //parameters.Last().Value = plant.PlantNum.PadLeft(12);

                    //filters.Add("SDDRQJ <= ? AND\r\n");
                    //parameters.Add(new iDB2Parameter("RequestedDateId", iDB2DbType.iDB2Decimal));
                    //parameters.Last().Precision = 6;
                    //parameters.Last().Scale = 0;
                    //parameters.Last().Value = 125000;

                    //filters.Add("RDZ58DST = ? AND\r\n");
                    //parameters.Add(new iDB2Parameter("Status", iDB2DbType.iDB2Graphic, 10));
                    //parameters.Last().Value = "1";
                }

                // Rack Status
                if (rackStatusList.Count > 0)
                {
                    string strInClause = "";
                    foreach (string rackStatus in rackStatusList)
                    {
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("rackStatus", iDB2DbType.iDB2Graphic, 1));
                        parameters.Last().Value = rackStatus;
                    }
                    filters.Add("RDZ58DST IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                cmd = GetOpenUnits(filters);
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
                        var unit = new GlassUnit();
                        unit.Plant = CastDbNull.To<String>(reader["RDMCU"], String.Empty).Trim();
                        unit.UnitIdChar = CastDbNull.To<String>(reader["RDZ58SN"], String.Empty).Trim();
                        unit.UnitId = Convert.ToDecimal(unit.UnitIdChar);
                        unit.RackId = CastDbNull.To<Decimal>(reader["RDZ58RID"], 0);
                        unit.AssetId = CastDbNull.To<String>(reader["RHURRF"], String.Empty).Trim();
                        unit.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
                        unit.RackLocation = CastDbNull.To<String>(reader["RHLOCN"], String.Empty).Trim();
                        unit.RackSide = CastDbNull.To<String>(reader["RDZ58SD"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<Decimal>(reader["RHZ58BOX"], 0);
                        unit.RackTareWeight = CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0) / 100;
                        unit.RackStatus = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                        unit.Status = CastDbNull.To<String>(reader["RDZ58DST"], String.Empty).Trim();
                        unit.ScheduleNum = CastDbNull.To<Decimal>(reader["S1Z58SCN"], 0);
                        unit.ScheduleDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["S1Z58SCD"], 0));
                        unit.OrderCompany = CastDbNull.To<String>(reader["SDKCOO"], String.Empty).Trim();
                        unit.OrderNum = CastDbNull.To<Decimal>(reader["SDDOCO"], 0);
                        unit.OrderType = CastDbNull.To<String>(reader["SDDCTO"], String.Empty).Trim();
                        unit.OrderLineNum = Math.Round(CastDbNull.To<Decimal>(reader["SDLNID"], 0), 3);
                        unit.WorkOrderNum = CastDbNull.To<Decimal>(reader["WADOCO"], 0);
                        unit.WorkOrderStatus = CastDbNull.To<String>(reader["WASRST"], String.Empty).Trim();
                        unit.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
                        unit.ItemStockType = CastDbNull.To<String>(reader["IMSTKT"], String.Empty).Trim();

                        unit.ScheduleType = CastDbNull.To<String>(reader["S1Z58SCTYP"], String.Empty).Trim();
                        unit.ProductType = CastDbNull.To<String>(reader["S5Z58PT"], String.Empty).Trim();
                        var orderDateId = CastDbNull.To<Decimal>(reader["SDTRDJ"], 0);
                        unit.OrderDate = Common.Utilities.Common.CastToDateTime(orderDateId, 0);
                        var dateId = CastDbNull.To<Decimal>(reader["RDUPMJ"], 0);
                        var timeId = CastDbNull.To<Decimal>(reader["RDTDAY"], 0);
                        unit.RackItemDate = Common.Utilities.Common.CastToDateTime(dateId, timeId).AddHours(plant.TimeOffset);
                        var dateDrqj = CastDbNull.To<Decimal>(reader["SDDRQJ"], 0);
                        unit.RequestedDate = Common.Utilities.Common.CastToDateTime(dateDrqj, 0);
                        unit.PromisedDate = Common.Utilities.Common.CastToDateTime(CastDbNull.To<Decimal>(reader["SDOPDJ"], 0), 0);
                        unit.NextStatus = CastDbNull.To<String>(reader["SDNXTR"], String.Empty).Trim();

                        // Delivery Time per Order Entry
                        var delDateId = CastDbNull.To<Decimal>(reader["SDPDDJ"], 0);
                        var delTimeId = CastDbNull.To<Decimal>(reader["SDDRQT"], 0);
                        unit.DeliveryDateTime = Common.Utilities.Common.CastToDateTime(delDateId, delTimeId);
                        TimeSpan ts = unit.DeliveryDateTime - DateTime.Now;
                        unit.DeliveryDateTimeHours = Math.Round(ts.TotalMinutes / 60.00, 3);
                        unit.CustomerTier = CastDbNull.To<String>(reader["CPZ58CCV05"], String.Empty).Trim();

                        unit.CustomerNum = CastDbNull.To<Decimal>(reader["WAAN8"], 0);
                        unit.CustomerName = CastDbNull.To<String>(reader["ABALPH1"], String.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        unit.CustomerShipName = CastDbNull.To<String>(reader["ABALPH2"], String.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<String>(reader["ATZ58CP2"], String.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<String>(reader["ATZ58CP3"], String.Empty).Trim();
                        unit.CustomerPart3 = CastDbNull.To<String>(reader["ATZ58CP4"], String.Empty).Trim();
                        unit.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                        unit.CustomerSeqNum = CastDbNull.To<String>(reader["ATZ58CSQNO"], String.Empty).Trim();
                        unit.SchedulingMessage = CastDbNull.To<string>(reader["ATZ5899010"], string.Empty).Trim();
                        var arr = unit.CustomerSeqNum.Split('/');
                        if (arr.Length > 0)
                        {
                            unit.CustomerSeqPart1 = arr[0];
                        }
                        unit.GenericSeqNum = CastDbNull.To<String>(reader["ATZ58GSQNO"], String.Empty).Trim();
                        unit.PickNotes = CastDbNull.To<String>(reader["ATZ5899011"], String.Empty).Trim();

                        // PELLA BUCKET SLOT
                        if (unit.CustomerNum == 10065870)
                        {
                            var arrPickNotes = unit.PickNotes.Split('.');
                            if (arrPickNotes.Length > 1)
                            {
                                unit.PickNotes1 = arrPickNotes[0].Trim();
                                unit.PickNotes2 = arrPickNotes[1].Trim();
                            }

                        }

                        unit.PackNotes = CastDbNull.To<String>(reader["ATZ5899012"], String.Empty).Trim();

                        // Customer PO Parsing
                        unit.CustomerPO = CastDbNull.To<String>(reader["SDVR01"], String.Empty).Trim();
                        var tokens = unit.CustomerPO.Split('-', '_');
                        if (tokens.Length > 0)
                        {
                            unit.CustomerPONum = Common.Utilities.Data.ParseStringToDecimal(tokens[0]);
                        }

                        if (tokens.Length > 1)
                        {
                            unit.CustomerPOLineNum = Common.Utilities.Data.ParseStringToDecimal(tokens[tokens.Length - 1]);
                        }

                        var shipDateId = CastDbNull.To<Decimal>(reader["LHADDJ"], 0);
                        var shipTimeId = CastDbNull.To<Decimal>(reader["LHADTM"], 0);
                        if (shipDateId > 0) unit.ShipDateTime = Common.Utilities.Common.CastToDateTime(shipDateId, shipTimeId).AddHours(plant.TimeOffset);
                        unit.IGLineScheduled = "";
                        unit.IsMissing = "N";
                        unit.OnHand = Convert.ToInt16(CastDbNull.To<Decimal>(reader["RDPQOH"], 0));
                        unit.Units = 1;
                        unit.ConfigId = CastDbNull.To<Decimal>(reader["CMCFGID"], 0);
                        units.Add(unit);
                    }

                    reader.Close();
                }

                _logger.Info("Units Returned: " + units.Count);
                if (units.Count > 0)
                {

                    // Delivery Time per HRIG Shipping Lane
                    if (plant.PlantCode == "HRIG")
                    {
                        var custShipLanes = ShippingLaneManager.GetCustomerLane(cnShopFloor);
                        foreach (var unit in units)
                        {
                            if (custShipLanes.ContainsKey(unit.CustomerShipNum))
                            {
                                var lane = custShipLanes[unit.CustomerShipNum];
                                unit.ShipRoute = lane.Name;
                                unit.DeliveryDateTime = unit.RequestedDate.AddHours((double)lane.Offset);
                                TimeSpan ts = unit.DeliveryDateTime - DateTime.Now.AddHours(plant.TimeOffset);
                                unit.DeliveryDateTimeHours = Math.Round(ts.TotalMinutes / 60.00, 3);
                            }
                        }
                    }

                    if (hoursToTruck != 0)
                    {
                        var x = units.RemoveAll(a => (a.DeliveryDateTimeHours > (double)hoursToTruck));
                    }

                    _logger.Info("E1 Get Scheduled Rack Start");
                    GetOriginalSchedule(units, session, plant);

                    if (optionsList.Contains("MU"))
                    {
                        _logger.Info("Local IsMissing Elimination");
                        var x = units.RemoveAll(a => a.IsMissing == "N");
                    }

                    if (optionsList.Contains("XHVMTO"))
                    {
                        var x = units.RemoveAll(a => a.ScheduleType == "HV MTO");
                    }

                    _logger.Info("Shop Floor Temp Table Start");
                    InsertTempTable(units, cnShopFloor);

                    _logger.Info("Shop Floor Get Remakes Start");
                    GetRemake(units, cnShopFloor);
                    if (optionsList.Contains("XR"))
                    {
                        var x = units.RemoveAll(a => a.RemakeScheduleNum > 0 || a.RackType == "EXCPT");
                    }

                    // Only for IGs 
                    string[] igPlants = { "02", "03", "06", "14", "16", "17", "19", "22", "25", "28", "45" };
                    if (igPlants.Contains(plant.PlantNum))
                    {
                        _logger.Debug("Shop Floor Get Remake Passes Start");
                        GetRemakePasses(units, cnShopFloor);
                        if (plant.PlantCode == "BEIG" || plant.PlantCode == "WBIG" || plant.PlantCode == "OCIG")
                        {
                            GetSealerStatus(units, cnShopFloor);
                            GetLastMachineTransactions(units, cnShopFloor);
                            GetLastStationTransactions(units, cnShopFloor);
                        }
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
                    _logger.Debug("Option RBASEQ");
                    if (optionsList.Contains("RBASEQ"))
                    {
                        foreach (var unit in units)
                        {
                            unit.RackFirstCustSeqNum = units.Where(x => x.RackId == unit.RackId).Min(z => z.CustomerPart1);
                            unit.RackLastCustSeqNum = units.Where(x => x.RackId == unit.RackId).Max(z => z.CustomerPart1);
                        }
                    }

                    // specific IG Lines from Shop Floor interface
                    _logger.Info("Shop Floor Get Attributes Start");
                    string[] cgPlants = { "04", "05", "10", "11", "19", "20" };
                    if (cgPlants.Contains(plant.PlantNum))
                    {
                        GetAttributesCg(units, cnShopFloor, plant);
                    }
                    else
                    {
                        GetAttributes(units, cnShopFloor, plant);
                    }


                    // Only for IGs 
                    if (igPlants.Contains(plant.PlantNum))
                    {
                        _logger.Info("Shop Floor Get Purchased Lites Start");
                        GetPurchasedLites(units, cnShopFloor);
                    }

                    if (optionsList.Contains("PRNC"))
                    {
                        var x = units.RemoveAll(a => a.IsPurchasedLite != "Y");
                    }

                    // QC Inspect
                    if (optionsList.Contains("QC"))
                    {
                        var x = units.RemoveAll(a => a.QcInspect == "N");
                    }

                    if (igLinesList.Any())
                    {
                        var x = units.RemoveAll(a => (!igLinesList.Contains(a.IGLineRun)));
                    }
                    else
                    {
                        _logger.Info("E1 Get Rack Lock Load Start");
                        GetLockLoad(units, session, plant);
                    }

                    _logger.Info("Shop Floor Get Last Production Start");
                    GetLastProduction(units, cnShopFloor);

                    _logger.Info("Item Att / Flex Start");
                    if (plant.PlantCode == "FMIG")
                    {
                        foreach (GlassUnit unit in units)
                        {
                            try
                            {
                                var itemAttributes = _itemAttMgr.GetConfiguredItemAttributes(session, unit.ConfigId);
                                var flexDescList = _flexItemMgr.GetDescription(session, itemAttributes);
                                unit.UnitFlex = flexDescList[0].Trim();
                                unit.DimFlex = flexDescList[1].Trim();
                                unit.AirFlex = flexDescList[2].Trim();
                                unit.PreserveFlex = flexDescList[3].Trim();
                                unit.BarPlaceFlex = flexDescList[4].Trim();
                                unit.ProcessingFlex = flexDescList[5].Trim();
                                unit.VGrooveFlex = flexDescList[6].Trim();
                                unit.RoutingFlex = flexDescList[7].Trim();
                            }
                            catch (Exception e)
                            {
                                throw new Exception(e.Message);
                            }
                        }
                    }

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
         * Shop Floor Temp Table
         *******************************************************************************/
        public SqlCommand CreateTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"CREATE TABLE #TempUnitList (UnitId int, UnitIdChar varchar(20), WorkOrderNum int)";
            return cmd;
        }

        public SqlCommand InsertTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"INSERT #TempUnitList (UnitId, UnitIdChar, WorkOrderNum) VALUES(@UnitId, @UnitIdChar, @WorkOrderNum)";
            cmd.Parameters.Add(new SqlParameter("@UnitId", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@UnitIdChar", SqlDbType.VarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@WorkOrderNum", SqlDbType.Int));
            return cmd;
        }

        public void InsertTempTable(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd1 = CreateTempTable();
                cmd1.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd1));
                int rows = cmd1.ExecuteNonQuery();

                DataTable dt = new DataTable("Units");
                dt.Columns.Add("UnitId", typeof(int));
                dt.Columns.Add("UnitIdChar", typeof(string));
                dt.Columns.Add("WorkOrderNum", typeof(int));
                //var uniqueUnits = units.GroupBy(unit => unit.UnitId).Select(grp => grp.First());
                //foreach (GlassUnit unit in uniqueUnits)
                foreach (GlassUnit unit in units)
                {
                    DataRow row = dt.NewRow();
                    row["UnitId"] = unit.UnitId;
                    row["UnitIdChar"] = unit.UnitIdChar;
                    row["WorkOrderNum"] = (int)unit.WorkOrderNum;
                    dt.Rows.Add(row);
                }
                var bulk = new SqlBulkCopy(cn)
                {
                    DestinationTableName = "#TempUnitList",
                    BulkCopyTimeout = 300,
                    BatchSize = 25000
                };
                bulk.WriteToServer(dt);
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Last Line
         *******************************************************************************/
        public SqlCommand GetLastProduction()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              UnitId ,
              LineID,
              ProductionLineCode ,
			  LineOperationCode,
              WrittenDate
            FROM
              ( SELECT
                  ub.UnitId ,
                  PD.LineID,
                  CASE WHEN ISNULL(PD.WrittenDate, '1/1/1900') > ISNULL(PL.WrittenDate, '1/1/1900') THEN PD.ProductionLineCode ELSE PL.ProductionLineCode END AS ProductionLineCode,
                  CASE WHEN ISNULL(PD.WrittenDate, '1/1/1900') > ISNULL(PL.WrittenDate, '1/1/1900') THEN PD.LineOperationCode ELSE PL.LineOperationCode END AS LineOperationCode,
                  CASE WHEN ISNULL(PD.WrittenDate, '1/1/1900') > ISNULL(PL.WrittenDate, '1/1/1900') THEN PD.WrittenDate ELSE PL.WrittenDate END AS WrittenDate,
                  ROW_NUMBER() OVER ( PARTITION BY ub.UnitId ORDER BY CASE WHEN PD.WrittenDate > PL.WrittenDate THEN PD.WrittenDate ELSE PL.WrittenDate END  DESC ) AS ROW_NUM
                FROM
				 #TempUnitList t
                  INNER JOIN UnitBox ub WITH (NOLOCK)
                  ON t.UnitId = ub.UnitId
                  LEFT OUTER JOIN ProdData PD WITH ( NOLOCK )
                  ON PD.UnitID = CAST(ub.UnitID AS VARCHAR(8)) AND
                     PD.OrderNum = ub.OrderItem    
                  LEFT OUTER JOIN ClearPath.ClearPathReporting.ProdLites pl WITH ( NOLOCK )
                  ON PL.SerialNumber = CAST(ub.UnitID AS VARCHAR(8)) AND
                     PL.WorkOrderNum = ub.OrderItem    
              ) A
            WHERE
              A.ROW_NUM = 1";
            //SELECT
            //  UnitId ,
            //  ProductionLineCode ,
            //  LineID,
            //  WrittenDate
            //FROM
            //  ( SELECT
            //      ub.UnitId ,
            //      PD.ProductionLineCode ,
            //      PD.LineID,
            //      PD.WrittenDate ,
            //      ROW_NUMBER() OVER ( PARTITION BY ub.UnitId ORDER BY PD.WrittenDate DESC ) AS ROW_NUM
            //    FROM
            //      UnitBox ub WITH (NOLOCK)
            //      INNER JOIN #TempUnitList t
            //        ON t.UnitId = ub.UnitId
            //      INNER JOIN ProdData PD WITH ( NOLOCK )
            //      ON PD.UnitID = CAST(ub.UnitID AS VARCHAR(8)) AND
            //         PD.OrderNum = ub.OrderItem    
            //  ) A
            //WHERE
            //  A.ROW_NUM = 1";
            return cmd;
        }

        public void GetLastProduction(GlassUnits units, SqlConnection cn)
        {
            try
            {
                if (units.Count > 0)
                {
                    var cmd = GetLastProduction();
                    cmd.Connection = cn;
                    cmd.CommandTimeout = 300;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        var dict = new Dictionary<decimal, Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                            var lastProdRec = new Dictionary<string, object>();
                            lastProdRec.Add("lastProductionLine", CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim());
                            lastProdRec.Add("lastLineOperation", CastDbNull.To<String>(reader["LineOperationCode"], String.Empty).Trim());
                            lastProdRec.Add("LineID", CastDbNull.To<String>(reader["LineID"], String.Empty).Trim());
                            lastProdRec.Add("lastLineDate", CastDbNull.To<DateTime>(reader["WrittenDate"], new DateTime(1900, 1, 1)));
                            dict.Add(unitId, lastProdRec);
                        }

                        reader.Close();

                        foreach (GlassUnit unit in units)
                        {
                            var lastProdRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                            if (lastProdRec != null)
                            {
                                unit.LastLine = (string) lastProdRec["LineID"];
                                unit.LastProductionLine = (string)lastProdRec["lastProductionLine"];
                                unit.LastLineOperation = (string)lastProdRec["lastLineOperation"];
                                unit.LastLineDate = (DateTime) lastProdRec["lastLineDate"];
                                //unit.Passes = 1;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Remakes
         *******************************************************************************/
        public SqlCommand GetRemake()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              r.RemakesId,
              r.UnitId ,
              r.RemakeSchedule,
              r.WrittenDate,
              r.InitiateDate + CAST(r.InitiateTime AS DATETIME) AS InitiateDateTime,
              CASE 
	            WHEN ISNULL(r.UnitReason, '') != '' THEN UnitReason
	            WHEN ISNULL(r.InteriorReason, '') != '' THEN InteriorReason
	            WHEN ISNULL(r.ExteriorReason, '') != '' THEN ExteriorReason
	            WHEN ISNULL(r.ThirdliteReason, '') != '' THEN ThirdliteReason
	            WHEN ISNULL(r.SpacerReason, '') != '' THEN SpacerReason
	            WHEN ISNULL(r.Spacer2Reason, '') != '' THEN Spacer2Reason
              END AS Reason,
              r.Comments
            FROM
              Remakes r WITH (NOLOCK)
              INNER JOIN #TempUnitList t
              on t.UnitIdChar = r.UnitId AND
                t.WorkOrderNum = r.OrderNum";
            return cmd;
        }

        public void GetRemake(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetRemake();
                //var cmd = GetRemake(units);
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var remakeRec = new Dictionary<string, object>();
                        var unitId = Common.Utilities.Common.CastStringToDecimal(CastDbNull.To<String>(reader["UnitId"], String.Empty).Trim());
                        remakeRec.Add("RemakesId", CastDbNull.To<int>(reader["RemakesId"], 0));
                        remakeRec.Add("ScheduleNum", Common.Utilities.Common.CastStringToDecimal(CastDbNull.To<String>(reader["RemakeSchedule"], String.Empty).Trim()));
                        remakeRec.Add("InitiateDate", CastDbNull.To<DateTime>(reader["InitiateDateTime"], new DateTime(1900, 1, 1)));
                        remakeRec.Add("Reason", CastDbNull.To<String>(reader["Reason"], String.Empty).Trim());
                        remakeRec.Add("Comments", CastDbNull.To<String>(reader["Comments"], String.Empty).Trim());
                        if (!dict.ContainsKey(unitId))
                        {
                            dict.Add(unitId, remakeRec);
                        }
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        var remakeRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (remakeRec != null)
                        {
                            unit.RemakeId = (int) remakeRec["RemakesId"];
                            unit.RemakeScheduleNum = (decimal) remakeRec["ScheduleNum"];
                            unit.RemakeInitiateDate = (DateTime) remakeRec["InitiateDate"];
                            unit.RemakeReason = (string) remakeRec["Reason"];
                            unit.RemakeComment = (string) remakeRec["Comments"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Passes
         *******************************************************************************/
        public SqlCommand GetRemakePasses()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              TRY_CAST(pd.UnitId AS INT) AS UnitId ,
              COUNT(DISTINCT pd.CurrentSchedule) AS Passes
            FROM
                ProdData pd WITH (NOLOCK)
                INNER JOIN IncomingOrders ord WITH (NOLOCK)
                ON ord.WOrderNum = pd.OrderNum
                INNER JOIN #TempUnitList t
                ON t.UnitIdChar = pd.UnitId and
                t.WorkOrderNum = pd.OrderNum
            GROUP BY
              pd.UnitID";
            return cmd;
        }

        public void GetRemakePasses(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetRemakePasses();
                //var cmd = GetRemakePasses(units);
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, object[]>();
                    while (reader.Read())
                    {
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        var passes = CastDbNull.To<int>(reader["Passes"], 0);
                        dict.Add(unitId, new object[]
                        {
                            passes
                        });
                    }

                    reader.Close();

                    foreach (GlassUnit unit in units)
                    {
                        if (dict.ContainsKey(unit.UnitId))
                        {
                            unit.Passes = (int) dict[unit.UnitId][0];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Attributes - IG, CT
         *******************************************************************************/
        public SqlCommand GetAttributes()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT DISTINCT
                ub.UnitId ,
                ub.CustName,
                ub.Box,
                ub.Pos,
                ub.RunSeq,
                ub.Construction,
                ub.BarDescription,
                ub.DimBase,
                ub.DimLeft,
                ub.ShapeNum,
                ic.QcInspect,
                ic.UnitWeight,
                CASE WHEN ic.Lite1_Base_Num > 0 THEN CAST(ic.Lite1_Base_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Base_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Base_Den AS VARCHAR) ELSE CAST(ic.Lite1_Base_Leg AS VARCHAR) END AS DimBaseChar,
                CASE WHEN ic.Lite1_Left_Num > 0 THEN CAST(ic.Lite1_Left_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Left_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Left_Den AS VARCHAR) ELSE CAST(ic.Lite1_Left_Leg AS VARCHAR) END AS DimLeftChar,
                CASE WHEN ic.Lite1_Right_Num > 0 THEN CAST(ic.Lite1_Right_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Right_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Right_Den AS VARCHAR) ELSE CAST(ic.Lite1_Right_Leg AS VARCHAR) END AS DimRightChar,
                CASE WHEN ic.Lite1_Top_Num > 0 THEN CAST(ic.Lite1_Top_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Top_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Top_Den AS VARCHAR) ELSE CAST(ic.Lite1_Top_Leg AS VARCHAR) END AS DimTopChar,
                CASE WHEN ic.Lite1_S1_Num > 0 THEN CAST(ic.Lite1_S1_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_S1_Num AS VARCHAR) + '/' + CAST(ic.Lite1_S1_Den AS VARCHAR) ELSE CAST(ic.Lite1_S1_Leg AS VARCHAR) END AS DimS1Char,
                CASE WHEN ic.Lite1_S2_Num > 0 THEN CAST(ic.Lite1_S2_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_S2_Num AS VARCHAR) + '/' + CAST(ic.Lite1_S2_Den AS VARCHAR) ELSE CAST(ic.Lite1_S2_Leg AS VARCHAR) END AS DimS2Char, 
                ub.IGLineRun,
                LTRIM(RTRIM(ic.Lite1_Routing)) AS Lite1_Routing,
                LTRIM(RTRIM(ic.Lite2_Routing)) AS Lite2_Routing,
                LTRIM(RTRIM(ic.Lite3_Routing)) AS Lite3_Routing,
                CASE
                    WHEN ic.Lite1_Lami <> '' OR ic.Lite2_Lami <> '' OR ic.Lite3_Lami <> '' THEN 'Y' ELSE 'N' 
                END AS IsLami,
                CASE 
                    WHEN ub.GlassThick2 > 0 THEN '3' 
                    WHEN ub.GlassThick1 > 0 THEN '2' 
                    ELSE '1' 
                END AS UnitType,
                CASE 
                    WHEN ub.GlassThick2 > 0 THEN ub.Glass1 + ' / ' + ub.Spacer1 + ' / ' + ub.Glass2 + ' / ' +  ub.Spacer2 + ' / ' +  ub.Glass3 
                    WHEN ub.GlassThick1 > 0 THEN ub.Glass1 + ' / ' + ub.Spacer1 + ' / ' + ub.Glass2 
                    ELSE ub.Glass1
                END AS UnitConstruction,
                CASE WHEN ub.Tubes = 1 THEN 1 ELSE 0 END AS Tubes,
                CASE WHEN ub.ShapeNum > 0 THEN 1 ELSE 0 END AS Shapes,
                CASE WHEN ub.PreserveExt > 0 OR ub.PreserveInt > 0 THEN 1 ELSE 0 END AS Preserve,
                CASE WHEN ub.ScoreOpenings > 0 THEN 1 ELSE 0 END AS Scoring,
                CASE WHEN ub.MCode > 0 THEN 1 ELSE 0 END AS Bars,
                CASE WHEN ub.PickUnit > 0 THEN 1 ELSE 0 END AS Picks,
                CASE WHEN ub.Purchase0 = 1 OR ub.Purchase1 = 1 OR ub.Purchase2 = 1 THEN 1 ELSE 0 END as Purs
            FROM
                UnitBox ub WITH (NOLOCK)
                INNER JOIN IncomingOrders ic WITH (NOLOCK)
                ON  ic.WOrderNum = ub.OrderItem
                INNER JOIN #TempUnitList t
                ON  t.UnitId = ub.UnitId and
                    t.WorkOrderNum = ub.OrderItem";
            return cmd;
        }

        public void GetAttributes(GlassUnits units, SqlConnection cn, Plant plant)
        {
            try
            {
                var cmd = GetAttributes();
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var attributeRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        attributeRec.Add("CustName", CastDbNull.To<String>(reader["CustName"], String.Empty).Trim());
                        attributeRec.Add("Box", CastDbNull.To<int>(reader["Box"], 0));
                        attributeRec.Add("Slot", CastDbNull.To<int>(reader["Pos"], 0));
                        attributeRec.Add("RunSeq", CastDbNull.To<int>(reader["RunSeq"], 0));
                        attributeRec.Add("Construction", CastDbNull.To<String>(reader["Construction"], String.Empty).Trim());
                        attributeRec.Add("BarDescription", CastDbNull.To<String>(reader["BarDescription"], String.Empty).Trim());
                        var uc = CastDbNull.To<String>(reader["UnitConstruction"], String.Empty).Trim();
                        attributeRec.Add("UnitConstruction", System.Text.RegularExpressions.Regex.Replace(uc, @"\s{2,}", " "));
                        attributeRec.Add("BaseDim", CastDbNull.To<Double>(reader["DimBase"], 0));
                        attributeRec.Add("LeftDim", CastDbNull.To<Double>(reader["DimLeft"], 0));
                        attributeRec.Add("BaseDimChar", CastDbNull.To<String>(reader["DimBaseChar"], String.Empty).Trim());
                        attributeRec.Add("LeftDimChar", CastDbNull.To<String>(reader["DimLeftChar"], String.Empty).Trim());
                        attributeRec.Add("RightDimChar", CastDbNull.To<String>(reader["DimRightChar"], String.Empty).Trim());
                        attributeRec.Add("TopDimChar", CastDbNull.To<String>(reader["DimTopChar"], String.Empty).Trim());
                        attributeRec.Add("S1DimChar", CastDbNull.To<String>(reader["DimS1Char"], String.Empty).Trim());
                        attributeRec.Add("S2DimChar", CastDbNull.To<String>(reader["DimS2Char"], String.Empty).Trim());
                        attributeRec.Add("ShapeNum", CastDbNull.To<int>(reader["ShapeNum"], 0));
                        attributeRec.Add("QcInspect", CastDbNull.To<String>(reader["QcInspect"], String.Empty).Trim());
                        attributeRec.Add("IsLami", CastDbNull.To<String>(reader["IsLami"], String.Empty).Trim());
                        attributeRec.Add("UnitWeight", CastDbNull.To<decimal>(reader["UnitWeight"], 0));
                        attributeRec.Add("IGLineRun", CastDbNull.To<int>(reader["IGLineRun"], 0));
                        attributeRec.Add("Lite1Routing", CastDbNull.To<String>(reader["Lite1_Routing"], String.Empty).Trim());
                        attributeRec.Add("Lite2Routing", CastDbNull.To<String>(reader["Lite2_Routing"], String.Empty).Trim());
                        attributeRec.Add("Lite3Routing", CastDbNull.To<String>(reader["Lite3_Routing"], String.Empty).Trim());
                        attributeRec.Add("UnitType", CastDbNull.To<String>(reader["UnitType"], String.Empty).Trim());
                        attributeRec.Add("Tubes", CastDbNull.To<int>(reader["Tubes"], 0));
                        attributeRec.Add("Shapes", CastDbNull.To<int>(reader["Shapes"], 0));
                        attributeRec.Add("Preserve", CastDbNull.To<int>(reader["Preserve"], 0));
                        attributeRec.Add("Scoring", CastDbNull.To<int>(reader["Scoring"], 0));
                        attributeRec.Add("Bars", CastDbNull.To<int>(reader["Bars"], 0));
                        attributeRec.Add("Picks", CastDbNull.To<int>(reader["Picks"], 0));
                        attributeRec.Add("Purs", CastDbNull.To<int>(reader["Purs"], 0));
                        dict.Add(unitId, attributeRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        var attributeRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (attributeRec != null)
                        {
                            unit.CustomerNameUnitBox = (string) attributeRec["CustName"];
                            unit.BoxNum = (int) attributeRec["Box"];
                            unit.SlotNum = (int) attributeRec["Slot"];
                            unit.RunSequence = (int) attributeRec["RunSeq"];
                            //unit.Construction = (string) attributeRec["Construction"];
                            unit.Construction = (string) attributeRec["UnitConstruction"];
                            unit.IsLami = (string) attributeRec["IsLami"];
                            unit.BarDescription = (string) attributeRec["BarDescription"];
                            unit.BaseDim = (double) attributeRec["BaseDim"];
                            unit.LeftDim = (double) attributeRec["LeftDim"];
                            unit.ShapeNum = (int) attributeRec["ShapeNum"];
                            unit.QcInspect = (string) attributeRec["QcInspect"];
                            unit.GlassWeight = (decimal) attributeRec["UnitWeight"];
                            unit.Tubes = (int) attributeRec["Tubes"];
                            unit.Shapes = (int) attributeRec["Shapes"];
                            unit.Preserve = (int) attributeRec["Preserve"];
                            unit.Scoring = (int) attributeRec["Scoring"];
                            unit.Bars = (int) attributeRec["Bars"];
                            unit.Picks = (int) attributeRec["Picks"];
                            unit.Purchases = (int) attributeRec["Purs"];
                            unit.IGLineRun = (int) attributeRec["IGLineRun"];
                            unit.LastLine = attributeRec["IGLineRun"].ToString();
                            unit.Lite1Routing = (string) attributeRec["Lite1Routing"];
                            unit.Lite2Routing = (string) attributeRec["Lite2Routing"];
                            unit.Lite3Routing = (string) attributeRec["Lite3Routing"];
                            unit.UnitType = (string) attributeRec["UnitType"];
                            unit.BaseDimChar = (string) attributeRec["BaseDimChar"];
                            unit.LeftDimChar = (string) attributeRec["LeftDimChar"];

                            var sb = new StringBuilder();
                            if ((string) attributeRec["BaseDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["BaseDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["LeftDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["LeftDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["RightDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["RightDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["TopDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["TopDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["S1DimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["S1DimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["S2DimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["S2DimChar"]);
                                sb.Append(" X ");
                            }

                            var dimMakeup = sb.ToString();
                            unit.DimMakeup = dimMakeup.Substring(0, dimMakeup.Length - 3);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Attributes - CG
          *******************************************************************************/
        public SqlCommand GetAttributesCg()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT DISTINCT
                ub.UnitId ,
                ub.CustName,
                ub.Box,
                ub.Pos,
                ub.RunSeq,
                ub.Construction,
                ub.BarDescription,
                ub.DimBase,
                ub.DimLeft,
                ub.ShapeNum,
                ic.UnitWeight,
                CASE WHEN ic.Lite1_Base_Num > 0 THEN CAST(ic.Lite1_Base_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Base_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Base_Den AS VARCHAR) ELSE CAST(ic.Lite1_Base_Leg AS VARCHAR) END AS DimBaseChar,
                CASE WHEN ic.Lite1_Left_Num > 0 THEN CAST(ic.Lite1_Left_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Left_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Left_Den AS VARCHAR) ELSE CAST(ic.Lite1_Left_Leg AS VARCHAR) END AS DimLeftChar,
                CASE WHEN ic.Lite1_Right_Num > 0 THEN CAST(ic.Lite1_Right_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Right_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Right_Den AS VARCHAR) ELSE CAST(ic.Lite1_Right_Leg AS VARCHAR) END AS DimRightChar,
                CASE WHEN ic.Lite1_Top_Num > 0 THEN CAST(ic.Lite1_Top_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_Top_Num AS VARCHAR) + '/' + CAST(ic.Lite1_Top_Den AS VARCHAR) ELSE CAST(ic.Lite1_Top_Leg AS VARCHAR) END AS DimTopChar,
                CASE WHEN ic.Lite1_S1_Num > 0 THEN CAST(ic.Lite1_S1_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_S1_Num AS VARCHAR) + '/' + CAST(ic.Lite1_S1_Den AS VARCHAR) ELSE CAST(ic.Lite1_S1_Leg AS VARCHAR) END AS DimS1Char,
                CASE WHEN ic.Lite1_S2_Num > 0 THEN CAST(ic.Lite1_S2_Leg AS VARCHAR) + ' ' + CAST(ic.Lite1_S2_Num AS VARCHAR) + '/' + CAST(ic.Lite1_S2_Den AS VARCHAR) ELSE CAST(ic.Lite1_S2_Leg AS VARCHAR) END AS DimS2Char, 
                ub.IGLineRun,
                LTRIM(RTRIM(ic.Lite1_Routing)) AS Lite1_Routing,
                LTRIM(RTRIM(ic.Lite2_Routing)) AS Lite2_Routing,
                LTRIM(RTRIM(ic.Lite3_Routing)) AS Lite3_Routing,
                CASE 
                    WHEN ub.GlassThick2 > 0 THEN '3' 
                    WHEN ub.GlassThick1 > 0 THEN '2' 
                    ELSE '1' 
                END AS UnitType,
                CASE 
                    WHEN ub.GlassThick2 > 0 THEN ub.Glass1 + ' / ' + ub.Spacer1 + ' / ' + ub.Glass2 + ' / ' +  ub.Spacer2 + ' / ' +  ub.Glass3 
                    WHEN ub.GlassThick1 > 0 THEN ub.Glass1 + ' / ' + ub.Spacer1 + ' / ' + ub.Glass2 
                    ELSE ub.Glass1
                END AS UnitConstruction,
                CASE WHEN ub.Tubes = 1 THEN 1 ELSE 0 END AS Tubes,
                CASE WHEN ub.ShapeNum > 0 THEN 1 ELSE 0 END AS Shapes,
                CASE WHEN ub.PreserveExt > 0 OR ub.PreserveInt > 0 THEN 1 ELSE 0 END AS Preserve,
                CASE WHEN ub.ScoreOpenings > 0 THEN 1 ELSE 0 END AS Scoring,
                CASE WHEN ub.MCode > 0 THEN 1 ELSE 0 END AS Bars,
                CASE WHEN ub.PickUnit > 0 THEN 1 ELSE 0 END AS Picks,
                CASE WHEN ub.Purchase0 = 1 OR ub.Purchase1 = 1 OR ub.Purchase2 = 1 THEN 1 ELSE 0 END as Purs
            FROM
                UnitBox ub WITH (NOLOCK)
                INNER JOIN IncomingOrders ic WITH (NOLOCK)
                ON  ic.WOrderNum = ub.OrderItem
                INNER JOIN #TempUnitList t
                ON  t.UnitId = ub.UnitId and
                    t.WorkOrderNum = ub.OrderItem";
            return cmd;
        }

        public void GetAttributesCg(GlassUnits units, SqlConnection cn, Plant plant)
        {
            try
            {
                var cmd = GetAttributesCg();
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var attributeRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        attributeRec.Add("CustName", CastDbNull.To<String>(reader["CustName"], String.Empty).Trim());
                        attributeRec.Add("Box", CastDbNull.To<int>(reader["Box"], 0));
                        attributeRec.Add("Slot", CastDbNull.To<int>(reader["Pos"], 0));
                        attributeRec.Add("RunSeq", CastDbNull.To<int>(reader["RunSeq"], 0));
                        attributeRec.Add("Construction", CastDbNull.To<String>(reader["Construction"], String.Empty).Trim());
                        attributeRec.Add("BarDescription", CastDbNull.To<String>(reader["BarDescription"], String.Empty).Trim());
                        var uc = CastDbNull.To<String>(reader["UnitConstruction"], String.Empty).Trim();
                        attributeRec.Add("UnitConstruction", System.Text.RegularExpressions.Regex.Replace(uc, @"\s{2,}", " "));
                        attributeRec.Add("BaseDim", CastDbNull.To<Double>(reader["DimBase"], 0));
                        attributeRec.Add("LeftDim", CastDbNull.To<Double>(reader["DimLeft"], 0));
                        attributeRec.Add("BaseDimChar", CastDbNull.To<String>(reader["DimBaseChar"], String.Empty).Trim());
                        attributeRec.Add("LeftDimChar", CastDbNull.To<String>(reader["DimLeftChar"], String.Empty).Trim());
                        attributeRec.Add("RightDimChar", CastDbNull.To<String>(reader["DimRightChar"], String.Empty).Trim());
                        attributeRec.Add("TopDimChar", CastDbNull.To<String>(reader["DimTopChar"], String.Empty).Trim());
                        attributeRec.Add("S1DimChar", CastDbNull.To<String>(reader["DimS1Char"], String.Empty).Trim());
                        attributeRec.Add("S2DimChar", CastDbNull.To<String>(reader["DimS2Char"], String.Empty).Trim());
                        attributeRec.Add("ShapeNum", CastDbNull.To<int>(reader["ShapeNum"], 0));
                        attributeRec.Add("UnitWeight", CastDbNull.To<decimal>(reader["UnitWeight"], 0));
                        attributeRec.Add("IGLineRun", CastDbNull.To<int>(reader["IGLineRun"], 0));
                        attributeRec.Add("Lite1Routing", CastDbNull.To<String>(reader["Lite1_Routing"], String.Empty).Trim());
                        attributeRec.Add("Lite2Routing", CastDbNull.To<String>(reader["Lite2_Routing"], String.Empty).Trim());
                        attributeRec.Add("Lite3Routing", CastDbNull.To<String>(reader["Lite3_Routing"], String.Empty).Trim());
                        attributeRec.Add("UnitType", CastDbNull.To<String>(reader["UnitType"], String.Empty).Trim());
                        attributeRec.Add("Tubes", CastDbNull.To<int>(reader["Tubes"], 0));
                        attributeRec.Add("Shapes", CastDbNull.To<int>(reader["Shapes"], 0));
                        attributeRec.Add("Preserve", CastDbNull.To<int>(reader["Preserve"], 0));
                        attributeRec.Add("Scoring", CastDbNull.To<int>(reader["Scoring"], 0));
                        attributeRec.Add("Bars", CastDbNull.To<int>(reader["Bars"], 0));
                        attributeRec.Add("Picks", CastDbNull.To<int>(reader["Picks"], 0));
                        attributeRec.Add("Purs", CastDbNull.To<int>(reader["Purs"], 0));
                        dict.Add(unitId, attributeRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        var attributeRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (attributeRec != null)
                        {
                            unit.CustomerNameUnitBox = (string) attributeRec["CustName"];
                            unit.BoxNum = (int) attributeRec["Box"];
                            unit.SlotNum = (int) attributeRec["Slot"];
                            unit.RunSequence = (int) attributeRec["RunSeq"];
                            //unit.Construction = (string) attributeRec["Construction"];
                            unit.Construction = (string) attributeRec["UnitConstruction"];
                            unit.BarDescription = (string) attributeRec["BarDescription"];
                            unit.BaseDim = (double) attributeRec["BaseDim"];
                            unit.LeftDim = (double) attributeRec["LeftDim"];
                            unit.ShapeNum = (int) attributeRec["ShapeNum"];
                            unit.GlassWeight = (decimal) attributeRec["UnitWeight"];
                            unit.Tubes = (int) attributeRec["Tubes"];
                            unit.Shapes = (int) attributeRec["Shapes"];
                            unit.Preserve = (int) attributeRec["Preserve"];
                            unit.Scoring = (int) attributeRec["Scoring"];
                            unit.Bars = (int) attributeRec["Bars"];
                            unit.Picks = (int) attributeRec["Picks"];
                            unit.Purchases = (int) attributeRec["Purs"];
                            unit.IGLineRun = (int) attributeRec["IGLineRun"];
                            unit.LastLine = attributeRec["IGLineRun"].ToString();
                            unit.Lite1Routing = (string) attributeRec["Lite1Routing"];
                            unit.Lite2Routing = (string) attributeRec["Lite2Routing"];
                            unit.Lite3Routing = (string) attributeRec["Lite3Routing"];
                            unit.UnitType = (string) attributeRec["UnitType"];
                            unit.BaseDimChar = (string) attributeRec["BaseDimChar"];
                            unit.LeftDimChar = (string) attributeRec["LeftDimChar"];

                            var sb = new StringBuilder();
                            if ((string) attributeRec["BaseDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["BaseDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["LeftDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["LeftDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["RightDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["RightDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["TopDimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["TopDimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["S1DimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["S1DimChar"]);
                                sb.Append(" X ");
                            }

                            if ((string) attributeRec["S2DimChar"] != "0")
                            {
                                sb.Append((string) attributeRec["S2DimChar"]);
                                sb.Append(" X ");
                            }

                            var dimMakeup = sb.ToString();
                            unit.DimMakeup = dimMakeup.Substring(0, dimMakeup.Length - 3);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
         * Purchased
         *******************************************************************************/
        public SqlCommand GetPurchasedLites0()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              UnitID ,
              ISNULL([1],'') AS Lite1 ,
              ISNULL([2],'') AS Lite2 ,
              ISNULL([3],'') AS Lite3
            FROM
              ( SELECT
                  ub.UnitID ,
                  wo.RelatedLiteNum ,
	              CASE WHEN SUM(pl.OpenQty) > 0 THEN pl.SupplierPlant + '*' ELSE pl.SupplierPlant END AS SupplierPlant
                FROM
                  UnitBox ub WITH ( NOLOCK )
                  INNER JOIN dbo.E1PurchasedLotWorkOrders wo WITH ( NOLOCK )
                  ON wo.WorkOrderNum = ub.OrderItem
                  INNER JOIN dbo.E1PurchasedLots pl WITH ( NOLOCK )
                  ON pl.LotOrderNum = wo.LotOrderNum AND
                     pl.LotOrderType = wo.LotOrderType AND
                     pl.LotOrderLineNum = wo.LotOrderLineNum
                  INNER JOIN #TempUnitList t
                    ON t.UnitId = ub.UnitID and
                        t.WorkOrderNum = ub.OrderItem
                GROUP BY
                  ub.UnitID ,
                  wo.RelatedLiteNum ,
                  pl.SupplierPlant
              ) AS STBL PIVOT
			              ( MAX(SupplierPlant) FOR RelatedLiteNum IN ( [1], [2], [3] ) ) AS PTBL";
            return cmd;
        }

        public SqlCommand GetPurchasedLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                 UnitID,
                 ISNULL([1], '') AS Lite1,
                 ISNULL([2], '') AS Lite2,
                 ISNULL([3], '') AS Lite3
            FROM (
                     SELECT
                         ub.UnitID,
                         ro.LiteNum AS RelatedLiteNum,
                         CASE
                             WHEN SUM(po.QuantityOrdered - po.QuantityReceived) > 0 THEN
                                  ro.RelatedSupplierPlant + '*'
                             ELSE ro.RelatedSupplierPlant
                         END AS SupplierPlant
                     FROM
                         UnitBox                                      ub WITH (NOLOCK)
                         INNER JOIN EBS.RelatedOrder ro WITH (NOLOCK)
                         ON  ro.WOrderNum = ub.OrderItem
                         INNER JOIN EBS.PurchaseOrder po WITH (NOLOCK)
			             ON  po.OrderNum = ro.RelatedOrderNum AND 
			                po.OrderType = ro.RelatedOrderType AND
			                po.OrderLineNum = ro.RelatedOrderLineNum
                      INNER JOIN #TempUnitList t
                        ON  t.UnitId = ub.UnitID and
                            t.WorkOrderNum = ub.OrderItem
                     GROUP BY
                         ub.UnitID,
                         ro.LiteNum,
                         ro.RelatedSupplierPlant
                 ) AS STBL
            PIVOT (
                      MAX(SupplierPlant)
                      FOR RelatedLiteNum IN ([1], [2], [3])
                  ) AS PTBL";
            return cmd;
        }

        public void GetPurchasedLites(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetPurchasedLites();
                //var cmd = GetPurchasedLites(units);
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var purLitesRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        purLitesRec.Add("Lite1", CastDbNull.To<String>(reader["Lite1"], String.Empty).Trim());
                        purLitesRec.Add("Lite2", CastDbNull.To<String>(reader["Lite2"], String.Empty).Trim());
                        purLitesRec.Add("Lite3", CastDbNull.To<String>(reader["Lite3"], String.Empty).Trim());
                        dict.Add(unitId, purLitesRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        unit.IsPurchasedLite = "N";
                        var purLitesRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (purLitesRec != null)
                        {
                            unit.IsPurchasedLite = "Y";
                            unit.Lite1Routing = unit.Lite1Routing + (string) purLitesRec["Lite1"];
                            unit.Lite2Routing = unit.Lite2Routing + (string) purLitesRec["Lite2"];
                            unit.Lite3Routing = unit.Lite3Routing + (string) purLitesRec["Lite3"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
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
         * Get Lock Load Timestamps
         *******************************************************************************/
        public iDB2Command GetLockLoad()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            *
            FROM 
	            (
	            SELECT 
		            DLUKID, 
		            DLZ58RID, 
		            DLZ58SN, 
		            DLZ58DST, 
		            DLPSN, 
		            DLUPMJ, 
		            DLTDAY,
		            DLMCU,
		            ROW_NUMBER() OVER (PARTITION BY DLLOTN, DLZ58SN, DLZ58DST ORDER BY DLUPMJ DESC, DLTDAY DESC) AS ROWNUM 
	            FROM 
		            F580569 
	            WHERE
		            DLLOTN = ? AND DLMCU = ? AND DLZ58SN = ? AND DLZ58DST IN ('4','5')
		            ) A
            WHERE 
	            A.ROWNUM = 1";
            cmd.Parameters.Add(new iDB2Parameter("LotNum", iDB2DbType.iDB2Graphic, 20));
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SerialNum", iDB2DbType.iDB2Graphic, 20));
            return cmd;
        }

        public void GetLockLoad(GlassUnits units, BusinessSystemSession session, Plant plant)
        {
            try
            {
                var cmd = (session.DbCommands.ContainsKey("OpenUnitsManager.GetLockLoad")) ? session.DbCommands["OpenUnitsManager.GetLockLoad"] : GetLockLoad();
                cmd.Connection = session.DbConnection;
                foreach (GlassUnit unit in units)
                {
                    if (unit.RackType != "EXCPT" && unit.Status != "1")
                    {
                        cmd.Parameters[0].Value = unit.WorkOrderNum.ToString();
                        cmd.Parameters[1].Value = plant.PlantNum.PadLeft(12);
                        cmd.Parameters[2].Value = unit.UnitId;
                        using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            while (reader.Read())
                            {
                                var status = CastDbNull.To<String>(reader["DLZ58DST"], String.Empty).Trim();
                                var statusDttm = Common.Utilities.Common.CastToDateTime(CastDbNull.To<decimal>(reader["DLUPMJ"], 0), CastDbNull.To<decimal>(reader["DLTDAY"], 0)).AddHours(plant.TimeOffset);
                                var loadNum = CastDbNull.To<Decimal>(reader["DLPSN"], 0);

                                if (status == "4")
                                {
                                    unit.RackCloseDate = statusDttm;
                                }
                                else
                                {
                                    if (status == "5" && loadNum == 0)
                                    {
                                        unit.RackLockDate = statusDttm;
                                    }
                                    else
                                    {
                                        if (status == "5" && loadNum > 0)
                                        {
                                            unit.RackLockLoadDate = statusDttm;
                                        }
                                    }
                                }
                            }

                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Missing Unit
         *******************************************************************************/
        public iDB2Command GetOriginalUKID()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        MIN(DLUKID) AS DLUKID
	        FROM 
		        F580569 
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

        public iDB2Command GetOriginalRack()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        DLZ58RID,
		        RHZ58HST,
                S1Z58SCN,
	            S1Z58SCD,
	            S1Z58SCTYP
	        FROM 
		        F580569 
		        INNER JOIN F580055
		        ON RHZ58RID = DLZ58RID
	            LEFT OUTER JOIN F5834A11 INNER JOIN F5834A01 ON S1MCU = S5MCU AND S1Z58SCN = S5Z58SCN AND S1Z58SCD = S5Z58SCD
	            ON S5DOCO = DLDOC1
            WHERE 
		        DLUKID = ?";
            cmd.Parameters.Add(new iDB2Parameter("UKID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["UKID"].Precision = 15;
            cmd.Parameters["UKID"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetOriginalSchedule()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            A.DLMCU,
	            A.DLLOTN,
	            A.DLZ58SN,
                A.DLZ58RID,
                S1Z58SCN,
	            S1Z58SCD,
	            S1Z58SCTYP
            FROM
	            F580569 A
	            INNER JOIN (
		            SELECT 
			            MIN(DLUKID) AS DLUKID
		            FROM 
			            F580569 
		            WHERE 
			            DLLOTN = ? AND 
			            DLZ58SN = ? and 
			            DLMCU = ?) B
	            ON B.DLUKID = A.DLUKID
	            INNER JOIN F580055 
	            ON RHZ58RID = A.DLZ58RID
	            LEFT OUTER JOIN F5834A11 INNER JOIN F5834A01 ON S1MCU = S5MCU AND S1Z58SCN = S5Z58SCN AND S1Z58SCD = S5Z58SCD
	            ON S5DOCO = A.DLDOC1";
            cmd.Parameters.Add(new iDB2Parameter("LotNum", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("SerialNum", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetOriginalSchedule2(GlassUnits units, BusinessSystemSession session, Plant plant)
        {
            try
            {
                var cmd1 = (session.DbCommands.ContainsKey("OpenUnitsManager.GetOriginalUKID")) ? session.DbCommands["OpenUnitsManager.GetOriginalUKID"] : GetOriginalUKID();
                var cmd2 = (session.DbCommands.ContainsKey("OpenUnitsManager.GetOriginalRack")) ? session.DbCommands["OpenUnitsManager.GetOriginalRack"] : GetOriginalRack();
                cmd1.Connection = session.DbConnection;
                cmd2.Connection = session.DbConnection;
                foreach (GlassUnit unit in units)
                {
                    cmd1.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                    cmd1.Parameters[1].Value = unit.UnitId;
                    cmd1.Parameters[2].Value = unit.WorkOrderNum.ToString();
                    //_logger.Info(Logging.DbCmdString(cmd));
                    using (IDataReader reader1 = cmd1.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        decimal ukid = 0;
                        while (reader1.Read())
                        {
                            ukid = CastDbNull.To<Decimal>(reader1["DLUKID"], 0);
                        }

                        reader1.Close();

                        cmd2.Parameters[0].Value = ukid;
                        using (IDataReader reader2 = cmd2.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            while (reader2.Read())
                            {
                                var rackId = CastDbNull.To<Decimal>(reader2["DLZ58RID"], 0);
                                var rackStatus = CastDbNull.To<String>(reader2["RHZ58HST"], String.Empty).Trim();
                                var scheduleNum = CastDbNull.To<Decimal>(reader2["S1Z58SCN"], 0);
                                var scheduleDate = Timestamp.CastToDateTime(CastDbNull.To<Decimal>(reader2["S1Z58SCD"], 0));
                                unit.ScheduledRackId = rackId;
                                unit.OriginalScheduleNum = scheduleNum;
                                unit.OriginalScheduleDate = scheduleDate;
                                if (unit.RackId != rackId && (rackStatus == "7" || rackStatus == "8"))
                                {
                                    unit.IsMissing = "Y";
                                }
                            }

                            reader2.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }


        public iDB2Command GetOriginalRackId()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            DLUKID, 
	            DLZ58RID, 
	            RHZ58HST,
	            S1Z58SCN,
	            S1Z58SCD
            FROM 
	            (
	            SELECT 
		            DLUKID, 
		            DLZ58RID, 
		            DLDOC1,
		            ROW_NUMBER() OVER (PARTITION BY DLLOTN, DLZ58SN, DLMCU ORDER BY DLUPMJ ASC, DLTDAY ASC) AS ROWNUM 
	            FROM 
		            F580569 
	            WHERE
		            DLLOTN = ? AND 
		            DLZ58SN = ? AND 
		            DLMCU = ? AND 
		            DLZ58DST = '1' AND 
		            DLIOPR = 'Insert'
		            ) A
	            INNER JOIN F580055
	            ON RHZ58RID = A.DLZ58RID
	            LEFT OUTER JOIN F5834A11 
		            INNER JOIN F5834A01 
		            ON	S1MCU = S5MCU AND 
			            S1Z58SCN = S5Z58SCN AND 
			            S1Z58SCD = S5Z58SCD
	            ON S5DOCO = A.DLDOC1
            WHERE 
	            A.ROWNUM = 1";
            cmd.Parameters.Add(new iDB2Parameter("LotNum", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("SerialNum", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetOriginalSchedule(GlassUnits units, BusinessSystemSession session, Plant plant)
        {
            try
            {
                var cmd = (session.DbCommands.ContainsKey("OpenUnitsManager.GetOriginalRackId")) ? session.DbCommands["OpenUnitsManager.GetOriginalRackId"] : GetOriginalRackId();
                cmd.Connection = session.DbConnection;
                foreach (GlassUnit unit in units)
                {
                    cmd.Parameters[0].Value = unit.WorkOrderNum.ToString();
                    cmd.Parameters[1].Value = unit.UnitId;
                    cmd.Parameters[2].Value = plant.PlantNum.PadLeft(12);
                    //_logger.Info(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            var rackId = CastDbNull.To<Decimal>(reader["DLZ58RID"], 0);
                            var rackStatus = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                            var scheduleNum = CastDbNull.To<Decimal>(reader["S1Z58SCN"], 0);
                            var scheduleDate = Timestamp.CastToDateTime(CastDbNull.To<Decimal>(reader["S1Z58SCD"], 0));
                            unit.ScheduledRackId = rackId;
                            unit.OriginalScheduleNum = scheduleNum;
                            unit.OriginalScheduleDate = scheduleDate;
                            if (unit.RackId != rackId && (rackStatus == "7" || rackStatus == "8"))
                            {
                                unit.IsMissing = "Y";
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
         * Sealer
         *******************************************************************************/
        public SqlCommand GetSealerStatus()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                A.WorkOrderNum,
                A.UnitID,
                COUNT(DISTINCT pl.record_id) AS Transactions
            FROM
                #TempUnitList A
                LEFT OUTER JOIN PlantReporting.rpt.ProdUnitSequence B WITH (NOLOCK)
                ON  B.WOrderNum = A.WorkOrderNum AND
                    B.UnitId = A.UnitID AND
                    B.EffectiveStartDate <= GETDATE() AND
                    B.EffectiveEndDate > GETDATE()
                LEFT OUTER JOIN AGDATA.dbo.AG_ProductionLogs pl WITH (NOLOCK)
                    INNER JOIN AGDATA.dbo.AG_LineInfo li WITH (NOLOCK)
                    ON  li.Machine_id = pl.Machine_id
                    INNER JOIN AGDATA.dbo.AG_MachineData md WITH (NOLOCK)
                    ON  md.UnitID = pl.UnitID
                ON  md.Prodline = A.WorkOrderNum AND
                    md.UnitID = A.UnitID AND
                    li.MachineName = 'Squid' AND
                    pl.ProdDate >= B.EffectiveStartDate AND
                    pl.ProdDate <= B.EffectiveEndDate
            GROUP BY
                A.WorkOrderNum,
                A.UnitID";
            return cmd;
        }

        public void GetSealerStatus(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetSealerStatus();
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var sealerRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        sealerRec.Add("Transactions", CastDbNull.To<int>(reader["Transactions"], 0));
                        _logger.Debug($"GetSealerStatus unitId={unitId} sealerRec={sealerRec}");
                        dict.Add(unitId, sealerRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        unit.SealerStatus = "N";
                        var sealerRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (sealerRec != null)
                        {
                            if ((int) sealerRec["Transactions"] > 0)
                            {
                                unit.SealerStatus = "Y";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        /*********************************************************************************
         * Last Machine/Station Transactions
         *******************************************************************************/
        public SqlCommand GetLastTransactions()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                t.WorkOrderNum,
	            t.UnitId,
                ps.Schedule,
                sq.SequenceNum,
                ps.OriginalSchedule,
                ag.MachineName,
	            ag.Line_id,
                ag.ProdDate AS MachineDate,
	            pd.ProductionLineCode,
	            pd.LineOperationCode,
	            pd.WrittenDate,
	            pd.Comment
            FROM
                #TempUnitList t
                INNER JOIN ShopFloor.LiteSource.ProdUnitStatus ps
                ON  ps.WorkOrder = t.WorkOrderNum AND
                    ps.UnitID = t.UnitId  AND
                    ps.IsCurrentStatus = 1
                INNER JOIN PlantReporting.rpt.ProdUnitSequence sq
                ON  sq.WOrderNum = ps.WorkOrder AND
                    sq.UnitId = ps.UnitID AND
                    sq.ScheduleNum = ps.Schedule
                LEFT OUTER JOIN
                (
                    SELECT
                        md.Prodline,
                        md.UnitID,
                        li.MachineName,
                        pl.ProdDate,
                        li.Line_id,
                        ROW_NUMBER() OVER (PARTITION BY
                                               md.Prodline,
                                               md.UnitID
                                           ORDER BY
                                               pl.record_id DESC
                                          ) AS RN
                    FROM
                        AGDATA.dbo.AG_ProductionLogs pl WITH (NOLOCK)
                        INNER JOIN AGDATA.dbo.AG_LineInfo li WITH (NOLOCK)
                        ON  li.Machine_id = pl.Machine_id
                        INNER JOIN AGDATA.dbo.AG_MachineData md WITH (NOLOCK)
                        ON  md.UnitID = pl.UnitID
                ) ag
                ON  ag.Prodline = ps.WorkOrder AND
                    ag.UnitID = ps.UnitID AND
                    ag.ProdDate >= sq.EffectiveStartDate AND
                    ag.ProdDate <= sq.EffectiveEndDate AND
                    ag.RN = 1
	            LEFT OUTER JOIN (
	                    SELECT
			            CASE WHEN ISNULL(pd.CustIntReject, '') <> '' THEN '[Int] ' + pd.CustIntReject + ' ' ELSE '' END +
			            CASE WHEN ISNULL(pd.UnRejInt, '') = 'U' AND ISNULL(pd.CustIntReject, '') <> '' THEN ' / UNREJ '  ELSE '' END +
			            CASE WHEN ISNULL(pd.CustExtReject, '') <> '' THEN '[Ext] ' + pd.CustExtReject + ' ' ELSE '' END +
			            CASE WHEN ISNULL(pd.UnRejExt, '') = 'U' AND ISNULL(pd.CustExtReject, '') <> '' THEN ' / UNREJ '  ELSE '' END +
			            CASE WHEN ISNULL(pd.ThirdLite, '') <> '' THEN '[Mid] ' + pd.ThirdLite + ' ' ELSE '' END +
			            CASE WHEN ISNULL(pd.UnRejThird, '') = 'U' AND ISNULL(pd.ThirdLite, '') <> '' THEN ' / UNREJ '  ELSE '' END +
			            CASE WHEN ISNULL(pd.CustSpacerReject, '') <> '' THEN '[Spc1] ' + pd.CustSpacerReject + ' ' ELSE '' END +
			            CASE WHEN ISNULL(pd.UnRejSpc, '') = 'U' AND ISNULL(pd.CustSpacerReject, '') <> '' THEN ' / UNREJ '  ELSE '' END +
			            CASE WHEN ISNULL(pd.CustSpacerReject2, '') <> '' THEN '[Spc2] ' + pd.CustSpacerReject2 + ' ' ELSE '' END +
			            CASE WHEN ISNULL(pd.UnRejSpc2, '') = 'U' AND ISNULL(pd.CustSpacerReject2, '') <> '' THEN ' / UNREJ '  ELSE '' END +
			            CASE WHEN ISNULL(pd.CustPackReject, '') <> '' THEN '[Pack] ' + pd.CustPackReject + ' ' ELSE '' END AS Comment,
                       pd.OrderNum,
                        pd.UnitId,
			            pd.WrittenDate,
			            pd.ProductionLineCode,
			            pd.LineOperationCode,
                        ROW_NUMBER() OVER (PARTITION BY
                                               pd.OrderNum,
                                               pd.UnitId
                                           ORDER BY
                                               pd.ProdId DESC
                                          ) AS RN
                    FROM
                        dbo.ProdData pd WITH (NOLOCK)) pd
			            ON pd.OrderNum = ps.WorkOrder AND
                        pd.UnitID = ps.UnitID AND
                        pd.RN = 1
            order by UnitId";
            return cmd;
        }

        public void GetLastTransactions(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetLastTransactions();
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var machineRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        machineRec.Add("LastMachineName", CastDbNull.To<string>(reader["MachineName"], String.Empty));
                        machineRec.Add("LastMachineLine", CastDbNull.To<string>(reader["Line_id"], String.Empty));
                        machineRec.Add("LastMachineDate", CastDbNull.To<DateTime>(reader["MachineDate"], DateTime.MinValue));
                        machineRec.Add("LastStationName", CastDbNull.To<string>(reader["LineOperationCode"], String.Empty));
                        machineRec.Add("LastStationLine", CastDbNull.To<string>(reader["ProductionLineCode"], String.Empty));
                        machineRec.Add("LastStationDate", CastDbNull.To<DateTime>(reader["WrittenDate"], DateTime.MinValue));
                        machineRec.Add("LastStationDesc", CastDbNull.To<string>(reader["Comment"], String.Empty));
                        _logger.Debug($"GetLastTransactions unitId={unitId}");
                        dict.Add(unitId, machineRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        unit.LastMachineName = "";
                        var machineRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (machineRec != null)
                        {
                            unit.LastMachineName = (String) machineRec["LastMachineName"];
                            unit.LastMachineLine = (String) machineRec["LastMachineLine"];
                            unit.LastMachineDate = (DateTime) machineRec["LastMachineDate"];
                            unit.LastStationName = (String) machineRec["LastStationName"];
                            unit.LastStationLine = (String) machineRec["LastStationLine"];
                            unit.LastStationDate = (DateTime) machineRec["LastStationDate"];
                            unit.LastStationDesc = (String) machineRec["LastStationDesc"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("ERROR: " + e.Message + " - " + e.StackTrace);
                throw new Exception(e.Message);
            }
        }


        /*********************************************************************************
         * Last Machine/Station Transactions
         *******************************************************************************/
        public SqlCommand GetLastMachineTransactions()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                 *
            FROM (
                     SELECT
                         t.WorkOrderNum,
                         t.UnitId,
                         ps.Schedule,
                         ps.OriginalSchedule,
                         li.MachineName,
                         li.Line_id,
                         pl.ProdDate         AS MachineDate,
                         ROW_NUMBER() OVER (PARTITION BY
                                                t.WorkOrderNum,
                                                t.UnitId
                                            ORDER BY
                                                pl.TransactionDate DESC
                                           ) AS RN
                     FROM
                         #TempUnitList                                  t
                         INNER JOIN ShopFloor.LiteSource.ProdUnitStatus ps WITH (NOLOCK)
                         ON  ps.WorkOrder = t.WorkOrderNum AND
                             ps.UnitID = t.UnitId AND
                             ps.IsCurrentStatus = 1
                         INNER JOIN AGDATA.dbo.AG_ProductionLogs         pl WITH (NOLOCK)
                                    INNER JOIN AGDATA.dbo.AG_LineInfo    li WITH (NOLOCK)
                                    ON  li.Machine_id = pl.Machine_id
                                    INNER JOIN AGDATA.dbo.AG_MachineData md WITH (NOLOCK)
                                    ON  md.UnitID = pl.UnitID
                         ON  pl.UnitID = t.UnitId AND
                             ABS(DATEDIFF(DAY, pl.TransactionDate, ps.CreatedDate)) < 30
                 ) A
            WHERE
                 A.RN = 1
            ORDER BY
                 UnitId";
            return cmd;
        }

        public void GetLastMachineTransactions(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetLastMachineTransactions();
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var machineRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        machineRec.Add("LastMachineName", CastDbNull.To<string>(reader["MachineName"], String.Empty));
                        machineRec.Add("LastMachineLine", CastDbNull.To<string>(reader["Line_id"], String.Empty));
                        machineRec.Add("LastMachineDate", CastDbNull.To<DateTime>(reader["MachineDate"], DateTime.MinValue));
                        _logger.Debug($"GetLastMachineTransactions unitId={unitId}");
                        dict.Add(unitId, machineRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        unit.LastMachineName = "";
                        var machineRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (machineRec != null)
                        {
                            unit.LastMachineName = (String)machineRec["LastMachineName"];
                            unit.LastMachineLine = (String)machineRec["LastMachineLine"];
                            unit.LastMachineDate = (DateTime)machineRec["LastMachineDate"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("ERROR: " + e.Message + " - " + e.StackTrace);
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
         * Last Machine/Station Transactions
         *******************************************************************************/
        public SqlCommand GetLastStationTransactions()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT * FROM (
            SELECT
                t.WorkOrderNum,
                t.UnitId,
                ps.Schedule,
                ps.OriginalSchedule,
                pd.ProductionLineCode,
                pd.LineOperationCode,
                pd.WrittenDate,
                CASE
                    WHEN ISNULL(pd.CustIntReject, '') <> '' THEN
                         '[Int] ' + pd.CustIntReject + ' '
                    ELSE ''
                END + CASE
                          WHEN ISNULL(pd.UnRejInt, '') = 'U' AND
                               ISNULL(pd.CustIntReject, '') <> '' THEN
                               ' / UNREJ '
                          ELSE ''
                      END + CASE
                                WHEN ISNULL(pd.CustExtReject, '') <> '' THEN
                                     '[Ext] ' + pd.CustExtReject + ' '
                                ELSE ''
                            END + CASE
                                      WHEN ISNULL(pd.UnRejExt, '') = 'U' AND
                                           ISNULL(pd.CustExtReject, '') <> '' THEN
                                           ' / UNREJ '
                                      ELSE ''
                                  END + CASE
                                            WHEN ISNULL(pd.ThirdLite, '') <> '' THEN
                                                 '[Mid] ' + pd.ThirdLite + ' '
                                            ELSE ''
                                        END + CASE
                                                  WHEN ISNULL(pd.UnRejThird, '') = 'U' AND
                                                       ISNULL(pd.ThirdLite, '') <> '' THEN
                                                       ' / UNREJ '
                                                  ELSE ''
                                              END + CASE
                                                        WHEN ISNULL(pd.CustSpacerReject, '') <> '' THEN
                                                             '[Spc1] ' + pd.CustSpacerReject + ' '
                                                        ELSE ''
                                                    END + CASE
                                                              WHEN ISNULL(pd.UnRejSpc, '') = 'U' AND
                                                                   ISNULL(pd.CustSpacerReject, '') <> '' THEN
                                                                   ' / UNREJ '
                                                              ELSE ''
                                                          END + CASE
                                                                    WHEN ISNULL(pd.CustSpacerReject2, '') <> '' THEN
                                                                         '[Spc2] ' + pd.CustSpacerReject2 + ' '
                                                                    ELSE ''
                                                                END + CASE
                                                                          WHEN ISNULL(pd.UnRejSpc2, '') = 'U' AND
                                                                               ISNULL(pd.CustSpacerReject2, '') <> '' THEN
                                                                               ' / UNREJ '
                                                                          ELSE ''
                                                                      END + CASE
                                                                                WHEN ISNULL(pd.CustPackReject, '') <> '' THEN
                                                                                     '[Pack] ' + pd.CustPackReject + ' '
                                                                                ELSE ''
                                                                            END AS Comment,
                ROW_NUMBER() OVER (PARTITION BY
                                       pd.OrderNum,
                                       pd.UnitID
                                   ORDER BY
                                       pd.ProdID DESC
                                  )                                             AS RN
            FROM
                #TempUnitList                                  t
                INNER JOIN ShopFloor.LiteSource.ProdUnitStatus ps WITH (NOLOCK)
                ON  ps.WorkOrder = t.WorkOrderNum AND
                    ps.UnitID = t.UnitId AND
                    ps.IsCurrentStatus = 1
                LEFT OUTER JOIN dbo.ProdData                   pd WITH (NOLOCK)
                ON  pd.OrderNum = ps.WorkOrder AND
                    pd.UnitID = ps.UnitID) A 
            WHERE
                A.RN = 1
            ORDER BY
                A.UnitID";
            return cmd;
        }

        public void GetLastStationTransactions(GlassUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetLastStationTransactions();
                cmd.Connection = cn;
                cmd.CommandTimeout = 0;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var stationRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        stationRec.Add("LastStationName", CastDbNull.To<string>(reader["LineOperationCode"], String.Empty));
                        stationRec.Add("LastStationLine", CastDbNull.To<string>(reader["ProductionLineCode"], String.Empty));
                        stationRec.Add("LastStationDate", CastDbNull.To<DateTime>(reader["WrittenDate"], DateTime.MinValue));
                        stationRec.Add("LastStationDesc", CastDbNull.To<string>(reader["Comment"], String.Empty));
                        _logger.Debug($"GetLastStationTransactions unitId={unitId}");
                        dict.Add(unitId, stationRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        unit.LastStationName = "";
                        var stationRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (stationRec != null)
                        {
                            unit.LastStationName = (String)stationRec["LastStationName"];
                            unit.LastStationLine = (String)stationRec["LastStationLine"];
                            unit.LastStationDate = (DateTime)stationRec["LastStationDate"];
                            unit.LastStationDesc = (String)stationRec["LastStationDesc"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("ERROR: " + e.Message + " - " + e.StackTrace);
                throw new Exception(e.Message);
            }
        }

    }
}



