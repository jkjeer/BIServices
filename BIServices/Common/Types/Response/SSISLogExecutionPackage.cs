using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Response
{
    [CollectionDataContract(Namespace = "")]
    public class SSISLogExecutionPackages : List<SSISLogExecutionPackage>
    {
    }

    [DataContract(Namespace = "")]
    public class SSISLogExecutionPackage
    {
        public SSISLogExecutionPackage()
        {
        }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public int StepNum { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string PackagePath { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public SSISLogExecutionSteps Steps { get; set; }
        
    }
}
