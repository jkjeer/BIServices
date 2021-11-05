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

namespace BIServices.Common.Managers.Enterprise
{
    public class ScheduleManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ScheduleManager(BusinessSystemSession session)
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
        public iDB2Command GetMtoSchedules(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
	        SELECT
		        S1MCU,
		        S1Z58SCN,
		        S1Z58SCD,
                S1Z58SCDDJ,
		        S1Z58SCTYP,
		        S1Z58SFS,
                S1Z58TQSC,
                S1Z58TQMAK,
                S1Z58TQPIK,
	            S1UB01,
	            S1UB02,
	            S1UB03,
	            S1UB04,
                S1UB06,
	            S1HQ01,
	            S1HQ02,
	            S1HQ03,
	            S1HQ04,
	            S1HQ05
	        FROM
		        F5834A01
	        WHERE
                {filterString}
		        S1Z58SCN >= 200 AND
		        S1Z58SCN < 1000 AND
                S1Z58SFS != '7' AND
                S1Z58SCSTS != '2'
            FOR READ ONLY";
            return cmd;
        }

        public List<ScheduleSummary> GetScheduleSummary(BusinessSystemSession session, Plant plant, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var schedules = new List<ScheduleSummary>();
            var cmd = new iDB2Command();
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant
                filters.Add("S1MCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Dates
                DateTime startDateValue;
                DateTime endDateValue;
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) ||
                    !string.IsNullOrEmpty(dateCode))
                {
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
                            startDateValue = DateCodeManager.GetDateCodeValue(startDate);
                        }
                        else
                        {
                            startDateValue = new DateTime(1900, 1, 1);
                        }


                        if (!string.IsNullOrEmpty(endDate))
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

                // Date Field
                if (!string.IsNullOrEmpty(dateProperty))
                {
                    switch (dateProperty.ToUpper())
                    {
                        case "DUE":
                        case "REQ":
                        case "REQUEST":
                        case "REQUESTED":
                            filters.Add("S1Z58SCDDJ >= ? AND\r\n");
                            filters.Add("S1Z58SCDDJ <= ? AND\r\n");
                            break;
                        case "SCH":
                        case "SCHEDULE":
                        case "SCHEDULED":
                            filters.Add("S1Z58SCD >= ? AND\r\n");
                            filters.Add("S1Z58SCD <= ? AND\r\n");
                            break;
                        default:
                            filters.Add("S1Z58SCD >= ? AND\r\n");
                            filters.Add("S1Z58SCD <= ? AND\r\n");
                            break;
                    }
                }
                else
                {
                    filters.Add("S1Z58SCD >= ? AND\r\n");
                    filters.Add("S1Z58SCD <= ? AND\r\n");
                }

                parameters.Add(new iDB2Parameter("StartDate", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)startDateValue);

                parameters.Add(new iDB2Parameter("EndDate", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime) endDateValue);
                
                // Schedule Date
                //filters.Add("S1Z58SCD >= ? AND\r\n");
                //parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
                //parameters.Last().Precision = 6;
                //parameters.Last().Scale = 0;
                //parameters.Last().Value = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(-30));

                //if (!String.IsNullOrEmpty(dateCode))
                //{
                //    var date = new DateTime(1900, 1, 1);
                //    try
                //    {
                //        date = DateTime.Parse(dateCode);
                //    }
                //    catch (FormatException e)
                //    {
                //        date = DateCodeManager.GetDateCodeValue(dateCode);
                //    }
                //    filters.Add("S1Z58SCD = ? AND\r\n");
                //    parameters.Add(new iDB2Parameter("SCD", iDB2DbType.iDB2Decimal));
                //    parameters.Last().Precision = 6;
                //    parameters.Last().Scale = 0;
                //    parameters.Last().Value = E1TimeManager.CastToE1Date(date);
                //}

                cmd = GetMtoSchedules(filters);
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
                        var schedule = new ScheduleSummary();
                        schedule.Plant = CastDbNull.To<string>(reader["S1MCU"], string.Empty).Trim();
                        schedule.ScheduleNum = CastDbNull.To<decimal>(reader["S1Z58SCN"], 0);
                        var schDateId = CastDbNull.To<decimal>(reader["S1Z58SCD"], 0);
                        schedule.ScheduledDate = E1TimeManager.CastToDateTime(schDateId);
                        schedule.ScheduleType = CastDbNull.To<string>(reader["S1Z58SCTYP"], string.Empty).Trim();
                        schedule.ScheduleSystem = CastDbNull.To<string>(reader["S1Z58SFS"], string.Empty).Trim();
                        schedule.Scheduled = CastDbNull.To<decimal>(reader["S1Z58TQSC"], 0);
                        schedule.Produce = CastDbNull.To<decimal>(reader["S1Z58TQMAK"], 0);
                        schedule.Pick = CastDbNull.To<decimal>(reader["S1Z58TQPIK"], 0);
                        schedule.Purchase = CastDbNull.To<decimal>(reader["S1HQ05"], 0) / 1000;
                        schedule.BarsQty = CastDbNull.To<decimal>(reader["S1UB01"], 0) / 1000;
                        schedule.TubesQty = CastDbNull.To<decimal>(reader["S1UB06"], 0) / 1000;
                        schedule.TotalSqft1 = CastDbNull.To<decimal>(reader["S1HQ02"], 0) / 1000;
                        schedule.TemperSqft1 = (CastDbNull.To<decimal>(reader["S1HQ01"], 0) / 1000) + (CastDbNull.To<decimal>(reader["S1HQ03"], 0) / 1000);
                        schedules.Add(schedule);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            GetScheduleLines(session, schedules);
            GetScheduleRackMetrics(session, schedules);
            GetPurchasedLites(session, schedules);
            GetBatchName(session, schedules);

