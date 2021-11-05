using System;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.RackSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class RackService : IRackService
    {
        public Racks GetStockRacksOnHand(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetStockRacksOnHand(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Racks GetHVMTORacksOnHand(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetHVMTORacksOnHand(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Racks GetHVRacksOnHand(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetHVRacksOnHand(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Racks GetPurchasedConfiguredRacks(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetPurchasedConfiguredRacks(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class RackRestService : IRackRestService
    {
        public Racks GetStockRacksOnHand(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetStockRacksOnHand(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Racks GetHVMTORacksOnHand(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetHVMTORacksOnHand(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Racks GetHVRacksOnHand(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetHVRacksOnHand(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Racks GetPurchasedConfiguredRacks(string plantCode, int agingDays)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetPurchasedConfiguredRacks(plantCode, agingDays);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public RemakeUnits GetPurchasedRemakes(string plantCode)
        {
            var processor = new RackProcessor();
            try
            {
                return processor.GetPurchasedRemakes(plantCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

}
