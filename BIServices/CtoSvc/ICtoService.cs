using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using SoaCore.Wcf;

namespace BIServices.CtoSvc
{
    [ServiceContract]
    public interface ICtoService
    {
        [OperationContract, OperationInvokerLogger]
        ConsumedSheets GetConsumedSheets(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        ProducedLites GetProducedLites(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        RejectedLites GetRejectedLites(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        ShiftAggregates GetShiftSummary(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        CutAggregates GetCutSummary(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        TemperAggregates GetTemperSummary(string plants, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        RawItems GetRawSupply(string plant, string itemNum);
    }

    [ServiceContract]
    public interface ICtoRestService
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
        [WebGet(UriTemplate = "/GetShiftSummary?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        ShiftAggregates GetShiftSummary(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetCutSummary?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        CutAggregates GetCutSummary(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetCutSummaryByDate?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        CutAggregates GetCutSummaryByDate(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetTemperSummary?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        TemperAggregates GetTemperSummary(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetTemperSummaryByDate?Plant={plants}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        TemperAggregates GetTemperSummaryByDate(string plants, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRawSupply?Plant={plant}&ItemNum={itemNum}")]
        RawItems GetRawSupply(string plant, string itemNum);
    }
}
