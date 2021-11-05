using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Response
{
    [CollectionDataContract(Namespace = "")]
    public class SSISLogExecutionSteps : List<SSISLogExecutionStep>
    {
    }

    [DataContract(Namespace = "")]
    public class SSISLogExecutionStep
    {
        public SSISLogExecutionStep()
        {
        }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Path { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string Result { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public DateTime StepStartTime { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public DateTime StepEndTime { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public decimal DurationSeconds { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public string StepInfo { get; set; }

        [DataMember(Order = 70, EmitDefaultValue = false)]
        public string StepName { get; set; }

        [DataMember(Order = 80, EmitDefaultValue = false)]
        public int StepNum { get; set; }
    }
}
