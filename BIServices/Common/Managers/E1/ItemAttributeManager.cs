using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class ItemAttributeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ItemAttributeManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>
            {
                {"ItemAttributeManager.GetDiscreteItemAttributes", GetDiscreteItemAttributes()},
                {"ItemAttributeManager.GetConfiguredItemAttributes", GetConfiguredItemAttributes()},
                {"ItemAttributeManager.GetConfiguredItemAttributesById", GetConfiguredItemAttributesById()},
                {"ItemAttributeManager.GetArchivedItemAttributesById", GetArchivedItemAttributesById()}
            };
            foreach (var keyName in hash.Keys.Where(keyName => !session.DbCommands.ContainsKey(keyName)))
            {
                session.DbCommands.Add(keyName, hash[keyName]);
            }
        }

        /********************************************************************************
        * Commands
        *******************************************************************************/
        public iDB2Command GetDiscreteItemAttributes()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
                    SELECT
	                    IAZ58ATNM AS ATNM,
	                    IADVAL AS SGVL,
                        A.DRDL01 AS DL01,
                        A.DRDL02 AS DL02,
                        A.DRSPHD AS SPHD
                    FROM
                        F580002
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005 A
                        ON A.DRSY = IASY AND
                            A.DRRT = IART AND
                            TRIM(A.DRKY) = IADVAL  
                    WHERE
	                    IAITM = ?"
            };
            cmd.Parameters.Add(new iDB2Parameter("ItemNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ItemNum"].Precision = 8;
            cmd.Parameters["ItemNum"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetConfiguredItemAttributes()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
                    SELECT
	                    C.AMZ58ATNM AS ATNM,
                        A.KZSGVL AS SGVL,
                        U.DRDL01 AS DL01,
                        U.DRDL02 AS DL02,
                        U.DRSPHD AS SPHD
                    FROM
                        F3201 CM
                        INNER JOIN F3211 A
                        ON CM.CMCFGID = A.KZCFGID
                        INNER JOIN ( SELECT
                                    KZCFGID ,
                                    KZPEL ,
                                    MAX(KZCFGCID) AS KZCFGCID
                                    FROM
                                    F3211
                                    GROUP BY
                                    KZCFGID ,
                                    KZPEL
                                    ) B
                        ON B.KZCFGID = A.KZCFGID AND
                            A.KZPEL = B.KZPEL AND
                            A.KZCFGCID = B.KZCFGCID
                        INNER JOIN F580001 C
                        ON C.AMPEL = A.KZPEL  
                        LEFT OUTER JOIN {ConfigurationManager.AppSettings["ControlLib"]}.F0005 U
                        ON U.DRSY = C.AMSY AND
                            U.DRRT = C.AMRT AND
                            TRIM(U.DRKY) = A.KZSGVL  
                    WHERE
                        A.KZSGVL <> '' AND
                        NOT (C.AMCNUM = 'Y' and A.KZSGVL IN ('0', '0.0', '0.00', '0.000','0.0000','.0','.00','.000','.0000')) AND
                        CM.CMKCOO = ? AND
                        CM.CMDOCO = ? AND
                        CM.CMDCTO = ? AND
                        CM.CMLNID = ?"
            };
            cmd.Parameters.Add(new iDB2Parameter("CompanyNum", iDB2DbType.iDB2Graphic, 5));
            cmd.Parameters.Add(new iDB2Parameter("OrderNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["OrderNum"].Precision = 8;
            cmd.Parameters["OrderNum"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("OrderType", iDB2DbType.iDB2Graphic, 2));
            cmd.Parameters.Add(new iDB2Parameter("OrderLineNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["OrderLineNum"].Precision = 6;
            cmd.Parameters["OrderLineNum"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetConfiguredItemAttributesById()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
	        SELECT
		        A.AMZ58ATNM AS ATNM,
		        A.AMSY,
		        A.AMRT,
		        A.KZSGVL AS SGVL,
                U1.DRDL01 AS DL01,
                U1.DRDL02 AS DL02,
                U1.DRSPHD AS SPHD
            FROM (
                SELECT
                    KZPEL,
		            KZSGVL,
                    AMZ58ATNM,
                    AMSY,
                    AMRT,
                    ROW_NUMBER() OVER (PARTITION BY KZPEL ORDER BY KZCFGCID DESC) AS RN
	            FROM
		            F3211		    
                    INNER JOIN F580001
		                ON AMPEL = KZPEL 

	            WHERE
		            KZCFGID = ? AND
		            (KZSGVL != '' OR KZPEL IN (390,2070,3660)) AND
		            NOT (AMCNUM = 'Y' and KZSGVL IN ('0', '0.0', '0.00', '0.000','0.0000','.0','.00','.000','.0000'))
                ) A
            LEFT OUTER JOIN {0}.F0005 U1
            ON  U1.DRSY = AMSY AND
                U1.DRRT = AMRT AND
                TRIM(U1.DRKY) = TRIM(A.KZSGVL)          
            WHERE
                A.RN = 1
            ORDER BY 1
            FOR READ ONLY", ConfigurationManager.AppSettings["ControlLib"]);
            cmd.Parameters.Add(new iDB2Parameter("CFGID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CFGID"].Precision = 15;
            cmd.Parameters["CFGID"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetArchivedItemAttributesById()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
	            ATNM, 
	            AMSY,
	            AMRT,
	            SGVL,
                COALESCE(U1.DRDL01, U2.DRDL01) AS DL01,
                COALESCE(U1.DRDL02, U2.DRDL02) AS DL02,
                COALESCE(U1.DRSPHD, U2.DRSPHD) AS SPHD
            FROM (
	            SELECT
		            C.AMZ58ATNM AS ATNM,
		            C.AMSY,
		            C.AMRT,
		            A.KZSGVL AS SGVL,
		            ROW_NUMBER() OVER (PARTITION BY A.KZCFGID, A.KZPEL ORDER BY A.KZCFGCID ASC) AS ROWNUM
	            FROM
		            F5832119 A
		            INNER JOIN F580001 C
		            ON C.AMPEL = A.KZPEL  
	            WHERE
		            A.KZCFGID = ? AND
		            (A.KZSGVL != '' OR KZPEL IN (390,2070,3660)) AND
		            NOT (A.KZCNUM = 'Y' and A.KZSGVL IN ('0', '0.0', '0.00', '0.000','0.0000','.0','.00','.000','.0000'))
		            ) A
                LEFT OUTER JOIN {0}.F0005 U1
                ON  U1.DRSY = A.AMSY AND
                    U1.DRRT = A.AMRT AND
                    CAST(U1.DRKY AS INT) = CAST(A.SGVL AS INT) AND
                    A.AMSY = '57' AND
                    A.AMRT = 'BR'
                LEFT OUTER JOIN {0}.F0005 U2
                ON  U2.DRSY = A.AMSY AND
                    U2.DRRT = A.AMRT AND
                    U2.DRKY = A.SGVL
            WHERE 
	            A.ROWNUM = 1 
            ORDER BY 1", ConfigurationManager.AppSettings["ControlLib"]);
            cmd.Parameters.Add(new iDB2Parameter("CFGID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CFGID"].Precision = 15;
            cmd.Parameters["CFGID"].Scale = 0;
            return cmd;
        }
        /**********************************************************************************************
         Discrete
         *********************************************************************************************/
        public ItemAttributes GetItemAttributes(BusinessSystemSession session, decimal itemNum, decimal configId)
        {
            var attributes = new ItemAttributes();
            try
            {
                attributes = configId > 0 ? GetConfiguredItemAttributes(session, configId) : GetDiscreteItemAttributes(session, itemNum);
            }
            catch (Exception e)
            {
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetItemAttributes(BusinessSystemSession session, decimal itemNum, decimal documentNum, string documentType, decimal documentLine, string companyCode)
        {
            var attributes = new ItemAttributes();
            try
            {
                attributes = documentNum > 0 ? GetConfiguredItemAttributes(session, documentNum, documentType, documentLine, companyCode) : GetDiscreteItemAttributes(session, itemNum);
            }
            catch (Exception e)
            {
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetDiscreteItemAttributes(BusinessSystemSession session, decimal itemNum)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetDiscreteItemAttributes"];
                cmd.Parameters["ItemNum"].Value = itemNum;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var attribute = new ItemAttribute
                    {
                        Name = reader["ATNM"].ToString().Trim(),
                        Value = reader["SGVL"].ToString().Trim(),
                        Desc1 = reader["DL01"].ToString().Trim(),
                        Desc2 = reader["DL02"].ToString().Trim(),
                        Desc3 = reader["SPHD"].ToString().Trim()
                    };
                    if (!attributes.Contains(attribute.Name))
                    {
                        attributes.Add(attribute);
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        /**********************************************************************************************
         Configured
         *********************************************************************************************/
        public ItemAttributes GetConfiguredItemAttributes(BusinessSystemSession session, decimal orderNum, string orderType, decimal orderLineNum, string companyCode)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = (session.DbCommands.ContainsKey("ItemAttributeManager.GetConfiguredItemAttributes")) ? session.DbCommands["ItemAttributeManager.GetConfiguredItemAttributes"] : GetConfiguredItemAttributes();
                cmd.Parameters["CompanyNum"].Value = companyCode;
                cmd.Parameters["OrderNum"].Value = orderNum;
                cmd.Parameters["OrderType"].Value = orderType;
                cmd.Parameters["OrderLineNum"].Value = orderLineNum;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var attribute = new ItemAttribute
                    {
                        Name = reader["ATNM"].ToString().Trim(),
                        Value = reader["SGVL"].ToString().Trim(),
                        Desc1 = reader["DL01"].ToString().Trim(),
                        Desc2 = reader["DL02"].ToString().Trim(),
                        Desc3 = reader["SPHD"].ToString().Trim()
                    };
                    if (!attributes.Contains(attribute.Name))
                    {
                        attributes.Add(attribute);
                    }
                }
                reader.Close();
                Logger.Trace("ROWS: " + attributes.Count);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetConfiguredItemAttributes(BusinessSystemSession session, decimal configId, List<string> pels = null)
        {
            var startTime = DateTime.Now.ToLocalTime();
            var attributes = new ItemAttributes();
            try
            {
                var cmd = (session.DbCommands.ContainsKey("ItemAttributeManager.GetConfiguredItemAttributesById")) ? session.DbCommands["ItemAttributeManager.GetConfiguredItemAttributesById"] : GetConfiguredItemAttributesById();
                cmd.Parameters[0].Value = configId;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                while (reader.Read())
                {
                    var attribute = new ItemAttribute
                    {
                        Name = reader["ATNM"].ToString().Trim(),
                        Value = reader["SGVL"].ToString().Trim(),
                        Desc1 = reader["DL01"].ToString().Trim(),
                        Desc2 = reader["DL02"].ToString().Trim(),
                        Desc3 = reader["SPHD"].ToString().Trim()
                    };
                    if (!attributes.Contains(attribute.Name))
                    {
                        attributes.Add(attribute);
                    }
                }
                reader.Close();

                if (attributes.Count == 0)
                {
                    cmd = (session.DbCommands.ContainsKey("ItemAttributeManager.GetArchivedItemAttributesById")) ? session.DbCommands["ItemAttributeManager.GetArchivedItemAttributesById"] : GetArchivedItemAttributesById();
                    cmd.Parameters[0].Value = configId;
                    cmd.Connection = session.DbConnection;
                    Logger.Trace(Logging.DbCmdString(cmd));
                    IDataReader reader1 = cmd.ExecuteReader(CommandBehavior.SingleResult);
                    while (reader1.Read())
                    {
                        var attribute = new ItemAttribute
                        {
                            Name = reader1["ATNM"].ToString().Trim(),
                            Value = reader1["SGVL"].ToString().Trim(),
                            Desc1 = reader1["DL01"].ToString().Trim(),
                            Desc2 = reader1["DL02"].ToString().Trim(),
                            Desc3 = reader1["SPHD"].ToString().Trim()
                        };
                        if (!attributes.Contains(attribute.Name))
                        {
                            attributes.Add(attribute);
                        }
                    }
                    reader1.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }


        public ItemAttributes GetConfiguredItemSalesLineAttributes(BusinessSystemSession session, decimal configId)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetConfiguredItemSalesLineAttributes"];
                cmd.Parameters[0].Value = configId;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                var columns = new List<string> { "Z58LPR", "Z58RT", "Z58RKS", "Z58ROW", "Z58STK", "Z58RTNIO", "Z58PKGIL" };
                while (reader.Read())
                {
                    foreach (var column in columns)
                    {
                        var attribute = new ItemAttribute
                        {
                            Name = column,
                            Value = reader[column].ToString().Trim()
                        };
                        if (!attributes.Contains(attribute.Name))
                        {
                            attributes.Add(attribute);
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

    }
}
