using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using SoaCore.Wcf;

namespace BIServices.QuerySvc
{
    [ServiceContract]
    public interface IQueryService
    {
        [OperationContract, OperationInvokerLogger]
        ClosedRacks GetClosedRacks(string plantCode);

        [OperationContract, OperationInvokerLogger]
        RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, DateTime? startDate = null, DateTime? endDate = null);

        [OperationContract, OperationInvokerLogger]
        RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, DateTime? startDate = null, DateTime? endDate = null);

        [OperationContract, OperationInvokerLogger]
        OpenUnits GetOpenUnits(string plantCode, string maxDate, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, decimal customerSoldNum = 0, decimal customerShipNum = 0);

        [OperationContract, OperationInvokerLogger]
        EdiRecords GetAndersenEdi(int startDaysBack, int endDaysBack);

        [OperationContract, OperationInvokerLogger]
        IntercompanyLoads GetIntercompanyLoads(string plantCode);

        [OperationContract, OperationInvokerLogger]
        Task<DataTable> HttpClientTestMethod();
    }

    [ServiceContract]
    public interface IQueryRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetClosedRacks?PlantCode={plantCode}")]
        ClosedRacks GetClosedRacks(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRackEventsByHour?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRacksByLane?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOpenUnits?PlantCode={plantCode}&RequestedDate={maxDate}&StartDate={startDate}&EndDate={endDate}&CustomerNum={custNum}&CustomerSoldNum={customerSoldNum}&CustomerShipNum={customerShipNum}&RackType={rackType}&ScheduleType={scheduleType}&ScheduleNums={scheduleNums}&IGLine={igLine}&RackStatus={rackStatus}&UnitType={unitType}&Options={options}")]
        OpenUnits GetOpenUnits(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, decimal customerSoldNum, decimal customerShipNum);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetAndersenEdi?StartDaysBack={startDaysBack}&EndDaysBack={endDaysBack}")]
        EdiRecords GetAndersenEdi(int startDaysBack, int endDaysBack);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetIntercompanyLoads?PlantCode={plantCode}")]
        IntercompanyLoads GetIntercompanyLoads(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/HttpClientTestMethod")]
        Task<DataTable> HttpClientTestMethod();
    }

}
