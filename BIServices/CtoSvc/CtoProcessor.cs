using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.ShopFloor;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Response;
using BIServices.Common.Types.ShopFloor;
using BIServices.HvMtoSvc;
using BIServices.QuerySvc;
using NLog;

namespace BIServices.CtoSvc
{
    public class CtoProcessor
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
                    cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
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
                    cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
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
                    var cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
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


        public ShiftAggregates GetShiftSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new ShiftAggregates();
            var aggsBag = new ConcurrentBag<ShiftAggregate>();
            var plantList = plants.Replace("'", "").Split(',').ToList();
            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantList, (plantItem) =>
                {
                    var mgr = new CtoSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
                        cn.Open();
                        aggsBag.Add(mgr.GetShiftSummary(plant, cn, dateCode, startDate, endDate));

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

        public CutAggregates GetCutSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new CutAggregates();
            var aggsBag = new ConcurrentBag<CutAggregate>();

            //var plantList = plants.Replace("'", "").Split(',').ToList();
            var plantObjects = PlantManager.GetPlants(plants);

            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantObjects, (plantItem) =>
                {
                    var mgr = new CtoSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem.PlantCode);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
                        cn.Open();
                        var results = mgr.GetCutSummary(plant, cn, dateCode, startDate, endDate);
                        results.AsParallel().ForAll(t => aggsBag.Add(t));
                        //aggs.AddRange(mgr.GetCutSummary(plant, cn, dateCode, startDate, endDate));

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

        public CutAggregates GetCutSummaryByDate(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new CutAggregates();
            var aggsBag = new ConcurrentBag<CutAggregate>();

            //var plantList = plants.Replace("'", "").Split(',').ToList();
            var plantObjects = PlantManager.GetPlants(plants);

            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantObjects, (plantItem) =>
                {
                    var mgr = new CtoSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem.PlantCode);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
                        cn.Open();
                        var results = mgr.GetCutSummaryByDate(plant, cn, dateCode, startDate, endDate);
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

        public TemperAggregates GetTemperSummary(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new TemperAggregates();
            var aggsBag = new ConcurrentBag<TemperAggregate>();
            var plantList = plants.Replace("'", "").Split(',').ToList();
            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantList, (plantItem) =>
                {
                    var mgr = new CtoSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
                        cn.Open();
                        var results = mgr.GetTemperSummary(plant, cn, dateCode, startDate, endDate);
                        results.AsParallel().ForAll(t => aggsBag.Add(t));
                        //aggs.AddRange(mgr.GetTemperSummary(plant, cn, dateCode, startDate, endDate));
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

        public TemperAggregates GetTemperSummaryByDate(string plants, string dateCode, DateTime startDate, DateTime endDate)
        {
            var aggs = new TemperAggregates();
            var aggsBag = new ConcurrentBag<TemperAggregate>();
            var plantList = plants.Replace("'", "").Split(',').ToList();
            var exceptions = new ConcurrentQueue<Exception>();
            try
            {
                Parallel.ForEach(plantList, (plantItem) =>
                {
                    var mgr = new CtoSummaryManager();
                    var cn = new SqlConnection();
                    try
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        cn = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ClearPath");
                        cn.Open();
                        var results = mgr.GetTemperSummaryByDate(plant, cn, dateCode, startDate, endDate);
                        results.AsParallel().ForAll(t => aggsBag.Add(t));
                        //aggs.AddRange(mgr.GetTemperSummary(plant, cn, dateCode, startDate, endDate));
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

        public RawItems GetRawSupply(string plant, string itemNum)
        {
            var session = new BusinessSystemSession();
            var cn = new SqlConnection();
            var items = new RawItems();
            try
            {
                var itemList = new List<string>();
                if (!String.IsNullOrEmpty(itemNum))
                {
                   itemList = itemNum.Replace("'", "").Split(',').ToList();
                }
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantObj = PlantManager.GetPlant(plant.Replace("'", ""));
                var mgr = new RawSupplyManager(session);
                cn = ConnectionManager.GetSqlConnectionFromKey(plantObj.PlantCode + ".ClearPath");
                cn.Open();
                items = mgr.GetRawItems(session, cn, plantObj, itemList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                cn.Close();
                session.Close();
            }
            return items;
        }

    }
}
