using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class ScheduleSummarys : List<ScheduleSummary>
    {
    }

    [DataContract(Namespace = "")]
    public class ScheduleSummary
    {

        // Schedule
        [DataMember(Order = 10)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 12)]
        public DateTime ScheduledDate { get; set; }

        [DataMember(Order = 14)]
        public DateTime DueDate { get; set; }

        [DataMember(Order = 16)]
        public string Plant { get; set; }

        [DataMember(Order = 18)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 20)]
        public string ScheduleSystem { get; set; }

        [DataMember(Order = 22)]
        public string ScheduleName { get; set; }


        // Aggregate Attributes
        [DataMember(Order = 40)]
        public string UnitType { get; set; }

        [DataMember(Order = 42)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 44)]
        public string CustomerName { get; set; }

        [DataMember(Order = 45)]
        public string CustomerTier { get; set; }

        [DataMember(Order = 46)]
        public DateTime MinRequestedDate { get; set; }

        [DataMember(Order = 48)]
        public DateTime MaxRequestedDate { get; set; }

        [DataMember(Order = 50)]
        public string IGLine { get; set; }


        // Planned Metrics
        [DataMember(Order = 100)]
        public decimal Scheduled { get; set; }

        [DataMember(Order = 102)]
        public decimal Produce { get; set; }

        [DataMember(Order = 104)]
        public decimal Pick { get; set; }

        [DataMember(Order = 104)]
        public decimal FinishedPicksQty { get; set; }

        [DataMember(Order = 106)]
        public decimal Purchase { get; set; }

        [DataMember(Order = 110)]
        public decimal TotalSqft { get; set; }

        [DataMember(Order = 110)]
        public decimal TotalSqft1 { get; set; }

        [DataMember(Order = 110)]
        public decimal TotalUnitSqft { get; set; }

        [DataMember(Order = 110)]
        public decimal TotalLiteSqft { get; set; }

        [DataMember(Order = 111)]
        public decimal CutSqft { get; set; }

        [DataMember(Order = 112)]
        public decimal TemperSqft { get; set; }

        [DataMember(Order = 113)]
        public decimal TemperSqft1 { get; set; }

        [DataMember(Order = 114)]
        public decimal BarsQty { get; set; }

        [DataMember(Order = 115)]
        public decimal PatternBarsQty { get; set; }

        [DataMember(Order = 116)]
        public decimal DiamondBarsQty { get; set; }

        [DataMember(Order = 118)]
        public decimal ScoredQty { get; set; }

        [DataMember(Order = 120)]
        public decimal MonoQty { get; set; }

        [DataMember(Order = 122)]
        public decimal DoubleQty { get; set; }

        [DataMember(Order = 124)]
        public decimal TripleQty { get; set; }

        [DataMember(Order = 139)]
        public decimal LineQty { get; set; }

        [DataMember(Order = 140)]
        public decimal Line0Qty { get; set; }

        [DataMember(Order = 141)]
        public decimal Line1Qty { get; set; }

        [DataMember(Order = 142)]
        public decimal Line2Qty { get; set; }

        [DataMember(Order = 144)]
        public decimal Line3Qty { get; set; }

        [DataMember(Order = 150)]
        public decimal Line4Qty { get; set; }

        [DataMember(Order = 152)]
        public decimal Line5Qty { get; set; }

        [DataMember(Order = 154)]
        public decimal Line6Qty { get; set; }

        [DataMember(Order = 155)]
        public decimal Line7Qty { get; set; }

        [DataMember(Order = 156)]
        public decimal Line8Qty { get; set; }

        [DataMember(Order = 157)]
        public decimal Line9Qty { get; set; }

        [DataMember(Order = 158)]
        public decimal LineOtherQty { get; set; }

        [DataMember(Order = 160)]
        public decimal ShapesQty { get; set; }

        [DataMember(Order = 162)]
        public decimal LamiQty { get; set; }

        [DataMember(Order = 200)]
        public decimal PickLite { get; set; }

        [DataMember(Order = 202)]
        public decimal PickSqft { get; set; }

        [DataMember(Order = 204)]
        public decimal PurchaseLite { get; set; }

        [DataMember(Order = 206)]
        public decimal PurchaseSqft { get; set; }

        [DataMember(Order = 208)]
        public decimal CutLite { get; set; }

        [DataMember(Order = 220)]
        public decimal TemperLite { get; set; }

        [DataMember(Order = 222)]
        public decimal TemperLbs { get; set; }

        [DataMember(Order = 224)]
        public decimal TemperTons { get; set; }

        [DataMember(Order = 226)]
        public decimal TemperPurchasedLite { get; set; }

        [DataMember(Order = 228)]
        public decimal TemperPurchasedLbs { get; set; }

        [DataMember(Order = 230)]
        public decimal TemperPurchasedTons { get; set; }

        [DataMember(Order = 260)]
        public decimal TubesQty { get; set; }

        // Actual Metrics
        [DataMember(Order = 300)]
        public decimal Cancelled { get; set; }

        [DataMember(Order = 302)]
        public decimal Voided { get; set; }

        [DataMember(Order = 304)]
        public decimal Open { get; set; }

        [DataMember(Order = 306)]
        public decimal Closed { get; set; }

        [DataMember(Order = 308)]
        public decimal Locked { get; set; }

        [DataMember(Order = 310)]
        public decimal Packed { get; set; }

        [DataMember(Order = 312)]
        public decimal Shipped { get; set; }

        [DataMember(Order = 314)]
        public decimal Held { get; set; }

        [DataMember(Order = 316)]
        public decimal Other { get; set; }

        [DataMember(Order = 318)]
        public decimal Produced { get; set; }

        [DataMember(Order = 320)]
        public decimal OpenPct { get; set; }

        // Descriptive
        [DataMember(Order = 400)]
        public string PurchasedLites { get; set; }


        [DataMember(Order = 900)]
        public ScheduleLines ScheduleLines { get; set; }

    }

    [CollectionDataContract(Namespace = "")]
    public class ScheduleLines : List<ScheduleLine>
    {
    }

    [DataContract(Namespace = "")]
    public class ScheduleLine
    {

        [DataMember(Order = 10)]
        public decimal SequenceNum { get; set; }

        [DataMember(Order = 14)]
        public DateTime DueDate { get; set; }

        [DataMember(Order = 40)]
        public string UnitType { get; set; }

        [DataMember(Order = 42)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 44)]
        public string CustomerName { get; set; }

        [DataMember(Order = 45)]
        public string CustomerTier { get; set; }

        [DataMember(Order = 50)]
        public string IGLine { get; set; }

        [DataMember(Order = 60)]
        public decimal Quantity { get; set; }

        [DataMember(Order = 70)]
        public decimal IntScoring { get; set; }

        [DataMember(Order = 72)]
        public decimal ExtScoring { get; set; }

        [DataMember(Order = 74)]
        public decimal ShapeNum { get; set; }

        [DataMember(Order = 76)]
        public decimal Mcode1Air1 { get; set; }

        [DataMember(Order = 78)]
        public string GridStyle { get; set; }

        [DataMember(Order = 80)]
        public string Lite1LamiProdType { get; set; }

        [DataMember(Order = 82)]
        public string Lite2LamiProdType { get; set; }

        [DataMember(Order = 84)]
        public string Lite3LamiProdType { get; set; }

        [DataMember(Order = 86)]
        public decimal Lite1Routing { get; set; }

        [DataMember(Order = 88)]
        public decimal Lite2Routing { get; set; }

        [DataMember(Order = 90)]
        public decimal Lite3Routing { get; set; }

        [DataMember(Order = 92)]
        public decimal UnitActualSqft { get; set; }

        [DataMember(Order = 94)]
        public string Lite1HeatTreatment { get; set; }

        [DataMember(Order = 96)]
        public string Lite2HeatTreatment { get; set; }

        [DataMember(Order = 98)]
        public string Lite3HeatTreatment { get; set; }




    }
}