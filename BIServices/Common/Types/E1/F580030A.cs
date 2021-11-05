using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.E1
{

    [CollectionDataContract(Namespace = "")]
    public class F580030As : List<F580030A>
    {
    }

    [DataContract(Namespace = "")]
    public class F580030A
    {
        public F580030A()
        {
            QAUKID = 0;
            QAEDSP = "";
            QAEDCT = "";
            QAMCU = "";
            QAEDBT = "";
            QAZ58SCN = 0;
            QACRTU = "";
            QACPGM = "";
            QAJOBN = "";
            QACRTDT = 0;
            QACRTM = 0;
            QAUPMB = "";
            QAUPGM = "";
            QAMJOB = "";
            QAUPMJ = 0;
            QAUPMT = 0;
        }

        [DataMember(Order = 1)]
        public decimal QAUKID { get; set; }
        [DataMember(Order = 2)]
        public string QAEDSP { get; set; }
        [DataMember(Order = 3)]
        public string QAEDCT { get; set; }
        [DataMember(Order = 4)]
        public string QAMCU { get; set; }
        [DataMember(Order = 5)]
        public string QAEDBT { get; set; }
        [DataMember(Order = 6)]
        public decimal QAZ58SCN { get; set; }
        [DataMember(Order = 7)]
        public string QACRTU { get; set; }
        [DataMember(Order = 8)]
        public string QACPGM { get; set; }
        [DataMember(Order = 9)]
        public string QAJOBN { get; set; }
        [DataMember(Order = 10)]
        public decimal QACRTDT { get; set; }
        [DataMember(Order = 11)]
        public decimal QACRTM { get; set; }
        [DataMember(Order = 12)]
        public string QAUPMB { get; set; }
        [DataMember(Order = 13)]
        public string QAUPGM { get; set; }
        [DataMember(Order = 14)]
        public string QAMJOB { get; set; }
        [DataMember(Order = 15)]
        public decimal QAUPMJ { get; set; }
        [DataMember(Order = 16)]
        public decimal QAUPMT { get; set; }
    }
}
