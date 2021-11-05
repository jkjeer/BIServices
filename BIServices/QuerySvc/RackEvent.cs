using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.QuerySvc
{

    [CollectionDataContract(Namespace = "")]
    public class RackEvents : List<RackEvent>
    {
    }

    [DataContract(Namespace = "")]
    public class RackEvent
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }
        [DataMember(Order = 20)]
        public decimal RackId { get; set; }
        [DataMember(Order = 30)]
        public string EventType { get; set; }
        [DataMember(Order = 32)]
        public DateTime EventDateTime { get; set; }
        [DataMember(Order = 34)]
        public DateTime EventDate { get; set; }
        [DataMember(Order = 36)]
        public int EventHour { get; set; }
        [DataMember(Order = 50)]
        public int Quantity { get; set; }
    }
}
