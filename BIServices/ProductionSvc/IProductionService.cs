using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using BIServices.QuerySvc;
using SoaCore.Wcf;

namespace BIServices.ProductionSvc
{
    [ServiceContract]
    public interface IProductionService
    {
        [OperationContract, OperationInvokerLogger]
        ClosedRacks GetClosedRacks(string plantCode);

        [OperationContract, OperationInvokerLogger]
        RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, DateTime? startDate = null, DateTime? endDate = null);

        [OperationContract, OperationInvokerLogger]
        RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, DateTime? startDate = null, DateTime? endDate = null);

        [OperationContract, OperationInvokerLogger]
        GlassUnits GetOpenUnits(string plantCode, string maxDate, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, string rackIds = null, string productionTypes = null);

        [OperationContract, OperationInvokerLogger]
        ProdUnits GetOpenUnitsLocal(string plantCode, string maxDate, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, string rackIds = null, string productionTypes = null);

        [OperationContract, OperationInvokerLogger]
        GlassRacks GetOpenRacks(string plantCode, string maxDate, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, string rackIds = null, string productionTypes = null);

        [OperationContract, OperationInvokerLogger]
        IntercompanyLoads GetIntercompanyLoads(string plantCode);

        [OperationContract, OperationInvokerLogger]
        RequestedDateFacts GetRequestedDateFacts(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate);

        [OperationContract, OperationInvokerLogger]
        ScheduledDateFacts GetScheduledDateFacts(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate);

        [OperationContract, OperationInvokerLogger]
        List<ScheduleSummary> GetScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty);

        [OperationContract, OperationInvokerLogger]
        RacksByProductSet GetRacksByProduct(string plantCode, decimal customerNum);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse FG_ShiftTonsPulled();
    }

    [ServiceContract]
    public interface IProductionRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetClosedRacks?PlantCode={plantCode}")]
        ClosedRacks GetClosedRacks(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRackEventsByHour?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRacksByLane?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOpenUnits?PlantCode={plantCode}&RequestedDate={maxDate}&StartDate={startDate}&EndDate={endDate}&CustNum={custNum}&CustomerSoldNum={customerSoldNum}&CustomerShipNum={customerShipNum}&RackType={rackType}&ScheduleType={scheduleType}&ScheduleNums={scheduleNums}&IGLine={igLine}&RackStatus={rackStatus}&UnitType={unitType}&BoxNum={boxNum}&Options={options}&OrderDate={orderDateCode}&HoursToTruck={hoursToTruck}&RackIds={rackIds}&ProductionType={productionTypes}")]
        GlassUnits GetOpenUnits(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, string customerSoldNum, string customerShipNum, decimal boxNum, string orderDateCode, decimal hoursToTruck, string rackIds, string productionTypes);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOpenUnitsLocal?PlantCode={plantCode}&RequestedDate={maxDate}&StartDate={startDate}&EndDate={endDate}&CustNum={custNum}&CustomerSoldNum={customerSoldNum}&CustomerShipNum={customerShipNum}&RackType={rackType}&ScheduleType={scheduleType}&ScheduleNums={scheduleNums}&IGLine={igLine}&RackStatus={rackStatus}&UnitType={unitType}&BoxNum={boxNum}&Options={options}&OrderDate={orderDateCode}&HoursToTruck={hoursToTruck}&RackIds={rackIds}&ProductionType={productionTypes}")]
        ProdUnits GetOpenUnitsLocal(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, string customerSoldNum, string customerShipNum, decimal boxNum, string orderDateCode, decimal hoursToTruck, string rackIds, string productionTypes);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetOpenRacks?PlantCode={plantCode}&RequestedDate={maxDate}&StartDate={startDate}&EndDate={endDate}&CustomerNum={custNum}&CustomerSoldNum={customerSoldNum}&CustomerShipNum={customerShipNum}&RackType={rackType}&ScheduleType={scheduleType}&ScheduleNums={scheduleNums}&IGLine={igLine}&RackStatus={rackStatus}&UnitType={unitType}&BoxNum={boxNum}&Options={options}&OrderDate={orderDateCode}&HoursToTruck={hoursToTruck}&RackIds={rackIds}&ProductionType={productionTypes}")]
        GlassRacks GetOpenRacks(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, string customerSoldNum, string customerShipNum, decimal boxNum, string orderDateCode, decimal hoursToTruck, string rackIds, string productionTypes);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetIntercompanyLoads?PlantCode={plantCode}")]
        IntercompanyLoads GetIntercompanyLoads(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRequestedDateFacts?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        RequestedDateFacts GetRequestedDateFacts(string plantCode, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetScheduledDateFacts?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        ScheduledDateFacts GetScheduledDateFacts(string plantCode, string dateCode, DateTime startDate, DateTime endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetScheduleSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}")]
        List<ScheduleSummary> GetScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSfScheduleSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}")]
        List<ScheduleSummary> GetSfScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetRacksByProduct?PlantCode={plantCode}&CustomerNum={customerNum}")]
        RacksByProductSet GetRacksByProduct(string plantCode, decimal customerNum);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetHoleDrillingRacks?Plant={plantCode}")]
        GlassUnits GetHoleDrillingRacks(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSalvagedGlass?Plant={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        ProdLites GetSalvagedGlass(string plantCode, string dateCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/FG_ShiftTonsPulled")]
        ProcessResponse FG_ShiftTonsPulled();
    }
}
