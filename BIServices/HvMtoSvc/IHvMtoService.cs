using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using BIServices.QuerySvc;
using SoaCore.Wcf;

namespace BIServices.HvMtoSvc
{

    [ServiceContract]
    public interface IHvMtoService
    {
        [OperationContract, OperationInvokerLogger]
        HvMtoOrders GetOpenOrders(string plantCode, bool isHvMto, decimal scheduleNum);

        [OperationContract, OperationInvokerLogger]
        PurchasedGlassItems GetPurchasedGlassSummary(string plantCode, bool isHvMto, decimal scheduleNum);
    }

    [ServiceContract]
    public interface IHvMtoRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOpenOrders?Plant={plantCode}&IsHvMto={isHvMto}&ScheduleNum={scheduleNum}")]
        HvMtoOrders GetOpenOrders(string plantCode, bool isHvMto, decimal scheduleNum);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetPurchasedGlassSummary?Plant={plantCode}&IsHvMto={isHvMto}&ScheduleNum={scheduleNum}")]
        PurchasedGlassItems GetPurchasedGlassSummary(string plantCode, bool isHvMto, decimal scheduleNum);
    }



}
