using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.ShopFloor;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using BIServices.QuerySvc;
using NLog;

// ReSharper disable InconsistentNaming

namespace BIServices.ProductionSvc
{
    public class ProductionProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProcessResponse FG_ShiftTonsPulled()
        {
            var response = new ProcessResponse("FG_ShiftTonsPulled");
            try
            {
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
            }
            response.Close();
            return response;
        }

        public ClosedRacks GetClosedRacks(string plantCode)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn = new SqlConnection();
            try
            {
                plantCode = plantCode.Replace("'", "");
                //var plantAttribute = session.PlantMap.GetAttributebyId(plantCode);
                var plant = PlantManager.GetPlant(plantCode);
                cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                cn.Open();
                var mgr = new ClosedRackManager(session);
                return mgr.GetClosedRacks(session, cn, plant);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
        }

        public RackEventHourSummarySet GetRackEventsByHourOLD(string plantCode, string dateCode, DateTime startDate, DateTime endDate)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn = new SqlConnection();
            dateCode = dateCode.Replace(",", "").Replace("'", "");
            plantCode = plantCode.Replace("'", "");
            try
            {
                //var plantAttribute = session.PlantMap.GetAttributebyId(plantCode);
                var plant = PlantManager.GetPlant(plantCode);
                cn = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn.Open();
                var mgr = new RackEventManager(session);
                return mgr.GetRackEventsByHour(session, cn, plant, dateCode, startDate, endDate);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
        }

