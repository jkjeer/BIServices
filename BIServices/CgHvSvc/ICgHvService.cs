using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.ShopFloor;
using BIServices.CtoSvc;
using SoaCore.Wcf;

namespace BIServices.CgHvSvc
{
    [ServiceContract]
    public interface ICgHvService
    {
        [OperationContract, OperationInvokerLogger]
        ConsumedSheets GetConsumedSheets(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        ProducedLites GetProducedLites(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        RejectedLites GetRejectedLites(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        CoaterAggregates GetCoaterSummary(string plants, string dateCode, DateTime startDate, DateTime endDate);
    }

    [ServiceContract]
    public interface ICgHvRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetConsumedSheets?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        ConsumedSheets GetConsumedSheets(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetProducedLites?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        ProducedLites GetProducedLites(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRejectedLites?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        RejectedLites GetRejectedLites(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetCoaterSummary?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        CoaterAggregates GetCoaterSummary(string plants, string dateCode, string startDate, string endDate);
    }
}
