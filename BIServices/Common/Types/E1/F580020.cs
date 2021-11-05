using System.Collections.Generic;
using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Types.E1
{
    [CollectionDataContract(Namespace = "")]
    public class F580020s : List<F580020>
    {
    }

    [DataContract(Namespace = "")]
    public class F580020
    {
        [DataMember(Order = 10)]
        public decimal S6Z58SCN { get; set; }

        [DataMember(Order = 20)]
        public string S6EDBT { get; set; }

        [DataMember(Order = 30)]
        public string S6MCU { get; set; }

        [DataMember(Order = 40)]
        public decimal S6Z58SCD { get; set; }

        [DataMember(Order = 50)]
        public string S6Z58SFS { get; set; }

        [DataMember(Order = 60)]
        public string S6TSC { get; set; }

        [DataMember(Order = 70)]
        public string S6EDSP { get; set; }

        [DataMember(Order = 80)]
        public decimal S6MATH06 { get; set; }

        [DataMember(Order = 90)]
        public string S6WOPST { get; set; }

        [DataMember(Order = 100)]
        public string S6DS50 { get; set; }

        [DataMember(Order = 110)]
        public decimal S6UPMJ { get; set; }

        [DataMember(Order = 120)]
        public decimal S6TDAY { get; set; }

        [DataMember(Order = 110)]
        public decimal S7UPMJ { get; set; }

        [DataMember(Order = 120)]
        public decimal S7TDAY { get; set; }

        [DataMember(Order = 110)]
        public decimal S8UPMJ { get; set; }

        [DataMember(Order = 120)]
        public decimal S8TDAY { get; set; }

        [DataMember(Order = 120)]
        public decimal S7SHAN { get; set; }

        [DataMember(Order = 120)]
        public string S7ALPH { get; set; }

        [DataMember(Order = 130)]
        public string DRDL01 { get; set; }

        [DataMember(Order = 140)]
        public int WOCount { get; set; }

        [DataMember(Order = 150)]
        public decimal S7Z58SCDDJ { get; set; }

        [DataMember(Order = 160)]
        public string S7Z58SCTYP { get; set; }
        
        [DataMember(Order = 170)]
        public decimal Quantity { get; set; }

        [DataMember(Order = 180)]
        public decimal S1Z58TQSC { get; set; }

        [DataMember(Order = 190)]
        public decimal ScheduledWorkorders { get; set; }
    }
}
