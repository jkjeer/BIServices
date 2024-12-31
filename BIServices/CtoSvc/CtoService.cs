using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Types.ShopFloor;
using BIServices.Common.Utilities;
using BIServices.QuerySvc;
using SoaCore.Wcf;

namespace BIServices.CtoSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class CtoService : ICtoService
    {
        public ConsumedSheets GetConsumedSheets(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetConsumedSummary(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProducedLites GetProducedLites(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetProducedLites(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RejectedLites GetRejectedLites(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetRejectedLites(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ShiftAggregates GetShiftSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetShiftSummary(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public CutAggregates GetCutSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetCutSummary(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public TemperAggregates GetTemperSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetTemperSummary(plants, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RawItems GetRawSupply(string plant, string itemNum)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetRawSupply(plant, itemNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class CtoRestService : ICtoRestService
    {
        public ConsumedSheets GetConsumedSheets(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetConsumedSummary(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProducedLites GetProducedLites(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetProducedLites(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RejectedLites GetRejectedLites(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetRejectedLites(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ShiftAggregates GetShiftSummary(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetShiftSummary(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public CutAggregates GetCutSummary(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetCutSummary(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public CutAggregates GetCutSummaryByDate(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetCutSummaryByDate(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public TemperAggregates GetTemperSummary(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetTemperSummary(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public TemperAggregates GetTemperSummaryByDate(string plants, string dateCode, string startDate, string endDate)
        {
            var processor = new CtoProcessor();
            dateCode = (string.IsNullOrEmpty(dateCode)) ? "" : dateCode.Replace("'", "");
            try
            {
                return processor.GetTemperSummaryByDate(plants, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public RawItems GetRawSupply(string plant, string itemNum)
        {
            var processor = new CtoProcessor();
            try
            {
                return processor.GetRawSupply(plant, itemNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
