using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;

namespace BIServices.QuerySvc
{
    public class RacksByLaneManager
    {

        public RacksByLaneManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("RacksByLaneManager.GetLockLoad", GetLockLoad());
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
        public iDB2Command GetRacksByLane(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
                SDMCU,
	            SDDRQJ,
	            SDAN8,
	            SDSHAN,
	            S1Z58SCTYP,
	            RHZ58HST,
	            RHLOCN,
	            RHZ58RT,
                RHLOCN,
	            RHZ58RID,
	            AB1.ABALPH AS ABALPH1,
	            AB2.ABALPH AS ABALPH2,
	            CAST(COUNT(DISTINCT RDZ58RID) AS INT) AS RACKS,
	            CAST(SUM(CASE WHEN RDZ58DST = '1' THEN RDZ58CQTY / 1000 ELSE RDPQOH / 1000 END) AS INT) AS UNITS
            FROM 
	            F4211 
	            INNER JOIN F580056
	            ON RDDOCO = SDDOCO AND
		            RDDCTO = SDDCTO AND
		            RDLNID = SDLNID AND
		            RDKCOO = SDKCOO
	            INNER JOIN F580055
	            ON RHZ58RID = RDZ58RID
	            INNER JOIN F4801
	            ON WADOCO = RDDOC1 AND
		            WADCTO = RDXDCTO
	            INNER JOIN F5834A11
	            ON S5DOCO = WADOCO 
	            INNER JOIN F5834A01
	            ON	S1MCU = S5MCU AND 
		            S1Z58SCN = S5Z58SCN AND 
		            S1Z58SCD = S5Z58SCD
	            INNER JOIN F0101 AB1
	            ON AB1.ABAN8 = SDAN8
	            INNER JOIN F0101 AB2
	            ON AB2.ABAN8 = SDSHAN
            WHERE
                {0}
	            SDRORN != '' AND
	            SDLTTR != '980'
            GROUP BY
                SDMCU,
	            SDDRQJ,
	            SDAN8,
	            SDSHAN,
	            S1Z58SCTYP,
	            RHZ58HST,
	            RHLOCN,
	            RHZ58RT,
                RHLOCN,
	            RHZ58RID,
	            AB1.ABALPH,
	            AB2.ABALPH", filterString.ToString());
            return cmd;
        }

