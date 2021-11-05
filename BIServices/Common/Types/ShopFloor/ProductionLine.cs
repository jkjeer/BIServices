using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class ProductionLines : List<ProductionLine>
    {
    }

    [DataContract(Namespace = "")]
    public class ProductionLine
    {
        [DataMember(Order = 10)]
        public string ProductionLineCode { get; set; }

        [DataMember(Order = 12)]
        public string WorkCenterId { get; set; }

        [DataMember(Order = 14)]
        public string Description { get; set; }

        [DataMember(Order = 16)]
        public string LineCode { get; set; }

        [DataMember(Order = 18)]
        public string GroupDescription { get; set; }
    }
}
