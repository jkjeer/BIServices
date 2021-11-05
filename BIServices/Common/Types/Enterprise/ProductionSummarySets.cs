using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{

    [CollectionDataContract(Namespace = "")]
    public class RequestedDateFacts : List<RequestedDateFact>
    {
    }

    [DataContract(Namespace = "")]
    public class RequestedDateFact
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 12)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 14)]
        public DateTime InitialRequestedDate { get; set; }

        [DataMember(Order = 20)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 22)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 24)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 26)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 40)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 42)]
        public string StockType { get; set; }

        [DataMember(Order = 44)]
        public string UnitType { get; set; }

        [DataMember(Order = 46)]
        public string LamiProdType { get; set; }

        [DataMember(Order = 50)]
        public string Description1 { get; set; }

        [DataMember(Order = 52)]
        public string Description2 { get; set; }

        [DataMember(Order = 54)]
        public string NextStatus { get; set; }

        [DataMember(Order = 100)]
        public decimal RequestedUnits { get; set; }

        [DataMember(Order = 102)]
        public decimal RequestedActualSqft { get; set; }

        [DataMember(Order = 104)]
        public decimal RequestedBillableSqft { get; set; }

        [DataMember(Order = 110)]
        public decimal NotScheduledUnits { get; set; }

        [DataMember(Order = 112)]
        public decimal NotScheduledActualSqft { get; set; }

        [DataMember(Order = 120)]
        public decimal ScheduledUnits { get; set; }

        [DataMember(Order = 122)]
        public decimal ScheduledActualSqft { get; set; }

        [DataMember(Order = 126)]
        public decimal UnboxedUnits { get; set; }

        [DataMember(Order = 128)]
        public decimal UnboxedActualSqft { get; set; }

        [DataMember(Order = 130)]
        public decimal VoidUnits { get; set; }

        [DataMember(Order = 132)]
        public decimal VoidActualSqft { get; set; }

        [DataMember(Order = 140)]
        public decimal OpenUnits { get; set; }

        [DataMember(Order = 142)]
        public decimal OpenActualSqft { get; set; }

        [DataMember(Order = 150)]
        public decimal CompletedUnits { get; set; }

        [DataMember(Order = 152)]
        public decimal CompletedActualSqft { get; set; }

        [DataMember(Order = 160)]
        public decimal ClosedUnits { get; set; }

        [DataMember(Order = 162)]
        public decimal ClosedActualSqft { get; set; }

        [DataMember(Order = 163)]
        public decimal LockedUnits { get; set; }

        [DataMember(Order = 164)]
        public decimal LockedActualSqft { get; set; }

        [DataMember(Order = 170)]
        public decimal ShippedUnits { get; set; }

        [DataMember(Order = 172)]
        public decimal ShippedActualSqft { get; set; }

        [DataMember(Order = 180)]
        public decimal ProducedUnits { get; set; }

        [DataMember(Order = 182)]
        public decimal ProducedActualSqft { get; set; }


    }

    [CollectionDataContract(Namespace = "")]
    public class ScheduledDateFacts : List<ScheduledDateFact>
    {
    }

    [DataContract(Namespace = "")]
    public class ScheduledDateFact
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 12)]
        public DateTime ScheduledDate { get; set; }

        [DataMember(Order = 20)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 22)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 24)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 26)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 40)]
        public string PriorityCode { get; set; }

        [DataMember(Order = 42)]
        public string StockType { get; set; }

        [DataMember(Order = 44)]
        public string UnitType { get; set; }

        [DataMember(Order = 46)]
        public string ProductionLine { get; set; }

        [DataMember(Order = 100)]
        public decimal RequestedUnits { get; set; }

        [DataMember(Order = 102)]
        public decimal RequestedActualSqft { get; set; }

        [DataMember(Order = 104)]
        public decimal RequestedBillableSqft { get; set; }

        [DataMember(Order = 110)]
        public decimal ProducedUnits { get; set; }

        [DataMember(Order = 112)]
        public decimal ProducedActualSqft { get; set; }

        [DataMember(Order = 120)]
        public decimal OpenUnits { get; set; }

        [DataMember(Order = 122)]
        public decimal OpenActualSqft { get; set; }
    }
    
}
