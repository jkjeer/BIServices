using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Integration
{

    [CollectionDataContract(Namespace = "")]
    public class Subscriptions : List<Subscription>
    {
    }

    [DataContract(Namespace = "")]
    public class Subscription
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }
        [DataMember(Order = 12)]
        public string TransferType { get; set; }
        [DataMember(Order = 20)]
        public int SubscriptionId { get; set; }
        [DataMember(Order = 30)]
        public int MappingId { get; set; }
        [DataMember(Order = 32)]
        public string MappingName { get; set; }
        [DataMember(Order = 40)]
        public int DataSourceId { get; set; }
        [DataMember(Order = 42)]
        public string ConnectionString { get; set; }
        [DataMember(Order = 50)]
        public string TableName { get; set; }
        [DataMember(Order = 52)]
        public string StagingTableName { get; set; }
        [DataMember(Order = 52)]
        public string StagingViewName { get; set; }
        [DataMember(Order = 54)]
        public string SelectionCriteria { get; set; }
        [DataMember(Order = 60)]
        public bool IsActive { get; set; }
    }
}

