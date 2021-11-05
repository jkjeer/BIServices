using System;
using BIServices.Common.Types.Enterprise;
using BIServices.ShippingSvc;
using SoaCore.Wcf;

namespace BIServices.ShippingSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class ShippingService : IShippingService
    {
        public Racks GetShippedRacks(string plantCode, decimal customerNum, string dateCode, string startDate, string endDate)
        {
            plantCode = plantCode.Replace("amp;", "");
            dateCode = dateCode?.Replace("amp;", "");
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            try
            {
                return ShippingProcessor.GetShippedRacks(plantCode, customerNum, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class ShippingRestService : IShippingRestService
    {
        public Racks GetShippedRacks(string plantCode, decimal customerNum, string dateCode, string startDate, string endDate)
        {
            plantCode = plantCode.Replace("amp;", "");
            dateCode = dateCode?.Replace("amp;", "");
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            try
            {
                return ShippingProcessor.GetShippedRacks(plantCode, customerNum, dateCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

}
