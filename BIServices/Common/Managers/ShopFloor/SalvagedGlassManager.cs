using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using BIServices.Common.Utilities;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public static class SalvagedGlassManager
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /*********************************************************************************
         * Consumption
         *******************************************************************************/
        public static SqlCommand GetLites()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
        SELECT
            *
        FROM
            (
                SELECT
                    ps.WorkOrder,
                    ps.UnitID,
                    1 AS LiteNum,
                    ps.LastUpdatedDate,
                    ub.Glass1 AS GlassCode,
		            ub.ShapeNum,
		            ub.Dimensions,
                    ub.ActualArea
                FROM
                    LiteSource.ProdUnitStatus                   ps
                    INNER JOIN PlantReporting.Shopfloor.UnitBox ub
                    ON  ub.UnitID = ps.UnitID AND
                        ub.OrderItem = ps.WorkOrder
                WHERE
                    ps.UnitRejected = 1 AND
                    ps.ExtRejected = 0 
                UNION ALL
                SELECT
                    ps.WorkOrder,
                    ps.UnitID,
                    2 AS LiteNum,
                    ps.LastUpdatedDate,
                    ub.Glass2,
		            ub.ShapeNum,
		            ub.Dimensions,
                    ub.ActualArea
                FROM
                    LiteSource.ProdUnitStatus                   ps
                    INNER JOIN PlantReporting.Shopfloor.UnitBox ub
                    ON  ub.UnitID = ps.UnitID AND
                        ub.OrderItem = ps.WorkOrder
                WHERE
                    ps.UnitRejected = 1 AND
                    ps.IntRejected = 0 AND
                    ub.Glass3 = ''
                UNION ALL
                SELECT
                    ps.WorkOrder,
                    ps.UnitID,
                    2 AS LiteNum,
                    ps.LastUpdatedDate,
                    ub.Glass2,
		            ub.ShapeNum,
		            ub.Dimensions,
                    ub.ActualArea
                FROM
                    LiteSource.ProdUnitStatus                   ps
                    INNER JOIN PlantReporting.Shopfloor.UnitBox ub
                    ON  ub.UnitID = ps.UnitID AND
                        ub.OrderItem = ps.WorkOrder
                WHERE
                    ps.UnitRejected = 1 AND
                    ps.MidRejected = 0 AND
                    ub.Glass3 <> ''
                UNION ALL
                SELECT
                    ps.WorkOrder,
                    ps.UnitID,
                    3 AS LiteNum,
                    ps.LastUpdatedDate,
                    ub.Glass3,
		            ub.ShapeNum,
		            ub.Dimensions,
                    ub.ActualArea
                FROM
                    LiteSource.ProdUnitStatus                   ps
                    INNER JOIN PlantReporting.Shopfloor.UnitBox ub
                    ON  ub.UnitID = ps.UnitID AND
                        ub.OrderItem = ps.WorkOrder
                WHERE
                    ps.UnitRejected = 1 AND
                    ps.IntRejected = 0 AND
                    ub.Glass3 <> '' 
            )     A
               WHERE
                    A.LastUpdatedDate
                    BETWEEN @StartDate AND DATEADD(DAY, 1, @EndDate)
	        ORDER BY 1,2,3";
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime));
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime));
            return cmd;
        }

        public static ProdLites GetLites(SqlConnection cn, Plant plant, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new ProdLites();
            try
            {
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dtArray = DateCodeManager.GetDateCodeRange(dateCode);
                    startDate = dtArray[0];
                    endDate = dtArray[1];
                }

                var cmd = GetLites();
                cmd.Connection = cn;
                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@EndDate"].Value = endDate;
                cmd.CommandTimeout = 60;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var lite = new ProdLite();
                        lite.Plant = plant.PlantCode;
                        lite.WorkOrderNum = (decimal) CastDbNull.To<int>(reader["WorkOrder"], 0);
                        lite.UnitId = CastDbNull.To<int>(reader["UnitId"], 0);
                        lite.LiteNum = CastDbNull.To<int>(reader["LiteNum"], 0);
                        lite.TransactionDate = CastDbNull.To<DateTime>(reader["LastUpdatedDate"], DateTime.MinValue);
                        lite.ActualSqft = (decimal) CastDbNull.To<double>(reader["ActualArea"], 0);
                        lite.ProductCode = CastDbNull.To<String>(reader["GlassCode"], String.Empty).Trim();
                        lite.ShapeNum = CastDbNull.To<int>(reader["ShapeNum"], 0);
                        lite.Dimension = CastDbNull.To<String>(reader["Dimensions"], String.Empty).Trim();
                        lite.Quantity = (int) 1;
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


    }
}
