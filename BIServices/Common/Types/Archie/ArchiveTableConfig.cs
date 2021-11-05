using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Types.Archie
{
    [CollectionDataContract(Namespace = "")]

    public class ArchiveTableConfigs : List<ArchiveTableConfig>
    {
    }

    [DataContract(Namespace = "")]

    public class ArchiveTableConfig
    {

        public ArchiveTableConfig()
        {
            Command = "";
        }

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

        [DataMember(Order = 15)]
        public string QualifiedName => $@"[{ServerName}].[{DatabaseName}].[{SchemaName}].[{TableName}]";

        [DataMember(Order = 16)]
        public string RepoDatabaseName => $"{PlantCode}_{DatabaseName}_Archive";


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
        public DateTime LastArchiveDate { get; set; }
        [DataMember(Order = 35)]
        public DateTime NextArchiveDate { get; set; }

        [DataMember(Order = 36)]
        public bool TailSF { get; set; }

        [DataMember(Order = 370)]
        public string KeyColumn { get; set; }
        [DataMember(Order = 380)]
        public string DateColumn { get; set; }
        [DataMember(Order = 390)]
        public int RetentionDays { get; set; }
        [DataMember(Order = 37)]
        public int ArchiveFrequencyDays { get; set; }

        [DataMember(Order = 380)]
        public long InitialRowCount { get; set; }

        [DataMember(Order = 390)]
        public long RowsToDelete { get; set; }

        [DataMember(Order = 400)]
        public long MovedRows { get; set; }

        [DataMember(Order = 410)]
        public long MissingRows { get; set; }

        [DataMember(Order = 420)]
        public long DeletedRows { get; set; }

        [DataMember(Order = 410)]
        public long FinalRowCount { get; set; }

        [DataMember(Order = 40)]
        public Guid Guid { get; set; }

        [DataMember(Order = 50)]
        public string Command { get; set; }

        [DataMember(Order = 230)]
        public ValidationErrors ValidationErrors { get; set; }
        [DataMember(Order = 60, EmitDefaultValue = false)]
        public string ValidationStatus { get; set; }

        [DataMember(Order = 100, EmitDefaultValue = false)]
        public Plant Plant { get; set; }
    }
}
