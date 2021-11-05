using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.QuerySvc
{
    [CollectionDataContract(Namespace = "")]
    public class RackEventHourSummarySet : List<RackEventHourSummary>
    {
    }

    [DataContract(Namespace = "")]
    public class RackEventHourSummary
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 20)]
        public DateTime EventDate { get; set; }
        [DataMember(Order = 22)]
        public int EventHour { get; set; }

        [DataMember(Order = 50)]
        public int ClosedRacks { get; set; }
        [DataMember(Order = 52)]
        public int LockedRacks { get; set; }
        [DataMember(Order = 54)]
        public int LockedLoadRacks { get; set; }

        [DataMember(Order = 100)]
        public string UnitTypeCode { get; set; }
    }
}
