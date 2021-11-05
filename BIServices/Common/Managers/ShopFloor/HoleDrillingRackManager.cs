using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;

namespace BIServices.Common.Managers.ShopFloor
{
    public static class HoleDrillingRackManager
    {


        public static SqlCommand GetHoleDrillingRacks()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                ord.ScheduleNum,
                ub.UnitID,
                bh.Rack2,
                bh.Slot2,
                ord.WOrderNum,
                ord.DueDate,
                ord.CustName,
                ub.IGLine,
                ord.Lite2_Thick,
                ord.Lite2_Color,
                ord.Lite2_Heat,
                ord.Lite2_3rdSrfCoat,
                ord.Lite2_4thSrfCoat,
                ord.Lite2_Pattern,
                ord.Lite2_Lami,
                ord.Lite2_Routing,
                ord.ShapeNum,
                ord.Lite1_Base,
                ord.Lite1_Left
            FROM
                dbo.IncomingOrders ord
                INNER JOIN dbo.UnitBox ub
                ON  ub.OrderItem = ord.WOrderNum
                INNER JOIN dbo.BoxHold bh
                ON  bh.WOrderNum = ub.OrderItem AND
                    bh.UnitID = ub.UnitID
            WHERE
                ord.Lite3_Thick > 0 AND
                ord.Lite2_Holes != '' AND
	            ord.Lite2_Heat = 'A' AND
                ord.DueDate > GETDATE() - 7 AND
                NOT EXISTS
            (
                SELECT
                    1
                FROM
                    ProdData pd
                WHERE
                    pd.OrderNum = ord.WOrderNum AND
                    pd.UnitID = ub.UnitID
            )
            ORDER BY
                1,
                2,
                3;";
            return cmd;
        }

        public static GlassUnits GetHoleDrillingRacks(SqlConnection cn, Plant plant)
        {
            var units = new GlassUnits();
            try
            {
                var cmd = GetHoleDrillingRacks();
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var unit = new GlassUnit();
                        unit.ScheduleNum = CastDbNull.To<decimal>(reader["ScheduleNum"], 0);
                        unit.UnitId = Convert.ToDecimal(CastDbNull.To<int>(reader["UnitID"], 0));
                        unit.WorkOrderNum = CastDbNull.To<decimal>(reader["WOrderNum"], 0);
                        unit.RequestedDate = CastDbNull.To<DateTime>(reader["DueDate"], DateTime.MinValue);
                        unit.CustomerName = CastDbNull.To<String>(reader["CustName"], string.Empty).Trim();
                        unit.WipRack2 = CastDbNull.To<String>(reader["Rack2"], String.Empty).Trim();
                        unit.WipSlot2 = CastDbNull.To<int>(reader["Slot2"], 0);
                        units.Add(unit);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return units;
        }
    }
}
