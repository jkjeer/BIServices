using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.IntegrationSvc
{
    [CollectionDataContract(Namespace = "")]
    public class Subscriptions : List<Subscription>
    {
    }
}
