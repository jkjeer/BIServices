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
    public class OpenUnitManagerLG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;

        public OpenUnitManagerLG(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("OpenUnitManagerLG.GetScheduledRack", GetScheduledRack());
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
                    RDUPMJ,
                    RDTDAY,
                    RDZ58DST,
                    RDPQOH/1000 AS RDPQOH,
                    RDITM,
                    RDZ58SL,
                    RDZ58PS,
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
                    ATZ5899011,
                    ATZ5899012,
                    SDURCD,
                    AB1.ABALPH AS ABALPH1,
                    AB2.ABALPH AS ABALPH2,
                    CASE WHEN IMSRP4 = 'L' THEN '1' ELSE IMSRP4 END AS IMSRP4,
                    CMCFGID,
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
            List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0)
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
                        unit.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<Decimal>(reader["RHZ58BOX"], 0);
                        unit.SlotNum = CastDbNull.To<Decimal>(reader["RDZ58SL"], 0);
                        if (unit.SlotNum == 0)
                        {
                            unit.SlotNum = CastDbNull.To<Decimal>(reader["RDZ58PS"], 0);
                        }

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

                        unit.CustomerNum = CastDbNull.To<Decimal>(reader["WAAN8"], 0);
                        unit.CustomerName = CastDbNull.To<String>(reader["ABALPH1"], String.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        unit.CustomerShipName = CastDbNull.To<String>(reader["ABALPH2"], String.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<String>(reader["ATZ58CP2"], String.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<String>(reader["ATZ58CP3"], String.Empty).Trim();
                        unit.CustomerPart3 = CastDbNull.To<String>(reader["ATZ58CP4"], String.Empty).Trim();
                        unit.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                        unit.CustomerSeqNum = CastDbNull.To<String>(reader["ATZ58CSQNO"], String.Empty).Trim();
                        var arr = unit.CustomerSeqNum.Split('/');
                        if (arr.Length > 0)
                        {
                            unit.CustomerSeqPart1 = arr[0];
                        }

                        unit.GenericSeqNum = CastDbNull.To<String>(reader["ATZ58GSQNO"], String.Empty).Trim();
                        unit.PickNotes = CastDbNull.To<String>(reader["ATZ5899011"], String.Empty).Trim();
                        unit.PackNotes = CastDbNull.To<String>(reader["ATZ5899012"], String.Empty).Trim();

                        // Customer PO Parsing
                        unit.CustomerPO = CastDbNull.To<String>(reader["SDVR01"], String.Empty).Trim();
                        var tokens = unit.CustomerPO.Split('-');
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

                    _logger.Info("Shop Floor Get Last Production Start");
                    GetSchedules(units, cnShopFloor);

                    if (scheduleNumList.Count() > 0)
                    {
                        if (scheduleNumList[0] < 2000)
                        {
                            var x = units.RemoveAll(a => (!scheduleNumList.Contains(a.RemakeScheduleNum)));
                        }
                        else
                        {
                            var x = units.RemoveAll(a => (!scheduleNumList.Contains(a.ScheduleNum)));
                        }

                    }

                    GetAttributes(units, cnShopFloor);
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
         * Shop Floor Temp Table
         *******************************************************************************/
        public SqlCommand CreateTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"CREATE TABLE #TempUnitList (UnitId int, WorkOrderNum int)";
            return cmd;
        }

        public SqlCommand InsertTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"INSERT #TempUnitList (UnitId, WorkOrderNum) VALUES(@UnitId, @WorkOrderNum)";
            cmd.Parameters.Add(new SqlParameter("@UnitId", SqlDbType.Int));
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
                dt.Columns.Add("WorkOrderNum", typeof(int));
                foreach (GlassUnit unit in units)
                {
                    DataRow row = dt.NewRow();
                    row["UnitId"] = unit.UnitId;
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
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
         * Last Line
         *******************************************************************************/
        public SqlCommand GetSchedules()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
               pu.WorkOrderNumber,
               pu.SerialNumber,
               pu.OriginalScheduleNum,
               pu.OriginalScheduleDate,
               pu.CurrentScheduleNum,
               pu.CurrentScheduleDate,
               pu.Schedules
            FROM
	            EBS.vw_ProdUnit pu
                INNER JOIN #TempUnitList t
                ON  pu.WorkOrderNumber = t.WorkOrderNum AND
                    pu.Serialnumber = t.UnitId";
            return cmd;
        }

        public void GetSchedules(GlassUnits units, SqlConnection cn)
        {
            try
            {
                if (units.Count > 0)
                {
                    var cmd = GetSchedules();
                    cmd.Connection = cn;
                    cmd.CommandTimeout = 300;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        var dict = new Dictionary<Int64, Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            var unitId = CastDbNull.To<Int64>(reader["SerialNumber"], 0);
                            var scheduleRec = new Dictionary<string, object>();
                            scheduleRec.Add("OriginalScheduleNum", CastDbNull.To<decimal>(reader["OriginalScheduleNum"], 0));
                            scheduleRec.Add("OriginalScheduleDate", CastDbNull.To<DateTime>(reader["OriginalScheduleDate"], new DateTime(1900, 1, 1)));
                            scheduleRec.Add("CurrentScheduleNum", CastDbNull.To<decimal>(reader["CurrentScheduleNum"], 0));
                            scheduleRec.Add("CurrentScheduleDate", CastDbNull.To<DateTime>(reader["CurrentScheduleDate"], new DateTime(1900, 1, 1)));
                            scheduleRec.Add("Schedules", CastDbNull.To<int>(reader["Schedules"], 0));
                            dict.Add(unitId, scheduleRec);
                        }

                        reader.Close();

                        foreach (GlassUnit unit in units)
                        {
                            var scheduleRec = CollectionUtils.GetValueOrDefault(dict, (int) unit.UnitId);
                            if (scheduleRec != null)
                            {
                                unit.OriginalScheduleNum = (decimal) scheduleRec["OriginalScheduleNum"];
                                unit.OriginalScheduleDate = (DateTime) scheduleRec["OriginalScheduleDate"];
                                if ((decimal) scheduleRec["OriginalScheduleNum"] != (decimal) scheduleRec["CurrentScheduleNum"])
                                {
                                    unit.RemakeScheduleNum = (decimal) scheduleRec["CurrentScheduleNum"];
                                    unit.RemakeInitiateDate = (DateTime) scheduleRec["CurrentScheduleDate"];
                                }

                                unit.Passes = (int) scheduleRec["Schedules"];
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
         * Attributes 
         *******************************************************************************/
        public SqlCommand GetAttributes()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
                t.UnitId,
                ord.WorkOrderNum,
                ord.Dimension,
                ord.ShapeNum,
                CASE 
                    WHEN ord.UnitTypeCode = 'MONO LAMI' THEN 'LAMI'
                    WHEN ord.UnitTypeCode = 'MONO GLASS' THEN 'GLASS'
                    ELSE ''
                END AS UnitTypeCode,
                ord.ProductCode
            FROM
                ShopFloor.EBS.ScheduledOrders ord WITH (NOLOCK)
                INNER JOIN #TempUnitList t
                ON  t.WorkOrderNum = ord.WorkOrderNum";
            return cmd;
        }

        public void GetAttributes(GlassUnits units, SqlConnection cn)
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
                        attributeRec.Add("Dimension", CastDbNull.To<String>(reader["Dimension"], String.Empty).Trim());
                        attributeRec.Add("UnitTypeCode", CastDbNull.To<String>(reader["UnitTypeCode"], String.Empty).Trim());
                        attributeRec.Add("ProductCode", CastDbNull.To<String>(reader["ProductCode"], String.Empty).Trim());
                        attributeRec.Add("ShapeNum", CastDbNull.To<int>(reader["ShapeNum"], 0));
                        dict.Add(unitId, attributeRec);
                    }

                    reader.Close();
                    foreach (GlassUnit unit in units)
                    {
                        var attributeRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                        if (attributeRec != null)
                        {
                            unit.DimMakeup = (string) attributeRec["Dimension"];
                            unit.ShapeNum = (int) attributeRec["ShapeNum"];
                            unit.UnitType = (string) attributeRec["UnitTypeCode"];
                            unit.Construction = (string) attributeRec["ProductCode"];
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
         * Missing Unit
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
                var cmd = session.DbCommands["OpenUnitManagerLG.GetScheduledRack"];
                cmd.Connection = session.DbConnection;
                foreach (GlassUnit unit in units)
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
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}



