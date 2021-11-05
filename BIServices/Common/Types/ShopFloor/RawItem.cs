using System.Collections.Generic;
using System.Runtime.Serialization;
using BIServices.CtoSvc;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class RawItems : List<RawItem>
    {
    }

    [DataContract(Namespace = "")]
    public class RawItem
    {
        [DataMember(Order = 10)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12)]
        public string PlantNum { get; set; }

        [DataMember(Order = 20)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 22)]
        public string Description { get; set; }

        [DataMember(Order = 23)]
        public string Description2 { get; set; }

        [DataMember(Order = 30)]
        public decimal Thickness { get; set; }

        [DataMember(Order = 31)]
        public string ColorCode { get; set; }

        [DataMember(Order = 32)]
        public string PatternCode { get; set; }

        [DataMember(Order = 34)]
        public string HeatTreatmentCode { get; set; }

        [DataMember(Order = 36)]
        public string Surface1Coating { get; set; }

        [DataMember(Order = 36)]
        public string Surface2Coating { get; set; }

        [DataMember(Order = 38)]
        public string LamiTypeCode { get; set; }

        [DataMember(Order = 50)]
        public string MakeupCode { get; set; }

        [DataMember(Order = 60)]
        public decimal ActualSqft { get; set; }

        [DataMember(Order = 62)]
        public decimal QtyOnHand { get; set; }

        [DataMember(Order = 63)]
        public decimal QtyOnOrder { get; set; }

        [DataMember(Order = 64)]
        public decimal SafetyStock { get; set; }

        [DataMember(Order = 100)]
        public decimal OrderRemainingSheets { get; set; }

        [DataMember(Order = 102)]
        public decimal ItemRemainingSheets { get; set; }

        [DataMember(Order = 200)]
        public List<RawItemOrder> RawItemOrders { get; set; }

    }
}
