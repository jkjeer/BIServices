using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Types.Archie
{
    [CollectionDataContract(Namespace = "")]

    public class TruncateTableConfigs : List<TruncateTableConfig>
    {
    }

    [DataContract(Namespace = "")]

    public class TruncateTableConfig
    {
        [DataMember(Order = 10)]
        public string TableName { get; set; }
        [DataMember(Order = 11)]
        public string SchemaName { get; set; }
        [DataMember(Order = 12)]
        public string DatabaseName { get; set; }
        [DataMember(Order = 13)]
        public string ServerName { get; set; }
        [DataMember(Order = 14)]
        public string PlantCode { get; set; }
        [DataMember(Order = 141)]
        public string GroupName { get; set; }

        [DataMember(Order = 20)]
        public int ID { get; set; }

        [DataMember(Order = 30)]
        public bool IsActive { get; set; }
        [DataMember(Order = 31)]
        public bool IsEnabled { get; set; }
        [DataMember(Order = 32)]
        public int LastRunStatus { get; set; }
        [DataMember(Order = 33)]
        public string HostName { get; set; }
        [DataMember(Order = 34)]
        public DateTime LastTruncateDate { get; set; }
        [DataMember(Order = 35)]
        public DateTime NextTruncateDate { get; set; }
        [DataMember(Order = 36)]
        public bool TruncateSF { get; set; }
        [DataMember(Order = 37)]
        public int TruncateFrequencyDays { get; set; }
        [DataMember(Order = 38)]
        public long InitRowCount { get; set; }
        [DataMember(Order = 39)]
        public long TruncatedRows { get; set; }

        [DataMember(Order = 40)]
        public Guid Guid { get; set; }

        [DataMember(Order = 230)]
        public ValidationErrors ValidationErrors { get; set; }
        [DataMember(Order = 60, EmitDefaultValue = false)]
        public string ValidationStatus { get; set; }

        [DataMember(Order = 100, EmitDefaultValue = false)]
        public Plant Plant { get; set; }
    }
}
