using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class CutAggregates : List<CutAggregate>
    {
    }

    [DataContract(Namespace = "")]
    public class CutAggregate
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

        // CUTTING

        [DataMember(Order = 20)]
        public int ConsumedSheets { get; set; }

        [DataMember(Order = 22)]
        public decimal ConsumedSqft { get; set; }

        [DataMember(Order = 22)]
        public decimal ConsumedActualSqft { get; set; }

        [DataMember(Order = 22)]
        public decimal ConsumedBillableSqft { get; set; }

        [DataMember(Order = 24)]
        public decimal ConsumedTons { get; set; }

        [DataMember(Order = 25)]
        public int ConsumedOffalLites { get; set; }

        [DataMember(Order = 26)]
        public decimal ConsumedOffalSqft { get; set; }

        [DataMember(Order = 26)]
        public decimal ConsumedOffalActualSqft { get; set; }

        [DataMember(Order = 26)]
        public decimal ConsumedOffalBillableSqft { get; set; }

        [DataMember(Order = 27)]
        public decimal ConsumedOffalTons { get; set; }

        [DataMember(Order = 30)]
        public int GrossCutProducedLites { get; set; }

        [DataMember(Order = 32)]
        public decimal GrossCutProducedSqft { get; set; }

        [DataMember(Order = 32)]
        public decimal GrossCutProducedActualSqft { get; set; }

        [DataMember(Order = 32)]
        public decimal GrossCutProducedBillableSqft { get; set; }

        [DataMember(Order = 34)]
        public decimal GrossCutProducedTons { get; set; }

        [DataMember(Order = 40)]
        public int LoProCutLites { get; set; }

        [DataMember(Order = 42)]
        public decimal LoProCutSqft { get; set; }

        [DataMember(Order = 42)]
        public decimal LoProCutActualSqft { get; set; }

        [DataMember(Order = 42)]
        public decimal LoProCutBillableSqft { get; set; }

        [DataMember(Order = 44)]
        public decimal LoProCutTons { get; set; }

        [DataMember(Order = 45)]
        public int RareCutLites { get; set; }

        [DataMember(Order = 46)]
        public decimal RareCutSqft { get; set; }

        [DataMember(Order = 46)]
        public decimal RareCutActualSqft { get; set; }

        [DataMember(Order = 46)]
        public decimal RareCutBillableSqft { get; set; }

        [DataMember(Order = 47)]
        public decimal RareCutTons { get; set; }

        [DataMember(Order = 50)]
        public int CustomCutLites { get; set; }

        [DataMember(Order = 52)]
        public decimal CustomCutSqft { get; set; }

        [DataMember(Order = 52)]
        public decimal CustomCutActualSqft { get; set; }

        [DataMember(Order = 52)]
        public decimal CustomCutBillableSqft { get; set; }

        [DataMember(Order = 54)]
        public decimal CustomCutTons { get; set; }

        [DataMember(Order = 60)]
        public int GrossCutRejectedLites { get; set; }

        [DataMember(Order = 62)]
        public decimal GrossCutRejectedSqft { get; set; }

        [DataMember(Order = 62)]
        public decimal GrossCutRejectedActualSqft { get; set; }

        [DataMember(Order = 62)]
        public decimal GrossCutRejectedBillableSqft { get; set; }

        [DataMember(Order = 64)]
        public decimal GrossCutRejectedTons { get; set; }

        [DataMember(Order = 70)]
        public int VendorCutRejectedLites { get; set; }

        [DataMember(Order = 72)]
        public decimal VendorCutRejectedSqft { get; set; }

        [DataMember(Order = 72)]
        public decimal VendorCutRejectedActualSqft { get; set; }

        [DataMember(Order = 72)]
        public decimal VendorCutRejectedBillableSqft { get; set; }

        [DataMember(Order = 74)]
        public decimal VendorCutRejectedTons { get; set; }

        [DataMember(Order = 80)]
        public int InternalCutRejectedLites { get; set; }

        [DataMember(Order = 82)]
        public decimal InternalCutRejectedSqft { get; set; }

        [DataMember(Order = 82)]
        public decimal InternalCutRejectedActualSqft { get; set; }

        [DataMember(Order = 82)]
        public decimal InternalCutRejectedBillableSqft { get; set; }

        [DataMember(Order = 84)]
        public decimal InternalCutRejectedTons { get; set; }

        [DataMember(Order = 90)]
        public int NetCutProducedLites { get; set; }

        [DataMember(Order = 92)]
        public decimal NetCutProducedSqft { get; set; }

        [DataMember(Order = 92)]
        public decimal NetCutProducedActualSqft { get; set; }

        [DataMember(Order = 92)]
        public decimal NetCutProducedBillableSqft { get; set; }

        [DataMember(Order = 94)]
        public decimal NetCutProducedTons { get; set; }

        [DataMember(Order = 100)]
        public decimal CuttingYield { get; set; }

        [DataMember(Order = 100)]
        public decimal CuttingActualYield { get; set; }

        [DataMember(Order = 100)]
        public decimal CuttingBillableYield { get; set; }

        [DataMember(Order = 102)]
        public decimal NetYield { get; set; }

        [DataMember(Order = 102)]
        public decimal NetActualYield { get; set; }

        [DataMember(Order = 102)]
        public decimal NetBillableYield { get; set; }

        [DataMember(Order = 104)]
        public decimal CuttingHours { get; set; }

        [DataMember(Order = 106)]
        public int GlassTypes { get; set; }

    }
}
