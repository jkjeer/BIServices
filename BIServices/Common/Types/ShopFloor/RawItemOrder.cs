using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.ShopFloor
{
    [CollectionDataContract(Namespace = "")]
    public class RawItemOrders : List<RawItemOrder>
    {
    }

    [DataContract(Namespace = "")]
    public class RawItemOrder
    {

        [DataMember(Order = 20)]
        public decimal ItemNum { get; set; }

        [DataMember(Order = 30)]
        public decimal WorkOrderNum { get; set; }

        [DataMember(Order = 32)]
        public int LiteNum { get; set; }

        [DataMember(Order = 34)]
        public DateTime RequestedDate { get; set; }

        [DataMember(Order = 40)]
        public decimal WorkOrderQty { get; set; }

        [DataMember(Order = 42)]
        public decimal WorkOrderSqft { get; set; }

        [DataMember(Order = 44)]
        public decimal WorkOrderSheets { get; set; }

        [DataMember(Order = 50)]
        public decimal WorkOrderCutSqft { get; set; }

        [DataMember(Order = 52)]
        public decimal WorkOrderCutSheets { get; set; }

        [DataMember(Order = 60)]
        public decimal OrderRemainingSheets { get; set; }

        [DataMember(Order = 70)]
        public decimal ItemRemainingSheets { get; set; }

    }
}
