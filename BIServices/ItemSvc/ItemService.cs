using System;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ItemSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class ItemService : IItemService
    {
        public DiscreteItems GetSetup(string plantCode)
        {
            var processor = new ItemProcessor();
            try
            {
                return processor.GetSetup(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class ItemRestService : IItemRestService
    {
        public DiscreteItems GetSetup(string plantCode)
        {
            var processor = new ItemProcessor();
            try
            {
                return processor.GetSetup(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

}
