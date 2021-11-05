using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

// ReSharper disable InconsistentNaming

namespace BIServices.Common.Types.Archie
{
    [CollectionDataContract(Namespace = "")]
    public class TCCConfigurations : List<TCCConfiguration>
    {
    }

    [DataContract(Namespace = "")]

    public class TCCConfiguration
    {
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Category { get; set; }
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string DOCSUBJECT { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = false)]
        public int Documents { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = false)]
        public int Offlined { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = false)]
        public int ToDo { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public int OfflineDays { get; set; }
        [DataMember(Order = 50, EmitDefaultValue = false)]
        public int DeleteMonths { get; set; }
        [DataMember(Order = 60, EmitDefaultValue = false)]
        public DateTime WrittenDate { get; set; }
        [DataMember(Order = 70, EmitDefaultValue = false)]
        public bool AllowDeleting { get; set; }
        [DataMember(Order = 80, EmitDefaultValue = false)]
        public string AllowDeletingString { get; set; }
    }
}
