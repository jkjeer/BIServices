using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Enterprise
{
    [CollectionDataContract(Namespace = "")]
    public class ScheduleEvents : List<ScheduleEvent>
    {
    }

    [DataContract(Namespace = "")]
    public class ScheduleEvent
    {
        [DataMember(Order = 10)]
        public string PlantNum { get; set; }

        [DataMember(Order = 11)]
        public string PlantCode { get; set; }

        [DataMember(Order = 12)]
        public string Location { get; set; }

        [DataMember(Order = 20)]
        public decimal ScheduleNum { get; set; }

        [DataMember(Order = 30)]
        public string BatchNum { get; set; }

        [DataMember(Order = 32)]
        public string CostCenter { get; set; }

        [DataMember(Order = 34)]
        public DateTime ScheduleDate { get; set; }
        [DataMember(Order = 35)]
        public decimal ScheduleDateE1 { get; set; }

        [DataMember(Order = 36)]
        public string ShopFloorSystem { get; set; }

        [DataMember(Order = 38)]
        public string StatusCode { get; set; }

        [DataMember(Order = 40)]
        public string ProcessFlag { get; set; }

        [DataMember(Order = 42)]
        public decimal MathNumeric06 { get; set; }

        [DataMember(Order = 44)]
        public string OperationStatus { get; set; }

        [DataMember(Order = 46)]
        public string Description { get; set; }

        [DataMember(Order = 500)]
        public string UpdateDateTimeString { get; set; }

        [DataMember(Order = 510)]
        public decimal Age { get; set; }

        [DataMember(Order = 520)]
        public decimal RemainingMinutes { get; set; }

        [DataMember(Order = 600)]
        public DateTime UpdateDateTimeS6 { get; set; }
        [DataMember(Order = 601)]
        public decimal UpdateDateE1S6 { get; set; }
        [DataMember(Order = 602)]
        public decimal UpdateTimeE1S6 { get; set; }

        [DataMember(Order = 700)]
        public DateTime UpdateDateTimeS7 { get; set; }
        [DataMember(Order = 701)]
        public decimal UpdateDateE1S7 { get; set; }
        [DataMember(Order = 702)]
        public decimal UpdateTimeE1S7 { get; set; }

        [DataMember(Order = 800)]
        public DateTime UpdateDateTimeS8 { get; set; }
        [DataMember(Order = 801)]
        public decimal UpdateDateE1S8 { get; set; }
        [DataMember(Order = 802)]
        public decimal UpdateTimeE1S8 { get; set; }

        [DataMember(Order = 900)]
        public string ShopFloorSystemName { get; set; }

        [DataMember(Order = 901)]
        public int WorkOrderCount { get; set; }

        [DataMember(Order = 902)]
        public DateTime ScheduleDueDate { get; set; }
        [DataMember(Order = 903)]
        public decimal ScheduleDueDateE1 { get; set; }

        [DataMember(Order = 904)]
        public decimal Quantity { get; set; }
        
        [DataMember(Order = 905)]
        public string ScheduleType { get; set; }

        [DataMember(Order = 906)]
        public decimal QuantityScheduled { get; set; }

        [DataMember(Order = 907)]
        public decimal ScheduledWorkorders { get; set; }

        [DataMember(Order = 908)]
        public string ForceLink { get; set; }

        [DataMember(Order = 909)]
        public string CancelLink { get; set; }

        [DataMember(Order = 910)]
        public string SuccessLink { get; set; }

        [DataMember(Order = 911)]
        public string ResetLink { get; set; }

        [DataMember(Order = 912)]
        public string CompleteLink { get; set; }
    }
}
