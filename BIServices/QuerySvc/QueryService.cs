using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SoaCore.Wcf;

namespace BIServices.QuerySvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class QueryService : IQueryService
    {
        public ClosedRacks GetClosedRacks(string plantCode)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetClosedRacks(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetRackEventsByHour(plantCode, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetRacksByLane(plantCode, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public OpenUnits GetOpenUnits(string plantCode, string maxDate, DateTime? startDate, DateTime? endDate, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, decimal customerSoldNum = 0, decimal customerShipNum = 0)
        {
            var processor = new QueryProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var optionsList = new List<string>();
                if (!String.IsNullOrEmpty(scheduleNums))
                {
                    try
                    {
                        scheduleNumsList =
                            Array.ConvertAll<string, decimal>(scheduleNums.Split(','), Convert.ToDecimal).ToList();
                    }
                    catch
                    {
                        //scheduleNumsList = null;
                    }
                }
                if (!String.IsNullOrEmpty(igLine))
                {
                    igLinesList = Array.ConvertAll<string, int>(igLine.Split(','), int.Parse).ToList();
                }
                if (!String.IsNullOrEmpty(rackStatus))
                {
                    rackStatusList = rackStatus.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(unitType))
                {
                    unitTypeList = unitType.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenUnits(plantCode, maxDate, startDate, endDate, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public EdiRecords GetAndersenEdi(int startDaysBack, int endDaysBack)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetAndersenEdi(startDaysBack, endDaysBack);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public IntercompanyLoads GetIntercompanyLoads(string plantCode)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetIntercompanyLoads(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public Task<DataTable> HttpClientTestMethod()
        {
            var processor = new QueryProcessor();
            try
            {
                return HttpClientTest.TestMethod();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class QueryRestService : IQueryRestService
    {
        public ClosedRacks GetClosedRacks(string plantCode)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetClosedRacks(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetRackEventsByHour(plantCode, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, string startDate, string endDate)
        {
            var processor = new QueryProcessor();
            try
            {
                if (dateCode != null)
                {
                    dateCode = dateCode.Replace(",", "").Replace("'", "");                    
                }

                DateTime? startDateVal;
                try
                {
                    startDateVal = DateTime.Parse(startDate);
                }
                catch
                {
                    startDateVal = null;
                }

                DateTime? endDateVal;
                try
                {
                    endDateVal = DateTime.Parse(endDate);
                }
                catch
                {
                    endDateVal = null;
                }

                plantCode = plantCode.Replace("'", "");
                return processor.GetRacksByLane(plantCode, dateCode, startDateVal, endDateVal);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public OpenUnits GetOpenUnits(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, decimal customerSoldNum, decimal customerShipNum)
        {
            var processor = new QueryProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var optionsList = new List<string>();
                DateTime? startDateVal = null;
                DateTime? endDateVal = null;
                if (!String.IsNullOrEmpty(scheduleNums))
                {
                    try
                    {
                        scheduleNumsList =
                            Array.ConvertAll<string, decimal>(scheduleNums.Split(','), Convert.ToDecimal).ToList();
                    }
                    catch
                    {
                        //scheduleNumsList = null;
                    }
                }
                if (!String.IsNullOrEmpty(igLine))
                {
                    igLinesList = Array.ConvertAll<string, int>(igLine.Split(','), int.Parse).ToList();
                }
                if (!String.IsNullOrEmpty(rackStatus))
                {
                    rackStatusList = rackStatus.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(unitType))
                {
                    unitTypeList = unitType.Split(',').ToList();
                }
                if (startDate != DateTime.MinValue)
                {
                    startDateVal = startDate;
                }
                if (endDate != DateTime.MinValue)
                {
                    endDateVal = endDate;
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenUnits(plantCode, maxDate, startDateVal, endDateVal, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public EdiRecords GetAndersenEdi(int startDaysBack, int endDaysBack)
        {
            var processor = new QueryProcessor();
            try
            {
                return processor.GetAndersenEdi(startDaysBack, endDaysBack);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public IntercompanyLoads GetIntercompanyLoads(string plantCode)
        {
            var processor = new QueryProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetIntercompanyLoads(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public Task<DataTable> HttpClientTestMethod()
        {
            var processor = new QueryProcessor();
            try
            {
                return HttpClientTest.TestMethod();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }


}
