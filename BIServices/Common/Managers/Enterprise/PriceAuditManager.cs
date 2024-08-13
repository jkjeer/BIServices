using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.E1;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.Enterprise
{
    public class PriceAuditManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private FlexItemManager _flexItemMgr;
        private ItemAttributeManager _itemAttMgr;
        private UdcItemManager _udcItemMgr;

        public PriceAuditManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            _udcItemMgr = new UdcItemManager(session);
            _flexItemMgr = new FlexItemManager(session, "P5842300");
        }

        /*********************************************************************************
          * Missing Bars
          *******************************************************************************/
        public iDB2Command GetMissingBarOrders(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT * FROM (
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    PAZ58PRLV,
    PADSCA,
    PAZ58PRTY,
    COALESCE(PALPRC, 0) AS PALPRC,
    COALESCE(PAUPRC, 0) AS PAUPRC,
    PAUOM,
    CMCFGID,
    ROW_NUMBER() OVER (PARTITION BY
                           SDKCOO,
                           SDDOCO,
                           SDDCTO,
                           SDLNID
                       ORDER BY
                           PALPRC DESC
                      ) AS RN
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    LEFT OUTER JOIN F580124
    ON  PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (4200, 4205, 4210, 4211, 4215, 4216, 4220, 4221, 4226, 4230, 4231, 4236, 4240, 4241, 4246, 4250,
                      4280, 4290
                     )
WHERE
    {filterString}
    SDUPRC > 0 AND
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
    SAZ58M1A1 <> 0 AND
    NOT EXISTS
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (375,380,381,396,398)
) AND 
    EXISTS 
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID 
)
) A
WHERE 
	A.RN = 1 and
    PAUPRC / 10000 <= 0.50 and
	MOD(SDLNID/1000, 1) = 0
FOR READ ONLY";
            return cmd;
        }
        public iDB2Command GetMissingBarOrdersAir1(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT * FROM (
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    PAZ58PRLV,
    PADSCA,
    PAZ58PRTY,
    COALESCE(PALPRC, 0) AS PALPRC,
    COALESCE(PAUPRC, 0) AS PAUPRC,
    PAUOM,
    CMCFGID,
    ROW_NUMBER() OVER (PARTITION BY
                           SDKCOO,
                           SDDOCO,
                           SDDCTO,
                           SDLNID
                       ORDER BY
                           PALPRC DESC
                      ) AS RN
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    LEFT OUTER JOIN F580124
    ON  PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (4200, 4205, 4210, 4211, 4215, 4216, 4230, 4231, 4236, 4250, 4280)
WHERE
    {filterString}
    SDUPRC > 0 AND
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
    SAZ58M1A1 <> 0 AND
    NOT EXISTS
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (375,380,381,396,398)
) AND 
    EXISTS 
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID 
)
) A
WHERE 
	A.RN = 1 and
    PAUPRC / 10000 <= 0.50 and
	MOD(SDLNID/1000, 1) = 0
FOR READ ONLY";
            return cmd;
        }

        public iDB2Command GetMissingBarOrdersAir2(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT * FROM (
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    CAST(KZ1.KZSGVL AS DECIMAL(5,0)) AS SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    PAZ58PRLV,
    PADSCA,
    PAZ58PRTY,
    COALESCE(PALPRC, 0) AS PALPRC,
    COALESCE(PAUPRC, 0) AS PAUPRC,
    PAUOM,
    CMCFGID,
    ROW_NUMBER() OVER (PARTITION BY
                           SDKCOO,
                           SDDOCO,
                           SDDCTO,
                           SDLNID
                       ORDER BY
                           PALPRC DESC
                      ) AS RN
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    INNER JOIN F580124
    ON  PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (4200, 4205, 4210, 4211, 4215, 4216, 4230, 4231, 4236, 4250, 4280,4220, 4221, 4226, 4240, 4241, 4246, 4290)
	INNER JOIN F3211 KZ1
	ON	KZ1.KZCFGID = CMCFGID AND
		KZ1.KZPEL = 5760 AND
		KZ1.KZSGVL NOT IN ('','0','00000')
WHERE
    {filterString}
    SDUPRC > 0 AND
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
	COALESCE(KZ1.KZSGVL, '')  NOT IN ('','0','00000') AND
    NOT EXISTS
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (375,380,381,396,398)
) AND 
    EXISTS 
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID 
)
) A
WHERE 
	A.RN = 1 and
    PAUPRC / 10000 <= 0.50 and
	MOD(SDLNID/1000, 1) = 0
FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetMissingBarOrders(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR < '570') OR (SDTRDJ = ? AND SDNXTR < '570')) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                cmd = GetMissingBarOrders(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.BarPriceLevel = CastDbNull.To<decimal>(reader["PAZ58PRLV"], 0);
                        line.BarPriceDesc = CastDbNull.To(reader["PADSCA"], string.Empty).Trim();
                        line.UnitBarPrice = CastDbNull.To<decimal>(reader["PAUPRC"], 0) / 10000;
                        line.TotalBarPrice = CastDbNull.To<decimal>(reader["PALPRC"], 0) / 10000;
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing Bar Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }

                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.OpeningsWide = itemAtts.GetDecimalAttributeValue("Z58OWA1");
                    line.OpeningsHigh = itemAtts.GetDecimalAttributeValue("Z58OHA1");
                    line.OpeningsOdd = itemAtts.GetDecimalAttributeValue("Z58OOA1");
                    line.OpeningsRect = itemAtts.GetDecimalAttributeValue("Z5805380");
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        public SalesOrderLines GetMissingBarOrdersAir1(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR < '570') OR (SDTRDJ = ? AND SDNXTR < '570')) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                cmd = GetMissingBarOrdersAir1(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.BarPriceLevel = CastDbNull.To<decimal>(reader["PAZ58PRLV"], 0);
                        line.BarPriceDesc = CastDbNull.To(reader["PADSCA"], string.Empty).Trim();
                        line.UnitBarPrice = CastDbNull.To<decimal>(reader["PAUPRC"], 0) / 10000;
                        line.TotalBarPrice = CastDbNull.To<decimal>(reader["PALPRC"], 0) / 10000;
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing Bar Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }

                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.OpeningsWide = itemAtts.GetDecimalAttributeValue("Z58OWA1");
                    line.OpeningsHigh = itemAtts.GetDecimalAttributeValue("Z58OHA1");
                    line.OpeningsOdd = itemAtts.GetDecimalAttributeValue("Z58OOA1");
                    line.OpeningsRect = itemAtts.GetDecimalAttributeValue("Z5805380");
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        public SalesOrderLines GetMissingBarOrdersAir2(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR < '570') OR (SDTRDJ = ? AND SDNXTR < '570')) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                cmd = GetMissingBarOrdersAir2(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.BarPriceLevel = CastDbNull.To<decimal>(reader["PAZ58PRLV"], 0);
                        line.BarPriceDesc = CastDbNull.To(reader["PADSCA"], string.Empty).Trim();
                        line.UnitBarPrice = CastDbNull.To<decimal>(reader["PAUPRC"], 0) / 10000;
                        line.TotalBarPrice = CastDbNull.To<decimal>(reader["PALPRC"], 0) / 10000;
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing Bar Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }

                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.OpeningsWide = itemAtts.GetDecimalAttributeValue("Z58OWA1");
                    line.OpeningsHigh = itemAtts.GetDecimalAttributeValue("Z58OHA1");
                    line.OpeningsOdd = itemAtts.GetDecimalAttributeValue("Z58OOA1");
                    line.OpeningsRect = itemAtts.GetDecimalAttributeValue("Z5805380");
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        /************************************************************************
        * Unit Pricing
        ************************************************************************/
        public iDB2Command GetMissingUnitPricing(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    CMCFGID
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
WHERE
    {filterString}
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
    SDAEXP = 0 and
	MOD(SDLNID, 1) = 0
FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetMissingUnitPricing(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR <= '570') OR (SDTRDJ = ? AND SDNXTR IN ('540','545'))) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                // Command
                cmd = GetMissingUnitPricing(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing Unit Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }

                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        /************************************************************************
        * I89 Pricing
        ************************************************************************/
        public iDB2Command GetMissingI89Pricing(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT * FROM (
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    PAZ58PRLV,
    PADSCA,
    PAZ58PRTY,
    COALESCE(PALPRC, 0) AS PALPRC,
    COALESCE(PAUPRC, 0) AS PAUPRC,
    PAUOM,
    CMCFGID,
    ROW_NUMBER() OVER (PARTITION BY
                           SDKCOO,
                           SDDOCO,
                           SDDCTO,
                           SDLNID
                       ORDER BY
                           PALPRC DESC
                      ) AS RN
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    LEFT OUTER JOIN F580124
    ON  PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (4010, 4020, 4030, 4040, 4050, 4150, 4160) AND
        (
            PADSCA LIKE '%81%' OR
            PADSCA LIKE '%89%'
        )
WHERE
    {filterString}
	MOD(SDLNID/1000, 1) = 0 AND
    SDUPRC > 0 AND
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
    SAZ58L24C LIKE '%I89%' AND
    NOT EXISTS
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
         (
             (
                 PAZ58PRLV BETWEEN 390 AND 399 AND
                 (
                     PADSCA LIKE '%89%' OR
                     PADSCA LIKE '%81%'
                 )
             ) OR
             (
                PAZ58PRLV IN (375,380,381,396,398)
            )
         )
) AND 
    EXISTS 
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID 
)
) A
WHERE 
	A.RN = 1 and
    PAUPRC = 0 
FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetMissingI89Pricing(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR <= '570') OR (SDTRDJ = ? AND SDNXTR IN ('540','545'))) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                // Command
                cmd = GetMissingI89Pricing(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing I89 Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }
                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        /************************************************************************
        * 366 Pricing
        ************************************************************************/
        public iDB2Command GetMissing366Pricing(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT * FROM (
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    PAZ58PRLV,
    PADSCA,
    PAZ58PRTY,
    COALESCE(PALPRC, 0) AS PALPRC,
    COALESCE(PAUPRC, 0) AS PAUPRC,
    PAUOM,
    CMCFGID,
    ROW_NUMBER() OVER (PARTITION BY
                           SDKCOO,
                           SDDOCO,
                           SDDCTO,
                           SDLNID
                       ORDER BY
                           PALPRC DESC
                      ) AS RN
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    LEFT OUTER JOIN F580124
    ON  PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (4010, 4020, 4030, 4040, 4050, 4060, 4150, 4160) 
WHERE
    {filterString}
	MOD(SDLNID/1000, 1) = 0 AND
    SDUPRC > 0 AND
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
	SAZ58L2TK > 0 AND
		(
			SAZ58L11C LIKE '%366%' OR 
			SAZ58L12C LIKE '%366%' OR 
			SAZ58L23C LIKE '%366%' OR 
			SAZ58L24C LIKE '%366%' OR
			SAZ58L35C LIKE '%366%' OR 
			SAZ58L36C LIKE '%366%'  
		) AND
    NOT EXISTS
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
            (
                PAZ58PRLT = '9' OR
                (PAZ58PRLV IN (300,374,390,391,392,393,394,395,398,399,401,402) AND PADSCA LIKE '%366%') OR 
				(PAZ58PRLV IN (399,402) AND (PADSCA LIKE '%XL%' OR PADSCA LIKE '%XM%' OR PADSCA LIKE '%E66%' OR PADSCA LIKE '%Q66%')) OR
				(PAZ58PRLV IN (300) AND (PADSCA LIKE '%XL%' OR PADSCA LIKE '%XM%' OR PADSCA LIKE '%E66%' OR PADSCA LIKE '%Q66%')) OR
				(PAZ58PRLV IN (375,377,380,381,396,397))
			)
) AND 
    EXISTS 
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
		PAZ58PRLV NOT IN (920,976)
)
) A
WHERE 
	A.RN = 1 AND
    PAUPRC = 0 
FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetMissing366Pricing(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR <= '570') OR (SDTRDJ = ? AND SDNXTR IN ('540','545'))) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                // Command
                cmd = GetMissing366Pricing(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing 366 Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }

                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

        /************************************************************************
        * 366 Pricing
        ************************************************************************/
        public iDB2Command GetMissing180Pricing(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $@"
SELECT * FROM (
SELECT
    SDMCU,
    SDAN8,
    ABALPH,
    SDDOCO,
    SDDCTO,
    SDLNID,
	SDLNTY,
    SDRORN,
    SDRCTO,
    SDTRDJ,
	SDDRQJ,
    SDLTTR,
    SDNXTR,
    SDRCD,
    SDPROV,
	ATZ58ZPO,
    TRIM(SDDSC1) CONCAT ' ' CONCAT TRIM(SDDSC2) CONCAT ' ' CONCAT TRIM(SDCATNM) AS SDDSC,
	SDUORG,
    SDAEXP,
    SDUPRC,
	SAZ58SHAPE,
    SAZ58USF,
    SAZ58M1A1,
	SAZ58GSS1,
	SAZ58L24C,
    PAZ58PRLV,
    PADSCA,
    PAZ58PRTY,
    COALESCE(PALPRC, 0) AS PALPRC,
    COALESCE(PAUPRC, 0) AS PAUPRC,
    PAUOM,
    CMCFGID,
    ROW_NUMBER() OVER (PARTITION BY
                           SDKCOO,
                           SDDOCO,
                           SDDCTO,
                           SDLNID
                       ORDER BY
                           PALPRC DESC
                      ) AS RN
FROM
    F4211
    INNER JOIN F0101
    ON  ABAN8 = SDAN8
	INNER JOIN F580032
    ON  ATKCOO = SDKCOO AND
        ATDOCO = SDDOCO AND
        ATDCTO = SDDCTO AND
        ATLNID = SDLNID
    INNER JOIN F5834A15
    ON  SAKCOO = SDKCOO AND
        SADOCO = SDDOCO AND
        SADCTO = SDDCTO AND
        SALNID = SDLNID
    INNER JOIN F3201
    ON  CMKCOO = SDKCOO AND
        CMDOCO = SDDOCO AND
        CMDCTO = SDDCTO AND
        CMLNID = SDLNID
    LEFT OUTER JOIN F580124
    ON  PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
        PAZ58PRLV IN (4010, 4020, 4030, 4040, 4050, 4060, 4150, 4160) 
WHERE
    {filterString}
	MOD(SDLNID/1000, 1) = 0 AND
    SDUPRC > 0 AND
	SDLTTR <> '980' AND
    SDGLC IN ('GLAF','GLAS') AND
    SDDCTO = 'SO' AND
    ATZ58ZPO = 'N' AND
    SDLNTY = 'W' AND
	SAZ58L2TK > 0 AND
		(
			SAZ58L11C LIKE '%180%' OR 
			SAZ58L12C LIKE '%180%' OR 
			SAZ58L23C LIKE '%180%' OR 
			SAZ58L24C LIKE '%180%' OR
			SAZ58L35C LIKE '%180%' OR 
			SAZ58L36C LIKE '%180%'  
		) AND
    NOT EXISTS
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
            (
                (PAZ58PRLV IN (300,374,390,391,392,393,394,395,398,399,401) AND PADSCA LIKE '%180%') OR 
				(PAZ58PRLV IN (375,377,380,381,396,397))
			)
) AND 
    EXISTS 
(
    SELECT
        1
    FROM
        F580124
    WHERE
        PAKCOO = SDKCOO AND
        PADOCO = SDDOCO AND
        PADCTO = SDDCTO AND
        PALNID = SDLNID AND
		PAZ58PRLV NOT IN (920,976)
)
) A
WHERE 
	A.RN = 1 AND
    PAUPRC = 0 
