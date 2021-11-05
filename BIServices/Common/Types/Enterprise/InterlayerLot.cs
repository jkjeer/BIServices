using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class InterlayerLots : List<InterlayerLot>
    {
    }

    [DataContract(Namespace = "")]
    public class InterlayerLot
    {
        [DataMember(Order = 10)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public string PlantCode { get; set; }

        [DataMember(Order = 20)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 22)]
        public string LotNum { get; set; }

        [DataMember(Order = 24)]
        public string SerialNum { get; set; }

        [DataMember(Order = 26)]
        public string Location { get; set; }

        [DataMember(Order = 30)]
        public string UnitType { get; set; }

        [DataMember(Order = 32)]
        public string Desc1 { get; set; }

        [DataMember(Order = 34)]
        public string Desc2 { get; set; }

        [DataMember(Order = 36)]
        public string StockType { get; set; }

        [DataMember(Order = 38)]
        public string GlCode { get; set; }

        [DataMember(Order = 50)]
        public decimal QtyOnHand { get; set; }

        [DataMember(Order = 52)]
        public decimal Sqft { get; set; }

        [DataMember(Order = 54)]
        public string UOM1 { get; set; }

        [DataMember(Order = 56)]
        public string UOM2 { get; set; }

        [DataMember(Order = 60)]
        public decimal SqftRate { get; set; }

        [DataMember(Order = 100)]
        public string ProductCode { get; set; }

        [DataMember(Order = 101)]
        public decimal Thickness { get; set; }

        [DataMember(Order = 102)]
        public string Material { get; set; }

        [DataMember(Order = 104)]
        public string Color { get; set; }

        [DataMember(Order = 106)]
        public string Direction { get; set; }

        [DataMember(Order = 108)]
        public decimal BaseDim { get; set; }

        [DataMember(Order = 110)]
        public decimal LeftDim { get; set; }

        [DataMember(Order = 150)]
        public decimal SupplierNum { get; set; }

        [DataMember(Order = 152)]
        public string SupplierName { get; set; }

        [DataMember(Order = 154)]
        public DateTime? ReceivedDate { get; set; }

    }


}