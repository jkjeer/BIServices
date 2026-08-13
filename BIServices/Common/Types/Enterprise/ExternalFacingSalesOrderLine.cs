using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class ExternalFacingSalesOrderLines : List<ExternalFacingSalesOrderLine>
    {
    }

    [DataContract(Namespace = "")]
    public class ExternalFacingSalesOrderLine
    {
        [DataMember(Order = 10, EmitDefaultValue = true)]
        public string PlantNumericCode { get; set; }
        [DataMember(Order = 12, EmitDefaultValue = true)]
        public string PlantAlphaCode { get; set; }
        [DataMember(Order = 20, EmitDefaultValue = true)]
        public decimal CustomerSoldNum { get; set; }
        [DataMember(Order = 22, EmitDefaultValue = false)]
        public string CustomerSoldName { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = true)]
        public decimal CustomerShipNum { get; set; }
        [DataMember(Order = 32, EmitDefaultValue = false)]
        public string CustomerShipName { get; set; }
        [DataMember(Order = 34, EmitDefaultValue = false)]
        public DateTime OrderDate { get; set; }
        [DataMember(Order = 34, EmitDefaultValue = false)]
        public string OrderDateString { get; set; }
        [DataMember(Order = 36, EmitDefaultValue = false)]
        public DateTime DueDate { get; set; }
        [DataMember(Order = 36, EmitDefaultValue = false)]
        public string DueDateString { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public string CustomerPart1 { get; set; }
        [DataMember(Order = 41, EmitDefaultValue = false)]
        public string CustomerPart2 { get; set; }
        [DataMember(Order = 42, EmitDefaultValue = false)]
        public string CustomerPart3 { get; set; }
        [DataMember(Order = 43, EmitDefaultValue = false)]
        public string ProductType { get; set; }
        [DataMember(Order = 44, EmitDefaultValue = false)]
        public string CardinalProductType { get; set; }
        [DataMember(Order = 45, EmitDefaultValue = false)]
        public string CustomerSeqNum { get; set; }
        [DataMember(Order = 46, EmitDefaultValue = false)]
        public string GenericSeqNum { get; set; }
        [DataMember(Order = 47, EmitDefaultValue = false)]
        public string CustomerPO { get; set; }
        [DataMember(Order = 48, EmitDefaultValue = false)]
        public string PriorityCode { get; set; }
        [DataMember(Order = 49, EmitDefaultValue = false)]
        public string UnitTypeCode { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = true)]
        public string SOrderLineType { get; set; }
        [DataMember(Order = 52, EmitDefaultValue = true)]
        public decimal SOrderNum { get; set; }
        [DataMember(Order = 60, EmitDefaultValue = true)]
        public string SOrderType { get; set; }
        [DataMember(Order = 70, EmitDefaultValue = true)]
        public decimal SOrderLineNum { get; set; }
        [DataMember(Order = 100, EmitDefaultValue = true)]
        public string SOrderLineLastStatus { get; set; }
        [DataMember(Order = 110, EmitDefaultValue = true)]
        public string SOrderLineNextStatus { get; set; }
        [DataMember(Order = 130, EmitDefaultValue = true)]
        public string Description1 { get; set; }
        [DataMember(Order = 140, EmitDefaultValue = true)]
        public string Description2 { get; set; }

        [DataMember(Order = 200, EmitDefaultValue = true)]
        public decimal Units { get; set; }
        [DataMember(Order = 210, EmitDefaultValue = true)]
        public decimal ActualSqft { get; set; }
        [DataMember(Order = 220, EmitDefaultValue = true)]
        public decimal BillableSqft { get; set; }
        [DataMember(Order = 230, EmitDefaultValue = true)]
        public decimal GlassWeight { get; set; }

        [DataMember(Order = 300, EmitDefaultValue = true)]
        public decimal WOrderNum { get; set; }
        [DataMember(Order = 302, EmitDefaultValue = true)]
        public decimal ItemNum { get; set; }
        [DataMember(Order = 304, EmitDefaultValue = true)]
        public string ItemCategory { get; set; }
        [DataMember(Order = 306, EmitDefaultValue = true)]
        public string ItemStockType { get; set; }

        [DataMember(Order = 400, EmitDefaultValue = true)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 500, EmitDefaultValue = true)]
        public decimal ConfigurationId { get; set; }

    }
}
