using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class ShippingLaneManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Dictionary<decimal, ShippingLane> GetCustomerLane(SqlConnection cn)
        {
            var dict = new Dictionary<decimal, ShippingLane>();
            try
            {
                var cmd = GetCustomerLane();
                cmd.Connection = cn;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var shan = CastDbNull.To<decimal>(reader["CustomerShipNum"], 0);
                        var lane = new ShippingLane();
                        lane.Name = CastDbNull.To<string>(reader["ShipLane"], string.Empty).Trim();
                        lane.Offset = CastDbNull.To<int>(reader["Offset"], 0);
                        dict.Add(shan, lane);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return dict;
        }

        public static SqlCommand GetCustomerLane()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT DISTINCT
	            CustomerShipNum,
	            ShipLane,
	            Offset
            FROM 
	            ShopFloor.EBS.CustomerShipLane
            ORDER BY 1;";
            return cmd;
        }
    }
}
