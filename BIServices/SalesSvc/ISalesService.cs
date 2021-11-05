using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.SalesSvc
{
    [ServiceContract]
    public interface ISalesService
    {
        [OperationContract, OperationInvokerLogger]
        SalesOrderLines GetInterlayerSales(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string lastStatus, string nextStatus);
    }

    [ServiceContract]
    public interface ISalesRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetInterlayerSales?Plant={plants}&CustomerSoldNum={customerSoldNums}&CustomerShipNum={customerShipNums}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&LastStatus={lastStatus}&NextStatus={nextStatus}")]
        SalesOrderLines GetInterlayerSales(string plants, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string lastStatus, string nextStatus);
    }
}