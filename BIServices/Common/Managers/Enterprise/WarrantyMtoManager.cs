using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class WarrantyMtoManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public WarrantyMtoManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        public SqlCommand GetLvUnit()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                CT.J_PLANT_SRCCD ,
                CT.J_SERIAL_NO ,
                CT.J_WORK_ORDER_NO ,
                WO.ORDER_DT ,
                WO.REQUESTED_DT ,
                WO.SCHEDULE_DT ,
                WO.SCHEDULE_NO ,
                WO.CUSTOMER_NO ,
                WO.ITEM_NO ,
                CS1.CUSTOMER_NAME ,
                SL.J_CUSTOMER_SHIP_TO_NO ,
                CS2.CUSTOMER_NAME AS CUSTOMER_SHIP_NAME ,
				SL.J_DOCUMENT_NO,
				SL.J_DOCUMENT_TYPE_SRCCD,
				SL.J_DOCUMENT_LINE_NO,
                SL.J_ACTUAL_SHIP_DATE ,
                SL.J_CUSTOMER_PART_NUMBER_1 ,
                SL.J_CUSTOMER_PART_NUMBER_2 ,
                SL.J_CUSTOMER_PART_NUMBER_3 ,
                SL.J_PRODUCT_TYPE ,
                SL.J_CUSTOMER_PO_NO,
                FD.DIMENSION ,
                FD.OVERALL_THICK ,
                FD.GRID ,
                FD.SURFACE ,
                FD.PROCESSING ,
                FD.LOGO ,
                MIN(CT.J_TRANSACTION_DATE) AS MIN_TRAN_DT ,
                MAX(CT.J_TRANSACTION_DATE) AS MAX_TRAN_DT ,
                SUM(CASE WHEN CT.J_LINE_TYPE_SRCCD = 'CUTLV' AND
                              CT.J_RECORD_TYPE_SRCCD = 'PRODUCED' THEN 1
                         ELSE 0
                    END) AS CUT_LITES ,
                SUM(CASE WHEN CT.J_LINE_TYPE_SRCCD = 'TEMPER' AND
                              CT.J_RECORD_TYPE_SRCCD = 'PRODUCED' THEN 1
                         ELSE 0
                    END) AS TEMP_LITES 
              FROM
                dbo.PS_JWF_MANUFACTURING_LV_CTO CT WITH ( NOLOCK )
                LEFT OUTER JOIN DW.WORK_ORDER WO WITH ( NOLOCK )
                  ON WO.WORK_ORDER_NO = CT.J_WORK_ORDER_NO
                LEFT OUTER JOIN dbo.PS_JWF_SALES_LINE_E1 SL WITH ( NOLOCK )
                  ON SL.J_SALES_LINE_ID = WO.SALES_LINE_ID
                LEFT OUTER JOIN DW1.ITEM_ATT_FLEX_DESC FD WITH ( NOLOCK )
                  ON FD.ITEM_REFERENCE_ID = WO.ITEM_REFERENCE_ID
                LEFT OUTER JOIN DW.CUSTOMER CS1 WITH ( NOLOCK )
                  ON CS1.CUSTOMER_NO = CT.J_CUSTOMER_NUM
                LEFT OUTER JOIN DW.CUSTOMER CS2 WITH ( NOLOCK )
                  ON CS2.CUSTOMER_NO = SL.J_CUSTOMER_SHIP_TO_NO
              WHERE
                CT.J_SERIAL_NO = @SerialNo AND
                CT.J_PLANT_SRCCD = @Plant AND
                CT.J_LEGACYYN = 'N'
              GROUP BY
                CT.J_PLANT_SRCCD ,
                CT.J_SERIAL_NO ,
                CT.J_WORK_ORDER_NO ,
                WO.ORDER_DT ,
                WO.REQUESTED_DT ,
                WO.SCHEDULE_DT ,
                WO.SCHEDULE_NO ,
                WO.CUSTOMER_NO ,
                WO.ITEM_NO ,
                CS1.CUSTOMER_NAME ,
                SL.J_CUSTOMER_SHIP_TO_NO ,
                CS2.CUSTOMER_NAME ,
				SL.J_DOCUMENT_NO,
				SL.J_DOCUMENT_TYPE_SRCCD,
				SL.J_DOCUMENT_LINE_NO,
                SL.J_ACTUAL_SHIP_DATE ,
                SL.J_CUSTOMER_PART_NUMBER_1 ,
                SL.J_CUSTOMER_PART_NUMBER_2 ,
                SL.J_CUSTOMER_PART_NUMBER_3 ,
                SL.J_PRODUCT_TYPE ,
                SL.J_CUSTOMER_PO_NO,
                FD.DIMENSION ,
                FD.OVERALL_THICK ,
                FD.GRID ,
                FD.SURFACE ,
                FD.PROCESSING ,
                FD.LOGO;";
            cmd.Parameters.Add(new SqlParameter("@SerialNo", SqlDbType.VarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@Plant", SqlDbType.VarChar, 12));
            return cmd;
        }

        public ProducedUnits GetLvUnit(BusinessSystemSession session, SqlConnection cn, Plant plant, decimal unitId)
        {
            var units = new ProducedUnits();
            try
            {
                var cmd = GetLvUnit();
                cmd.Parameters["@SerialNo"].Value = unitId.ToString();
                cmd.Parameters["@Plant"].Value = plant.PlantNum;
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var unit = new ProducedUnit();
                        unit.Plant = CastDbNull.To<string>(reader["J_PLANT_SRCCD"], string.Empty).Trim();
                        unit.UnitId = Common.Utilities.Common.CastStringToDecimal(CastDbNull.To<string>(reader["J_SERIAL_NO"], string.Empty).Trim());
                        unit.WorkOrderNum = CastDbNull.To<decimal>(reader["J_WORK_ORDER_NO"], 0);
                        unit.ScheduleNum = CastDbNull.To<decimal>(reader["SCHEDULE_NO"], 0);
                        unit.OrderNum = CastDbNull.To<decimal>(reader["J_DOCUMENT_NO"], 0);
                        unit.OrderType = CastDbNull.To<string>(reader["J_DOCUMENT_TYPE_SRCCD"], string.Empty).Trim();
                        unit.OrderLineNum = CastDbNull.To<decimal>(reader["J_DOCUMENT_LINE_NO"], 0);
                        unit.CustomerNum = CastDbNull.To<decimal>(reader["CUSTOMER_NO"], 0);
                        unit.CustomerName = CastDbNull.To<string>(reader["CUSTOMER_NAME"], string.Empty).Trim();
                        unit.CustomerShipNum = CastDbNull.To<decimal>(reader["J_CUSTOMER_SHIP_TO_NO"], 0);
                        unit.CustomerShipName = CastDbNull.To<string>(reader["CUSTOMER_SHIP_NAME"], string.Empty).Trim();
                        unit.CustomerOrderNum = CastDbNull.To<string>(reader["J_CUSTOMER_PO_NO"], string.Empty).Trim();
                        unit.CustomerPart1 = CastDbNull.To<string>(reader["J_CUSTOMER_PART_NUMBER_1"], string.Empty).Trim();
                        unit.CustomerPart2 = CastDbNull.To<string>(reader["J_CUSTOMER_PART_NUMBER_2"], string.Empty).Trim();
                        unit.CustomerPart3 = CastDbNull.To<string>(reader["J_CUSTOMER_PART_NUMBER_3"], string.Empty).Trim();
                        unit.ProductType = CastDbNull.To<string>(reader["J_PRODUCT_TYPE"], string.Empty).Trim();
                        unit.RequestedDate = CastDbNull.To<DateTime>(reader["REQUESTED_DT"], new DateTime(1900, 1, 1));
                        unit.ScheduleDate = CastDbNull.To<DateTime>(reader["SCHEDULE_DT"], new DateTime(1900, 1, 1));
                        unit.MinProductionDate = CastDbNull.To<DateTime>(reader["MIN_TRAN_DT"], new DateTime(1900, 1, 1));
                        unit.MaxProductionDate = CastDbNull.To<DateTime>(reader["MAX_TRAN_DT"], new DateTime(1900, 1, 1));
                        unit.ShipDate = CastDbNull.To<DateTime>(reader["J_ACTUAL_SHIP_DATE"], new DateTime(1900, 1, 1));
                        unit.OrderDate = CastDbNull.To<DateTime>(reader["ORDER_DT"], new DateTime(1900, 1, 1));
                        unit.Dimension = CastDbNull.To<string>(reader["DIMENSION"], string.Empty).Trim();
                        unit.Construction = CastDbNull.To<string>(reader["OVERALL_THICK"], string.Empty).Trim();
                        unit.Routing = CastDbNull.To<string>(reader["PROCESSING"], string.Empty).Trim();
                        unit.CutQty = CastDbNull.To<int>(reader["CUT_LITES"], 0);
                        unit.TemperQty = CastDbNull.To<int>(reader["TEMP_LITES"], 0);
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
