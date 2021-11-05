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

namespace BIServices.QuerySvc
{
    public class OpenUnitManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OpenUnitManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("OpenUnitsManager.GetLockLoad", GetLockLoad());
            hash.Add("OpenUnitsManager.GetOriginalSchedule", GetOriginalSchedule());
            hash.Add("OpenUnitsManager.GetOriginalUKID", GetOriginalUKID());
            hash.Add("OpenUnitsManager.GetOriginalRack", GetOriginalRack());
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
	        SELECT 
                RDMCU,
                RDZ58RID, 
                RDZ58SN,
                RHZ58RT,
                RHZ58BOX,
                RHZ58HST,
                RDUPMJ,
                RDTDAY,
                RDZ58DST,
                RDPQOH/1000 AS RDPQOH,
                S1Z58SCN ,
                S1Z58SCTYP,
                S5DRQJ,
                SDDRQJ,
                S5SHAN,
                S5Z58PT,
                WADOCO,
                WASRST,
                WAAN8,
                SDDOCO,
                SDDCTO,
                SDLNID/1000 AS SDLNID,
                SDAN8,
                SDSHAN,
                SDTRDJ,
                SDVR01,
                ATZ58CP2,
                ATZ58CP3,
                SDURCD,
                AB1.ABALPH AS ABALPH1,
                AB2.ABALPH AS ABALPH2,
                CASE WHEN IMSRP4 = 'L' THEN '1' ELSE IMSRP4 END AS IMSRP4
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
	        WHERE 
                {0}
                RHZ58HST IN ('0','1','2','3','4','5') AND
                WASRST NOT IN ('98','99')", filterString.ToString());
            return cmd;
        }

        public OpenUnits GetOpenUnits(BusinessSystemSession session, SqlConnection cnShopFloorRpt, SqlConnection cnShopFloor, SqlConnection cnDw, PlantAttribute plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, List<string> rackStatusList = null, List<string> unitTypeList = null,
            List<string> optionsList = null, decimal customerSoldNum = 0, decimal customerShipNum = 0)
        {
            var units = new OpenUnits();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant
                filters.Add("RHMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("PlantNum1", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.NumericCode.PadLeft(12);

                // Max Requested Date
                if (String.IsNullOrEmpty(maxDateCode) && startDate == null)
                {
                    maxDateCode = "TDAY";
                }

                if (!String.IsNullOrEmpty(maxDateCode))
                {
                    var maxDate = new DateTime(1900,1,1);
                    maxDateCode = maxDateCode.Replace(",", "").Replace("'", "");
                    try
                    {
                        maxDate = DateTime.Parse(maxDateCode);
                    }
                    catch (FormatException)
                    {
                        maxDate = GetRequestedDate(maxDateCode, cnDw);
                    }
                    if (maxDateCode == "NBDAY")
                    {
                        var plant1 = PlantManager.GetPlant(plant.NumericCode);
                        maxDate = CalendarManager.GetBusinessDate(session, plant1, "", "", DateTime.Now.Date, 1);
                    }
                    filters.Add("SDDRQJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("RequestedDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(maxDate);
                }

                // Start End Date
                if (startDate != null && String.IsNullOrEmpty(maxDateCode))
                {
                    filters.Add("SDDRQJ >= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("StartDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)startDate);

                    filters.Add("SDDRQJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("EndDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)endDate);
                }

                // Customer Ship Num
                if (customerShipNum > 0 || custNum > 0)
                {
                    filters.Add("SDSHAN = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("customerShipNum", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 8;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = (customerShipNum > 0) ? customerShipNum : custNum;
                }

                // Customer Sold Num
                if (customerSoldNum > 0)
                {
                    filters.Add("SDAN8 = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("customerSoldNum", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 8;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = customerSoldNum;
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

                // Rack Status
                if (rackStatusList.Count() > 0)
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

                // Rejects Not Completed
                if (optionsList.Contains("RNC"))
                {
                    filters.Clear();
                    parameters.Clear();

                    filters.Add("RHMCU = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("PlantNum1", iDB2DbType.iDB2Graphic, 12));
                    parameters.Last().Value = plant.NumericCode.PadLeft(12);

                    filters.Add("SDDRQJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("RequestedDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = 125000;

                    filters.Add("RDZ58DST = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("Status", iDB2DbType.iDB2Graphic, 10));
                    parameters.Last().Value = "1";

                }

                cmd = GetOpenUnits(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }                 
                cmd.Connection = session.DbConnection;
                _logger.Debug(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var unit = new OpenUnit();
                        unit.Plant = CastDbNull.To<String>(reader["RDMCU"], String.Empty).Trim();
                        unit.UnitIdChar = CastDbNull.To<String>(reader["RDZ58SN"], String.Empty).Trim();
                        unit.UnitId = Convert.ToDecimal(unit.UnitIdChar);
                        unit.RackId = CastDbNull.To<Decimal>(reader["RDZ58RID"], 0);
                        unit.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
                        unit.BoxNum = CastDbNull.To<Decimal>(reader["RHZ58BOX"], 0);
                        unit.Status = CastDbNull.To<String>(reader["RDZ58DST"], String.Empty).Trim();
                        unit.ScheduleNum = CastDbNull.To<Decimal>(reader["S1Z58SCN"], 0);
                        unit.OrderNum = CastDbNull.To<Decimal>(reader["SDDOCO"], 0);
                        unit.OrderType = CastDbNull.To<String>(reader["SDDCTO"], String.Empty).Trim();
                        unit.OrderLineNum = Math.Round(CastDbNull.To<Decimal>(reader["SDLNID"], 0), 3);
                        unit.WorkOrderNum = CastDbNull.To<Decimal>(reader["WADOCO"], 0);
                        unit.WorkOrderStatus = CastDbNull.To<String>(reader["WASRST"], String.Empty).Trim();
                        unit.ScheduleType = CastDbNull.To<String>(reader["S1Z58SCTYP"], String.Empty).Trim();
                        unit.ProductType = CastDbNull.To<String>(reader["S5Z58PT"], String.Empty).Trim();
                        var orderDateId = CastDbNull.To<Decimal>(reader["SDTRDJ"], 0);
                        unit.OrderDate = Common.Utilities.Common.CastToDateTime(orderDateId, 0);
                        var dateId = CastDbNull.To<Decimal>(reader["RDUPMJ"], 0);
                        var timeId = CastDbNull.To<Decimal>(reader["RDTDAY"], 0);
                        unit.RackItemDate = Common.Utilities.Common.CastToDateTime(dateId, timeId).AddHours(plant.TimeOffset);
                        var dateId1 = CastDbNull.To<Decimal>(reader["SDDRQJ"], 0);
                        unit.RequestedDate = Common.Utilities.Common.CastToDateTime(dateId1, 0);
                        unit.CustomerNum = CastDbNull.To<Decimal>(reader["WAAN8"], 0);
                        unit.CustomerName = CastDbNull.To<String>(reader["ABALPH1"], String.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        unit.CustomerShipName = CastDbNull.To<String>(reader["ABALPH2"], String.Empty).Trim();
                        unit.CustomerPO = CastDbNull.To<String>(reader["SDVR01"], String.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<String>(reader["ATZ58CP2"], String.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<String>(reader["ATZ58CP3"], String.Empty).Trim();
                        unit.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                        unit.IGLineScheduled = "";
                        unit.OnHand = Convert.ToInt16(CastDbNull.To<Decimal>(reader["RDPQOH"], 0));
                        unit.Units = 1;
                        units.Add(unit);
                    }

                    reader.Close();
                }

                if (units.Count > 0)
                {
                    _logger.Debug("Shop Floor Temp Table Start");
                    InsertTempTable(units, cnShopFloor);
                    _logger.Debug("Shop Floor Get Remakes Start");
                    GetRemake(units, cnShopFloor);
                    _logger.Debug("Shop Floor Get Attributes Start");
                    GetAttributes(units, cnShopFloor);
                    _logger.Debug("Shop Floor Get Last Production Start");
                    GetLastProduction(units, cnShopFloor);
                    string[] igPlants = { "02", "03", "06", "14", "16", "17", "19", "22", "25", "28" };
                    if (igPlants.Contains(units[0].Plant))
                    {
                        _logger.Debug("Shop Floor Get Remake Passes Start");
                        GetRemakePasses(units, cnShopFloor);
                        _logger.Debug("Shop Floor Get Purchased Lites Start");
                        GetPurchasedLites(units, cnShopFloor);
                        _logger.Debug("E1 Get Scheduled Rack Start");
                        GetOriginalSchedule(units, session, plant);
                    }

                    if (optionsList.Contains("MU"))
                    {
                        _logger.Debug("Local IsMissing Elimination");
                        var x = units.RemoveAll(a => a.IsMissing == false);
                    }
                    if (optionsList.Contains("XR"))
                    {
                        var x = units.RemoveAll(a => a.RemakeScheduleNum > 0 || a.RackType == "EXCPT");
                    }
                    if (optionsList.Contains("MP"))
                    {
                        var x = units.RemoveAll(a => a.Passes < 2);
                    }
                    if (optionsList.Contains("RNC"))
                    {
                        var x = units.RemoveAll(a => a.Passes < 1);
                    }
                    if (optionsList.Contains("XHVMTO"))
                    {
                        var x = units.RemoveAll(a => a.ScheduleType == "HV MTO");
                    }
                    if (igLinesList.Any())
                    {
                        var x = units.RemoveAll(a => (!igLinesList.Contains(a.IGLineRun)));
                    }
                    else
                    {
                        _logger.Debug("E1 Get Rack Lock Load Start");
                        GetLockLoad(units, session, plant);
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            _logger.Debug("Return Units");
            return units;
        }

        /*********************************************************************************
         * Shop Floor Temp Table
         *******************************************************************************/
        public SqlCommand CreateTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"CREATE TABLE #TempUnitList (UnitId int, UnitIdChar varchar(20))";
            return cmd;
        }

        public SqlCommand InsertTempTable()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"INSERT #TempUnitList (UnitId, UnitIdChar) VALUES(@UnitId, @UnitIdChar)";
            cmd.Parameters.Add(new SqlParameter("@UnitId", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@UnitIdChar", SqlDbType.VarChar, 20));
            return cmd;
        }

        public void InsertTempTable(OpenUnits units, SqlConnection cn)
        {
            try
            {
                var cmd1 = CreateTempTable();
                cmd1.Connection = cn;
                int rows = cmd1.ExecuteNonQuery();

                DataTable dt = new DataTable("Units");
                dt.Columns.Add("UnitId", typeof (int));
                dt.Columns.Add("UnitIdChar", typeof (string));
                foreach (OpenUnit unit in units)
                {
                    DataRow row = dt.NewRow();
                    row["UnitId"] = unit.UnitId;
                    row["UnitIdChar"] = unit.UnitIdChar;
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
              ProductionLineCode ,
              LineID,
              WrittenDate
            FROM
              ( SELECT
                  ub.UnitId ,
                  PD.ProductionLineCode ,
                  PD.LineID,
                  PD.WrittenDate ,
                  ROW_NUMBER() OVER ( PARTITION BY ub.UnitId ORDER BY PD.WrittenDate DESC ) AS ROW_NUM
                FROM
                  UnitBox ub WITH (NOLOCK)
                  INNER JOIN #TempUnitList t
                    ON t.UnitId = ub.UnitId
                  INNER JOIN ProdData PD WITH ( NOLOCK )
                  ON TRY_CAST(PD.UnitID AS INT) = ub.UnitID AND
                     PD.OrderNum = ub.OrderItem    
              ) A
            WHERE
              A.ROW_NUM = 1";
            return cmd;
        }

        public SqlCommand GetLastProduction2(OpenUnits units)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              UnitId ,
              ProductionLineCode ,
              LineID,
              WrittenDate
            FROM
              ( SELECT
                  ub.UnitId ,
                  PD.ProductionLineCode ,
                  PD.LineID,
                  PD.WrittenDate ,
                  ROW_NUMBER() OVER ( PARTITION BY ub.UnitId ORDER BY PD.WrittenDate DESC ) AS ROW_NUM
                FROM
                  UnitBox ub WITH (NOLOCK)
                  INNER JOIN ProdData PD WITH ( NOLOCK )
                  ON TRY_CAST(PD.UnitID AS INT) = ub.UnitID AND
                     PD.OrderNum = ub.OrderItem
                WHERE
                  ub.UnitId IN ({0})
              ) A
            WHERE
              A.ROW_NUM = 1", String.Join(",", units.Select(s => s.UnitIdChar)));
            return cmd;
        }

        public void GetLastProduction(OpenUnits units, SqlConnection cn)
        {
            try
            {
                if (units.Count > 0)
                {
                    //var cmd = GetLastProduction(units);
                    var cmd = GetLastProduction();
                    cmd.Connection = cn;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        var dict = new Dictionary<decimal, Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                            var lastProdRec = new Dictionary<string, object>();
                            lastProdRec.Add("lastLine", CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim());
                            lastProdRec.Add("LineID", CastDbNull.To<String>(reader["LineID"], String.Empty).Trim());
                            lastProdRec.Add("lastLineDate", CastDbNull.To<DateTime>(reader["WrittenDate"], new DateTime(1900, 1, 1)));
                            dict.Add(unitId, lastProdRec);
                        }

                        reader.Close();

                        foreach (OpenUnit unit in units)
                        {
                            var lastProdRec = CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
                            if (lastProdRec != null)
                            {
                                unit.LastLine = (string) lastProdRec["LineID"];
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

        //public void GetLastProduction2(OpenUnits units, SqlConnection cn)
        //{
        //    try
        //    {
        //        var cmd = GetLastProduction(units);
        //        cmd.Connection = cn;
        //        using( IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
        //        var dict = new Dictionary<decimal, ExpandoObject>();
        //        while (reader.Read())
        //        {
        //            var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
        //            dynamic e = new ExpandoObject();
        //            e.lastLine = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
        //            e.lastLineDate = CastDbNull.To<DateTime>(reader["WrittenDate"], new DateTime(1900, 1, 1));
        //            dict.Add(unitId, e);
        //        }
        //        reader.Close();
        //        foreach (OpenUnit unit in units)
        //        {
        //            var lastProdRec = (IDictionary<string, object>)CollectionUtils.GetValueOrDefault(dict, unit.UnitId);
        //            if (lastProdRec != null)
        //            {
        //                unit.LastLine = (string) lastProdRec["lastLine"];
        //                unit.LastLineDate = (DateTime) lastProdRec["lastLineDate"];
        //                unit.Passes = 1;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new FaultException(e.Message);
        //    }
        //}

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
              TRY_CAST(r.UnitId AS INT) AS UnitId ,
              ISNULL(TRY_CAST(r.RemakeSchedule AS DECIMAL(5,0)), 0) AS RemakeSchedule,
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
              on t.UnitIdChar = r.UnitId";
            return cmd;
        }

        public SqlCommand GetRemake2(OpenUnits units)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              RemakesId,
              TRY_CAST(UnitId AS INT) AS UnitId ,
              ISNULL(TRY_CAST(RemakeSchedule AS DECIMAL(5,0)), 0) AS RemakeSchedule,
              WrittenDate,
              InitiateDate + CAST(InitiateTime AS DATETIME) AS InitiateDateTime,
              CASE 
	            WHEN ISNULL(UnitReason, '') != '' THEN UnitReason
	            WHEN ISNULL(InteriorReason, '') != '' THEN InteriorReason
	            WHEN ISNULL(ExteriorReason, '') != '' THEN ExteriorReason
	            WHEN ISNULL(ThirdliteReason, '') != '' THEN ThirdliteReason
	            WHEN ISNULL(SpacerReason, '') != '' THEN SpacerReason
	            WHEN ISNULL(Spacer2Reason, '') != '' THEN Spacer2Reason
              END AS Reason,
              Comments
            FROM
              Remakes WITH (NOLOCK)
            WHERE
              UnitId IN ({0})", String.Join(",", units.Select(s => "'" + s.UnitIdChar + "'")));
            return cmd;
        }

        public void GetRemake(OpenUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetRemake();
                //var cmd = GetRemake(units);
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var remakeRec = new Dictionary<string, object>();
                        var unitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitId"], 0));
                        remakeRec.Add("RemakesId", CastDbNull.To<int>(reader["RemakesId"], 0));
                        remakeRec.Add("ScheduleNum", CastDbNull.To<Decimal>(reader["RemakeSchedule"], 0));
                        remakeRec.Add("InitiateDate", CastDbNull.To<DateTime>(reader["InitiateDateTime"], new DateTime(1900, 1, 1)));
                        remakeRec.Add("Reason", CastDbNull.To<String>(reader["Reason"], String.Empty).Trim());
                        remakeRec.Add("Comments", CastDbNull.To<String>(reader["Comments"], String.Empty).Trim());
                        if (!dict.ContainsKey(unitId))
                        {
                            dict.Add(unitId, remakeRec);
                        }
                    }

                    reader.Close();
                    foreach (OpenUnit unit in units)
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
                ON t.UnitIdChar = pd.UnitId
            GROUP BY
              pd.UnitID";
            return cmd;
        }

        public SqlCommand GetRemakePasses2(OpenUnits units)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              TRY_CAST(pd.UnitId AS INT) AS UnitId ,
              COUNT(DISTINCT pd.CurrentSchedule) AS Passes
            FROM
                ProdData pd WITH (NOLOCK)
                INNER JOIN IncomingOrders ord WITH (NOLOCK)
                ON ord.WOrderNum = pd.OrderNum
            WHERE
              pd.UnitId IN ({0})
            GROUP BY
              pd.UnitID", String.Join(",", units.Select(s => "'" + s.UnitIdChar + "'")));
            return cmd;
        }

        public SqlCommand GetRemakePasses1(OpenUnits units)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              TRY_CAST(rmk.UnitId AS INT) AS UnitId ,
              COUNT(DISTINCT rmk.RemakeSchedule) as Passes
            FROM
                Remakes_Audit rmk WITH (NOLOCK)
                INNER JOIN IncomingOrders ord WITH (NOLOCK)
                ON ord.WOrderNum = rmk.OrderNum
            WHERE
              rmk.UnitId IN ({0})
            GROUP BY
              rmk.UnitID", String.Join(",", units.Select(s => "'" + s.UnitIdChar + "'")));
            return cmd;
        }

        public void GetRemakePasses(OpenUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetRemakePasses();
                //var cmd = GetRemakePasses(units);
                cmd.Connection = cn;
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

                    foreach (OpenUnit unit in units)
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
         * Attributes
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
                ub.DimBase,
                ub.DimLeft,
                ub.ShapeNum,
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
                ON  t.UnitId = ub.UnitId";
            return cmd;
        }

        public SqlCommand GetAttributes2(OpenUnits units)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT DISTINCT
                ub.UnitId ,
                ub.CustName,
                ub.Box,
                ub.Pos,
                ub.RunSeq,
                ub.Construction,
                ub.DimBase,
                ub.DimLeft,
                ub.ShapeNum,
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
            WHERE
                ub.UnitId IN ({0})", String.Join(",", units.Select(s => s.UnitIdChar)));
            return cmd;
        }

        public void GetAttributes(OpenUnits units, SqlConnection cn)
        {
            try
            {
                //var cmd = GetAttributes(units);
                var cmd = GetAttributes();
                cmd.Connection = cn;
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
                    foreach (OpenUnit unit in units)
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
                            unit.BaseDim = (double) attributeRec["BaseDim"];
                            unit.LeftDim = (double) attributeRec["LeftDim"];
                            unit.ShapeNum = (int) attributeRec["ShapeNum"];
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
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Purchased
         *******************************************************************************/
        public SqlCommand GetPurchasedLites()
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
                    ON t.UnitId = ub.UnitID
                GROUP BY
                  ub.UnitID ,
                  wo.RelatedLiteNum ,
                  pl.SupplierPlant
              ) AS STBL PIVOT
			              ( MAX(SupplierPlant) FOR RelatedLiteNum IN ( [1], [2], [3] ) ) AS PTBL";
            return cmd;
        }

        public SqlCommand GetPurchasedLites2(OpenUnits units)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
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
                WHERE
                  ub.UnitID IN ( {0} )
                GROUP BY
                  ub.UnitID ,
                  wo.RelatedLiteNum ,
                  pl.SupplierPlant
              ) AS STBL PIVOT
			              ( MAX(SupplierPlant) FOR RelatedLiteNum IN ( [1], [2], [3] ) ) AS PTBL", String.Join(",", units.Select(s => s.UnitIdChar)));
            return cmd;
        }

        public void GetPurchasedLites(OpenUnits units, SqlConnection cn)
        {
            try
            {
                var cmd = GetPurchasedLites();
                //var cmd = GetPurchasedLites(units);
                cmd.Connection = cn;
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
                    foreach (OpenUnit unit in units)
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
	            A.DLUKID, 
	            A.DLZ58RID, 
	            A.DLZ58SN, 
	            A.DLZ58DST, 
	            A.DLPSN, 
	            A.DLUPMJ, 
	            A.DLTDAY 
            FROM 
	            F580569 A 
	            INNER JOIN (SELECT DLZ58DST, DLPSN, MAX(DLUKID) AS DLUKID FROM F580569 WHERE DLDOC1 = ? AND DLMCU = ? AND DLZ58SN = ? AND DLZ58DST IN ('4','5') GROUP BY DLZ58DST, DLPSN ) B 
	            ON B.DLUKID = A.DLUKID
            WHERE
                DLDOC1 > 0";
            //cmd.Parameters.Add(new iDB2Parameter("RackId", iDB2DbType.iDB2Decimal));
            //cmd.Parameters["RackId"].Precision = 15;
            //cmd.Parameters["RackId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("WOrderNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["WOrderNum"].Precision = 8;
            cmd.Parameters["WOrderNum"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SerialNum", iDB2DbType.iDB2Graphic, 20));
            return cmd;
        }

        public void GetLockLoad(OpenUnits units, BusinessSystemSession session, PlantAttribute plant)
        {
            try
            {
                var cmd = (session.DbCommands.ContainsKey("OpenUnitsManager.GetLockLoad")) ? session.DbCommands["OpenUnitsManager.GetLockLoad"] : GetLockLoad();
                cmd.Connection = session.DbConnection;
                foreach (OpenUnit unit in units)
                {
                    //cmd.Parameters[0].Value = unit.RackId;
                    cmd.Parameters[0].Value = unit.WorkOrderNum;
                    cmd.Parameters[1].Value = plant.NumericCode.PadLeft(12);
                    cmd.Parameters[2].Value = unit.UnitId;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var status = CastDbNull.To<String>(reader["DLZ58DST"], String.Empty).Trim();
                            var loadNum = CastDbNull.To<Decimal>(reader["DLPSN"], 0);
                            if (status == "4")
                            {
                                unit.RackCloseDate = Common.Utilities.Common.CastToDateTime(CastDbNull.To<decimal>(reader["DLUPMJ"], 0), CastDbNull.To<decimal>(reader["DLTDAY"], 0)).AddHours(plant.TimeOffset);
                            }
                            else
                            {
                                if (status == "5")
                                {
                                    if (loadNum == 0)
                                    {
                                        unit.RackLockDate = Common.Utilities.Common.CastToDateTime(CastDbNull.To<decimal>(reader["DLUPMJ"], 0), CastDbNull.To<decimal>(reader["DLTDAY"], 0)).AddHours(plant.TimeOffset);
                                    }
                                    else
                                    {
                                        unit.RackLockLoadDate = Common.Utilities.Common.CastToDateTime(CastDbNull.To<decimal>(reader["DLUPMJ"], 0), CastDbNull.To<decimal>(reader["DLTDAY"], 0)).AddHours(plant.TimeOffset);
                                    }
                                }
                            }

                        }

                        reader.Close();
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

        public void GetOriginalSchedule(OpenUnits units, BusinessSystemSession session, PlantAttribute plant)
        {
            try
            {
                var cmd1 = (session.DbCommands.ContainsKey("OpenUnitsManager.GetOriginalUKID")) ? session.DbCommands["OpenUnitsManager.GetOriginalUKID"] : GetOriginalUKID();
                var cmd2 = (session.DbCommands.ContainsKey("OpenUnitsManager.GetOriginalRack")) ? session.DbCommands["OpenUnitsManager.GetOriginalRack"] : GetOriginalRack();
                cmd1.Connection = session.DbConnection;
                cmd2.Connection = session.DbConnection;
                foreach (OpenUnit unit in units)
                {
                    cmd1.Parameters[0].Value = plant.NumericCode.PadLeft(12);
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
                                    unit.IsMissing = true;
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

    }
}



