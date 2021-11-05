using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.ShopFloor;
using BIServices.Common.Types.Response;
using BIServices.Common.Types.ShopFloor;
using BIServices.CtoSvc;
using NLog;

namespace BIServices.CgHvSvc
{
    public class CgHvProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ConsumedSheets GetConsumedSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var sheets = new ConsumedSheets();
            var cn = new SqlConnection();
            try
            {
                var mgr = new CtoSummaryManager();
                var plantList = plants.Replace("'", "").Split(',').ToList();
                foreach (var plantItem in plantList)
                {
                    var plant = PlantManager.GetPlant(plantItem);
                    cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                    cn.Open();
                    sheets.AddRange(mgr.GetConsumedSheets(cn, plant, dateCode, startDate, endDate));
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                cn.Close();
            }
            return sheets;
        }

        public ProducedLites GetProducedLites(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new ProducedLites();
            var cn = new SqlConnection();
            try
            {
                var mgr = new CtoSummaryManager();
                var plantList = plants.Replace("'", "").Split(',').ToList();
                foreach (var plantItem in plantList)
                {
                    var plant = PlantManager.GetPlant(plantItem);
                    cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                    cn.Open();
                    lites.AddRange(mgr.GetProducedLites(cn, plant, dateCode, startDate, endDate));

                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                cn.Close();
            }
            return lites;
        }

        public RejectedLites GetRejectedLites(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var lites = new RejectedLites();
            try
            {
                var mgr = new CtoSummaryManager();
                var plantList = plants.Replace("'", "").Split(',').ToList();
                foreach (var plantItem in plantList)
                {
                    var plant = PlantManager.GetPlant(plantItem);
                    var cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                    cn.Open();
                    lites.AddRange(mgr.GetRejectedLites(cn, plant, dateCode, startDate, endDate));
                    cn.Close();
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
            }
            return lites;
        }

        public CoaterAggregates GetCoaterSummary2(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new CoaterAggregates();
            var plantList = plants.Replace("'", "").Split(',').ToList();
            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantList, (plantItem) =>
                {
                    var mgr = new HvCoatSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn.Open();
                        aggs.AddRange(mgr.GetCoaterSummary(plant, cn, dateCode, startDate, endDate));

                    }
                    catch (Exception e)
                    {
                        var ex = new Exception($"Plant: {plantItem} Message: {e.Message}");
                        exceptions.Enqueue(ex);
                    }
                    finally
                    {
                        cn.Close();
                    }
                });
                if (exceptions.Count > 0) throw new AggregateException(exceptions);
            }
            catch (AggregateException es)
            {
                foreach (var e in es.InnerExceptions)
                {
                    var ez = new Error(e);
                }
            }
            return aggs;
        }

        public CoaterAggregates GetCoaterSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new CoaterAggregates();
            var aggsBag = new ConcurrentBag<CoaterAggregate>();
            var plantList = plants.Replace("'", "").Split(',').ToList();
            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantList, (plantItem) =>
                {
                    var mgr = new HvCoatSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cn.Open();
                        var results = mgr.GetCoaterSummary(plant, cn, dateCode, startDate, endDate);
                        results.AsParallel().ForAll(t => aggsBag.Add(t));
                    }
                    catch (Exception e)
                    {
                        var ex = new Exception($"Plant: {plantItem} Message: {e.Message}");
                        exceptions.Enqueue(ex);
                    }
                    finally
                    {
                        cn.Close();
                    }
                });
                if (exceptions.Count > 0) throw new AggregateException(exceptions);
            }
            catch (AggregateException es)
            {
                foreach (var e in es.InnerExceptions)
                {
                    var ez = new Error(e);
                }
            }
            aggs.AddRange(aggsBag.ToList());
            return aggs;
        }
    }

}
