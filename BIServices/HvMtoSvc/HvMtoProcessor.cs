using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.ShopFloor;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.ShopFloor;
using BIServices.HvMtoSvc;
using BIServices.QuerySvc;

namespace BIServices.ProductionSvc
{
    public class HvMtoProcessor
    {
        public HvMtoOrders GetOpenOrders(string plantCode, bool isHvMto, decimal scheduleNum)
        {
            var session = new BusinessSystemSession();
            var cn = new SqlConnection();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                var mgr = new HvMtoManager(session);
                return mgr.GetOpenOrders(session, cn, plant, isHvMto, scheduleNum);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
        }

        public PurchasedGlassItems GetPurchasedGlassSummary(string plantCode, bool isHvMto, decimal scheduleNum)
        {
            var session = new BusinessSystemSession();
            var cn = new SqlConnection();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);

                var mgr = new HvMtoManager(session);
                var orders = mgr.GetOpenOrders(session, cn, plant, isHvMto, scheduleNum);
                return mgr.GetPurchasedGlassSummary(session, orders);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
        }
    }
}