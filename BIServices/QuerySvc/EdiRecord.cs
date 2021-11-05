using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BIServices.QuerySvc
{
    [CollectionDataContract(Namespace = "")]
    public class EdiRecords : List<EdiRecord>
    {
    }

    [DataContract(Namespace = "")]
    public class EdiRecord
    {
        [DataMember(Order = 1)]
        public string EAEKCO { get; set; }
        [DataMember(Order = 2)]
        public string EAEDBT { get; set; }
        [DataMember(Order = 3)]
        public decimal EAUPMJ { get; set; }
        [DataMember(Order = 3)]
        public decimal EATDAY { get; set; }
        [DataMember(Order = 3)]
        public DateTime? EATIME { get; set; }
        [DataMember(Order = 4)]
        public decimal EAEDOC { get; set; }
        [DataMember(Order = 5)]
        public decimal EAEDLN { get; set; }
        [DataMember(Order = 6)]
        public string P96181 { get; set; }
        [DataMember(Order = 7)]
        public string P95154 { get; set; }
        [DataMember(Order = 8)]
        public string P95000 { get; set; }
        [DataMember(Order = 9)]
        public string P95001 { get; set; }
        [DataMember(Order = 10)]
        public string P95801 { get; set; }
        [DataMember(Order = 11)]
        public string P95901 { get; set; }
        [DataMember(Order = 12)]
        public string P95160 { get; set; }
        [DataMember(Order = 13)]
        public string SDMCU { get; set; }
        [DataMember(Order = 14)]
        public decimal SDDOCO { get; set; }
        [DataMember(Order = 15)]
        public string SDDCTO { get; set; }
        [DataMember(Order = 16)]
        public decimal SDLNID { get; set; }
        [DataMember(Order = 17)]
        public decimal SDAN8 { get; set; }
        [DataMember(Order = 18)]
        public string SDAN8NM { get; set; }
        [DataMember(Order = 19)]
        public string SDVR01 { get; set; }
        [DataMember(Order = 20)]
        public string SDDSC1 { get; set; }
        [DataMember(Order = 21)]
        public string SDDSC2 { get; set; }
        [DataMember(Order = 22)]
        public string SDCATNM { get; set; }
    }
}
