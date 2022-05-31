using System;
using System.Collections.Generic;
using System.Data;
using IBM.Data.DB2.iSeries;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using System.Linq;
using NLog;
using System.Collections;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.E1
{
    public class F4102Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public F4102Manager()
        {
        }

        public F4102Manager(BusinessSystemSession e1Session)
        {
            var hash = new Dictionary<string, iDB2Command>
            {
                {"F4102Manager.GetPlannerNum", GetPlannerNum()},
                {"F4102Manager.GetCustomerPartNum", GetCustomerPartNum()}
            };
            foreach (var keyName in hash.Keys.Where(keyName => !e1Session.DbCommands.ContainsKey(keyName)))
            {
                e1Session.DbCommands.Add(keyName, hash[keyName]);
            }
        }

        /*********************************************************************************
         * Prepared Commands
         ********************************************************************************/
        public iDB2Command GetPlannerNum()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = @"
                    SELECT
                        IBANPL,
                        IBPRP4,
                        ABALPH AS IBANPLALPH
                    FROM
                        F4102
                        LEFT OUTER JOIN F0101
                        ON ABAN8 = IBANPL
                    WHERE
                        IBMCU = ? AND
                        IBITM = ?"
            };
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("ITM", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ITM"].Precision = 8;
            cmd.Parameters["ITM"].Scale = 0;
            return cmd;
        }

        public Hashtable GetPlannerNum(BusinessSystemSession e1Session, string plantNum, decimal itemNum)
        {
            decimal customerNum = 0;
            var table = new Hashtable();
            try
            {
                var cmd = e1Session.DbCommands["F4102Manager.GetPlannerNum"];
                cmd.Parameters["MCU"].Value = plantNum.PadLeft(12);
                cmd.Parameters["ITM"].Value = itemNum;
                cmd.Connection = e1Session.DbConnection;
                Logger.Debug(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        table.Add("IBPRP4", CastDbNull.To(reader["IBPRP4"], string.Empty).Trim());
                        table.Add("IBANPL", CastDbNull.To<decimal>(reader["IBANPL"], 0));
                        var family = CastDbNull.To(reader["IBPRP4"], string.Empty).Trim();
                        var planner = CastDbNull.To<decimal>(reader["IBANPL"], 0);
                        if (family == "PLN")
                        {
                            customerNum = planner;
                        }
                        table.Add("PlannerNum", customerNum);
                        table.Add("PlannerName", CastDbNull.To(reader["IBANPLALPH"], string.Empty).Trim());
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + e1Session.DbConnection.ConnectionString + " JobName: " + e1Session.DbConnection.JobName + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return table;
        }

        public iDB2Command GetCustomerPartNum()
        {
            var cmd = new iDB2Command();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    @"SELECT
                    IVCITM,
                    IVXRT,
                    IVDSC1,
                    IVDSC2
                FROM
                    F4104 
                    INNER JOIN F0101
                    ON ABAN8 = IVAN8
                WHERE
                    IVAN8 = ? AND
                    IVITM = ? AND
                    IVEFTJ <= ? AND
                    IVEXDJ >= ? AND
                    IVXRT = 'C' AND
                    ABAT1 = 'C'";
                cmd.Parameters.Add(new iDB2Parameter("AddressNum", iDB2DbType.iDB2Decimal));
                cmd.Parameters["AddressNum"].Precision = 8;
                cmd.Parameters["AddressNum"].Scale = 0;
                cmd.Parameters.Add(new iDB2Parameter("ItemNum", iDB2DbType.iDB2Decimal));
                cmd.Parameters["ItemNum"].Precision = 8;
                cmd.Parameters["ItemNum"].Scale = 0;
                cmd.Parameters.Add(new iDB2Parameter("EffectiveStartDateId", iDB2DbType.iDB2Decimal));
                cmd.Parameters["EffectiveStartDateId"].Precision = 6;
                cmd.Parameters["EffectiveStartDateId"].Scale = 0;
                cmd.Parameters.Add(new iDB2Parameter("EffectiveEndDateId", iDB2DbType.iDB2Decimal));
                cmd.Parameters["EffectiveEndDateId"].Precision = 6;
                cmd.Parameters["EffectiveEndDateId"].Scale = 0;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            return cmd;
        }

        public string GetCustomerPartNum(BusinessSystemSession session, Decimal addressNum, Decimal itemNum)
        {
            string customerPartNum = "";
            try
            {
                var cmd = session.DbCommands["F4102Manager.GetCustomerPartNum"];
                cmd.Parameters[0].Value = addressNum;
                cmd.Parameters[1].Value = itemNum;
                var dateId = Utilities.Common.CastToE1Date(DateTime.Now);
                cmd.Parameters[2].Value = dateId;
                cmd.Parameters[3].Value = dateId;
                cmd.Connection = session.DbConnection;
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    customerPartNum = CastDbNull.To<String>(reader["IVCITM"], String.Empty).Trim();
                }
                reader.Close();
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            return customerPartNum;
        }

    }
}
