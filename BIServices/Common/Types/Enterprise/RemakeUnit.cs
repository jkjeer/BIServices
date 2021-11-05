using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class RemakeUnits : List<RemakeUnit>
    {
    }

    [DataContract(Namespace = "")]
    public class RemakeUnit
    {
        // REMAKE UNIT PROPERTIES

        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 11)]
        public string PurchaserPlantNum { get; set; }

        [DataMember(Order = 12)]
        public decimal UnitId { get; set; }

        [DataMember(Order = 14)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 16)]
        public string SalesOrder { get; set; }

        [DataMember(Order = 16)]
        public decimal SalesOrderNum { get; set; }

        [DataMember(Order = 18)]
        public string SalesOrderType { get; set; }

        [DataMember(Order = 20)]
        public decimal SalesOrderLineNum { get; set; }

        [DataMember(Order = 22)]
        public decimal RemakeSchedule { get; set; }

        [DataMember(Order = 24)]
        public decimal OriginalSchedule { get; set; }

        [DataMember(Order = 26)]
        public int Quantity { get; set; }

        [DataMember(Order = 28)]
        public DateTime RemakeDate { get; set; }

        [DataMember(Order = 30)]
        public int Attempts { get; set; }

        // MAKEUP PROPERTIES

        [DataMember(Order = 40)]
        public string Construction { get; set; }

        [DataMember(Order = 42)]
        public string Dimensions { get; set; }

        [DataMember(Order = 44)]
        public decimal ShapeNum { get; set; }

        // CUSTOMER PROPERTIES

        [DataMember(Order = 50)]
        public string CustomerName { get; set; }

        [DataMember(Order = 52)]
        public DateTime RequestedDate { get; set; }

        // RELATED ORDER

        [DataMember(Order = 80)]
        public string RelatedOrder { get; set; }

        [DataMember(Order = 81)]
        public decimal RelatedOrderNum { get; set; }

        [DataMember(Order = 82)]
        public string RelatedOrderType { get; set; }

        [DataMember(Order = 84)]
        public decimal RelatedOrderLineNum { get; set; }

        [DataMember(Order = 86)]
        public int LiteNum { get; set; }

        // SUPPLIER PROPERTIES

        [DataMember(Order = 100)]
        public string SupplierPlant { get; set; }

        [DataMember(Order = 102)]
        public decimal RackId { get; set; }

        [DataMember(Order = 102)]
        public string SerialNum { get; set; }

        [DataMember(Order = 104)]
        public string RackType { get; set; }

        [DataMember(Order = 106)]
        public string RackItemStatus { get; set; }

        [DataMember(Order = 108)]
        public DateTime? RackTransactionDate { get; set; }

        [DataMember(Order = 120)]
        public string SupplierOrder { get; set; }

        [DataMember(Order = 121)]
        public decimal SupplierOrderNum { get; set; }

        [DataMember(Order = 122)]
        public string SupplierOrderType { get; set; }

        [DataMember(Order = 124)]
        public decimal SupplierOrderLineNum { get; set; }

        [DataMember(Order = 150)]
        public decimal SupplierScheduleNum { get; set; }

        [DataMember(Order = 152)]
        public DateTime SupplierScheduleDate { get; set; }

        [DataMember(Order = 154)]
        public int SupplierSchedules { get; set; }

    }
}
