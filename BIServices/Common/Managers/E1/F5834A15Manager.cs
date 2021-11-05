using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class F5834A15Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private UdcItemManager _udcItemManager;

        public F5834A15Manager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>
            {
                {"F5834A15Manager.GetLine", GetLine()},
            };
            foreach (var keyName in hash.Keys.Where(keyName => !session.DbCommands.ContainsKey(keyName)))
            {
                session.DbCommands.Add(keyName, hash[keyName]);
            }
            _udcItemManager = new UdcItemManager(session);
        }

        /********************************************************************************
        * Commands
        *******************************************************************************/
        public iDB2Command GetLine()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
                SELECT
	                *
                FROM
                    F5834A15
                WHERE
	                SAKCOO = ? AND
                    SADOCO = ? AND
                    SADCTO = ? AND
                    SALNID = ?
                FOR READ ONLY"
            };
            cmd.Parameters.Add(new iDB2Parameter("KCOO", iDB2DbType.iDB2Graphic, 5));
            cmd.Parameters.Add(new iDB2Parameter("DOCO", iDB2DbType.iDB2Decimal));
            cmd.Parameters["DOCO"].Precision = 8;
            cmd.Parameters["DOCO"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("DCTO", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("LNID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["LNID"].Precision = 6;
            cmd.Parameters["LNID"].Scale = 0;
            return cmd;
        }

        public ItemAttributes GetItemAttributes(BusinessSystemSession session, string kcoo, decimal doco, string dcto, decimal lnid)
        {
            var atts = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["F5834A15Manager.GetLine"];
                cmd.Parameters[0].Value = kcoo;
                cmd.Parameters[1].Value = doco;
                cmd.Parameters[2].Value = dcto;
                cmd.Parameters[3].Value = lnid;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        var att1 = new ItemAttribute();
                        att1.Name = "Z58L1TK";
                        att1.Value = (CastDbNull.To<Decimal>(reader["SAZ58L1TK"], 0) / 10).ToString();
                        atts.Add(att1);

                        var att2 = new ItemAttribute();
                        att2.Name = "Z58SHAPE";
                        att2.Value = CastDbNull.To<String>(reader["RHMCU"], String.Empty).Trim();
                        var udc2 = _udcItemManager.GetUdcItem(session, "57", "SH", att2.Value);
                        att2.Desc1 = udc2.Description1;
                        att2.Desc2 = udc2.Description2;
                        att2.Desc3 = udc2.SpecialHandlingCode;

                        atts.Add(att2);
                    }

                    reader.Close();
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return atts;
        }

        public ItemAttributes GetItemAttributes(BusinessSystemSession session, Dictionary<string, string> vals)
        {
            var atts = new ItemAttributes();
            try
            {
                foreach (var item in vals)
                {
                    var att = new ItemAttribute();
                    att.Name = item.Key;
                    att.Value = item.Value;
                    att.Desc1 = "";
                    att.Desc2 = "";
                    att.Desc3 = "";
                    atts.Add(att);
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            return atts;
        }
    }
}
