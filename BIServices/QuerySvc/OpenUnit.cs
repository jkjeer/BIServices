using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.QuerySvc
{
    [CollectionDataContract(Namespace = "")]
    public class OpenUnits : List<OpenUnit>
    {
    }

    [DataContract(Namespace = "")]
    public class OpenUnit
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 15)]
        public decimal UnitId { get; set; }

        [DataMember(Order = 16)]
        public string UnitIdChar { get; set; }

        // RACK PROPERTIES

        [DataMember(Order = 20)]
        public decimal RackId { get; set; }

        [DataMember(Order = 30)]
        public string RackType { get; set; }

        [DataMember(Order = 40)]
        public decimal BoxNum { get; set; }

        [DataMember(Order = 42)]
        public decimal SlotNum { get; set; }

        [DataMember(Order = 44)]
        public int RunSequence { get; set; }

        [DataMember(Order = 50)]
        public string Status { get; set; }

        [DataMember(Order = 52)]
        public DateTime? RackItemDate { get; set;}

        [DataMember(Order = 53)]
        public DateTime? RackCloseDate { get; set; }

        [DataMember(Order = 53)]
        public DateTime? RackLockDate { get; set; }

        [DataMember(Order = 53)]
        public DateTime? RackLockLoadDate { get; set; }

        // CUSTOMER PROPERTIES

        [DataMember(Order = 54)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 60)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 61)]
        public string CustomerName { get; set; }

        [DataMember(Order = 62)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 63)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 64)]
        public string CustomerNameUnitBox { get; set; }

        [DataMember(Order = 65)]
        public string CustomerPO { get; set; }

        [DataMember(Order = 66)]
        public string CustomerPart1 { get; set; }

        [DataMember(Order = 67)]
        public string CustomerPart2 { get; set; }

        [DataMember(Order = 68)]
        public string CustomerPart3 { get; set; }

        // ORDER PROPERTIES

        [DataMember(Order = 70)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 71)]
        public decimal OrderNum { get; set; }

        [DataMember(Order = 72)]
        public string OrderType { get; set; }

        [DataMember(Order = 73)]
        public decimal OrderLineNum { get; set; }

        [DataMember(Order = 73)]
        public string OrderCompany { get; set; }

        [DataMember(Order = 76)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 77)]
        public string WorkOrderStatus { get; set; }

        [DataMember(Order = 80)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 81)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 82)]
        public string ProductType { get; set; }

        [DataMember(Order = 83)]
        public string UnitType { get; set; }

        [DataMember(Order = 84)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 86)]
        public string IGLineScheduled { get; set; }

        // PRODUCTION PROPERTIES

        [DataMember(Order = 100)]
        public string LastLine { get; set; }

        [DataMember(Order = 102)]
        public int IGLineRun { get; set; }

        [DataMember(Order = 110)]
        public DateTime LastLineDate { get; set; }

        [DataMember(Order = 112)]
        public int Passes { get; set; }

        [DataMember(Order = 120)]
        public int RemakeId { get; set; }

        [DataMember(Order = 121)]
        public Decimal RemakeScheduleNum { get; set; }

        [DataMember(Order = 122)]
        public DateTime? RemakeInitiateDate { get; set; }

        [DataMember(Order = 124)]
        public string RemakeReason { get; set; }

        [DataMember(Order = 126)]
        public string RemakeComment { get; set; }

        [DataMember(Order = 130)]
        public Decimal OriginalScheduleNum { get; set; }

        [DataMember(Order = 132)]
        public DateTime OriginalScheduleDate { get; set; }

        [DataMember(Order = 133)]
        public Decimal ScheduledRackId { get; set; }

        [DataMember(Order = 134)]
        public bool IsMissing { get; set; }

        // CONSTRUCTION PROPERTIES

        [DataMember(Order = 150)]
        public string Construction { get; set; }

        [DataMember(Order = 152)]
        public int ShapeNum { get; set; }

        [DataMember(Order = 154)]
        public double BaseDim { get; set; }

        [DataMember(Order = 156)]
        public string BaseDimChar { get; set; }

        [DataMember(Order = 158)]
        public double LeftDim { get; set; }

        [DataMember(Order = 160)]
        public string LeftDimChar { get; set; }

        [DataMember(Order = 162)]
        public string DimMakeup { get; set; }

        [DataMember(Order = 170)]
        public string Lite1Routing { get; set; }

        [DataMember(Order = 172)]
        public string Lite2Routing { get; set; }

        [DataMember(Order = 174)]
        public string Lite3Routing { get; set; }

        // PURCHASED LITES
        [DataMember(Order = 180)]
        public string IsPurchasedLite { get; set; }


        // METRICS

        [DataMember(Order = 200)]
        public int Units { get; set; }

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

        [DataMember(Order = 242)]
        public int Purchases { get; set; }


    }
}
