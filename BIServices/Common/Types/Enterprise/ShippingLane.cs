using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BIServices.Common.Types.Enterprise
{

    [DataContract(Namespace = "")]
    public class ShippingLane
    {
        [DataMember(Order = 10)]
        public string Name { get; set; }

        [DataMember(Order = 12)]
        public decimal Offset { get; set; }

    }
}
