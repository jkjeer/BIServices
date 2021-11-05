using System;
using System.Collections.Generic;
using System.Data;
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
    public class ShippingManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ShippingManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("ShippingManager.GetCost", GetCost());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
        * Shipped Racks
        *******************************************************************************/
        public static iDB2Command GetShippedRacks(List<string> filters)
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
                LHMCU,
                LHLDNM,
	            LHSHAN,
	            LHBOLN,
	            LHLDDT,
	            LHZ58TMLC,
	            LHZ58TN,
	            CASE WHEN LHSHAN = 10069785 THEN SUBSTR(SDVR01, 1, LENGTH(TRIM(SDVR01)) - 5) ELSE SDVR01 END AS SDVR01,
	            RHZ58RT,
	            RHZ58RID,
                SDURCD,
	            SUM(RDPQOH) AS RDPQOH
            FROM
	            F580150
	            INNER JOIN F580055
	            ON RHPSN = LHLDNM AND
		            RHMCU = LHMCU 
	            INNER JOIN F580056
	            ON RDZ58RID = RHZ58RID
	            INNER JOIN F4211 
	            ON SDKCOO = RDKCOO AND
		            SDDOCO = RDDOCO AND
		            SDDCTO = RDDCTO AND
		            SDLNID = RDLNID
            WHERE
                {filterString} 1=1
            GROUP BY
                LHMCU,
                LHLDNM,
	            LHSHAN,
	            LHBOLN,
	            LHLDDT,
	            LHZ58TMLC,
	            LHZ58TN,
	            CASE WHEN LHSHAN = 10069785 THEN SUBSTR(SDVR01, 1, LENGTH(TRIM(SDVR01)) - 5) ELSE SDVR01 END,
	            RHZ58RT,
	            RHZ58RID,
                SDURCD
            ORDER BY 1,2,3,4,5,6
            FOR READ ONLY";
            return cmd;
        }

        public static Racks GetShippedRacks(BusinessSystemSession session, string plants, decimal customerNum, string dateCode, string startDate, string endDate)
        {
            var racks = new Racks();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                var plantsList = plants.Replace("'", "").Split(',').ToList();
                string strInClause = "";
                foreach (string plantItem in plantsList)
                {
                    var plant = PlantManager.GetPlant(plantItem);
                    strInClause = strInClause + "?,";
                    parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                    parameters.Last().Value = plant.PlantNum.PadLeft(12);
                }
                filters.Add("LHMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");

                // Customer Num
                if (customerNum > 0)
                {
                    filters.Add("LHSHAN = ? AND\r\n");
                    parameters.Add(new iDB2Parameter("SHAN", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 8;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = customerNum;
                }

                // Date Code
                if (!String.IsNullOrEmpty(dateCode))
                {
                    var dts = DateCodeManager.GetDateCodeRange(dateCode);
                    filters.Add("(LHLDDT >= ? AND LHLDDT <= ?) AND\r\n");

                    parameters.Add(new iDB2Parameter("StartDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = E1TimeManager.CastToE1DateId(dts[0]);

                    parameters.Add(new iDB2Parameter("EndDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = E1TimeManager.CastToE1DateId(dts[1]);
                }

                // Dates
                if (!String.IsNullOrEmpty(startDate))
                {
                    filters.Add("(LHLDDT >= ? AND LHLDDT <= ?) AND\r\n");

                    parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = E1TimeManager.CastToE1DateId(Convert.ToDateTime(startDate));

                    parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = E1TimeManager.CastToE1DateId(Convert.ToDateTime(endDate));
                }


                // Command
                cmd = GetShippedRacks(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rack = new Rack();
                        rack.PlantNum = CastDbNull.To(reader["LHMCU"], string.Empty).Trim();
                        rack.CustomerNum = CastDbNull.To<decimal>(reader["LHSHAN"], 0);
                        rack.LoadNum = CastDbNull.To<decimal>(reader["LHLDNM"], 0);
                        rack.LoadConfirmDateTime = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["LHLDDT"], 0),
                            CastDbNull.To<decimal>(reader["LHZ58TMLC"], 0));
                        rack.CustomerPO = CastDbNull.To(reader["SDVR01"], string.Empty).Trim();
                        rack.TrailerNum = CastDbNull.To(reader["LHZ58TN"], string.Empty).Trim();
                        rack.BillOfLadingNum = CastDbNull.To<decimal>(reader["LHBOLN"], 0);
                        rack.RackId = CastDbNull.To<decimal>(reader["RHZ58RID"], 0);
                        rack.RackType = CastDbNull.To(reader["RHZ58RT"], string.Empty).Trim();
                        rack.Quantity = CastDbNull.To<decimal>(reader["RDPQOH"], 0) / 1000;
                        rack.ProcessingPriority = CastDbNull.To(reader["SDURCD"], string.Empty).Trim();
                        racks.Add(rack);
                    }

                    reader.Close();
                }
                _logger.Info($"{i} Rows");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }
    }
}