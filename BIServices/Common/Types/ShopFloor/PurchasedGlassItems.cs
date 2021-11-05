using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class PurchasedGlassItems : List<PurchasedGlassItem>
    {
    }

    [DataContract(Namespace = "")]
    public class PurchasedGlassItem
    {

        [DataMember(Order = 20)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 22)]
        public string WorkOrderStatus { get; set; }

        [DataMember(Order = 23)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 24)]
        public decimal Units { get; set; }

        [DataMember(Order = 26)]
        public string UnitType { get; set; }

        [DataMember(Order = 28)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 30)]
        public DateTime DueDate { get; set; }

        [DataMember(Order = 40)]
        public string CustomerName { get; set; }

        [DataMember(Order = 50)]
        public int LiteNum { get; set; }

        [DataMember(Order = 51)]
        public string LiteName { get; set; }

        [DataMember(Order = 60)]
        public string Supplier { get; set; }

        [DataMember(Order = 70)]
        public string Construction { get; set; }

        [DataMember(Order = 80)]
        public string Dimension { get; set; }

        [DataMember(Order = 100)]
        public decimal PurchasedLites { get; set; }

        [DataMember(Order = 110)]
        public decimal ReceivedLites { get; set; }

        [DataMember(Order = 120)]
        public decimal OnHandLites { get; set; }

        [DataMember(Order = 130)]
        public string RackPlant { get; set; }

        [DataMember(Order = 140)]
        public string RacksDesc { get; set; }
    }
}