        public RackEventHourSummarySet GetRackEventsByHour(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            dateCode = dateCode.Replace(",", "").Replace("'", "");
            plantCode = plantCode.Replace("'", "");
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn = new SqlConnection();
            try
            {
                var plant = PlantManager.GetPlant(plantCode);

                if (plant.PlantCode.EndsWith("LG"))
                {
                    cn = ConnectionManager.GetSqlConnectionFromKey($"{plant.PlantCode}.SHOPFLOOR");
                    cn.Open();
                    var mgr = new RackEventManagerLG(session);
                    return mgr.GetRackEventsByHour(cn, plant, dateCode, startDate, endDate);
                }
                else
                {
                    cn = ConnectionManager.GetSqlConnectionFromKey("DW");
                    cn.Open();
                    var mgr = new RackEventManager(session);
                    return mgr.GetRackEventsByHour(session, cn, plant, dateCode, startDate, endDate);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
        }

        public RacksByLaneSet GetRacksByLane(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn = new SqlConnection();
            try
            {
                var plant = PlantManager.GetPlant(plantCode);
                cn = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn.Open();
                var mgr = new RacksByLaneManager(session);
                return mgr.GetRacksByLane(session, cn, plant, dateCode, startDate, endDate);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
        }

        public GlassUnits GetOpenUnits(string plantCode,
            string maxDateCode,
            DateTime? startDate,
            DateTime? endDate,
            decimal custNum,
            string rackType,
            string scheduleType,
            List<decimal> scheduleNums,
            List<int> igLinesList,
            List<string> rackStatus,
            List<string> unitTypeList,
            List<string> optionsList,
            string customerSoldNum,
            string customerShipNum,
            decimal boxNum,
            string orderDateCode,
            decimal hoursToTruck,
            List<decimal> rackIdsList,
            List<string> productionTypeList            )
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn1 = new SqlConnection();
            var cn2 = new SqlConnection();
            var cn3 = new SqlConnection();
            var glassUnits = new GlassUnits();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plants = PlantManager.GetPlants(plantCode);
                cn3 = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn3.Open();

                foreach (Plant plant in plants) {
                    if (plant.PlantCode.EndsWith("LG"))
                    {
                        try
                        {
                            cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                            cn1.Open();

                            var mgr = new Common.Managers.ShopFloor.OpenUnitManagerLG(session);
                            glassUnits.AddRange(mgr.GetOpenUnits(session,
                                cn1,
                                cn3,
                                plant,
                                maxDateCode,
                                startDate,
                                endDate,
                                custNum,
                                rackType,
                                scheduleType,
                                scheduleNums,
                                igLinesList,
                                rackStatus,
                                unitTypeList,
                                optionsList,
                                customerSoldNum,
                                customerShipNum,
                                boxNum,
                                orderDateCode,
                                hoursToTruck,
                                rackIdsList,
                                productionTypeList));
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.ToString());
                        }
                        finally
                        {
                            cn1.Close();
                        }
                    } else if (plant.PlantCode.EndsWith("IG"))
                    {
                        cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn1.Open();
                        cn2.Open();

                        var mgr = new Common.Managers.ShopFloor.OpenUnitManagerIG(session);
                        glassUnits.AddRange(mgr.GetOpenUnitsGlassUnit(session,
                            cn1,
                            cn2,
                            cn3,
                            plant,
                            maxDateCode,
                            startDate,
                            endDate,
                            custNum,
                            rackType,
                            scheduleType,
                            scheduleNums,
                            igLinesList,
                            rackStatus,
                            unitTypeList,
                            optionsList,
                            customerSoldNum,
                            customerShipNum,
                            boxNum,
                            orderDateCode,
                            hoursToTruck,
                            rackIdsList));

                        //var mgr = new Common.Managers.E1.OpenUnitManagerIG(session);
                        //glassUnits.AddRange(mgr.GetOpenUnits(session,
                        //    cn1,
                        //    cn2,
                        //    cn3,
                        //    plant,
                        //    maxDateCode,
                        //    startDate,
                        //    endDate,
                        //    custNum,
                        //    rackType,
                        //    scheduleType,
                        //    scheduleNums,
                        //    igLinesList,
                        //    rackStatus,
                        //    unitTypeList,
                        //    optionsList,
                        //    customerSoldNum,
                        //    customerShipNum,
                        //    boxNum,
                        //    orderDateCode,
                        //    hoursToTruck,
                        //    rackIdsList));
                    }
                    else
                    {
                        try
                        {
                            cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                            cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                            cn1.Open();
                            cn2.Open();
                            var mgr = new Common.Managers.E1.OpenUnitManager(session);
                            glassUnits.AddRange(mgr.GetOpenUnits(session,
                                cn1,
                                cn2,
                                cn3,
                                plant,
                                maxDateCode,
                                startDate,
                                endDate,
                                custNum,
                                rackType,
                                scheduleType,
                                scheduleNums,
                                igLinesList,
                                rackStatus,
                                unitTypeList,
                                optionsList,
                                customerSoldNum,
                                customerShipNum,
                                boxNum,
                                orderDateCode,
                                hoursToTruck,
                                rackIdsList));
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                        finally
                        {
                            cn1.Close();
                            cn2.Close();
                        }
                    }
                }
                return glassUnits;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cn3.Close();
            }
        }

        public ProdUnits GetOpenUnitsLocal(string plantCode,
            string maxDateCode,
            DateTime? startDate,
            DateTime? endDate,
            decimal custNum,
            string rackType,
            string scheduleType,
            List<decimal> scheduleNums,
            List<int> igLinesList,
            List<string> rackStatus,
            List<string> unitTypeList,
            List<string> optionsList,
            string customerSoldNum,
            string customerShipNum,
            decimal boxNum,
            string orderDateCode,
            decimal hoursToTruck,
            List<decimal> rackIdsList,
            List<string> productionTypeList)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn1 = new SqlConnection();
            var cn2 = new SqlConnection();
            var cn3 = new SqlConnection();
            var prodUnits = new ProdUnits();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plants = PlantManager.GetPlants(plantCode);
                cn3 = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn3.Open();

                foreach (Plant plant in plants)
                {
                    if (plant.PlantCode.EndsWith("LG"))
                    {
                        //try
                        //{
                        //    cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        //    cn1.Open();

                        //    var mgr = new Common.Managers.ShopFloor.OpenUnitManagerLG(session);
                        //    prodUnits.AddRange(mgr.GetOpenUnits(session,
                        //        cn1,
                        //        cn3,
                        //        plant,
                        //        maxDateCode,
                        //        startDate,
                        //        endDate,
                        //        custNum,
                        //        rackType,
                        //        scheduleType,
                        //        scheduleNums,
                        //        igLinesList,
                        //        rackStatus,
                        //        unitTypeList,
                        //        optionsList,
                        //        customerSoldNum,
                        //        customerShipNum,
                        //        boxNum,
                        //        orderDateCode,
                        //        hoursToTruck,
                        //        rackIdsList,
                        //        productionTypeList));
                        //}
                        //catch (Exception e)
                        //{
                        //    throw new Exception(e.ToString());
                        //}
                        //finally
                        //{
                        //    cn1.Close();
                        //}
                    }
                    else if (plant.PlantCode.EndsWith("IG"))
                    {
                        cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn1.Open();
                        cn2.Open();

                        var mgr = new Common.Managers.ShopFloor.OpenUnitManagerIG(session);
                        prodUnits.AddRange(mgr.GetOpenUnits(session,
                            cn1,
                            cn3,
                            plant,
                            maxDateCode,
                            startDate,
                            endDate,
                            custNum,
                            rackType,
                            scheduleType,
                            scheduleNums,
                            igLinesList,
                            rackStatus,
                            unitTypeList,
                            optionsList,
                            customerSoldNum,
                            customerShipNum,
                            boxNum,
                            orderDateCode,
                            hoursToTruck,
                            rackIdsList,
                            productionTypeList));
                    }
                    else
                    {
                        //try
                        //{
                        //    cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        //    cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        //    cn1.Open();
                        //    cn2.Open();
                        //    var mgr = new Common.Managers.E1.OpenUnitManager(session);
                        //    glassUnits.AddRange(mgr.GetOpenUnits(session,
                        //        cn1,
                        //        cn2,
                        //        cn3,
                        //        plant,
                        //        maxDateCode,
                        //        startDate,
                        //        endDate,
                        //        custNum,
                        //        rackType,
                        //        scheduleType,
                        //        scheduleNums,
                        //        igLinesList,
                        //        rackStatus,
                        //        unitTypeList,
                        //        optionsList,
                        //        customerSoldNum,
                        //        customerShipNum,
                        //        boxNum,
                        //        orderDateCode,
                        //        hoursToTruck,
                        //        rackIdsList));
                        //}
                        //catch (Exception e)
                        //{
                        //    throw new Exception(e.Message);
                        //}
                        //finally
                        //{
                        //    cn1.Close();
                        //    cn2.Close();
                        //}
                    }
                }
                return prodUnits;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cn3.Close();
            }
        }


        public GlassRacks GetOpenRacks(string plantCode,
             string maxDateCode,
             DateTime? startDate,
             DateTime? endDate,
             decimal custNum,
             string rackType,
             string scheduleType,
             List<decimal> scheduleNums,
             List<int> igLinesList,
             List<string> rackStatus,
             List<string> unitTypeList,
             List<string> optionsList,
             string customerSoldNum,
             string customerShipNum,
             decimal boxNum,
             string orderDateCode,
             decimal hoursToTruck,
             List<decimal> rackIdsList,
             List<string> productionTypesList)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn1 = new SqlConnection();
            var cn2 = new SqlConnection();
            var cn3 = new SqlConnection();
            var glassRacks = new GlassRacks();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plants = PlantManager.GetPlants(plantCode);
                cn3 = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn3.Open();

                foreach (Plant plant in plants)
                {
                    if (plant.PlantCode.EndsWith("LG"))
                    {
                        try
                        {
                            cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                            cn1.Open();

                            var mgr = new Common.Managers.ShopFloor.OpenRackManagerLG(session);
                            glassRacks.AddRange(mgr.GetOpenRacks(session,
                                cn1,
                                cn3,
                                plant,
                                maxDateCode,
                                startDate,
                                endDate,
                                custNum,
                                rackType,
                                scheduleType,
                                scheduleNums,
                                igLinesList,
                                rackStatus,
                                unitTypeList,
                                optionsList,
                                customerSoldNum,
                                customerShipNum,
                                boxNum,
                                orderDateCode,
                                hoursToTruck,
                                rackIdsList,
                                productionTypesList));
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.ToString());
                        }
                        finally
                        {
                            cn1.Close();
                        }
                    }
                    else
                    {
                        try
                        {
                            cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                            cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                            cn1.Open();
                            cn2.Open();
                            var mgr = new Common.Managers.E1.OpenRackManager(session);
                            glassRacks.AddRange(mgr.GetOpenRacks(session,
                                cn1,
                                cn2,
                                cn3,
                                plant,
                                maxDateCode,
                                startDate,
                                endDate,
                                custNum,
                                rackType,
                                scheduleType,
                                scheduleNums,
                                igLinesList,
                                rackStatus,
                                unitTypeList,
                                optionsList,
                                customerSoldNum,
                                customerShipNum,
                                boxNum,
                                orderDateCode,
                                hoursToTruck,
                                rackIdsList,
                                productionTypesList));
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                        finally
                        {
                            cn1.Close();
                            cn2.Close();
                        }
                    }
                }
                return glassRacks;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cn3.Close();
            }
        }

        public GlassRacks GetOpenRacksOLD(string plantCode,
            string maxDateCode,
            DateTime? startDate,
            DateTime? endDate,
            decimal custNum,
            string rackType,
            string scheduleType,
            List<decimal> scheduleNums,
            List<int> igLinesList,
            List<string> rackStatus,
            List<string> unitTypeList,
            List<string> optionsList,
            string customerSoldNum,
            string customerShipNum,
            decimal boxNum,
            string orderDateCode,
            decimal hoursToTruck,
            List<decimal> rackIdsList,
            List<string> productionTypesList)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn1 = new SqlConnection();
            var cn2 = new SqlConnection();
            var cn3 = new SqlConnection();
            var racks = new GlassRacks();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plants = PlantManager.GetPlants(plantCode);
                cn3 = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn3.Open();

                foreach (var plant in plants)
                {
                    try
                    {
                        cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn1.Open();
                        cn2.Open();

                        var mgr = new Common.Managers.Enterprise.OpenRackManager(session);
                        racks.AddRange(mgr.GetOpenRacks(session,
                            cn1,
                            cn2,
                            cn3,
                            plant,
                            maxDateCode,
                            startDate,
                            endDate,
                            custNum,
                            rackType,
                            scheduleType,
                            scheduleNums,
                            igLinesList,
                            rackStatus,
                            unitTypeList,
                            optionsList,
                            customerSoldNum,
                            customerShipNum,
                            boxNum,
                            orderDateCode,
                            hoursToTruck,
                            rackIdsList,
                            productionTypesList));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());
                    }
                    finally
                    {
                        cn1.Close();
                    }
                }
                return racks;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cn3.Close();
            }
        }

        public IntercompanyLoads GetIntercompanyLoads(string plantCode)
        {
            var loads = new IntercompanyLoads();
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn = new SqlConnection();
            try
            {
                var plant = PlantManager.GetPlant(plantCode);
                var mgr = new IntercompanyLoadManager(session);
                loads = mgr.GetLoads(session, plant);
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
            return loads;
        }

        public RequestedDateFacts GetRequestedDateFacts(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate) //, bool notScheduledUnits)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cnDt = new SqlConnection();
            try
            {
                cnDt = ConnectionManager.GetSqlConnectionFromKey("DW");
                cnDt.Open();
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                var mgr = new ProductionSummaryManager(session);
                return mgr.GetRequestedDateFacts(session, cnDt, plant, dateCode, startDate, endDate); //, notScheduledUnits);
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cnDt.Close();
            }
        }

        public ScheduledDateFacts GetScheduledDateFacts(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cnDt = new SqlConnection();
            try
            {
                cnDt = ConnectionManager.GetSqlConnectionFromKey("DW");
                cnDt.Open();

                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                var mgr = new ProductionSummaryManager(session);
                return mgr.GetScheduledDateFacts(session, cnDt, plant, dateCode, startDate, endDate);
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
                cnDt.Close();
            }
        }

        public RacksByProductSet GetRacksByProduct(string plantCode, decimal customerNum)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cnDt = new SqlConnection();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                var mgr = new ProductionSummaryManager(session);
                return mgr.GetRacksByProduct(session, plant, customerNum);
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
            }
        }

        public GlassUnits GetHoleDrillingRacks(string plantCode)
        {
            var cn = new SqlConnection();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);

                cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".SHOPFLOOR");
                cn.Open();

                return HoleDrillingRackManager.GetHoleDrillingRacks(cn, plant);
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                cn.Close();
            }
        }

        public ProdLites GetSalvagedGlass(string plantCode, string dateCode, string startDate, string endDate)
        {
            var cnSf = new SqlConnection();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plant = PlantManager.GetPlant(plantCode);
                cnSf = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".SHOPFLOOR");
                cnSf.Open();
                return SalvagedGlassManager.GetLites(cnSf, plant, dateCode, Data.ParseStringToDateTime(startDate), Data.ParseStringToDateTime(endDate));
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                cnSf.Close();
            }
        }

    }
}