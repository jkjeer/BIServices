using System;
using System.Collections.Generic;
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
    public static class StaleOrderManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /*********************************************************************************
          * Stock Orders
          *******************************************************************************/
        public static iDB2Command GetStaleOrders(List<string> filters)
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
	            AB1.ABALPH AS SDAN8ALPH,
	            SDSHAN,
	            AB2.ABALPH AS SDSHANALPH,
	            SDDRQJ,
                SDTRDJ,
	            SDIVD,
	            SDLTTR,
	            SDNXTR,
                SDKCOO,
	            SDDOCO,
	            SDDCTO,
	            SDLNID,
                SDRORN,
	            SDVR01,
	            SDURCD,
	            SDITM,
	            SDUORG,
                SDUNCS,
                SDECST,
                SDUPRC,
                SDAEXP,
                SDRCD,
	            ATZ58PT,
	            ATZ58CP2,
	            ATZ58CP3,
	            ATZ58CP4,
	            ATZ58LPR,
	            ATZ58RT,
                ATZ5840460,
	            IBSTKT,
	            IBGLPT,
                IBSRP4,
	            IMDSC1,
	            IMDSC2,
                WASRST
            FROM
	            F4211
	            INNER JOIN F4201
	            ON	SHDOCO = SDDOCO AND
		            SHDCTO = SDDCTO
	            INNER JOIN F580032
	            ON	ATDOCO = SDDOCO AND
		            ATDCTO = SDDCTO AND
		            ATLNID = SDLNID
	            INNER JOIN F0101 AB1
	            ON	AB1.ABAN8 = SDAN8	            
                INNER JOIN F0101 AB2
	            ON	AB2.ABAN8 = SDSHAN
                INNER JOIN F4102
                ON  IBMCU = SDMCU AND
                    IBITM = SDITM
                INNER JOIN F4101
                ON  IMITM = IBITM
	            INNER JOIN F4801
	            ON  WARORN = SDDOCO AND
		            WALNID = SDLNID AND
		            WARCTO = SDDCTO AND
		            WARORN != ''''
            WHERE
                {filterString}
                WASRST IN ('98','99') AND
                SDLNTY = 'W' AND
	            SDNXTR < ('999') AND
                SDIVD = 0 AND
	            SDDCTO IN ('SO','ST') AND
                SDGLC IN ('GLAS','GLAF','GLAX','GLAZ')
            ORDER BY
	            1,2
            FOR READ ONLY";
            return cmd;
        }

        public static SalesOrderLines GetStaleOrders(BusinessSystemSession session, string plants, string dateProperty, string dateCode, string startDate, string endDate)
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

                // Dates
                if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate) || !string.IsNullOrEmpty(dateProperty) || !string.IsNullOrEmpty(dateCode))
                {
                    DateTime startDateValue;
                    DateTime endDateValue;

                    if (!string.IsNullOrEmpty(dateCode))
                    {
                        var dateValues = DateCodeManager.GetDateCodeRange(dateCode);
                        //startDateValue = dateValues[0];
                        startDateValue = new DateTime(1900, 1, 1);
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

                    // Date Field
                    if (!string.IsNullOrEmpty(dateProperty))
                    {
                        switch (dateProperty.ToUpper())
                        {
                            case "DUE":
                            case "REQ":
                            case "REQUEST":
                            case "REQUESTED":
                                filters.Add("SDDRQJ >= ? AND\r\n");
                                filters.Add("SDDRQJ <= ? AND\r\n");
                                break;
                            case "ORD":
                            case "ORDER":
                            case "ORDERED":
                                filters.Add("SDTRDJ >= ? AND\r\n");
                                filters.Add("SDTRDJ <= ? AND\r\n");
                                break;
                            default:
                                filters.Add("SDTRDJ >= ? AND\r\n");
                                filters.Add("SDTRDJ <= ? AND\r\n");
                                break;
                        }
                    }
                    else
                    {
                        filters.Add("SDDRQJ >= ? AND\r\n");
                        filters.Add("SDDRQJ <= ? AND\r\n");
                    }

                    parameters.Add(new iDB2Parameter("StartDate", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Utilities.Common.CastToE1Date(startDateValue);

                    parameters.Add(new iDB2Parameter("EndDate", iDB2DbType.iDB2Decimal));
                    parameters.Last().Precision = 6;
                    parameters.Last().Scale = 0;
                    parameters.Last().Value = Utilities.Common.CastToE1Date(endDateValue);
                }

                // Command
                cmd = GetStaleOrders(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                _logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        var line = new SalesOrderLine();
                        line.PlantNum = CastDbNull.To(reader["SDMCU"], string.Empty).Trim();
                        line.PlantCode = PlantManager.GetPlant(line.PlantNum).PlantCode;
                        line.WorkOrderNum = Utilities.Common.CastStringToDecimal(CastDbNull.To(reader["SDRORN"], string.Empty).Trim());
                        line.WorkOrderStatus = CastDbNull.To(reader["WASRST"], string.Empty).Trim();
                        line.OrderDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDTRDJ"], 0));
                        line.RequestedDate = E1TimeManager.CastToDateTime(CastDbNull.To<decimal>(reader["SDDRQJ"], 0));
                        line.ReasonCode = CastDbNull.To(reader["SDRCD"], string.Empty).Trim();
                        //line.PurchaseDate = E1TimeManager.CastToDateTime(CastDbNull.To<Decimal>(reader["ROUPMJ"], 0));
                        line.OrderNum = CastDbNull.To<decimal>(reader["SDDOCO"], 0);
                        line.OrderType = CastDbNull.To(reader["SDDCTO"], string.Empty).Trim();
                        line.OrderLineNum = CastDbNull.To<decimal>(reader["SDLNID"], 0) / 1000;
                        line.LastStatus = CastDbNull.To(reader["SDLTTR"], string.Empty).Trim();
                        line.NextStatus = CastDbNull.To(reader["SDNXTR"], string.Empty).Trim();
                        line.CustomerNum = CastDbNull.To<decimal>(reader["SDAN8"], 0);
                        line.CustomerName = CastDbNull.To(reader["SDAN8ALPH"], string.Empty).Trim();
                        line.CustomerShipNum = CastDbNull.To<decimal>(reader["SDSHAN"], 0);
                        line.CustomerShipName = CastDbNull.To(reader["SDSHANALPH"], string.Empty).Trim();
                        line.CustomerPart1 = CastDbNull.To(reader["ATZ58CP2"], string.Empty).Trim();
                        line.CustomerPart2 = CastDbNull.To(reader["ATZ58CP3"], string.Empty).Trim();
                        line.CustomerPart3 = CastDbNull.To(reader["ATZ58CP4"], string.Empty).Trim();
                        line.CustomerPO = CastDbNull.To(reader["SDVR01"], string.Empty).Trim();
                        line.ProductType = CastDbNull.To(reader["ATZ58PT"], string.Empty).Trim();
                        line.OrderQty = CastDbNull.To<decimal>(reader["SDUORG"], 0) / 1000;
                        line.UnitCost = CastDbNull.To<decimal>(reader["SDUNCS"], 0) / 10000;
                        line.TotalCost = CastDbNull.To<decimal>(reader["SDECST"], 0) / 100;
                        line.UnitPrice = CastDbNull.To<decimal>(reader["SDUPRC"], 0) / 10000;
                        line.TotalPrice = CastDbNull.To<decimal>(reader["SDAEXP"], 0) / 100;
                        line.LitesPerRack = CastDbNull.To<decimal>(reader["ATZ58LPR"], 0);
                        line.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                        line.ItemNum = CastDbNull.To<decimal>(reader["SDITM"], 0);
                        line.ItemDesc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        line.ItemDesc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        line.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                        line.GlCategory = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                        line.OrderedRacksQty = (line.LitesPerRack > 0) ? (int) Math.Ceiling(line.OrderQty / line.LitesPerRack) : 0;
                        line.HvLv = CastDbNull.To(reader["ATZ5840460"], string.Empty).Trim();
                        lines.Add(line);
                    }

                    reader.Close();
                    _logger.Info($"{i} Rows");
                    _logger.Info("End Reader");
                }

                //  foreach (var line in lines)
                //  {
                    //var itemAtts = _itemAttMgr.GetItemAttributes(session, line.ItemNum, 0);
                    //line.ItemAttributes = itemAtts;
                    //line.Lite1Thickness = itemAtts.GetDecimalAttributeValue("Z58L1TK");
                    //line.TotalActualSqft = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z58USF");
                    //line.TotalBillableSqft = line.OrderQty * itemAtts.GetDecimalAttributeValue("Z58BSF");
                    //line.TotalGlassWeight = itemAtts.GetDecimalAttributeValue("Z58GLWT");
                    //line.Construction = _flexItemMgr.GetUnitConstruction(itemAtts);
                    //line.Dimension = _flexItemMgr.GetDimension(itemAtts);
                    //line.Lite1GlassType = _flexItemMgr.GetLite1GlassType(itemAtts);
                    //line.Lite2GlassType = _flexItemMgr.GetLite2GlassType(itemAtts);
                    //line.Lite3GlassType = _flexItemMgr.GetLite3GlassType(itemAtts);
                    //line.LiteRouting = _flexItemMgr.GetLiteRouting(itemAtts);
                    //line.Airspace = _flexItemMgr.GetAirspace(itemAtts, true);
                    //line.SpacerType = _flexItemMgr.GetSpacerType(itemAtts, true);
                    //line.BarAssembly = _flexItemMgr.GetBarAssembly(itemAtts, true);
                    //line.Grid = _flexItemMgr.GetGrid(itemAtts, true);
                    //line.GridIndexes = _flexItemMgr.GetGridIndexes(itemAtts);
                    //line.Dimensions = _flexItemMgr.GetDimension(itemAtts);
                    //line.Logo = _flexItemMgr.GetLogo(itemAtts);
                    //line.PreserveProcessing = _flexItemMgr.GetPreserveProcessing(itemAtts, true);
                    //line.EdgeProcessing = _flexItemMgr.GetEdgeProcessing(itemAtts, true);
                    //line.HoleProcessing = _flexItemMgr.GetHoleProcessing(itemAtts, true);
                    //line.ConfigHash = _flexItemMgr.GetHashValue(itemAtts);

                //    // New Metrics
                //    var lite1Routing = 0;
                //    var lite2Routing = 0;
                //    var lite3Routing = 0;
                //    Int32.TryParse(line.Lite1Routing, out lite1Routing);
                //    Int32.TryParse(line.Lite2Routing, out lite2Routing);
                //    Int32.TryParse(line.Lite3Routing, out lite3Routing);
                //    line.MonoQty = 0;
                //    line.MonoSqft = 0;
                //    line.DoubleQty = 0;
                //    line.DoubleSqft = 0;
                //    line.TripleQty = 0;
                //    line.TripleSqft = 0;
                //    line.TemperedSqft = 0;
                //    line.CutSqft = 0;
                //    switch (line.UnitType)
                //    {
                //        case "1":
                //        case "L":
                //            line.MonoQty = line.OrderQty;
                //            line.MonoSqft = line.TotalActualSqft;
                //            if (line.Lite1GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TotalActualSqft;
                //            if (lite1Routing >= 10 && lite1Routing <= 20) line.CutSqft = line.TotalActualSqft;
                //            break;
                //        case "2":
                //            line.DoubleQty = line.OrderQty;
                //            line.DoubleSqft = line.TotalActualSqft;
                //            if (line.Lite1GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TotalActualSqft;
                //            if (line.Lite2GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TemperedSqft + line.TotalActualSqft;
                //            if (lite1Routing >= 10 && lite1Routing <= 20) line.CutSqft = line.TotalActualSqft;
                //            if (lite2Routing >= 10 && lite2Routing <= 20) line.CutSqft = line.CutSqft + line.TotalActualSqft;
                //            break;
                //        case "3":
                //            line.TripleQty = line.OrderQty;
                //            line.TripleSqft = line.TotalActualSqft;
                //            if (line.Lite1GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TotalActualSqft;
                //            if (line.Lite2GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TemperedSqft + line.TotalActualSqft;
                //            if (line.Lite3GlassType.Contains(":T:") || line.Lite1GlassType.Contains(":H:")) line.TemperedSqft = line.TemperedSqft + line.TotalActualSqft;
                //            if (lite1Routing >= 10 && lite1Routing <= 20) line.CutSqft = line.TotalActualSqft;
                //            if (lite2Routing >= 10 && lite2Routing <= 20) line.CutSqft = line.CutSqft + line.TotalActualSqft;
                //            if (lite3Routing >= 10 && lite3Routing <= 20) line.CutSqft = line.CutSqft + line.TotalActualSqft;
                //            break;
                //    }
                //}

                //_logger.Info("End Attributes");



                //foreach (var line in lines)
                //{
                //    var configLines = lines.Where(x => x.ConfigHash == line.ConfigHash);
                //    var configLinesList = configLines as IList<SalesOrderLine> ?? configLines.ToList();
                //    var cfgAgg = new ConfigAggregate();
                //    cfgAgg.ConfigHashCount = configLinesList.Count();
                //    cfgAgg.Lite1TotalWeightAggregate = configLinesList.Sum(x => x.Lite1TotalWeight);
                //    cfgAgg.Lite2TotalWeightAggregate = configLinesList.Sum(x => x.Lite2TotalWeight);
                //    cfgAgg.Lite3TotalWeightAggregate = configLinesList.Sum(x => x.Lite3TotalWeight);
                //    cfgAgg.TotalGlassWeightAggregate = configLinesList.Sum(x => x.TotalGlassWeight);
                //    cfgAgg.AllLitesPurchased = configLinesList.Min(x => x.LitesPurchased);
                //    line.ConfigAggregate = cfgAgg;
                //}
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return lines;
        }


    }
}
