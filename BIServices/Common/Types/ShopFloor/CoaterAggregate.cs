using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class CoaterAggregates : List<CoaterAggregate>
    {
    }

    [DataContract(Namespace = "")]
    public class CoaterAggregate
    {
        [DataMember(Order = 10)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12)]
        public string PlantNum { get; set; }

        [DataMember(Order = 14)]
        public DateTime StartDate { get; set; }

        [DataMember(Order = 16)]
        public DateTime EndDate { get; set; }

        [DataMember(Order = 18)]
        public string ProductionLineCode { get; set; }

        [DataMember(Order = 19)]
        public string WorkCenterId { get; set; }

        // CONSUMED
        [DataMember(Order = 20)]
        public int ConsumedLites { get; set; }

        [DataMember(Order = 22)]
        public decimal ConsumedActualSqft { get; set; }

        [DataMember(Order = 22)]
        public decimal ConsumedBillableSqft { get; set; }

        [DataMember(Order = 24)]
        public decimal ConsumedTons { get; set; }

        // PRODUCED
        [DataMember(Order = 30)]
        public int ProducedLites { get; set; }

        [DataMember(Order = 32)]
        public decimal ProducedActualSqft { get; set; }

        [DataMember(Order = 34)]
        public decimal ProducedBillableSqft { get; set; }

        [DataMember(Order = 36)]
        public decimal ProducedTons { get; set; }

        // TOTAL REJECTS
        [DataMember(Order = 100)]
        public int TotalRejectedLites { get; set; }

        [DataMember(Order = 102)]
        public decimal TotalRejectedActualSqft { get; set; }

        [DataMember(Order = 104)]
        public decimal TotalRejectedBillableSqft { get; set; }

        [DataMember(Order = 106)]
        public decimal TotalRejectedTons { get; set; }

        // RAW REJECTS
        [DataMember(Order = 110)]
        public int RawRejectedLites { get; set; }

        [DataMember(Order = 112)]
        public decimal RawRejectedActualSqft { get; set; }

        [DataMember(Order = 114)]
        public decimal RawRejectedBillableSqft { get; set; }

        [DataMember(Order = 116)]
        public decimal RawRejectedTons { get; set; }

        // FINISHED REJECTS
        [DataMember(Order = 120)]
        public int FinishedRejectedLites { get; set; }

        [DataMember(Order = 122)]
        public decimal FinishedRejectedActualSqft { get; set; }

        [DataMember(Order = 124)]
        public decimal FinishedRejectedBillableSqft { get; set; }

        [DataMember(Order = 126)]
        public decimal FinishedRejectedTons { get; set; }

        // INTERNAL REJECTS
        [DataMember(Order = 130)]
        public int InternalRejectedLites { get; set; }

        [DataMember(Order = 132)]
        public decimal InternalRejectedActualSqft { get; set; }

        [DataMember(Order = 134)]
        public decimal InternalRejectedBillableSqft { get; set; }

        [DataMember(Order = 136)]
        public decimal InternalRejectedTons { get; set; }

        // VENDOR REJECTS
        [DataMember(Order = 140)]
        public int VendorRejectedLites { get; set; }

        [DataMember(Order = 142)]
        public decimal VendorRejectedActualSqft { get; set; }

        [DataMember(Order = 144)]
        public decimal VendorRejectedBillableSqft { get; set; }

        [DataMember(Order = 146)]
        public decimal VendorRejectedTons { get; set; }

        // CUSTOMER SUPPLIED
        [DataMember(Order = 150)]
        public int CustomerSuppliedLites { get; set; }

        [DataMember(Order = 152)]
        public decimal CustomerSuppliedActualSqft { get; set; }

        [DataMember(Order = 154)]
        public decimal CustomerSuppliedBillableSqft { get; set; }

        // QC HOLD
        [DataMember(Order = 160)]
        public int QcHoldLites { get; set; }

        [DataMember(Order = 162)]
        public decimal QcHoldActualSqft { get; set; }

        [DataMember(Order = 164)]
        public decimal QcHoldBillableSqft { get; set; }

        [DataMember(Order = 166)]
        public decimal QcHoldTons { get; set; }

        // EXTRA
        [DataMember(Order = 170)]
        public int ExtraLites { get; set; }

        [DataMember(Order = 172)]
        public decimal ExtraActualSqft { get; set; }

        [DataMember(Order = 174)]
        public decimal ExtraBillableSqft { get; set; }

        [DataMember(Order = 176)]
        public decimal ExtraTons { get; set; }

        // NET
        //[DataMember(Order = 200)]
        //public int NetProducedLites { get; set; }

        //[DataMember(Order = 202)]
        //public decimal NetProducedActualSqft { get; set; }

        //[DataMember(Order = 204)]
        //public decimal NetProducedBillableSqft { get; set; }

        //[DataMember(Order = 206)]
        //public decimal NetProducedTons { get; set; }

        [DataMember(Order = 210)]
        public decimal NetYield { get; set; }

        [DataMember(Order = 300)]
        public decimal ProductionHours { get; set; }

    }
}
