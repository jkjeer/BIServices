using System;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.InventorySvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class InventoryService : IInventoryService
    {
        public InterlayerLots GetInterlayerLots(string plantCode)
        {
            var processor = new InventoryProcessor();
            try
            {
                return processor.GetInterlayerLots(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class InventoryRestService : IInventoryRestService
    {
        public InterlayerLots GetInterlayerLots(string plantCode)
        {
            var processor = new InventoryProcessor();
            try
            {
                return processor.GetInterlayerLots(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

}
