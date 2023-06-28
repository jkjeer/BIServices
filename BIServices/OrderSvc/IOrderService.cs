using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.OrderSvc
{
    [ServiceContract]
    public interface IOrderService
    {
        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetStockOrders(string plant, decimal customerNum);

        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetCustomOrders(string plant, decimal customerNum);

        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetStaleOrders(string plant, string dateProperty, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks);

        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetPriceAuditOrders(string plant);

        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetAgcOpenOrders(string plant, string customerSoldNums);
    }

    [ServiceContract]
    public interface IOrderRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOrders?Plant={plants}&CustomerSoldNum={customerSoldNums}&CustomerShipNum={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockType={stockTypes}&LastStatusCode={lastStatusCodes}&NextStatusCode={nextStatusCodes}&DocumentType={documentTypes}&Option={options}&Racks={racks}")]
        SalesOrderLines GetOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOpenOrders?Plant={plants}&CustomerSoldNum={customerSoldNums}&CustomerShipNum={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockType={stockTypes}&LastStatusCode={lastStatusCodes}&NextStatusCode={nextStatusCodes}&DocumentType={documentTypes}&Option={options}&Racks={racks}")]
        SalesOrderLines GetOpenOrders(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options, string racks);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetCustomerOrderSummary?Plant={plants}&CustomerSoldNum={customerSoldNums}&CustomerShipNum={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&StockType={stockTypes}&LastStatusCode={lastStatusCodes}&NextStatusCode={nextStatusCodes}&DocumentType={documentTypes}&Option={options}")]
        SalesOrderLines GetCustomerOrderSummary(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string dateProperty, string stockTypes, string lastStatusCodes, string nextStatusCodes, string documentTypes, string options);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetStockOrders?Plant={plant}&CustomerNum={customerNum}")]
        SalesOrderLines GetStockOrders(string plant, decimal customerNum);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetCustomOrders?Plant={plant}&CustomerNum={customerNum}")]
        SalesOrderLines GetCustomOrders(string plant, decimal customerNum);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetStaleOrders?Plant={plant}&DateProperty={dateProperty}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        SalesOrderLines GetStaleOrders(string plant, string dateProperty, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetAgcOpenOrders?Plant={plant}&CustomerSoldNum={customerSoldNums}")]
        SalesOrderLines GetAgcOpenOrders(string plant, string customerSoldNums);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetPriceAuditOrders?Plant={plant}")]
        SalesOrderLines GetPriceAuditOrders(string plant);
    }
}