FOR READ ONLY";
            return cmd;
        }

        public SalesOrderLines GetMissing180Pricing(BusinessSystemSession session, string plants)
        {
            var lines = new SalesOrderLines();
            iDB2Command cmd;
            List<string> filters = new List<string>();
            List<iDB2Parameter> parameters = new List<iDB2Parameter>();
            try
            {

                // Plant List
                if (!string.IsNullOrEmpty(plants))
                {
                    var plantsList = plants.Replace("'", "").Split(',').ToList();
                    string strInClause = "";
                    foreach (string plantItem in plantsList)
                    {
                        var plant = PlantManager.GetPlant(plantItem);
                        strInClause = strInClause + "?,";
                        parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
                        parameters.Last().Value = plant.PlantNum.PadLeft(12);
                    }
                    filters.Add("SDMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                // Order Date
                filters.Add("((SDTRDJ < ? AND SDNXTR <= '570') OR (SDTRDJ = ? AND SDNXTR IN ('540','545'))) AND\r\n");

                parameters.Add(new iDB2Parameter("StartDate1", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                parameters.Add(new iDB2Parameter("StartDate2", iDB2DbType.iDB2Decimal));
                parameters.Last().Precision = 6;
                parameters.Last().Scale = 0;
                parameters.Last().Value = E1TimeManager.CastToE1DateId(DateTime.Now);

                // Command
                cmd = GetMissing180Pricing(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                int i = 0;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["ABALPH"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.ItemDesc1 = CastDbNull.To(reader["SDDSC"], string.Empty).Trim();
                        line.ShapeNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SAZ58SHAPE"], string.Empty).Trim());
                        line.TotalActualSqft = CastDbNull.To<decimal>(reader["SAZ58USF"], 0) / 10000 * CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.MCode = CastDbNull.To<decimal>(reader["SAZ58M1A1"], 0);
                        line.GridStyle = CastDbNull.To(reader["SAZ58GSS1"], string.Empty).Trim();
                        line.Surface4Coating = CastDbNull.To(reader["SAZ58L24C"], string.Empty).Trim();
                        line.ConfiguratorId = CastDbNull.To<decimal>(reader["CMCFGID"], 0);
                        line.PriceAuditDesc = "Missing 180 Pricing";
                        lines.Add(line);
                        i++;
                    }
                    reader.Close();
                }

                _logger.Info($"{i} Rows");

                foreach (var line in lines)
                {
                    line.MCodeDesc = _udcItemMgr.GetBarDesc(session, line.MCode);
                    var itemAtts = _itemAttMgr.GetConfiguredItemAttributes(session, line.ConfiguratorId);
                    line.TotalBillableSqft = itemAtts.GetDecimalAttributeValue("Z58BSF") * line.OrderQty;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }

    }
}
