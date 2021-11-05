using System;
using BIServices.Common.Types.ShopFloor;
using BIServices.Common.Utilities;
using BIServices.CtoSvc;
using SoaCore.Wcf;

namespace BIServices.CgHvSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class CgHvService : ICgHvService
    {
        public ConsumedSheets GetConsumedSheets(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CgHvProcessor();
            try
            {
                return processor.GetConsumedSummary(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ProducedLites GetProducedLites(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CgHvProcessor();
            try
            {
                return processor.GetProducedLites(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public RejectedLites GetRejectedLites(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CgHvProcessor();
            try
            {
                return processor.GetRejectedLites(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public CoaterAggregates GetCoaterSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CgHvProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetCoaterSummary(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class CgHvRestService : ICgHvRestService
    {
        public ConsumedSheets GetConsumedSheets(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CgHvProcessor();
            try
            {
                return processor.GetConsumedSummary(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ProducedLites GetProducedLites(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CgHvProcessor();
            try
            {
                return processor.GetProducedLites(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public RejectedLites GetRejectedLites(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CgHvProcessor();
            try
            {
                return processor.GetRejectedLites(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public CoaterAggregates GetCoaterSummary(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CgHvProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetCoaterSummary(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
