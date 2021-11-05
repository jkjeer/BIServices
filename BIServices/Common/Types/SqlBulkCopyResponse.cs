using System;
using System.Runtime.Serialization;

namespace BIServices.Common.Types
{
    [DataContract(Namespace = "")]
    public class SqlBulkCopyResponse
    {
        [DataMember(Order = 10)]
        public string ConnectionString { get; set; }
        [DataMember(Order = 20)]
        public string TableName { get; set; }
        [DataMember(Order = 30)]
        public long RowsCopied { get; set; }
        [DataMember(Order = 40)]
        public DateTime StartTime { get; set; }
        [DataMember(Order = 50)]
        public DateTime EndTime { get; set; }
        [DataMember(Order = 55)]
        public Boolean HasErrors { get; set; }
        [DataMember(Order = 60)]
        public string Message { get; set; }
    }
}
