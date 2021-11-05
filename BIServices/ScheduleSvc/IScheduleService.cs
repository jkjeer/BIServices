using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ScheduleSvc
{
    [ServiceContract]
    public interface IScheduleService
    {
        [OperationContract, OperationInvokerLogger]
        ProcessResponse GetOverdueSchedules(int lookBackMinutes);

        [OperationContract, OperationInvokerLogger]
        ScheduleEvents GetScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty, string options = null);

        [OperationContract, OperationInvokerLogger]
        ScheduleEvents GetIncompleteMTOScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty);
    }

    [ServiceContract]
    public interface IScheduleRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOverdueSchedules?LookBackMinutes={lookBackMinutes}")]
        ProcessResponse GetOverdueSchedules(int lookBackMinutes);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&Options={options}")]
        ScheduleEvents GetScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty, string options = null);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetIncompleteMTOScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}")]
        ScheduleEvents GetIncompleteMTOScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty);
    }
}
