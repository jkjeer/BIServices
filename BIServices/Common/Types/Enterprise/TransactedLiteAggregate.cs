using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class TransactedLiteAggregates : List<TransactedLiteAggregate>
    {
    }

    [DataContract(Namespace = "")]
    public class TransactedLiteAggregate
    {
        [DataMember(Order = 10)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public DateTime ReportDate { get; set; }

        [DataMember(Order = 13)]
        public DateTime MinTransactionDate { get; set; }

        [DataMember(Order = 14)]
        public DateTime MinProductionDate { get; set; }

        [DataMember(Order = 15)]
        public DateTime MaxTransactionDate { get; set; }

        [DataMember(Order = 16)]
        public DateTime MaxProductionDate { get; set; }

        [DataMember(Order = 20)]
        public string ProductionLineCode { get; set; }

        [DataMember(Order = 21)]
        public string WorkCenterId { get; set; }

        [DataMember(Order = 22)]
        public int Shift { get; set; }

        [DataMember(Order = 24)]
        public string Crew { get; set; }

        [DataMember(Order = 25)]
        public string OriginTypeCode { get; set; }


        // ITEM ATTRIBUTES
        [DataMember(Order = 50)]
        public decimal Thickness { get; set; }

        [DataMember(Order = 52)]
        public string Color { get; set; }

        [DataMember(Order = 54)]
        public string HeatTreatment { get; set; }

        [DataMember(Order = 56)]
        public string FirstSrfCoat { get; set; }

        [DataMember(Order = 58)]
        public string SecondSrfCoat { get; set; }

        // REJECT PROPERTIES
        [DataMember(Order = 80)]
        public string TransRejectType { get; set; }

        [DataMember(Order = 82)]
        public string TransRejectCode { get; set; }

        [DataMember(Order = 83)]
        public string LineMapRejectType { get; set; }

        [DataMember(Order = 84)]
        public string LineMapRejectCode { get; set; }

        [DataMember(Order = 84)]
        public string LineMapRejectDesc { get; set; }

        [DataMember(Order = 86)]
        public string LineMapBenchmarkCode { get; set; }

        [DataMember(Order = 88)]
        public string LineMapBenchmarkDesc { get; set; }

        // METRICS
        [DataMember(Order = 100)]
        public int Lites { get; set; }

        [DataMember(Order = 110)]
        public decimal ActualSqft { get; set; }

        [DataMember(Order = 112)]
        public decimal BillableSqft { get; set; }

        [DataMember(Order = 120)]
        public decimal Tons { get; set; }
    }
}
