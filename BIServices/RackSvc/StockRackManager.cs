using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.RackSvc
{
    public class StockRackManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;

        public StockRackManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("StockRackManager.GetCost", GetCost());
            hash.Add("StockRackManager.GetReceivedDate", GetReceivedDate());
            hash.Add("StockRackManager.GetItemProperties", GetItemProperties());
            hash.Add("StockRackManager.GetConsumption", GetConsumption());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            _flexItemMgr = new FlexItemManager(session, "P5842300");
        }

        /*********************************************************************************
          * Get Requested Date Facts
          *******************************************************************************/
        public iDB2Command GetRacks(List<string> filters)
        {

            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
	            RHZ58RID,
                RHMCU,
                MCDL01,
	            RHZ58RT,
	            RHAN8,
	            RHSHAN,
	            RHZ58HST,
                RHZ58RSDT,
	            RHLOCN,
	            RHZ58RTWT,
	            RHZ58MTRW,
                RHZ58GWTT,
                RHURRF,
                MIN(RDZ58SN) AS RDZ58SN,
	            MIN(RDITM) AS RDITM,
	            MAX(RDZ58MDT) AS RDZ58MDT,
	            MAX(RDZ58PDT) AS RDZ58PDT,
	            SUM(RDPQOH) AS RDPQOH,
                SUM(RDZ58CQTY) AS RDZ58CQTY
            FROM 
	            F580055
	            INNER JOIN F580056
	            ON RDZ58RID = RHZ58RID
                INNER JOIN F0006
                ON MCMCU = RHMCU
            WHERE
                {0}
                RDLOTN = ''
            GROUP BY
	            RHZ58RID,
                RHMCU,
                MCDL01,
	            RHZ58RT,
	            RHAN8,
	            RHSHAN,
	            RHZ58HST,
                RHZ58RSDT,
	            RHLOCN,
	            RHZ58RTWT,
	            RHZ58MTRW,
                RHZ58GWTT,
                RHURRF,
	            RHZ58BOX,
	            RHZ58SCN
            HAVING
                SUM(RDPQOH) > 0
            ORDER BY 1,2,3	
            FOR READ ONLY", filterString);
            return cmd;
        }

        public Racks GetStockRacks(BusinessSystemSession session, List<string> plantList, int agingDays)
        {
            var racks = new Racks();
            iDB2Command cmd;
            List<String> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (plantList.Count() > 0)
                {
                    string strInClause = "";
                    foreach (string plantItem in plantList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("RHMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Plant
                //filters.Add("RHMCU = ? AND\r\n");
                //parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                //parameters.Last().Value = plant.PlantNum.PadLeft(12);

                // Status
                filters.Add("RHZ58HST NOT IN ('8') AND\r\n");

                // Aging Days
                if (agingDays > 0)
                {
                    var dt = E1TimeManager.CastToE1Date(DateTime.Now.AddDays(agingDays*-1));
                    filters.Add("(RDZ58MDT <= ? OR RDZ58PDT <= ?) AND\r\n");
                    parameters.Add(new iDB2Parameter("DT1", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                    parameters.Add(new iDB2Parameter("DT2", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = dt;
                }

                //// Customer Num
                //if (customerNum > 0)
                //{
                //    filters.Add("RHAN8 = ? AND\r\n");
                //    parameters.Add(new iDB2Parameter("AN8", iDB2DbType.iDB2Decimal));
                //    parameters.Last().Precision = 8;
                //    parameters.Last().Scale = 0;
                //    parameters.Last().Value = customerNum;
                //}

                cmd = GetRacks(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));
                var f4102Mgr = new F4102Manager(session);

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rack = new Rack();
                        rack.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
                        rack.AssetId = CastDbNull.To(reader["RHURRF"], String.Empty).Trim();
                        rack.StockSerialNum = CastDbNull.To(reader["RDZ58SN"], String.Empty).Trim();
                        rack.CostCenter = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.PlantNum = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.PlantCode = CastDbNull.To(reader["MCDL01"], String.Empty).Trim();
                        rack.RackType = CastDbNull.To(reader["RHZ58RT"], String.Empty).Trim();
                        rack.Location = CastDbNull.To(reader["RHLOCN"], String.Empty).Trim();
                        rack.HeaderStatus = CastDbNull.To(reader["RHZ58HST"], String.Empty).Trim();
                        rack.HeaderStatusDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RHZ58RSDT"], 0));
                        rack.ItemNum = CastDbNull.To<Decimal>(reader["RDITM"], 0);
                        rack.CustomerNum = CastDbNull.To<Decimal>(reader["RHAN8"], 0);
                        rack.ShipToCustomerNum = CastDbNull.To<Decimal>(reader["RHSHAN"], 0);
                        rack.ManufacturedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RDZ58MDT"], 0));
                        rack.ProductionDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["RDZ58PDT"], 0));
                        rack.ManufacturedAgingDays = (DateTime.Now - rack.ManufacturedDate.Value).Days;
                        rack.TareWeight = CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0) / 100;
                        rack.GlassWeight = CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0) / 100;
                        rack.GlassWeightTons = CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0) / 100 / 2000;
                        rack.GrossWeight = CastDbNull.To<Decimal>(reader["RHZ58MTRW"], 0) / 100;
                        rack.GrossWeightTons = CastDbNull.To<Decimal>(reader["RHZ58MTRW"], 0) / 100 / 2000;
                        rack.Quantity = CastDbNull.To<Decimal>(reader["RDPQOH"], 0) / 1000;
                        rack.OriginalQuantity = CastDbNull.To<Decimal>(reader["RDZ58CQTY"], 0) / 1000;
                        var itemAtts = _itemAttMgr.GetDiscreteItemAttributes(session, rack.ItemNum);
                        rack.ItemAttributes = itemAtts;
                        rack.UnitActualSqft = itemAtts.GetDecimalAttributeValue("Z58USF");
                        rack.LitesPerRack = itemAtts.GetDecimalAttributeValue("Z58LPR");
                        rack.TotalActualSqft = rack.UnitActualSqft * rack.Quantity;
                        var flexDescs = _flexItemMgr.GetDescription(session, itemAtts);
                        rack.Construction = flexDescs[0];
                        rack.Dimension = flexDescs[1];
                        rack.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                        rack.OverallThickness = itemAtts.GetDecimalAttributeValue("Z5851705");
                        rack.PlannerNum = f4102Mgr.GetPlannerNum(session, rack.PlantNum, rack.ItemNum);
                        rack.CustomerPartNum = f4102Mgr.GetCustomerPartNum(session, rack.PlannerNum, rack.ItemNum);
                        racks.Add(rack);
                    }

                    reader.Close();
                }

                GetItemProperties(session, racks);
                GetCost(session, racks);
                GetReceivedDate(session, racks);
                GetAverageConsumption(session, racks, 90);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return racks;
        }

        /*********************************************************************************
          * Discrete Item Attributes
          *******************************************************************************/
        public iDB2Command GetItemProperties()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            IBSTKT,
                IBGLPT,
                IMDSC1,
                IMDSC2
            FROM
	            F4102
                INNER JOIN F4101
                ON IMITM = IBITM
            WHERE
                IBITM = ? AND
                IBMCU = ? 
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetItemProperties(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetItemProperties"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.ItemNum;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.ItemDesc = CastDbNull.To(reader["IMDSC1"], String.Empty).Trim();
                            rack.StockType = CastDbNull.To(reader["IBSTKT"], String.Empty).Trim();
                            rack.GlCategory = CastDbNull.To(reader["IBGLPT"], String.Empty).Trim();
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetCost()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(IECSL) AS IECSL
            FROM
	            F30026
            WHERE
                IEITM = ? AND
                IEMMCU = ? AND
                IELEDG = '07'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetCost(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetCost"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.ItemNum;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.UnitCost = CastDbNull.To<Decimal>(reader["IECSL"], 0) / 10000;
                            rack.TotalCost = rack.UnitCost * rack.Quantity;
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetReceivedDate()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                HLUPMJ
            FROM (
                SELECT
	                HLUPMJ,
                    ROW_NUMBER() OVER (PARTITION BY HLZ58RID ORDER BY HLUKID ASC) AS ROWNUM
                FROM
	                F580559
                WHERE
                    HLZ58RID = ? AND
                    HLMCU = ?) A
            WHERE
                A.ROWNUM = 1
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("RID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["RID"].Precision = 15;
            cmd.Parameters["RID"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public void GetReceivedDate(BusinessSystemSession session, Racks racks)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetReceivedDate"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.RackId;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.ReceivedDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["HLUPMJ"], 0));
                            rack.ReceivedAgingDays = (DateTime.Now - rack.ReceivedDate.Value).Days;
                            rack.ReceivedQty = rack.OriginalQuantity;
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /*********************************************************************************
          * Stock Racks
          *******************************************************************************/
        public iDB2Command GetConsumption()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(ILTRQT) AS ILTRQT
            FROM
	            F4111
            WHERE
                ILITM = ? AND
                ILMCU = ? AND
                ILTRDJ >= ? AND
                ILDCT IN ('2I','IM','1A')
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("TRDJ", iDB2DbType.iDB2Decimal));
            cmd.Parameters["TRDJ"].Precision = 6;
            cmd.Parameters["TRDJ"].Scale = 0;
            return cmd;
        }

        public void GetAverageConsumption(BusinessSystemSession session, Racks racks, int interval)
        {
            try
            {
                var cmd = session.DbCommands["StockRackManager.GetConsumption"];
                cmd.Connection = session.DbConnection;
                foreach (var rack in racks)
                {
                    cmd.Parameters[0].Value = rack.ItemNum;
                    cmd.Parameters[1].Value = rack.CostCenter.PadLeft(12);
                    cmd.Parameters[2].Value = E1TimeManager.CastToE1Date(DateTime.Now.Date.AddDays(interval * -1));
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            rack.ItemConsumptionDays = interval;
                            rack.ItemConsumptionAmt = CastDbNull.To<Decimal>(reader["ILTRQT"], 0) / -1000;
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
