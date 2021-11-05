using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{

    [CollectionDataContract(Namespace = "")]
    public class HvMtoOrders : List<HvMtoOrder>
    {
    }

    [DataContract(Namespace = "")]
    public class HvMtoOrder
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 20)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 22)]
        public string WorkOrderStatus { get; set; }

        [DataMember(Order = 30)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 32)]
        public DateTime DueDate { get; set; }

        [DataMember(Order = 40)]
        public decimal SalesOrderNum { get; set; }

        [DataMember(Order = 42)]
        public string SalesOrderType { get; set; }

        [DataMember(Order = 44)]
        public decimal SalesOrderLineNum { get; set; }

        [DataMember(Order = 46)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 47)]
        public string StockType { get; set; }

        [DataMember(Order = 48)]
        public string LotNum { get; set; }

        [DataMember(Order = 50)]
        public decimal ConfigId { get; set; }

        [DataMember(Order = 52)]
        public string UnitType { get; set; }

        [DataMember(Order = 54)]
        public string Construction { get; set; }

        [DataMember(Order = 56)]
        public string Dimension { get; set; }

        [DataMember(Order = 60)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 62)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 70)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 62)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 100)]
        public decimal OrderedUnitsQty { get; set; }

        [DataMember(Order = 102)]
        public decimal ProducedUnitsQty { get; set; }

        [DataMember(Order = 104)]
        public decimal ShippedUnitsQty { get; set; }

        [DataMember(Order = 106)]
        public decimal UnshippedUnitsQty { get; set; }

        [DataMember(Order = 120)]
        public DateTime? LastShippedDate { get; set; }

        [DataMember(Order = 200)]
        public ShippedLines ShippedLines { get; set; }

        [DataMember(Order = 210)]
        public PurchasedLites PurchasedLites { get; set; }
    }


    [CollectionDataContract(Namespace = "")]
    public class PurchasedLites : List<PurchasedLite>
    {
    }

    [DataContract(Namespace = "")]
    public class PurchasedLite
    {

        [DataMember(Order = 10)]
        public decimal LiteNum { get; set; }

        [DataMember(Order = 12)]
        public string LiteName { get; set; }

        [DataMember(Order = 14)]
        public string TransactionType { get; set; }

        [DataMember(Order = 16)]
        public string Plant { get; set; }

        [DataMember(Order = 22)]
        public string PurchaseOrderStatus { get; set; }

        [DataMember(Order = 30)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 40)]
        public decimal PurchaseOrderNum { get; set; }

        [DataMember(Order = 42)]
        public string PurchaseOrderType { get; set; }

        [DataMember(Order = 44)]
        public decimal PurchaseOrderLineNum { get; set; }

        [DataMember(Order = 50)]
        public decimal SupplierNum { get; set; }

        [DataMember(Order = 52)]
        public string SupplierName { get; set; }

        [DataMember(Order = 54)]
        public string SupplierPlant { get; set; }

        [DataMember(Order = 60)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 61)]
        public decimal ConfigId { get; set; }

        [DataMember(Order = 62)]
        public string LotNum { get; set; }

        [DataMember(Order = 70)]
        public string StockType { get; set; }

        [DataMember(Order = 72)]
        public string Construction { get; set; }

        [DataMember(Order = 100)]
        public decimal PurchasedQty { get; set; }

        [DataMember(Order = 102)]
        public decimal ReceivedQty { get; set; }

        [DataMember(Order = 104)]
        public DateTime ReceivedDate { get; set; }

        [DataMember(Order = 200)]
        public ReceivedRacks Racks { get; set; }
    }


    [CollectionDataContract(Namespace = "")]
    public class ReceivedRacks : List<ReceivedRack>
    {
    }

    [DataContract(Namespace = "")]
    public class ReceivedRack
    {

        [DataMember(Order = 10)]
        public decimal RackId { get; set; }

        [DataMember(Order = 12)]
        public string SerialNumMin { get; set; }

        [DataMember(Order = 14)]
        public string SerialNumMax { get; set; }

        [DataMember(Order = 16)]
        public string Plant { get; set; }

        [DataMember(Order = 20)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 22)]
        public string LotNum { get; set; }

        [DataMember(Order = 30)]
        public string Location { get; set; }

        [DataMember(Order = 100)]
        public decimal OnHandQty { get; set; }
    }

    [CollectionDataContract(Namespace = "")]
    public class ShippedLines : List<ShippedLine>
    {
    }

    [DataContract(Namespace = "")]
    public class ShippedLine
    {

        [DataMember(Order = 10)]
        public decimal LineNum { get; set; }

        [DataMember(Order = 12)]
        public DateTime ShippedDate { get; set; }

        [DataMember(Order = 14)]
        public decimal ShippedQty { get; set; }

        [DataMember(Order = 16)]
        public decimal LoadNum { get; set; }


    }
}
