using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.QuerySvc
{
    [CollectionDataContract(Namespace = "")]
    public class ClosedRacks : List<ClosedRack>
    {
    }

    [DataContract(Namespace = "")]
    public class ClosedRack
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 20)]
        public decimal RackId { get; set; }

        [DataMember(Order = 30)]
        public string RackType { get; set; }

        [DataMember(Order = 32)]
        public string Location { get; set; }

        [DataMember(Order = 40)]
        public decimal BoxNum { get; set; }

        [DataMember(Order = 50)]
        public string Status { get; set; }

        [DataMember(Order = 60)]
        public DateTime RackDate { get; set; }

        [DataMember(Order = 62)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 64)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 66)]
        public string CustomerName { get; set; }

        [DataMember(Order = 67)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 68)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 70)]
        public decimal ScheduleNum { get; set; }        
        
        [DataMember(Order = 80)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 100)]
        public string LastLine { get; set; }

        [DataMember(Order = 110)]
        public DateTime LastLineDate { get; set; }

        [DataMember(Order = 120)]
        public string Lines { get; set; }

        // METRICS

        [DataMember(Order = 201)]
        public int OnHand { get; set; }

        [DataMember(Order = 202)]
        public int Tubes { get; set; }

        [DataMember(Order = 210)]
        public int Shapes { get; set; }

        [DataMember(Order = 220)]
        public int Preserve { get; set; }

        [DataMember(Order = 222)]
        public int Scoring { get; set; }

        [DataMember(Order = 230)]
        public int Bars { get; set; }

        [DataMember(Order = 240)]
        public int Picks { get; set; }


    }
}

