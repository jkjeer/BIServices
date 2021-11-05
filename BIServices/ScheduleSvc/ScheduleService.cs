using System;
using BIServices.Common.Types.Enterprise;
using NLog;
using SoaCore.Wcf;

namespace BIServices.ScheduleSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class ScheduleService : IScheduleService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProcessResponse GetOverdueSchedules(int lookBackMinutes = 0)
        {
            try
            {
                return ScheduleProcessor.GetOverdueSchedules(lookBackMinutes);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ScheduleEvents GetScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty, string options = null)
        {
            plantCode = plantCode.Replace("amp;", "");
            dateCode = dateCode?.Replace("amp;", "");
            dateProperty = dateProperty?.Replace("amp;", "");
            Logger.Info($@"/GetScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&Options={options}");
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            //if (string.IsNullOrEmpty(dateCode))
            //{
            //    throw new Exception("Date Code is REQUIRED.");
            //}
            //if (string.IsNullOrEmpty(dateProperty))
            //{
            //    throw new Exception("Date Property is REQUIRED.");
            //}
            try
            {
                return ScheduleProcessor.GetScheduleEventSummary(plantCode, dateCode, startDate, endDate, dateProperty, options);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ScheduleEvents GetIncompleteMTOScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            plantCode = plantCode.Replace("amp;", "");
            dateCode = dateCode?.Replace("amp;", "");
            dateProperty = dateProperty?.Replace("amp;", "");
            Logger.Info($@"/GetIncompleteMTOScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}");
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            //if (string.IsNullOrEmpty(dateCode))
            //{
            //    throw new Exception("Date Code is REQUIRED.");
            //}
            //if (string.IsNullOrEmpty(dateProperty))
            //{
            //    throw new Exception("Date Property is REQUIRED.");
            //}
            try
            {
                var options = "S6EDSP<>'C' AND S6Z58SCN<1000";
                return ScheduleProcessor.GetScheduleEventSummary(plantCode, dateCode, startDate, endDate, dateProperty, options);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class ScheduleRestService : IScheduleRestService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProcessResponse GetOverdueSchedules(int lookBackMinutes = 0)
        {
            try
            {
                return ScheduleProcessor.GetOverdueSchedules(lookBackMinutes);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ScheduleEvents GetScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty, string options = null)
        {
            plantCode = plantCode.Replace("amp;", "");
            dateCode = dateCode?.Replace("amp;", "");
            dateProperty = dateProperty?.Replace("amp;", "");
            Logger.Info($@"/GetScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}&Options={options}");
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            //if (string.IsNullOrEmpty(dateCode))
            //{
            //    throw new Exception("Date Code is REQUIRED.");
            //}
            //if (string.IsNullOrEmpty(dateProperty))
            //{
            //    throw new Exception("Date Property is REQUIRED.");
            //}
            try
            {
                return ScheduleProcessor.GetScheduleEventSummary(plantCode, dateCode, startDate, endDate, dateProperty, options);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ScheduleEvents GetIncompleteMTOScheduleEventSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            plantCode = plantCode.Replace("amp;", "");
            dateCode = dateCode?.Replace("amp;", "");
            dateProperty = dateProperty?.Replace("amp;", "");
            Logger.Info($@"/GetIncompleteMTOScheduleEventSummary?PlantCode={plantCode}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}&DateProperty={dateProperty}");
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            //if (string.IsNullOrEmpty(dateCode))
            //{
            //    throw new Exception("Date Code is REQUIRED.");
            //}
            //if (string.IsNullOrEmpty(dateProperty))
            //{
            //    throw new Exception("Date Property is REQUIRED.");
            //}
            try
            {
                var options = "S6EDSP<>'C' AND S6Z58SCN<1000";
                return ScheduleProcessor.GetScheduleEventSummary(plantCode, dateCode, startDate, endDate, dateProperty, options);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
