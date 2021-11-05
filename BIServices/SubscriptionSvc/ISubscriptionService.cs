using System.ServiceModel;
using System.ServiceModel.Web;
using SoaCore.Wcf;

namespace BIServices.SubscriptionSvc
{
    [ServiceContract]
    public interface ISubscriptionService
    {
        [OperationContract, OperationInvokerLogger]
        string GetSubscription(string plantCode);

    }

    [ServiceContract]
    public interface ISubscriptionRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSubscription?PlantCode={plantCode}")]
        string GetSubscription(string plantCode);
    }
}
