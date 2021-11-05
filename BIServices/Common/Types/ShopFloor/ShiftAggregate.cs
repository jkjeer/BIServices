using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class ShiftAggregates : List<ShiftAggregate>
    {
    }

    [DataContract(Namespace = "")]
    public class ShiftAggregate
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

        [DataMember(Order = 202)]
        public decimal TempActualYield { get; set; }

        [DataMember(Order = 204)]
        public decimal TempBillableYield { get; set; }


    }
}
