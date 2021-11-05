using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BIServices.QuerySvc
{

    [CollectionDataContract(Namespace = "")]
    public class IntercompanyLoads : List<IntercompanyLoad>
    {
    }

    [DataContract(Namespace = "")]
    public class IntercompanyLoad
    {
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string SupplierPlant { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public string SupplierPlantCode { get; set; }

        [DataMember(Order = 14, EmitDefaultValue = false)]
        public string SupplierPlantName { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = false)]
        public decimal SupplierPlantNum { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string ReceivingPlant { get; set; }

        [DataMember(Order = 22, EmitDefaultValue = false)]
        public string ReceivingPlantCode { get; set; }

        [DataMember(Order = 24, EmitDefaultValue = false)]
        public string ReceivingPlantName { get; set; }

        [DataMember(Order = 26, EmitDefaultValue = false)]
        public decimal ReceivingPlantNum { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public decimal LoadNum { get; set; }

        [DataMember(Order = 32, EmitDefaultValue = false)]
        public DateTime LoadDateTime { get; set; }

        [DataMember(Order = 32, EmitDefaultValue = false)]
        public DateTime LoadDateTimeLocal { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public decimal RackId { get; set; }

        [DataMember(Order = 42, EmitDefaultValue = false)]
        public string RackType { get; set; }

        [DataMember(Order = 44, EmitDefaultValue = false)]
        public string StockType { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public decimal GlassWeight { get; set; }

        [DataMember(Order = 52, EmitDefaultValue = false)]
        public decimal RackWeight { get; set; }

        [DataMember(Order = 54, EmitDefaultValue = false)]
        public decimal TotalWeight { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public int Racks { get; set; }

        [DataMember(Order = 62, EmitDefaultValue = false)]
        public decimal Units { get; set; }
    }
}
