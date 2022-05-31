using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.ShopFloor
{
    public class OpenRackManagerLG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OpenRackManagerLG(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        public GlassRacks GetOpenRacks(BusinessSystemSession session,  SqlConnection cnShopFloor, SqlConnection cnDw, Plant plant,
            string maxDateCode, DateTime? startDate = null, DateTime? endDate = null, decimal custNum = 0, string rackType = null, string scheduleType = null, List<decimal> scheduleNumList = null, List<int> igLinesList = null, 
            List<string> rackStatusList = null, List<string> unitTypeList = null, List<string> optionsList = null, string customerSoldNum = null, string customerShipNum = null, decimal boxNum = 0, string orderDateCode = null, 
            decimal hoursToTruck = 0, List<decimal> rackIdsList = null, List<string> productionTypesList = null)
        {
            var racks = new GlassRacks();
            var openUnitMgr = new ShopFloor.OpenUnitManagerLG(session);
            try
            {
                _logger.Trace("Get Open Units");

                var units = openUnitMgr.GetOpenUnits(session, cnShopFloor, cnDw, plant, maxDateCode, startDate,
                    endDate, custNum,
                    rackType, scheduleType, scheduleNumList, igLinesList, rackStatusList, unitTypeList, optionsList,
                    customerSoldNum, customerShipNum, boxNum, orderDateCode, hoursToTruck, rackIdsList, productionTypesList);

                _logger.Trace(units.Count);

                var gRacks = units.GroupBy(x => x.RackId).OrderBy(z => z.Key);
                foreach (var gRack in gRacks)
                {
                    var rack = new GlassRack();
                    var firstUnit = gRack.First();
                    _logger.Trace(gRack.First().RackId);

                    rack.Plant = plant.PlantNum;
                    rack.RackId = gRack.Key;
                    rack.AssetId = firstUnit.AssetId;
                    rack.FirstUnitId = gRack.Min(x => x.UnitId);
                    rack.LastUnitId = gRack.Max(x => x.UnitId);
                    rack.UnitType = gRack.GroupBy(x => x.UnitType).OrderByDescending(z => z.Sum(t => t.Units)).First().Key;
                    rack.ProductType = gRack.GroupBy(x => x.ProductType).OrderByDescending(z => z.Sum(t => t.Units)).First().Key;
                    rack.CardinalProductType = gRack.GroupBy(x => x.CardinalProductType).OrderByDescending(z => z.Sum(t => t.Units)).First().Key;
                    rack.RackType = firstUnit.RackType;
                    rack.RackLocation = firstUnit.RackLocation;
                    rack.BoxNum = firstUnit.BoxNum;
                    rack.Status = firstUnit.Status;
                    rack.HeaderStatus = firstUnit.RackStatusCode;
                    rack.RackCloseDate = firstUnit.RackCloseDate;
                    rack.RackLockDate = firstUnit.RackLockDate;
                    rack.RackLockLoadDate = firstUnit.RackLockLoadDate;
                    rack.PriorityCode = firstUnit.PriorityCode;
                    rack.CustomerName = firstUnit.CustomerName;
                    rack.CustomerNum = firstUnit.CustomerNum;
                    rack.CustomerPO = firstUnit.CustomerName;
                    rack.CustomerPOLineNum = firstUnit.CustomerPOLineNum;
                    rack.CustomerPONum = firstUnit.CustomerPONum;
                    rack.CustomerPart1 = firstUnit.CustomerPart1;
                    rack.CustomerPart2 = firstUnit.CustomerPart2;
                    rack.CustomerPart3 = firstUnit.CustomerPart3;
                    rack.CustomerSeqNum = firstUnit.CustomerSeqNum;
                    rack.CustomerSeqPart1 = firstUnit.CustomerSeqPart1;
                    rack.CustomerShipName = firstUnit.CustomerShipName;
                    rack.CustomerShipNum = firstUnit.CustomerShipNum;
                    rack.CustomerTier = firstUnit.CustomerTier;
                    rack.FirstCustSeqNum = gRack.Min(x => x.CustomerPart1);
                    rack.LastCustSeqNum = gRack.Max(x => x.CustomerPart1);
                    rack.ScheduleNum = firstUnit.ScheduleNum;
                    rack.ScheduleType = firstUnit.ScheduleType;
                    rack.MinOrderDate = gRack.Min(x => x.OrderDate);
                    rack.MaxOrderDate = gRack.Max(x => x.OrderDate);
                    rack.OrderDate = gRack.Min(x => x.OrderDate);
                    rack.MinRequestedDate = gRack.Min(x => x.RequestedDate);
                    rack.MaxRequestedDate = gRack.Max(x => x.RequestedDate);
                    rack.RequestedDate = gRack.Min(x => x.RequestedDate);
                    rack.ShipRoute = gRack.GroupBy(x => x.ShipRoute).OrderByDescending(z => z.Sum(t => t.Units)).First().Key;
                    rack.DeliveryDateTime = gRack.Min(x => x.DeliveryDateTime);
                    rack.DeliveryDateTimeHours = gRack.Min(x => x.DeliveryDateTimeHours);
                    rack.IGLineRun = gRack.GroupBy(x => x.IGLineRun).OrderByDescending(z => z.Sum(t => t.Units)).First().Key;
                    rack.IGLineScheduled = gRack.GroupBy(x => x.IGLineScheduled).OrderByDescending(z => z.Sum(t => t.Units)).First().Key;

                    rack.Units = gRack.Sum(x => x.Units);
                    rack.Tubes = gRack.Sum(x => x.Tubes);
                    rack.Scoring = gRack.Sum(x => x.Scoring);
                    rack.Picks = gRack.Sum(x => x.Picks);
                    rack.Purchases = gRack.Sum(x => x.Purchases);
                    rack.Units = gRack.Sum(x => x.Units);
                    rack.Bars = gRack.Sum(x => x.Bars);
                    rack.Shapes = gRack.Sum(x => x.Shapes);
                    rack.OnHand = gRack.Sum(x => x.OnHand);
                    rack.TareWeight = gRack.First().RackTareWeight;
                    rack.GlassWeight = gRack.Sum(x => x.GlassWeight);
                    rack.TotalWeight = rack.TareWeight + rack.GlassWeight;

                    racks.Add(rack);
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            _logger.Info($"Return Racks {racks.Count}");
            return racks;
        }

    }
}



