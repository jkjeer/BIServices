using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public class ScheduleManagerShopFloor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ScheduleManagerShopFloor(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("ScheduleManager.GetScheduleRackMetrics", GetScheduleRackMetrics());
            hash.Add("ScheduleManager.GetScheduleLines", GetScheduleLines());
            hash.Add("ScheduleManager.GetPurchasedLites", GetPurchasedLites());
            hash.Add("ScheduleManager.GetBatchName", GetBatchName());
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
        public SqlCommand GetMtoSchedules()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                A.ScheduleNum,
                A.ScheduleDate,
	            A.ScheduleType,
                A.DueDate,
                A.CustNum,
                A.CustName,
	            --
	            SUM(A.Quantity) AS QTY,
	            --
                SUM(CASE WHEN A.UnitType = '1' THEN A.Quantity ELSE 0 END) AS [MONO],
                SUM(CASE WHEN A.UnitType = '2' THEN A.Quantity ELSE 0 END) AS [DOUBLE],
                SUM(CASE WHEN A.UnitType = '3' THEN A.Quantity ELSE 0 END) AS [TRIPLE],
	            --
	            SUM(CASE WHEN A.IGLine = '0' THEN A.Quantity ELSE 0 END) AS IG0, 
	            SUM(CASE WHEN A.IGLine = '1' THEN A.Quantity ELSE 0 END) AS IG1, 
	            SUM(CASE WHEN A.IGLine = '2' THEN A.Quantity ELSE 0 END) AS IG2, 
	            SUM(CASE WHEN A.IGLine = '3' THEN A.Quantity ELSE 0 END) AS IG3, 
	            SUM(CASE WHEN A.IGLine = '4' THEN A.Quantity ELSE 0 END) AS IG4, 
	            SUM(CASE WHEN A.IGLine = '5' THEN A.Quantity ELSE 0 END) AS IG5, 
	            SUM(CASE WHEN A.IGLine = '6' THEN A.Quantity ELSE 0 END) AS IG6, 
	            SUM(CASE WHEN A.IGLine = '7' THEN A.Quantity ELSE 0 END) AS IG7, 
	            SUM(CASE WHEN A.IGLine = '8' THEN A.Quantity ELSE 0 END) AS IG8, 
	            SUM(CASE WHEN A.IGLine = '9' THEN A.Quantity ELSE 0 END) AS IG9, 
	            SUM(CASE WHEN A.IGLine NOT IN ('0','1','2','3','4','5','6','7','8','9') THEN A.Quantity ELSE 0 END) AS IGX, 
	            --
                SUM(CASE WHEN A.ShapeNum > 0 THEN 1 ELSE 0 END) AS SHAPES,
                SUM(CASE
                           WHEN A.UnitType = '2' AND
                                A.Lite1_Routing LIKE '%U%' AND
                                A.Lite2_Routing LIKE '%U%' THEN A.Quantity
                           WHEN A.UnitType = '3' AND
                                A.Lite1_Routing LIKE '%U%' AND
                                A.Lite2_Routing LIKE '%U%' AND
                                A.Lite3_Routing LIKE '%U%' THEN A.Quantity
                           ELSE 0
                       END
                   ) AS FIN_PICKS,
                SUM(CASE
                           WHEN A.MCode1_Air1 > 0 THEN A.Quantity
                           ELSE 0
                       END
                   ) AS BARS,
	            --
                SUM(CASE
                           WHEN A.UnitType = '2' THEN A.Quantity * 2
                           WHEN A.UnitType = '3' THEN A.Quantity * 3
                           ELSE A.Quantity
                       END
                   ) AS LITES,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%C%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%C%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%C%' THEN A.Quantity ELSE 0 END) AS CUT_LITE,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%T%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%T%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%T%' THEN A.Quantity ELSE 0 END) AS TEMP_LITE,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%K%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%K%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%K%' THEN A.Quantity ELSE 0 END) AS PICK_LITE,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%P%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%P%' THEN A.Quantity ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%P%' THEN A.Quantity ELSE 0 END) AS PRCH_LITE,
	            --
	            SUM(A.ActualSqft) AS UNIT_ACTUAL_SQFT,
                SUM(CASE
                           WHEN A.UnitType = '2' THEN A.ActualSqft * 2
                           WHEN A.UnitType = '3' THEN A.ActualSqft * 3
                           ELSE A.ActualSqft
                       END
                   ) AS LITE_ACTUAL_SQFT,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%C%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%C%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%C%' THEN A.ActualSqft ELSE 0 END) AS CUT_SQFT,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%T%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%T%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%T%' THEN A.ActualSqft ELSE 0 END) AS TEMP_SQFT,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%K%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%K%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%K%' THEN A.ActualSqft ELSE 0 END) AS PICK_SQFT,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%P%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%P%' THEN A.ActualSqft ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%P%' THEN A.ActualSqft ELSE 0 END) AS PRCH_SQFT,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%T%' THEN A.Lite1Weight ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%T%' THEN A.Lite2Weight ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%T%' THEN A.Lite3Weight ELSE 0 END) AS TEMP_LBS,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%T%' THEN A.Lite1Weight/2000.000 ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%T%' THEN A.Lite2Weight/2000.000 ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%T%' THEN A.Lite3Weight/2000.000 ELSE 0 END) AS TEMP_TONS,
	            SUM(CASE WHEN A.Lite1_Routing LIKE '%P%' AND A.Lite1_Heat = 'T' THEN A.Lite1Weight/2000.000 ELSE 0 END + 
		            CASE WHEN A.Lite2_Routing LIKE '%P%' AND A.Lite1_Heat = 'T' THEN A.Lite2Weight/2000.000 ELSE 0 END + 
		            CASE WHEN A.Lite3_Routing LIKE '%P%' AND A.Lite1_Heat = 'T' THEN A.Lite3Weight/2000.000 ELSE 0 END) AS TEMP_PUR_TONS
            FROM
            (
                SELECT
		            ord.ScheduleNum,
		            ord.ScheduleDate,
		            ord.ScheduleType,
		            ord.DueDate,
		            ord.CustNum,
		            ord.CustName,
		            ord.SqftActual AS ActualSqft,
		            ord.ShapeNum,
		            1 AS Quantity,
		            CASE    WHEN ord.Lite3_Thick > 0 THEN '3'
		                    WHEN ord.Lite2_thick > 0 THEN '2' 
		                    ELSE '1' 
                    END AS UnitType,
		            ord.Lite1_Routing,
		            ord.Lite2_Routing,
		            ord.Lite3_Routing,
		            ord.Lite1_Heat,
		            ord.Lite2_Heat,
		            ord.Lite3_Heat,
		            ord.LineID,
		            ub.IGLine,
		            ub.IGLineRun,
					ord.Lite1Weight,
					ord.Lite2Weight,
					ord.Lite3Weight,
					ord.UnitWeight,
                    ord.MCode1_Air1
                FROM
		            ShopFloor.dbo.IncomingOrders ord WITH (NOLOCK)
		            LEFT OUTER JOIN ShopFloor.dbo.UnitBox ub 
		            ON ub.OrderItem = ord.WOrderNum AND
                    ub.SchedNum = ord.ScheduleNum
                WHERE
                    ord.ScheduleDate >= @StartDate AND
                    ord.ScheduleDate <= @EndDate
            ) A
            GROUP BY
                ScheduleNum,
                ScheduleDate,
	            ScheduleType,
                DueDate,
                CustNum,
                CustName
            ORDER BY 1,2,3,4,5,6";
            //cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            //cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public List<ScheduleSummary> GetScheduleSummary(BusinessSystemSession session, SqlConnection cnSf, Plant plant, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var schedules = new List<ScheduleSummary>();
            var cmd = new SqlCommand();
            List<String> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {

                // Dates
                DateTime startDateValue;
                DateTime endDateValue;
                if (!String.IsNullOrEmpty(startDate) || !String.IsNullOrEmpty(endDate) || !String.IsNullOrEmpty(dateCode))
                {
                    if (!String.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        startDateValue = dateValues[0];
                        endDateValue = dateValues[1];
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(startDate))
                        {
                            startDateValue = DateCodeManager.GetDateCodeValue(startDate);
                        }
                        else
                        {
                            startDateValue = new DateTime(1900, 1, 1);
                        }

                        if (!String.IsNullOrEmpty(endDate))
                        {
                            endDateValue = DateCodeManager.GetDateCodeValue(endDate);
                        }
                        else
                        {
                            endDateValue = new DateTime(2050, 12, 31);
                        }
                    }
                }
                else
                {
                    startDateValue = DateTime.Now.Date;
                    endDateValue = DateTime.Now.Date;
                }

                parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
                parameters.Last().Value = startDateValue;

                parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
                parameters.Last().Value = endDateValue;

                cmd = GetMtoSchedules();
                foreach (SqlParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = cnSf;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var schedule = new ScheduleSummary();
                        schedule.Plant = plant.PlantNum;
                        schedule.ScheduleNum = CastDbNull.To<Decimal>(reader["ScheduleNum"], 0);
                        schedule.ScheduledDate = CastDbNull.To<DateTime>(reader["ScheduleDate"], new DateTime(1900, 1, 1));
                        schedule.DueDate = CastDbNull.To<DateTime>(reader["DueDate"], new DateTime(1900, 1, 1));
                        schedule.ScheduleType = CastDbNull.To<String>(reader["ScheduleType"], String.Empty).Trim();
                        schedule.CustomerName = CastDbNull.To<String>(reader["CustName"], String.Empty).Trim();

                        schedule.LineQty = CastDbNull.To<int>(reader["QTY"], 0);

                        schedule.MonoQty = CastDbNull.To<int>(reader["MONO"], 0);
                        schedule.DoubleQty = CastDbNull.To<int>(reader["DOUBLE"], 0);
                        schedule.TripleQty = CastDbNull.To<int>(reader["TRIPLE"], 0);
                        schedule.ShapesQty = CastDbNull.To<int>(reader["SHAPES"], 0);
                        schedule.Pick = CastDbNull.To<int>(reader["FIN_PICKS"], 0);
                        schedule.BarsQty = CastDbNull.To<int>(reader["BARS"], 0);

                        schedule.Line0Qty = CastDbNull.To<int>(reader["IG0"], 0);
                        schedule.Line1Qty = CastDbNull.To<int>(reader["IG1"], 0);
                        schedule.Line2Qty = CastDbNull.To<int>(reader["IG2"], 0);
                        schedule.Line3Qty = CastDbNull.To<int>(reader["IG3"], 0);
                        schedule.Line4Qty = CastDbNull.To<int>(reader["IG4"], 0);
                        schedule.Line5Qty = CastDbNull.To<int>(reader["IG5"], 0);
                        schedule.Line6Qty = CastDbNull.To<int>(reader["IG6"], 0);
                        schedule.Line7Qty = CastDbNull.To<int>(reader["IG7"], 0);
                        schedule.Line8Qty = CastDbNull.To<int>(reader["IG8"], 0);
                        schedule.Line9Qty = CastDbNull.To<int>(reader["IG9"], 0);
                        schedule.LineOtherQty = CastDbNull.To<int>(reader["IGX"], 0);

                        schedule.TemperLite = CastDbNull.To<int>(reader["TEMP_LITE"], 0);
                        schedule.CutLite = CastDbNull.To<int>(reader["CUT_LITE"], 0);
                        schedule.PickLite = CastDbNull.To<int>(reader["PICK_LITE"], 0);
                        schedule.PurchaseLite = CastDbNull.To<int>(reader["PRCH_LITE"], 0);

                        schedule.TotalUnitSqft = CastDbNull.To<decimal>(reader["UNIT_ACTUAL_SQFT"], 0);
                        schedule.TotalLiteSqft = CastDbNull.To<decimal>(reader["LITE_ACTUAL_SQFT"], 0);
                        schedule.TemperSqft = CastDbNull.To<decimal>(reader["TEMP_SQFT"], 0);
                        schedule.CutSqft = CastDbNull.To<decimal>(reader["CUT_SQFT"], 0);
                        schedule.PickSqft = CastDbNull.To<decimal>(reader["PICK_SQFT"], 0);
                        schedule.PurchaseSqft = CastDbNull.To<decimal>(reader["PRCH_SQFT"], 0);
                        schedule.TemperLbs = CastDbNull.To<decimal>(reader["TEMP_LBS"], 0);
                        schedule.TemperTons = CastDbNull.To<decimal>(reader["TEMP_TONS"], 0);
                        schedule.TemperPurchasedTons = CastDbNull.To<decimal>(reader["TEMP_PUR_TONS"], 0);

                        schedules.Add(schedule);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            //GetScheduleLines(session, schedules);
            //GetScheduleRackMetrics(session, schedules);
            //GetPurchasedLites(session, schedules);
            //GetBatchName(session, schedules);
            return schedules;
        }

        /*********************************************************************************
          * Get Schedule Lines
          *******************************************************************************/
        public iDB2Command GetScheduleLines()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        S1Z58SCN,
		        S1Z58SCD,
		        S1MCU,  
                S5Z58SCSN,
                S5DOCO,
                S5ALPH,
                S5Z58IGLN,
                S5DRQJ,
                S5Z58QTY,
                SAZ587569,   
                SAZ587581, 
                SAZ58SHAPE,
                SAZ58M1A1,
                SAZ58GSS1,
                SAZ58L1HT,
                SAZ58L2HT,
                SAZ58L3HT,
                SAZ58L1LPT,
                SAZ58L2LPT,
                SAZ58L3LPT,
                SAZ58L1RTG,
                SAZ58L2RTG,
                SAZ58L3RTG,
                SAZ58USF,
                IBSRP4,
                SDURCD
	        FROM
		        F5834A01
		        INNER JOIN F5834A11
		        ON	S5MCU = S1MCU AND
			        S5Z58SCN = S1Z58SCN AND
			        S5Z58SCD = S1Z58SCD
		        INNER JOIN F5834A15
		        ON	SAMCU = S5MCU AND
			        SAZ58WODC = S5DOCO AND
			        SAZ58WOTY = S5DCTO
                INNER JOIN F4102
                ON  IBMCU = S5MCU AND
                    IBITM = S5ITM
                INNER JOIN F4211
                ON  SDMCU = S5MCU AND
					SDKCOO = S5RKCO AND
					SDDOCO = S5ODOC AND
					SDDCTO = S5RCTO AND
					SDLNID = S5LNID AND
                    S5DOCO > 0
	        WHERE
		        S1MCU = ? AND
		        S1Z58SCN = ? AND
		        S1Z58SCD = ?
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SCN", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCN"].Precision = 5;
            cmd.Parameters["SCN"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD"].Precision = 6;
            cmd.Parameters["SCD"].Scale = 0;
            return cmd;
        }

        public void GetScheduleLines(BusinessSystemSession session, List<ScheduleSummary> schedules)
        {
            try
            {
                var cmd = session.DbCommands["ScheduleManager.GetScheduleLines"];
                cmd.Connection = session.DbConnection;
                foreach (var schedule in schedules)
                {
                    var lines = new ScheduleLines();
                    cmd.Parameters[0].Value = schedule.Plant.PadLeft(12);
                    cmd.Parameters[1].Value = schedule.ScheduleNum;
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(schedule.ScheduledDate);
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var line = new ScheduleLine();
                            var reqDateId = CastDbNull.To<Decimal>(reader["S5DRQJ"], 0);
                            line.DueDate = E1TimeManager.CastToDateTime(reqDateId);
                            line.UnitType = CastDbNull.To<String>(reader["IBSRP4"], String.Empty).Trim();
                            //line.UnitType = (srp4 == "1" || srp4 == "L") ? "MONO" : "IG";
                            line.CustomerName = CastDbNull.To<String>(reader["S5ALPH"], String.Empty).Trim();
                            line.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                            line.IGLine = CastDbNull.To<String>(reader["S5Z58IGLN"], String.Empty).Trim();
                            line.Quantity = CastDbNull.To<Decimal>(reader["S5Z58QTY"], 0);
                            line.IntScoring = CastDbNull.To<Decimal>(reader["SAZ587581"], 0);
                            line.ExtScoring = CastDbNull.To<Decimal>(reader["SAZ587569"], 0);
                            line.ShapeNum = Common.Utilities.Data.ParseStringToDecimal(CastDbNull.To<String>(reader["SAZ58SHAPE"], String.Empty).Trim());
                            line.Mcode1Air1 = CastDbNull.To<Decimal>(reader["SAZ58M1A1"], 0);
                            line.GridStyle = CastDbNull.To<String>(reader["SAZ58GSS1"], String.Empty).Trim();
                            line.Lite1LamiProdType = CastDbNull.To<String>(reader["SAZ58L1LPT"], String.Empty).Trim();
                            line.Lite2LamiProdType = CastDbNull.To<String>(reader["SAZ58L2LPT"], String.Empty).Trim();
                            line.Lite3LamiProdType = CastDbNull.To<String>(reader["SAZ58L3LPT"], String.Empty).Trim();
                            line.Lite1HeatTreatment = CastDbNull.To<String>(reader["SAZ58L1HT"], String.Empty).Trim();
                            line.Lite2HeatTreatment = CastDbNull.To<String>(reader["SAZ58L2HT"], String.Empty).Trim();
                            line.Lite3HeatTreatment = CastDbNull.To<String>(reader["SAZ58L3HT"], String.Empty).Trim();
                            line.Lite1Routing = CastDbNull.To<Decimal>(reader["SAZ58L1RTG"], 0);
                            line.Lite2Routing = CastDbNull.To<Decimal>(reader["SAZ58L2RTG"], 0);
                            line.Lite3Routing = CastDbNull.To<Decimal>(reader["SAZ58L3RTG"], 0);
                            line.UnitActualSqft = CastDbNull.To<Decimal>(reader["SAZ58USF"], 0) / 10000;
                            lines.Add(line);
                            //schedule.ScheduleLines = lines;
                        }

                        reader.Close();
                    }

                    if (lines.Any())
                    {
                        schedule.DueDate = lines.Min(x => x.DueDate);
                        schedule.MinRequestedDate = lines.Min(x => x.DueDate);
                        schedule.MaxRequestedDate = lines.Max(x => x.DueDate);
                        schedule.CustomerName = lines.GroupBy(x => x.CustomerName).OrderByDescending(z => z.Sum(t => t.Quantity)).First().Key;
                        var unitType = lines.GroupBy(x => x.UnitType).OrderByDescending(z => z.Sum(t => t.Quantity)).First().Key;
                        schedule.UnitType = (unitType == "1" || unitType == "L") ? "MONO" : "IG";
                        schedule.PriorityCode = lines.GroupBy(x => x.PriorityCode).OrderByDescending(z => z.Sum(t => t.Quantity)).First().Key;
                        schedule.IGLine = lines.GroupBy(x => x.IGLine).OrderByDescending(z => z.Sum(t => t.Quantity)).First().Key;
                        schedule.LineQty = lines.Sum(y => y.Quantity);
                        schedule.Line0Qty = lines.Where(x => x.IGLine == "0").Sum(y => y.Quantity);
                        schedule.Line1Qty = lines.Where(x => x.IGLine == "1").Sum(y => y.Quantity);
                        schedule.Line2Qty = lines.Where(x => x.IGLine == "2").Sum(y => y.Quantity);
                        schedule.Line3Qty = lines.Where(x => x.IGLine == "3").Sum(y => y.Quantity);
                        schedule.Line4Qty = lines.Where(x => x.IGLine == "4").Sum(y => y.Quantity);
                        schedule.Line5Qty = lines.Where(x => x.IGLine == "5").Sum(y => y.Quantity);
                        schedule.Line6Qty = lines.Where(x => x.IGLine == "6").Sum(y => y.Quantity);
                        schedule.Line7Qty = lines.Where(x => x.IGLine == "7").Sum(y => y.Quantity);
                        schedule.Line8Qty = lines.Where(x => x.IGLine == "8").Sum(y => y.Quantity);
                        schedule.Line9Qty = lines.Where(x => x.IGLine == "9").Sum(y => y.Quantity);

                        var includedLines = new string[] { };
                        switch (schedule.Plant)
                        {
                            case "19":
                                includedLines = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                                break;
                            default:
                                includedLines = new[] { "1", "2", "3", "4", "5", "6", "7" };
                                break;
                        }
                        schedule.LineOtherQty = lines
                            .Where(x => !includedLines.Contains(x.IGLine))
                            .Sum(y => y.Quantity);

                        schedule.MonoQty = lines.Where(x => x.UnitType == "1" || x.UnitType == "L").Sum(y => y.Quantity);
                        schedule.DoubleQty = lines.Where(x => x.UnitType == "2").Sum(y => y.Quantity);
                        schedule.TripleQty = lines.Where(x => x.UnitType == "3").Sum(y => y.Quantity);
                        schedule.ScoredQty = lines.Where(x => x.IntScoring > 0 || x.ExtScoring > 0).Sum(y => y.Quantity);
                        schedule.ShapesQty = lines.Where(x => x.ShapeNum > 0).Sum(y => y.Quantity);

                        if (schedule.Plant == "19")
                        {
                            var lamiLines = from l in lines
                                where (l.Lite1Routing >= 10 && l.Lite1Routing < 20 && l.Lite1LamiProdType != "") ||
                                      (l.Lite2Routing >= 10 && l.Lite2Routing < 20 && l.Lite2LamiProdType != "") ||
                                      (l.Lite3Routing >= 10 && l.Lite3Routing < 20 && l.Lite3LamiProdType != "")
                                select l;
                            schedule.LamiQty = lamiLines.Sum(x => x.Quantity);
                        }
                        else
                        {
                            schedule.LamiQty = lines
                                .Where(x => x.Lite1LamiProdType != "" || x.Lite2LamiProdType != "" ||
                                            x.Lite3LamiProdType != "").Sum(y => y.Quantity);
                        }

                        schedule.CutSqft = lines.Where(x => x.Lite1Routing >= 10 && x.Lite1Routing < 20)
                                               .Sum(y => y.UnitActualSqft * y.Quantity) +
                                           lines.Where(x => x.Lite2Routing >= 10 && x.Lite2Routing < 20)
                                               .Sum(y => y.UnitActualSqft * y.Quantity) +
                                           lines.Where(x => x.Lite3Routing >= 10 && x.Lite3Routing < 20)
                                               .Sum(y => y.UnitActualSqft * y.Quantity);
                        var heatTreatments = new[] { "H", "T" };
                        schedule.TemperSqft = lines.Where(x => heatTreatments.Contains(x.Lite1HeatTreatment) && x.Lite1Routing < 20)
                                                  .Sum(y => y.UnitActualSqft * y.Quantity) +
                                              lines.Where(x => heatTreatments.Contains(x.Lite2HeatTreatment) && x.Lite2Routing < 20)
                                                  .Sum(y => y.UnitActualSqft * y.Quantity) +
                                              lines.Where(x => heatTreatments.Contains(x.Lite3HeatTreatment) && x.Lite3Routing < 20)
                                                  .Sum(y => y.UnitActualSqft * y.Quantity);
                        schedule.TotalUnitSqft = lines.Sum(x => x.UnitActualSqft * x.Quantity);
                        schedule.TotalLiteSqft = lines.Where(x => x.Lite1Routing > 0)
                                                     .Sum(y => y.UnitActualSqft * y.Quantity) +
                                                 lines.Where(x => x.Lite2Routing > 0)
                                                     .Sum(y => y.UnitActualSqft * y.Quantity) +
                                                 lines.Where(x => x.Lite3Routing > 0)
                                                     .Sum(y => y.UnitActualSqft * y.Quantity);
                        schedule.TotalSqft = schedule.TotalUnitSqft;
                        schedule.PatternBarsQty =
                            lines.Where(x => x.ShapeNum > 0 && x.Mcode1Air1 > 0).Sum(y => y.Quantity) +
                            lines.Where(x => x.ShapeNum == 0 && x.PriorityCode == "1D").Sum(y => y.Quantity);
                        schedule.DiamondBarsQty =
                            lines.Where(x => x.ShapeNum == 0 && x.PriorityCode == "1D" && x.GridStyle != "400" && x.Mcode1Air1 > 0).Sum(y => y.Quantity) +
                            lines.Where(x => x.ShapeNum == 0 && x.GridStyle == "400" && x.Mcode1Air1 > 0).Sum(y => y.Quantity);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Get Requested Date Facts
          *******************************************************************************/
        public iDB2Command GetScheduleAggregateProperties()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        S1Z58SCN,
		        S1Z58SCD,
		        S1MCU,  
                MAX(SDURCD) AS SDURCD,
                MAX(S5ALPH) AS S5ALPH,
                MAX(S5Z58IGLN) AS S5Z58IGLN,
                MAX(IBSRP4) AS IBSRP4,         
		        MIN(S5DRQJ) AS S5DRQJ1,
		        MAX(S5DRQJ) AS S5DRQJ2
	        FROM
		        F5834A01
		        INNER JOIN F5834A11
		        ON	S5MCU = S1MCU AND
			        S5Z58SCN = S1Z58SCN AND
			        S5Z58SCD = S1Z58SCD
                INNER JOIN F4102
                ON  IBMCU = S5MCU AND
                    IBITM = S5ITM
                INNER JOIN F4211
                ON  SDMCU = S5MCU AND
					SDKCOO = S5RKCO AND
					SDDOCO = S5ODOC AND
					SDDCTO = S5RCTO AND
					SDLNID = S5LNID AND
                    S5DOCO > 0
	        WHERE
		        S1MCU = ? AND
		        S1Z58SCN = ? AND
		        S1Z58SCD = ?
	        GROUP BY
		        S1Z58SCN,
		        S1Z58SCD,
		        S1MCU
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SCN", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCN"].Precision = 5;
            cmd.Parameters["SCN"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD"].Precision = 6;
            cmd.Parameters["SCD"].Scale = 0;
            return cmd;
        }

        public void GetScheduleAggregateProperties(BusinessSystemSession session, List<ScheduleSummary> schedules)
        {
            try
            {
                var cmd = session.DbCommands["ScheduleManager.GetScheduleAggregateProperties"];
                cmd.Connection = session.DbConnection;
                foreach (var schedule in schedules)
                {
                    cmd.Parameters[0].Value = schedule.Plant.PadLeft(12);
                    cmd.Parameters[1].Value = schedule.ScheduleNum;
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(schedule.ScheduledDate);
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var reqDateId1 = CastDbNull.To<Decimal>(reader["S5DRQJ1"], 0);
                            schedule.MinRequestedDate = E1TimeManager.CastToDateTime(reqDateId1);
                            var reqDateId2 = CastDbNull.To<Decimal>(reader["S5DRQJ2"], 0);
                            schedule.MaxRequestedDate = E1TimeManager.CastToDateTime(reqDateId2);
                            var srp4 = CastDbNull.To<String>(reader["IBSRP4"], String.Empty).Trim();
                            schedule.UnitType = (srp4 == "1" || srp4 == "L") ? "MONO" : "IG";
                            schedule.CustomerName = CastDbNull.To<String>(reader["S5ALPH"], String.Empty).Trim();
                            schedule.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                            schedule.IGLine = CastDbNull.To<String>(reader["S5Z58IGLN"], String.Empty).Trim();
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
          * Get Requested Date Facts
          *******************************************************************************/
        public iDB2Command GetScheduleRackMetrics()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        S1Z58SCN,
		        S1Z58SCD,
		        S1MCU,  
		        SUM(CASE WHEN WASRST IN ('98') THEN 1 ELSE 0 END) AS CANCELLED,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('0') THEN 1 ELSE 0 END) AS VOID,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('1') THEN 1 ELSE 0 END) AS OPEN,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('4') THEN 1 ELSE 0 END) AS CLOSED,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('5') THEN 1 ELSE 0 END) AS LOCKED,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('7') THEN 1 ELSE 0 END) AS PACKED,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('8') THEN 1 ELSE 0 END) AS SHIPPED,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST IN ('9') THEN 1 ELSE 0 END) AS HOLD,
		        SUM(CASE WHEN WASRST NOT IN ('98') AND RDZ58DST NOT IN ('0','1','4','5','7','8','9') THEN 1 ELSE 0 END) AS OTHER
	        FROM
		        F5834A01
		        INNER JOIN F5834A11
		        ON	S5MCU = S1MCU AND
			        S5Z58SCN = S1Z58SCN AND
			        S5Z58SCD = S1Z58SCD
		        INNER JOIN F580056 
		        ON  RDMCU = S5MCU AND
			        RDDOC1 = S5DOCO AND
			        RDXDCTO = S5DCTO
                INNER JOIN F4801
                ON  WADOCO = S5DOCO AND
                    WADCTO = S5DCTO
	        WHERE
		        S1MCU = ? AND
		        S1Z58SCN = ? AND
		        S1Z58SCD = ?
	        GROUP BY
		        S1Z58SCN,
		        S1Z58SCD,
		        S1MCU
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SCN", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCN"].Precision = 5;
            cmd.Parameters["SCN"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD"].Precision = 6;
            cmd.Parameters["SCD"].Scale = 0;
            return cmd;
        }

        public void GetScheduleRackMetrics(BusinessSystemSession session, List<ScheduleSummary> schedules)
        {
            try
            {
                var cmd = session.DbCommands["ScheduleManager.GetScheduleRackMetrics"];
                cmd.Connection = session.DbConnection;
                foreach (var schedule in schedules)
                {
                    cmd.Parameters[0].Value = schedule.Plant.PadLeft(12);
                    cmd.Parameters[1].Value = schedule.ScheduleNum;
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(schedule.ScheduledDate);
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            schedule.Cancelled = CastDbNull.To<int>(reader["CANCELLED"], 0);
                            schedule.Voided = CastDbNull.To<int>(reader["VOID"], 0);
                            schedule.Open = CastDbNull.To<int>(reader["OPEN"], 0);
                            schedule.Closed = CastDbNull.To<int>(reader["CLOSED"], 0);
                            schedule.Locked = CastDbNull.To<int>(reader["LOCKED"], 0);
                            schedule.Packed = CastDbNull.To<int>(reader["PACKED"], 0);
                            schedule.Shipped = CastDbNull.To<int>(reader["SHIPPED"], 0);
                            schedule.Held = CastDbNull.To<int>(reader["HOLD"], 0);
                            schedule.Other = CastDbNull.To<int>(reader["OTHER"], 0);
                            schedule.Produced = schedule.Closed + schedule.Locked + schedule.Packed + schedule.Shipped + schedule.Held;
                            schedule.OpenPct = (schedule.Scheduled > 0) ? Math.Round(schedule.Open / schedule.Scheduled, 3) : 0;
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
          * Get Purchased Lites
          *******************************************************************************/
        public iDB2Command GetPurchasedLites()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        S5Z58SCN,
		        S5Z58SCD,
		        S5MCU,  
                ROMCU2,
                ROEDTR,
		        SUM(PDUORG) AS PDUORG,
		        SUM(PDUOPN) AS PDUOPN,
		        SUM(PDUREC) AS PDUREC,
		        MAX(PDUPMJ) AS PDUPMJ
	        FROM
		        F5834A11
                INNER JOIN F580400
                ON  ROKCOO = S5RKCO AND
			        RODOCO = S5ODOC AND
			        RODCTO = S5RCTO AND
			        ROLNID = S5LNID AND
                    S5DOCO > 0
		        INNER JOIN F4311 
		        ON	PDKCOO = RORKCO AND 
			        PDDOCO = RODOC1 AND 
			        PDDCTO = RORCTO AND 
			        PDLNID = RORLLN
	        WHERE
		        S5MCU = ? AND
                S5Z58SCN = ? AND
		        S5Z58SCD = ? AND
		        PDLTTR != '980'
	        GROUP BY
		        S5Z58SCN,
		        S5Z58SCD,
		        S5MCU,
                ROMCU2,
                ROEDTR
            ORDER BY
                ROMCU2 ASC,
                ROEDTR DESC
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SCN", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCN"].Precision = 5;
            cmd.Parameters["SCN"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD"].Precision = 6;
            cmd.Parameters["SCD"].Scale = 0;
            return cmd;
        }


        public void GetPurchasedLites(BusinessSystemSession session, List<ScheduleSummary> schedules)
        {
            try
            {
                var cmd = session.DbCommands["ScheduleManager.GetPurchasedLites"];
                cmd.Connection = session.DbConnection;
                foreach (var schedule in schedules)
                {
                    cmd.Parameters[0].Value = schedule.Plant.PadLeft(12);
                    cmd.Parameters[1].Value = schedule.ScheduleNum;
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(schedule.ScheduledDate);
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        var sb = new StringBuilder();
                        while (reader.Read())
                        {
                            var purcLites = CastDbNull.To<Decimal>(reader["PDUORG"], 0) / 1000;
                            var recdLites = CastDbNull.To<Decimal>(reader["PDUREC"], 0) / 1000;
                            var supplier = CastDbNull.To<String>(reader["ROMCU2"], String.Empty).Trim();
                            var tranType = (CastDbNull.To<String>(reader["ROEDTR"], String.Empty).Trim() == "R") ? "R" : "";
                            sb.Append($"[{supplier}{tranType}] {recdLites}/{purcLites} ");
                        }

                        schedule.PurchasedLites = sb.ToString();
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
         * Get Open Units Dynamic
         *******************************************************************************/
        public iDB2Command GetBatchName()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        BHZ58GDES
	        FROM
		        F580230
	        WHERE
		        BHMCU = ? AND
                BHZ58SCN = ? AND
		        BHZ58SCD = ?
            ORDER BY
                BHZ58SCSN
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SCN", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCN"].Precision = 5;
            cmd.Parameters["SCN"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD"].Precision = 6;
            cmd.Parameters["SCD"].Scale = 0;
            return cmd;
        }

        public void GetBatchName(BusinessSystemSession session, List<ScheduleSummary> schedules)
        {
            try
            {
                var cmd = session.DbCommands["ScheduleManager.GetBatchName"];
                cmd.Connection = session.DbConnection;
                foreach (var schedule in schedules)
                {
                    cmd.Parameters[0].Value = schedule.Plant.PadLeft(12);
                    cmd.Parameters[1].Value = schedule.ScheduleNum;
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(schedule.ScheduledDate);
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            schedule.ScheduleName = CastDbNull.To<String>(reader["BHZ58GDES"], String.Empty).Trim();
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
