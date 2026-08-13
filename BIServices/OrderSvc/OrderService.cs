using BIServices.Common.Managers.E1;
using BIServices.Common.Types.Enterprise;
using NLog;
using SoaCore.Wcf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BIServices.OrderSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class OrderService : IOrderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SalesOrderLines GetStockOrders(string plantCode, decimal customerNum)
        {
            Logger.Trace($"/GetStockOrders?Plant={plantCode}&CustomerNum={customerNum}");
            var processor = new OrderProcessor();
            try
            {
                return processor.GetStockOrders(plantCode, customerNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetCustomOrders(string plantCode, decimal customerNum)
        {
            Logger.Trace($"/GetCustomOrders?Plant={plantCode}&CustomerNum={customerNum}");
            var processor = new OrderProcessor();
            try
            {
                return processor.GetCustomOrders(plantCode, customerNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetStaleOrders(string plantCode, string dateProperty, string dateCode, string startDate, string endDate)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetStaleOrders(plantCode, dateProperty, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
        {
            Logger.Trace($"/GetOrders?Plant={plants}&CustomerSoldNums={customerSoldNums}&CustomerShipNums={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockTypes={stockTypes}&LastStatusCodes={lastStatusCodes}&NextStatusCodes={nextStatusCodes}&DocumentTypes={documentTypes}&Options={options}&Racks={racks}");
            var processor = new OrderProcessor();
            try
            {
                return processor.GetOrders(plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetPriceAuditOrders(string plantCode)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetPriceAuditOrders(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetAgcOpenOrders(string plants, string customerSoldNums)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetAgcOpenOrders(plants, customerSoldNums);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class OrderRestService : IOrderRestService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SalesOrderLines GetStockOrders(string plantCode, decimal customerNum)
        {
            Logger.Trace($"/GetStockOrders?Plant={plantCode}&CustomerNum={customerNum}");
            var processor = new OrderProcessor();
            try
            {
                return processor.GetStockOrders(plantCode, customerNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetCustomOrders(string plantCode, decimal customerNum)
        {
            Logger.Trace($"/GetCustomOrders?Plant={plantCode}&CustomerNum={customerNum}");
            var processor = new OrderProcessor();
            try
            {
                return processor.GetCustomOrders(plantCode, customerNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetStaleOrders(string plantCode, string dateProperty, string dateCode, string startDate, string endDate)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetStaleOrders(plantCode, dateProperty, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
        {
            Logger.Trace($"/GetOrders?Plant={plants}&CustomerSoldNums={customerSoldNums}&CustomerShipNums={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockTypes={stockTypes}&LastStatusCodes={lastStatusCodes}&NextStatusCodes={nextStatusCodes}&DocumentTypes={documentTypes}&Options={options}&Racks={racks}");
            var processor = new OrderProcessor();
            try
            {
                return processor.GetOrders(plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetCustomerOrderSummary(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetCustomerOrderSummary(plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetOpenOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetOpenOrders(plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, dateProperty, stockTypes, lastStatusCodes, nextStatusCodes, documentTypes, options, racks);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetAgcOpenOrders(string plants, string customerSoldNums)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetAgcOpenOrders(plants, customerSoldNums);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public SalesOrderLines GetPriceAuditOrders(string plantCode)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetPriceAuditOrders(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ExternalFacingSalesOrderLines GetExternalFacingSalesOrderLines(decimal customerNum, string plants, string customerPoNum, string customerPoLine, string customerSeq,
            string customerPartNum1, string requestedDate, string productType, decimal scheduleNum, string orderStatus, string priorityCode,
            string genericSeq, string customerPartNum2, string customerPartNum3, string message1, string message2, string message3, string message4, string message5,
            string unitStatus, decimal salesOrderNum)
        {
            DateTime? reqDate = null;
            if (!String.IsNullOrEmpty(requestedDate))
            {
                DateTime outVal;
                if (DateTime.TryParse(requestedDate, out outVal))
                {
                    reqDate = (DateTime?)outVal;
                }
                else
                {
                    throw new Exception("Requested Date is INVALID");
                }
            }

            //if (customerNum == 0)
            //{
            //    throw new Exception("Customer Number is REQUIRED");
            //}

            var parms = new Dictionary<string, object>();
            parms.Add("CustomerNum", customerNum);
            parms.Add("Plants", plants?.Split(',').ToList());
            parms.Add("DueDate", requestedDate);
            parms.Add("CustomerSeqNum", customerSeq);
            parms.Add("GenericSeqNum", genericSeq);
            parms.Add("ProductType", productType);
            parms.Add("CustomerPONum", customerPoNum);
            parms.Add("CustomerPart1", customerPartNum1);
            parms.Add("CustomerPart2", customerPartNum2);
            parms.Add("CustomerPart3", customerPartNum3);
            parms.Add("Message1", message1);
            parms.Add("Message2", message2);
            parms.Add("Message3", message3);
            parms.Add("Message4", message4);
            parms.Add("Message5", message5);
            parms.Add("PriorityCode", priorityCode);
            parms.Add("ScheduleNum", scheduleNum);
            parms.Add("SOrderNum", salesOrderNum);
            parms.Add("SOrderStatuses", orderStatus?.Split(',').ToList());
            var session = BusinessSystemSessionManager.GetBusinessSessionEnvironment(ConfigurationManager.AppSettings["ReplicationEnvironment"]);
            return OrderProcessor.GetExternalFacingSalesOrderLines(session, parms);
        }


    }
}
