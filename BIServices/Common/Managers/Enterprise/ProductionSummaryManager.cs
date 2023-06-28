using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using BIServices.ProductionSvc;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class ProductionSummaryManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UdcItemManager _udcItemManager;

        public ProductionSummaryManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _udcItemManager = new UdcItemManager(session);
        }

        /*********************************************************************************
          * Get Requested Date Facts
          *******************************************************************************/
        public iDB2Command GetRequestedDateFacts(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
			cmd.CommandTimeout = 0;
            cmd.CommandText = $@"
            SELECT 
                SDMCU,
	            SDDRQJ,
				COALESCE(SLDRQJ, SDDRQJ) AS SLDRQJ,
	            SDAN8,
	            SDAN8_NM,
	            SDSHAN,
	            SDSHAN_NM,
	            SDURCD,
                SDDSC1,
                SDDSC2,
                SDNXTR,
	            IBSTKT,
	            IMSRP4,
                SAZ58L1LPT,
	            SUM(REQUESTED_UNITS) AS REQUESTED_UNITS,
                SUM(REQUESTED_SQFT) AS REQUESTED_SQFT,
--
	            SUM(UNSCHEDULED_UNITS) AS UNSCHEDULED_UNITS,
                SUM(UNSCHEDULED_SQFT) AS UNSCHEDULED_SQFT,
	            SUM(SCHEDULED_UNITS) AS SCHEDULED_UNITS,
                SUM(SCHEDULED_SQFT) AS SCHEDULED_SQFT,
--
				SUM(CASE WHEN BOXED_UNITS = 0 THEN SCHEDULED_UNITS ELSE 0 END) AS UNBOXED_UNITS,
				SUM(CASE WHEN BOXED_UNITS = 0 THEN SCHEDULED_SQFT ELSE 0 END) AS UNBOXED_SQFT,
	            SUM(VOID_UNITS)  AS VOID_UNITS,
                SUM(VOID_SQFT)  AS VOID_SQFT,
	            SUM(OPEN_UNITS)  AS OPEN_UNITS,
                SUM(OPEN_SQFT)  AS OPEN_SQFT,
	            SUM(COMPLETED_UNITS)  AS COMPLETED_UNITS,
                SUM(COMPLETED_SQFT)  AS COMPLETED_SQFT,
	            SUM(CLOSED_UNITS)  AS CLOSED_UNITS,
                SUM(CLOSED_SQFT)  AS CLOSED_SQFT,
	            SUM(LOCKED_UNITS)  AS LOCKED_UNITS,
                SUM(LOCKED_SQFT)  AS LOCKED_SQFT,
	            SUM(COALESCE(SHIPPED_UNITS, SCHEDULED_UNITS) - CASE WHEN BOXED_UNITS = 0 THEN SCHEDULED_UNITS ELSE 0 END) AS SHIPPED_UNITS,
	            SUM(COALESCE(SHIPPED_SQFT, SCHEDULED_SQFT) - CASE WHEN BOXED_UNITS = 0 THEN SCHEDULED_SQFT ELSE 0 END) AS SHIPPED_SQFT,
--
	            SUM(COALESCE(COMPLETED_UNITS, 0) + COALESCE(CLOSED_UNITS, 0) + COALESCE(LOCKED_UNITS, 0) + COALESCE(SHIPPED_UNITS, SCHEDULED_UNITS, 0) - CASE WHEN BOXED_UNITS = 0 THEN SCHEDULED_UNITS ELSE 0 END )  AS PRODUCED_UNITS,
	            SUM(COALESCE(COMPLETED_SQFT, 0) + COALESCE(CLOSED_SQFT, 0) + COALESCE(LOCKED_SQFT, 0) + COALESCE(SHIPPED_SQFT, SCHEDULED_SQFT, 0)  - CASE WHEN BOXED_UNITS = 0 THEN SCHEDULED_SQFT ELSE 0 END )  AS PRODUCED_SQFT
            FROM 
	            (
                SELECT 
                    SDMCU,
		            SDDRQJ,
		            SDAN8,
		            AB1.ABALPH AS SDAN8_NM,
		            SDSHAN,
		            AB2.ABALPH AS SDSHAN_NM,
					SDKCOO,
		            SDDOCO,
		            SDDCTO,
		            SDLNID,
		            SDURCD,
                    SDDSC1,
                    SDDSC2,
                    SDNXTR,
		            IBSTKT,
		            IMSRP4,
                    SAZ58L1LPT,
					SLDRQJ,
					ROW_NUMBER() OVER (PARTITION BY SDKCOO, SDDOCO, SDDCTO, SDLNID ORDER BY SLUPMJ, SLTDAY) AS RN,
					-- REQUESTED
		            CAST(SDUORG / 1000 AS INT) AS REQUESTED_UNITS,
					CAST((COALESCE(SAZ58USF, 0) / 10000) AS DECIMAL(15,4))  *  CAST(SDUORG / 1000 AS INT)  AS REQUESTED_SQFT,
					-- UNSCHEDULED
		            CAST(CASE WHEN SDNXTR IN ('523','525','530','535','540') THEN SDUORG/1000 ELSE 0 END AS INT) AS UNSCHEDULED_UNITS,
					CAST((COALESCE(SAZ58USF, 0) / 10000) AS DECIMAL(15,4))  *  CAST(CASE WHEN SDNXTR IN ('523','525','530','535','540') THEN SDUORG/1000 ELSE 0 END AS INT)  AS UNSCHEDULED_SQFT,
					-- SCHEDULED
		            CAST(CASE WHEN SDNXTR NOT IN ('523','525','530','535','540') THEN SDUORG/1000 ELSE 0 END AS INT) AS SCHEDULED_UNITS,
					CAST((COALESCE(SAZ58USF, 0) / 10000) AS DECIMAL(15,4)) * CAST(CASE WHEN SDNXTR NOT IN ('523','525','530','535','540') THEN SDUORG/1000 ELSE 0 END AS INT)  AS SCHEDULED_SQFT,
					-- BOXED
					(select count(*) from F580200 where BIMCU = SDMCU AND BIDOCO = SAZ58WODC AND BIDCTO = SAZ58WOTY) AS BOXED_UNITS,
					-- VOID
					(select SUM(CASE WHEN RDZ58DST IN ('0') THEN CAST(RDZ58CQTY/1000 AS INT) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS VOID_UNITS,
					(select SUM(CASE WHEN RDZ58DST IN ('0') THEN CAST(SAZ58USF/10000 AS DECIMAL(15,4)) * (SDUORG/1000)  ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS VOID_SQFT,
					-- OPEN
					(select SUM(CASE WHEN RDZ58DST IN ('1') THEN CAST(RDZ58CQTY/1000 AS INT) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS OPEN_UNITS,
					(select SUM(CASE WHEN RDZ58DST IN ('1') THEN CAST(SAZ58USF/10000 AS DECIMAL(15,4))  * (RDZ58CQTY/1000) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS OPEN_SQFT,
					-- COMPLETED
					(select SUM(CASE WHEN RDZ58DST IN ('3') THEN CAST(RDPQOH/1000 AS INT) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS COMPLETED_UNITS,
					(select SUM(CASE WHEN RDZ58DST IN ('3') THEN CAST(SAZ58USF/10000 AS DECIMAL(15,4)) * (RDPQOH/1000) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS COMPLETED_SQFT,
					-- CLOSED
					(select SUM(CASE WHEN RDZ58DST IN ('4') THEN CAST(RDPQOH/1000 AS INT) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS CLOSED_UNITS,
					(select SUM(CASE WHEN RDZ58DST IN ('4') THEN CAST(SAZ58USF/10000 AS DECIMAL(15,4)) * (RDPQOH/1000) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS CLOSED_SQFT,
					-- LOCKED
					(select SUM(CASE WHEN RDZ58DST IN ('5') THEN CAST(RDPQOH/1000 AS INT) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS LOCKED_UNITS,
					(select SUM(CASE WHEN RDZ58DST IN ('5') THEN CAST(SAZ58USF/10000 AS DECIMAL(15,4))  * (RDPQOH/1000) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS LOCKED_SQFT,
					-- SHIPPED
					(select SUM(CASE WHEN RDZ58DST IN ('7','8') THEN CAST(RDPQOH/1000 AS INT) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS SHIPPED_UNITS,
					(select SUM(CASE WHEN RDZ58DST IN ('7','8') THEN CAST(SAZ58USF/10000 AS DECIMAL(15,4))  * (RDPQOH/1000) ELSE 0 END) from F580056 where RDMCU = SDMCU AND RDDOCO = SDDOCO AND RDDCTO = SDDCTO AND RDLNID = SDLNID) AS SHIPPED_SQFT
	            FROM 
		            F4211
		            INNER JOIN F4102
		            ON	IBMCU = SDMCU AND
			            IBITM = SDITM
		            INNER JOIN F4101
		            ON	IMITM = IBITM
		            INNER JOIN F0101 AB1
		            ON AB1.ABAN8 = SDAN8
		            INNER JOIN F0101 AB2
		            ON AB2.ABAN8 = SDSHAN
					INNER JOIN F5834A15
						ON SADOCO = SDDOCO AND
						SADCTO = SDDCTO AND
						SALNID = SDLNID AND
						SAKCOO = SDKCOO
					--INNER JOIN F5834A11
					--ON  S5DOCO = SAZ58WODC AND
					--	S5DCTO = SAZ58WOTY
					LEFT OUTER JOIN F42199
					ON SLKCOO = SDKCOO AND
						SLDOCO = SDDOCO AND
						SLDCTO = SDDCTO AND
						SLLNID = SDLNID
	            WHERE 
		            {filterString}
                    SDDCTO NOT IN ('SQ') AND
                    SDGLC IN ('GLAS','GLAF','GLAX','GLAC','GLAZ') AND
		            SDLNTY = 'W' AND
                    SDLTTR != '980' AND
		            SDMCU = ? AND
		            SDDRQJ >= ? AND 
                    SDDRQJ <= ?) A
			WHERE 
                A.RN = 1
            GROUP BY
                SDMCU,
		        SDDRQJ,
		        SDURCD,
                SDDSC1,
                SDDSC2,
                SDNXTR,
		        SDAN8,
		        SDAN8_NM,
		        SDSHAN,
		        SDSHAN_NM,
		        IBSTKT,
		        IMSRP4,
                SAZ58L1LPT,
				SLDRQJ
			FOR READ ONLY ";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("DRQJ1", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DRQJ1"].Precision = 6;
            cmd.Parameters["DRQJ1"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DRQJ2", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DRQJ2"].Precision = 6;
            cmd.Parameters["DRQJ2"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetRequestedDateFactsOLD(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT 
                SDMCU,
	            SDDRQJ,
	            SDAN8,
	            SDAN8_NM,
	            SDSHAN,
	            SDSHAN_NM,
	            SDURCD,
	            IBSTKT,
	            IMSRP4,
                SAZ58L1LPT,
	            SUM(SDUORG) AS REQUESTED_UNITS,
                SUM(SAZ58USF) AS REQUESTED_SQFT,
	            SUM(PRODUCED) AS PRODUCED_UNITS,
				SUM(PRODUCED_SQFT) AS PRODUCED_SQFT,
	            SUM(NOTSCHEDULED_UNITS) AS NOT_SCHEDULED_UNITS,
	            SUM(NOTSCHEDULED_SQFT) AS NOT_SCHEDULED_SQFT,
				SUM(SDUORG) - SUM(PRODUCED)  - SUM(NOTSCHEDULED_UNITS) AS OPEN_UNITS,
				SUM(SAZ58USF) - SUM(PRODUCED_SQFT)  - SUM(NOTSCHEDULED_SQFT) AS OPEN_SQFT
            FROM 
	            (SELECT 
                    SDMCU,
		            SDDRQJ,
		            SDAN8,
		            AB1.ABALPH AS SDAN8_NM,
		            SDSHAN,
		            AB2.ABALPH AS SDSHAN_NM,
		            SDDOCO,
		            SDDCTO,
		            SDLNID,
		            SDURCD,
		            IBSTKT,
		            IMSRP4,
		            SDUORG / 1000 AS SDUORG,
                    SAZ58L1LPT,
                    SUM((SAZ58USF / 10000.000) ) AS SAZ58USF,
		            SUM(CASE WHEN RDZ58DST IN ('0','1','2','3') THEN 0.00 ELSE RDPQOH/1000 END) AS PRODUCED,
		            SUM(CASE WHEN RDZ58DST IN ('0','1','2','3') THEN 0.000 ELSE RDPQOH * SAZ58USF / SDUORG END) / 10000 AS PRODUCED_SQFT,
                    SUM(CASE WHEN SDNXTR IN ('523','525','530','535','540') THEN SDUORG/1000 ELSE 0.00 END) AS NOTSCHEDULED_UNITS,
                    SUM(CASE WHEN SDNXTR IN ('523','525','530','535','540') THEN SAZ58USF/10000 ELSE 0.00 END) AS NOTSCHEDULED_SQFT
	            FROM 
		            F4211 
		            INNER JOIN F4102
		            ON	IBMCU = SDMCU AND
			            IBITM = SDITM
		            --INNER JOIN F4801
		            --ON	CAST(WADOCO AS VARCHAR(8)) = SDRORN AND
			        --    WADCTO = SDRCTO
		            INNER JOIN F4101
		            ON	IMITM = IBITM
		            INNER JOIN F0101 AB1
		            ON AB1.ABAN8 = SDAN8
		            INNER JOIN F0101 AB2
		            ON AB2.ABAN8 = SDSHAN
		            LEFT OUTER JOIN F580056
		            ON  RDDOCO = SDDOCO AND
			            RDDCTO = SDDCTO AND
			            RDLNID = SDLNID
					LEFT OUTER JOIN F5834A15
						ON SADOCO = SDDOCO AND
						SADCTO = SDDCTO AND
						SALNID = SDLNID AND
						SAKCOO = SDKCOO
	            WHERE 
		            {filterString}
                    SDGLC IN ('GLAS','GLAF','GLAX','GLAC','GLAZ') AND
		            SDLNTY = 'W' AND
                    SDLTTR != '980' AND
		            SDMCU = ? AND
		            SDDRQJ >= ? AND 
                    SDDRQJ <= ?
	            GROUP BY
                    SDMCU,
		            SDDRQJ,
		            SDAN8,
		            AB1.ABALPH,
		            SDSHAN,
		            AB2.ABALPH,
		            SDUORG,
		            SDDOCO,
		            SDDCTO,
		            SDLNID,
		            SDURCD,
		            IBSTKT,
		            IMSRP4,
                    SAZ58L1LPT
                    ) A
            GROUP BY
                SDMCU,
		        SDDRQJ,
		        SDURCD,
		        SDAN8,
		        SDAN8_NM,
		        SDSHAN,
		        SDSHAN_NM,
		        IBSTKT,
		        IMSRP4,
                SAZ58L1LPT
            ORDER BY 1,2,3,4,5
			FOR READ ONLY
            ";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("DRQJ1", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DRQJ1"].Precision = 6;
            cmd.Parameters["DRQJ1"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DRQJ2", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DRQJ2"].Precision = 6;
            cmd.Parameters["DRQJ2"].Scale = 0;
            return cmd;
        }

        public RequestedDateFacts GetRequestedDateFacts(BusinessSystemSession session, SqlConnection cnDt, Plant plant, string dateCode, DateTime? startDate, DateTime? endDate) //, bool notScheduledUnits)
        {
            var facts = new RequestedDateFacts();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dt = Timestamp.GetDateCodeRange(cnDt, dateCode);
                    startDate = dt["MIN"];
                    endDate = dt["MAX"];
                }
                List<string> filters = new List<string>();

                filters.Add("SDNXTR NOT IN ('000') AND \r\n");
                // filters.Add("(SDNXTR IN ('523','525','530','535','540') OR SDNXTR > '540') AND \r\n");

                var cmd = GetRequestedDateFacts(filters);
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                cmd.Parameters[1].Value = Timestamp.CastToE1Date((DateTime)startDate);
                cmd.Parameters[2].Value = Timestamp.CastToE1Date((DateTime)endDate);
                _logger.Info(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var fact = new RequestedDateFact();
                        fact.Plant = CastDbNull.To<String>(reader["SDMCU"], String.Empty).Trim();
                        var reqDateId = CastDbNull.To<Decimal>(reader["SDDRQJ"], 0);
                        fact.RequestedDate = Timestamp.CastToDateTime(reqDateId);
                        fact.InitialRequestedDate = Timestamp.CastToDateTime(CastDbNull.To<Decimal>(reader["SLDRQJ"], 0));
                        fact.CustomerSoldNum = CastDbNull.To<Decimal>(reader["SDAN8"], 0);
                        fact.CustomerSoldName = CastDbNull.To<String>(reader["SDAN8_NM"], String.Empty).Trim();
                        fact.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        fact.CustomerShipName = CastDbNull.To<String>(reader["SDSHAN_NM"], String.Empty).Trim();
                        fact.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                        fact.Description1 = CastDbNull.To<String>(reader["SDDSC1"], String.Empty).Trim();
                        fact.Description2 = CastDbNull.To<String>(reader["SDDSC2"], String.Empty).Trim();
                        fact.NextStatus = CastDbNull.To<String>(reader["SDNXTR"], String.Empty).Trim();
                        fact.StockType = CastDbNull.To<String>(reader["IBSTKT"], String.Empty).Trim();
                        fact.UnitType = CastDbNull.To<String>(reader["IMSRP4"], String.Empty).Trim();
                        fact.LamiProdType = _udcItemManager.GetUdcItem(session, "57", "LM", CastDbNull.To<String>(reader["SAZ58L1LPT"], String.Empty).Trim()).Description1;
                        fact.RequestedUnits = CastDbNull.To<int>(reader["REQUESTED_UNITS"], 0);
                        fact.RequestedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["REQUESTED_SQFT"], 0), 2);
                        fact.ScheduledUnits = CastDbNull.To<int>(reader["SCHEDULED_UNITS"], 0);
                        fact.ScheduledActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["SCHEDULED_SQFT"], 0), 2);
                        fact.NotScheduledUnits = CastDbNull.To<int>(reader["UNSCHEDULED_UNITS"], 0);
                        fact.NotScheduledActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["UNSCHEDULED_SQFT"], 0), 2);
                        fact.UnboxedUnits = CastDbNull.To<int>(reader["UNBOXED_UNITS"], 0);
                        fact.UnboxedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["UNBOXED_SQFT"], 0), 2);
                        fact.VoidUnits = CastDbNull.To<int>(reader["VOID_UNITS"], 0);
                        fact.VoidActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["VOID_SQFT"], 0), 2);
                        fact.OpenUnits = CastDbNull.To<int>(reader["OPEN_UNITS"], 0);
                        fact.OpenActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["OPEN_SQFT"], 0), 2);
                        fact.CompletedUnits = CastDbNull.To<int>(reader["COMPLETED_UNITS"], 0);
                        fact.CompletedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["COMPLETED_SQFT"], 0), 2);
                        fact.ClosedUnits = CastDbNull.To<int>(reader["CLOSED_UNITS"], 0);
                        fact.ClosedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["CLOSED_SQFT"], 0), 2);
                        fact.LockedUnits = CastDbNull.To<int>(reader["LOCKED_UNITS"], 0);
                        fact.LockedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["LOCKED_SQFT"], 0), 2);
                        fact.ShippedUnits = CastDbNull.To<int>(reader["SHIPPED_UNITS"], 0);
                        fact.ShippedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["SHIPPED_SQFT"], 0), 2);
                        fact.ProducedUnits = CastDbNull.To<int>(reader["PRODUCED_UNITS"], 0);
                        fact.ProducedActualSqft = Math.Round(CastDbNull.To<Decimal>(reader["PRODUCED_SQFT"], 0), 2);
                        facts.Add(fact);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return facts;
        }

        /*********************************************************************************
          * Get Scheduled Date Facts
          *******************************************************************************/
        public iDB2Command GetScheduledDateFacts()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
                S5MCU,
	            S5Z58SCD,
	            S5Z58IGLN,
	            SDAN8,
	            SDAN8_NM,
	            SDSHAN,
	            SDSHAN_NM,
	            SDURCD,
	            IBSTKT,
	            IMSRP4,
	            SUM(SDUORG) AS TOTAL_UNITS,
	            SUM(PRODUCED) AS PRODUCED_UNITS
            FROM 
	            (SELECT 
                    S5MCU,
		            S5Z58SCD,
		            S5Z58IGLN,
		            SDAN8,
		            AB1.ABALPH AS SDAN8_NM,
		            SDSHAN,
		            AB2.ABALPH AS SDSHAN_NM,
		            SDDOCO,
		            SDDCTO,
		            SDLNID,
		            SDURCD,
		            IBSTKT,
		            IMSRP4,
		            S5Z58QTY,
		            SDUORG / 1000 AS SDUORG,
		            SUM(CASE WHEN RDZ58DST IN ('0','1','2','3') THEN 0.00 ELSE RDPQOH/1000 END) AS PRODUCED
	            FROM 
		            F5834A11
		            INNER JOIN F4801 
		            ON	WADOCO = S5DOCO AND
			            WADCTO = S5DCTO
		            INNER JOIN F4211
		            ON	CAST(WADOCO AS VARCHAR(8)) = SDRORN AND
			            WADCTO = SDRCTO
		            INNER JOIN F4102
		            ON	IBMCU = SDMCU AND
			            IBITM = SDITM
		            INNER JOIN F4101
		            ON	IMITM = IBITM
		            INNER JOIN F0101 AB1
		            ON AB1.ABAN8 = SDAN8
		            INNER JOIN F0101 AB2
		            ON AB2.ABAN8 = SDSHAN
		            LEFT OUTER JOIN F580056
		            ON  RDDOCO = SDDOCO AND
			            RDDCTO = SDDCTO AND
			            RDLNID = SDLNID
	            WHERE
                    S5MCU = ? AND
		            S5Z58SCD >= ? AND 
		            S5Z58SCD <= ? AND 
		            S5Z58SFS = '2' AND 
		            SDGLC IN ('GLAS','GLAF','GLAX','GLAC','GLAZ') AND
		            SDLNTY = 'W' AND
		            SDNXTR > '540' AND
                    SDLTTR != '980'
	            GROUP BY
                    S5MCU,
		            S5Z58SCD,
		            S5Z58IGLN,
		            SDAN8,
		            AB1.ABALPH,
		            SDSHAN,
		            AB2.ABALPH,
		            SDDOCO,
		            SDDCTO,
		            SDLNID,
		            SDURCD,
		            IBSTKT,
		            IMSRP4,
		            S5Z58QTY,
		            SDUORG) A
            GROUP BY
                S5MCU,
		        S5Z58SCD,
		        S5Z58IGLN,
		        SDURCD,
		        SDAN8,
		        SDAN8_NM,
		        SDSHAN,
		        SDSHAN_NM,
		        IBSTKT,
		        IMSRP4
            ORDER BY 1,2,3,4,5";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("SCD1", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD1"].Precision = 6;
            cmd.Parameters["SCD1"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SCD2", iDB2DbType.iDB2Decimal));
            cmd.Parameters["SCD2"].Precision = 6;
            cmd.Parameters["SCD2"].Scale = 0;
            return cmd;
        }

        public ScheduledDateFacts GetScheduledDateFacts(BusinessSystemSession session, SqlConnection cnDt, Plant plant, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var facts = new ScheduledDateFacts();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dt = Timestamp.GetDateCodeRange(cnDt, dateCode);
                    startDate = dt["MIN"];
                    endDate = dt["MAX"];
                }
                var cmd = GetScheduledDateFacts();
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                cmd.Parameters[1].Value = Timestamp.CastToE1Date((DateTime)startDate);
                cmd.Parameters[2].Value = Timestamp.CastToE1Date((DateTime)endDate);
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var fact = new ScheduledDateFact();
                        fact.Plant = CastDbNull.To<String>(reader["S5MCU"], String.Empty).Trim();
                        var reqDateId = CastDbNull.To<Decimal>(reader["S5Z58SCD"], 0);
                        fact.ScheduledDate = Timestamp.CastToDateTime(reqDateId);
                        fact.ProductionLine = CastDbNull.To<String>(reader["S5Z58IGLN"], String.Empty).Trim();
                        fact.CustomerSoldNum = CastDbNull.To<Decimal>(reader["SDAN8"], 0);
                        fact.CustomerSoldName = CastDbNull.To<String>(reader["SDAN8_NM"], String.Empty).Trim();
                        fact.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        fact.CustomerShipName = CastDbNull.To<String>(reader["SDSHAN_NM"], String.Empty).Trim();
                        fact.PriorityCode = CastDbNull.To<String>(reader["SDURCD"], String.Empty).Trim();
                        fact.StockType = CastDbNull.To<String>(reader["IBSTKT"], String.Empty).Trim();
                        fact.UnitType = CastDbNull.To<String>(reader["IMSRP4"], String.Empty).Trim();
                        fact.RequestedUnits = Math.Round(CastDbNull.To<Decimal>(reader["TOTAL_UNITS"], 0), 0);
                        fact.ProducedUnits = Math.Round(CastDbNull.To<Decimal>(reader["PRODUCED_UNITS"], 0), 0);
                        fact.OpenUnits = fact.RequestedUnits - fact.ProducedUnits;
                        facts.Add(fact);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return facts;
        }

        /*********************************************************************************
          * Get Scheduled Date Facts
          *******************************************************************************/
        public iDB2Command GetRacksByProduct()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
		        SDAN8,
		        AN8ALPH,
		        SDSHAN,
		        SHANALPH,
		        KZSGVL,
		        COUNT(DISTINCT CASE WHEN RHZ58HST IN ('1','2') AND TODAY = 'Y' THEN RHZ58RID ELSE NULL END) AS OPEN_RACKS_THRU_TODAY,
		        SUM(CASE WHEN RHZ58HST IN ('1','2') AND TODAY = 'Y' THEN 1 ELSE 0 END) AS OPEN_UNITS_THRU_TODAY,
		        COUNT(DISTINCT CASE WHEN RHZ58HST IN ('1','2') THEN RHZ58RID ELSE NULL END) AS OPEN_RACKS_ALL,
		        SUM(CASE WHEN RHZ58HST IN ('1','2') THEN 1 ELSE 0 END) AS OPEN_UNITS_ALL,
		        COUNT(DISTINCT CASE WHEN RHZ58HST = '4' THEN RHZ58RID ELSE NULL END) AS CLOSED_RACKS,
		        SUM(CASE WHEN RHZ58HST = '4' THEN 1 ELSE 0 END) AS CLOSED_UNITS,
		        COUNT(DISTINCT CASE WHEN RHZ58HST = '5' THEN RHZ58RID ELSE NULL END) AS LOCKED_RACKS,
		        SUM(CASE WHEN RHZ58HST = '5' THEN 1 ELSE 0 END) AS LOCKED_UNITS,
		        COUNT(DISTINCT CASE WHEN RHZ58HST IN ('4','5') THEN RHZ58RID ELSE NULL END) AS TOTAL_RACKS,
		        COUNT(DISTINCT CASE WHEN RHZ58HST IN ('4','5') THEN RDZ58SN ELSE NULL END) AS TOTAL_UNITS
            FROM (
	            SELECT
		            RHZ58RID,
		            RHZ58HST,
		            RDZ58SN,
		            RDZ58DST,
		            SDAN8,
		            AB1.ABALPH AS AN8ALPH,
		            SDSHAN,
		            AB2.ABALPH AS SHANALPH,
		            ATZ58PT,
		            KZSGVL,
					CASE WHEN SDDRQJ <= ? THEN 'Y' ELSE 'N' END AS TODAY,
		            ROW_NUMBER() OVER (PARTITION BY RHZ58RID, RDZ58SN ORDER BY KZCFGCID ASC) AS ROWNUM
	            FROM
		            F580055
		            INNER JOIN F580056
		            ON RHZ58RID = RDZ58RID
		            INNER JOIN F4211
		            ON SDDOCO = RDDOCO AND
			            SDDCTO = RDDCTO AND
			            SDLNID = RDLNID AND
			            SDKCOO = RDKCOO
		            INNER JOIN F580032
		            ON SDDOCO = ATDOCO AND
			            SDDCTO = ATDCTO AND
			            SDLNID = ATLNID AND
			            SDKCOO = ATKCOO
		            INNER JOIN F3201 
		            ON SDDOCO = CMDOCO AND
			            SDDCTO = CMDCTO AND
			            SDLNID = CMLNID AND
			            SDKCOO = CMKCOO
		            INNER JOIN F3211
		            ON KZCFGID = CMCFGID AND
			                KZPEL = 40310
		            INNER JOIN F0101 AB1
		            ON AB1.ABAN8 = SDAN8
		            INNER JOIN F0101 AB2
		            ON AB2.ABAN8 = SDSHAN
	            WHERE
	                RHZ58HST IN ('1','2','4','5') AND
		            RHMCU = ? AND
		            SDAN8 = ?) A
            WHERE
	            A.ROWNUM = 1
            GROUP BY
	            SDAN8,
	            AN8ALPH,
	            SDSHAN,
	            SHANALPH,
	            KZSGVL
            ORDER BY 1,2,3", 116193);
            cmd.Parameters.Add(new iDB2Parameter("DRQJ", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DRQJ"].Precision = 6;
            cmd.Parameters["DRQJ"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
            cmd.Parameters["AN8"].Precision = 8;
            cmd.Parameters["AN8"].Scale = 0;
            return cmd;
        }

        public RacksByProductSet GetRacksByProduct(BusinessSystemSession session, Plant plant, decimal customerNum)
        {
            var facts = new RacksByProductSet();
            try
            {
                var cmd = GetRacksByProduct();
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = E1TimeManager.CastToE1DateId(DateTime.Now);
                cmd.Parameters[1].Value = plant.PlantNum.PadLeft(12);
                cmd.Parameters[2].Value = customerNum;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var fact = new RacksByProduct();
                        fact.Plant = plant.PlantNum;
                        fact.CustomerSoldNum = CastDbNull.To<Decimal>(reader["SDAN8"], 0);
                        fact.CustomerSoldName = CastDbNull.To<String>(reader["AN8ALPH"], String.Empty).Trim();
                        fact.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        fact.CustomerShipName = CastDbNull.To<String>(reader["SHANALPH"], String.Empty).Trim();
                        //fact.RackStatus = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                        fact.CardinalProductType = CastDbNull.To<String>(reader["KZSGVL"], String.Empty).Trim();
                        fact.OpenRacksThruToday = CastDbNull.To<int>(reader["OPEN_RACKS_THRU_TODAY"], 0);
                        fact.OpenUnitsThruToday = CastDbNull.To<int>(reader["OPEN_UNITS_THRU_TODAY"], 0);
                        fact.OpenRacksAll = CastDbNull.To<int>(reader["OPEN_RACKS_ALL"], 0);
                        fact.OpenUnitsAll = CastDbNull.To<int>(reader["OPEN_UNITS_ALL"], 0);
                        fact.ClosedRacks = CastDbNull.To<int>(reader["CLOSED_RACKS"], 0);
                        fact.ClosedUnits = CastDbNull.To<int>(reader["CLOSED_UNITS"], 0);
                        fact.LockedRacks = CastDbNull.To<int>(reader["LOCKED_RACKS"], 0);
                        fact.LockedUnits = CastDbNull.To<int>(reader["LOCKED_UNITS"], 0);
                        fact.TotalRacks = CastDbNull.To<int>(reader["TOTAL_RACKS"], 0);
                        fact.TotalUnits = CastDbNull.To<int>(reader["TOTAL_UNITS"], 0);
                        facts.Add(fact);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return facts;
        }

    }
}
