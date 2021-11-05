using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{

    [CollectionDataContract(Namespace = "")]
    public class RacksByProductSet : List<RacksByProduct>
    {
    }

    [DataContract(Namespace = "")]
    public class RacksByProduct
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 20)]
        public decimal CustomerSoldNum { get; set; }

        [DataMember(Order = 22)]
        public string CustomerSoldName { get; set; }

        [DataMember(Order = 24)]
        public decimal CustomerShipNum { get; set; }

        [DataMember(Order = 26)]
        public string CustomerShipName { get; set; }

        [DataMember(Order = 42)]
        public string CardinalProductType { get; set; }

        [DataMember(Order = 100)]
        public int OpenRacksThruToday { get; set; }

        [DataMember(Order = 102)]
        public int OpenUnitsThruToday { get; set; }

        [DataMember(Order = 104)]
        public int OpenRacksAll { get; set; }

        [DataMember(Order = 106)]
        public int OpenUnitsAll { get; set; }

        [DataMember(Order = 110)]
        public int ClosedRacks { get; set; }

        [DataMember(Order = 112)]
        public int ClosedUnits { get; set; }

        [DataMember(Order = 120)]
        public int LockedRacks { get; set; }

        [DataMember(Order = 122)]
        public int LockedUnits { get; set; }

        [DataMember(Order = 130)]
        public int TotalRacks { get; set; }

        [DataMember(Order = 132)]
        public int TotalUnits { get; set; }
    }
}
