using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class WorkOrders : List<WorkOrder>
    {
        public WorkOrders(Int32 size)
        {
            this.Capacity = size;
        }

        public WorkOrders()
        {
        }
    }

    [DataContract(Namespace = "")]
    public class WorkOrder
    {
        public WorkOrder()
        {
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
        public decimal RelatedOrderNum { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = false)]
        public string RelatedOrderType { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = false)]
        public decimal RelatedOrderLineNum { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string StatusCode { get; set; }

        [DataMember(Order = 22, EmitDefaultValue = false)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 24, EmitDefaultValue = false)]
        public string LotNum { get; set; }

        [DataMember(Order = 26, EmitDefaultValue = false)]
        public string MinUnitId { get; set; }

        [DataMember(Order = 28, EmitDefaultValue = false)]
        public string MaxUnitId { get; set; }


        [DataMember(Order = 40, EmitDefaultValue = false)]
        public DateTime CreatedDate { get; set; }

        [DataMember(Order = 41, EmitDefaultValue = false)]
        public DateTime OrderDate { get; set; }

        [DataMember(Order = 42, EmitDefaultValue = false)]
        public DateTime InitialDueDate { get; set; }

        [DataMember(Order = 43, EmitDefaultValue = false)]
        public DateTime DueDate { get; set; }


        [DataMember(Order = 50, EmitDefaultValue = false)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 52, EmitDefaultValue = false)]
        public string StockType { get; set; }

        [DataMember(Order = 54, EmitDefaultValue = false)]
        public string GlCategory { get; set; }

        [DataMember(Order = 56, EmitDefaultValue = false)]
        public string ItemDesc1 { get; set; }

        [DataMember(Order = 58, EmitDefaultValue = false)]
        public string ItemDesc2 { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public string UnitType { get; set; }

        [DataMember(Order = 62, EmitDefaultValue = false)]
        public string UnitTypeDesc { get; set; }

        [DataMember(Order = 64, EmitDefaultValue = false)]
        public string Description1 { get; set; }

        [DataMember(Order = 66, EmitDefaultValue = false)]
        public string Description2 { get; set; }

        [DataMember(Order = 68, EmitDefaultValue = false)]
        public string VolumeIndicator { get; set; }

        [DataMember(Order = 70, EmitDefaultValue = false)]
        public decimal LitesPerRack { get; set; }



        [DataMember(Order = 80, EmitDefaultValue = false)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 82, EmitDefaultValue = false)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 84, EmitDefaultValue = false)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 86, EmitDefaultValue = false)]
        public string CustomerShipName { get; set; }




        // Attributes

        [DataMember(Order = 200, EmitDefaultValue = false)]
        public decimal ConfiguratorId { get; set; }

        [DataMember(Order = 202, EmitDefaultValue = false)]
        public decimal Lite1Thickness { get; set; }

        [DataMember(Order = 204, EmitDefaultValue = false)]
        public string Lite1HeatTreatment { get; set; }

        [DataMember(Order = 206, EmitDefaultValue = false)]
        public decimal MCode { get; set; }

        [DataMember(Order = 208, EmitDefaultValue = false)]
        public string MCodeDesc { get; set; }

        [DataMember(Order = 210, EmitDefaultValue = false)]
        public string GridStyle { get; set; }

        [DataMember(Order = 214, EmitDefaultValue = false)]
        public string LamiType { get; set; }

        [DataMember(Order = 216, EmitDefaultValue = false)]
        public string Construction { get; set; }

        [DataMember(Order = 218, EmitDefaultValue = false)]
        public string Dimension { get; set; }


        [DataMember(Order = 250, EmitDefaultValue = false)]
        public decimal ShapeNum { get; set; }

        [DataMember(Order = 252, EmitDefaultValue = false)]
        public decimal UnitActualSqft { get; set; }

        [DataMember(Order = 225401, EmitDefaultValue = false)]
        public decimal TotalGlassPounds { get; set; }


        [DataMember(Order = 300, EmitDefaultValue = false)]
        public string Lite1Routing { get; set; }

        [DataMember(Order = 302, EmitDefaultValue = false)]
        public string Lite1Supplier { get; set; }


        [DataMember(Order = 400, EmitDefaultValue = false)]
        public decimal UnitCost { get; set; }

        // Pricing Attributes
        [DataMember(Order = 402, EmitDefaultValue = true)]
        public decimal UnitPrice { get; set; }


        // Metrics
        [DataMember(Order = 500, EmitDefaultValue = false)]
        public decimal OrderedQty { get; set; }

        [DataMember(Order = 502, EmitDefaultValue = false)]
        public decimal ShippedQty { get; set; }

        [DataMember(Order = 504, EmitDefaultValue = false)]
        public decimal CancelledQty { get; set; }

        [DataMember(Order = 506, EmitDefaultValue = false)]
        public decimal CompletedQty { get; set; }

        [DataMember(Order = 506, EmitDefaultValue = false)]
        public decimal ScheduledQty { get; set; }

        [DataMember(Order = 520, EmitDefaultValue = false)]
        public decimal TotalActualSqft { get; set; }

        [DataMember(Order = 522, EmitDefaultValue = false)]
        public decimal TotalBillableSqft { get; set; }

        [DataMember(Order = 550, EmitDefaultValue = false)]
        public decimal TotalCost { get; set; }

        [DataMember(Order = 552, EmitDefaultValue = true)]
        public decimal TotalPrice { get; set; }


        // Item Attributes
        public Dictionary<string, string> AttributesDict { get; set; }
        public ItemAttributes ItemAttributes { get; set; }


    }


}
