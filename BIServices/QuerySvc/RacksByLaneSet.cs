using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.QuerySvc
{
    [CollectionDataContract(Namespace = "")]
    public class RacksByLaneSet : List<RacksByLaneRecord>
    {
    }

    [DataContract(Namespace = "")]
    public class RacksByLaneRecord
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        // Customer
        [DataMember(Order = 20)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 22)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 24)]
        public string CustomerName { get; set; }

        [DataMember(Order = 26)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 28)]
        public string CustomerShipName { get; set; }

        // Rack
        [DataMember(Order = 40)]
        public decimal RackId { get; set; }

        [DataMember(Order = 42)]
        public string RackType { get; set; }

        [DataMember(Order = 43)]
        public string Location { get; set; }

        [DataMember(Order = 44)]
        public string RackTypeGroup { get; set; }

        [DataMember(Order = 46)]
        public string Status { get; set; }

        // Schedule
        [DataMember(Order = 60)]
        public string ScheduleType { get; set; }

        // Shipping
        [DataMember(Order = 80)]
        public int ShippingLaneId { get; set; }

        [DataMember(Order = 82)]
        public string ShippingLaneName { get; set; }

        // METRICS
        [DataMember(Order = 100)]
        public int Racks { get; set; }

        [DataMember(Order = 102)]
        public int Units { get; set; }

    }
}

