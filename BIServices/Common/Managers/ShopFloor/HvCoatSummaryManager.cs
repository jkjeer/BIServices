using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using BIServices.Common.Utilities;
using BIServices.CtoSvc;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public class HvCoatSummaryManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public HvCoatSummaryManager()
        {
        }

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
                ProductionLineCode,
                LineID,
                Shift,
                Crew,
	            MIN(TransactionDate) AS MinTransactionDate,
	            MIN(ProductionDate) AS MinProductionDate,
                MAX(TransactionDate) AS MaxTransactionDate,
                MAX(ProductionDate) AS MaxProductionDate,
                SUM(Quantity + QuantityCanceled) AS Lites,
                SUM((Quantity + QuantityCanceled) * SqftActual) AS ActualSqft,               
                SUM((Quantity + QuantityCanceled) * SqftBillable) AS BillableSqft,
                SUM((Quantity + QuantityCanceled) * Thickness * SqftActual * .000249) AS Tons
            FROM
                dbo.HVWOIssues WITH (NOLOCK)
            WHERE
                ProductionDate BETWEEN @StartDate AND @EndDate AND
                ISNULL(RejectCode, '') = '' AND
	            ISNULL(DoNotUpload, 0) = 0
            GROUP BY
                ProductionLineCode,
                LineID,
                Shift,
                Crew";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public TransactedLiteAggregates GetConsumedSheets(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var sheets = new TransactedLiteAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetConsumedSheets();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var sheet = new TransactedLiteAggregate();
                        sheet.PlantCode = plant.PlantCode;
                        sheet.PlantNum = plant.PlantNum;
                        sheet.ReportDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        sheet.MinTransactionDate = CastDbNull.To<DateTime>(reader["MinTransactionDate"], DateTime.MinValue);
                        sheet.MinProductionDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        sheet.MaxTransactionDate = CastDbNull.To<DateTime>(reader["MaxTransactionDate"], DateTime.MinValue);
                        sheet.MaxProductionDate = CastDbNull.To<DateTime>(reader["MaxProductionDate"], DateTime.MinValue);
                        sheet.ProductionLineCode = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
                        //sheet.WorkCenterId = CastDbNull.To<String>(reader["E1WorkCenterID"], String.Empty).Trim();
                        sheet.Shift = Convert.ToInt16(CastDbNull.To<String>(reader["Shift"], String.Empty).Trim());
                        sheet.Crew = CastDbNull.To<String>(reader["Crew"], String.Empty).Trim();
                        sheet.Lites = Convert.ToInt32(CastDbNull.To<decimal>(reader["Lites"], 0));
                        sheet.ActualSqft = CastDbNull.To<decimal>(reader["ActualSqft"], 0);
                        sheet.BillableSqft = CastDbNull.To<decimal>(reader["BillableSqft"], 0);
                        sheet.Tons = CastDbNull.To<decimal>(reader["Tons"], 0);
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
         * Produced
         *******************************************************************************/
        //public SqlCommand GetProducedLitesOld()
        //{
        //    var cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = @"
        //    SELECT
        //        HC.ProductionLineCode,
        //        HC.LineID,
        //        HC.Shift,
        //        HC.Crew,
        //        CR.Thickness,
        //        CR.Lite1_Color AS Color,
        //        CR.Lite1_HeatTreatment AS HeatTreatment,
        //        CR.Lite1_1stSrfCoat AS FirstSrfCoat,
        //        CR.Lite1_2ndSrfCoat AS SecondSrfCoat,
        //        HC.RejectType As TransRejectType,
        //        HC.RejectCode As TransRejectCode,            
        //        MIN(HC.TransactionDate) AS MinTransactionDate,
	       //     MIN(HC.ProductionDate) AS MinProductionDate,
        //        MAX(HC.TransactionDate) AS MaxTransactionDate,
        //        MAX(HC.ProductionDate) AS MaxProductionDate,
        //        SUM(HC.Quantity - HC.PartialQty) AS Lites,
        //        SUM((HC.Quantity - HC.PartialQty) * HC.SqftActual) AS ActualSqft,               
        //        SUM((HC.Quantity - HC.PartialQty) * HC.SqftBillable) AS BillableSqft,
        //        SUM((HC.Quantity - HC.PartialQty) * CR.Thickness * HC.SqftActual * .000249) AS Tons
        //    FROM
        //        dbo.HVCompletions HC WITH (NOLOCK)
	       //     INNER JOIN dbo.HVCCrateInfoE1 CR WITH (NOLOCK)
	       //         ON  HC.SerialNumber = CR.SerialNumber AND
			     //       HC.FirmSchedule = CR.ScheduleNumber
        //    WHERE
        //        HC.ProductionDate BETWEEN @StartDate AND @EndDate AND
	       //     ISNULL(HC.DoNotUpload, 0) = 0 AND
        //        (
        //        ISNULL(HC.RejectCode, '') IN ('','11','12') OR
        //        ISNULL(HC.RejectType, '') = 'E'
        //        ) 
        //    GROUP BY
        //        HC.ProductionLineCode,
        //        HC.RejectCode,
        //        HC.RejectType,
        //        HC.LineID,
        //        HC.Shift,
        //        HC.Crew,
        //        CR.Thickness,
        //        CR.Lite1_Color,
        //        CR.Lite1_HeatTreatment,
        //        CR.Lite1_1stSrfCoat,
        //        CR.Lite1_2ndSrfCoat";
        //    cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
        //    cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
        //    return cmd;
        //}

        public SqlCommand GetProducedLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                    HC.ProductionLineCode,
                    HC.LineID,
                    HC.Shift,
                    HC.Crew,
                    CR.Thickness,
                    CR.Lite1_Color AS Color,
                    CR.Lite1_HeatTreatment AS HeatTreatment,
                    CR.Lite1_1stSrfCoat AS FirstSrfCoat,
                    CR.Lite1_2ndSrfCoat AS SecondSrfCoat,        
                    MIN(HC.TransactionDate) AS MinTransactionDate,
	                MIN(HC.ProductionDate) AS MinProductionDate,
                    MAX(HC.TransactionDate) AS MaxTransactionDate,
                    MAX(HC.ProductionDate) AS MaxProductionDate,
                    SUM(HC.Quantity - HC.PartialQty) AS Lites,
                    SUM((HC.Quantity - HC.PartialQty) * HC.SqftActual) AS ActualSqft,               
                    SUM((HC.Quantity - HC.PartialQty) * HC.SqftBillable) AS BillableSqft,
                    SUM((HC.Quantity - HC.PartialQty) * CR.Thickness * HC.SqftActual * .000249) AS Tons
                FROM
                    dbo.HVReceipts HC WITH (NOLOCK)
	                INNER JOIN dbo.HVCCrateInfoE1 CR WITH (NOLOCK)
	                    ON  HC.SerialNumber = CR.SerialNumber AND
			                HC.FirmSchedule = CR.ScheduleNumber
                WHERE
                    HC.ProductionDate BETWEEN @StartDate AND @EndDate
                GROUP BY
                    HC.ProductionLineCode,
                    HC.LineID,
                    HC.Shift,
                    HC.Crew,
                    CR.Thickness,
                    CR.Lite1_Color,
                    CR.Lite1_HeatTreatment,
                    CR.Lite1_1stSrfCoat,
                    CR.Lite1_2ndSrfCoat";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public TransactedLiteAggregates GetProducedLites(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new TransactedLiteAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetProducedLites();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new TransactedLiteAggregate();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MinTransactionDate = CastDbNull.To<DateTime>(reader["MinTransactionDate"], DateTime.MinValue);
                        lite.MinProductionDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MaxTransactionDate = CastDbNull.To<DateTime>(reader["MaxTransactionDate"], DateTime.MinValue);
                        lite.MaxProductionDate = CastDbNull.To<DateTime>(reader["MaxProductionDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
                        lite.Shift = Convert.ToInt16(CastDbNull.To<String>(reader["Shift"], String.Empty).Trim());
                        lite.Crew = CastDbNull.To<String>(reader["Crew"], String.Empty).Trim();

                        lite.Thickness = CastDbNull.To<decimal>(reader["Thickness"], 0);
                        lite.Color = CastDbNull.To<String>(reader["Color"], String.Empty).Trim();
                        lite.HeatTreatment = CastDbNull.To<String>(reader["HeatTreatment"], String.Empty).Trim();
                        lite.FirstSrfCoat = CastDbNull.To<String>(reader["FirstSrfCoat"], String.Empty).Trim();
                        lite.SecondSrfCoat = CastDbNull.To<String>(reader["SecondSrfCoat"], String.Empty).Trim();

                        //lite.TransRejectType = CastDbNull.To<String>(reader["TransRejectType"], String.Empty).Trim();
                        //lite.TransRejectCode = CastDbNull.To<String>(reader["TransRejectCode"], String.Empty).Trim();

                        lite.Lites = Convert.ToInt32(CastDbNull.To<decimal>(reader["Lites"], 0));
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
                throw new ApplicationException(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
         * Finished Rejects
         *******************************************************************************/
        public SqlCommand GetFinishedRejectedLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                HC.ProductionLineCode,
                HC.LineOperationCode,
                HC.Shift,
                HC.Crew,
                CR.Thickness,
                CR.Lite1_Color AS Color,
                CR.Lite1_HeatTreatment AS HeatTreatment,
                CR.Lite1_1stSrfCoat AS FirstSrfCoat,
                CR.Lite1_2ndSrfCoat AS SecondSrfCoat,
                HC.RejectType As TransRejectType,
                HC.RejectCode As TransRejectCode,
                RM.LineOperationRejectTypeCode AS LineMapRejectType,
                RM.RejectCode AS LineMapRejectCode,
                RM.RejectDescription AS LineMapRejectDesc,
                RM.BenchmarkCode As LineMapBenchmarkCode,
                RM.BenchmarkDescription AS LineMapBenchmarkDesc,
                MIN(HC.TransactionDate) AS MinTransactionDate,
                MIN(HC.ProductionDate) AS MinProductionDate,
                MAX(HC.TransactionDate) AS MaxTransactionDate,
                MAX(HC.ProductionDate) AS MaxProductionDate,
                SUM(HC.Quantity + HC.QuantityCanceled) AS Lites,
                SUM((HC.Quantity + HC.QuantityCanceled) * HC.SqftActual) AS ActualSqft,
                SUM((HC.Quantity + HC.QuantityCanceled) * HC.SqftBillable) AS BillableSqft,
                SUM((HC.Quantity + HC.QuantityCanceled) * CR.Thickness * HC.SqftActual * .000249) AS Tons
            FROM
                dbo.HVCompletions HC WITH (NOLOCK)
                INNER JOIN
                (
                    SELECT
                        PL.Code AS ProductionLineCode,
                        LT.Code AS LineTypeCode
                    FROM
                        ClearPath.LineType LT WITH (NOLOCK)
                        INNER JOIN ClearPath.LineArea LA WITH (NOLOCK)
                        ON  LT.LineTypeID = LA.LineTypeID
                        INNER JOIN ClearPath.LineOperation LO WITH (NOLOCK)
                        ON  LA.LineAreaID = LO.LineAreaID
                        INNER JOIN ClearPath.WorkflowPathItem WPI WITH (NOLOCK)
                        ON  LO.LineOperationID = WPI.LineOperationID
                        INNER JOIN ClearPath.WorkflowPath WP WITH (NOLOCK)
                        ON  WPI.WorkflowPathID = WP.WorkflowPathID AND
                            WP.Active = 1
                        INNER JOIN ClearPath.ProductionLine PL WITH (NOLOCK)
                        ON  WP.LineID = PL.LineID
                    GROUP BY
                        PL.Code,
                        LT.Code
                ) PL
                ON  PL.ProductionLineCode = HC.ProductionLineCode
                LEFT OUTER JOIN HVCCrateInfoE1 CR WITH (NOLOCK)
                ON  HC.SerialNumber = CR.SerialNumber AND
                    HC.FirmSchedule = CR.ScheduleNumber
                LEFT OUTER JOIN ClearPath.vw_RejectReasonMapping RM
                ON  HC.RejectCode = RM.RejectCode AND
                    HC.LineOperationCode = RM.OperationCode AND
                    PL.LineTypeCode = RM.LineTypeCode
            WHERE
	            HC.ProductionDate BETWEEN @StartDate AND @EndDate AND
                ISNULL(HC.DoNotUpload, 0) = 0 AND
                ISNULL(HC.RejectCode, '') NOT IN ('') 
                --ISNULL(HC.RejectCode, '') NOT IN ( '', '11', '12') AND
                --ISNULL(HC.RejectType, '') NOT IN ('E')
            GROUP BY
                HC.ProductionLineCode,
                HC.LineOperationCode,
                HC.Shift,
                HC.Crew,
                CR.Thickness,
                CR.Lite1_Color,
                CR.Lite1_HeatTreatment,
                CR.Lite1_1stSrfCoat,
                CR.Lite1_2ndSrfCoat,
                HC.RejectType,
                HC.RejectCode,
                RM.LineOperationRejectTypeCode,
                RM.RejectCode,
                RM.RejectDescription,
                RM.BenchmarkCode,
                RM.BenchmarkDescription";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public TransactedLiteAggregates GetFinishedRejectedLites(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new TransactedLiteAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetFinishedRejectedLites();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new TransactedLiteAggregate();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MinTransactionDate = CastDbNull.To<DateTime>(reader["MinTransactionDate"], DateTime.MinValue);
                        lite.MinProductionDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MaxTransactionDate = CastDbNull.To<DateTime>(reader["MaxTransactionDate"], DateTime.MinValue);
                        lite.MaxProductionDate = CastDbNull.To<DateTime>(reader["MaxProductionDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
                        lite.Shift = Convert.ToInt16(CastDbNull.To<String>(reader["Shift"], String.Empty).Trim());
                        lite.Crew = CastDbNull.To<String>(reader["Crew"], String.Empty).Trim();

                        lite.Thickness = CastDbNull.To<decimal>(reader["Thickness"], 0);
                        lite.Color = CastDbNull.To<String>(reader["Color"], String.Empty).Trim();
                        lite.HeatTreatment = CastDbNull.To<String>(reader["HeatTreatment"], String.Empty).Trim();
                        lite.FirstSrfCoat = CastDbNull.To<String>(reader["FirstSrfCoat"], String.Empty).Trim();
                        lite.SecondSrfCoat = CastDbNull.To<String>(reader["SecondSrfCoat"], String.Empty).Trim();

                        lite.TransRejectType = CastDbNull.To<String>(reader["TransRejectType"], String.Empty).Trim();
                        lite.TransRejectCode = CastDbNull.To<String>(reader["TransRejectCode"], String.Empty).Trim();
                        lite.LineMapRejectType = CastDbNull.To<String>(reader["LineMapRejectType"], String.Empty).Trim();
                        lite.LineMapRejectCode = CastDbNull.To<String>(reader["LineMapRejectCode"], String.Empty).Trim();
                        lite.LineMapRejectDesc = CastDbNull.To<String>(reader["LineMapRejectDesc"], String.Empty).Trim();
                        lite.LineMapBenchmarkCode = CastDbNull.To<String>(reader["LineMapBenchmarkCode"], String.Empty).Trim();
                        lite.LineMapBenchmarkDesc = CastDbNull.To<String>(reader["LineMapBenchmarkDesc"], String.Empty).Trim();

                        lite.Lites = Convert.ToInt32(CastDbNull.To<decimal>(reader["Lites"], 0));
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
                throw new ApplicationException(e.Message);
            }

            return lites;
        }


        /*********************************************************************************
         * Raw Rejects
         *******************************************************************************/
        public SqlCommand GetRawRejectedLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                HI.ProductionDate,
	            HI.ProductionLineCode,
	            HI.LineOperationCode,
                HI.Shift,
                HI.Crew,
                HI.Thickness,
                HI.Color,
                HI.HeatTreatment,
                HI.[1stSrfCoating] AS FirstSrfCoat,
                HI.[2ndSrfCoating] AS SecondSrfCoat,
                HI.RejectCode AS TransRejectCode,
                RM.LineOperationRejectTypeCode AS LineMapRejectType,
                RM.RejectCode AS LineMapRejectCode,
                RM.RejectDescription AS LineMapRejectDesc,
                RM.BenchmarkCode As LineMapBenchmarkCode,
                RM.BenchmarkDescription AS LineMapBenchmarkDesc,
                MIN(HI.TransactionDate) AS MinTransactionDate,
                MIN(HI.ProductionDate) AS MinProductionDate,
                MAX(HI.TransactionDate) AS MaxTransactionDate,
                MAX(HI.ProductionDate) AS MaxProductionDate,
                SUM(HI.Quantity + HI.QuantityCanceled) AS Lites,
                SUM((HI.Quantity + HI.QuantityCanceled) * HI.SqftActual) AS ActualSqft,
                SUM((HI.Quantity + HI.QuantityCanceled) * HI.SqftBillable) AS BillableSqft,
                SUM((HI.Quantity + HI.QuantityCanceled) * HI.Thickness * HI.SqftActual * .000249) AS Tons
            FROM
                dbo.HVIssues HI WITH (NOLOCK)
	            INNER JOIN (
		            SELECT
				            PL.Code AS ProductionLineCode ,
				            LT.Code AS LineTypeCode
		            FROM
				            ClearPath.LineType LT WITH (NOLOCK)
				            INNER JOIN ClearPath.LineArea LA WITH (NOLOCK)
				            ON LT.LineTypeID = LA.LineTypeID
				            INNER JOIN ClearPath.LineOperation LO WITH (NOLOCK)
				            ON LA.LineAreaID = LO.LineAreaID
				            INNER JOIN ClearPath.WorkflowPathItem WPI WITH (NOLOCK)
				            ON LO.LineOperationID = WPI.LineOperationID
				            INNER JOIN ClearPath.WorkflowPath WP WITH (NOLOCK)
				            ON WPI.WorkflowPathID = WP.WorkflowPathID AND
					             WP.Active = 1
				            INNER JOIN ClearPath.ProductionLine PL WITH (NOLOCK)
				            ON WP.LineID = PL.LineID
		            GROUP BY
				            PL.Code ,
				            LT.Code ) PL
				            ON PL.ProductionLineCode = HI.ProductionLineCode
                LEFT OUTER JOIN ClearPath.vw_RejectReasonMapping RM
                ON  HI.RejectCode = RM.RejectCode AND
                    HI.LineOperationCode = RM.OperationCode AND
		            PL.LineTypeCode = RM.LineTypeCode 
            WHERE    
	            HI.ProductionDate BETWEEN @StartDate AND @EndDate AND
	            ISNULL(HI.DoNotUpload, 0) = 0 AND
	            ISNULL(HI.RejectCode, '') != ''
            GROUP BY
                HI.ProductionDate,
	            HI.ProductionLineCode,
	            HI.LineOperationCode,
                HI.Shift,
                HI.Crew,
                HI.Thickness,
                HI.Color,
                HI.HeatTreatment,
                HI.[1stSrfCoating],
                HI.[2ndSrfCoating],
                HI.RejectCode,
                RM.LineOperationRejectTypeCode,
                RM.RejectCode,
                RM.RejectDescription,
                RM.BenchmarkCode,
                RM.BenchmarkDescription";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public TransactedLiteAggregates GetRawRejectedLites(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new TransactedLiteAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetRawRejectedLites();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new TransactedLiteAggregate();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MinTransactionDate = CastDbNull.To<DateTime>(reader["MinTransactionDate"], DateTime.MinValue);
                        lite.MinProductionDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MaxTransactionDate = CastDbNull.To<DateTime>(reader["MaxTransactionDate"], DateTime.MinValue);
                        lite.MaxProductionDate = CastDbNull.To<DateTime>(reader["MaxProductionDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
                        //lite.WorkCenterId = CastDbNull.To<String>(reader["E1WorkCenterID"], String.Empty).Trim();
                        lite.Shift = Convert.ToInt16(CastDbNull.To<String>(reader["Shift"], String.Empty).Trim());
                        lite.Crew = CastDbNull.To<String>(reader["Crew"], String.Empty).Trim();

                        lite.Thickness = CastDbNull.To<decimal>(reader["Thickness"], 0);
                        lite.Color = CastDbNull.To<String>(reader["Color"], String.Empty).Trim();
                        lite.HeatTreatment = CastDbNull.To<String>(reader["HeatTreatment"], String.Empty).Trim();
                        lite.FirstSrfCoat = CastDbNull.To<String>(reader["FirstSrfCoat"], String.Empty).Trim();
                        lite.SecondSrfCoat = CastDbNull.To<String>(reader["SecondSrfCoat"], String.Empty).Trim();

                        lite.TransRejectCode = CastDbNull.To<String>(reader["TransRejectCode"], String.Empty).Trim();
                        lite.LineMapRejectType = CastDbNull.To<String>(reader["LineMapRejectType"], String.Empty).Trim();
                        lite.LineMapRejectCode = CastDbNull.To<String>(reader["LineMapRejectCode"], String.Empty).Trim();
                        lite.LineMapRejectDesc = CastDbNull.To<String>(reader["LineMapRejectDesc"], String.Empty).Trim();
                        lite.LineMapBenchmarkCode = CastDbNull.To<String>(reader["LineMapBenchmarkCode"], String.Empty).Trim();
                        lite.LineMapBenchmarkDesc = CastDbNull.To<String>(reader["LineMapBenchmarkDesc"], String.Empty).Trim();

                        lite.Lites = Convert.ToInt32(CastDbNull.To<decimal>(reader["Lites"], 0));
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
                throw new ApplicationException(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
          * Customer Supplied
          *******************************************************************************/
        public SqlCommand GetCustomerSupplied()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                    HC.ProductionLineCode,
                    HC.LineID,
                    HC.Shift,
                    HC.Crew,     
                    MIN(HC.TransactionDate) AS MinTransactionDate,
	                MIN(HC.ProductionDate) AS MinProductionDate,
                    MAX(HC.TransactionDate) AS MaxTransactionDate,
                    MAX(HC.ProductionDate) AS MaxProductionDate,
                    SUM(HC.Quantity - HC.PartialQty) AS Lites,
                    SUM((HC.Quantity - HC.PartialQty) * HC.SqftActual) AS ActualSqft,               
                    SUM((HC.Quantity - HC.PartialQty) * HC.SqftBillable) AS BillableSqft
                FROM
                    dbo.HVReceipts HC WITH (NOLOCK)
                WHERE
                    HC.ProductionDate BETWEEN @StartDate AND @EndDate AND
                    HC.WOrderType='LV'
                GROUP BY
                    HC.ProductionLineCode,
                    HC.LineID,
                    HC.Shift,
                    HC.Crew";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public TransactedLiteAggregates GetCustomerSupplied(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new TransactedLiteAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetCustomerSupplied();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new TransactedLiteAggregate();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MinTransactionDate = CastDbNull.To<DateTime>(reader["MinTransactionDate"], DateTime.MinValue);
                        lite.MinProductionDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MaxTransactionDate = CastDbNull.To<DateTime>(reader["MaxTransactionDate"], DateTime.MinValue);
                        lite.MaxProductionDate = CastDbNull.To<DateTime>(reader["MaxProductionDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
                        lite.Shift = Convert.ToInt16(CastDbNull.To<String>(reader["Shift"], String.Empty).Trim());
                        lite.Crew = CastDbNull.To<String>(reader["Crew"], String.Empty).Trim();

                        lite.Lites = Convert.ToInt32(CastDbNull.To<decimal>(reader["Lites"], 0));
                        lite.ActualSqft = CastDbNull.To<decimal>(reader["ActualSqft"], 0);
                        lite.BillableSqft = CastDbNull.To<decimal>(reader["BillableSqft"], 0);
                        lites.Add(lite);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
          * QC Hold
          *******************************************************************************/
        public SqlCommand GetQcHold()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                    HC.ProductionLineCode,
                    HC.LineID,
                    HC.Shift,
                    HC.Crew,     
                    CR.Thickness,
                    CR.Lite1_Color AS Color,
                    CR.Lite1_HeatTreatment AS HeatTreatment,
                    CR.Lite1_1stSrfCoat AS FirstSrfCoat,
                    CR.Lite1_2ndSrfCoat AS SecondSrfCoat,        
                    MIN(HC.TransactionDate) AS MinTransactionDate,
	                MIN(HC.ProductionDate) AS MinProductionDate,
                    MAX(HC.TransactionDate) AS MaxTransactionDate,
                    MAX(HC.ProductionDate) AS MaxProductionDate,
                    SUM(HC.Quantity - HC.PartialQty) AS Lites,
                    SUM((HC.Quantity - HC.PartialQty) * HC.SqftActual) AS ActualSqft,               
                    SUM((HC.Quantity - HC.PartialQty) * HC.SqftBillable) AS BillableSqft,
                    SUM((HC.Quantity - HC.PartialQty) * CR.Thickness * HC.SqftActual * .000249) AS Tons
                FROM
                    dbo.HVReceipts HC WITH (NOLOCK)
	                INNER JOIN dbo.HVCCrateInfoE1 CR WITH (NOLOCK)
	                    ON  HC.SerialNumber = CR.SerialNumber AND
			                HC.FirmSchedule = CR.ScheduleNumber
                    INNER JOIN dbo.RawTags RT WITH (NOLOCK)
                        ON  RT.SerialNum = HC.SerialNumber
                WHERE
                    HC.ProductionDate BETWEEN @StartDate AND @EndDate AND
                    RT.Status IN ( '9', 'N' )
                GROUP BY
                    HC.ProductionLineCode,
                    HC.LineID,
                    HC.Shift,
                    HC.Crew,
                    CR.Thickness,
                    CR.Lite1_Color,
                    CR.Lite1_HeatTreatment,
                    CR.Lite1_1stSrfCoat,
                    CR.Lite1_2ndSrfCoat";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public TransactedLiteAggregates GetQcHold(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new TransactedLiteAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetQcHold();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new TransactedLiteAggregate();
                        lite.PlantCode = plant.PlantCode;
                        lite.PlantNum = plant.PlantNum;
                        lite.ReportDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MinTransactionDate = CastDbNull.To<DateTime>(reader["MinTransactionDate"], DateTime.MinValue);
                        lite.MinProductionDate = CastDbNull.To<DateTime>(reader["MinProductionDate"], DateTime.MinValue);
                        lite.MaxTransactionDate = CastDbNull.To<DateTime>(reader["MaxTransactionDate"], DateTime.MinValue);
                        lite.MaxProductionDate = CastDbNull.To<DateTime>(reader["MaxProductionDate"], DateTime.MinValue);
                        lite.ProductionLineCode = CastDbNull.To<String>(reader["ProductionLineCode"], String.Empty).Trim();
                        lite.Shift = Convert.ToInt16(CastDbNull.To<String>(reader["Shift"], String.Empty).Trim());
                        lite.Crew = CastDbNull.To<String>(reader["Crew"], String.Empty).Trim();

                        lite.Thickness = CastDbNull.To<decimal>(reader["Thickness"], 0);
                        lite.Color = CastDbNull.To<String>(reader["Color"], String.Empty).Trim();
                        lite.HeatTreatment = CastDbNull.To<String>(reader["HeatTreatment"], String.Empty).Trim();
                        lite.FirstSrfCoat = CastDbNull.To<String>(reader["FirstSrfCoat"], String.Empty).Trim();
                        lite.SecondSrfCoat = CastDbNull.To<String>(reader["SecondSrfCoat"], String.Empty).Trim();

                        lite.Lites = Convert.ToInt32(CastDbNull.To<decimal>(reader["Lites"], 0));
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
                throw new ApplicationException(e.Message);
            }

            return lites;
        }

        /*********************************************************************************
         * Production Lines
         *******************************************************************************/
        public SqlCommand GetProductionLines()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                Code,
                Description,
                E1WorkCenterID
              FROM
                ClearPath.ProductionLine WITH (NOLOCK)";
            return cmd;
        }

        public ProductionLines GetProductionLines(SqlConnection cn, Plant plant)
        {
            var lines = new ProductionLines();
            try
            {
                var cmd = GetProductionLines();
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new ProductionLine();
                        line.ProductionLineCode = CastDbNull.To<String>(reader["Code"], String.Empty).Trim();
                        line.WorkCenterId = CastDbNull.To<String>(reader["E1WorkCenterID"], String.Empty).Trim();
                        line.Description = CastDbNull.To<String>(reader["Description"], String.Empty).Trim();
                        lines.Add(line);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            return lines;
        }

        /*********************************************************************************
         * Production Line Aggregate
         *******************************************************************************/
        public CoaterAggregates GetCoaterSummary(Plant plant, SqlConnection cn, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new CoaterAggregates();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var consumedSheetsAll = GetConsumedSheets(cn, plant, dateCode, startDate, endDate);
                var producedLitesAll = GetProducedLites(cn, plant, dateCode, startDate, endDate);
                var customerSuppliedAll = GetCustomerSupplied(cn, plant, dateCode, startDate, endDate);
                var qcHoldLitesAll = GetQcHold(cn, plant, dateCode, startDate, endDate);

                var finishedRejectedLites = GetFinishedRejectedLites(cn, plant, dateCode, startDate, endDate);
                var rawRejectedLites = GetRawRejectedLites(cn, plant, dateCode, startDate, endDate);

                var finishedRejectedLitesAll = finishedRejectedLites.Where(x => x.TransRejectType != "E" && x.LineMapRejectDesc != "EXTRA").ToList();
                var rawRejectedLitesAll = rawRejectedLites.Where(x => x.FirstSrfCoat == "" && x.SecondSrfCoat == "" && x.TransRejectType != "E" && x.LineMapRejectDesc != "EXTRA").ToList();
                var totalRejectedLitesAll = new TransactedLiteAggregates();
                totalRejectedLitesAll.AddRange(finishedRejectedLitesAll);
                totalRejectedLitesAll.AddRange(rawRejectedLitesAll);

                var extraLitesAll = new TransactedLiteAggregates();
                extraLitesAll.AddRange(finishedRejectedLites.Where(x => x.TransRejectType == "E" || x.LineMapRejectDesc == "EXTRA"));
                extraLitesAll.AddRange(rawRejectedLites.Where(x => x.TransRejectType == "E" || x.LineMapRejectDesc == "EXTRA"));

                var lines = GetProductionLines(cn, plant).Where(x => x.ProductionLineCode.StartsWith("COATHV")).ToList();
                switch (plant.PlantCode)
                {
                    case "TWCG":
                        lines.RemoveAll(x => x.ProductionLineCode == "CUTLV4");
                        break;
                    case "GTCG":
                        lines.RemoveAll(x => x.ProductionLineCode == "CUTLV2");
                        break;
                }
                foreach (var line in lines)
                {
                    var agg = new CoaterAggregate();
                    agg.PlantCode = plant.PlantCode;
                    agg.PlantNum = plant.PlantNum;
                    agg.StartDate = startDate;
                    agg.EndDate = endDate;
                    agg.ProductionLineCode = line.ProductionLineCode;
                    agg.WorkCenterId = line.WorkCenterId;

                    var consumedSheetsLine = consumedSheetsAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var producedLitesLine = producedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var customerLitesLine = customerSuppliedAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var finishedRejectedLitesLine = finishedRejectedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var rawRejectedLitesLine = rawRejectedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var totalRejectedLitesLine = totalRejectedLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var qcHoldLitesLine = qcHoldLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();
                    var extraLitesLine = extraLitesAll.Where(x => x.ProductionLineCode == line.ProductionLineCode).ToList();

                    if (consumedSheetsLine.Any())
                    {
                        agg.StartDate = consumedSheetsLine.Min(x => x.ReportDate);
                        agg.EndDate = consumedSheetsLine.Max(x => x.ReportDate);

                        // CONSUMED
                        agg.ConsumedLites = consumedSheetsLine.Sum(x => x.Lites);
                        agg.ConsumedActualSqft = consumedSheetsLine.Sum(x => x.ActualSqft);
                        agg.ConsumedBillableSqft = consumedSheetsLine.Sum(x => x.BillableSqft);
                        agg.ConsumedTons = consumedSheetsLine.Sum(x => x.Tons);

                        // PRODUCED
                        agg.ProducedLites = producedLitesLine.Sum(x => x.Lites);
                        agg.ProducedActualSqft = producedLitesLine.Sum(x => x.ActualSqft);
                        agg.ProducedBillableSqft = producedLitesLine.Sum(x => x.BillableSqft);
                        agg.ProducedTons = producedLitesLine.Sum(x => x.Tons);

                        // CUSTOMER SUPPLIED
                        agg.CustomerSuppliedLites = customerLitesLine.Sum(x => x.Lites);
                        agg.CustomerSuppliedActualSqft = customerLitesLine.Sum(x => x.ActualSqft);
                        agg.CustomerSuppliedBillableSqft = customerLitesLine.Sum(x => x.BillableSqft);

                        // TOTAL REJECTED
                        agg.TotalRejectedLites = totalRejectedLitesLine.Sum(x => x.Lites);
                        agg.TotalRejectedActualSqft = totalRejectedLitesLine.Sum(x => x.ActualSqft);
                        agg.TotalRejectedBillableSqft = totalRejectedLitesLine.Sum(x => x.BillableSqft);
                        agg.TotalRejectedTons = totalRejectedLitesLine.Sum(x => x.Tons);

                        // FINISHED REJECTED
                        agg.FinishedRejectedLites = finishedRejectedLitesLine.Sum(x => x.Lites);
                        agg.FinishedRejectedActualSqft = finishedRejectedLitesLine.Sum(x => x.ActualSqft);
                        agg.FinishedRejectedBillableSqft = finishedRejectedLitesLine.Sum(x => x.BillableSqft);
                        agg.FinishedRejectedTons = finishedRejectedLitesLine.Sum(x => x.Tons);

                        // RAW REJECTED
                        agg.RawRejectedLites = rawRejectedLitesLine.Sum(x => x.Lites);
                        agg.RawRejectedActualSqft = rawRejectedLitesLine.Sum(x => x.ActualSqft);
                        agg.RawRejectedBillableSqft = rawRejectedLitesLine.Sum(x => x.BillableSqft);
                        agg.RawRejectedTons = rawRejectedLitesLine.Sum(x => x.Tons);

                        // VENDOR REJECTS
                        agg.VendorRejectedLites = totalRejectedLitesLine.Where(x => x.LineMapRejectType == "VEN").Sum(x => x.Lites);
                        agg.VendorRejectedActualSqft = totalRejectedLitesLine.Where(x => x.LineMapRejectType == "VEN").Sum(x => x.ActualSqft);
                        agg.VendorRejectedBillableSqft = totalRejectedLitesLine.Where(x => x.LineMapRejectType == "VEN").Sum(x => x.BillableSqft);
                        agg.VendorRejectedTons = totalRejectedLitesLine.Where(x => x.LineMapRejectType == "VEN").Sum(x => x.Tons);

                        // INTERNAL REJECTS
                        agg.InternalRejectedLites = finishedRejectedLitesLine.Where(x => x.TransRejectType == "I").Sum(x => x.Lites);
                        agg.InternalRejectedActualSqft = finishedRejectedLitesLine.Where(x => x.TransRejectType == "I").Sum(x => x.ActualSqft);
                        agg.InternalRejectedBillableSqft = finishedRejectedLitesLine.Where(x => x.TransRejectType == "I").Sum(x => x.BillableSqft);
                        agg.InternalRejectedTons = finishedRejectedLitesLine.Where(x => x.TransRejectType == "I").Sum(x => x.Tons);

                        // QC HOLD
                        agg.QcHoldLites = qcHoldLitesLine.Sum(x => x.Lites);
                        agg.QcHoldActualSqft = qcHoldLitesLine.Sum(x => x.ActualSqft);
                        agg.QcHoldBillableSqft = qcHoldLitesLine.Sum(x => x.BillableSqft);
                        agg.QcHoldTons = qcHoldLitesLine.Sum(x => x.Tons);

                        // EXTRA PRODUCTION
                        agg.ExtraLites = extraLitesLine.Sum(x => x.Lites);
                        agg.ExtraActualSqft = extraLitesLine.Sum(x => x.ActualSqft);
                        agg.ExtraBillableSqft = extraLitesLine.Sum(x => x.BillableSqft);
                        agg.ExtraTons = extraLitesLine.Sum(x => x.Tons);

                        // NET
                        agg.NetYield = Math.Round(agg.ProducedActualSqft / agg.ConsumedActualSqft, 5);
                        agg.ProductionHours =
                            (decimal)Math.Round(consumedSheetsLine.GroupBy(x => new { x.ReportDate, x.Shift })
                                .Select(
                                    lg =>
                                        new
                                        {
                                            Date = lg.Key,
                                            Hours =
                                                (lg.Max(x => x.MaxTransactionDate) - lg.Min(x => x.MinTransactionDate))
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
