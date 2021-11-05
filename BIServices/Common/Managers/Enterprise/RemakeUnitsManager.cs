using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class RemakeUnitsManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public RemakeUnitsManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("RemakeUnitsManager.GetSupplierOrder", GetSupplierOrder());
            hash.Add("RemakeUnitsManager.GetSupplierRack", GetSupplierRack());
            hash.Add("RemakeUnitsManager.GetSupplierRackLedger", GetSupplierRackLedger());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        public SqlCommand GetPurchasedRemakes()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                TRY_CAST(RemakeSchedule AS DECIMAL(5,0)) AS RemakeSchedule,
                UnitID,
                r.OrderNum AS WOrderNum,
	            ord.SOrderNum,
	            ord.SOrderType,
	            ord.SOrderLine,
                TRY_CAST(OriginalSchedule AS DECIMAL(5,0)) AS OriginalSchedule,
                Construction,
                Dimensions,
                ord.ShapeNum,
                r.CustName,
                ord.CustomerDate,
                r.WrittenDate,
                TRY_CAST(RemakeQuantity AS INT) AS RemakeQuantity,
                ro.RelatedOrderNum,
                ro.RelatedOrderType,
                ro.RelatedOrderLineNum,
                ro.LiteNum,
 	            ro.RelatedSupplierPlant,
                Schedules = (SELECT COUNT(DISTINCT CurrentSchedule) FROM ProdData pd WITH (NOLOCK) WHERE pd.Ordernum = ro.WOrderNum AND pd.WrittenDate < ro.TransactionDate)
            FROM
                Remakes r WITH (NOLOCK) 
                LEFT OUTER JOIN IncomingOrders ord WITH (NOLOCK) 
                ON  ord.WOrderNum = r.OrderNum
                LEFT OUTER JOIN EBS.RelatedOrder ro WITH (NOLOCK) 
                ON  ro.WOrderNum = r.OrderNum AND
                    ro.RelatedOrderType = 'OT' AND
                    ro.TransactionType = 'R' AND
                    ro.TransactionDate > r.WrittenDate AND
                    ro.RelatedSupplierNum > 0
            WHERE
                PurchaseOrderSent = 1
            ORDER BY 1,2,3";
            return cmd;
        }

        public RemakeUnits GetPurchasedRemakes(BusinessSystemSession session, SqlConnection purchaserCn, SqlConnection supplierCn, Plant plant)
        {
            var units = new RemakeUnits();
            try
            {
                var cmd = GetPurchasedRemakes();
                cmd.Connection = purchaserCn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var unit = new RemakeUnit();
                        unit.Plant = plant.PlantCode;
                        unit.PurchaserPlantNum = plant.PlantNum;
                        unit.UnitId = Utilities.Data.CastStringToDecimal(CastDbNull.To<string>(reader["UnitID"], string.Empty).Trim());
                        unit.WorkOrderNum = CastDbNull.To<decimal>(reader["WOrderNum"], 0);
                        unit.SalesOrderNum = CastDbNull.To<decimal>(reader["SOrderNum"], 0);
                        unit.SalesOrderType = CastDbNull.To<string>(reader["SOrderType"], string.Empty).Trim();
                        unit.SalesOrderLineNum = CastDbNull.To<decimal>(reader["SOrderLine"], 0);
                        unit.SalesOrder =
                            $"{unit.SalesOrderNum.ToString()}-{unit.SalesOrderType}-{unit.SalesOrderLineNum.ToString()}";
                        unit.OriginalSchedule = CastDbNull.To<decimal>(reader["OriginalSchedule"], 0);
                        unit.RemakeSchedule = CastDbNull.To<decimal>(reader["RemakeSchedule"], 0);
                        unit.Construction = CastDbNull.To<string>(reader["Construction"], string.Empty).Trim();
                        unit.Dimensions = CastDbNull.To<string>(reader["Dimensions"], string.Empty).Trim();
                        unit.ShapeNum = CastDbNull.To<decimal>(reader["ShapeNum"], 0);
                        unit.CustomerName = CastDbNull.To<string>(reader["CustName"], string.Empty).Trim();
                        unit.RequestedDate = CastDbNull.To<DateTime>(reader["CustomerDate"], new DateTime(1900, 1, 1));
                        unit.Quantity = CastDbNull.To<int>(reader["RemakeQuantity"], 0);
                        unit.RelatedOrderNum = CastDbNull.To<decimal>(reader["RelatedOrderNum"], 0);
                        unit.RelatedOrderType = CastDbNull.To<string>(reader["RelatedOrderType"], string.Empty).Trim();
                        unit.RelatedOrderLineNum = CastDbNull.To<decimal>(reader["RelatedOrderLineNum"], 0);
                        unit.RelatedOrder =
                            $"{unit.RelatedOrderNum.ToString()}-{unit.RelatedOrderType}-{unit.RelatedOrderLineNum.ToString()}";
                        unit.LiteNum = CastDbNull.To<int>(reader["LiteNum"], 0);
                        unit.RemakeDate = CastDbNull.To<DateTime>(reader["WrittenDate"], new DateTime(1900, 1, 1));
                        unit.SupplierPlant = CastDbNull.To<string>(reader["RelatedSupplierPlant"], string.Empty).Trim();
                        unit.Attempts = CastDbNull.To<int>(reader["Schedules"], 0);
                        units.Add(unit);
                    }

                    reader.Close();
                }

                GetSupplierOrder(session, units);
                GetSupplierRack(session, units);
                GetSupplierScheduleLG(session, supplierCn, units);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return units;
        }

        public iDB2Command GetSupplierOrder()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
	            PDMCU, 
	            PDRORN, 
	            PDRCTO, 
	            PDRLLN
            FROM 
	            F4311 
            WHERE 
                PDMCU = ? AND
                PDDOCO = ? AND
                PDDCTO = ? AND
                PDLNID = ?
            FOR READ ONLY");
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetSupplierOrder(BusinessSystemSession session,  RemakeUnits units)
        {
            try
            {
                var cmd = session.DbCommands["RemakeUnitsManager.GetSupplierOrder"];
                cmd.Connection = session.DbConnection;

                foreach (var unit in units)
                {
                    cmd.Parameters[0].Value = unit.PurchaserPlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = unit.RelatedOrderNum;
                    cmd.Parameters[2].Value = unit.RelatedOrderType;
                    cmd.Parameters[3].Value = unit.RelatedOrderLineNum * 1000;
                    _logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            unit.SupplierOrderNum =
                                Utilities.Data.CastStringToDecimal(CastDbNull.To<String>(reader["PDRORN"], String.Empty)
                                    .Trim());
                            unit.SupplierOrderType = CastDbNull.To<String>(reader["PDRCTO"], String.Empty).Trim();
                            unit.SupplierOrderLineNum = CastDbNull.To<Decimal>(reader["PDRLLN"], 0) / 1000;
                            unit.SupplierOrder =
                                $"{unit.SupplierOrderNum.ToString()}-{unit.SupplierOrderType}-{string.Format("{0:0.000}", unit.SupplierOrderLineNum)}";
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
        }

        public iDB2Command GetSupplierRack()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
	            RDMCU AS MCU,
	            RDZ58RID AS Z58RID, 
	            RDZ58SN AS Z58SN,
	            RDZ58DST AS Z58DST,
	            RDUPMJ AS UPMJ,
	            RDTDAY AS TDAY,
	            RHZ58RT AS Z58RT
            FROM 
                F580056
	            INNER JOIN F580055
	            ON RHZ58RID = RDZ58RID            
            WHERE 
	            RDMCU = ?  AND 
	            RDDOCO = ? AND 
	            RDDCTO = ? AND 
	            RDLNID = ?
            FOR READ ONLY");
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetSupplierRackLedger()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
	            DLMCU AS MCU,
	            DLZ58RID AS Z58RID, 
	            DLZ58SN AS Z58SN,
	            DLZ58DST AS Z58DST,
	            DLMUPM AS UPMJ,
	            DLUPMT AS TDAY,
	            RHZ58RT AS Z58RT
            FROM (
            SELECT 
	            DLMCU,
	            DLZ58RID, 
	            DLZ58SN,
	            DLZ58DST,
	            DLMUPM,
	            DLUPMT,
	            ROW_NUMBER() OVER (PARTITION BY DLLOTN, DLITM, DLZ58SN ORDER BY DLUKID DESC) AS RN
            FROM 
	            F580569 
            WHERE 
	            DLMCU = ?  AND 
	            DLDOCO = ? AND 
	            DLDCTO = ? AND 
	            DLLNID = ?) A
	            INNER JOIN F580055
	            ON RHZ58RID = DLZ58RID
            WHERE
	            A.RN = 1
            FOR READ ONLY");
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public void GetSupplierRack(BusinessSystemSession session, RemakeUnits units)
        {
            try
            {
                var cmdRack = session.DbCommands["RemakeUnitsManager.GetSupplierRack"];
                cmdRack.Connection = session.DbConnection;
                var cmdRackLedger = session.DbCommands["RemakeUnitsManager.GetSupplierRackLedger"];
                cmdRackLedger.Connection = session.DbConnection;

                foreach (var unit in units)
                {
                    cmdRack.Parameters[0].Value = unit.SupplierPlant.PadLeft(12);
                    cmdRack.Parameters[1].Value = unit.SupplierOrderNum;
                    cmdRack.Parameters[2].Value = unit.SupplierOrderType;
                    cmdRack.Parameters[3].Value = unit.SupplierOrderLineNum * 1000;
                    _logger.Trace(Logging.DbCmdString(cmdRack));
                    using (IDataReader readerRack = cmdRack.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (readerRack.Read())
                        {
                            unit.RackId = CastDbNull.To<Decimal>(readerRack["Z58RID"], 0);
                            unit.RackType = CastDbNull.To<String>(readerRack["Z58RT"], String.Empty).Trim();
                            unit.RackItemStatus = CastDbNull.To<String>(readerRack["Z58DST"], String.Empty).Trim();
                            unit.SerialNum = CastDbNull.To<String>(readerRack["Z58SN"], String.Empty).Trim();
                            if (unit.RackId > 0)
                            {
                                unit.RackTransactionDate = Utilities.Common.CastToDateTime(
                                    CastDbNull.To<Decimal>(readerRack["UPMJ"], 0),
                                    CastDbNull.To<Decimal>(readerRack["TDAY"], 0));
                            }
                        }

                        readerRack.Close();
                    }

                    if (unit.RackId == 0)
                    {
                        cmdRackLedger.Parameters[0].Value = unit.SupplierPlant.PadLeft(12);
                        cmdRackLedger.Parameters[1].Value = unit.SupplierOrderNum;
                        cmdRackLedger.Parameters[2].Value = unit.SupplierOrderType;
                        cmdRackLedger.Parameters[3].Value = unit.SupplierOrderLineNum * 1000;
                        _logger.Trace(Logging.DbCmdString(cmdRackLedger));
                        using (IDataReader readerRackLedger = cmdRackLedger.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            while (readerRackLedger.Read())
                            {
                                unit.RackId = CastDbNull.To<Decimal>(readerRackLedger["Z58RID"], 0);
                                unit.RackType = CastDbNull.To<String>(readerRackLedger["Z58RT"], String.Empty).Trim();
                                unit.RackItemStatus = CastDbNull.To<String>(readerRackLedger["Z58DST"], String.Empty).Trim();
                                unit.SerialNum = CastDbNull.To<String>(readerRackLedger["Z58SN"], String.Empty).Trim();
                                if (unit.RackId > 0)
                                {
                                    unit.RackTransactionDate = Utilities.Common.CastToDateTime(
                                        CastDbNull.To<Decimal>(readerRackLedger["UPMJ"], 0),
                                        CastDbNull.To<Decimal>(readerRackLedger["TDAY"], 0));
                                }
                            }

                            readerRackLedger.Close();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
        }


        //public SqlCommand GetSupplierScheduleLG()
        //{
        //    var cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = @"
        //    SELECT
        //        WorkOrderNumber,
	       //     SerialNumber,
	       //     OriginalScheduleNum,
	       //     OriginalScheduleDate,
        //        CurrentScheduleNum,
        //        CurrentScheduleDate,
        //        Schedules
        //    FROM
        //        ShopFloor.EBS.vw_ProdUnit
        //    WHERE
        //        WorkOrderNumber = @WorkOrderNumber AND
        //        SerialNumber = @SerialNumber";
        //    cmd.Parameters.Add(new SqlParameter("@WorkOrderNumber", SqlDbType.Decimal));
        //    cmd.Parameters["@WorkOrderNumber"].Precision = 8;
        //    cmd.Parameters["@WorkOrderNumber"].Scale = 0;
        //    cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.BigInt));
        //    return cmd;
        //}

        //public RemakeUnits GetSupplierScheduleLG(BusinessSystemSession session, SqlConnection cn, RemakeUnits units)
        //{
        //    try
        //    {
        //        var cmd = GetSupplierScheduleLG();
        //        cmd.Connection = cn;
        //        foreach (var unit in units)
        //        {
        //            cmd.Parameters["@WorkOrderNumber"].Value = unit.WorkOrderNum;
        //            cmd.Parameters["@SerialNumber"].Value = (Int64)unit.UnitId;
        //            using( IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
        //            while (reader.Read())
        //            {
        //                unit.SupplierScheduleNum = CastDbNull.To<decimal>(reader["CurrentScheduleNum"], 0);
        //                unit.SupplierScheduleDate = CastDbNull.To<DateTime>(reader["CurrentScheduleDate"], new DateTime(1900, 1, 1));
        //                unit.SupplierSchedules = CastDbNull.To<int>(reader["Schedules"], 0);
        //            }
        //            reader.Close();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //    return units;
        //}


        public SqlCommand GetSupplierScheduleLG()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                *
            FROM
                (
                    SELECT
                        sl.OrderNumber,
                        sl.SalesOrderLineNumber,
                        wo.WorkOrderNumber,
                        sd.ScheduleNumber,
                        sd.ScheduleDate,
                        ROW_NUMBER() OVER (PARTITION BY
                                               sl.OrderNumber,
                                               sl.SalesOrderLineNumber
                                           ORDER BY
                                               ps.ProdScheduleId DESC
                                          ) AS RN
                    FROM
                        CommandCenter.CommandCenter.SalesOrderLine                   sl
                        LEFT OUTER JOIN CommandCenter.CommandCenter.WorkOrder        wo
                        ON  wo.WorkOrderId = sl.WorkOrderId
                        LEFT OUTER JOIN CommandCenter.CommandCenter.SpecUnit         su
                        ON  su.WorkOrderId = wo.WorkOrderId
                        LEFT OUTER JOIN CommandCenter.CommandCenter.ProdUnit         pu
                        ON  pu.SpecUnitId = su.SpecUnitId
                        LEFT OUTER JOIN CommandCenter.CommandCenter.ProdScheduleUnit psu
                        ON  psu.ProdUnitId = pu.ProdUnitId
                        LEFT OUTER JOIN CommandCenter.CommandCenter.ProdSchedule     ps
                        ON  ps.ProdScheduleId = psu.ProdScheduleId
                        LEFT OUTER JOIN CommandCenter.CommandCenter.Schedule         sd
                        ON  sd.ScheduleId = ps.ScheduleId
                    WHERE
                        sl.OrderNumber = @OrderNumber AND
                        sl.SalesOrderLineNumber = @SalesOrderLineNumber
                ) A
            WHERE
                A.RN = 1";
            cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Decimal));
            cmd.Parameters["@OrderNumber"].Precision = 8;
            cmd.Parameters["@OrderNumber"].Scale = 0;
            cmd.Parameters.Add(new SqlParameter("@SalesOrderLineNumber", SqlDbType.VarChar, 12));
            cmd.Parameters["@SalesOrderLineNumber"].Precision = 6;
            cmd.Parameters["@SalesOrderLineNumber"].Scale = 3;
            return cmd;
        }

        public SqlCommand GetSupplierScheduleLG_OLD()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                sl.OrderNumber,
	            sl.SalesOrderLineNumber,
	            wo.WorkOrderNumber,
	            sd.ScheduleNumber,
	            sd.ScheduleDate
            FROM
                CommandCenter.CommandCenter.SalesOrderLine sl
	            LEFT OUTER JOIN CommandCenter.CommandCenter.WorkOrder wo
	            ON wo.WorkOrderId = sl.WorkOrderId
	            LEFT OUTER JOIN CommandCenter.CommandCenter.ScheduleLine sln
	            ON sln.ScheduleLineId = wo.ScheduleLineId
	            LEFT OUTER JOIN CommandCenter.CommandCenter.Schedule sd
	            ON sd.ScheduleId = sln.ScheduleId
            WHERE
                sl.OrderNumber = @OrderNumber AND
                sl.SalesOrderLineNumber = @SalesOrderLineNumber";
            cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Decimal));
            cmd.Parameters["@OrderNumber"].Precision = 8;
            cmd.Parameters["@OrderNumber"].Scale = 0;
            cmd.Parameters.Add(new SqlParameter("@SalesOrderLineNumber", SqlDbType.VarChar, 12));
            cmd.Parameters["@SalesOrderLineNumber"].Precision = 6;
            cmd.Parameters["@SalesOrderLineNumber"].Scale = 3;
            return cmd;
        }

        public RemakeUnits GetSupplierScheduleLG(BusinessSystemSession session, SqlConnection cn, RemakeUnits units)
        {
            try
            {
                var cmd = GetSupplierScheduleLG();
                cmd.Connection = cn;
                foreach (var unit in units)
                {
                    cmd.Parameters["@OrderNumber"].Value = unit.SupplierOrderNum;
                    cmd.Parameters["@SalesOrderLineNumber"].Value = unit.SupplierOrderLineNum;
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            unit.SupplierScheduleNum = CastDbNull.To<decimal>(reader["ScheduleNumber"], 0);
                            unit.SupplierScheduleDate = CastDbNull.To<DateTime>(reader["ScheduleDate"], new DateTime(1900, 1, 1));
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return units;
        }
    }
}

