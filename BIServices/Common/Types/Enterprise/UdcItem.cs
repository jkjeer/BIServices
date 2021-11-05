using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class UdcItems : List<UdcItem>
    {
    }

    [DataContract(Namespace = "")]
    public class UdcItem
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string ProductCode { get; set; }
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string UserCode { get; set; }
        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string UserCodeType { get; set; }
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string UserValue { get; set; }
        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string Description1 { get; set; }
        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string Description2 { get; set; }
        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string SpecialHandlingCode { get; set; }
        [DataMember(Order = 8, EmitDefaultValue = false)]
        public DateTime UpdateDateTime { get; set; }
    }
}
