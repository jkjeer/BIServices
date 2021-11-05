using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.E1
{
    [CollectionDataContract(Namespace = "")]
    // ReSharper disable once InconsistentNaming
    public class F0005s : List<F0005>
    {
    }

    [DataContract(Namespace = "")]
    public class F0005
    {
        public F0005()
        {
            DRSY = "";
            DRRT = "";
            DRKY = "";
            DRDL01 = "";
            DRDL02 = "";
            DRSPHD = "";
            DRUDCO = "";
            DRHRDC = "";
            DRUSER = "";
            DRPID = "";
            DRUPMJ = 0;
            DRJOBN = "";
            DRUPMT = 0;
            DTDL01 = "";
        }

        [DataMember(Order = 1)]
        public string DRSY { get; set; }
        [DataMember(Order = 2)]
        public string DRRT { get; set; }
        [DataMember(Order = 3)]
        public string DRKY { get; set; }
        [DataMember(Order = 4)]
        public string DRDL01 { get; set; }
        [DataMember(Order = 5)]
        public string DRDL02 { get; set; }
        [DataMember(Order = 6)]
        public string DRSPHD { get; set; }
        [DataMember(Order = 7)]
        public string DRUDCO { get; set; }
        [DataMember(Order = 8)]
        public string DRHRDC { get; set; }
        [DataMember(Order = 9)]
        public string DRUSER { get; set; }
        [DataMember(Order = 10)]
        public string DRPID { get; set; }
        [DataMember(Order = 11)]
        public decimal DRUPMJ { get; set; }
        [DataMember(Order = 12)]
        public string DRJOBN { get; set; }
        [DataMember(Order = 13)]
        public decimal DRUPMT { get; set; }
        [DataMember(Order = 20)]
        public string DTDL01 { get; set; }
    }
}