        public RacksByLaneSet GetRacksByLane(BusinessSystemSession session, SqlConnection cnDw, Plant plant, string dateCode = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var racks = new RacksByLaneSet();
            var cmd = new iDB2Command();
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant
                filters.Add("SDMCU = ? AND\r\n");
                parameters.Add(new iDB2Parameter("PlantNum1", iDB2DbType.iDB2Graphic, 12));
                parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Max Requested Date
                if (String.IsNullOrEmpty(dateCode) && startDate == null)
                {
                    dateCode = "TDAY";
                }

                if (!String.IsNullOrEmpty(dateCode))
                {
                    dateCode = dateCode.Replace(",", "").Replace("'", "");
                    var dates = GetRequestedDate(dateCode, cnDw);

                    filters.Add("SDDRQJ >= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("RequestedDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(dates["StartDate"]);

                    filters.Add("SDDRQJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("RequestedDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date(dates["EndDate"]);
                }

                // Start End Date
                if (startDate != null && String.IsNullOrEmpty(dateCode))
                {
                    filters.Add("SDDRQJ >= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("StartDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)startDate);

                    filters.Add("SDDRQJ <= ? AND\r\n");
                    parameters.Add(new iDB2Parameter("EndDateId", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Common.Utilities.Common.CastToE1Date((DateTime)endDate);
                }

                cmd = GetRacksByLane(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rack = new RacksByLaneRecord();
                        rack.Plant = CastDbNull.To<String>(reader["SDMCU"], String.Empty).Trim();
                        rack.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
                        rack.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
                        rack.Location = CastDbNull.To<String>(reader["RHLOCN"], String.Empty).Trim();
                        rack.RackTypeGroup = GetRackTypeGroup(rack.RackType);
                        rack.Status = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                        rack.ScheduleType = CastDbNull.To<String>(reader["S1Z58SCTYP"], String.Empty).Trim();
                        var dateId1 = CastDbNull.To<Decimal>(reader["SDDRQJ"], 0);
                        rack.RequestedDate = Common.Utilities.Common.CastToDateTime(dateId1, 0);
                        rack.CustomerNum = CastDbNull.To<Decimal>(reader["SDAN8"], 0);
                        rack.CustomerName = CastDbNull.To<String>(reader["ABALPH1"], String.Empty).Trim();
                        rack.CustomerShipNum = CastDbNull.To<Decimal>(reader["SDSHAN"], 0);
                        rack.CustomerShipName = CastDbNull.To<String>(reader["ABALPH2"], String.Empty).Trim();
                        rack.Racks = CastDbNull.To<int>(reader["RACKS"], 0);
                        rack.Units = CastDbNull.To<int>(reader["UNITS"], 0);
                        racks.Add(rack);
                    }

                    reader.Close();
                }

                if (racks.Count > 0)
                {
                    GetCustomerAttributes(racks, cnDw);
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return racks;
        }

        /*********************************************************************************
         * Last Line
         *******************************************************************************/
        public SqlCommand GetCustomerAttributes(List<Decimal> custNums)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              CUSTOMER_NO ,
              SL.SHIPPING_LANE_ID,
              SL.SHIPPING_LANE_NAME
            FROM
              DW.CUSTOMER CS WITH (NOLOCK)
              INNER JOIN DW.SHIPPING_LANE SL WITH (NOLOCK)
                ON SL.SHIPPING_LANE_ID = CS.SHIPPING_LANE_ID
            WHERE 
	            CUSTOMER_NO IN ({0})", String.Join(",", custNums));
            return cmd;
        }

        public void GetCustomerAttributes(RacksByLaneSet racks, SqlConnection cn)
        {
            try
            {
                var custNums = racks.Select(x => x.CustomerShipNum).Distinct().ToList();
                var cmd = GetCustomerAttributes(custNums);
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, Hashtable>();
                    while (reader.Read())
                    {
                        var custNum = Convert.ToDecimal(CastDbNull.To<decimal>(reader["CUSTOMER_NO"], 0));
                        var custAttributes = new Hashtable
                        {
                            { "ShippingLaneId", CastDbNull.To<int>(reader["SHIPPING_LANE_ID"], 0) },
                            { "ShippingLaneName", CastDbNull.To<String>(reader["SHIPPING_LANE_NAME"], String.Empty).Trim() }
                        };
                        dict.Add(custNum, custAttributes);
                    }

                    reader.Close();

                    foreach (RacksByLaneRecord rack in racks)
                    {
                        var custAttributesRec = CollectionUtils.GetValueOrDefault(dict, rack.CustomerShipNum);
                        if (custAttributesRec != null)
                        {
                            rack.ShippingLaneId = (int) custAttributesRec["ShippingLaneId"];
                            rack.ShippingLaneName = (string) custAttributesRec["ShippingLaneName"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        /*********************************************************************************
         * Rack Type Group
         *******************************************************************************/
        public string GetRackTypeGroup(string code)
        {
            string groupName = "OTHER";
            if (code.StartsWith("B"))
            {
                groupName = "BUNGEE";
            }
            else
            {
                if (code.StartsWith("A"))
                {
                    groupName = "AFRAME";
                }
                else
                {
                    if (code.StartsWith("IG"))
                    {
                        groupName = "IG";
                    }
                    else
                    {
                        if (code.StartsWith("EXCPT"))
                        {
                            groupName = "EXCPT";
                        }           
                    }                
                }
            }
            return groupName;
        }

        /*********************************************************************************
         * Requested Date
         *******************************************************************************/
        public SqlCommand GetRequestedDate()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              MIN(CAL_DATE) AS START_DATE,
              MAX(CAL_DATE) AS END_DATE
            FROM
              DW.CALENDAR_DATE_TYPE CDT
              INNER JOIN DW.CALENDAR_DATE_ATTRIBUTES CDA
              ON CDA.CAL_DATE_TYPE_ID = CDT.DATE_TYPE_ID
            WHERE
              CDT.DATE_TYPE_CD = @DateCode;";
            cmd.Parameters.Add(new SqlParameter("@DateCode", SqlDbType.VarChar, 30));
            return cmd;
        }

        public Dictionary<string, DateTime> GetRequestedDate(string code, SqlConnection cn)
        {
            var dict = new Dictionary<string, DateTime>();
            try
            {
                var cmd = GetRequestedDate();
                cmd.Connection = cn;
                cmd.Parameters[0].Value = code;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        dict.Add("StartDate", CastDbNull.To<DateTime>(reader["START_DATE"], new DateTime(1900, 1, 1)));
                        dict.Add("EndDate", CastDbNull.To<DateTime>(reader["END_DATE"], new DateTime(1900, 1, 1)));
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }

            return dict;
        }
    }
}
