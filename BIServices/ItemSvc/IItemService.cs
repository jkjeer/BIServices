using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ItemSvc
{
    [ServiceContract]
    public interface IItemService
    {
        [OperationContract, OperationInvokerLogger]
        DiscreteItems GetSetup(string plant);
    }

    [ServiceContract]
    public interface IItemRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSetup?Plant={plant}")]
        DiscreteItems GetSetup(string plant);
    }
}
