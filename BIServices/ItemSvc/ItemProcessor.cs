using System;
using System.Linq;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using NLog;

namespace BIServices.ItemSvc
{
    public class ItemProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public DiscreteItems GetSetup(string plantCode)
        {
            var items = new DiscreteItems();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantList = plantCode.Replace("'", "").Split(',').ToList();
                var mgr = new DiscreteItemManager(session);
                items = mgr.GetSetup(session, plantList);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return items;
        }
    }
}