using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class OffalLites : List<OffalLite>
    {
    }

    [DataContract(Namespace = "")]
    public class OffalLite
    {
        [DataMember(Order = 10)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public DateTime ReportDate { get; set; }

        [DataMember(Order = 20)]
        public string ProductionLineCode { get; set; }

        [DataMember(Order = 20)]
        public string GroupDescription { get; set; }

        [DataMember(Order = 21)]
        public string WorkCenterId { get; set; }

        [DataMember(Order = 22)]
        public int Shift { get; set; }

        [DataMember(Order = 24)]
        public string Crew { get; set; }

        [DataMember(Order = 26)]
        public int Lites { get; set; }

        [DataMember(Order = 40)]
        public decimal ActualSqft { get; set; }

        [DataMember(Order = 42)]
        public decimal Tons { get; set; }
    }
}
