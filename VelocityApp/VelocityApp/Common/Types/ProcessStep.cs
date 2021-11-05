using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VelocityApp.Common.Types
{
    [CollectionDataContract(Namespace = "")]
    public class ProcessSteps : List<ProcessStep>
    {
    }

    [DataContract(Namespace = "")]
    public class ProcessStep
    {
        public ProcessStep()
        {
            StartDate = DateTime.Now;
            Errors = new Errors();
            Messages = new Messages();
            RowsAffected = 0;
        }

        public ProcessStep(string name)
        {
            Name = name;
            StartDate = DateTime.Now;
            Errors = new Errors();
            Messages = new Messages();
            ProcessResponses = new ProcessResponses();
            RowsAffected = 0;
        }

        [DataMember(Order = 10)]
        public string Name { get; set; }
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 22, EmitDefaultValue = false)]
        public DateTime EndDate { get; set; }
        [DataMember(Order = 23, EmitDefaultValue = false)]
        public decimal ElapsedSeconds { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = true)]
        public int RowsAffected { get; set; }
        [DataMember(Order = 50, EmitDefaultValue = false)]
        public Messages Messages { get; set; }
        [DataMember(Order = 100, EmitDefaultValue = false)]
        public Errors Errors { get; set; }
        [DataMember(Order = 200, EmitDefaultValue = false)]
        public ProcessResponses ProcessResponses { get; set; }

        public void Done()
        {
            EndDate = DateTime.Now;
            ElapsedSeconds = (decimal)(EndDate - StartDate).TotalSeconds;
        }

        public bool HasErrors()
        {
            return ProcessStepHasErrors(this);
            //return Errors.Any();
        }

        public bool ProcessStepHasErrors(ProcessStep processStep)
        {
            if (Errors.Any())
            {
                return true;
            }
            foreach (var childResponse in processStep.ProcessResponses)
            {
                if (childResponse.Errors.Any() || childResponse.ProcessSteps.Any(step => step.Errors.Any()))
                {
                    return true;
                }
                if (ResponseHasErrors(childResponse))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ResponseHasErrors(ProcessResponse response)
        {
            if (response.Errors.Any())
            {
                return true;
            }
            foreach (var childResponse in response.ProcessResponses)
            {
                if (childResponse.Errors.Any() || childResponse.ProcessSteps.Any(step => step.Errors.Any()))
                {
                    return true;
                }
                if (ResponseHasErrors(childResponse))
                {
                    return true;
                }
            }
            foreach (var step in response.ProcessSteps)
            {
                if (step.Errors.Any())
                {
                    return true;
                }
                if (step.ProcessResponses.Any(ResponseHasErrors))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
