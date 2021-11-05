using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class ProducedUnits : List<ProducedUnit>
    {
    }

    [DataContract(Namespace = "")]
    public class ProducedUnit
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 15)]
        public decimal UnitId { get; set; }

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

        [DataMember(Order = 65)]
        public string CustomerOrderNum { get; set; }

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

        [DataMember(Order = 76)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 80)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 81)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 80)]
        public DateTime ScheduleDate { get; set; }

        [DataMember(Order = 82)]
        public string ProductType { get; set; }

        [DataMember(Order = 90)]
        public DateTime ShipDate { get; set; }

        // CONSTRUCTION PROPERTIES

        [DataMember(Order = 150)]
        public string Construction { get; set; }

        [DataMember(Order = 152)]
        public int ShapeNum { get; set; }

        [DataMember(Order = 153)]
        public string Dimension { get; set; }

        [DataMember(Order = 170)]
        public string Routing { get; set; }

        // PRODUCTION PROPERTIES

        [DataMember(Order = 180)]
        public DateTime MinProductionDate { get; set; }

        [DataMember(Order = 182)]
        public DateTime MaxProductionDate { get; set; }

        // METRICS

        [DataMember(Order = 200)]
        public int CutQty { get; set; }

        [DataMember(Order = 201)]
        public int TemperQty { get; set; }

        [DataMember(Order = 202)]
        public int RejectedQty { get; set; }

    }
}
