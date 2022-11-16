using System;
using System.Data.SqlClient;
using System.Linq;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using NLog;

namespace BIServices.RackSvc
{
    public class RackProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public Racks GetStockRacksOnHand(string plantCode, int agingDays, string status)
        {
            var racks = new Racks();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantList = plantCode.Replace("'", "").Split(',').ToList();
                var statusList = status.Replace("'", "").Split(',').ToList();
                var mgr = new StockRackManager(session);
                racks = mgr.GetStockRacks(session, plantList, agingDays, statusList);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return racks;
        }

        public Racks GetHVMTORacksOnHand(string plantCode, int agingDays, string status)
        {
            var racks = new Racks();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantList = plantCode.Replace("'", "").Split(',').ToList();
                var statusList = status.Replace("'", "").Split(',').ToList();
                var mgr = new ConfiguredRackManager(session);
                //racks = mgr.GetHVManufacturedConfiguredRacks(session, plantList, agingDays);
                racks = mgr.GetHVMTORacks(session, plantList, agingDays, statusList);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return racks;
        }

        public Racks GetHVRacksOnHand(string plantCode, int agingDays, string status)
        {
            var racks = new Racks();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantList = plantCode.Replace("'", "").Split(',').ToList();
                var statusList = status.Replace("'", "").Split(',').ToList();
                var mgr1 = new StockRackManager(session);
                racks.AddRange(mgr1.GetStockRacks(session, plantList, agingDays, statusList));
                var mgr2 = new ConfiguredRackManager(session);
                racks.AddRange(mgr2.GetHVMTORacks(session, plantList, agingDays, statusList));
                //racks.AddRange(mgr2.GetHVManufacturedConfiguredRacks(session, plantList, agingDays));
                //racks.AddRange(mgr2.GetHVPurchasedConfiguredRacks(session, plantList, agingDays));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return racks;
        }

        //public Racks GetHVRacksOnHandVer1(string plantCode, int agingDays)
        //{
        //    var racks = new Racks();
        //    var session = new BusinessSystemSession();
        //    try
        //    {
        //        session = BusinessSystemSessionManager.GetBusinessSession();
        //        var plantList = plantCode.Replace("'", "").Split(',').ToList();
        //        var mgr1 = new StockRackManager(session);
        //        racks.AddRange(mgr1.GetStockRacksVer1(session, plantList, agingDays));
        //        var mgr2 = new ConfiguredRackManager(session);
        //        racks.AddRange(mgr2.GetHVManufacturedConfiguredRacks(session, plantList, agingDays));
        //        racks.AddRange(mgr2.GetHVPurchasedConfiguredRacks(session, plantList, agingDays));
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.ToString());
        //    }
        //    finally
        //    {
        //        session.Close();
        //    }
        //    return racks;
        //}

        public Racks GetPurchasedConfiguredRacks(string plantCode, int agingDays)
        {
            var racks = new Racks();
            var session = new BusinessSystemSession();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantList = plantCode.Replace("'", "").Split(',').ToList();
                var mgr = new ConfiguredRackManager(session);
                racks.AddRange(mgr.GetHVPurchasedConfiguredRacks(session, plantList, agingDays));
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
            }
            return racks;
        }

        public RemakeUnits GetPurchasedRemakes(string plantCode)
        {
            var units = new RemakeUnits();
            var session = new BusinessSystemSession();
            var purchaserConnection = new SqlConnection();
            var supplierConnection = new SqlConnection();
            try
            {
                session = BusinessSystemSessionManager.GetBusinessSession();

                var plantString = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantString);

                purchaserConnection = ConnectionManager.GetSqlConnectionFromKey($"{plant.PlantCode}.SHOPFLOOR");
                purchaserConnection.Open();
                supplierConnection = ConnectionManager.GetSqlConnectionFromKey($"OCLG.CommandCenter");
                supplierConnection.Open();

                var mgr = new RemakeUnitsManager(session);
                units = mgr.GetPurchasedRemakes(session, purchaserConnection, supplierConnection, plant);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                purchaserConnection.Close();
                supplierConnection.Close();
                session.Close();
            }
            return units;
        }
    }
}