using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Response
{
    [CollectionDataContract(Namespace = "")]
    public class SSISLogErrors : List<SSISLogError>
    {
    }

    [DataContract(Namespace = "")]
    public class SSISLogError
    {
        public SSISLogError()
        {
        }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Caller { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string Package { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public string ExecuTableName { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public DateTime MessageTime { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public string Information { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public long ExecutionID { get; set; }

    }
}
