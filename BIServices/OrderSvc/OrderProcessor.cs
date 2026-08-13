using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace BIServices.OrderSvc
{
    public class OrderProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SalesOrderLines GetStockOrders(string plantCode, decimal customerNum)
        {
            Logger.Trace($"/GetStockOrders?Plant={plantCode}&CustomerNum={customerNum}");
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var mgr = new OrderManager(session);
                var customerSoldNums = (customerNum > 0) ? customerNum.ToString(CultureInfo.InvariantCulture) : String.Empty;
                lines.AddRange(mgr.GetStockOrders(session, plantCode, customerSoldNums, null, null, null, null, null, null, null, null, null, "OPEN", "Y"));
                lines.AddRange(mgr.GetCustomOrders(session, plantCode, customerSoldNums, null, null, null, null, null, null, null, null, null, "OPEN", "Y"));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lines;
        }

        public SalesOrderLines GetCustomOrders(string plantCode, decimal customerNum)
        {
            Logger.Trace($"/GetCustomOrders?Plant={plantCode}&CustomerNum={customerNum}");
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var mgr = new OrderManager(session);
                var customerSoldNums = (customerNum > 0) ? customerNum.ToString(CultureInfo.InvariantCulture) : String.Empty;
                lines.AddRange(mgr.GetCustomOrders(session, plantCode, customerSoldNums, null, null, null, null, null, null, null, null, null, "OPEN", "Y"));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lines;
        }

        public SalesOrderLines GetStaleOrders(string plantCode, string dateProperty, string dateCode, string startDate, string endDate)
        {
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                lines.AddRange(StaleOrderManager.GetStaleOrders(session, plantCode, dateProperty, dateCode, startDate, endDate));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lines;
        }

        public SalesOrderLines GetOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
        {
            Logger.Trace($"/GetOrders?Plant={plants}&CustomerSoldNums={customerSoldNums}&CustomerShipNums={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockTypes={stockTypes}&LastStatusCodes={lastStatusCodes}&NextStatusCodes={nextStatusCodes}&DocumentTypes={documentTypes}&Options={options}&Racks={racks}");
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var mgr = new OrderManager(session);
                lines.AddRange(mgr.GetStockOrders(session, plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks));
                lines.AddRange(mgr.GetCustomOrders(session, plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lines;
        }

        public SalesOrderLines GetAgcOpenOrders(string plantCodes, string customerSoldNums)
        {
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            var sqlCns = new Dictionary<string, SqlConnection>();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var cnPlantDb1 = ConnectionManager.GetSqlConnectionFromKey("PTFG.FGWeb");
                cnPlantDb1.Open();
                sqlCns.Add("PLANT.FGWEB", cnPlantDb1);
                var mgr = new AgcOpenOrderManager(session);
                lines.AddRange(mgr.GetOpenOrders(session, sqlCns, plantCodes, customerSoldNums));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
                foreach (var sqlCn in sqlCns)
                {
                    sqlCn.Value.Close();
                }
            }
            return lines;
        }

        public SalesOrderLines GetCustomerOrderSummary(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options)
        {
            Logger.Trace($"/GetOrders?Plant={plants}&CustomerSoldNums={customerSoldNums}&CustomerShipNums={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockTypes={stockTypes}&LastStatusCodes={lastStatusCodes}&NextStatusCodes={nextStatusCodes}&DocumentTypes={documentTypes}&Options={options}");
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var mgr = new CustomerOrderManager(session);
                lines.AddRange(mgr.GetCurrentOrders(session, plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options));
                lines.AddRange(mgr.GetArchivedOrders(session, plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lines;
        }

        public SalesOrderLines GetPriceAuditOrders(string plantCode)
        {
            var lines = new SalesOrderLines();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var mgr = new PriceAuditManager(session);
                lines.AddRange(mgr.GetMissingBarOrdersAir1(session, plantCode));
                lines.AddRange(mgr.GetMissingBarOrdersAir2(session, plantCode));
                lines.AddRange(mgr.GetMissingUnitPricing(session, plantCode));
                lines.AddRange(mgr.GetMissingI89Pricing(session, plantCode));
                lines.AddRange(mgr.GetMissing366Pricing(session, plantCode));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lines;
        }


        public SalesOrderLines GetOpenOrders(string plantCodes, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cnSf = new SqlConnection();
            var lines = new SalesOrderLines();
            try
            {
                var plantCodesClean = plantCodes.Replace("'", "");
                var plants = PlantManager.GetPlants(plantCodesClean);
                foreach (Plant plant in plants)
                {
                    try
                    {
                        cnSf = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cnSf.Open();
                        if (plant.PlantCode.EndsWith("LG"))
                        {

                            var mgr = new Common.Managers.ShopFloor.OpenOrderManagerLG(session);
                            lines.AddRange(mgr.GetOpenOrders(session, cnSf, plant, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks));
                        }
                        else if (plant.PlantCode.EndsWith("IG"))
                        {
                            var mgr = new Common.Managers.ShopFloor.OpenOrderManagerIG(session);
                            lines.AddRange(mgr.GetOpenOrders(session, cnSf, plant, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks));
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());
                    }
                    finally
                    {
                        cnSf.Close();
                    }
                }
                return lines;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
            }
        }

        public static ExternalFacingSalesOrderLines GetExternalFacingSalesOrderLines(BusinessSystemSession session, Dictionary<string, object> parms)
        {
            var orders = new ExternalFacingSalesOrderLines();
            try
            {
                var customerNum = Data.UnboxParm.To<decimal>(parms, "CustomerNum", 0);
                var plants = Data.UnboxParm.To(parms, "Plants", new List<string>());
                var dueDate = Data.UnboxParm.To(parms, "DueDate", new DateTime(1900, 1, 1));
                var customerSeqNum = Data.UnboxParm.To(parms, "CustomerSeqNum", "");
                var genericSeqNum = Data.UnboxParm.To(parms, "GenericSeqNum", "");
                var customerPONum = Data.UnboxParm.To(parms, "CustomerPONum", "");
                var customerPart1 = Data.UnboxParm.To(parms, "CustomerPart1", "");
                var customerPart2 = Data.UnboxParm.To(parms, "CustomerPart2", "");
                var customerPart3 = Data.UnboxParm.To(parms, "CustomerPart3", "");
                var message1 = Data.UnboxParm.To(parms, "Message1", "");
                var message2 = Data.UnboxParm.To(parms, "Message2", "");
                var message3 = Data.UnboxParm.To(parms, "Message3", "");
                var message4 = Data.UnboxParm.To(parms, "Message4", "");
                var message5 = Data.UnboxParm.To(parms, "Message5", "");
                var priorityCode = Data.UnboxParm.To(parms, "PriorityCode", "");
                var productType = Data.UnboxParm.To(parms, "ProductType", "");
                var scheduleNum = Data.UnboxParm.To<decimal>(parms, "ScheduleNum", 0);
                var sOrderNum = Data.UnboxParm.To<decimal>(parms, "SOrderNum", 0);
                var sOrderStatuses = Data.UnboxParm.To(parms, "SOrderStatuses", new List<string>());
                orders = ExternalFacingCustomerOrderManager.GetOpenOrders(session, customerNum, plants, customerPONum, customerSeqNum, customerPart1, dueDate, productType, scheduleNum, sOrderStatuses, priorityCode, genericSeqNum, customerPart2, customerPart3, message1, message2, message3, message4, message5, sOrderNum);
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
            }
            return orders;
        }

    }
}