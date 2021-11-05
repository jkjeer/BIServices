using System;
using System.Collections.Generic;
using System.Data;
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
    public class DiscreteItemManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ItemAttributeManager _itemAttMgr;

        public DiscreteItemManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("DiscreteItemManager.GetStandardCost", GetStandardCost());
            hash.Add("DiscreteItemManager.GetUomValue", GetUomValue());
            hash.Add("DiscreteItemManager.GetBomSqft", GetBomSqft());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
            _itemAttMgr = new ItemAttributeManager(session);
            new FlexItemManager(session, "P5842300");
        }

        /*********************************************************************************
          * Get Requested Date Facts
          *******************************************************************************/
        public iDB2Command GetItems(List<string> filters)
        {

            var filterString = new StringBuilder();
            foreach (string filter in filters)
            {
                filterString.Append(filter);
            }

            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
                    SELECT 
	                    IBMCU,
                        IBITM,
                        IMDSC1,
	                    IMDSC2,
	                    IBSTKT,
	                    IBGLPT,
	                    IBSRP4,
                        SUM(LIPQOH) AS LIPQOH
                    FROM 
	                    F4102
	                    INNER JOIN F4101
	                    ON IMITM = IBITM
                        INNER JOIN F41021
                        ON LIMCU = IBMCU AND
                            LIITM = IBITM
                    WHERE
                        {filterString}
                        IBSTKT NOT IN ('C') AND
                        IBGLPT IN ('GLAS','GLAF','GLAX','GLAZ')
                    GROUP BY
	                    IBMCU,
                        IBITM,
                        IMDSC1,
	                    IMDSC2,
	                    IBSTKT,
	                    IBGLPT,
	                    IBSRP4  
                    HAVING
                        SUM(LIPQOH) != 0               
                    ORDER BY 1,2
                    FOR READ ONLY"
            };
            return cmd;
        }

        public DiscreteItems GetSetup(BusinessSystemSession session, List<string> plantList)
        {
            var items = new DiscreteItems();
            List<string> filters = new List<string>();
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
                    filters.Add("IBMCU IN (" + strInClause.Substring(0, strInClause.Length - 1) + ") AND\r\n");
                }

                var cmd = GetItems(filters);
                foreach (iDB2Parameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = session.DbConnection;
                cmd.CommandTimeout = 0;
                Logger.Info(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var item = new DiscreteItem();
                        item.PlantNum = CastDbNull.To(reader["IBMCU"], string.Empty).Trim();
                        item.ItemNum = CastDbNull.To<decimal>(reader["IBITM"], 0);
                        item.UnitType = CastDbNull.To(reader["IBSRP4"], string.Empty).Trim();
                        item.Desc1 = CastDbNull.To(reader["IMDSC1"], string.Empty).Trim();
                        item.Desc2 = CastDbNull.To(reader["IMDSC2"], string.Empty).Trim();
                        item.StockType = CastDbNull.To(reader["IBSTKT"], string.Empty).Trim();
                        item.GlCode = CastDbNull.To(reader["IBGLPT"], string.Empty).Trim();
                        item.QtyOnHand = CastDbNull.To<decimal>(reader["LIPQOH"], 0) / 1000;
                        items.Add(item);
                    }

                    reader.Close();
                }

                GetStandardCost(session, items);
                GetUomSqft(session, items);
                GetUomLbs(session, items);
                GetItemAttributes(session, items);
                var dateId = E1TimeManager.CastToE1DateId(DateTime.Now);
                GetBomSqft(session, items, dateId);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return items;
        }

        /*********************************************************************************
          * Discrete Item Attributes
          *******************************************************************************/
        public iDB2Command GetStandardCost()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
	            SUM(IECSL) AS IECSL
            FROM
	            F30026
            WHERE
                IEMMCU = ? AND
                IEITM = ? AND
                IELEDG = '07'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            return cmd;
        }

        public void GetStandardCost(BusinessSystemSession session, DiscreteItems items)
        {
            try
            {
                var cmd = session.DbCommands["DiscreteItemManager.GetStandardCost"];
                cmd.Connection = session.DbConnection;
                foreach (var item in items)
                {
                    cmd.Parameters[0].Value = item.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = item.ItemNum;
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            item.StandardCost = CastDbNull.To<decimal>(reader["IECSL"], 0) / 10000;
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
          * Discrete Item Attributes
          *******************************************************************************/
        public iDB2Command GetUomValue()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            UMCONV
            FROM 
                F41002
	        WHERE
                UMMCU = ? AND
		        UMITM = ? AND
		        UMUM = ? AND
		        UMRUM = ?
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("UM", iDB2DbType.iDB2Graphic, 4));
            cmd.Parameters.Add(new iDB2Parameter("RUM", iDB2DbType.iDB2Graphic, 4));
            return cmd;
        }

        public void GetUomSqft(BusinessSystemSession session, DiscreteItems items)
        {
            try
            {
                var cmd = session.DbCommands["DiscreteItemManager.GetUomValue"];
                cmd.Connection = session.DbConnection;
                foreach (var item in items)
                {
                    cmd.Parameters[0].Value = item.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = item.ItemNum;
                    cmd.Parameters[2].Value = "EA";
                    cmd.Parameters[3].Value = "SF";
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            item.UomSqft = CastDbNull.To<decimal>(reader["UMCONV"], 0) / 10000000;
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

        public void GetUomLbs(BusinessSystemSession session, DiscreteItems items)
        {
            try
            {
                var cmd = session.DbCommands["DiscreteItemManager.GetUomValue"];
                cmd.Connection = session.DbConnection;
                foreach (var item in items)
                {
                    cmd.Parameters[0].Value = item.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = item.ItemNum;
                    cmd.Parameters[2].Value = "EA";
                    cmd.Parameters[3].Value = "LB";
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            item.UomLbs = CastDbNull.To<decimal>(reader["UMCONV"], 0) / 10000000;
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
          * Get Item Attributes
          *******************************************************************************/
        public void GetItemAttributes(BusinessSystemSession session, DiscreteItems items)
        {
            try
            {
                var cmd = session.DbCommands["DiscreteItemManager.GetUomValue"];
                cmd.Connection = session.DbConnection;
                foreach (var item in items)
                {
                    //var atts = _itemAttMgr.GetDiscreteItemAttributes(session, item.ItemNum);
                    item.ItemAttributes = _itemAttMgr.GetDiscreteItemAttributes(session, item.ItemNum);
                    item.AttActualSqft = item.ItemAttributes.GetDecimalAttributeValue("Z58USF");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        /*********************************************************************************
          * BOM Sqft
          *******************************************************************************/
        public iDB2Command GetBomSqft()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
                IXQNTY
            FROM
                F3002 
                INNER JOIN F4102 
                ON  IBMCU = IXMMCU AND 
                    IBITM = IXITM AND 
                    IBGLPT IN ('GLAS', 'GLAF', 'GLAX', 'GLAZ')
            WHERE
                IXMMCU = ? AND
                IXKIT = ? AND
                IXEFFF <= ? AND
                IXEFFT >= ? AND 
                IXUM = 'SF' AND 
	            IXUOM = 'EA'
            FOR READ ONLY";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EFFF", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EFFF"].Precision = 6;
            cmd.Parameters["EFFF"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EFFT", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EFFT"].Precision = 6;
            cmd.Parameters["EFFT"].Scale = 0;
            return cmd;
        }

        public void GetBomSqft(BusinessSystemSession session, DiscreteItems items, decimal dateId)
        {
            try
            {
                var cmd = session.DbCommands["DiscreteItemManager.GetBomSqft"];
                cmd.Connection = session.DbConnection;
                foreach (var item in items)
                {
                    cmd.Parameters[0].Value = item.PlantNum.PadLeft(12);
                    cmd.Parameters[1].Value = item.ItemNum;
                    cmd.Parameters[2].Value = dateId;
                    cmd.Parameters[3].Value = dateId;
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (reader.Read())
                        {
                            item.BomSqft = CastDbNull.To<decimal>(reader["IXQNTY"], 0) / 1000;
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
