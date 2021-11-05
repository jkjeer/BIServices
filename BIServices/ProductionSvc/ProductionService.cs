using System;
using System.Collections.Generic;
using System.Linq;
using BIServices.Common.Types.Enterprise;
using BIServices.QuerySvc;
using BIServices.ScheduleSvc;
using NLog;
using SoaCore.Wcf;

namespace BIServices.ProductionSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class ProductionService : IProductionService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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

        public GlassUnits GetOpenUnits(string plantCode, string maxDate, DateTime? startDate, DateTime? endDate, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, string rackIds = null, string productionTypes = null)
        {
            Logger.Trace($@"/GetOpenUnits?PlantCode={plantCode}&RequestedDate={maxDate}&StartDate={startDate}&EndDate={endDate}&CustomerNum={custNum}&CustomerSoldNum={customerSoldNum}&CustomerShipNum={customerShipNum}&RackType={rackType}&ScheduleType={scheduleType}&ScheduleNums={scheduleNums}&IGLine={igLine}&RackStatus={rackStatus}&UnitType={unitType}&BoxNum={boxNum}&Options={options}&OrderDate={orderDateCode}&HoursToTruck={hoursToTruck}");
            var processor = new ProductionProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var rackIdsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var customerSoldNumList = new List<decimal>();
                var customerShipNumList = new List<decimal>();
                var optionsList = new List<string>();
                var productionTypeList = new List<string>();
                if (!String.IsNullOrEmpty(scheduleNums))
                {
                    try
                    {
                        scheduleNumsList = Array.ConvertAll<string, decimal>(scheduleNums.Split(','), Convert.ToDecimal).ToList();
                    }
                    catch
                    {
                        //scheduleNumsList = null;
                    }
                }
                if (!String.IsNullOrEmpty(rackIds))
                {
                    try
                    {
                        rackIdsList = Array.ConvertAll<string, decimal>(rackIds.Split(','), Convert.ToDecimal).ToList();
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
                if (!String.IsNullOrEmpty(productionTypes))
                {
                    productionTypeList = productionTypes.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenUnits(plantCode, maxDate, startDate, endDate, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypeList);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProdUnits GetOpenUnitsLocal(string plantCode, string maxDate, DateTime? startDate, DateTime? endDate, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, string rackIds = null, string productionTypes = null)
        {
            Logger.Trace($@"/GetOpenUnits?PlantCode={plantCode}&RequestedDate={maxDate}&StartDate={startDate}&EndDate={endDate}&CustomerNum={custNum}&CustomerSoldNum={customerSoldNum}&CustomerShipNum={customerShipNum}&RackType={rackType}&ScheduleType={scheduleType}&ScheduleNums={scheduleNums}&IGLine={igLine}&RackStatus={rackStatus}&UnitType={unitType}&BoxNum={boxNum}&Options={options}&OrderDate={orderDateCode}&HoursToTruck={hoursToTruck}");
            var processor = new ProductionProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var rackIdsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var customerSoldNumList = new List<decimal>();
                var customerShipNumList = new List<decimal>();
                var optionsList = new List<string>();
                var productionTypeList = new List<string>();
                if (!String.IsNullOrEmpty(scheduleNums))
                {
                    try
                    {
                        scheduleNumsList = Array.ConvertAll<string, decimal>(scheduleNums.Split(','), Convert.ToDecimal).ToList();
                    }
                    catch
                    {
                        //scheduleNumsList = null;
                    }
                }
                if (!String.IsNullOrEmpty(rackIds))
                {
                    try
                    {
                        rackIdsList = Array.ConvertAll<string, decimal>(rackIds.Split(','), Convert.ToDecimal).ToList();
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
                if (!String.IsNullOrEmpty(productionTypes))
                {
                    productionTypeList = productionTypes.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenUnitsLocal(plantCode, maxDate, startDate, endDate, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypeList);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public GlassRacks GetOpenRacks(string plantCode, string maxDate, DateTime? startDate, DateTime? endDate, decimal custNum = 0, string rackType = null, string scheduleType = null, string scheduleNums = null, string igLine = null, string rackStatus = null, string unitType = null, string options = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, decimal hoursToTruck = 0, string rackIds = null, string productionTypes = null)
        {
            var processor = new ProductionProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var rackIdsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var customerSoldNumList = new List<decimal>();
                var customerShipNumList = new List<decimal>();
                var productionTypeList = new List<string>();
                var optionsList = new List<string>();
                if (!String.IsNullOrEmpty(scheduleNums))
                {
                    try
                    {
                        scheduleNumsList = Array.ConvertAll<string, decimal>(scheduleNums.Split(','), Convert.ToDecimal).ToList();
                    }
                    catch
                    {
                        //scheduleNumsList = null;
                    }
                }
                if (!String.IsNullOrEmpty(rackIds))
                {
                    try
                    {
                        rackIdsList = Array.ConvertAll<string, decimal>(rackIds.Split(','), Convert.ToDecimal).ToList();
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
                if (!String.IsNullOrEmpty(productionTypes))
                {
                    productionTypeList = productionTypes.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenRacks(plantCode, maxDate, startDate, endDate, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypeList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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

        public RequestedDateFacts GetRequestedDateFacts(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate) //, bool notScheduledUnits)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetRequestedDateFacts(plantCode, dateCode, startDate, endDate); //, notScheduledUnits);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ScheduledDateFacts GetScheduledDateFacts(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetScheduledDateFacts(plantCode, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public List<ScheduleSummary> GetScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var processor = new ScheduleProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetScheduleSummary(plantCode, dateCode, startDate, endDate, dateProperty);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RacksByProductSet GetRacksByProduct(string plantCode, decimal customerNum)
        {
            var processor = new ProductionProcessor();
            try
            {
                return processor.GetRacksByProduct(plantCode, customerNum);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse FG_ShiftTonsPulled()
        {
            var processor = new ProductionProcessor();
            try
            {
                return processor.FG_ShiftTonsPulled();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class ProductionRestService : IProductionRestService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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

        public RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, string startDate, string endDate)
        {
            var processor = new ProductionProcessor();
            try
            {
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
                return processor.GetRackEventsByHour(plantCode, dateCode, startDateVal, endDateVal);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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

        public GlassUnits GetOpenUnits(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, string customerSoldNum, string customerShipNum, decimal boxNum, string orderDateCode, decimal hoursToTruck, string rackIds, string productionTypes)
        {
            var processor = new ProductionProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var rackIdsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var customerSoldNumList = new List<decimal>();
                var customerShipNumList = new List<decimal>();
                var optionsList = new List<string>();
                var productionTypeList = new List<string>();
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
                if (!String.IsNullOrEmpty(rackIds))
                {
                    try
                    {
                        rackIdsList =
                            Array.ConvertAll<string, decimal>(rackIds.Split(','), Convert.ToDecimal).ToList();
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
                if (!String.IsNullOrEmpty(productionTypes))
                {
                    productionTypeList = productionTypes.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenUnits(plantCode, maxDate, startDateVal, endDateVal, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypeList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ProdUnits GetOpenUnitsLocal(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, string customerSoldNum, string customerShipNum, decimal boxNum, string orderDateCode, decimal hoursToTruck, string rackIds, string productionTypes)
        {
            var processor = new ProductionProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var rackIdsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var customerSoldNumList = new List<decimal>();
                var customerShipNumList = new List<decimal>();
                var optionsList = new List<string>();
                var productionTypeList = new List<string>();
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
                if (!String.IsNullOrEmpty(rackIds))
                {
                    try
                    {
                        rackIdsList =
                            Array.ConvertAll<string, decimal>(rackIds.Split(','), Convert.ToDecimal).ToList();
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
                if (!String.IsNullOrEmpty(productionTypes))
                {
                    productionTypeList = productionTypes.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenUnitsLocal(plantCode, maxDate, startDateVal, endDateVal, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypeList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public GlassRacks GetOpenRacks(string plantCode, string maxDate, DateTime startDate, DateTime endDate, decimal custNum, string rackType, string scheduleType, string scheduleNums, string igLine, string rackStatus, string unitType, string options, string customerSoldNum, string customerShipNum, decimal boxNum, string orderDateCode, decimal hoursToTruck, string rackIds, string productionTypes)
        {
            var processor = new ProductionProcessor();
            try
            {
                var scheduleNumsList = new List<decimal>();
                var rackIdsList = new List<decimal>();
                var igLinesList = new List<int>();
                var rackStatusList = new List<string>();
                var unitTypeList = new List<string>();
                var customerSoldNumList = new List<decimal>();
                var customerShipNumList = new List<decimal>();
                var productionTypeList = new List<string>();
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
                if (!String.IsNullOrEmpty(rackIds))
                {
                    try
                    {
                        rackIdsList =
                            Array.ConvertAll<string, decimal>(rackIds.Split(','), Convert.ToDecimal).ToList();
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
                if (!String.IsNullOrEmpty(productionTypes))
                {
                    productionTypeList = productionTypes.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(options))
                {
                    optionsList = options.Split(',').ToList();
                }
                return processor.GetOpenRacks(plantCode, maxDate, startDateVal, endDateVal, custNum, rackType, scheduleType, scheduleNumsList, igLinesList, rackStatusList, unitTypeList, optionsList, customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypeList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
                throw new Exception(e.Message);
            }
        }

        public RequestedDateFacts GetRequestedDateFacts(string plantCode, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                if (!String.IsNullOrEmpty(dateCode))
                {
                    dateCode = dateCode.Replace("'", "");
                }
                return processor.GetRequestedDateFacts(plantCode, dateCode, startDate, endDate); //, notScheduledUnits);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ScheduledDateFacts GetScheduledDateFacts(string plantCode, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                if (!String.IsNullOrEmpty(dateCode))
                {
                    dateCode = dateCode.Replace("'", "");
                }
                return processor.GetScheduledDateFacts(plantCode, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<ScheduleSummary> GetScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var processor = new ScheduleProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetScheduleSummary(plantCode, dateCode, startDate, endDate, dateProperty);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<ScheduleSummary> GetSfScheduleSummary(string plantCode, string dateCode, string startDate, string endDate, string dateProperty)
        {
            var processor = new ScheduleProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetSfScheduleSummary(plantCode, dateCode, startDate, endDate, dateProperty);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public RacksByProductSet GetRacksByProduct(string plantCode, decimal customerNum)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetRacksByProduct(plantCode, customerNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public GlassUnits GetHoleDrillingRacks(string plantCode)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetHoleDrillingRacks(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ProdLites GetSalvagedGlass(string plantCode, string dateCode, string startDate, string endDate)
        {
            var processor = new ProductionProcessor();
            try
            {
                plantCode = plantCode.Replace("'", "");
                return processor.GetSalvagedGlass(plantCode, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ProcessResponse FG_ShiftTonsPulled()
        {
            var processor = new ProductionProcessor();
            try
            {
                return processor.FG_ShiftTonsPulled();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
