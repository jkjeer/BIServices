using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Response
{
    [CollectionDataContract(Namespace = "")]
    public class SSISLogExecutions : List<SSISLogExecution>
    {
    }

    [DataContract(Namespace = "")]
    public class SSISLogExecution
    {
        public SSISLogExecution()
        {
        }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Package { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string Result { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public DateTime ExecutionStartTime { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public bool UseHighWaterMarks { get; set; }

        [DataMember(Order = 50, EmitDefaultValue = false)]
        public int HighWaterOffsetMinutes { get; set; }

        [DataMember(Order = 60, EmitDefaultValue = false)]
        public String StartDate { get; set; }

        [DataMember(Order = 70, EmitDefaultValue = false)]
        public String EndDate { get; set; }

        [DataMember(Order = 80, EmitDefaultValue = false)]
        public int StartRelativeDays { get; set; }

        [DataMember(Order = 90, EmitDefaultValue = false)]
        public int EndRelativeDays { get; set; }

        [DataMember(Order = 100, EmitDefaultValue = false)]
        public long ExecutionID { get; set; }

        [DataMember(Order = 110, EmitDefaultValue = false)]
        public SSISLogExecutionSteps Summary { get; set; }

        [DataMember(Order = 120, EmitDefaultValue = false)]
        public SSISLogExecutionPackages Detail { get; set; }

    }
}
