using System;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.WarrantySvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class WarrantyService : IWarrantyService
    {
        public ProducedUnits GetLvUnit(string plant, decimal unitId, DateTime startDate, DateTime endDate)
        {
            var processor = new WarrantyProcessor();
            try
            {
                return processor.GetLvUnit(plant, unitId, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class WarrantyRestService : IWarrantyRestService
    {
        public ProducedUnits GetLvUnit(string plant, decimal unitId, DateTime startDate, DateTime endDate)
        {
            var processor = new WarrantyProcessor();
            try
            {
                return processor.GetLvUnit(plant, unitId, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

    }


}
