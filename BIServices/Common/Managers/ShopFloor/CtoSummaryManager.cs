using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using BIServices.Common.Utilities;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public class CtoSummaryManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //public CtoSummaryManager(BusinessSystemSession session)
        //{
        //    var hash = new Dictionary<string, iDB2Command>();
        //    foreach (string keyName in hash.Keys)
        //    {
        //        if (!session.DbCommands.ContainsKey(keyName))
        //        {
        //            session.DbCommands.Add(keyName, hash[keyName]);
        //        }
        //    }
        //}

        /*********************************************************************************
         * Consumption
         *******************************************************************************/
        public SqlCommand GetConsumedSheets()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                 A.ReportDate,
                 A.ProductionLineCode,
                A.GroupDescription,
                 A.GlassTypeCode,
                 A.E1WorkcenterID,
                 A.Shift,
                 A.Crew,
                 MIN (WrittenDate)                               AS MinWrittenDate,
                 MAX (WrittenDate)                               AS MaxWrittenDate,
                 SUM (A.SHEETS * A.FACTOR)                       AS Sheets,
                 SUM (CAST(A.SQFT * A.FACTOR AS DECIMAL(15, 3))) AS Sqft,
                 SUM (CAST(A.Tons * A.FACTOR AS DECIMAL(15, 3))) AS Tons
            FROM (
                     SELECT
                         ReportDate,
                         ProductionLineCode,
                         pl.E1WorkcenterID,
                        pl.GroupDescription,
                         Shift,
                         Crew,
                         GlassTypeCode,
                         WrittenDate,
                         CASE
                             WHEN IsNegative = 0 THEN
                                  1
                             ELSE -1
                         END                                                                         AS FACTOR,
                         Quantity                                                                    AS SHEETS,
                         ROUND (SizeVertical * SizeHorizontal / 144, 2)                              AS SQFT,
                         ROUND (SizeVertical * SizeHorizontal / 144, 2) * GlassThicknessMM * .000249 AS Tons
                     FROM
                         ClearPathReporting.ConsumedStoce    cs WITH (NOLOCK)
                         INNER JOIN ClearPath.ProductionLine pl WITH (NOLOCK)
                         ON  pl.Code = cs.ProductionLineCode
                     WHERE
                         ReportDate BETWEEN @StartDate AND @EndDate 
                 ) A
            GROUP BY
                 A.ReportDate,
                 A.ProductionLineCode,
                A.GroupDescription,
                 A.GlassTypeCode,
                 A.E1WorkcenterID,
                 A.Shift,
                A.Crew";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public ConsumedSheets GetConsumedSheets(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var sheets = new ConsumedSheets();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetConsumedSheets();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                cmd.CommandTimeout = 60;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var sheet = new ConsumedSheet();
                        sheet.PlantCode = plant.PlantCode;
                        sheet.PlantNum = plant.PlantNum;
                        sheet.ReportDate = CastDbNull.To(reader["ReportDate"], DateTime.MinValue);
                        sheet.ProductionLineCode = CastDbNull.To(reader["ProductionLineCode"], string.Empty).Trim();
                        sheet.GroupDescription = CastDbNull.To(reader["GroupDescription"], string.Empty).Trim();
                        sheet.GlassTypeCode = CastDbNull.To(reader["GlassTypeCode"], string.Empty).Trim();
                        sheet.WorkCenterId = CastDbNull.To(reader["E1WorkCenterID"], string.Empty).Trim();
                        sheet.Shift = CastDbNull.To(reader["Shift"], 0);
                        sheet.Crew = CastDbNull.To(reader["Crew"], string.Empty).Trim();
                        sheet.Sheets = CastDbNull.To(reader["Sheets"], 0);
                        sheet.ActualSqft = CastDbNull.To<decimal>(reader["Sqft"], 0);
                        sheet.Tons = CastDbNull.To<decimal>(reader["Tons"], 0);
                        sheet.MinWrittenDate = CastDbNull.To(reader["MinWrittenDate"], DateTime.MinValue);
                        sheet.MaxWrittenDate = CastDbNull.To(reader["MaxWrittenDate"], DateTime.MinValue);
                        sheets.Add(sheet);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return sheets;
        }

        /*********************************************************************************
         * Offal
         *******************************************************************************/
        public SqlCommand GetConsumedOffal()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                SC.startDate AS ReportDate ,
                PL.Code AS ProductionLineCode ,
                PL.E1WorkcenterID ,
                PL.GroupDescription,
                SC.Shift ,
                SC.Crew ,
                GlassTypeCode AS Code ,
                COUNT(RSN) AS Lites,
                SUM(CAST(SizeHorizontal * SizeVertical / 144.00 AS DECIMAL(15,3))) AS Sqft ,
                SUM(CAST(SizeHorizontal * SizeVertical / 144.00 AS DECIMAL(15,3)) *
                CAST(SUBSTRING(GlassTypeCode, 1, CHARINDEX(':', GlassTypeCode) - 1) AS DECIMAL(3,
                                                                          1)) * .000249) AS Tons 
              FROM
                ClearPathReporting.OffalEvents OE WITH (NOLOCK)
                JOIN ClearPathReporting.SpecCuttingTable CT WITH (NOLOCK)
                  JOIN ClearPathReporting.ProductionLine PL WITH (NOLOCK)
                    ON CT.ProductionLineCode = PL.Code
                  ON ISNULL(OE.ConsumedByCutTable, OE.OffalRackCutTable) = CT.SpecCuttingTableCode
                JOIN dbo.tblShiftCalendar SC WITH (NOLOCK)
                  ON PL.E1WorkcenterID = SC.E1Workstation AND
                     OE.DecisionDate BETWEEN SC.startTime
                                     AND     SC.endTime
              WHERE
                SC.startDate >= @StartDate AND
                SC.startDate <= @EndDate AND
                EventCode = 'CONSUMED' AND
                PL.GroupDescription = 'LVCutMono'
              GROUP BY
                GlassTypeCode ,
                PL.Code ,
                PL.E1WorkcenterID ,
                PL.GroupDescription,
                SC.startDate ,
                SC.Shift ,
                SC.Crew;";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public OffalLites GetConsumedOffal(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new OffalLites();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetConsumedOffal();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                cmd.CommandTimeout = 60;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new OffalLite();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To(reader["ReportDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To(reader["ProductionLineCode"], string.Empty).Trim();
                        lite.GroupDescription = CastDbNull.To(reader["GroupDescription"], string.Empty).Trim();
                        lite.WorkCenterId = CastDbNull.To(reader["E1WorkCenterID"], string.Empty).Trim();
                        lite.Shift = Convert.ToInt16(CastDbNull.To(reader["Shift"], string.Empty).Trim());
                        lite.Crew = CastDbNull.To(reader["Crew"], string.Empty).Trim();
                        lite.Lites = CastDbNull.To(reader["Lites"], 0);
                        lite.ActualSqft = CastDbNull.To<decimal>(reader["Sqft"], 0);
                        lite.Tons = CastDbNull.To<decimal>(reader["Tons"], 0);
                        lites.Add(lite);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
         * Produced
         *******************************************************************************/
        public SqlCommand GetProducedLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                A.ReportDate ,
                A.ProductionLineCode ,
                A.GroupDescription,
                A.E1WorkCenterID,
                A.Shift ,
                A.Crew ,
                A.OriginTypeCode ,
                MIN(A.WrittenDate) AS MinWrittenDate,
                MAX(A.WrittenDate) AS MaxWrittenDate,
                SUM(A.Quantity * A.FACTOR) AS Lites,
                SUM(CAST((A.BaseDimSales * A.LeftDimSales / 144.0) * A.FACTOR AS DECIMAL(15,3))) AS Sqft,        
                SUM(CAST(A.AreaActual * A.FACTOR AS DECIMAL(15,3))) AS ActualSqft,   
                SUM(CAST(A.AreaBlockSales * A.FACTOR AS DECIMAL(15,3))) AS BillableSqft,   
                SUM(CAST((A.BaseDimSales * A.LeftDimSales / 144.0 * A.GlassThicknessMM * .000249 )  * A.FACTOR AS DECIMAL(15,3))) AS Tons,
				SUM(CASE WHEN A.Rare = 1 THEN 1 ELSE 0 END) AS RareLites,	
			    SUM(CASE WHEN A.Rare = 1 THEN CAST((A.BaseDimSales * A.LeftDimSales / 144.0) * A.FACTOR AS DECIMAL(15,3)) ELSE 0 END) AS RareSqft, 
			    SUM(CASE WHEN A.Rare = 1 THEN CAST(A.AreaActual * A.FACTOR AS DECIMAL(15,3)) ELSE 0 END) AS RareActualSqft, 
			    SUM(CASE WHEN A.Rare = 1 THEN CAST(A.AreaBlockSales * A.FACTOR AS DECIMAL(15,3)) ELSE 0 END) AS RareBillableSqft,  
			    SUM(CASE WHEN A.Rare = 1 THEN CAST((A.BaseDimSales * A.LeftDimSales / 144.0 * A.GlassThicknessMM * .000249 )  * A.FACTOR AS DECIMAL(15,3)) ELSE 0 END) AS RareTons
              FROM
                ( SELECT
                      ReportDate ,
                      ProductionLineCode ,
                        pd.GroupDescription,
                        pl.E1WorkCenterID,
                      Shift ,
                      Crew ,
                      OriginTypeCode ,
                      CASE WHEN IsNegative = 0 THEN 1
                           ELSE -1
                      END AS FACTOR ,
                      Quantity ,
					  GlassThicknessMM,
                      AreaBlockSales ,
                    CASE WHEN LiteShapeCode LIKE 'Custom%' THEN AreaBlock ELSE AreaActual END AS AreaActual,
					  CASE 
						WHEN GlassThicknessMM < 4.7 THEN CEILING(DimensionBlockH)
						WHEN cast(ceiling(DimensionBlockH) as int) % 2 = 0 then ceiling(DimensionBlockH)
						ELSE ceiling(DimensionBlockH) + 1
					  END AS BaseDimSales,
					  CASE 
						WHEN GlassThicknessMM < 4.7 THEN CEILING(DimensionBlockV)
						WHEN cast(ceiling(DimensionBlockV) as int) % 2 = 0 then ceiling(DimensionBlockV)
						ELSE ceiling(DimensionBlockV) + 1
					  END AS LeftDimSales,
                        WrittenDate,
                        Rare
                    FROM
                      ClearPathReporting.ProdLites AS pl WITH ( NOLOCK )                         
                        INNER JOIN ClearPath.ProductionLine pd WITH (NOLOCK)
                         ON  pd.Code = pl.ProductionLineCode
                    WHERE
                      ReportDate BETWEEN @StartDate AND @EndDate AND
                      ISNULL(RejectReasonDescription, '') = '' AND
                        (pl.ProductionLineCode LIKE 'CUTLV%' OR pl.ProductionLineCode LIKE 'TEMPER%'  OR pl.ProductionLineCode LIKE 'SEAM%')
                ) A
              GROUP BY
                A.ReportDate ,
                A.ProductionLineCode ,
                A.GroupDescription,
                A.E1WorkCenterID,
                A.Shift ,
                A.Crew ,
                A.OriginTypeCode";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public ProducedLites GetProducedLites(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new ProducedLites();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetProducedLites();
                cmd.Connection = cn;
                cmd.CommandTimeout = 60;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new ProducedLite();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To(reader["ReportDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To(reader["ProductionLineCode"], string.Empty).Trim();
                        lite.GroupDescription = CastDbNull.To(reader["GroupDescription"], string.Empty).Trim();
                        lite.WorkCenterId = CastDbNull.To(reader["E1WorkCenterID"], string.Empty).Trim();
                        lite.Shift = CastDbNull.To(reader["Shift"], 0);
                        lite.Crew = CastDbNull.To(reader["Crew"], string.Empty).Trim();
                        lite.OriginTypeCode = CastDbNull.To(reader["OriginTypeCode"], string.Empty).Trim();
                        lite.Lites = CastDbNull.To(reader["Lites"], 0);
                        lite.Sqft = CastDbNull.To<decimal>(reader["Sqft"], 0);
                        lite.ActualSqft = CastDbNull.To<decimal>(reader["ActualSqft"], 0);
                        lite.BillableSqft = CastDbNull.To<decimal>(reader["BillableSqft"], 0);
                        lite.Tons = CastDbNull.To<decimal>(reader["Tons"], 0);
                        lite.RareLites = CastDbNull.To(reader["RareLites"], 0);
                        lite.RareSqft = CastDbNull.To<decimal>(reader["RareSqft"], 0);
                        lite.RareActualSqft = CastDbNull.To<decimal>(reader["RareActualSqft"], 0);
                        lite.RareBillableSqft = CastDbNull.To<decimal>(reader["RareBillableSqft"], 0);
                        lite.RareTons = CastDbNull.To<decimal>(reader["RareTons"], 0);
                        lite.MinWrittenDate = CastDbNull.To(reader["MinWrittenDate"], DateTime.MinValue);
                        lite.MaxWrittenDate = CastDbNull.To(reader["MaxWrittenDate"], DateTime.MinValue);
                        lites.Add(lite);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
         * Produced
         *******************************************************************************/
        public SqlCommand GetRejectedLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                 A.ReportDate,
                 A.ProductionLineCode,
                 A.CutProductionLineCode,
                    A.GroupDescription,
                 A.E1WorkCenterID,
                 A.Shift,
                 A.Crew,
                 A.OriginTypeCode,
                 A.RejectReasonDescription,
                 A.RejectReasonBenchmarkDescription,
	             A.UpstreamType_Code,
                 SUM(A.Quantity * A.FACTOR)                                                                                       AS Lites,
                 SUM(CAST((A.BaseDimSales * A.LeftDimSales / 144.0) * A.FACTOR AS DECIMAL(15, 3)))                                AS Sqft,
                 SUM(CAST(A.AreaActual * A.FACTOR AS DECIMAL(15, 3)))                                                             AS ActualSqft,
                 SUM(CAST(A.AreaBlockSales * A.FACTOR AS DECIMAL(15, 3)))                                                         AS BillableSqft,
                 SUM(CAST((A.BaseDimSales * A.LeftDimSales / 144.0 * A.GlassThicknessMM * .000249) * A.FACTOR AS DECIMAL(15, 3))) AS Tons
            FROM (
                     SELECT
                         pl.ReportDate,
                         pl.ProductionLineCode,
                         cs.ProductionLineCode AS CutProductionLineCode,
                        pd.GroupDescription,
                         pl.E1WorkCenterID,
                         pl.Shift,
                         pl.Crew,
                         pl.OriginTypeCode,
                         pl.RejectReasonDescription,
                         pl.RejectReasonBenchmarkDescription,
			             rm.UpstreamType_Code,
                         CASE
                             WHEN pl.IsNegative = 0 THEN
                                  1
                             ELSE -1
                         END                   AS FACTOR,
                         pl.Quantity,
                         pl.GlassThicknessMM,
                         pl.AreaBlockSales,
                         pl.AreaActual,
                         CASE
                             WHEN pl.GlassThicknessMM < 4.7 THEN
                                  CEILING(pl.DimensionBlockH)
                             WHEN CAST(CEILING(pl.DimensionBlockH) AS INT) % 2 = 0 THEN
                                  CEILING(pl.DimensionBlockH)
                             ELSE CEILING(pl.DimensionBlockH) + 1
                         END                   AS BaseDimSales,
                         CASE
                             WHEN pl.GlassThicknessMM < 4.7 THEN
                                  CEILING(pl.DimensionBlockV)
                             WHEN CAST(CEILING(pl.DimensionBlockV) AS INT) % 2 = 0 THEN
                                  CEILING(pl.DimensionBlockV)
                             ELSE CEILING(pl.DimensionBlockV) + 1
                         END                   AS LeftDimSales
                     FROM
                         ClearPathReporting.ProdLites                     pl WITH (NOLOCK)
                        INNER JOIN ClearPath.ProductionLine pd WITH (NOLOCK)
                         ON  pd.Code = pl.ProductionLineCode
                         LEFT OUTER JOIN ClearPathReporting.ConsumedStoce cs WITH (NOLOCK)
                         ON  cs.StoceConsumptionID = pl.StoceConsumptionID
			             LEFT OUTER JOIN ClearPath.vw_RejectReasonMapping rm
			             ON rm.LineTypeCode = pl.LineTypeCode AND
                         rm.OperationCode = pl.LineOperationCode AND
                         rm.RejectDescription = pl.RejectReasonDescription
                     WHERE
                        pl.ReportDate BETWEEN @StartDate AND @EndDate AND
                         ISNULL(pl.RejectReasonBenchmarkDescription, '') NOT IN ( '', 'LASTLITE' ) 
                 ) A
            GROUP BY
                 A.ReportDate,
                 A.ProductionLineCode,
                 A.CutProductionLineCode,
                        A.GroupDescription,
                 A.E1WorkCenterID,
                 A.Shift,
                 A.Crew,
                 A.OriginTypeCode,
                 A.RejectReasonDescription,
                 A.RejectReasonBenchmarkDescription,
	             A.UpstreamType_Code";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public RejectedLites GetRejectedLites(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new RejectedLites();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetRejectedLites();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                cmd.CommandTimeout = 60;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new RejectedLite();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To(reader["ReportDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To(reader["ProductionLineCode"], string.Empty).Trim();
                        lite.GroupDescription = CastDbNull.To(reader["GroupDescription"], string.Empty).Trim();
                        lite.CutProductionLineCode = CastDbNull.To(reader["CutProductionLineCode"], string.Empty).Trim();
                        lite.WorkCenterId = CastDbNull.To(reader["E1WorkCenterID"], string.Empty).Trim();
                        lite.Shift = CastDbNull.To(reader["Shift"], 0);
                        lite.Crew = CastDbNull.To(reader["Crew"], string.Empty).Trim();
                        lite.OriginTypeCode = CastDbNull.To(reader["OriginTypeCode"], string.Empty).Trim();
                        lite.RejectReasonDescription = CastDbNull.To(reader["RejectReasonDescription"], string.Empty).Trim();
                        lite.RejectReasonBenchmarkDescription = CastDbNull.To(reader["RejectReasonBenchmarkDescription"], string.Empty).Trim();
                        lite.UpstreamTypeCode = CastDbNull.To(reader["UpstreamType_Code"], string.Empty).Trim();
                        lite.Lites = CastDbNull.To(reader["Lites"], 0);
                        lite.Sqft = CastDbNull.To<decimal>(reader["Sqft"], 0);
                        lite.ActualSqft = CastDbNull.To<decimal>(reader["ActualSqft"], 0);
                        lite.BillableSqft = CastDbNull.To<decimal>(reader["BillableSqft"], 0);
                        lite.Tons = CastDbNull.To<decimal>(reader["Tons"], 0);
                        lites.Add(lite);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
         * Produced
         *******************************************************************************/
        public SqlCommand GetProductionLines()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                Code,
                Description,
                E1WorkCenterID,
                E1LineCode,
                GroupDescription
              FROM
                ClearPath.ProductionLine";
            return cmd;
        }

        public ProductionLines GetProductionLines(SqlConnection cn, Plant plant)
        {
            var lines = new ProductionLines();
            try
            {
                var cmd = GetProductionLines();
                cmd.Connection = cn;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new ProductionLine();
                        line.ProductionLineCode = CastDbNull.To(reader["Code"], string.Empty).Trim();
                        line.WorkCenterId = CastDbNull.To(reader["E1WorkCenterID"], string.Empty).Trim();
                        line.Description = CastDbNull.To(reader["Description"], string.Empty).Trim();
                        line.LineCode = CastDbNull.To(reader["E1LineCode"], string.Empty).Trim();
                        line.GroupDescription = CastDbNull.To(reader["GroupDescription"], string.Empty).Trim();
                        lines.Add(line);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return lines;
        }

        /*********************************************************************************
         * Shift Summary
         *******************************************************************************/
        public ShiftAggregate GetShiftSummary(Plant plant, SqlConnection cn, string dateCode, DateTime startDate, DateTime endDate)
        {
            var agg = new ShiftAggregate();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                _logger.Trace($"Consumed Sheets {plant.PlantCode} {DateTime.Now}");
                var consumedSheetsAll = GetConsumedSheets(cn, plant, dateCode, startDate, endDate);
                _logger.Trace($"Consumed Offal {plant.PlantCode} {DateTime.Now}");
                var consumedOffalAll = GetConsumedOffal(cn, plant, dateCode, startDate, endDate);
                _logger.Trace($"Produced Lites {plant.PlantCode} {DateTime.Now}");
                var producedLitesAll = GetProducedLites(cn, plant, dateCode, startDate, endDate);
                _logger.Trace($"Rejected Lites {plant.PlantCode} {DateTime.Now}");
                var rejectedLitesAll = GetRejectedLites(cn, plant, dateCode, startDate, endDate);
                _logger.Trace($"Done {DateTime.Now}");
                agg.PlantCode = plant.PlantCode;
                agg.PlantNum = plant.PlantNum;
                agg.StartDate = startDate;
                agg.EndDate = endDate;

                if (consumedSheetsAll.Any())
                {
                    agg.StartDate = consumedSheetsAll.Min(x => x.ReportDate);
                    agg.EndDate = consumedSheetsAll.Max(x => x.ReportDate);

                    // Consumed
                    agg.ConsumedSheets = consumedSheetsAll.Sum(x => x.Sheets);
                    agg.GlassTypes = consumedSheetsAll.Select(x => x.GlassTypeCode).Distinct().Count();
                    agg.ConsumedSqft = consumedSheetsAll.Sum(x => x.ActualSqft);
                    agg.ConsumedActualSqft = consumedSheetsAll.Sum(x => x.ActualSqft);
                    agg.ConsumedBillableSqft = consumedSheetsAll.Sum(x => x.ActualSqft);
                    agg.ConsumedTons = consumedSheetsAll.Sum(x => x.Tons);
                    agg.ConsumedOffalLites = consumedOffalAll.Sum(x => x.Lites);
                    agg.ConsumedOffalSqft = consumedOffalAll.Sum(x => x.ActualSqft);
                    agg.ConsumedOffalActualSqft = consumedOffalAll.Sum(x => x.ActualSqft);
                    agg.ConsumedOffalBillableSqft = consumedOffalAll.Sum(x => x.ActualSqft);
                    agg.ConsumedOffalTons = consumedOffalAll.Sum(x => x.Tons);

                    // Gross Cut
                    agg.GrossCutProducedLites =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono")).Sum(x => x.Lites);
                    agg.GrossCutProducedSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono")).Sum(x => x.Sqft);
                    agg.GrossCutProducedActualSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono")).Sum(x => x.ActualSqft);
                    agg.GrossCutProducedBillableSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono")).Sum(x => x.BillableSqft);
                    agg.GrossCutProducedTons =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono")).Sum(x => x.Tons);

                    // Low Pro
                    agg.LoProCutLites =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                            .Sum(x => x.Lites);
                    agg.LoProCutSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                            .Sum(x => x.Sqft);
                    agg.LoProCutActualSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                            .Sum(x => x.ActualSqft);
                    agg.LoProCutBillableSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                            .Sum(x => x.BillableSqft);
                    agg.LoProCutTons =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                            .Sum(x => x.Tons);

                    // Custom
                    agg.CustomCutLites =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "C")
                            .Sum(x => x.Lites);
                    agg.CustomCutSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "C")
                            .Sum(x => x.Sqft);
                    agg.CustomCutActualSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "C")
                            .Sum(x => x.ActualSqft);
                    agg.CustomCutBillableSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "C")
                            .Sum(x => x.BillableSqft);
                    agg.CustomCutTons =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "C")
                            .Sum(x => x.Tons);

                    // Gross Rejected
                    agg.GrossCutRejectedLites =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVCutMono") ||
                                x.RejectReasonBenchmarkDescription == "CUTTING").Sum(x => x.Lites);
                    agg.GrossCutRejectedSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVCutMono") ||
                                x.RejectReasonBenchmarkDescription == "CUTTING").Sum(x => x.Sqft);
                    agg.GrossCutRejectedActualSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVCutMono") ||
                                x.RejectReasonBenchmarkDescription == "CUTTING").Sum(x => x.ActualSqft);
                    agg.GrossCutRejectedBillableSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVCutMono") ||
                                x.RejectReasonBenchmarkDescription == "CUTTING").Sum(x => x.BillableSqft);
                    agg.GrossCutRejectedTons =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVCutMono") ||
                                x.RejectReasonBenchmarkDescription == "CUTTING").Sum(x => x.Tons);

                    // Vendor Rejected
                    agg.VendorCutRejectedLites =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVCutMono") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVCutMono") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.Lites);
                    agg.VendorCutRejectedSqft =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVCutMono") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVCutMono") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.Sqft);
                    agg.VendorCutRejectedActualSqft =
                        rejectedLitesAll.Where(x =>
                                    x.GroupDescription.StartsWith("LVCutMono") && x.UpstreamTypeCode == "V" ||
                                    x.GroupDescription.StartsWith("LVCutMono") &&
                                    x.RejectReasonBenchmarkDescription == "VENDOR")
                                .Sum(x => x.ActualSqft);
                    agg.VendorCutRejectedBillableSqft =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVCutMono") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVCutMono") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.BillableSqft);
                    agg.VendorCutRejectedTons =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVCutMono") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVCutMono") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.Tons);

                    // Internal Reject
                    agg.InternalCutRejectedLites =
                        rejectedLitesAll.Where(
                            x =>
                                (x.GroupDescription.StartsWith("LVCutMono") ||
                                 x.RejectReasonBenchmarkDescription == "CUTTING") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.Lites);
                    agg.InternalCutRejectedSqft =
                        rejectedLitesAll.Where(
                            x =>
                                (x.GroupDescription.StartsWith("LVCutMono") ||
                                 x.RejectReasonBenchmarkDescription == "CUTTING") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.Sqft);
                    agg.InternalCutRejectedActualSqft =
                        rejectedLitesAll.Where(
                            x =>
                                (x.GroupDescription.StartsWith("LVCutMono") ||
                                 x.RejectReasonBenchmarkDescription == "CUTTING") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.ActualSqft);
                    agg.InternalCutRejectedBillableSqft =
                        rejectedLitesAll.Where(
                            x =>
                                (x.GroupDescription.StartsWith("LVCutMono") ||
                                 x.RejectReasonBenchmarkDescription == "CUTTING") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.BillableSqft);
                    agg.InternalCutRejectedTons =
                        rejectedLitesAll.Where(
                            x =>
                                (x.GroupDescription.StartsWith("LVCutMono") ||
                                 x.RejectReasonBenchmarkDescription == "CUTTING") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.Tons);

                    // Net
                    agg.NetCutProducedLites = agg.GrossCutProducedLites - agg.GrossCutRejectedLites;
                    agg.NetCutProducedSqft = agg.GrossCutProducedSqft - agg.GrossCutRejectedSqft;
                    agg.NetCutProducedActualSqft = agg.GrossCutProducedActualSqft - agg.GrossCutRejectedActualSqft;
                    agg.NetCutProducedBillableSqft = agg.GrossCutProducedBillableSqft - agg.GrossCutRejectedBillableSqft;
                    agg.NetCutProducedTons = agg.GrossCutProducedTons - agg.GrossCutRejectedTons;

                    // Yield
                    agg.CuttingYield = Math.Round(agg.GrossCutProducedSqft / agg.ConsumedSqft, 5);
                    agg.CuttingActualYield = Math.Round(agg.GrossCutProducedActualSqft / agg.ConsumedSqft, 5);
                    agg.CuttingBillableYield = Math.Round(agg.GrossCutProducedBillableSqft / agg.ConsumedSqft, 5);
                    agg.NetYield = Math.Round(agg.NetCutProducedSqft / agg.ConsumedSqft, 5);
                    agg.NetActualYield = Math.Round(agg.NetCutProducedActualSqft / agg.ConsumedActualSqft, 5);
                    agg.NetBillableYield = Math.Round(agg.NetCutProducedBillableSqft / agg.ConsumedBillableSqft, 5);

                    // Hours
                    agg.CuttingHours =
                        (decimal) Math.Round(consumedSheetsAll.GroupBy(x => new {x.ReportDate, x.Shift})
                            .Select(
                                lg =>
                                    new
                                    {
                                        Date = lg.Key,
                                        Hours =
                                            (lg.Max(x => x.MaxWrittenDate) - lg.Min(x => x.MinWrittenDate)).TotalHours
                                    })
                            .Sum(a => a.Hours), 2);
                }

                // TEMPERING
                if (producedLitesAll.Any(x => x.GroupDescription.StartsWith("LVFurnace")))
                {

                    // Gross
                    agg.GrossTempProducedLites =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace")).Sum(x => x.Lites);
                    agg.GrossTempProducedSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace")).Sum(x => x.Sqft);
                    agg.GrossTempProducedActualSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace")).Sum(x => x.ActualSqft);
                    agg.GrossTempProducedBillableSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace")).Sum(x => x.BillableSqft);
                    agg.GrossTempProducedTons =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace")).Sum(x => x.Tons);

                    // Low Pro
                    agg.LoProTempLites =
                        producedLitesAll.Where(
                                x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "LP")
                            .Sum(x => x.Lites);
                    agg.LoProTempSqft =
                        producedLitesAll.Where(
                                x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "LP")
                            .Sum(x => x.Sqft);
                    agg.LoProTempActualSqft =
                        producedLitesAll.Where(
                                x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "LP")
                            .Sum(x => x.ActualSqft);
                    agg.LoProTempBillableSqft =
                        producedLitesAll.Where(
                                x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "LP")
                            .Sum(x => x.BillableSqft);
                    agg.LoProTempTons =
                        producedLitesAll.Where(
                            x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "LP").Sum(x => x.Tons);

                    // Custom
                    agg.CustomTempLites =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "C")
                            .Sum(x => x.Lites);
                    agg.CustomTempSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "C")
                            .Sum(x => x.Sqft);
                    agg.CustomTempActualSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "C")
                            .Sum(x => x.ActualSqft);
                    agg.CustomTempBillableSqft =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "C")
                            .Sum(x => x.BillableSqft);
                    agg.CustomTempTons =
                        producedLitesAll.Where(x => x.GroupDescription.StartsWith("LVFurnace") && x.OriginTypeCode == "C")
                            .Sum(x => x.Tons);

                    // Gross Reject
                    agg.GrossTempRejectedLites =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.Lites);
                    agg.GrossTempRejectedSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.Sqft);
                    agg.GrossTempRejectedActualSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.ActualSqft);
                    agg.GrossTempRejectedBillableSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.BillableSqft);
                    agg.GrossTempRejectedTons =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.Tons);

                    // Vendor Reject
                    agg.VendorCutRejectedLites =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVFurnace") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.Lites);
                    agg.VendorCutRejectedSqft =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVFurnace") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.Sqft);
                    agg.VendorCutRejectedActualSqft =
                        rejectedLitesAll.Where(x =>
                                    x.GroupDescription.StartsWith("LVFurnace") && x.UpstreamTypeCode == "V" ||
                                    x.GroupDescription.StartsWith("LVFurnace") &&
                                    x.RejectReasonBenchmarkDescription == "VENDOR")
                                .Sum(x => x.ActualSqft);
                    agg.VendorCutRejectedBillableSqft =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVFurnace") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.BillableSqft);
                    agg.VendorCutRejectedTons =
                        rejectedLitesAll.Where(x =>
                                x.GroupDescription.StartsWith("LVFurnace") && x.UpstreamTypeCode == "V" ||
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.RejectReasonBenchmarkDescription == "VENDOR")
                            .Sum(x => x.Tons);

                    // Internal Reject
                    agg.InternalTempRejectedLites =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.Lites);
                    agg.InternalTempRejectedSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.Sqft);
                    agg.InternalTempRejectedActualSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.ActualSqft);
                    agg.InternalTempRejectedBillableSqft =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.BillableSqft);
                    agg.InternalTempRejectedTons =
                        rejectedLitesAll.Where(
                            x =>
                                x.GroupDescription.StartsWith("LVFurnace") &&
                                x.UpstreamTypeCode != "V").Sum(x => x.Tons);

                    // Net
                    agg.NetTempProducedLites = agg.GrossTempProducedLites - agg.GrossTempRejectedLites;
                    agg.NetTempProducedSqft = agg.GrossTempProducedSqft - agg.GrossTempRejectedSqft;
                    agg.NetTempProducedActualSqft = agg.GrossTempProducedActualSqft - agg.GrossTempRejectedActualSqft;
                    agg.NetTempProducedBillableSqft = agg.GrossTempProducedBillableSqft - agg.GrossTempRejectedBillableSqft;
                    agg.NetTempProducedTons = agg.GrossTempProducedTons - agg.GrossTempRejectedTons;

                    // Yield
                    agg.TempYield = Math.Round(agg.NetTempProducedSqft / agg.GrossTempProducedSqft, 5);
                    agg.TempActualYield = Math.Round(agg.NetTempProducedActualSqft / agg.GrossTempProducedActualSqft, 5);
                    agg.TempBillableYield = Math.Round(agg.NetTempProducedBillableSqft / agg.GrossTempProducedBillableSqft, 5);
                    
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }

            return agg;
        }


        /*********************************************************************************
         * Production Line Cut Aggregate
         *******************************************************************************/
        public CutAggregates GetCutSummary(Plant plant, SqlConnection cn, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new CutAggregates();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                _logger.Info($"Consumed Sheets {plant.PlantCode} {DateTime.Now}");
                var consumedSheetsAll = GetConsumedSheets(cn, plant, dateCode, startDate, endDate);
                _logger.Info($"Consumed Offal {plant.PlantCode} {DateTime.Now}");
                var consumedOffalAll = GetConsumedOffal(cn, plant, dateCode, startDate, endDate);
                _logger.Info($"Produced Lites {plant.PlantCode} {DateTime.Now}");
                var producedLitesAll = GetProducedLites(cn, plant, dateCode, startDate, endDate);
                _logger.Info($"Rejected Lites {plant.PlantCode} {DateTime.Now}");
                var rejectedLitesAll = GetRejectedLites(cn, plant, dateCode, startDate, endDate);
                _logger.Info($"Done {DateTime.Now}");

                var lines = GetProductionLines(cn, plant).Where(x => x.GroupDescription.StartsWith("LVCutMono")).ToList();
                switch (plant.PlantCode)
                {
                    case "TWCG":
                        lines.RemoveAll(x => x.ProductionLineCode == "CUTLV4");
                        break;
                    case "GTCG":
                        lines.RemoveAll(x => x.ProductionLineCode == "CUTLV2");
                        break;
                    case "CGCG":
                        lines.RemoveAll(x => x.ProductionLineCode == "CUTLV3");
                        break;
                    case "LLCG":
                        lines.RemoveAll(x => x.ProductionLineCode == "CUTLV2");
                        break;
                }

                foreach (var line in lines)
                {
                    var agg = new CutAggregate();
                    agg.PlantCode = plant.PlantCode;
                    agg.PlantNum = plant.PlantNum;
                    agg.StartDate = startDate;
                    agg.EndDate = endDate;
                    agg.ProductionLineCode = line.ProductionLineCode;
                    agg.WorkCenterId = line.WorkCenterId;
                    agg.ProductionLineNum = line.LineCode;
                    agg.ProductionLineGroupDesc = line.GroupDescription;
                    agg.ProductionLineDesc = line.Description;

                    var consumedSheetsLine = consumedSheetsAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var consumedOffalLine = consumedOffalAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var producedLitesLine = producedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var rejectedLitesLine = rejectedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode || x.CutProductionLineCode == line.ProductionLineCode && x.RejectReasonBenchmarkDescription == "CUTTING").ToList();

                    if (consumedSheetsLine.Any())
                    {
                        agg.StartDate = consumedSheetsLine.Min(x => x.ReportDate);
                        agg.EndDate = consumedSheetsLine.Max(x => x.ReportDate);

                        // Consumed
                        agg.ConsumedSheets = consumedSheetsLine.Sum(x => x.Sheets);
                        agg.GlassTypes = consumedSheetsLine.Select(x => x.GlassTypeCode).Distinct().Count();
                        agg.ConsumedSqft = consumedSheetsLine.Sum(x => x.ActualSqft);
                        agg.ConsumedActualSqft = consumedSheetsLine.Sum(x => x.ActualSqft);
                        agg.ConsumedBillableSqft = consumedSheetsLine.Sum(x => x.ActualSqft);
                        agg.ConsumedTons = consumedSheetsLine.Sum(x => x.Tons);
                        agg.ConsumedOffalLites = consumedOffalLine.Sum(x => x.Lites);
                        agg.ConsumedOffalSqft = consumedOffalLine.Sum(x => x.ActualSqft);
                        agg.ConsumedOffalActualSqft = consumedOffalLine.Sum(x => x.ActualSqft);
                        agg.ConsumedOffalBillableSqft = consumedOffalLine.Sum(x => x.ActualSqft);
                        agg.ConsumedOffalTons = consumedOffalLine.Sum(x => x.Tons);

                        // Produced
                        agg.GrossCutProducedLites =
                            producedLitesLine.Sum(x => x.Lites);
                        agg.GrossCutProducedSqft =
                            producedLitesLine.Sum(x => x.Sqft);
                        agg.GrossCutProducedActualSqft =
                            producedLitesLine.Sum(x => x.ActualSqft);
                        agg.GrossCutProducedBillableSqft =
                            producedLitesLine.Sum(x => x.BillableSqft);
                        agg.GrossCutProducedTons =
                            producedLitesLine.Sum(x => x.Tons);

                        // Low Pro
                        agg.LoProCutLites =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.Lites);
                        agg.LoProCutSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.ActualSqft);
                        agg.LoProCutActualSqft =
                            producedLitesLine.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                                .Sum(x => x.ActualSqft);
                        agg.LoProCutBillableSqft =
                            producedLitesLine.Where(x => x.GroupDescription.StartsWith("LVCutMono") && x.OriginTypeCode == "LP")
                                .Sum(x => x.BillableSqft);
                        agg.LoProCutTons =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.Tons);

                        // Rare
                        agg.RareCutLites =
                            producedLitesLine.Sum(x => x.RareLites);
                        agg.RareCutSqft =
                            producedLitesLine.Sum(x => x.RareSqft);
                        agg.RareCutActualSqft =
                            producedLitesLine.Sum(x => x.RareActualSqft);
                        agg.RareCutBillableSqft =
                            producedLitesLine.Sum(x => x.RareBillableSqft);
                        agg.RareCutTons =
                            producedLitesLine.Sum(x => x.RareTons);

                        // Custom
                        agg.CustomCutLites =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.Lites);
                        agg.CustomCutSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.Sqft);
                        agg.CustomCutActualSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.ActualSqft);
                        agg.CustomCutBillableSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.BillableSqft);
                        agg.CustomCutTons =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.Tons);

                        // Gross
                        agg.GrossCutRejectedLites =
                            rejectedLitesLine.Sum(x => x.Lites);
                        agg.GrossCutRejectedSqft =
                            rejectedLitesLine.Sum(x => x.Sqft);
                        agg.GrossCutRejectedActualSqft =
                            rejectedLitesLine.Sum(x => x.ActualSqft);
                        agg.GrossCutRejectedBillableSqft =
                            rejectedLitesLine.Sum(x => x.BillableSqft);
                        agg.GrossCutRejectedTons =
                            rejectedLitesLine.Sum(x => x.Tons);

                        // Vendor Reject
                        agg.VendorCutRejectedLites =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.Lites);
                        agg.VendorCutRejectedSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.Sqft);
                        agg.VendorCutRejectedActualSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.ActualSqft);
                        agg.VendorCutRejectedBillableSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.BillableSqft);
                        agg.VendorCutRejectedTons =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.Tons);

                        // Internal Reject
                        agg.InternalCutRejectedLites =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.Lites);
                        agg.InternalCutRejectedSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.Sqft);
                        agg.InternalCutRejectedActualSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.ActualSqft);
                        agg.InternalCutRejectedBillableSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.BillableSqft);
                        agg.InternalCutRejectedTons =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.Tons);

                        // Net
                        agg.NetCutProducedLites = agg.GrossCutProducedLites - agg.GrossCutRejectedLites;
                        agg.NetCutProducedSqft = agg.GrossCutProducedSqft - agg.GrossCutRejectedSqft;
                        agg.NetCutProducedActualSqft = agg.GrossCutProducedActualSqft - agg.GrossCutRejectedActualSqft;
                        agg.NetCutProducedBillableSqft = agg.GrossCutProducedBillableSqft - agg.GrossCutRejectedBillableSqft;
                        agg.NetCutProducedTons = agg.GrossCutProducedTons - agg.GrossCutRejectedTons;

                        // Yield
                        agg.CuttingYield = Math.Round(agg.GrossCutProducedSqft / agg.ConsumedSqft, 5);
                        agg.CuttingActualYield = Math.Round(agg.GrossCutProducedActualSqft / agg.ConsumedActualSqft, 5);
                        agg.CuttingBillableYield = Math.Round(agg.GrossCutProducedBillableSqft / agg.ConsumedBillableSqft, 5);
                        agg.NetYield = Math.Round(agg.NetCutProducedSqft / agg.ConsumedSqft, 5);
                        agg.NetActualYield = Math.Round(agg.NetCutProducedActualSqft / agg.ConsumedActualSqft, 5);
                        agg.NetBillableYield = Math.Round(agg.NetCutProducedBillableSqft / agg.ConsumedBillableSqft, 5);

                        // Hours
                        agg.CuttingHours =
                            (decimal) Math.Round(consumedSheetsLine.GroupBy(x => new {x.ReportDate, x.Shift})
                                .Select(
                                    lg =>
                                        new
                                        {
                                            Date = lg.Key,
                                            Hours =
                                                (lg.Max(x => x.MaxWrittenDate) - lg.Min(x => x.MinWrittenDate))
                                                .TotalHours
                                        })
                                .Sum(a => a.Hours), 2);
                    }

                    aggs.Add(agg);
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }

            return aggs;
        }


        /*********************************************************************************
          * Production Line Cut Aggregate
          *******************************************************************************/
        public TemperAggregates GetTemperSummary(Plant plant, SqlConnection cn, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new TemperAggregates();
            try
            {
                if (!string.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var producedLitesAll = GetProducedLites(cn, plant, dateCode, startDate, endDate);
                var rejectedLitesAll = GetRejectedLites(cn, plant, dateCode, startDate, endDate);

                var lines = GetProductionLines(cn, plant).Where(x => x.GroupDescription.StartsWith("LVFurnace"));
                foreach (var line in lines)
                {
                    var agg = new TemperAggregate();
                    agg.PlantCode = plant.PlantCode;
                    agg.PlantNum = plant.PlantNum;
                    agg.StartDate = startDate;
                    agg.EndDate = endDate;
                    agg.ProductionLineCode = line.ProductionLineCode;
                    agg.WorkCenterId = line.WorkCenterId;
                    agg.ProductionLineNum = line.LineCode;
                    agg.ProductionLineGroupDesc = line.GroupDescription;
                    agg.ProductionLineDesc = line.Description;

                    var producedLitesLine =
                        producedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var rejectedLitesLine =
                        rejectedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode &&
                                                    x.RejectReasonBenchmarkDescription != "CUTTING").ToList();

                    if (producedLitesLine.Any())
                    {
                        agg.StartDate = producedLitesLine.Min(x => x.ReportDate);
                        agg.EndDate = producedLitesLine.Max(x => x.ReportDate);

                        // Gross Produced
                        agg.GrossTempProducedLites =
                            producedLitesLine.Sum(x => x.Lites);
                        agg.GrossTempProducedSqft =
                            producedLitesLine.Sum(x => x.Sqft);
                        agg.GrossTempProducedActualSqft =
                            producedLitesLine.Sum(x => x.ActualSqft);
                        agg.GrossTempProducedBillableSqft =
                            producedLitesLine.Sum(x => x.BillableSqft);
                        agg.GrossTempProducedTons =
                            producedLitesLine.Sum(x => x.Tons);

                        // Low Pro
                        agg.LoProTempLites =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.Lites);
                        agg.LoProTempSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.Sqft);
                        agg.LoProTempActualSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.ActualSqft);
                        agg.LoProTempBillableSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.BillableSqft);
                        agg.LoProTempTons =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "LP")
                                .Sum(x => x.Tons);

                        // Custom
                        agg.CustomTempLites =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.Lites);
                        agg.CustomTempSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.Sqft);
                        agg.CustomTempActualSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.ActualSqft);
                        agg.CustomTempBillableSqft =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.BillableSqft);
                        agg.CustomTempTons =
                            producedLitesLine.Where(
                                    x => x.OriginTypeCode == "C")
                                .Sum(x => x.Tons);

                        // Gross Rejected
                        agg.GrossTempRejectedLites =
                            rejectedLitesLine.Where(
                                x => x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.Lites);
                        agg.GrossTempRejectedSqft =
                            rejectedLitesLine.Where(
                                x => x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.Sqft);
                        agg.GrossTempRejectedActualSqft =
                            rejectedLitesLine.Where(
                                x => x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.ActualSqft);
                        agg.GrossTempRejectedBillableSqft =
                            rejectedLitesLine.Where(
                                x => x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.BillableSqft);
                        agg.GrossTempRejectedTons =
                            rejectedLitesLine.Where(
                                x => x.RejectReasonBenchmarkDescription != "CUTTING").Sum(x => x.Tons);

                        // Vendor Rejected
                        agg.VendorTempRejectedLites =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.Lites);
                        agg.VendorTempRejectedSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.Sqft);
                        agg.VendorTempRejectedActualSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.ActualSqft);
                        agg.VendorTempRejectedBillableSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.BillableSqft);
                        agg.VendorTempRejectedTons =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode == "V" || x.RejectReasonBenchmarkDescription == "VENDOR").Sum(x => x.Tons);

                        // Internal Rejected
                        agg.InternalTempRejectedLites =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.Lites);
                        agg.InternalTempRejectedSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.Sqft);
                        agg.InternalTempRejectedActualSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.ActualSqft);
                        agg.InternalTempRejectedBillableSqft =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.BillableSqft);
                        agg.InternalTempRejectedTons =
                            rejectedLitesLine.Where(
                                x => x.UpstreamTypeCode != "V").Sum(x => x.Tons);

                        // Net Produced
                        agg.NetTempProducedLites = agg.GrossTempProducedLites - agg.GrossTempRejectedLites;
                        agg.NetTempProducedSqft = agg.GrossTempProducedSqft - agg.GrossTempRejectedSqft;
                        agg.NetTempProducedActualSqft = agg.GrossTempProducedActualSqft - agg.GrossTempRejectedActualSqft;
                        agg.NetTempProducedBillableSqft = agg.GrossTempProducedBillableSqft - agg.GrossTempRejectedBillableSqft;
                        agg.NetTempProducedTons = agg.GrossTempProducedTons - agg.GrossTempRejectedTons;

                        // Yield
                        agg.TempYield = Math.Round(agg.NetTempProducedSqft / agg.GrossTempProducedSqft, 5);
                        agg.TempActualYield = Math.Round(agg.NetTempProducedActualSqft / agg.GrossTempProducedActualSqft, 5);
                        agg.TempBillableYield = Math.Round(agg.NetTempProducedBillableSqft / agg.GrossTempProducedBillableSqft, 5);

                        // Hours
                        agg.TemperingHours =
                            (decimal) Math.Round(producedLitesLine.GroupBy(x => new {x.ReportDate, x.Shift})
                                //(decimal)Math.Round(producedLitesLine.GroupBy(x => x.ReportDate)
                                .Select(
                                    lg =>
                                        new
                                        {
                                            Date = lg.Key,
                                            Hours =
                                                (lg.Max(x => x.MaxWrittenDate) - lg.Min(x => x.MinWrittenDate))
                                                .TotalHours
                                        })
                                .Sum(a => a.Hours), 2);
                    }

                    aggs.Add(agg);
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }

            return aggs;
        }
    }
}