using System;
using System.Linq;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using NLog;

namespace BIServices.InventorySvc
{
    public class InventoryProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public InterlayerLots GetInterlayerLots(string plantCode)
        {
            var lots = new InterlayerLots();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantList = plantCode.Replace("'", "").Split(',').ToList();
                var mgr = new InterlayerLotManager(session);
                lots = mgr.GetLots(session, plantList);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return lots;
        }
    }
}