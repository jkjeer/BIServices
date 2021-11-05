using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.InventorySvc
{
    [ServiceContract]
    public interface IInventoryService
    {
        [OperationContract, OperationInvokerLogger]
        InterlayerLots GetInterlayerLots(string plant);
    }

    [ServiceContract]
    public interface IInventoryRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetInterlayerLots?Plant={plant}")]
        InterlayerLots GetInterlayerLots(string plant);
    }
}
