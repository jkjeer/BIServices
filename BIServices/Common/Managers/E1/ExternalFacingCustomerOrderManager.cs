using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using BIServices.CustomerSvc;
using IBM.Data.DB2.iSeries;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static BIServices.Common.Utilities.Sql;

namespace BIServices.Common.Managers.E1
{
    public static class ExternalFacingCustomerOrderManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /******************************************************************
        * Get Orders
        ******************************************************************/
        public static ExternalFacingSalesOrderLines GetOpenOrders(BusinessSystemSession session, decimal customerNum, List<string> plants = null,
               string customerPoNum = null, string customerSeqNum = null, string customerPartNum1 = null,
               DateTime? requestedDate = null, string productType = null, decimal scheduleNum = 0, List<string> orderStatus = null,
               string priorityCode = null, string genericSeq = null, string customerPartNum2 = null, string customerPartNum3 = null,
               string message1 = null, string message2 = null, string message3 = null, string message4 = null, string message5 = null,
                decimal salesOrderNum = 0)
        {
            ExternalFacingSalesOrderLines orders = new ExternalFacingSalesOrderLines();
            var filters = new Sql.SqlFilters();
            var parameters = new List<iDB2Parameter>();
            try
            {
                // Customer List
                if (customerNum > 0)
                {

                    var customerChildNums = CustomerHierarchyManager.GetChildCustomerNums(session, customerNum);
                    if (customerChildNums.Any())
                    {
                        filters.Add(new Sql.SqlFilter($"SDAN8 IN ({string.Concat(Enumerable.Repeat("?,", customerChildNums.Count)).TrimEnd(',')})", "AND"));
                        foreach (var customerChildNum in customerChildNums)
                        {
                            parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                            parameters.Last().Precision = 8;
                            parameters.Last().Scale = 0;
                            parameters.Last().Value = customerChildNum;
                        }
                    }
                }

                // Plants
                if (plants.Any())
                {
                    filters.Add(new Sql.SqlFilter($"SDMCU IN ({string.Concat(Enumerable.Repeat("?,", plants.Count)).TrimEnd(',')})", "AND"));
                    foreach (var plantText in plants)
                    {
                        var plant = PlantManager.GetPlant(plantText);
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                }

                // Order Next Status - Only Open Orders
                filters.Add(new Sql.SqlFilter("SDNXTR NOT IN ('999')", "AND"));

                // External Sales Only
                filters.Add(new Sql.SqlFilter("SDDCTO IN ('SO')", "AND"));

                // Omit Miscellaneous Sales Lines
                filters.Add(new Sql.SqlFilter("SDGLC NOT IN ('MSAL')", "AND"));

                // Command
                var cmd = GetOrders(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                logger.Debug(Logging.DbCmdString(cmd));

                // Reader
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var order = new ExternalFacingSalesOrderLine();
                    order.PlantNumericCode = reader.Get<string>("SDMCU");
                    order.PlantAlphaCode = PlantManager.GetPlant(reader.Get<string>("SDMCU")).PlantCode ?? "";
                    order.CustomerSoldNum = reader.Get<decimal>("SDAN8");
                    order.CustomerSoldName = reader.Get<string>("SOLD_ALPH");
                    order.CustomerShipNum = reader.Get<decimal>("SDSHAN");
                    order.CustomerShipName = reader.Get<string>("SHIP_ALPH");
                    order.CustomerPart1 = reader.Get<string>("ATZ58CP2");
                    order.CustomerPart2 = reader.Get<string>("ATZ58CP3");
                    order.CustomerPart3 = reader.Get<string>("ATZ58CP4");
                    order.ProductType = reader.Get<string>("ATZ58PT");
                    order.CardinalProductType = reader.Get<string>("CARDINALPT");
                    order.CustomerSeqNum = reader.Get<string>("ATZ58CSQNO");
                    order.GenericSeqNum = reader.Get<string>("ATZ58GSQNO");
                    order.CustomerPO = reader.Get<string>("SDVR01");
                    order.PriorityCode = reader.Get<string>("SDURCD");

                    order.SOrderLineType = reader.Get<string>("SDLNTY");
                    order.SOrderNum = reader.Get<decimal>("SDDOCO");
                    order.SOrderType = reader.Get<string>("SDDCTO");
                    order.SOrderLineNum = reader.Get<decimal>("SDLNID") / 1000;
                    order.SOrderLineLastStatus = reader.Get<string>("SDLTTR");
                    order.SOrderLineNextStatus = reader.Get<string>("SDNXTR");
                    order.OrderDate = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDTRDJ"), 0);
                    order.OrderDateString = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDTRDJ"), 0).ToShortDateString();
                    order.DueDate = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDDRQJ"), 0);
                    order.DueDateString = E1TimeManager.CastToDateTime(reader.Get<decimal>("SDDRQJ"), 0).ToShortDateString();

                    order.ScheduleNum = reader.Get<decimal>("S5Z58SCN");
                    order.Units = reader.Get<decimal>("SDUORG") / 1000;
                    order.ActualSqft = (reader.Get<decimal>("ATZ58USF") / 10000) * (reader.Get<decimal>("SDUORG") / 1000);
                    order.BillableSqft = (reader.Get<decimal>("ATZ58BSF") / 10000) * (reader.Get<decimal>("SDUORG") / 1000);
                    order.GlassWeight = reader.Get<decimal>("ATZ58GWTT") / 100;
                    order.WOrderNum = reader.Get<decimal>("ATZ58WODC");
                    order.ItemNum = reader.Get<decimal>("SDITM");
                    order.ItemCategory = reader.Get<string>("IBGLPT");
                    order.ItemStockType = reader.Get<string>("IBSTKT");
                    order.Description1 = reader.Get<string>("SDDSC1");
                    order.Description2 = reader.Get<string>("SDDSC2");
                    order.ConfigurationId = reader.Get<decimal>("CMCFGID");

                    order.UnitTypeCode = (reader.Get<string>("L3TKC") != "") ? "3" : (reader.Get<string>("L3TKD") != "") ? "3" : (reader.Get<string>("L2TKC") != "") ? "2" : (reader.Get<string>("L2TKD") != "") ? "2" : (reader.Get<string>("L1TKC") != "") ? "1" : (reader.Get<string>("L1TKD") != "") ? "1" : "";
                    orders.Add(order);
                }
                reader.Close();

                //foreach (var order in orders.Where(x => x.ItemStockType == "C"))
                //{
                //    var atts = ConfiguredUnitAttributeManager.GetItemAttributes(session, order.ConfigurationId);
                //    order.CardinalProductType = atts.GetStringAttributeValue("Z5840460");
                //}
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            return orders;
        }


        public static iDB2Command GetOrders(SqlFilters filters)
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT 
                SDMCU,
	            SDKCOO,
	            SDDOCO,
	            SDDCTO,
	            SDLNID,
	            SDGLC,
	            SDLNTY,
	            SDLTTR,
	            SDNXTR,
                SDAN8,
	            SDSHAN,
                SDVR01,
                SDVR02,
                SDUORG,
                SDUPRC,
                SDAEXP,
                SDTRDJ,
                SDDRQJ,
                SDDRQT,
                SDDOC,
                SDIVD,
                SDPSN,
                SDITM,
                SDDSC1,
                SDDSC2,
                SDCATNM,
                SDURCD,
	            SDRORN,
	            SDRCTO,
	            SDRLLN,
	            ATZ58WODC,
                ATZ58CP2,
                ATZ58CP3,
                ATZ58CP4,
                ATZ58CSQNO,
                ATZ58GSQNO,
                ATZ58PT,
                ATZ5899011,
                ATZ5899012,
                ATZ58USF,
                ATZ58BSF,
                ATZ58GWTT,
                ATZ5840460,
                SOLD.ABALPH AS SOLD_ALPH,
                SHIP.ABALPH AS SHIP_ALPH,
                IBGLPT,
                IBSTKT,
	            (SELECT S5Z58SCN FROM F5834A11 WHERE S5DOCO = ATZ58WODC AND ATZ58WODC > 0 ORDER BY S5UPMJ DESC FETCH FIRST 1 ROWS ONLY) S5Z58SCN,
                (SELECT CMCFGID FROM F3201 WHERE CMKCOO = SDKCOO AND CMDOCO = SDDOCO AND CMDCTO = SDDCTO AND CMLNID = SDLNID order by CMUPMJ DESC FETCH FIRST 1 ROWS ONLY) AS CMCFGID,
                (SELECT KZSGVL FROM F3201 INNER JOIN F3211 ON KZCFGID = CMCFGID AND KZPEL = 40310 WHERE CMKCOO = SDKCOO AND CMDOCO = SDDOCO AND CMDCTO = SDDCTO AND CMLNID = SDLNID ORDER BY KZUPMJ DESC FETCH FIRST 1 ROWS ONLY) AS CARDINALPT,
                (select KZSGVL from F3201 INNER JOIN F3211 ON KZCFGID = CMCFGID and KZPEL = 330 where CMKCOO = SDKCOO AND CMDOCO = SDDOCO AND CMDCTO = SDDCTO AND CMLNID = SDLNID order by KZUPMJ DESC  FETCH FIRST 1 ROW ONLY) AS L1TKC,
                (select KZSGVL from F3201 INNER JOIN F3211 ON KZCFGID = CMCFGID and KZPEL = 2010 where CMKCOO = SDKCOO AND CMDOCO = SDDOCO AND CMDCTO = SDDCTO AND CMLNID = SDLNID order by KZUPMJ DESC  FETCH FIRST 1 ROW ONLY) AS L2TKC,
                (select KZSGVL from F3201 INNER JOIN F3211 ON KZCFGID = CMCFGID and KZPEL = 3600 where CMKCOO = SDKCOO AND CMDOCO = SDDOCO AND CMDCTO = SDDCTO AND CMLNID = SDLNID order by KZUPMJ DESC  FETCH FIRST 1 ROW ONLY) AS L3TKC,
                (select IADVAL from F580002 where IAZ58ATNM = 'Z58L1TK' AND IAITM = SDITM FETCH FIRST 1 ROW ONLY) AS L1TKD,
                (select IADVAL from F580002 where IAZ58ATNM = 'Z58L2TK' AND IAITM = SDITM FETCH FIRST 1 ROW ONLY) AS L2TKD,
                (select IADVAL from F580002 where IAZ58ATNM = 'Z58L3TK' AND IAITM = SDITM FETCH FIRST 1 ROW ONLY) AS L3TKD
            FROM
	            F4211
                INNER JOIN F4102
                ON  IBITM = SDITM AND
                    IBMCU = SDMCU
	            INNER JOIN F580032
	            ON  ATKCOO = SDKCOO AND
		            ATDOCO = SDDOCO AND
		            ATDCTO = SDDCTO AND
		            ATLNID = SDLNID
	            INNER JOIN F0101 SOLD
	            ON  SOLD.ABAN8 = SDAN8
	            INNER JOIN F0101 SHIP
	            ON  SHIP.ABAN8 = SDSHAN
            WHERE
                {filters}
            FOR READ ONLY";
            return cmd;
        }
    }
}
