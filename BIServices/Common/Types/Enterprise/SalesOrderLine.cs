using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class SalesOrderLines : List<SalesOrderLine>
    {
        public SalesOrderLines(Int32 size)
        {
            this.Capacity = size;
        }

        public SalesOrderLines()
        {
        }
    }

    [DataContract(Namespace = "")]
    public class SalesOrderLine
    {
        public SalesOrderLine()
        {
            MonoQty = 0;
            MonoSqft = 0;
            DoubleQty = 0;
            DoubleSqft = 0;
            TripleQty = 0;
            TripleSqft = 0;
            TemperedSqft = 0;
            CutSqft = 0;
        }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string PlantNum { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string PlantCode { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string OrderCompany { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public decimal OrderNum { get; set; }

        [DataMember(Order = 14, EmitDefaultValue = false)]
        public string OrderType { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = false)]
        public decimal OrderLineNum { get; set; }

        [DataMember(Order = 17, EmitDefaultValue = false)]
        public string SOrderDesc { get; set; }

        [DataMember(Order = 18, EmitDefaultValue = false)]
        public string LineType { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string WorkOrderStatus { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string WOrderDesc { get; set; }

        [DataMember(Order = 21, EmitDefaultValue = false)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 21, EmitDefaultValue = false)]
        public string LotNum { get; set; }

        [DataMember(Order = 22, EmitDefaultValue = false)]
        public string LastStatus { get; set; }

        [DataMember(Order = 23, EmitDefaultValue = false)]
        public string NextStatus { get; set; }

        [DataMember(Order = 24, EmitDefaultValue = false)]
        public string MinUnitId { get; set; }

        [DataMember(Order = 25, EmitDefaultValue = false)]
        public string MaxUnitId { get; set; }

        [DataMember(Order = 26, EmitDefaultValue = false)]
        public string MinUnitStatus { get; set; }

        [DataMember(Order = 27, EmitDefaultValue = false)]
        public string MaxUnitStatus { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 31, EmitDefaultValue = false)]
        public string CustomerName { get; set; }

        [DataMember(Order = 32, EmitDefaultValue = false)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 33, EmitDefaultValue = false)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 34, EmitDefaultValue = false)]
        public string CustomerTier { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 42, EmitDefaultValue = false)]
        public string StockType { get; set; }

        [DataMember(Order = 44, EmitDefaultValue = false)]
        public string GlCategory { get; set; }

        [DataMember(Order = 44, EmitDefaultValue = false)]
        public string ItemDesc1 { get; set; }

        [DataMember(Order = 46, EmitDefaultValue = false)]
        public string ItemDesc2 { get; set; }

        [DataMember(Order = 48, EmitDefaultValue = false)]
        public decimal PlannerNum { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public string UnitType { get; set; }

        [DataMember(Order = 52, EmitDefaultValue = false)]
        public string UnitTypeDesc { get; set; }

        [DataMember(Order = 54, EmitDefaultValue = false)]
        public string Description1 { get; set; }

        [DataMember(Order = 56, EmitDefaultValue = false)]
        public string Description2 { get; set; }

        [DataMember(Order = 58, EmitDefaultValue = false)]
        public decimal EndItemPickNum { get; set; }

        [DataMember(Order = 59, EmitDefaultValue = false)]
        public decimal Lite1ItemNum { get; set; }

        // Attributes

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public decimal ConfiguratorId { get; set; }

        [DataMember(Order = 62, EmitDefaultValue = false)]
        public decimal Lite1Thickness { get; set; }

        [DataMember(Order = 63, EmitDefaultValue = false)]
        public string Lite1HeatTreatment { get; set; }

        [DataMember(Order = 63, EmitDefaultValue = false)]
        public string Lite2HeatTreatment { get; set; }

        [DataMember(Order = 63, EmitDefaultValue = false)]
        public string Lite3HeatTreatment { get; set; }

        [DataMember(Order = 64, EmitDefaultValue = false)]
        public string Lite1Routing { get; set; }

        [DataMember(Order = 66, EmitDefaultValue = false)]
        public string Lite2Routing { get; set; }

        [DataMember(Order = 68, EmitDefaultValue = false)]
        public string Lite3Routing { get; set; }

        [DataMember(Order = 70, EmitDefaultValue = false)]
        public string Lite1Supplier { get; set; }

        [DataMember(Order = 72, EmitDefaultValue = false)]
        public string Lite2Supplier { get; set; }

        [DataMember(Order = 74, EmitDefaultValue = false)]
        public string Lite3Supplier { get; set; }

        [DataMember(Order = 80, EmitDefaultValue = false)]
        public string VolumeIndicator { get; set; }

        [DataMember(Order = 82, EmitDefaultValue = false)]
        public decimal MCode { get; set; }

        [DataMember(Order = 83, EmitDefaultValue = false)]
        public string MCodeDesc { get; set; }

        [DataMember(Order = 84, EmitDefaultValue = false)]
        public string GridStyle { get; set; }

        [DataMember(Order = 86, EmitDefaultValue = true)]
        public decimal ShapeNum { get; set; }

        [DataMember(Order = 88, EmitDefaultValue = false)]
        public decimal BarsQty { get; set; }

        [DataMember(Order = 89, EmitDefaultValue = false)]
        public decimal BarOpenings { get; set; }

        [DataMember(Order = 90, EmitDefaultValue = false)]
        public decimal OpeningsHigh { get; set; }

        [DataMember(Order = 92, EmitDefaultValue = false)]
        public decimal OpeningsWide { get; set; }

        [DataMember(Order = 94, EmitDefaultValue = false)]
        public decimal OpeningsOdd { get; set; }

        [DataMember(Order = 96, EmitDefaultValue = false)]
        public decimal OpeningsRect { get; set; }

        [DataMember(Order = 98, EmitDefaultValue = false)]
        public string Surface4Coating { get; set; }

        [DataMember(Order = 99, EmitDefaultValue = false)]
        public string QCInspectFlag { get; set; }

        [DataMember(Order = 99, EmitDefaultValue = false)]
        public string LamiType { get; set; }

        [DataMember(Order = 99, EmitDefaultValue = false)]
        public decimal UnitActualSqft { get; set; }

        [DataMember(Order = 99, EmitDefaultValue = false)]
        public string InterlayerCode { get; set; }

        // Customer References

        [DataMember(Order = 100, EmitDefaultValue = false)]
        public string CustomerSequenceNum { get; set; }

        [DataMember(Order = 102, EmitDefaultValue = false)]
        public string GenericSequenceNum { get; set; }

        [DataMember(Order = 104, EmitDefaultValue = false)]
        public string ProductType { get; set; }

        [DataMember(Order = 106, EmitDefaultValue = false)]
        public string CustomerPart1 { get; set; }

        [DataMember(Order = 108, EmitDefaultValue = false)]
        public string CustomerPart2 { get; set; }

        [DataMember(Order = 108, EmitDefaultValue = false)]
        public string CustomerPart3 { get; set; }

        [DataMember(Order = 110, EmitDefaultValue = false)]
        public string ShopFloorMsg1 { get; set; }

        [DataMember(Order = 112, EmitDefaultValue = false)]
        public string ShopFloorMsg2 { get; set; }

        [DataMember(Order = 114, EmitDefaultValue = false)]
        public string PickNotes { get; set; }

        [DataMember(Order = 116, EmitDefaultValue = false)]
        public string PackNotes { get; set; }

        [DataMember(Order = 117, EmitDefaultValue = false)]
        public string CustomerPOHdr { get; set; }

        [DataMember(Order = 118, EmitDefaultValue = false)]
        public string CustomerPO { get; set; }

        [DataMember(Order = 120, EmitDefaultValue = false)]
        public string Department { get; set; }

        [DataMember(Order = 122, EmitDefaultValue = false)]
        public string ReasonCode { get; set; }

        [DataMember(Order = 123, EmitDefaultValue = false)]
        public string ReasonCodeDescription { get; set; }

        [DataMember(Order = 124, EmitDefaultValue = false)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 126, EmitDefaultValue = false)]
        public string Reference2 { get; set; }

        [DataMember(Order = 128, EmitDefaultValue = false)]
        public string AccountManager { get; set; }

        [DataMember(Order = 150, EmitDefaultValue = false)]
        public DateTime CreatedDate { get; set; }

        [DataMember(Order = 151, EmitDefaultValue = false)]
        public DateTime PromiseDate { get; set; }

        [DataMember(Order = 151, EmitDefaultValue = false)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 152, EmitDefaultValue = false)]
        public DateTime InitialRequestedDate { get; set; }

        [DataMember(Order = 152, EmitDefaultValue = false)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 153, EmitDefaultValue = false)]
        public int RequestedTime { get; set; }

        [DataMember(Order = 154, EmitDefaultValue = false)]
        public DateTime InvoiceDate { get; set; }

        [DataMember(Order = 156, EmitDefaultValue = false)]
        public DateTime GlDate { get; set; }

        [DataMember(Order = 158, EmitDefaultValue = false)]
        public DateTime PurchaseDate { get; set; }

        [DataMember(Order = 159, EmitDefaultValue = false)]
        public DateTime ScheduleDate { get; set; }

        [DataMember(Order = 160, EmitDefaultValue = false)]
        public DateTime Lite1PurDate { get; set; }

        [DataMember(Order = 162, EmitDefaultValue = false)]
        public DateTime Lite2PurDate { get; set; }

        [DataMember(Order = 164, EmitDefaultValue = false)]
        public DateTime Lite3PurDate { get; set; }

        [DataMember(Order = 166, EmitDefaultValue = false)]
        public string LitesPurchased { get; set; }

        [DataMember(Order = 210, EmitDefaultValue = false)]
        public string Construction { get; set; }

        [DataMember(Order = 220, EmitDefaultValue = false)]
        public string Dimension { get; set; }

        [DataMember(Order = 200, EmitDefaultValue = false)]
        public decimal LitesPerRack { get; set; }

        [DataMember(Order = 201, EmitDefaultValue = false)]
        public decimal TotalGlassPounds { get; set; }

        [DataMember(Order = 202, EmitDefaultValue = false)]
        public string DecimalShift { get; set; }

        [DataMember(Order = 203, EmitDefaultValue = false)]
        public string WeightUnitOfMeasure { get; set; }

        [DataMember(Order = 204, EmitDefaultValue = false)]
        public decimal TotalGlassTons { get; set; }

        [DataMember(Order = 205, EmitDefaultValue = false)]
        public decimal UnitCost { get; set; }

        // Pricing Attributes
        [DataMember(Order = 220, EmitDefaultValue = true)]
        public decimal UnitPrice { get; set; }

        [DataMember(Order = 222, EmitDefaultValue = true)]
        public decimal UnitBarPrice { get; set; }

        [DataMember(Order = 224, EmitDefaultValue = true)]
        public decimal BarPriceLevel { get; set; }

        [DataMember(Order = 225, EmitDefaultValue = false)]
        public string BarPriceDesc { get; set; }

        [DataMember(Order = 250, EmitDefaultValue = false)]
        public string PriceAuditDesc { get; set; }

        // Shipping
        [DataMember(Order = 260, EmitDefaultValue = false)]
        public decimal LoadNum { get; set; }

        [DataMember(Order = 262, EmitDefaultValue = false)]
        public string CarrierFlag { get; set; }

        // Metrics
        [DataMember(Order = 300, EmitDefaultValue = false)]
        public decimal OrderQty { get; set; }

        [DataMember(Order = 302, EmitDefaultValue = false)]
        public decimal TotalCost { get; set; }

        [DataMember(Order = 304, EmitDefaultValue = true)]
        public decimal TotalPrice { get; set; }

        [DataMember(Order = 305, EmitDefaultValue = true)]
        public decimal TotalBarPrice { get; set; }

        [DataMember(Order = 320, EmitDefaultValue = false)]
        public decimal TotalActualSqft { get; set; }

        [DataMember(Order = 322, EmitDefaultValue = false)]
        public decimal TotalBillableSqft { get; set; }

        [DataMember(Order = 324, EmitDefaultValue = false)]
        public decimal TotalGlassWeight { get; set; }

        [DataMember(Order = 326, EmitDefaultValue = false)]
        public decimal Lite1TotalWeight { get; set; }

        [DataMember(Order = 328, EmitDefaultValue = false)]
        public decimal Lite2TotalWeight { get; set; }

        [DataMember(Order = 330, EmitDefaultValue = false)]
        public decimal Lite3TotalWeight { get; set; }



        // New Metrics
        [DataMember(Order = 400, EmitDefaultValue = true)]
        public decimal MonoQty { get; set; }

        [DataMember(Order = 401, EmitDefaultValue = true)]
        public decimal DoubleQty { get; set; }

        [DataMember(Order = 402, EmitDefaultValue = true)]
        public decimal TripleQty { get; set; }

        [DataMember(Order = 410, EmitDefaultValue = true)]
        public decimal MonoSqft { get; set; }

        [DataMember(Order = 411, EmitDefaultValue = true)]
        public decimal DoubleSqft { get; set; }

        [DataMember(Order = 412, EmitDefaultValue = true)]
        public decimal TripleSqft { get; set; }

        [DataMember(Order = 420, EmitDefaultValue = true)]
        public decimal TemperedSqft { get; set; }

        [DataMember(Order = 421, EmitDefaultValue = true)]
        public decimal TemperedPurchasedSqft { get; set; }

        [DataMember(Order = 421, EmitDefaultValue = true)]
        public decimal TemperedInternalSqft { get; set; }

        [DataMember(Order = 422, EmitDefaultValue = true)]
        public decimal AnnealedSqft { get; set; }

        [DataMember(Order = 424, EmitDefaultValue = true)]
        public decimal LaminatedSqft { get; set; }

        [DataMember(Order = 426, EmitDefaultValue = true)]
        public decimal CutSqft { get; set; }

        [DataMember(Order = 430, EmitDefaultValue = true)]
        public string HvLv { get; set; }

        [DataMember(Order = 440, EmitDefaultValue = true)]
        public int ScheduledUnits { get; set; }

        [DataMember(Order = 441, EmitDefaultValue = true)]
        public int Units { get; set; }

        [DataMember(Order = 442, EmitDefaultValue = true)]
        public int CompletedUnits { get; set; }

        [DataMember(Order = 444, EmitDefaultValue = true)]
        public int ClosedUnits { get; set; }

        // Racks
        [DataMember(Order = 500, EmitDefaultValue = false)]
        public Racks Racks { get; set; }


        // AGC Specific
        [DataMember(Order = 502, EmitDefaultValue = true)]
        public int OrderedRacksQty { get; set; }

        [DataMember(Order = 504, EmitDefaultValue = true)]
        public decimal OrderedGlassWeight { get; set; }

        [DataMember(Order = 506, EmitDefaultValue = true)]
        public decimal OrderedTareWeight { get; set; }

        // Ordered

        [DataMember(Order = 508, EmitDefaultValue = true)]
        public int OrderOnHandRacksQty { get; set; }

        [DataMember(Order = 510, EmitDefaultValue = true)]
        public decimal OrderOnHandGlassWeight { get; set; }

        [DataMember(Order = 512, EmitDefaultValue = true)]
        public decimal OrderOnHandTareWeight { get; set; }

        [DataMember(Order = 514, EmitDefaultValue = true)]
        public int OrderPickedRacksQty { get; set; }

        [DataMember(Order = 516, EmitDefaultValue = true)]
        public decimal OrderPickedGlassWeight { get; set; }

        [DataMember(Order = 518, EmitDefaultValue = true)]
        public decimal OrderPickedTareWeight { get; set; }

        [DataMember(Order = 520, EmitDefaultValue = true)]
        public int OrderLoadedRacksQty { get; set; }

        [DataMember(Order = 522, EmitDefaultValue = true)]
        public decimal OrderLoadedGlassWeight { get; set; }

        [DataMember(Order = 524, EmitDefaultValue = true)]
        public decimal OrderLoadedTareWeight { get; set; }

        [DataMember(Order = 526, EmitDefaultValue = true)]
        public string OrderRoutingDesc { get; set; }

        [DataMember(Order = 528, EmitDefaultValue = true)]
        public string RelatedOrderDesc { get; set; }

        [DataMember(Order = 530, EmitDefaultValue = true)]
        public string OrderInventoryDesc { get; set; }

        // Item

        [DataMember(Order = 540, EmitDefaultValue = true)]
        public int ItemOnHandRacksQty { get; set; }

        [DataMember(Order = 542, EmitDefaultValue = true)]
        public decimal ItemOnHandGlassWeight { get; set; }

        [DataMember(Order = 544, EmitDefaultValue = true)]
        public decimal ItemOnHandTareWeight { get; set; }

        [DataMember(Order = 546, EmitDefaultValue = true)]
        public int ItemPickedRacksQty { get; set; }

        [DataMember(Order = 548, EmitDefaultValue = true)]
        public decimal ItemPickedGlassWeight { get; set; }

        [DataMember(Order = 548, EmitDefaultValue = true)]
        public decimal ItemPickedTareWeight { get; set; }

        [DataMember(Order = 550, EmitDefaultValue = true)]
        public int ItemLoadedRacksQty { get; set; }

        [DataMember(Order = 552, EmitDefaultValue = true)]
        public decimal ItemLoadedGlassWeight { get; set; }

        [DataMember(Order = 554, EmitDefaultValue = true)]
        public decimal ItemLoadedTareWeight { get; set; }

        [DataMember(Order = 558, EmitDefaultValue = true)]
        public string ItemInventoryDesc{ get; set; }


        // Glass
        [DataMember(Order = 600, EmitDefaultValue = false)]
        public GlassTypes GlassTypes { get; set; }

        // Item Attributes
        public Dictionary<string, string> AttributesDict { get; set; }
        public ItemAttributes ItemAttributes { get; set; }

        // Flex Descriptions
        [DataMember(Order = 800, EmitDefaultValue = false)]
        public string Dimensions { get; set; }

        [DataMember(Order = 802, EmitDefaultValue = false)]
        public string Lite1GlassType { get; set; }

        [DataMember(Order = 804, EmitDefaultValue = false)]
        public string Lite2GlassType { get; set; }

        [DataMember(Order = 806, EmitDefaultValue = false)]
        public string Lite3GlassType { get; set; }

        [DataMember(Order = 808, EmitDefaultValue = false)]
        public string LiteRouting { get; set; }

        [DataMember(Order = 810, EmitDefaultValue = false)]
        public string Airspace { get; set; }

        [DataMember(Order = 812, EmitDefaultValue = false)]
        public string SpacerType { get; set; }

        [DataMember(Order = 814, EmitDefaultValue = false)]
        public string BarAssembly { get; set; }

        [DataMember(Order = 816, EmitDefaultValue = false)]
        public string Grid { get; set; }

        [DataMember(Order = 818, EmitDefaultValue = false)]
        public string GridIndexes { get; set; }

        [DataMember(Order = 819, EmitDefaultValue = false)]
        public string PreserveProcessing { get; set; }

        [DataMember(Order = 820, EmitDefaultValue = false)]
        public string Logo { get; set; }

        [DataMember(Order = 822, EmitDefaultValue = false)]
        public string EdgeProcessing { get; set; }

        [DataMember(Order = 824, EmitDefaultValue = false)]
        public string HoleProcessing { get; set; }

        [DataMember(Order = 826, EmitDefaultValue = false)]
        public string VGrooveProcessing { get; set; }

        [DataMember(Order = 850, EmitDefaultValue = false)]
        public string ConfigHash { get; set; }

        [DataMember(Order = 851, EmitDefaultValue = false)]
        public ConfigAggregate ConfigAggregate { get; set; }

        // Config Aggregates
        [DataMember(Order = 1000, EmitDefaultValue = false)]
        public int ConfigHashCount { get; set; }

        [DataMember(Order = 1001, EmitDefaultValue = false)]
        public decimal Lite1TotalWeightAggregate { get; set; }

        [DataMember(Order = 1002, EmitDefaultValue = false)]
        public decimal Lite2TotalWeightAggregate { get; set; }

        [DataMember(Order = 1002, EmitDefaultValue = false)]
        public decimal Lite3TotalWeightAggregate { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string AllLitesPurchased { get; set; }
    }


    [DataContract(Namespace = "")]
    public class ConfigAggregate
    {

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public int ConfigHashCount { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public decimal Lite1TotalWeightAggregate { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public decimal Lite2TotalWeightAggregate { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public decimal Lite3TotalWeightAggregate { get; set; }

        [DataMember(Order = 42, EmitDefaultValue = false)]
        public decimal MaximumLiteWeightAggregate { get; set; }

        [DataMember(Order = 44, EmitDefaultValue = false)]
        public decimal TotalGlassWeightAggregate { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public string AllLitesPurchased { get; set; }
    }
}
