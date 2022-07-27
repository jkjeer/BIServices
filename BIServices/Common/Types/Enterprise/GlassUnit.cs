using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class GlassUnits : List<GlassUnit>
    {
    }

    [DataContract(Namespace = "")]
    public class GlassUnit
    {
        // Key
        [DataMember(Order = 10, EmitDefaultValue = true)]
        public string Plant { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = true)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = true)]
        public string SerialNum { get; set; }

        [DataMember(Order = 14, EmitDefaultValue = true)]
        public string LotNum { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = true)]
        public decimal UnitId { get; set; }

        [DataMember(Order = 18, EmitDefaultValue = true)]
        public string UnitIdChar { get; set; }

        // Rack
        [DataMember(Order = 50, EmitDefaultValue = true)]
        public decimal RackId { get; set; }

        [DataMember(Order = 52, EmitDefaultValue = true)]
        public string RackType { get; set; }

        [DataMember(Order = 54, EmitDefaultValue = true)]
        public string AssetId { get; set; }

        [DataMember(Order = 56, EmitDefaultValue = true)]
        public string RackLocation { get; set; }

        [DataMember(Order = 57, EmitDefaultValue = true)]
        public decimal RackScheduleNum { get; set; }

        [DataMember(Order = 58, EmitDefaultValue = true)]
        public decimal RackTareWeight { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = true)]
        public decimal BoxNum { get; set; }

        [DataMember(Order = 62, EmitDefaultValue = true)]
        public string RackStatusCode { get; set; }

        [DataMember(Order = 63, EmitDefaultValue = true)]
        public string RackStatus { get; set; }

        [DataMember(Order = 66, EmitDefaultValue = true)]
        public DateTime? RackCloseDate { get; set; }

        [DataMember(Order = 68, EmitDefaultValue = true)]
        public DateTime? RackLockDate { get; set; }

        [DataMember(Order = 70, EmitDefaultValue = true)]
        public DateTime? RackLockLoadDate { get; set; }

        [DataMember(Order = 72, EmitDefaultValue = true)]
        public string RackFirstCustSeqNum { get; set; }

        [DataMember(Order = 74, EmitDefaultValue = true)]
        public string RackLastCustSeqNum { get; set; }

        [DataMember(Order = 76, EmitDefaultValue = true)]
        public DateTime RackTransactionDate { get; set; }

        // Rack Item 
        [DataMember(Order = 100, EmitDefaultValue = true)]
        public string RackSide { get; set; }

        [DataMember(Order = 102, EmitDefaultValue = true)]
        public string RackStack { get; set; }

        [DataMember(Order = 104, EmitDefaultValue = true)]
        public string RackRow { get; set; }

        [DataMember(Order = 106, EmitDefaultValue = true)]
        public decimal SlotNum { get; set; }

        [DataMember(Order = 108, EmitDefaultValue = true)]
        public string Status { get; set; }

        [DataMember(Order = 110, EmitDefaultValue = true)]
        public string RackItemStatusCode { get; set; }

        [DataMember(Order = 111, EmitDefaultValue = true)]
        public string RackDetailStatusCode { get; set; }

        [DataMember(Order = 112, EmitDefaultValue = true)]
        public DateTime? RackItemDate { get; set; }

        // Wip Racking
        [DataMember(Order = 150, EmitDefaultValue = true)]
        public string WipRack1 { get; set; }

        [DataMember(Order = 152, EmitDefaultValue = true)]
        public int WipSlot1 { get; set; }

        [DataMember(Order = 154, EmitDefaultValue = true)]
        public string WipRack2 { get; set; }

        [DataMember(Order = 156, EmitDefaultValue = true)]
        public int WipSlot2 { get; set; }

        [DataMember(Order = 158, EmitDefaultValue = true)]
        public string WipRack3 { get; set; }

        [DataMember(Order = 160, EmitDefaultValue = true)]
        public int WipSlot3 { get; set; }

        [DataMember(Order = 162, EmitDefaultValue = true)]
        public string WipRack4 { get; set; }

        [DataMember(Order = 164, EmitDefaultValue = true)]
        public int WipSlot4 { get; set; }

        [DataMember(Order = 170, EmitDefaultValue = true)]
        public string VirtualRackName { get; set; }

        [DataMember(Order = 172, EmitDefaultValue = true)]
        public int VirtualSlotNum { get; set; }

        [DataMember(Order = 174, EmitDefaultValue = true)]
        public int VirtualSideNum { get; set; }

        [DataMember(Order = 176, EmitDefaultValue = true)]
        public int VirtualStackNum { get; set; }

        // CUSTOMER PROPERTIES

        [DataMember(Order = 200, EmitDefaultValue = true)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 201, EmitDefaultValue = true)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 202, EmitDefaultValue = true)]
        public string CustomerName { get; set; }

        [DataMember(Order = 203, EmitDefaultValue = true)]
        public string CustomerSoldType { get; set; }

        [DataMember(Order = 204, EmitDefaultValue = true)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 206, EmitDefaultValue = true)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 208, EmitDefaultValue = true)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 209, EmitDefaultValue = true)]
        public decimal OrigCustomerNum { get; set; }

        [DataMember(Order = 209, EmitDefaultValue = true)]
        public string OrigCustomerName { get; set; }

        [DataMember(Order = 210, EmitDefaultValue = true)]
        public string CustomerNameUnitBox { get; set; }

        [DataMember(Order = 212, EmitDefaultValue = true)]
        public string ShipRoute { get; set; }

        [DataMember(Order = 214, EmitDefaultValue = true)]
        public string CustomerTier { get; set; }

        [DataMember(Order = 220, EmitDefaultValue = true)]
        public string CustomerPO { get; set; }

        [DataMember(Order = 222, EmitDefaultValue = true)]
        public decimal CustomerPONum { get; set; }

        [DataMember(Order = 224, EmitDefaultValue = true)]
        public decimal CustomerPOLineNum { get; set; }

        [DataMember(Order = 226, EmitDefaultValue = true)]
        public string CustomerPart1 { get; set; }

        [DataMember(Order = 228, EmitDefaultValue = true)]
        public string CustomerPart2 { get; set; }

        [DataMember(Order = 230, EmitDefaultValue = true)]
        public string CustomerPart3 { get; set; }

        [DataMember(Order = 232, EmitDefaultValue = true)]
        public string CustomerSeqNum { get; set; }

        [DataMember(Order = 234, EmitDefaultValue = true)]
        public string CustomerSeqPart1 { get; set; }

        [DataMember(Order = 236, EmitDefaultValue = true)]
        public string GenericSeqNum { get; set; }

        [DataMember(Order = 240, EmitDefaultValue = true)]
        public string CardinalProductType { get; set; }

        [DataMember(Order = 242, EmitDefaultValue = true)]
        public string ProjectName { get; set; }

        [DataMember(Order = 242, EmitDefaultValue = true)]
        public string PriceCode1 { get; set; }

        [DataMember(Order = 242, EmitDefaultValue = true)]
        public string PriceCode2 { get; set; }

        [DataMember(Order = 242, EmitDefaultValue = true)]
        public string PriceCode3 { get; set; }

        [DataMember(Order = 244, EmitDefaultValue = true)]
        public string Originator { get; set; }

        [DataMember(Order = 250, EmitDefaultValue = true)]
        public decimal OrigWOrderNum { get; set; }

        [DataMember(Order = 252, EmitDefaultValue = true)]
        public DateTime CustomerProductionDate { get; set; }

        // Related Documents
        [DataMember(Order = 250, EmitDefaultValue = true)]
        public decimal OrderNum { get; set; }

        [DataMember(Order = 251, EmitDefaultValue = true)]
        public decimal SOrderNum { get; set; }

        [DataMember(Order = 252, EmitDefaultValue = true)]
        public string OrderType { get; set; }

        [DataMember(Order = 253, EmitDefaultValue = true)]
        public string SOrderType { get; set; }

        [DataMember(Order = 254, EmitDefaultValue = true)]
        public decimal OrderLineNum { get; set; }

        [DataMember(Order = 255, EmitDefaultValue = true)]
        public decimal SOrderLineNum { get; set; }

        [DataMember(Order = 256, EmitDefaultValue = true)]
        public string OrderCompany { get; set; }

        [DataMember(Order = 257, EmitDefaultValue = true)]
        public string SOrderCompany { get; set; }

        [DataMember(Order = 258, EmitDefaultValue = true)]
        public string NextStatus { get; set; }

        [DataMember(Order = 259, EmitDefaultValue = true)]
        public string SOrderNextStatus { get; set; }

        [DataMember(Order = 270, EmitDefaultValue = true)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 271, EmitDefaultValue = true)]
        public decimal WOrderNum { get; set; }

        [DataMember(Order = 272, EmitDefaultValue = true)]
        public string WorkOrderStatus { get; set; }

        [DataMember(Order = 273, EmitDefaultValue = true)]
        public string WOrderStatusCode { get; set; }

        [DataMember(Order = 276, EmitDefaultValue = true)]
        public decimal EndPickItemNum { get; set; }

        // Order
        [DataMember(Order = 300, EmitDefaultValue = true)]
        public bool IsInternalOrder { get; set; }

        [DataMember(Order = 302, EmitDefaultValue = true)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 304, EmitDefaultValue = true)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 306, EmitDefaultValue = true)]
        public DateTime OrigRequestedDate { get; set; }

        [DataMember(Order = 308, EmitDefaultValue = true)]
        public DateTime PromisedDate { get; set; }

        [DataMember(Order = 310, EmitDefaultValue = true)]
        public DateTime DeliveryDateTime { get; set; }

        [DataMember(Order = 312, EmitDefaultValue = true)]
        public double DeliveryDateTimeHours { get; set; }

        [DataMember(Order = 320, EmitDefaultValue = true)]
        public string VolumeCode { get; set; }

        [DataMember(Order = 322, EmitDefaultValue = true)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 324, EmitDefaultValue = true)]
        public string PickNotes { get; set; }

        [DataMember(Order = 326, EmitDefaultValue = true)]
        public string PickNotes1 { get; set; }

        [DataMember(Order = 328, EmitDefaultValue = true)]
        public string PickNotes2 { get; set; }

        [DataMember(Order = 330, EmitDefaultValue = true)]
        public string PackNotes { get; set; }

        [DataMember(Order = 332, EmitDefaultValue = true)]
        public string SchedulingMessage { get; set; }

        [DataMember(Order = 334, EmitDefaultValue = true)]
        public DateTime ShipDateTime { get; set; }

        [DataMember(Order = 336, EmitDefaultValue = true)]
        public int RunSequence { get; set; }

        [DataMember(Order = 338, EmitDefaultValue = true)]
        public string MfgSource { get; set; }

        [DataMember(Order = 340, EmitDefaultValue = true)]
        public string QcInspect { get; set; }

        [DataMember(Order = 350, EmitDefaultValue = true)]
        public decimal UnitCostAmt { get; set; }

        [DataMember(Order = 352, EmitDefaultValue = true)]
        public decimal UnitActualPriceAmt { get; set; }

        [DataMember(Order = 354, EmitDefaultValue = true)]
        public decimal UnitListPriceAmt { get; set; }

        // Scheduling
        [DataMember(Order = 400, EmitDefaultValue = true)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 402, EmitDefaultValue = true)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 403, EmitDefaultValue = true)]
        public string ScheduleName { get; set; }

        [DataMember(Order = 404, EmitDefaultValue = true)]
        public DateTime ScheduleDate { get; set; }

        [DataMember(Order = 406, EmitDefaultValue = true)]
        public decimal OriginalScheduleNum { get; set; }

        [DataMember(Order = 408, EmitDefaultValue = true)]
        public DateTime OriginalScheduleDate { get; set; }

        [DataMember(Order = 410, EmitDefaultValue = true)]
        public decimal RemakeScheduleNum { get; set; }

        [DataMember(Order = 412, EmitDefaultValue = true)]
        public DateTime? RemakeInitiateDate { get; set; }

        [DataMember(Order = 414, EmitDefaultValue = true)]
        public string IGLineScheduled { get; set; }

        [DataMember(Order = 416, EmitDefaultValue = true)]
        public string ScheduledLineId { get; set; }

        [DataMember(Order = 416, EmitDefaultValue = true)]
        public int ScheduledLineNum { get; set; }

        [DataMember(Order = 418, EmitDefaultValue = true)]
        public decimal ScheduledRackId { get; set; }

        [DataMember(Order = 419, EmitDefaultValue = true)]
        public string ScheduledRackStatusCode { get; set; }

        [DataMember(Order = 420, EmitDefaultValue = true)]
        public int Passes { get; set; }

        // Item
        [DataMember(Order = 500, EmitDefaultValue = true)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 502, EmitDefaultValue = true)]
        public string ItemStockType { get; set; }

        [DataMember(Order = 504, EmitDefaultValue = true)]
        public string ProductType { get; set; }

        [DataMember(Order = 506, EmitDefaultValue = true)]
        public string UnitType { get; set; }

        // Construction

        [DataMember(Order = 550, EmitDefaultValue = true)]
        public string ProductCode { get; set; }

        [DataMember(Order = 551, EmitDefaultValue = true)]
        public string Construction { get; set; }

        [DataMember(Order = 552, EmitDefaultValue = true)]
        public int ShapeNum { get; set; }

        [DataMember(Order = 554, EmitDefaultValue = true)]
        public decimal UnitActualSqft { get; set; }

        [DataMember(Order = 555, EmitDefaultValue = true)]
        public decimal UnitBillableSqft { get; set; }

        [DataMember(Order = 556, EmitDefaultValue = true)]
        public string DimMakeup { get; set; }

        [DataMember(Order = 558, EmitDefaultValue = true)]
        public double BaseDim { get; set; }

        [DataMember(Order = 560, EmitDefaultValue = true)]
        public string BaseDimChar { get; set; }

        [DataMember(Order = 562, EmitDefaultValue = true)]
        public double LeftDim { get; set; }

        [DataMember(Order = 564, EmitDefaultValue = true)]
        public string LeftDimChar { get; set; }

        [DataMember(Order = 566, EmitDefaultValue = true)]
        public decimal GlassWeight { get; set; }

        [DataMember(Order = 568, EmitDefaultValue = true)]
        public string BarDescription { get; set; }

        [DataMember(Order = 570, EmitDefaultValue = true)]
        public string IsLami { get; set; }

        [DataMember(Order = 572, EmitDefaultValue = true)]
        public string IsFabrication { get; set; }

        // Parts
        [DataMember(Order = 700, EmitDefaultValue = true)]
        public string Lite1Routing { get; set; }

        [DataMember(Order = 702, EmitDefaultValue = true)]
        public string Lite2Routing { get; set; }

        [DataMember(Order = 704, EmitDefaultValue = true)]
        public string Lite3Routing { get; set; }

        [DataMember(Order = 706, EmitDefaultValue = true)]
        public string IsPurchasedLite { get; set; }

        [DataMember(Order = 710, EmitDefaultValue = true)]
        public string Glass1Code { get; set; }

        [DataMember(Order = 712, EmitDefaultValue = true)]
        public string Glass2Code { get; set; }

        [DataMember(Order = 714, EmitDefaultValue = true)]
        public string Glass3Code { get; set; }

        [DataMember(Order = 716, EmitDefaultValue = true)]
        public string Glass4Code { get; set; }

        [DataMember(Order = 720, EmitDefaultValue = true)]
        public string Interlayer1Code { get; set; }

        [DataMember(Order = 722, EmitDefaultValue = true)]
        public string Interlayer2Code { get; set; }

        [DataMember(Order = 724, EmitDefaultValue = true)]
        public string Interlayer3Code { get; set; }

        // Production State
        [DataMember(Order = 750, EmitDefaultValue = true)]
        public string StatusPointName { get; set; }

        [DataMember(Order = 751, EmitDefaultValue = true)]
        public string ProductionStatus { get; set; }

        [DataMember(Order = 752, EmitDefaultValue = true)]
        public string IsMissing { get; set; }

        // Remakes
        [DataMember(Order = 800, EmitDefaultValue = true)]
        public int RemakeId { get; set; }

        [DataMember(Order = 802, EmitDefaultValue = true)]
        public string RemakeReason { get; set; }

        [DataMember(Order = 804, EmitDefaultValue = true)]
        public string RemakeComment { get; set; }

        // Production Transactions
        [DataMember(Order = 850, EmitDefaultValue = true)]
        public string LastProductionLine { get; set; }

        [DataMember(Order = 852, EmitDefaultValue = true)]
        public string LastLineOperation { get; set; }

        [DataMember(Order = 854, EmitDefaultValue = true)]
        public string LastTransactionCode { get; set; }

        [DataMember(Order = 856, EmitDefaultValue = true)]
        public DateTime LastLineDate { get; set; }

        [DataMember(Order = 858, EmitDefaultValue = true)]
        public int IGLineRun { get; set; }

        [DataMember(Order = 860, EmitDefaultValue = true)]
        public string LastLine { get; set; }

        // Machine Transactions
        [DataMember(Order = 900, EmitDefaultValue = true)]
        public string SealerStatus { get; set; }

        [DataMember(Order = 902, EmitDefaultValue = true)]
        public string LastMachineName { get; set; }

        [DataMember(Order = 904, EmitDefaultValue = true)]
        public string LastMachineLine { get; set; }

        [DataMember(Order = 906, EmitDefaultValue = true)]
        public DateTime LastMachineDate { get; set; }

        [DataMember(Order = 908, EmitDefaultValue = true)]
        public string LastStationName { get; set; }

        [DataMember(Order = 910, EmitDefaultValue = true)]
        public string LastStationLine { get; set; }

        [DataMember(Order = 912, EmitDefaultValue = true)]
        public DateTime LastStationDate { get; set; }

        [DataMember(Order = 914, EmitDefaultValue = true)]
        public string LastStationDesc { get; set; }

        // Flex Descriptions
        [DataMember(Order = 950, EmitDefaultValue = true)]
        public decimal ConfigId { get; set; }

        [DataMember(Order = 952, EmitDefaultValue = true)]
        public string DimFlex { get; set; }

        [DataMember(Order = 954, EmitDefaultValue = true)]
        public string UnitFlex { get; set; }

        [DataMember(Order = 956, EmitDefaultValue = true)]
        public string AirFlex { get; set; }

        [DataMember(Order = 958, EmitDefaultValue = true)]
        public string PreserveFlex { get; set; }

        [DataMember(Order = 960, EmitDefaultValue = true)]
        public string BarPlaceFlex { get; set; }

        [DataMember(Order = 962, EmitDefaultValue = true)]
        public string ProcessingFlex { get; set; }

        [DataMember(Order = 964, EmitDefaultValue = true)]
        public string VGrooveFlex { get; set; }

        [DataMember(Order = 966, EmitDefaultValue = true)]
        public string RoutingFlex { get; set; }

        // Metrics
        [DataMember(Order = 1000, EmitDefaultValue = true)]
        public int Units { get; set; }

        [DataMember(Order = 1002, EmitDefaultValue = true)]
        public int OnHand { get; set; }

        [DataMember(Order = 1004, EmitDefaultValue = true)]
        public int Tubes { get; set; }

        [DataMember(Order = 1006, EmitDefaultValue = true)]
        public int Shapes { get; set; }

        [DataMember(Order = 1008, EmitDefaultValue = true)]
        public int Preserve { get; set; }

        [DataMember(Order = 1010, EmitDefaultValue = true)]
        public int Scoring { get; set; }

        [DataMember(Order = 1012, EmitDefaultValue = true)]
        public int Bars { get; set; }

        [DataMember(Order = 1014, EmitDefaultValue = true)]
        public int Picks { get; set; }

        [DataMember(Order = 1016, EmitDefaultValue = true)]
        public int Purchases { get; set; }

        [DataMember(Order = 1200, EmitDefaultValue = true)]
        public int UnrackedUnits { get; set; }

        [DataMember(Order = 1202, EmitDefaultValue = true)]
        public int UncompletedUnits { get; set; }

        [DataMember(Order = 1204, EmitDefaultValue = true)]
        public int CompletedUnits { get; set; }

        [DataMember(Order = 1206, EmitDefaultValue = true)]
        public int ShippedUnits { get; set; }


        // Attributes
        public ItemAttributes ItemAttributes { get; set; }

    }
}
