using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.QuerySvc
{
    public class RackEventManagerLG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public RackEventManagerLG(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("OpenUnitManagerLGLocal.GetScheduledRack", GetScheduledRack());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
         * Get Rack Events
         *******************************************************************************/
        public SqlCommand GetRackEventsByHour(List<string> filters)
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
                PVT.TransactionDate,
                PVT.TransactionHour,
                PVT.UnitTypeCode,
                PVT.CLOSED,
                PVT.LOCKED
            FROM
                (
                    SELECT
                        A.RackId,
                        --A.StatusCode,
                        A.StatusDesc,
                        CAST(A.TransactionDate AS DATE)   AS TransactionDate,
                        DATEPART(HOUR, A.TransactionDate) AS TransactionHour,
                        MAX(ord.UnitTypeCode)             AS UnitTypeCode
                    FROM
                        (
                            SELECT
                                rl.RackId,
                                rl.RackType,
                                rl.StatusCode,
                                CASE
                                    WHEN rl.StatusCode = '4' THEN
                                        'CLOSED'
                                    WHEN rl.StatusCode = '5' THEN
                                        'LOCKED'
                                    ELSE
                                        ''
                                END                     AS StatusDesc,
                                MAX(rl.TransactionDate) AS TransactionDate
                            FROM
                                ShopFloor.EBS.RackArchive rl
                            WHERE
                                {filterString}
                                --rl.TransactionDate > GETDATE() - 7 AND
                                rl.StatusCode IN (
                                                     '4', '5'
                                                 )
                            GROUP BY
                                rl.RackType,
                                rl.RackId,
                                rl.StatusCode
                        )                                              A
                        INNER JOIN ShopFloor.EBS.RackDetail              ri
                        ON  ri.RackId = A.RackId AND
                            ri.WOrderNum > 0
                        LEFT OUTER JOIN ShopFloor.EBS.ScheduledOrders ord
                        ON  ord.WorkOrderNum = ri.WOrderNum
                    GROUP BY
                        A.RackId,
                        A.StatusCode,
                        A.StatusDesc,
                        CAST(A.TransactionDate AS DATE),
                        DATEPART(HOUR, A.TransactionDate)
                    HAVING
                        SUM(ri.QtyOnHand) > 0
                ) src
            PIVOT
                (
                    COUNT(RackId)
                    FOR StatusDesc IN (
                                          [CLOSED], [LOCKED]
                                      )
                ) AS PVT
            ORDER BY
                1,
                2;";
            return cmd;
        }

        public RackEventHourSummarySet GetRackEventsByHour(SqlConnection cnShopFloor, Plant plant, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var rackEvents = new RackEvents();
            var rackHourSummarySet = new RackEventHourSummarySet();
            List<String> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();

            try
            {
                Hashtable dateRange = new Hashtable();
                ;
                if (dateCode.ToUpper().Equals("RANGE"))
                {
                    dateRange.Add("StartDate", (DateTime) startDate);
                    dateRange.Add("EndDate", (DateTime) endDate);
                }
                else
                {
                    var dateArr = DateCodeManager.GetDateCodeRange(dateCode);
                    dateRange.Add("StartDate", dateArr[0]);
                    dateRange.Add("EndDate", dateArr[1].AddDays(1));
                }

                //var enterpriseStartDate = (DateTime) dateRange["StartDate"];
                //var enterpriseEndDate = (DateTime) dateRange["EndDate"];

                //var plantStartDateTime = enterpriseStartDate.AddHours(-plant.TimeOffset);
                //var plantEndDateTime = enterpriseEndDate.AddDays(1).AddHours(-plant.TimeOffset);

                filters.Add("rl.TransactionDate >= @StartDate AND rl.TransactionDate <= @EndDate AND\r\n");
                parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
                parameters.Last().Value = (DateTime)dateRange["StartDate"];
                parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
                parameters.Last().Value = (DateTime)dateRange["EndDate"];

                var cmd = GetRackEventsByHour(filters);
                cmd.Connection = cnShopFloor;

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
                        var rackEvent = new RackEventHourSummary();
                        rackEvent.Plant = plant.PlantCode;
                        rackEvent.EventDate =
                            CastDbNull.To<DateTime>(reader["TransactionDate"], (DateTime) SqlDateTime.MinValue);
                        rackEvent.EventHour = CastDbNull.To<int>(reader["TransactionHour"], 0);
                        rackEvent.UnitTypeCode = CastDbNull.To<string>(reader["UnitTypeCode"], String.Empty).Trim();
                        rackEvent.ClosedRacks = CastDbNull.To<int>(reader["CLOSED"], 0);
                        rackEvent.LockedRacks = CastDbNull.To<int>(reader["LOCKED"], 0);
                        rackEvent.LockedLoadRacks = 0;
                        rackHourSummarySet.Add(rackEvent);
                    }

                    reader.Close();
                }
            }  catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return rackHourSummarySet;
        }
    }
}
