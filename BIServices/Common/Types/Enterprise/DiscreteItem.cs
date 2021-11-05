using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class DiscreteItems : List<DiscreteItem>
    {
    }

    [DataContract(Namespace = "")]
    public class DiscreteItem
    {
        [DataMember(Order = 10)]
        public string PlantNum { get; set; }

        [DataMember(Order = 12)]
        public string PlantCode { get; set; }

        [DataMember(Order = 20)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 20)]
        public string UnitType { get; set; }

        [DataMember(Order = 22)]
        public string Desc1 { get; set; }

        [DataMember(Order = 24)]
        public string Desc2 { get; set; }

        [DataMember(Order = 26)]
        public string  StockType{ get; set; }

        [DataMember(Order = 28)]
        public string GlCode { get; set; }

        [DataMember(Order = 50)]
        public decimal QtyOnHand { get; set; }

        [DataMember(Order = 60)]
        public decimal StandardCost { get; set; }

        [DataMember(Order = 70)]
        public decimal UomSqft { get; set; }

        [DataMember(Order = 72)]
        public decimal UomLbs { get; set; }

        [DataMember(Order = 80)]
        public decimal AttActualSqft { get; set; }

        [DataMember(Order = 90)]
        public decimal BomSqft { get; set; }

        // [DataMember(Order = 100)]
        public ItemAttributes ItemAttributes { get; set; }

        [DataMember(Order = 200)]
        public List<String> Messages { get; set; }


    }

  
}