using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class RackManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private ItemAttributeManager _itemAttMgr;

        public RackManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("RackManager.GetRack", GetRack());
            hash.Add("RackManager.GetRackItems", GetRackItems());
            hash.Add("RackManager.GetRackItemReceiving", GetRackItemReceiving());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
        }

        /******************************************************************
        * Racks - Dynamic
        ******************************************************************/
        public iDB2Command GetRack()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
	        SELECT 
                RHZ58RID,
                RHMCU,
                RHLOCN,
                RHZ58RT,
                RHZ58HST,
                RHZ58RSDT,
                RHZ58GWTT / 100 AS RHZ58GWTT,
                RHZ58RTWT / 100 AS RHZ58RTWT,
                RHGWEI,
                RHUWUM,
                RHZ58RCD,
                RHZ58SCN,
                RHZ58BOX,
                RHZ58SLDN,
                RHPSN,
                RHAN8,
                RHUPMJ,
                RHTDAY
	        FROM
		        F580055
            WHERE 
                RHZ58RID = ?
            FOR READ ONLY");
            cmd.Parameters.Add(new iDB2Parameter("Z58RID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["Z58RID"].Precision = 15;
            cmd.Parameters["Z58RID"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetRackItems()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
                RDMCU,
                RDZ58RID,
                RDMCU,
                RDKCOO,
                RDZ58SD,
                RDZ58PS,
                RDZ58STK,
                RDZ58ROW,
                RDZ58SL,
                RDITM,
                RDLOTN,
                RDZ58DST,
                RDTRDJ,
                RDZ58MDT,
                RDZ58PDT,
                RDPQOH,
                RDUOM1,
                RDZ58GWT,
                RDUWUM, 
                RDZ58SN,
                RDUPMJ,
                RDTDAY,
                RDDOCO,
                RDDCTO,
                RDLNID,
                RDDOC1,
                RDDOC2,
                RDXCTO,
                RDLNIC,
                RDPKCO,
                RDPID                     ,
                WAMCU,
                IBSTKT,
                IBGLPT
            FROM 
	            F580056
                INNER JOIN F4102 
                ON  IBMCU = RDMCU AND 
                    IBITM = RDITM
                LEFT OUTER JOIN F4801
                ON WADOCO = RDDOC1 AND
                    RDXDCTO = 'WO'
            WHERE 
                RDZ58RID = ?
            FOR READ ONLY");
            cmd.Parameters.Add(new iDB2Parameter("Z58RID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["Z58RID"].Precision = 15;
            cmd.Parameters["Z58RID"].Scale = 0;
            return cmd;
        }

        public Rack GetRack(BusinessSystemSession session, Decimal rackId, bool attributes = true)
        {
            var rack = new Rack();
            try
            {
                //var f4102Mgr = new F4102Manager(session);
                var cmd = session.DbCommands["RackManager.GetRack"];
                cmd.Parameters[0].Value = rackId;
                cmd.Connection = session.DbConnection;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        rack.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
                        rack.CostCenter = CastDbNull.To<String>(reader["RHMCU"], String.Empty).Trim();
                        rack.Location = CastDbNull.To<String>(reader["RHLOCN"], String.Empty).Trim();
                        rack.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
                        rack.HeaderStatus = CastDbNull.To<String>(reader["RHZ58HST"], String.Empty).Trim();
                        var statusDateId = CastDbNull.To<Decimal>(reader["RHZ58RSDT"], 0);
                        rack.HeaderStatusDate = Utilities.Common.CastToDateTime(statusDateId, 0);
                        rack.GlassWeight = Math.Round(CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0), 4);
                        rack.TareWeight = Math.Round(Data.CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0), 4);
                        rack.GrossWeight = CastDbNull.To<Decimal>(reader["RHGWEI"], 0);
                        rack.ReasonCode = CastDbNull.To<String>(reader["RHZ58RCD"], String.Empty).Trim();
                        rack.SuggestedLoadNum = CastDbNull.To<Decimal>(reader["RHZ58SLDN"], 0);
                        rack.LoadNum = CastDbNull.To<Decimal>(reader["RHPSN"], 0);
                        rack.ScheduleNum = CastDbNull.To<Decimal>(reader["RHZ58SCN"], 0);
                        rack.BoxNum = CastDbNull.To<Decimal>(reader["RHZ58BOX"], 0);
                        var updateDateId = CastDbNull.To<Decimal>(reader["RHUPMJ"], 0);
                        var updateTimeId = CastDbNull.To<Decimal>(reader["RHTDAY"], 0);
                        rack.UpdateDateTime = Utilities.Common.CastToDateTime(updateDateId, updateTimeId);
                        rack.CustomerNum = CastDbNull.To<Decimal>(reader["RHAN8"], 0);
                        var rackItems = GetRackItems(session, rack.RackId, attributes);
                        rack.ManufacturedDate = rackItems.Min(x => x.ManufacturedDate);
                        rack.Quantity = rackItems.Sum(x => x.Quantity);
                        //rack.RackItems = GetRackItems(session, rack.RackId, attributes);
                        //rack.SupplierNum = rack.RackItems[0].SupplierNum;
                    }

                    reader.Close();
                }

                var racks = new Racks();
                racks.Add(rack);
                //AddRackReceiving(session, racks);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return rack;
        }

        public RackItems GetRackItems(BusinessSystemSession session, Decimal rackId, bool attributes = true)
        {
            var rackItems = new RackItems();
            try
            {
                var f4102Mgr = new F4102Manager(session);
                var cmd = new iDB2Command();
                cmd = session.DbCommands["RackManager.GetRackItems"];
                cmd.Parameters[0].Value = rackId;
                cmd.Connection = session.DbConnection;
                _logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rackItem = new RackItem();
                        rackItem.RackId = CastDbNull.To<Decimal>(reader["RDZ58RID"], 0);
                        rackItem.SerialNum = CastDbNull.To<String>(reader["RDZ58SN"], String.Empty).Trim();
                        rackItem.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
                        rackItem.GLCategory = CastDbNull.To<String>(reader["IBGLPT"], String.Empty).Trim();
                        rackItem.PlantNum = CastDbNull.To<String>(reader["RDMCU"], String.Empty).Trim();
                        rackItem.Side = CastDbNull.To<String>(reader["RDZ58SD"], String.Empty).Trim();
                        rackItem.Position = CastDbNull.To<Decimal>(reader["RDZ58PS"], 0);
                        rackItem.StackTier = CastDbNull.To<Decimal>(reader["RDZ58STK"], 0);
                        rackItem.Row = CastDbNull.To<Decimal>(reader["RDZ58ROW"], 0);
                        rackItem.Slot = CastDbNull.To<Decimal>(reader["RDZ58SL"], 0);
                        rackItem.LotNum = CastDbNull.To<String>(reader["RDLOTN"], String.Empty).Trim();
                        rackItem.DetailStatus = CastDbNull.To<String>(reader["RDZ58DST"], String.Empty).Trim();
                        var transactionDateId = CastDbNull.To<Decimal>(reader["RDTRDJ"], 0);
                        rackItem.TransactionDate = Utilities.Common.CastToDateTime(transactionDateId, 0);
                        var manufacturedDateId = CastDbNull.To<Decimal>(reader["RDZ58MDT"], 0);
                        rackItem.ManufacturedDate = Utilities.Common.CastToDateTime(manufacturedDateId, 0);
                        rackItem.ManufacturedPlant = CastDbNull.To<String>(reader["WAMCU"], String.Empty).Trim();
                        var productionDateId = CastDbNull.To<Decimal>(reader["RDZ58PDT"], 0);
                        rackItem.ProductionDate = Utilities.Common.CastToDateTime(productionDateId, 0);
                        var updateDateId = CastDbNull.To<Decimal>(reader["RDUPMJ"], 0);
                        var updateTimeId = CastDbNull.To<Decimal>(reader["RDTDAY"], 0);
                        rackItem.UpdateDateTime = Utilities.Common.CastToDateTime(updateDateId, updateTimeId);
                        rackItem.Quantity = CastDbNull.To<Decimal>(reader["RDPQOH"], 0) / 1000;
                        rackItem.QuantityUom = CastDbNull.To<String>(reader["RDUOM1"], String.Empty).Trim();
                        rackItem.GlassWeight = CastDbNull.To<Decimal>(reader["RDZ58GWT"], 0) / 100;
                        rackItem.GlassWeightUom = CastDbNull.To<String>(reader["RDUWUM"], String.Empty).Trim();
                        rackItem.ProcessId = CastDbNull.To<String>(reader["RDPID"], String.Empty).Trim();
                        rackItem.WorkOrderNum = CastDbNull.To<Decimal>(reader["RDDOC1"], 0);
                        rackItem.WorkOrderPlantNum = CastDbNull.To<String>(reader["WAMCU"], String.Empty).Trim();
                        rackItem.SalesOrderNum = CastDbNull.To<Decimal>(reader["RDDOCO"], 0);
                        rackItem.SalesOrderType = CastDbNull.To<String>(reader["RDDCTO"], String.Empty).Trim();
                        rackItem.SalesOrderLineNum = CastDbNull.To<Decimal>(reader["RDLNID"], 0) / 1000;
                        rackItem.PurchaseOrderNum = CastDbNull.To<Decimal>(reader["RDDOC2"], 0);
                        rackItem.PurchaseOrderType = CastDbNull.To<String>(reader["RDXCTO"], String.Empty).Trim();
                        rackItem.PurchaseOrderLineNum = CastDbNull.To<Decimal>(reader["RDLNIC"], 0) / 1000;
                        var itemBranchAttributes = f4102Mgr.GetPlannerNum(session, rackItem.PlantNum, rackItem.ItemNum);
                        rackItem.PlannerNum = (decimal)itemBranchAttributes["PlannerNum"];
                        rackItem.PlanningFamily = (string)itemBranchAttributes["IBPRP4"];
                        rackItems.Add(rackItem);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return rackItems;
        }


        public iDB2Command GetRackItemReceiving()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText =
            @"SELECT 
                DLUPMJ,
                DLTDAY,
                DLPID,
                DLUSER,
                DLPQOH,
                DLDOC1,
                PHAN8        
            FROM 
	            F580569
                INNER JOIN F4301
                ON  PHDOCO = DLDOC2 AND
                    PHDCTO = DLXCTO AND
                    PHKCOO = DLPKCO AND
                    DLDOC2 > 0
            WHERE 
                DLZ58RID = ? AND
                DLZ58SN = ? AND
                DLMCU = ? AND
                (
                    (DLPID IN ('N580060') AND DLZ58DST = '8' AND DLPSN > 0) OR
                    (DLPID IN ('P4312') AND DLPSN = 0)
                )";
            cmd.Parameters.Add(new iDB2Parameter("RackId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["RackId"].Precision = 15;
            cmd.Parameters["RackId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("SerialNum", iDB2DbType.iDB2Graphic, 30));
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void AddRackReceiving(BusinessSystemSession e1Session, Racks racks)
        {
            try
            {
                foreach (Rack rack in racks)
                {
                    decimal tot = 0;
                    foreach (RackItem rackItem in rack.RackItems)
                    {
                        var cmd = (e1Session.DbCommands.ContainsKey("RackManger.GetRackItemReceiving"))
                                      ? e1Session.DbCommands["RackManger.GetRackItemReceiving"]
                                      : GetRackItemReceiving();
                        cmd.Parameters[0].Value = rack.RackId;
                        cmd.Parameters[1].Value = rackItem.SerialNum;
                        cmd.Parameters[2].Value = rackItem.PlantNum.PadLeft(12);
                        cmd.Connection = e1Session.DbConnection;
                        using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            while (reader.Read())
                            {
                                var recdDateId = CastDbNull.To<Decimal>(reader["DLUPMJ"], 0);
                                rackItem.ReceivedDate = Utilities.Common.CastToDateTime(recdDateId, 0);
                                rackItem.SupplierNum = CastDbNull.To<Decimal>(reader["PHAN8"], 0);
                                rackItem.ReceivedQty = CastDbNull.To<Decimal>(reader["DLPQOH"], 0) / 1000;
                                tot = tot + rackItem.ReceivedQty;
                            }

                            reader.Close();
                        }

                        rack.ReceivedDate = rackItem.ReceivedDate;
                        rack.SupplierNum = rackItem.SupplierNum;
                        rack.ReceivedQty = tot;
                    }
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }


    }
}





