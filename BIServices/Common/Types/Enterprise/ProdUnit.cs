using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class ProdUnits : List<ProdUnit>
    {
    }

    [DataContract(Namespace = "")]
    public class ProdUnit
    {
        // Key
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Plant { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public string PlantCode { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public decimal WOrderNum { get; set; }

        [DataMember(Order = 22, EmitDefaultValue = false)]
        public string LotNum { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public decimal UnitId { get; set; }

        [DataMember(Order = 32, EmitDefaultValue = false)]
        public string SerialNum { get; set; }

        // Status Codes
        [DataMember(Order = 100, EmitDefaultValue = false)]
        public string RackDetailStatusCode { get; set; }

        [DataMember(Order = 102, EmitDefaultValue = false)]
        public string RackStatusCode { get; set; }

        [DataMember(Order = 104, EmitDefaultValue = false)]
        public string WOrderStatusCode { get; set; }

        [DataMember(Order = 106, EmitDefaultValue = false)]
        public string SOrderLastStatusCode { get; set; }

        [DataMember(Order = 108, EmitDefaultValue = false)]
        public string SOrderNextStatusCode { get; set; }

        [DataMember(Order = 110, EmitDefaultValue = false)]
        public string MfgSource { get; set; }

        [DataMember(Order = 112, EmitDefaultValue = false)]
        public string UnitOriginCode { get; set; }

        // Related Dates
        [DataMember(Order = 150, EmitDefaultValue = false)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 152, EmitDefaultValue = false)]
        public DateTime DueDate { get; set; }

        [DataMember(Order = 154, EmitDefaultValue = false)]
        public DateTime OrigDueDate { get; set; }

        [DataMember(Order = 156, EmitDefaultValue = false)]
        public DateTime DeliveryDateTime { get; set; }

        [DataMember(Order = 158, EmitDefaultValue = false)]
        public double DeliveryDateTimeHours { get; set; }

        [DataMember(Order = 160, EmitDefaultValue = false)]
        public DateTime ShipDate { get; set; }

        [DataMember(Order = 162, EmitDefaultValue = false)]
        public DateTime ShipDateTime { get; set; }

        // Customer
        [DataMember(Order = 200, EmitDefaultValue = false)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 202, EmitDefaultValue = false)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 204, EmitDefaultValue = false)]
        public string CustomerSoldType { get; set; }

        [DataMember(Order = 206, EmitDefaultValue = false)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 208, EmitDefaultValue = false)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 250, EmitDefaultValue = false)]
        public string CustomerTier { get; set; }

        [DataMember(Order = 252, EmitDefaultValue = false)]
        public string ShipRoute { get; set; }

        [DataMember(Order = 254, EmitDefaultValue = false)]
        public DateTime CustomerProductionDate { get; set; }

        [DataMember(Order = 256, EmitDefaultValue = false)]
        public DateTime CustomerOrigProductionDate { get; set; }

        // Related Sales Order
        [DataMember(Order = 300, EmitDefaultValue = false)]
        public decimal SOrderNum { get; set; }

        [DataMember(Order = 302, EmitDefaultValue = false)]
        public string SOrderType { get; set; }

        [DataMember(Order = 304, EmitDefaultValue = false)]
        public decimal SOrderLineNum { get; set; }

        [DataMember(Order = 306, EmitDefaultValue = false)]
        public bool IsInternalOrder { get; set; }

        [DataMember(Order = 310, EmitDefaultValue = false)]
        public string CustomerOrderNum { get; set; }

        [DataMember(Order = 312, EmitDefaultValue = false)]
        public string CustomerPart1 { get; set; }

        [DataMember(Order = 314, EmitDefaultValue = false)]
        public string CustomerPart2 { get; set; }

        [DataMember(Order = 316, EmitDefaultValue = false)]
        public string CustomerPart3 { get; set; }

        [DataMember(Order = 318, EmitDefaultValue = false)]
        public string CustomerSeqNum { get; set; }

        [DataMember(Order = 320, EmitDefaultValue = false)]
        public string CustomerSeqPart1 { get; set; }

        [DataMember(Order = 322, EmitDefaultValue = false)]
        public string GenericSeqNum { get; set; }

        [DataMember(Order = 324, EmitDefaultValue = false)]
        public string ProductType { get; set; }

        [DataMember(Order = 326, EmitDefaultValue = false)]
        public string CardinalProductType { get; set; }

        [DataMember(Order = 328, EmitDefaultValue = false)]
        public string ProjectName { get; set; }

        [DataMember(Order = 330, EmitDefaultValue = false)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 332, EmitDefaultValue = false)]
        public string ReasonCode { get; set; }

        [DataMember(Order = 334, EmitDefaultValue = false)]
        public string PickNotes { get; set; }

        [DataMember(Order = 336, EmitDefaultValue = false)]
        public string PickNotes1 { get; set; }

        [DataMember(Order = 338, EmitDefaultValue = false)]
        public string PickNotes2 { get; set; }

        [DataMember(Order = 340, EmitDefaultValue = false)]
        public string PackNotes { get; set; }

        [DataMember(Order = 342, EmitDefaultValue = false)]
        public string SchedulingMessage1 { get; set; }

        [DataMember(Order = 344, EmitDefaultValue = false)]
        public decimal UnitActualPriceAmt { get; set; }

        [DataMember(Order = 346, EmitDefaultValue = false)]
        public decimal UnitListPriceAmt { get; set; }

        [DataMember(Order = 348, EmitDefaultValue = false)]
        public decimal UnitCostAmt { get; set; }

        [DataMember(Order = 350, EmitDefaultValue = false)]
        public decimal LoadNum { get; set; }

        [DataMember(Order = 352, EmitDefaultValue = false)]
        public string QcInspect { get; set; }

        [DataMember(Order = 354, EmitDefaultValue = false)]
        public bool IsPick { get; set; }

        // Item
        [DataMember(Order = 400, EmitDefaultValue = false)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 402, EmitDefaultValue = false)]
        public string StockTypeCode { get; set; }

        [DataMember(Order = 404, EmitDefaultValue = false)]
        public string VolumeCode { get; set; }

        [DataMember(Order = 406, EmitDefaultValue = false)]
        public string UnitTypeCode { get; set; }

        [DataMember(Order = 408, EmitDefaultValue = false)]
        public int LitesNum { get; set; }

        // Scheduling
        [DataMember(Order = 500, EmitDefaultValue = false)]
        public int Attempts { get; set; }

        [DataMember(Order = 501, EmitDefaultValue = false)]
        public decimal OrigScheduleNum { get; set; }

        [DataMember(Order = 502, EmitDefaultValue = false)]
        public DateTime OrigScheduleDate { get; set; }

        [DataMember(Order = 504, EmitDefaultValue = false)]
        public string OrigScheduleType { get; set; }

        [DataMember(Order = 506, EmitDefaultValue = false)]
        public decimal OrigScheduleRackId { get; set; }

        [DataMember(Order = 508, EmitDefaultValue = false)]
        public string OrigScheduleRackStatusCode { get; set; }

        [DataMember(Order = 510, EmitDefaultValue = false)]
        public string OrigScheduleProductionLine { get; set; }

        [DataMember(Order = 520, EmitDefaultValue = false)]
        public decimal RemakeScheduleNum { get; set; }

        [DataMember(Order = 522, EmitDefaultValue = false)]
        public DateTime? RemakeScheduleDate { get; set; }

        [DataMember(Order = 524, EmitDefaultValue = false)]
        public string RemakeScheduleReason { get; set; }

        [DataMember(Order = 526, EmitDefaultValue = false)]
        public string RemakeScheduleComment { get; set; }

        [DataMember(Order = 540, EmitDefaultValue = false)]
        public decimal CurrScheduleNum { get; set; }

        [DataMember(Order = 542, EmitDefaultValue = false)]
        public DateTime CurrScheduleDate { get; set; }

        [DataMember(Order = 550, EmitDefaultValue = false)]
        public int TargetLineId { get; set; }

        [DataMember(Order = 552, EmitDefaultValue = false)]
        public int TargetShift { get; set; }

        [DataMember(Order = 554, EmitDefaultValue = false)]
        public DateTime TargetShiftDate { get; set; }

        // Boxing
        [DataMember(Order = 600, EmitDefaultValue = false)]
        public string WipRack1 { get; set; }

        [DataMember(Order = 602, EmitDefaultValue = false)]
        public int WipSlot1 { get; set; }

        [DataMember(Order = 604, EmitDefaultValue = false)]
        public string WipRack2 { get; set; }

        [DataMember(Order = 606, EmitDefaultValue = false)]
        public int WipSlot2 { get; set; }

        [DataMember(Order = 608, EmitDefaultValue = false)]
        public string WipRack3 { get; set; }

        [DataMember(Order = 610, EmitDefaultValue = false)]
        public int WipSlot3 { get; set; }

        [DataMember(Order = 612, EmitDefaultValue = false)]
        public string WipRack4 { get; set; }

        [DataMember(Order = 614, EmitDefaultValue = false)]
        public int WipSlot4 { get; set; }

        [DataMember(Order = 616, EmitDefaultValue = false)]
        public string VirtualRackName { get; set; }

        [DataMember(Order = 618, EmitDefaultValue = false)]
        public int VirtualSlotNum { get; set; }

        [DataMember(Order = 620, EmitDefaultValue = false)]
        public int VirtualSideNum { get; set; }

        [DataMember(Order = 622, EmitDefaultValue = false)]
        public int VirtualStackNum { get; set; }

        [DataMember(Order = 624, EmitDefaultValue = false)]
        public int RunSequence { get; set; }

        // Racking
        [DataMember(Order = 700, EmitDefaultValue = false)]
        public decimal RackId { get; set; }

        [DataMember(Order = 702, EmitDefaultValue = false)]
        public string RackType { get; set; }

        [DataMember(Order = 704, EmitDefaultValue = false)]
        public string RackAssetId { get; set; }

        [DataMember(Order = 706, EmitDefaultValue = false)]
        public string RackLocation { get; set; }

        [DataMember(Order = 708, EmitDefaultValue = false)]
        public decimal RackScheduleNum { get; set; }

        [DataMember(Order = 710, EmitDefaultValue = false)]
        public decimal RackTareWeight { get; set; }

        [DataMember(Order = 712, EmitDefaultValue = false)]
        public decimal RackBoxNum { get; set; }

        [DataMember(Order = 714, EmitDefaultValue = false)]
        public DateTime? RackCloseDate { get; set; }

        [DataMember(Order = 716, EmitDefaultValue = false)]
        public DateTime? RackLockDate { get; set; }

        [DataMember(Order = 718, EmitDefaultValue = false)]
        public DateTime? RackLockLoadDate { get; set; }

        [DataMember(Order = 720, EmitDefaultValue = false)]
        public DateTime RackTransactionDate { get; set; }

        [DataMember(Order = 722, EmitDefaultValue = false)]
        public string RackSide { get; set; }

        [DataMember(Order = 724, EmitDefaultValue = false)]
        public string RackStack { get; set; }

        [DataMember(Order = 726, EmitDefaultValue = false)]
        public string RackRow { get; set; }

        [DataMember(Order = 728, EmitDefaultValue = false)]
        public decimal SlotNum { get; set; }

        [DataMember(Order = 730, EmitDefaultValue = false)]
        public DateTime? RackDetailTransactionDate { get; set; }

        [DataMember(Order = 732, EmitDefaultValue = false)]
        public string IsRackOrphan { get; set; }

        // Unit Construction
        [DataMember(Order = 800, EmitDefaultValue = false)]
        public string ProductCode { get; set; }

        [DataMember(Order = 802, EmitDefaultValue = false)]
        public string ProductCodeDesc { get; set; }

        [DataMember(Order = 850, EmitDefaultValue = false)]
        public string IsLami { get; set; }

        [DataMember(Order = 852, EmitDefaultValue = false)]
        public string IsFabrication { get; set; }

        [DataMember(Order = 854, EmitDefaultValue = false)]
        public string IsScored { get; set; }
        
        [DataMember(Order = 856, EmitDefaultValue = false)]
        public string IsTubed { get; set; }

        [DataMember(Order = 858, EmitDefaultValue = false)]
        public string HasPurchasedLite { get; set; }

        // Shape
        [DataMember(Order = 900, EmitDefaultValue = false)]
        public int ShapeNum { get; set; }

        [DataMember(Order = 902, EmitDefaultValue = false)]
        public string ShapeGroupName { get; set; }

        [DataMember(Order = 904, EmitDefaultValue = false)]
        public decimal BaseDimension { get; set; }

        [DataMember(Order = 906, EmitDefaultValue = false)]
        public decimal LeftDimension { get; set; }

        [DataMember(Order = 908, EmitDefaultValue = false)]
        public decimal RightDimension { get; set; }

        [DataMember(Order = 910, EmitDefaultValue = false)]
        public decimal TopDimension { get; set; }

        [DataMember(Order = 912, EmitDefaultValue = false)]
        public decimal Side1Dimension { get; set; }

        [DataMember(Order = 914, EmitDefaultValue = false)]
        public decimal Side2Dimension { get; set; }

        [DataMember(Order = 916, EmitDefaultValue = false)]
        public string DimensionDesc { get; set; }

        [DataMember(Order = 918, EmitDefaultValue = false)]
        public string BaseDimensionDesc { get; set; }

        [DataMember(Order = 920, EmitDefaultValue = false)]
        public string LeftDimensionDesc { get; set; }

        [DataMember(Order = 922, EmitDefaultValue = false)]
        public string RightDimensionDesc { get; set; }

        [DataMember(Order = 924, EmitDefaultValue = false)]
        public string TopDimensionDesc { get; set; }

        [DataMember(Order = 926, EmitDefaultValue = false)]
        public string Side1DimensionDesc { get; set; }

        [DataMember(Order = 928, EmitDefaultValue = false)]
        public string Side2DimensionDesc { get; set; }

        [DataMember(Order = 930, EmitDefaultValue = false)]
        public decimal UnitActualSqft { get; set; }

        [DataMember(Order = 932, EmitDefaultValue = false)]
        public decimal UnitBillableSqft { get; set; }

        [DataMember(Order = 934, EmitDefaultValue = false)]
        public decimal UnitWeight { get; set; }

        // Parts Detail
        [DataMember(Order = 1000, EmitDefaultValue = false)]
        public string Glass1 { get; set; }

        [DataMember(Order = 1002, EmitDefaultValue = false)]
        public string Glass2 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Glass3 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Glass4 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Spacer1 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Spacer2 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Interlayer1 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Interlayer2 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Interlayer3 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Interlayer4 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string BarDescription { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Air1MCode1 { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string GridStyleCode { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string GridStyleGroupName { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Lite1Routing { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Lite2Routing { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Lite3Routing { get; set; }

        // Production Events
        [DataMember(Order = 2000, EmitDefaultValue = false)]
        public string ProductionStatus { get; set; }

        [DataMember(Order = 904, EmitDefaultValue = false)]
        public string LastProductionLineCode { get; set; }

        [DataMember(Order = 906, EmitDefaultValue = false)]
        public string LastLineOperationCode { get; set; }

        [DataMember(Order = 908, EmitDefaultValue = false)]
        public string LastTransactionCode { get; set; }

        [DataMember(Order = 910, EmitDefaultValue = false)]
        public string LastProductionDesc { get; set; }

        [DataMember(Order = 912, EmitDefaultValue = false)]
        public DateTime? LastProductionDate { get; set; }

        [DataMember(Order = 914, EmitDefaultValue = false)]
        public string LastMachineName { get; set; }

        [DataMember(Order = 916, EmitDefaultValue = false)]
        public string LastMachineLineCode { get; set; }

        [DataMember(Order = 918, EmitDefaultValue = false)]
        public DateTime? LastMachineDate { get; set; }

        [DataMember(Order = 920, EmitDefaultValue = false)]
        public string LastMachineDesc { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Lite1RecdStatus { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Lite2RecdStatus { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = false)]
        public string Lite3RecdStatus { get; set; }

        // Unit Metrics
        [DataMember(Order = 5000, EmitDefaultValue = false)]
        public int Units { get; set; }

        [DataMember(Order = 5000, EmitDefaultValue = false)]
        public int MonoUnits { get; set; }

        [DataMember(Order = 5000, EmitDefaultValue = false)]
        public int DoublePaneUnits { get; set; }

        [DataMember(Order = 5000, EmitDefaultValue = false)]
        public int TriplePaneUnits { get; set; }

        [DataMember(Order = 5014, EmitDefaultValue = false)]
        public int PickUnits { get; set; }

        [DataMember(Order = 5014, EmitDefaultValue = false)]
        public int PickUnitsAvl { get; set; }

        [DataMember(Order = 5002, EmitDefaultValue = false)]
        public int OnHand { get; set; }

        [DataMember(Order = 5004, EmitDefaultValue = false)]
        public int TubeUnits { get; set; }

        [DataMember(Order = 5006, EmitDefaultValue = false)]
        public int ShapeUnits { get; set; }

        [DataMember(Order = 5008, EmitDefaultValue = false)]
        public int PreserveUnits { get; set; }

        [DataMember(Order = 5010, EmitDefaultValue = false)]
        public int ScoringUnits { get; set; }

        [DataMember(Order = 5012, EmitDefaultValue = false)]
        public int BarUnits { get; set; }

        [DataMember(Order = 5012, EmitDefaultValue = false)]
        public int LamiUnits { get; set; }

        [DataMember(Order = 5016, EmitDefaultValue = false)]
        public int PurchasedLiteUnits { get; set; }

        [DataMember(Order = 5200, EmitDefaultValue = false)]
        public int UnrackedUnits { get; set; }

        [DataMember(Order = 5202, EmitDefaultValue = false)]
        public int UncompletedUnits { get; set; }

        [DataMember(Order = 5204, EmitDefaultValue = false)]
        public int CompletedUnits { get; set; }

        [DataMember(Order = 5206, EmitDefaultValue = false)]
        public int ShippedUnits { get; set; }

        // Component Metrics
        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int CutDemandLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int CutCompletedLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal CutDemandSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal CutCompletedSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int Temper1DemandLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int Temper1CompletedLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper1DemandSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper1CompletedSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper1DemandTons { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper1CompletedTons { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int Temper2DemandLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int Temper2CompletedLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper2DemandSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper2CompletedSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper2DemandTons { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal Temper2CompletedTons { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int PickDemandLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int PickCompletedLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal PickDemandSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal PickCompletedSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int PurchDemandLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int PurchReceivedLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal PurchDemandSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal PurchReceivedSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int HolesDemandLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int HolesCompletedLites { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal HolesDemandSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal HolesCompletedSqft { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int ChannelDemandAirspaces { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal ChannelDemandInches { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public int ChannelCompletedAirspaces { get; set; }

        [DataMember(Order = 6000, EmitDefaultValue = false)]
        public decimal ChannelCompletedInches { get; set; }

        // Metadata
        [DataMember(Order = 7000, EmitDefaultValue = false)]
        public string CategoryCode { get; set; }

        [DataMember(Order = 7002, EmitDefaultValue = false)]
        public DateTime UpdateDate { get; set; }


    }
}



