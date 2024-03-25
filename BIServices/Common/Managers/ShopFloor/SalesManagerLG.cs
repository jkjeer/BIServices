using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
using static BIServices.Common.Utilities.Sql;

namespace BIServices.Common.Managers.ShopFloor
{
    public class SalesManagerLG
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SalesManagerLG()
        {
        }

        /*********************************************************************************
         * Get Open Units Dynamic
         *******************************************************************************/
        public SqlCommand GetSubstrateSales(SqlFilters filters)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT
                *
            FROM
            (   SELECT
                    sl.CustomerSoldNum                     AS SoldToCustNum,
                    ab1.Name                               AS SoldToCustName,
                    sl.CustomerShipNum                     AS ShipToCustNum,
                    ab2.Name                               AS ShipToCustName,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC                                   AS SubstrateCode,
                    s.TK                                   AS SubstrateThickness,
                    s.MTRL                                 AS SubstrateMaterial,
                    CAST(SUM(sl.Quantity) AS INT)          AS Units,
                    SUM(sl.UnitBillableSqft * sl.Quantity) AS BillableSqft,
                    SUM(sl.UnitPriceAmt * sl.Quantity)     AS SalesAmt
                FROM
                    EBS.SalesLine                           sl
                    INNER JOIN EBS.Item                     i
                    ON  i.ItemNum = sl.ItemNum
                    INNER JOIN EBS.AddressBook              ab1
                    ON  ab1.AddressNum = sl.CustomerSoldNum
                    INNER JOIN EBS.AddressBook              ab2
                    ON  ab2.AddressNum = sl.CustomerShipNum
                    INNER JOIN EBS.vw_SpecItemSubstrateType s
                    ON  s.ItemNum = sl.ItemNum
                WHERE
                    sl.LineType = 'S'
                GROUP BY
                    sl.CustomerSoldNum,
                    sl.CustomerShipNum,
                    ab1.Name,
                    ab2.Name,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    sl.InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC,
                    s.TK,
                    s.MTRL
                UNION ALL
                SELECT
                    sl.CustomerSoldNum                     AS SoldToCustNum,
                    ab1.Name                               AS SoldToCustName,
                    sl.CustomerShipNum                     AS ShipToCustNum,
                    ab2.Name                               AS ShipToCustName,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC                                   AS SubstrateCode,
                    s.TK                                   AS SubstrateThickness,
                    s.MTRL                                 AS SubstrateMaterial,
                    CAST(SUM(sl.Quantity) AS INT)          AS Units,
                    SUM(sl.UnitBillableSqft * sl.Quantity) AS BillableSqft,
                    SUM(sl.UnitPriceAmt * sl.Quantity)     AS SalesAmt
                FROM
                    EBS.SalesLine                           sl
                    INNER JOIN EBS.Item                     i
                    ON  i.ItemNum = sl.ItemNum
                    INNER JOIN EBS.AddressBook              ab1
                    ON  ab1.AddressNum = sl.CustomerSoldNum
                    INNER JOIN EBS.AddressBook              ab2
                    ON  ab2.AddressNum = sl.CustomerShipNum
                    INNER JOIN EBS.vw_SpecUnitSubstrateType s
                    ON  s.ConfigurationId = sl.ConfigurationId
                WHERE
                    sl.LineType = 'W'
                GROUP BY
                    sl.CustomerSoldNum,
                    sl.CustomerShipNum,
                    ab1.Name,
                    ab2.Name,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    sl.InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC,
                    s.TK,
                    s.MTRL) A
            WHERE
                {filters} AND
                LastStatus <> '980'";
        return cmd;
                }

        public SqlCommand GetInterlayerSales(SqlFilters filters)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT
                *
            FROM
            (   SELECT
                    sl.CustomerSoldNum                     AS SoldToCustNum,
                    ab1.Name                               AS SoldToCustName,
                    sl.CustomerShipNum                     AS ShipToCustNum,
                    ab2.Name                               AS ShipToCustName,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC                                   AS InterlayerCode,
                    s.TK                                   AS InterlayerThickness,
                    s.MTRL                                 AS InterlayerMaterial,
                    CAST(SUM(sl.Quantity) AS INT)          AS Units,
                    SUM(sl.UnitBillableSqft * sl.Quantity) AS BillableSqft,
                    SUM(sl.UnitPriceAmt * sl.Quantity)     AS SalesAmt
                FROM
                    EBS.SalesLine                            sl
                    INNER JOIN EBS.Item                      i
                    ON  i.ItemNum = sl.ItemNum
                    INNER JOIN EBS.AddressBook               ab1
                    ON  ab1.AddressNum = sl.CustomerSoldNum
                    INNER JOIN EBS.AddressBook               ab2
                    ON  ab2.AddressNum = sl.CustomerShipNum
                    INNER JOIN EBS.vw_SpecItemInterlayerType s
                    ON  s.ItemNum = sl.ItemNum
                WHERE
                    sl.LineType = 'S'
                GROUP BY
                    sl.CustomerSoldNum,
                    sl.CustomerShipNum,
                    ab1.Name,
                    ab2.Name,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    sl.InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC,
                    s.TK,
                    s.MTRL
                UNION ALL
                SELECT
                    sl.CustomerSoldNum                     AS SoldToCustNum,
                    ab1.Name                               AS SoldToCustName,
                    sl.CustomerShipNum                     AS ShipToCustNum,
                    ab2.Name                               AS ShipToCustName,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC                                   AS InterlayerCode,
                    s.TK                                   AS InterlayerThickness,
                    s.MTRL                                 AS InterlayerMaterial,
                    CAST(SUM(sl.Quantity) AS INT)          AS Units,
                    SUM(sl.UnitBillableSqft * sl.Quantity) AS BillableSqft,
                    SUM(sl.UnitPriceAmt * sl.Quantity)     AS SalesAmt
                FROM
                    EBS.SalesLine                            sl
                    INNER JOIN EBS.Item                      i
                    ON  i.ItemNum = sl.ItemNum
                    INNER JOIN EBS.AddressBook               ab1
                    ON  ab1.AddressNum = sl.CustomerSoldNum
                    INNER JOIN EBS.AddressBook               ab2
                    ON  ab2.AddressNum = sl.CustomerShipNum
                    INNER JOIN EBS.vw_SpecUnitInterlayerType s
                    ON  s.ConfigurationId = sl.ConfigurationId
                WHERE
                    sl.LineType = 'W'
                GROUP BY
                    sl.CustomerSoldNum,
                    sl.CustomerShipNum,
                    ab1.Name,
                    ab2.Name,
                    i.StockType,
                    sl.VolumeType,
                    sl.RequestedDate,
                    sl.InvoiceDate,
                    sl.LastStatus,
                    sl.NextStatus,
                    s.PC,
                    s.TK,
                    s.MTRL) A
            WHERE
                {filters} AND
                LastStatus <> '980'";
            return cmd;
        }



        public SqlCommand GetInterlayerSalesLG0(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT
                 *
            FROM (
                     SELECT
                         ord.SoldToCustName                     AS SoldToCustName,
                         ord.SoldToCustNum                      AS SoldToCustNum,
                         ord.ShipToCustName                     AS ShipToCustName,
                         ord.ShipToCustNum                      AS ShipToCustNum,
                         gi.StockType                           AS StockType,
                         CASE
                             WHEN ord.ScheduleType LIKE '%HV%MTO%' THEN
                                  'HV'
                             ELSE sl.VolumeType
                         END                                    AS VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                         REPLACE (ord.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (sl.Quantity)  AS INT)                       AS Units,
                         SUM (ord.BillableSqft * sl.Quantity)   AS BillableSqft,
                         SUM (sl.UnitPriceAmt * sl.Quantity)    AS SalesAmt
                     FROM
                         EBS.ScheduledOrders     ord WITH (NOLOCK)
                         INNER JOIN EBS.WorkOrder wk WITH (NOLOCK)
                         ON  wk.OrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.GlassItem gi WITH (NOLOCK)
                         ON  gi.ItemNum = ord.ItemNum
                         INNER JOIN EBS.SalesLine sl WITH (NOLOCK)
                         ON  sl.WOrderNum = ord.WorkOrderNum
                     WHERE
                         gi.StockType = 'C' AND
                         ISNULL (ord.Interlayer1Code, '') <> ''
                     GROUP BY
                         ord.SoldToCustName,
                         ord.SoldToCustNum,
                         ord.ShipToCustName,
                         ord.ShipToCustNum,
                         gi.StockType,
                         REPLACE (ord.Interlayer1Code, '.', ''),
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         sl.InvoiceDate,
                         CASE
                             WHEN ord.ScheduleType LIKE '%HV%MTO%' THEN
                                  'HV'
                             ELSE sl.VolumeType
                         END
                     UNION ALL
                     SELECT
                         ord.SoldToCustName                     AS SoldToCustName,
                         ord.SoldToCustNum                      AS SoldToCustNum,
                         ord.ShipToCustName                     AS ShipToCustName,
                         ord.ShipToCustNum                      AS ShipToCustNum,
                         gi.StockType                           AS StockType,
                         'HV'                                   AS VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                         REPLACE (ord.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (sl.Quantity)  AS INT)                      AS Units,
                         SUM (ord.BillableSqft * sl.Quantity)   AS BillableSqft,
                         SUM (sl.UnitPriceAmt * sl.Quantity)    AS SalesAmt
                     FROM
                         EBS.ScheduledOrders      ord WITH (NOLOCK)
                         INNER JOIN EBS.WorkOrder  wk WITH (NOLOCK)
                         ON  wk.OrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.GlassItem  gi WITH (NOLOCK)
                         ON  gi.ItemNum = ord.ItemNum
                         INNER JOIN EBS.RackDetail rd WITH (NOLOCK)
                         ON  rd.WOrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.SalesLine  sl WITH (NOLOCK)
                         ON  sl.OrderNum = rd.SOrderNum AND
                             sl.OrderType = rd.SOrderType AND
                             sl.OrderLineNum = rd.SOrderLineNum
                     WHERE
                         gi.StockType <> 'C' AND
                         ISNULL (ord.Interlayer1Code, '') <> ''
                     GROUP BY
                         ord.SoldToCustName,
                         ord.SoldToCustNum,
                         ord.ShipToCustName,
                         ord.ShipToCustNum,
                         gi.StockType,
                         REPLACE (ord.Interlayer1Code, '.', ''),
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         sl.InvoiceDate
                     UNION ALL
                     SELECT
                         ord.SoldToCustName                     AS SoldToCustName,
                         ord.SoldToCustNum                      AS SoldToCustNum,
                         ord.ShipToCustName                     AS ShipToCustName,
                         ord.ShipToCustNum                      AS ShipToCustNum,
                         gi.StockType                           AS StockType,
                         'HV'                                   AS VolumeType,
                         ord.RequestedDate,
                         wk.StatusCode,
                         '000'                                     AS LastStatus,
                         '000'                                     AS NextStatus,
                         NULL                                   AS InvoiceDate,
                         REPLACE (ord.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (ord.Quantity)  AS INT)                      AS Units,
                         SUM (ord.BillableSqft * ord.Quantity)  AS BillableSqft,
                         0                                      AS SalesAmt
                     FROM
                         EBS.ScheduledOrders      ord WITH (NOLOCK)
                         INNER JOIN EBS.WorkOrder  wk WITH (NOLOCK)
                         ON  wk.OrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.GlassItem  gi WITH (NOLOCK)
                         ON  gi.ItemNum = ord.ItemNum
                         INNER JOIN EBS.RackDetail rd WITH (NOLOCK)
                         ON  rd.WOrderNum = ord.WorkOrderNum
                     WHERE
                         gi.StockType <> 'C' AND
                         NOT EXISTS (
                                        SELECT
                                            *
                                        FROM
                                            EBS.SalesLine sl WITH (NOLOCK)
                                        WHERE
                                            sl.OrderNum = rd.SOrderNum AND
                                            sl.OrderType = rd.SOrderType AND
                                            sl.OrderLineNum = rd.SOrderLineNum
                                    )
                     GROUP BY
                         ord.SoldToCustName,
                         ord.SoldToCustNum,
                         ord.ShipToCustName,
                         ord.ShipToCustNum,
                         gi.StockType,
                         ord.RequestedDate,
                         wk.StatusCode,
                         REPLACE (ord.Interlayer1Code, '.', '')
                     UNION ALL
                     SELECT
                         ab1.Name                              AS SoldCustomerName,
                         sl.CustomerSoldNum                    AS SoldCustomerNum,
                         ab2.Name                              AS ShipToCustName,
                         sl.CustomerShipNum                    AS ShipToCustNum,
                         gi.StockType                          AS StockType,
                         sl.VolumeType                         AS VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900')  AS InvoiceDate,
                         REPLACE (su.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (sl.Quantity)  AS INT)                     AS Units,
                         SUM (su.BillableSqft * sl.Quantity)   AS BillableSqft,
                         SUM (sl.UnitPriceAmt * sl.Quantity)   AS SalesAmt
                     FROM
                         EBS.SalesLine               sl
                         INNER JOIN EBS.WorkOrder    wk WITH (NOLOCK)
                         ON  wk.OrderNum = sl.WOrderNum
                         INNER JOIN EBS.GlassItem    gi WITH (NOLOCK)
                         ON  gi.ItemNum = wk.ItemNum
                         INNER JOIN EBS.AddressBook  ab1 WITH (NOLOCK)
                         ON  ab1.AddressNum = sl.CustomerSoldNum
                         INNER JOIN EBS.AddressBook  ab2 WITH (NOLOCK)
                         ON  ab2.AddressNum = sl.CustomerShipNum
                         INNER JOIN EBS.vw_SpecUnits su WITH (NOLOCK)
                         ON  su.ConfigurationId = sl.ConfigurationId
                     WHERE
                         sl.LineType = 'W' AND
                         sl.OrderLineNum % 1 = 0 AND
                         su.UnitType = 'MONO LAMI' AND
                         NextStatus <> '999' AND
                         OrderType IN ( 'SO', 'ST' ) AND
                         NOT EXISTS (
                                        SELECT
                                            1
                                        FROM
                                            EBS.ScheduledOrders ord
                                        WHERE
                                            ord.WorkOrderNum = wk.OrderNum
                                    )
                     GROUP BY
                         sl.CustomerSoldNum,
                         ab1.Name,
                         sl.CustomerShipNum,
                         ab2.Name,
                         gi.StockType,
                         sl.VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900'),
                         REPLACE (su.Interlayer1Code, '.', '')
                 ) A
            WHERE
                {filterString}
                LastStatus <> '980' AND
                StatusCode NOT IN ( '98' )
            ORDER BY
                 1,
                 2,
                 3";
            return cmd;
        }

        public SqlCommand GetInterlayerSalesOCLG(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
            SELECT
                 *
            FROM (
                     SELECT
                         ord.SoldToCustName                     AS SoldToCustName,
                         ord.SoldToCustNum                      AS SoldToCustNum,
                         ord.ShipToCustName                     AS ShipToCustName,
                         ord.ShipToCustNum                      AS ShipToCustNum,
                         gi.StockType                           AS StockType,
                         CASE
                             WHEN ord.ScheduleType LIKE '%HV%MTO%' THEN
                                  'HV'
                             ELSE sl.VolumeType
                         END                                    AS VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                         REPLACE (ord.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (sl.Quantity)  AS INT)                       AS Units,
                         SUM (ord.BillableSqft * sl.Quantity)   AS BillableSqft,
                         SUM (sl.UnitPriceAmt * sl.Quantity)    AS SalesAmt
                     FROM
                         EBS.ScheduledOrders     ord WITH (NOLOCK)
                         INNER JOIN EBS.WorkOrder wk WITH (NOLOCK)
                         ON  wk.OrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.GlassItem gi WITH (NOLOCK)
                         ON  gi.ItemNum = ord.ItemNum
                         INNER JOIN EBS.SalesLine sl WITH (NOLOCK)
                         ON  sl.WOrderNum = ord.WorkOrderNum
                     WHERE
                         gi.StockType = 'C' AND
                         ISNULL (ord.Interlayer1Code, '') <> ''
                     GROUP BY
                         ord.SoldToCustName,
                         ord.SoldToCustNum,
                         ord.ShipToCustName,
                         ord.ShipToCustNum,
                         gi.StockType,
                         REPLACE (ord.Interlayer1Code, '.', ''),
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         sl.InvoiceDate,
                         CASE
                             WHEN ord.ScheduleType LIKE '%HV%MTO%' THEN
                                  'HV'
                             ELSE sl.VolumeType
                         END
                     UNION ALL
                     SELECT
                         ord.SoldToCustName                     AS SoldToCustName,
                         ord.SoldToCustNum                      AS SoldToCustNum,
                         ord.ShipToCustName                     AS ShipToCustName,
                         ord.ShipToCustNum                      AS ShipToCustNum,
                         gi.StockType                           AS StockType,
                         'HV'                                   AS VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900')   AS InvoiceDate,
                         REPLACE (ord.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (sl.Quantity)  AS INT)                      AS Units,
                         SUM (ord.BillableSqft * sl.Quantity)   AS BillableSqft,
                         SUM (sl.UnitPriceAmt * sl.Quantity)    AS SalesAmt
                     FROM
                         EBS.ScheduledOrders      ord WITH (NOLOCK)
                         INNER JOIN EBS.WorkOrder  wk WITH (NOLOCK)
                         ON  wk.OrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.GlassItem  gi WITH (NOLOCK)
                         ON  gi.ItemNum = ord.ItemNum
                         INNER JOIN EBS.RackDetail rd WITH (NOLOCK)
                         ON  rd.WOrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.SalesLine  sl WITH (NOLOCK)
                         ON  sl.OrderNum = rd.SOrderNum AND
                             sl.OrderType = rd.SOrderType AND
                             sl.OrderLineNum = rd.SOrderLineNum
                     WHERE
                         gi.StockType <> 'C' AND
                         ISNULL (ord.Interlayer1Code, '') <> ''
                     GROUP BY
                         ord.SoldToCustName,
                         ord.SoldToCustNum,
                         ord.ShipToCustName,
                         ord.ShipToCustNum,
                         gi.StockType,
                         REPLACE (ord.Interlayer1Code, '.', ''),
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         sl.InvoiceDate
                     UNION ALL
                     SELECT
                         ord.SoldToCustName                     AS SoldToCustName,
                         ord.SoldToCustNum                      AS SoldToCustNum,
                         ord.ShipToCustName                     AS ShipToCustName,
                         ord.ShipToCustNum                      AS ShipToCustNum,
                         gi.StockType                           AS StockType,
                         'HV'                                   AS VolumeType,
                         ord.RequestedDate,
                         wk.StatusCode,
                         '000'                                     AS LastStatus,
                         '000'                                     AS NextStatus,
                         NULL                                   AS InvoiceDate,
                         REPLACE (ord.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (ord.Quantity)  AS INT)                      AS Units,
                         SUM (ord.BillableSqft * ord.Quantity)  AS BillableSqft,
                         0                                      AS SalesAmt
                     FROM
                         EBS.ScheduledOrders      ord WITH (NOLOCK)
                         INNER JOIN EBS.WorkOrder  wk WITH (NOLOCK)
                         ON  wk.OrderNum = ord.WorkOrderNum
                         INNER JOIN EBS.GlassItem  gi WITH (NOLOCK)
                         ON  gi.ItemNum = ord.ItemNum
                         INNER JOIN EBS.RackDetail rd WITH (NOLOCK)
                         ON  rd.WOrderNum = ord.WorkOrderNum
                     WHERE
                         gi.StockType <> 'C' AND
                         NOT EXISTS (
                                        SELECT
                                            *
                                        FROM
                                            EBS.SalesLine sl WITH (NOLOCK)
                                        WHERE
                                            sl.OrderNum = rd.SOrderNum AND
                                            sl.OrderType = rd.SOrderType AND
                                            sl.OrderLineNum = rd.SOrderLineNum
                                    )
                     GROUP BY
                         ord.SoldToCustName,
                         ord.SoldToCustNum,
                         ord.ShipToCustName,
                         ord.ShipToCustNum,
                         gi.StockType,
                         ord.RequestedDate,
                         wk.StatusCode,
                         REPLACE (ord.Interlayer1Code, '.', '')
                     UNION ALL
                     SELECT
                         ab1.Name                              AS SoldCustomerName,
                         sl.CustomerSoldNum                    AS SoldCustomerNum,
                         ab2.Name                              AS ShipToCustName,
                         sl.CustomerShipNum                    AS ShipToCustNum,
                         gi.StockType                          AS StockType,
                         sl.VolumeType                         AS VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900')  AS InvoiceDate,
                         REPLACE (su.Interlayer1Code, '.', '') AS InterlayerCode,
                         CAST(SUM (sl.Quantity)  AS INT)                     AS Units,
                         SUM (su.BillableSqft * sl.Quantity)   AS BillableSqft,
                         SUM (sl.UnitPriceAmt * sl.Quantity)   AS SalesAmt
                     FROM
                         EBS.SalesLine               sl
                         INNER JOIN EBS.WorkOrder    wk WITH (NOLOCK)
                         ON  wk.OrderNum = sl.WOrderNum
                         INNER JOIN EBS.GlassItem    gi WITH (NOLOCK)
                         ON  gi.ItemNum = wk.ItemNum
                         INNER JOIN EBS.AddressBook  ab1 WITH (NOLOCK)
                         ON  ab1.AddressNum = sl.CustomerSoldNum
                         INNER JOIN EBS.AddressBook  ab2 WITH (NOLOCK)
                         ON  ab2.AddressNum = sl.CustomerShipNum
                         INNER JOIN EBS.vw_SpecUnits su WITH (NOLOCK)
                         ON  su.ConfigurationId = sl.ConfigurationId
                     WHERE
                         sl.LineType = 'W' AND
                         sl.OrderLineNum % 1 = 0 AND
                         su.UnitType = 'MONO LAMI' AND
                         NextStatus <> '999' AND
                         OrderType IN ( 'SO', 'ST' ) AND
                         NOT EXISTS (
                                        SELECT
                                            1
                                        FROM
                                            EBS.ScheduledOrders ord
                                        WHERE
                                            ord.WorkOrderNum = wk.OrderNum
                                    )
                     GROUP BY
                         sl.CustomerSoldNum,
                         ab1.Name,
                         sl.CustomerShipNum,
                         ab2.Name,
                         gi.StockType,
                         sl.VolumeType,
                         sl.RequestedDate,
                         wk.StatusCode,
                         sl.LastStatus,
                         sl.NextStatus,
                         NULLIF(sl.InvoiceDate, '01/01/1900'),
                         REPLACE (su.Interlayer1Code, '.', '')
                 ) A
            WHERE
                {filterString}
                LastStatus <> '980' AND
                StatusCode NOT IN ( '98' )
            ORDER BY
                 1,
                 2,
                 3";
            return cmd;
        }


        public SalesOrderLines GetInterlayerSales0(SqlConnection cnShopFloor, Plant plant, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, List<string> lastStatusCodes, List<string> nextStatusCodes)
        {
            var lines = new SalesOrderLines();
            var cmd = new SqlCommand();
            List<String> filters = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {
                // Dates
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) ||
                     !string.IsNullOrEmpty(dateCode))
                {
                    DateTime startDateValue;
                    DateTime endDateValue;

                    if (!string.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        startDateValue = dateValues[0];
                        endDateValue = dateValues[1];
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(startDate))
                        {
                            startDateValue = DateCodeManager.IsDate(startDate)
                                ? Convert.ToDateTime(startDate)
                                : DateCodeManager.GetDateCodeValue(startDate);
                        }
                        else
                        {
                            startDateValue = new DateTime(1900, 1, 1);
                        }


                        if (!string.IsNullOrEmpty(endDate))
                        {
                            endDateValue = DateCodeManager.IsDate(endDate)
                                ? Convert.ToDateTime(endDate)
                                : DateCodeManager.GetDateCodeValue(endDate);
                        }
                        else
                        {
                            endDateValue = new DateTime(2050, 12, 31);
                        }
                    }

                    if (nextStatusCodes.Any())
                    {
                        if (nextStatusCodes.Contains("999") && nextStatusCodes.Count == 1)
                        {
                            filters.Add("InvoiceDate BETWEEN @StartDate AND @EndDate AND\r\n");
                        }
                        else if (nextStatusCodes.Contains("999") && nextStatusCodes.Count > 1)
                        {
                            filters.Add("((NextStatus = '999' AND InvoiceDate BETWEEN @StartDate AND @EndDate) OR (NextStatus <> '999' AND RequestedDate BETWEEN @StartDate AND @EndDate)) AND\r\n");

                        }
                        else
                        {
                            filters.Add("RequestedDate BETWEEN @StartDate AND @EndDate AND\r\n");
                        }
                        parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date));
                        parameters.Last().Value = startDateValue;
                        parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date));
                        parameters.Last().Value = endDateValue;
                    }
                }

                // Customer Ship Num
                if (!String.IsNullOrEmpty(customerShipNums))
                {
                    var exclude = false;
                    List<decimal> customerShipNumList;
                    if (customerShipNums.Substring(0, 1) == "!")
                    {
                        exclude = true;
                        customerShipNumList = Array
                            .ConvertAll<string, decimal>(
                                customerShipNums.Substring(1, customerShipNums.Length - 1).Split(','),
                                Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerShipNumList =
                            Array.ConvertAll<string, decimal>(customerShipNums.Split(','), Convert.ToDecimal)
                                .ToList();
                    }

                    string strInClause = "";
                    foreach (decimal customerShip in customerShipNumList)
                    {
                        strInClause = strInClause + $"{customerShip.ToString()},";
                    }

                    filters.Add(
                        $"ShipToCustNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                // Customer Sold Num
                if (!String.IsNullOrEmpty(customerSoldNums))
                {
                    var exclude = false;
                    List<decimal> customerSoldNumList;
                    if (customerSoldNums.Substring(0, 1) == "!")
                    {
                        exclude = true;
                        customerSoldNumList = Array
                            .ConvertAll<string, decimal>(
                                customerSoldNums.Substring(1, customerSoldNums.Length - 1).Split(','),
                                Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerSoldNumList =
                            Array.ConvertAll<string, decimal>(customerSoldNums.Split(','), Convert.ToDecimal)
                                .ToList();
                    }

                    string strInClause = "";
                    foreach (decimal customerSold in customerSoldNumList)
                    {
                        strInClause = strInClause + $"{customerSold.ToString()},";
                    }

                    filters.Add(
                        $"SoldToCustNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} ) AND\r\n");
                }

                // Last Status
                if (lastStatusCodes.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string statusCode in lastStatusCodes)
                    {
                        if (statusCode == lastStatusCodes.First())
                        {
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append(",");
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                    }
                    filters.Add(
                        "LastStatus IN (" + sb + ") AND\r\n");
                }

                // Next Status
                if (nextStatusCodes.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string statusCode in nextStatusCodes)
                    {
                        if (statusCode == nextStatusCodes.First())
                        {
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append(",");
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                    }
                    filters.Add(
                        "NextStatus IN (" + sb + ") AND\r\n");
                }

                foreach (SqlParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = cnShopFloor;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = plant.PlantNum;
                        line.PlantCode = plant.PlantCode;
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SoldToCustNum"], 0);
                        line.CustomerName = CastDbNull.To(reader["SoldToCustName"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["ShipToCustNum"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["ShipToCustName"], string.Empty).Trim();
                        line.StockType = CastDbNull.To(reader["StockType"], string.Empty).Trim();
                        line.VolumeIndicator = CastDbNull.To(reader["VolumeType"], string.Empty).Trim();
                        line.LastStatus = CastDbNull.To(reader["LastStatus"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["NextStatus"], string.Empty).Trim();
                        line.RequestedDate = CastDbNull.To<DateTime>(reader["RequestedDate"], new DateTime(1900, 1, 1));
                        if (reader["InvoiceDate"] != DBNull.Value)
                            line.InvoiceDate = CastDbNull.To<DateTime>(reader["InvoiceDate"], new DateTime(1900, 1, 1));
                        line.InterlayerCode = CastDbNull.To(reader["InterlayerCode"], string.Empty).Trim();
                        line.Units = CastDbNull.To<int>(reader["Units"], 0);
                        line.TotalBillableSqft = CastDbNull.To<decimal>(reader["BillableSqft"], 0);
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SalesAmt"], 0);
                        lines.Add(line);
                    }

                    reader.Close();
                }
                _logger.Info("Lines Returned: " + lines.Count);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            _logger.Info("Return Lines");
            return lines;
        }

        public SalesOrderLines GetInterlayerSales(SqlConnection cnShopFloor, Plant plant, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, List<string> lastStatusCodes, List<string> nextStatusCodes)
        {
            var lines = new SalesOrderLines();
            SqlFilters filters = new SqlFilters();
            List<SqlParameter> parameters = new List<SqlParameter>();
            var cmd = GetInterlayerSales(filters);
            try
            {
                // Dates
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) ||
                     !string.IsNullOrEmpty(dateCode))
                {
                    DateTime startDateValue;
                    DateTime endDateValue;

                    if (!string.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        startDateValue = dateValues[0];
                        endDateValue = dateValues[1];
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(startDate))
                        {
                            startDateValue = DateCodeManager.IsDate(startDate)
                                ? Convert.ToDateTime(startDate)
                                : DateCodeManager.GetDateCodeValue(startDate);
                        }
                        else
                        {
                            startDateValue = new DateTime(1900, 1, 1);
                        }


                        if (!string.IsNullOrEmpty(endDate))
                        {
                            endDateValue = DateCodeManager.IsDate(endDate)
                                ? Convert.ToDateTime(endDate)
                                : DateCodeManager.GetDateCodeValue(endDate);
                        }
                        else
                        {
                            endDateValue = new DateTime(2050, 12, 31);
                        }
                    }

                    if (nextStatusCodes.Any())
                    {
                        if (nextStatusCodes.Contains("999") && nextStatusCodes.Count == 1)
                        {
                            filters.Add(new SqlFilter("InvoiceDate BETWEEN @StartDate AND @EndDate", "AND"));
                        }
                        else if (nextStatusCodes.Contains("999") && nextStatusCodes.Count > 1)
                        {
                            filters.Add(new SqlFilter("((NextStatus = '999' AND InvoiceDate BETWEEN @StartDate AND @EndDate) OR (NextStatus <> '999' AND RequestedDate BETWEEN @StartDate AND @EndDate))", "AND"));
                            
                        } else
                        {
                            filters.Add(new SqlFilter("RequestedDate BETWEEN @StartDate AND @EndDate", "AND"));
                        }
                        parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date));
                        parameters.Last().Value = startDateValue;
                        parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date));
                        parameters.Last().Value = endDateValue;
                    }
                }

                // Customer Ship Num
                if (!String.IsNullOrEmpty(customerShipNums))
                {
                    var exclude = false;
                    List<decimal> customerShipNumList;
                    if (customerShipNums.Substring(0, 1) == "!")
                    {
                        exclude = true;
                        customerShipNumList = Array
                            .ConvertAll<string, decimal>(
                                customerShipNums.Substring(1, customerShipNums.Length - 1).Split(','),
                                Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerShipNumList =
                            Array.ConvertAll<string, decimal>(customerShipNums.Split(','), Convert.ToDecimal)
                                .ToList();
                    }

                    string strInClause = "";
                    foreach (decimal customerShip in customerShipNumList)
                    {
                        strInClause = strInClause + $"{customerShip.ToString()},";
                    }
                    filters.Add(new SqlFilter($"ShipToCustNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} )", "AND"));
                }

                // Customer Sold Num
                if (!String.IsNullOrEmpty(customerSoldNums))
                {
                    var exclude = false;
                    List<decimal> customerSoldNumList;
                    if (customerSoldNums.Substring(0, 1) == "!")
                    {
                        exclude = true;
                        customerSoldNumList = Array
                            .ConvertAll<string, decimal>(
                                customerSoldNums.Substring(1, customerSoldNums.Length - 1).Split(','),
                                Convert.ToDecimal)
                            .ToList();
                    }
                    else
                    {
                        customerSoldNumList =
                            Array.ConvertAll<string, decimal>(customerSoldNums.Split(','), Convert.ToDecimal)
                                .ToList();
                    }

                    string strInClause = "";
                    foreach (decimal customerSold in customerSoldNumList)
                    {
                        strInClause = strInClause + $"{customerSold.ToString()},";
                    }
                    filters.Add(new SqlFilter($"SoldToCustNum {(exclude ? "NOT" : "")} IN ( {strInClause.Substring(0, strInClause.Length - 1)} )", "AND"));
                }

                // Last Status
                if (lastStatusCodes.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string statusCode in lastStatusCodes)
                    {
                        if (statusCode == lastStatusCodes.First())
                        {
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append(",");
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                    }
                    filters.Add(new SqlFilter("LastStatus IN (" + sb + ")", "AND"));
                }

                // Next Status
                if (nextStatusCodes.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string statusCode in nextStatusCodes)
                    {
                        if (statusCode == nextStatusCodes.First())
                        {
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append(",");
                            sb.Append("'");
                            sb.Append(statusCode);
                            sb.Append("'");
                        }
                    }
                    filters.Add(new SqlFilter("NextStatus IN (" + sb + ")", "AND"));
                }

                foreach (SqlParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = cnShopFloor;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = plant.PlantNum;
                        line.PlantCode = plant.PlantCode;
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SoldToCustNum"], 0);
                        line.CustomerName = CastDbNull.To(reader["SoldToCustName"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["ShipToCustNum"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["ShipToCustName"], string.Empty).Trim();
                        line.StockType = CastDbNull.To(reader["StockType"], string.Empty).Trim();
                        line.VolumeIndicator = CastDbNull.To(reader["VolumeType"], string.Empty).Trim();
                        line.LastStatus = CastDbNull.To(reader["LastStatus"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["NextStatus"], string.Empty).Trim();
                        line.RequestedDate = CastDbNull.To<DateTime>(reader["RequestedDate"], new DateTime(1900, 1, 1));
                        if (reader["InvoiceDate"] != DBNull.Value)
                            line.InvoiceDate = CastDbNull.To<DateTime>(reader["InvoiceDate"], new DateTime(1900, 1, 1));
                        line.InterlayerCode = CastDbNull.To(reader["InterlayerCode"], string.Empty).Trim();
                        line.Units = CastDbNull.To<int>(reader["Units"], 0);
                        line.TotalBillableSqft = CastDbNull.To<decimal>(reader["BillableSqft"], 0);
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SalesAmt"], 0);
                        lines.Add(line);
                    }

                    reader.Close();
                }
                _logger.Info("Lines Returned: " + lines.Count);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            _logger.Info("Return Lines");
            return lines;
        }

    }




}
