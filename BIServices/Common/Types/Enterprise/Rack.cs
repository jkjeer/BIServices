using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class Racks : List<Rack>
    {
    }

    [DataContract(Namespace = "")]
    public class Rack
    {
        [DataMember(Order = 2)]
        public decimal RackId { get; set; }

        [DataMember(Order = 4)]
        public string AssetId { get; set; }

        [DataMember(Order = 11)]
        public string StockSerialNum { get; set; }

        [DataMember(Order = 12)]
        public string CostCenter { get; set; }

        [DataMember(Order = 12)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public string PlantCode { get; set; }

        [DataMember(Order = 13)]
        public string RackType { get; set; }

        [DataMember(Order = 14)]
        public string Location { get; set; }

        [DataMember(Order = 16)]
        public string HeaderStatus { get; set; }

        [DataMember(Order = 18)]
        public DateTime HeaderStatusDate { get; set; }

        [DataMember(Order = 20)]
        public decimal GlassWeight { get; set; }

        [DataMember(Order = 21)]
        public decimal GlassWeightTons { get; set; }

        [DataMember(Order = 22)]
        public decimal TareWeight { get; set; }

        [DataMember(Order = 22)]
        public decimal GrossWeight { get; set; }

        [DataMember(Order = 23)]
        public decimal GrossWeightTons { get; set; }

        [DataMember(Order = 24)]
        public string ReasonCode { get; set; }

        [DataMember(Order = 40)]
        public decimal CustomerNum { get; set; }

        [DataMember(Order = 41)]
        public decimal ShipToCustomerNum { get; set; }

        [DataMember(Order = 42)]
        public string CustomerPO { get; set; }

        [DataMember(Order = 44)]
        public decimal SupplierNum { get; set; }

        [DataMember(Order = 50)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 51)]
        public decimal BoxNum { get; set; }

        [DataMember(Order = 52)]
        public decimal SuggestedLoadNum { get; set; }

        [DataMember(Order = 52)]
        public decimal LoadNum { get; set; }

        [DataMember(Order = 53)]
        public DateTime LoadConfirmDateTime { get; set; }

        [DataMember(Order = 54)]
        public string TrailerNum { get; set; }

        [DataMember(Order = 55)]
        public decimal BillOfLadingNum { get; set; }

        [DataMember(Order = 60)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 60)]
        public decimal ItemConsumptionAmt { get; set; }

        [DataMember(Order = 60)]
        public int ItemConsumptionDays { get; set; }

        [DataMember(Order = 61)]
        public string CustomerPartNum { get; set; }

        [DataMember(Order = 61)]
        public string CustomerPartNum1 { get; set; }

        [DataMember(Order = 61)]
        public string CustomerPartNum2 { get; set; }

        [DataMember(Order = 61)]
        public string CustomerPartNum3 { get; set; }

        [DataMember(Order = 61)]
        public string ProductType { get; set; }

        [DataMember(Order = 62)]
        public string ProcessingPriority { get; set; }

        [DataMember(Order = 62)]
        public string ItemDesc { get; set; }

        [DataMember(Order = 63)]
        public string LotNum { get; set; }

        [DataMember(Order = 64)]
        public string StockType { get; set; }

        [DataMember(Order = 66)]
        public string GlCategory { get; set; }

        [DataMember(Order = 68)]
        public decimal LitesPerRack { get; set; }

        [DataMember(Order = 70)]
        public decimal WOrderNum { get; set; }

        [DataMember(Order = 72)]
        public decimal POrderNum { get; set; }

        [DataMember(Order = 73)]
        public string POrderType { get; set; }

        [DataMember(Order = 74)]
        public decimal POrderLineNum { get; set; }

        [DataMember(Order = 75)]
        public string POrderCompany { get; set; }

        [DataMember(Order = 80)]
        public DateTime? ManufacturedDate { get; set; }

        [DataMember(Order = 81)]
        public DateTime? ProductionDate { get; set; }

        [DataMember(Order = 82)]
        public int ManufacturedAgingDays { get; set; }

        [DataMember(Order = 82)]
        public int ReceivedAgingDays { get; set; }

        [DataMember(Order = 86)]
        public decimal Quantity { get; set; }

        [DataMember(Order = 87)]
        public decimal OriginalQuantity { get; set; }

        [DataMember(Order = 90)]
        public DateTime? ReceivedDate { get; set; }

        [DataMember(Order = 92)]
        public decimal ReceivedQty { get; set; }

        [DataMember(Order = 100)]
        public DateTime UpdateDateTime { get; set; }

        [DataMember(Order = 102)]
        public string ShipRoute { get; set; }

        [DataMember(Order = 104)]
        public DateTime DeliveryDateTime { get; set; }

        [DataMember(Order = 150)]
        public decimal ConfigId { get; set; }
        public ItemAttributes ItemAttributes { get; set; }

        [DataMember(Order = 202)]
        public string Construction { get; set; }

        [DataMember(Order = 203)]
        public decimal Lite1Thickness { get; set; }

        [DataMember(Order = 204)]
        public decimal OverallThickness { get; set; }

        [DataMember(Order = 205)]
        public string Dimension { get; set; }

        [DataMember(Order = 206)]
        public decimal UnitActualSqft { get; set; }

        [DataMember(Order = 208)]
        public decimal TotalActualSqft { get; set; }

        [DataMember(Order = 220)]
        public decimal UnitCost { get; set; }

        [DataMember(Order = 222)]
        public decimal TotalCost { get; set; }

        [DataMember(Order = 224, EmitDefaultValue = false)]
        public decimal PlannerNum { get; set; }

        [DataMember(Order = 225, EmitDefaultValue = false)]
        public string PlannerName { get; set; }

        [DataMember(Order = 226)]
        public string PlanningFamily { get; set; }

        [DataMember(Order = 230)]
        public string Description1 { get; set; }

        [DataMember(Order = 232)]
        public string Description2 { get; set; }

        [DataMember(Order = 300)]
        public decimal RackSlot { get; set; }

        [DataMember(Order = 302)]
        public RackItems RackItems { get; set; }
    }

    [CollectionDataContract(Namespace = "")]
    public class RackItems : List<RackItem>
    {
    }

    [DataContract(Namespace = "")]
    public class RackItem
    {
        [DataMember(Order = 1)]
        public decimal RackId { get; set; }

        [DataMember(Order = 2)]
        public string SerialNum { get; set; }

        [DataMember(Order = 4)]
        public string GLCategory { get; set; }

        [DataMember(Order = 5)]
        public string PlantNum { get; set; }

        [DataMember(Order = 10)]
        public string Side { get; set; }

        [DataMember(Order = 12)]
        public decimal Position { get; set; }

        [DataMember(Order = 14)]
        public decimal StackTier { get; set; }

        [DataMember(Order = 16)]
        public decimal Row { get; set; }

        [DataMember(Order = 18)]
        public decimal Slot { get; set; }

        [DataMember(Order = 20)]
        public string BoxRotated { get; set; }

        [DataMember(Order = 30)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 31)]
        public string CustomerPartNum { get; set; }

        [DataMember(Order = 32)]
        public string LotNum { get; set; }

        [DataMember(Order = 40)]
        public string DetailStatus { get; set; }

        [DataMember(Order = 42)]
        public decimal Quantity { get; set; }

        [DataMember(Order = 44)]
        public string QuantityUom { get; set; }

        [DataMember(Order = 46)]
        public decimal GlassWeight { get; set; }

        [DataMember(Order = 48)]
        public string GlassWeightUom { get; set; }

        [DataMember(Order = 50)]
        public DateTime TransactionDate { get; set; }

        [DataMember(Order = 52)]
        public DateTime ProductionDate { get; set; }

        [DataMember(Order = 60)]
        public DateTime ManufacturedDate { get; set; }

        [DataMember(Order = 62)]
        public String ManufacturedPlant { get; set; }

        [DataMember(Order = 70)]
        public DateTime? ReceivedDate { get; set; }

        [DataMember(Order = 74)]
        public decimal ReceivedQty { get; set; }

        [DataMember(Order = 80)]
        public decimal SalesOrderNum { get; set; }

        [DataMember(Order = 82)]
        public String SalesOrderType { get; set; }

        [DataMember(Order = 84)]
        public decimal SalesOrderLineNum { get; set; }

        [DataMember(Order = 85)]
        public string SalesOrderCompany { get; set; }

        [DataMember(Order = 86)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 88)]
        public string WorkOrderPlantNum { get; set; }

        [DataMember(Order = 90)]
        public decimal PurchaseOrderNum { get; set; }

        [DataMember(Order = 92)]
        public String PurchaseOrderType { get; set; }

        [DataMember(Order = 94)]
        public decimal PurchaseOrderLineNum { get; set; }

        [DataMember(Order = 95)]
        public string PurchaseOrderCompany { get; set; }

        [DataMember(Order = 96)]
        public decimal SupplierNum { get; set; }

        [DataMember(Order = 100)]
        public DateTime UpdateDateTime { get; set; }

        [DataMember(Order = 110)]
        public String ProcessId { get; set; }

        [DataMember(Order = 120, EmitDefaultValue = false)]
        public decimal PlannerNum { get; set; }

        [DataMember(Order = 122)]
        public string PlanningFamily { get; set; }

        [DataMember(Order = 124)]
        public string Description1 { get; set; }

        [DataMember(Order = 126)]
        public string Description2 { get; set; }


        [DataMember(Order = 200)]
        public bool HasGlassAttributes { get; set; }

        [DataMember(Order = 210)]
        public ItemAttributes ItemAttributes { get; set; }
    }
}