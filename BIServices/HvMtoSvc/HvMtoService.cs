using System;
using System.Data;
using BIServices.Common.Types.ShopFloor;
using BIServices.ProductionSvc;
using BIServices.QuerySvc;
using SoaCore.Wcf;

namespace BIServices.HvMtoSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class HvMtoService : IHvMtoService
    {
        public HvMtoOrders GetOpenOrders(string plantCode, bool isHvMto, decimal scheduleNum)
        {
            var processor = new HvMtoProcessor();
            try
            {
                return processor.GetOpenOrders(plantCode, isHvMto, scheduleNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public PurchasedGlassItems GetPurchasedGlassSummary(string plantCode, bool isHvMto, decimal scheduleNum)
        {
            var processor = new HvMtoProcessor();
            try
            {
                return processor.GetPurchasedGlassSummary(plantCode, isHvMto, scheduleNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class HvMtoRestService : IHvMtoRestService
    {
        public HvMtoOrders GetOpenOrders(string plantCode, bool isHvMto, decimal scheduleNum)
        {
            var processor = new HvMtoProcessor();
            try
            {
                return processor.GetOpenOrders(plantCode, isHvMto, scheduleNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public PurchasedGlassItems GetPurchasedGlassSummary(string plantCode, bool isHvMto, decimal scheduleNum)
        {
            var processor = new HvMtoProcessor();
            try
            {
                return processor.GetPurchasedGlassSummary(plantCode, isHvMto, scheduleNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }


}
