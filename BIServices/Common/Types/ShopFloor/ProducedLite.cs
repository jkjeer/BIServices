using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class ProducedLites : List<ProducedLite>
    {
    }

    [DataContract(Namespace = "")]
    public class ProducedLite
    {
        [DataMember(Order = 10)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public DateTime ReportDate { get; set; }

        [DataMember(Order = 13)]
        public DateTime MinWrittenDate { get; set; }

        [DataMember(Order = 14)]
        public DateTime MaxWrittenDate { get; set; }

        [DataMember(Order = 20)]
        public string ProductionLineCode { get; set; }

        [DataMember(Order = 21)]
        public string GroupDescription { get; set; }

        [DataMember(Order = 21)]
        public string WorkCenterId { get; set; }

        [DataMember(Order = 22)]
        public int Shift { get; set; }

        [DataMember(Order = 24)]
        public string Crew { get; set; }

        [DataMember(Order = 25)]
        public string OriginTypeCode { get; set; }

        [DataMember(Order = 30)]
        public int Lites { get; set; }

        [DataMember(Order = 32)]
        public decimal Sqft { get; set; }

        [DataMember(Order = 32)]
        public decimal ActualSqft { get; set; }

        [DataMember(Order = 33)]
        public decimal BillableSqft { get; set; }

        [DataMember(Order = 34)]
        public decimal Tons { get; set; }

        [DataMember(Order = 40)]
        public int RareLites { get; set; }

        [DataMember(Order = 42)]
        public decimal RareSqft { get; set; }

        [DataMember(Order = 42)]
        public decimal RareActualSqft { get; set; }

        [DataMember(Order = 42)]
        public decimal RareBillableSqft { get; set; }

        [DataMember(Order = 44)]
        public decimal RareTons { get; set; }
    }
}