            //if (!String.IsNullOrEmpty(minDueDateCode))
            //{
            //    var date = new DateTime(1900, 1, 1);
            //    try
            //    {
            //        date = DateTime.Parse(minDueDateCode);
            //    }
            //    catch (FormatException e)
            //    {
            //        date = DateCodeManager.GetDateCodeValue(minDueDateCode);
            //    }
            //    return schedules.Where(x => x.DueDate >= date).ToList();
            //}
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
                SDURCD,
                CPZ58CCV05
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
				LEFT OUTER JOIN F580375
				ON  CPMCU = S5MCU AND
					CPSHAN = S5SHAN
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
                            var reqDateId = CastDbNull.To<decimal>(reader["S5DRQJ"], 0);
                            line.DueDate = E1TimeManager.CastToDateTime(reqDateId);
                            line.UnitType = CastDbNull.To<string>(reader["IBSRP4"], string.Empty).Trim();
                            //line.UnitType = (srp4 == "1" || srp4 == "L") ? "MONO" : "IG";
                            line.CustomerName = CastDbNull.To<string>(reader["S5ALPH"], string.Empty).Trim();
                            line.CustomerTier = CastDbNull.To<string>(reader["CPZ58CCV05"], string.Empty).Trim();
                            line.PriorityCode = CastDbNull.To<string>(reader["SDURCD"], string.Empty).Trim();
                            line.IGLine = CastDbNull.To<string>(reader["S5Z58IGLN"], string.Empty).Trim();
                            line.Quantity = CastDbNull.To<decimal>(reader["S5Z58QTY"], 0);
                            line.IntScoring = CastDbNull.To<decimal>(reader["SAZ587581"], 0);
                            line.ExtScoring = CastDbNull.To<decimal>(reader["SAZ587569"], 0);
                            line.ShapeNum = Common.Utilities.Data.ParseStringToDecimal(CastDbNull.To<string>(reader["SAZ58SHAPE"], string.Empty).Trim());
                            line.Mcode1Air1 = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                            line.GridStyle = CastDbNull.To<string>(reader["SAZ58GSS1"], string.Empty).Trim();
                            line.Lite1LamiProdType = CastDbNull.To<string>(reader["SAZ58L1LPT"], string.Empty).Trim();
                            line.Lite2LamiProdType = CastDbNull.To<string>(reader["SAZ58L2LPT"], string.Empty).Trim();
                            line.Lite3LamiProdType = CastDbNull.To<string>(reader["SAZ58L3LPT"], string.Empty).Trim();
                            line.Lite1HeatTreatment = CastDbNull.To<string>(reader["SAZ58L1HT"], string.Empty).Trim();
                            line.Lite2HeatTreatment = CastDbNull.To<string>(reader["SAZ58L2HT"], string.Empty).Trim();
                            line.Lite3HeatTreatment = CastDbNull.To<string>(reader["SAZ58L3HT"], string.Empty).Trim();
                            line.Lite1Routing = CastDbNull.To<decimal>(reader["SAZ58L1RTG"], 0);
                            line.Lite2Routing = CastDbNull.To<decimal>(reader["SAZ58L2RTG"], 0);
                            line.Lite3Routing = CastDbNull.To<decimal>(reader["SAZ58L3RTG"], 0);
                            line.UnitActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000;
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
                        schedule.CustomerTier = lines.GroupBy(x => x.CustomerTier).OrderByDescending(z => z.Sum(t => t.Quantity)).First().Key;
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

                        var includedLines = new string[]{};
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
                            var reqDateId1 = CastDbNull.To<decimal>(reader["S5DRQJ1"], 0);
                            schedule.MinRequestedDate = E1TimeManager.CastToDateTime(reqDateId1);
                            var reqDateId2 = CastDbNull.To<decimal>(reader["S5DRQJ2"], 0);
                            schedule.MaxRequestedDate = E1TimeManager.CastToDateTime(reqDateId2);
                            var srp4 = CastDbNull.To<string>(reader["IBSRP4"], string.Empty).Trim();
                            schedule.UnitType = (srp4 == "1" || srp4 == "L") ? "MONO" : "IG";
                            schedule.CustomerName = CastDbNull.To<string>(reader["S5ALPH"], string.Empty).Trim();
                            schedule.PriorityCode = CastDbNull.To<string>(reader["SDURCD"], string.Empty).Trim();
                            schedule.IGLine = CastDbNull.To<string>(reader["S5Z58IGLN"], string.Empty).Trim();
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
                            var purcLites = CastDbNull.To<decimal>(reader["PDUORG"], 0) / 1000;
                            var recdLites = CastDbNull.To<decimal>(reader["PDUREC"], 0) / 1000;
                            var supplier = CastDbNull.To<string>(reader["ROMCU2"], string.Empty).Trim();
                            var tranType = (CastDbNull.To<string>(reader["ROEDTR"], string.Empty).Trim() == "R") ? "R" : "";
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
                            schedule.ScheduleName = CastDbNull.To<string>(reader["BHZ58GDES"], string.Empty).Trim();
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
