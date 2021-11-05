using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{

    [CollectionDataContract(Namespace = "")]
    public class GlassTypes : List<GlassType>
    {
    }

    [DataContract(Namespace = "")]
    public class GlassType
    {
        [DataMember(Order = 10)]
        public decimal Thickness { get; set; }

        [DataMember(Order = 20)]
        public string LamiType { get; set; }

    }

}
