using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class GlassRacks : List<GlassRack>
    {
    }

    [DataContract(Namespace = "")]
    public class GlassRack
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 12)]
        public decimal FirstUnitId { get; set; }

        [DataMember(Order = 14)]
        public decimal LastUnitId { get; set; }

        // RACK PROPERTIES

        [DataMember(Order = 20)]
        public decimal RackId { get; set; }

        [DataMember(Order = 21)]
        public string AssetId { get; set; }

        [DataMember(Order = 22)]
        public string RackType { get; set; }

        [DataMember(Order = 221)]
        public string RackLocation { get; set; }

        [DataMember(Order = 24)]
        public decimal BoxNum { get; set; }

        [DataMember(Order = 26)]
        public decimal SlotNum { get; set; }

        [DataMember(Order = 28)]
        public int RunSequence { get; set; }

        [DataMember(Order = 30)]
        public string HeaderStatus { get; set; }

        [DataMember(Order = 31)]
        public string Status { get; set; }

        [DataMember(Order = 32)]
        public DateTime? RackItemDate { get; set; }

        [DataMember(Order = 34)]
        public DateTime? RackCloseDate { get; set; }

        [DataMember(Order = 36)]
        public DateTime? RackLockDate { get; set; }

        [DataMember(Order = 38)]
        public DateTime? RackLockLoadDate { get; set; }

        [DataMember(Order = 40)]
        public string FirstCustSeqNum { get; set; }

        [DataMember(Order = 42)]
        public string LastCustSeqNum { get; set; }

        // CUSTOMER PROPERTIES

        [DataMember(Order = 60)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 60)]
        public DateTime MinRequestedDate { get; set; }

        [DataMember(Order = 61)]
        public DateTime MaxRequestedDate { get; set; }

        [DataMember(Order = 62)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 64)]
        public string CustomerName { get; set; }

        [DataMember(Order = 66)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 68)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 70)]
        public string CustomerNameUnitBox { get; set; }

        [DataMember(Order = 72)]
        public string CustomerPO { get; set; }

        [DataMember(Order = 74)]
        public decimal CustomerPONum { get; set; }

        [DataMember(Order = 76)]
        public decimal CustomerPOLineNum { get; set; }

        [DataMember(Order = 78)]
        public string CustomerPart1 { get; set; }

        [DataMember(Order = 80)]
        public string CustomerPart2 { get; set; }

        [DataMember(Order = 82)]
        public string CustomerPart3 { get; set; }

        [DataMember(Order = 84)]
        public string CustomerSeqNum { get; set; }

        [DataMember(Order = 85)]
        public string CustomerSeqPart1 { get; set; }

        [DataMember(Order = 86)]
        public string GenericSeqNum { get; set; }

        [DataMember(Order = 88)]
        public string CustomerTier { get; set; }

        // ORDER PROPERTIES

        [DataMember(Order = 100)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 100)]
        public DateTime MinOrderDate { get; set; }

        [DataMember(Order = 101)]
        public DateTime MaxOrderDate { get; set; }

        [DataMember(Order = 102)]
        public decimal OrderNum { get; set; }

        [DataMember(Order = 104)]
        public string OrderType { get; set; }

        [DataMember(Order = 106)]
        public decimal OrderLineNum { get; set; }

        [DataMember(Order = 108)]
        public string OrderCompany { get; set; }

        [DataMember(Order = 110)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 112)]
        public string WorkOrderStatus { get; set; }

        [DataMember(Order = 114)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 116)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 118)]
        public string ProductType { get; set; }

        [DataMember(Order = 120)]
        public string UnitType { get; set; }

        [DataMember(Order = 122)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 124)]
        public string IGLineScheduled { get; set; }

        [DataMember(Order = 125)]
        public int IGLineRun { get; set; }

        [DataMember(Order = 126)]
        public DateTime ShipDateTime { get; set; }

        [DataMember(Order = 128)]
        public string ShipRoute { get; set; }

        [DataMember(Order = 130)]
        public DateTime DeliveryDateTime { get; set; }

        [DataMember(Order = 132)]
        public double DeliveryDateTimeHours { get; set; }

        // METRICS

        [DataMember(Order = 400)]
        public int Units { get; set; }

        [DataMember(Order = 402)]
        public int OnHand { get; set; }

        [DataMember(Order = 404)]
        public int Tubes { get; set; }

        [DataMember(Order = 406)]
        public int Shapes { get; set; }

        [DataMember(Order = 408)]
        public int Preserve { get; set; }

        [DataMember(Order = 410)]
        public int Scoring { get; set; }

        [DataMember(Order = 412)]
        public int Bars { get; set; }

        [DataMember(Order = 414)]
        public int Picks { get; set; }

        [DataMember(Order = 416)]
        public int Purchases { get; set; }

        [DataMember(Order = 420)]
        public decimal TareWeight { get; set; }

        [DataMember(Order = 422)]
        public decimal GlassWeight { get; set; }

        [DataMember(Order = 424)]
        public decimal TotalWeight { get; set; }



        //[DataMember(Order = 500)]
        public ItemAttributes ItemAttributes { get; set; }

    }
}
