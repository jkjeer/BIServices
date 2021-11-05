using BIServices.Common.Types.Enterprise;
using NLog;
using SoaCore.Wcf;
using System;

namespace BIServices.SalesSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class SalesService : ISalesService
    {
        public SalesOrderLines GetInterlayerSales(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string lastStatus, string nextStatus)
        {
            var processor = new SalesProcessor();
            try
            {
                return processor.GetInterlayerSales(plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, lastStatus, nextStatus);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class SalesRestService : ISalesRestService
    {
        public SalesOrderLines GetInterlayerSales(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string lastStatus, string nextStatus)
        {
            var processor = new SalesProcessor();
            try
            {
                return processor.GetInterlayerSales(plants, customerSoldNums, customerShipNums, dateCode, startDate, endDate, lastStatus, nextStatus);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
