using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using NLog;

namespace BIServices.ShippingSvc
{
    public class RbaProductLineManager
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /**********************************************************************
         *  Product Line Summary
         *********************************************************************/
        public SqlCommand GetRbaProductLine()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                ProductLineCode,
                ProductLineDesc,
                HourlyRunRate,
                ShiftHours,
                MinFloorQty,
                MaxFloorQty,
                ReorderQty,
                RetireDays,
                UnitRatio,
                Display,
                WrittenDate
            FROM
                EBS.RbaProductLine
            ORDER BY
                ProductLineCode";
            return cmd;
        }

        public SqlCommand GetTargetShift()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                pl.ProductLineCode,
                A.ShiftNum,
                A.Active,
                A.StartDateTime,
                A.EndDateTime,
                A.DateType,
                A.ProductionDate
            FROM
                EBS.RbaShipCalendar sc
                INNER JOIN EBS.RbaShiftCalendar A
                ON  A.StartDateTime <= sc.LoadEndDate AND
                    A.EndDateTime >= sc.LoadEndDate
                INNER JOIN EBS.RbaProductLine pl
                ON  pl.ProductLineId = A.ProductLineId
            WHERE
                sc.LoadStartDate < GETDATE() AND
                sc.LoadEndDate > GETDATE()
            ORDER BY
                A.StartDateTime ASC;";
            return cmd;
        }

        public SqlCommand GetRbaProductLines()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                p.ProductLineCode,
                p.ProductLineDesc,
                p.HourlyRunRate,
                p.ShiftHours,
                p.MinFloorQty,
                p.MaxFloorQty,
                p.ReorderQty,
                p.RetireDays,
                p.UnitRatio,
                p.Display,
                b.LastConsumedDate
            FROM
                EBS.RbaProductLine p
                LEFT OUTER JOIN
                (
                    SELECT
                        ri.CardinalProductType,
                        MAX(bb.TransactionDate) AS LastConsumedDate
                    FROM
                        EBS.RbaConsumedBatchBin bb
                        INNER JOIN EBS.RbaShippedRackItem ri
                        ON  ri.Batch = bb.Batch AND
                            ri.Bin = bb.Bin
                    GROUP BY
                        ri.CardinalProductType
                ) b
                ON  b.CardinalProductType = p.ProductLineCode
            WHERE
                Display = 1
            ORDER BY
                1;";
            cmd.Parameters.Add(new SqlParameter("@BizDate", SqlDbType.DateTime));
            return cmd;
        }

        public SqlCommand GetMinShipDate()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                A.CalendarDate
            FROM
            (
                SELECT
                    CalendarDate,
                    ROW_NUMBER() OVER (ORDER BY
                                           CalendarDate DESC
                                      ) AS RN
                FROM
                    EBS.Calendar
                WHERE
                    CalendarName = 'SCHEDULE' AND
                    CalendarDate < GETDATE() AND
                    CalendarCode = 'W'
            ) A
            WHERE
                A.RN = 3";
            return cmd;
        }

        public RbaProductLines GetRbaProductLines(string plantCode, SqlConnection cn)
        {
            var lines = new RbaProductLines();
            try
            {
                var cmd = GetRbaProductLines();
                cmd.Parameters[0].Value = GetMinShipDate(cn);
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var line = new RbaProductLine();
                    line.ProductLineCode = CastDbNull.To<String>(reader["ProductLineCode"], String.Empty);
                    line.ProductLineName = CastDbNull.To<String>(reader["ProductLineDesc"], String.Empty);
                    line.Display = CastDbNull.To<bool>(reader["Display"], false) ? "Y" : "N";
                    line.HourlyRunRate = CastDbNull.To<int>(reader["HourlyRunRate"], 0);
                    line.ShiftHours = CastDbNull.To<int>(reader["ShiftHours"], 0);
                    line.ShiftRunRate = line.HourlyRunRate * line.ShiftHours;
                    line.MinFloorQty = CastDbNull.To<int>(reader["MinFloorQty"], 0);
                    line.MaxFloorQty = CastDbNull.To<int>(reader["MaxFloorQty"], 0);
                    line.ReorderQty = CastDbNull.To<int>(reader["ReorderQty"], 0);
                    line.RetireDays = CastDbNull.To<int>(reader["RetireDays"], 0);
                    line.UnitRatio = CastDbNull.To<decimal>(reader["UnitRatio"], 0);
                    line.LastConsumedDate = CastDbNull.To<DateTime>(reader["LastConsumedDate"], new DateTime(1900, 1, 1));
                    lines.Add(line);
                }
                reader.Close();
                var nextRacks = GetNextRacks(plantCode, 10, cn);
                var floorRacks = GetFloorRacks(plantCode, cn);
                var targetShifts = GetTargetShifts(plantCode, cn);
                foreach (var productLine in lines)
                {
                    productLine.NextRacks = nextRacks.Where(x => x.ProductLineCode == productLine.ProductLineCode).ToList();
                    productLine.FloorRacks = floorRacks.Where(x => x.ProductLineCode == productLine.ProductLineCode).ToList();
                    productLine.RbaFloorRacks = productLine.FloorRacks.Count;
                    productLine.RbaFloorUnits = productLine.FloorRacks.Sum(x => x.Units);
                    productLine.LastShipDate = productLine.FloorRacks.Any()
                        ? productLine.FloorRacks.Max(x => x.TransactionDate)
                        : new DateTime(1900, 1, 1);

                    var targetShift = targetShifts
                        .AsEnumerable()
                        .FirstOrDefault(
                            x => x.Field<string>("ProductLineCode") == productLine.ProductLineCode);
                    productLine.TargetShift = targetShift?.Field<int>("ShiftNum") ?? 0;
                    productLine.TargetShiftActive = targetShift?.Field<bool>("Active") ?? false ? "Y" : "N";

                    if (productLine.TargetShiftActive == "N")
                    {
                        productLine.RequiredUnits = 0;
                    }
                    else
                    {
                        productLine.RequiredUnits = productLine.ReorderQty - productLine.RbaFloorUnits;
                        int rackAmt = 0;
                        foreach (var rack in productLine.NextRacks)
                        {
                            if (rackAmt <= productLine.RequiredUnits)
                            {
                                rack.IsRequired = "Y";
                            }
                            rackAmt = rackAmt + rack.Units;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return lines;
        }

        public DateTime GetMinShipDate(SqlConnection cn)
        {
            DateTime minShipDate = new DateTime(1900,1,1);
            try
            {
                var cmd = GetMinShipDate();
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    minShipDate = Data.CastDbNull.To<DateTime>(reader["CalendarDate"], new DateTime(1900,1,1));
                }
                reader.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return minShipDate;
        }

        /**********************************************************************
         *  Next Racks
         *********************************************************************/
        public SqlCommand GetNextRacks()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                *
            FROM
            (
                SELECT
                    *,
                    ROW_NUMBER() OVER (PARTITION BY
                                           A.CardinalProductType
                                       ORDER BY
                                           RackNum ASC
                                      ) AS RN
                FROM
                (
                    SELECT
                        ri.CardinalProductType,
                        ri.RackNum,
                        ri.ScheduleNum,
                        ri.BoxNum,
                        ri.RackType,
                        MIN(ri.Status) AS Status,
                        MAX(ri.PriorityCode) AS PriorityCode,
                        MAX(ri.TransactionDate) AS TransactionDate,
                        COUNT(*) AS Units
                    FROM
                        ShopFloor.EBS.RbaCurrentRackItem ri
                    WHERE
                        ri.BoxNum < 30 AND
                        ri.BoxNum > 0 AND
                        EXISTS
                    (
                        SELECT
                            1
                        FROM
                            ShopFloor.EBS.RbaCurrentRackItem ri2
                        WHERE
                            ri2.RackNum = ri.RackNum AND
                            ri2.PriorityCode IN ('', '1D')
                    )
                    GROUP BY
                        ri.RackNum,
                        ri.CardinalProductType,
                        ri.ScheduleNum,
                        ri.BoxNum,
                        ri.RackType
                ) A
            ) B
            WHERE
                B.RN <= @Rows
            ORDER BY
                CardinalProductType,
                RackNum;";
            cmd.Parameters.Add(new SqlParameter("@Rows", SqlDbType.Int));
            return cmd;
        }

        public RbaRacks GetNextRacks(string plantCode, int rows, SqlConnection cn)
        {
            var racks = new RbaRacks();
            try
            {
                var cmd = GetNextRacks();
                cmd.Parameters[0].Value = rows;
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var rack = new RbaRack();
                    rack.RackNum = CastDbNull.To<decimal>(reader["RackNum"], 0);
                    rack.ScheduleNum = CastDbNull.To<decimal>(reader["ScheduleNum"], 0);
                    rack.BoxNum = CastDbNull.To<decimal>(reader["BoxNum"], 0);
                    rack.RackType = CastDbNull.To<String>(reader["RackType"], String.Empty);
                    rack.Status = CastDbNull.To<String>(reader["Status"], String.Empty);
                    rack.ProductLineCode = CastDbNull.To<String>(reader["CardinalProductType"], String.Empty);
                    rack.PriorityCode = CastDbNull.To<String>(reader["PriorityCode"], String.Empty);
                    rack.TransactionDate = CastDbNull.To<DateTime>(reader["TransactionDate"], new DateTime(1900, 1, 1));
                    rack.Units = CastDbNull.To<int>(reader["Units"], 0);
                    rack.IsRequired = "N";
                    racks.Add(rack);
                }
                reader.Close();

                //var productLines = GetRbaProductLines(plantCode, cn);
                //SetIsRequired(productLines, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return racks;
        }

        public void SetIsRequired(RbaProductLines lines, RbaRacks racks)
        {
            try
            {
                var rackLines = racks.GroupBy(x => x.ProductLineCode).ToList();
                foreach (var rackLine in rackLines)
                {
                    var productLine = lines.FirstOrDefault(x => x.ProductLineCode == rackLine.Key);
                    if (productLine != null)
                    {
                        var requiredAmt = productLine.RequiredUnits;
                        int rackAmt = 0;
                        foreach (var rack in rackLine)
                        {
                            if (rackAmt <= requiredAmt)
                            {
                                rack.IsRequired = "Y";
                            }
                            rackAmt = rackAmt + rack.Units;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        /**********************************************************************
         *  Next Racks
         *********************************************************************/
        public SqlCommand GetFloorRacks()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                ri.CardinalProductType,
                ri.RackNum,
	            ri.RackType,
                ri.ScheduleNum,
                ri.BoxNum,
                ri.Status,
                ri.PriorityCode,
                ri.ShipDate,
                COUNT(*) AS Units
            FROM
                ShopFloor.EBS.RbaShippedRackItem ri
            WHERE
                ri.ShipDate >= @BizDate AND
                ri.RackNum NOT IN
                (
                    SELECT
                        ri.RackNum
                    FROM
                        ShopFloor.EBS.RbaShippedRackItem ri
                        INNER JOIN ShopFloor.EBS.RbaConsumedBatchBin bb
                        ON  bb.Batch = ri.Batch AND
                            bb.Bin = ri.Bin
                    GROUP BY
                        ri.RackNum
                ) AND
                EXISTS
            (
                SELECT
                    1
                FROM
                    ShopFloor.EBS.RbaShippedRackItem ri2
                WHERE
                    ri2.RackNum = ri.RackNum AND
                    ri2.PriorityCode IN ('', '1D')
            )
            GROUP BY
                ri.RackNum,
	            ri.RackType,
                ri.ShipDate,
                ri.CardinalProductType,
                ri.ScheduleNum,
                ri.BoxNum,
                ri.Status,
                ri.PriorityCode
            ORDER BY 1,2,3,4,5";
            cmd.Parameters.Add(new SqlParameter("@BizDate", SqlDbType.DateTime));
            return cmd;
        }

        public RbaRacks GetFloorRacks(string plantCode, SqlConnection cn)
        {
            var racks = new RbaRacks();
            try
            {
                var cmd = GetFloorRacks();
                cmd.Parameters[0].Value = GetMinShipDate(cn);
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var rack = new RbaRack();
                    rack.RackNum = CastDbNull.To<decimal>(reader["RackNum"], 0);
                    rack.ScheduleNum = CastDbNull.To<decimal>(reader["ScheduleNum"], 0);
                    rack.BoxNum = CastDbNull.To<decimal>(reader["BoxNum"], 0);
                    rack.RackType = CastDbNull.To<String>(reader["RackType"], String.Empty);
                    rack.Status = CastDbNull.To<String>(reader["Status"], String.Empty);
                    rack.ProductLineCode = CastDbNull.To<String>(reader["CardinalProductType"], String.Empty);
                    rack.PriorityCode = CastDbNull.To<String>(reader["PriorityCode"], String.Empty);
                    rack.TransactionDate = CastDbNull.To<DateTime>(reader["ShipDate"], new DateTime(1900, 1, 1));
                    rack.Units = CastDbNull.To<int>(reader["Units"], 0);
                    racks.Add(rack);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return racks;
        }

        public DataTable GetTargetShifts(string plantCode, SqlConnection cn)
        {
            var dt = new DataTable("shifts");
            try
            {
                var cmd = GetTargetShift();
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return dt;
        }
    }
}
