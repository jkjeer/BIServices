using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.RackSvc
{
        [ServiceContract]
        public interface IRackService
        {
            [OperationContract, OperationInvokerLogger]
            Racks GetHVRacksOnHand(string plant, int agingDays, string status);

            [OperationContract, OperationInvokerLogger]
            Racks GetStockRacksOnHand(string plant, int agingDays, string status);

            [OperationContract, OperationInvokerLogger]
            Racks GetHVMTORacksOnHand(string plant, int agingDays, string status);

            [OperationContract, OperationInvokerLogger]
            Racks GetPurchasedConfiguredRacks(string plant, int agingDays);
    }

        [ServiceContract]
        public interface IRackRestService
        {
            [OperationContract, OperationInvokerLogger]
            [WebGet(UriTemplate = "/GetHVRacksOnHand?Plant={plant}&AgingDays={agingDays}&Status={status}")]
            Racks GetHVRacksOnHand(string plant, int agingDays, string status);

            //[OperationContract, OperationInvokerLogger]
            //[WebGet(UriTemplate = "/GetHVRacksOnHandVer1?Plant={plant}&AgingDays={agingDays}")]
            //Racks GetHVRacksOnHandVer1(string plant, int agingDays);

            [OperationContract, OperationInvokerLogger]
            [WebGet(UriTemplate = "/GetStockRacksOnHand?Plant={plant}&AgingDays={agingDays}&Status={status}")]
            Racks GetStockRacksOnHand(string plant, int agingDays, string status);

            [OperationContract, OperationInvokerLogger]
            [WebGet(UriTemplate = "/GetHVMTORacksOnHand?Plant={plant}&AgingDays={agingDays}&Status={status}")]
            Racks GetHVMTORacksOnHand(string plant, int agingDays, string status);

            [OperationContract, OperationInvokerLogger]
            [WebGet(UriTemplate = "/GetPurchasedConfiguredRacks?Plant={plant}&AgingDays={agingDays}")]
            Racks GetPurchasedConfiguredRacks(string plant, int agingDays);

            [OperationContract, OperationInvokerLogger]
            [WebGet(UriTemplate = "/GetPurchasedRemakes?Plant={plant}")]
            RemakeUnits GetPurchasedRemakes(string plant);
    }
}
