using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class ProdLites : List<ProdLite>
    {
    }

    [DataContract(Namespace = "")]
    public class ProdLite
    {
        [DataMember(Order = 10)]
        public string Plant { get; set; }

        [DataMember(Order = 12)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 14)]
        public int UnitId { get; set; }

        [DataMember(Order = 16)]
        public int LiteNum { get; set; }

        // CONSTRUCTION
        [DataMember(Order = 150)]
        public string ProductCode { get; set; }

        [DataMember(Order = 152)]
        public int ShapeNum { get; set; }

        [DataMember(Order = 154)]
        public string Dimension { get; set; }

        [DataMember(Order = 156)]
        public decimal ActualSqft { get; set; }

        // TRANSACTION
        [DataMember(Order = 200)]
        public DateTime TransactionDate { get; set; }

        [DataMember(Order = 202)]
        public int Quantity { get; set; }

    }
}
