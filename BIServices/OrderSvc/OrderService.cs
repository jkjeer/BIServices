using BIServices.Common.Types.Enterprise;
using NLog;
using SoaCore.Wcf;
using System;

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

        public SalesOrderLines GetAgcOpenOrders(string plants)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetAgcOpenOrders(plants);
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

        public SalesOrderLines GetAgcOpenOrders(string plants)
        {
            var processor = new OrderProcessor();
            try
            {
                return processor.GetAgcOpenOrders(plants);
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


    }
}
