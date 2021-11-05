using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types;
using BIServices.Common.Types.E1;
using NLog;

namespace BIServices.QuerySvc
{
    public class QueryProcessor
    {
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

        public RackEventHourSummarySet GetRackEventsByHourOLD(string plantCode, string dateCode, DateTime? startDate, DateTime? endDate)
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

                if (plant.PlantCode == "OCLG")
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
                //var plantAttribute = session.PlantMap.GetAttributebyId(plantCode);
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

        public OpenUnits GetOpenUnits(string plantCode, string maxDateCode, DateTime? startDate, DateTime? endDate,
            decimal custNum, string rackType, string scheduleType, List<decimal> scheduleNums, List<int> igLinesList,
            List<string> rackStatus, List<string> unitTypeList, List<string> optionsList, decimal customerSoldNum, decimal customerShipNum)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn1 = new SqlConnection();
            var cn2 = new SqlConnection();
            var cn3 = new SqlConnection();
            try
            {
                plantCode = plantCode.Replace("'", "");
                var plant = session.PlantMap.GetAttributebyId(plantCode);
                cn1 = ConnectionManager.GetSqlConnectionFromKey(plant.Code + ".ShopFloor");
                cn2 = ConnectionManager.GetSqlConnectionFromKey(plant.Code + ".ShopFloor");
                cn3 = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn1.Open();
                cn2.Open();
                cn3.Open();
                var mgr = new OpenUnitManager(session);
                return mgr.GetOpenUnits(session, cn1, cn2, cn3, plant, maxDateCode, startDate, endDate, custNum,
                    rackType, scheduleType, scheduleNums, igLinesList, rackStatus, unitTypeList, optionsList, customerSoldNum, customerShipNum);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cn1.Close();
                cn2.Close();
                cn3.Close();
            }
        }

        public EdiRecords GetAndersenEdi(int startDaysBack, int endDaysBack)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cn = new SqlConnection();
            try
            {
                var mgr = new AndersenEdiManager(session);
                return mgr.GetEdiRecords(session, DateTime.Now.Date.AddDays(startDaysBack),
                    DateTime.Now.Date.AddDays(endDaysBack));
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
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
            return loads;
        }
    }
}