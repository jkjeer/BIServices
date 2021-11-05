using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class TemperAggregates : List<TemperAggregate>
    {
    }

    [DataContract(Namespace = "")]
    public class TemperAggregate
    {
        [DataMember(Order = 10)]
        public string PlantCode { get; set; }

        [DataMember(Order = 11)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public DateTime StartDate { get; set; }

        [DataMember(Order = 13)]
        public DateTime EndDate { get; set; }

        [DataMember(Order = 15)]
        public string ProductionLineCode { get; set; }

        [DataMember(Order = 16)]
        public string WorkCenterId { get; set; }

        [DataMember(Order = 17)]
        public string ProductionLineNum { get; set; }

        [DataMember(Order = 18)]
        public string ProductionLineGroupDesc { get; set; }

        [DataMember(Order = 19)]
        public string ProductionLineDesc { get; set; }

        // TEMPERING

        [DataMember(Order = 130)]
        public int GrossTempProducedLites { get; set; }

        [DataMember(Order = 132)]
        public decimal GrossTempProducedSqft { get; set; }

        [DataMember(Order = 132)]
        public decimal GrossTempProducedActualSqft { get; set; }

        [DataMember(Order = 132)]
        public decimal GrossTempProducedBillableSqft { get; set; }

        [DataMember(Order = 134)]
        public decimal GrossTempProducedTons { get; set; }

        [DataMember(Order = 140)]
        public int LoProTempLites { get; set; }

        [DataMember(Order = 142)]
        public decimal LoProTempSqft { get; set; }

        [DataMember(Order = 142)]
        public decimal LoProTempActualSqft { get; set; }

        [DataMember(Order = 142)]
        public decimal LoProTempBillableSqft { get; set; }

        [DataMember(Order = 144)]
        public decimal LoProTempTons { get; set; }

        [DataMember(Order = 150)]
        public int CustomTempLites { get; set; }

        [DataMember(Order = 152)]
        public decimal CustomTempSqft { get; set; }

        [DataMember(Order = 152)]
        public decimal CustomTempActualSqft { get; set; }

        [DataMember(Order = 152)]
        public decimal CustomTempBillableSqft { get; set; }

        [DataMember(Order = 154)]
        public decimal CustomTempTons { get; set; }

        [DataMember(Order = 160)]
        public int GrossTempRejectedLites { get; set; }

        [DataMember(Order = 162)]
        public decimal GrossTempRejectedSqft { get; set; }

        [DataMember(Order = 162)]
        public decimal GrossTempRejectedActualSqft { get; set; }

        [DataMember(Order = 162)]
        public decimal GrossTempRejectedBillableSqft { get; set; }

        [DataMember(Order = 164)]
        public decimal GrossTempRejectedTons { get; set; }

        [DataMember(Order = 170)]
        public int VendorTempRejectedLites { get; set; }

        [DataMember(Order = 172)]
        public decimal VendorTempRejectedSqft { get; set; }

        [DataMember(Order = 172)]
        public decimal VendorTempRejectedActualSqft { get; set; }

        [DataMember(Order = 172)]
        public decimal VendorTempRejectedBillableSqft { get; set; }

        [DataMember(Order = 174)]
        public decimal VendorTempRejectedTons { get; set; }

        [DataMember(Order = 180)]
        public int InternalTempRejectedLites { get; set; }

        [DataMember(Order = 182)]
        public decimal InternalTempRejectedSqft { get; set; }

        [DataMember(Order = 182)]
        public decimal InternalTempRejectedActualSqft { get; set; }

        [DataMember(Order = 182)]
        public decimal InternalTempRejectedBillableSqft { get; set; }

        [DataMember(Order = 184)]
        public decimal InternalTempRejectedTons { get; set; }

        [DataMember(Order = 190)]
        public int NetTempProducedLites { get; set; }

        [DataMember(Order = 192)]
        public decimal NetTempProducedSqft { get; set; }

        [DataMember(Order = 192)]
        public decimal NetTempProducedActualSqft { get; set; }

        [DataMember(Order = 192)]
        public decimal NetTempProducedBillableSqft { get; set; }

        [DataMember(Order = 194)]
        public decimal NetTempProducedTons { get; set; }

        [DataMember(Order = 200)]
        public decimal TempYield { get; set; }

        [DataMember(Order = 200)]
        public decimal TempActualYield { get; set; }

        [DataMember(Order = 200)]
        public decimal TempBillableYield { get; set; }

        [DataMember(Order = 300)]
        public decimal TemperingHours { get; set; }


    }
}
