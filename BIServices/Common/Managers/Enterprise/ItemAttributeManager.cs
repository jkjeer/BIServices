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

namespace BIServices.Common.Managers.Enterprise
{
    public class ItemAttributeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private UdcItemManager _udcItemManager;

        public ItemAttributeManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>
            {
                {"ItemAttributeManager.GetDiscreteItemAttributes", GetDiscreteItemAttributes()},
                {"ItemAttributeManager.GetConfiguredItemAttributesById", GetConfiguredItemAttributesById()},
                {"ItemAttributeManager.GetConfiguredItemArchiveAttributesById", GetConfiguredItemArchiveAttributesById()},
                {"ItemAttributeManager.GetConfigIdByLot", GetConfigIdByLot()}
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
	                IAITM = ?
                FOR READ ONLY"
            };
            cmd.Parameters.Add(new iDB2Parameter("ItemNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["ItemNum"].Precision = 8;
            cmd.Parameters["ItemNum"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetConfiguredItemAttributesById1()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
	        SELECT
		        AMZ58ATNM AS ATNM,
		        AMSY,
		        AMRT,
		        A.KZSGVL AS SGVL,
                U1.DRDL01 AS DL01,
                U1.DRDL02 AS DL02,
                U1.DRSPHD AS SPHD
            FROM (
                SELECT
                    KZPEL,
		            KZSGVL,
                    ROW_NUMBER() OVER (PARTITION BY KZPEL ORDER BY KZCFGCID DESC) AS RN
	            FROM
		            F3211
	            WHERE
		            KZCFGID = ? AND
		            (KZSGVL != '' OR KZPEL IN (390,2070,3660)) AND
		            NOT (KZCNUM = 'Y' and KZSGVL IN ('0', '0.0', '0.00', '0.000','0.0000','.0','.00','.000','.0000'))
                ) A
		    INNER JOIN F580001
		    ON AMPEL = KZPEL 
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

        public iDB2Command GetConfiguredItemArchiveAttributesById1()
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
            ORDER BY 1
            FOR READ ONLY", ConfigurationManager.AppSettings["ControlLib"]);
            cmd.Parameters.Add(new iDB2Parameter("CFGID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CFGID"].Precision = 15;
            cmd.Parameters["CFGID"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetConfiguredItemArchiveAttributesById()
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
		            NOT (C.AMCNUM = 'Y' and A.KZSGVL IN ('0', '0.0', '0.00', '0.000','0.0000','.0','.00','.000','.0000'))
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
            ORDER BY 1
            FOR READ ONLY", ConfigurationManager.AppSettings["ControlLib"]);
            cmd.Parameters.Add(new iDB2Parameter("CFGID", iDB2DbType.iDB2Decimal));
            cmd.Parameters["CFGID"].Precision = 15;
            cmd.Parameters["CFGID"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetConfigIdByLot()
        {
            var cmd = new iDB2Command
            {
                CommandType = CommandType.Text,
                CommandText = $@"
                select 
	                IOLOTN,
	                COALESCE(SDC.CMCFGID,WAC.CMCFGID,PDC.CMCFGID, 0) AS CMCFGID
                from 
	                F4108
	                LEFT OUTER JOIN F4211
		                INNER JOIN F3201 SDC
		                ON	SDC.CMEMCU = SDMCU AND
			                SDC.CMKCOO = SDKCOO AND
			                SDC.CMDOCO = SDDOCO AND
			                SDC.CMDCTO = SDDCTO AND
			                SDC.CMLNID = SDLNID
	                ON	SDDOCO = IODOCO AND
		                SDDCTO = IODCTO AND
		                SDMCU = IOMCU AND
		                SDLOTN = IOLOTN
	                LEFT OUTER JOIN F4801
		                INNER JOIN F3201 WAC
		                ON	WAC.CMEMCU = WAMCU AND
			                WAC.CMDOCO = WADOCO AND
			                WAC.CMDCTO = WADCTO 
	                ON	WADOCO = IODOCO AND
		                WADCTO = IODCTO AND
		                WAMCU = IOMCU AND
		                WALOTN = IOLOTN
	                LEFT OUTER JOIN F4311
		                INNER JOIN F3201 PDC
		                ON	PDC.CMEMCU = PDMCU AND
			                PDC.CMKCOO = PDKCOO AND
			                PDC.CMDOCO = PDDOCO AND
			                PDC.CMDCTO = PDDCTO AND
			                PDC.CMLNID = PDLNID
	                ON	PDDOCO = IODOCO AND
		                PDDCTO = IODCTO AND
		                PDMCU = IOMCU AND
		                PDLOTN = IOLOTN
                where
	                IOLOTN = ?
                FETCH FIRST 1 ROWS ONLY
                FOR READ ONLY"
            };
            cmd.Parameters.Add(new iDB2Parameter("LOTN", iDB2DbType.iDB2Graphic, 30));
            return cmd;
        }

        public decimal GetConfigIdByLot(BusinessSystemSession session, string lotNum)
        {
            decimal val = 0;
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetConfigIdByLot"];
                cmd.Parameters["LOTN"].Value = lotNum;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        val = reader.Get<decimal>("CMCFGID");
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return val;
        }

        /********************************************************************************
        * Methods
        *******************************************************************************/
        public ItemAttributes GetItemAttributes(BusinessSystemSession session, decimal itemNum, decimal configId)
        {
            var attributes = new ItemAttributes();
            try
            {
                if (configId > 0)
                {
                    attributes = GetConfiguredItemAttributes(session, configId);
                    if (!attributes.Any())
                    {
                        attributes = GetConfiguredItemArchiveAttributes(session, configId);
                    }
                }
                else
                {
                    if (itemNum > 0)
                    {
                        attributes = GetDiscreteItemAttributes(session, itemNum);
                    }
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
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
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
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
                        attributes.Add(attribute);
                    }

                    reader.Close();
                }

                Logger.Trace("ROWS: " + attributes.Count);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetDiscreteItemAttributes(BusinessSystemSession session, decimal itemNum, HashSet<string> atnms)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetDiscreteItemAttributes"];
                cmd.Parameters["ItemNum"].Value = itemNum;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var name = reader["ATNM"].ToString().Trim();
                        if (atnms.Contains(name))
                        {
                            var attribute = new ItemAttribute
                            {
                                Name = reader["ATNM"].ToString().Trim(),
                                Value = reader["SGVL"].ToString().Trim(),
                                Desc1 = reader["DL01"].ToString().Trim(),
                                Desc2 = reader["DL02"].ToString().Trim(),
                                Desc3 = reader["SPHD"].ToString().Trim()
                            };
                            attributes.Add(attribute);
                        }
                    }

                    reader.Close();
                }

                Logger.Trace("ROWS: " + attributes.Count);
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetConfiguredItemAttributes(BusinessSystemSession session, decimal configId)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetConfiguredItemAttributesById"];
                cmd.Parameters[0].Value = configId;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var name = reader["ATNM"].ToString().Trim();
                        if (!attributes.Contains(name))
                        {
                            var attribute = new ItemAttribute();
                            var amsy = reader["AMSY"].ToString().Trim();
                            var amrt = reader["AMRT"].ToString().Trim();
                            var sgvl = reader["SGVL"].ToString().Trim();
                            if (amsy == "57" && amrt == "BR")
                            {
                                var udcItem = _udcItemManager.GetUdcItem(session, "57", "BR", sgvl.PadLeft(5, '0'));
                                attribute.Name = name;
                                attribute.Value = reader["SGVL"].ToString().Trim();
                                attribute.Desc1 = udcItem.Description1;
                                attribute.Desc2 = udcItem.Description2;
                                attribute.Desc3 = udcItem.SpecialHandlingCode;
                            }
                            else
                            {
                                attribute.Name = name;
                                attribute.Value = reader["SGVL"].ToString().Trim();
                                attribute.Desc1 = reader["DL01"].ToString().Trim();
                                attribute.Desc2 = reader["DL02"].ToString().Trim();
                                attribute.Desc3 = reader["SPHD"].ToString().Trim();
                            }

                            attributes.Add(attribute);
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetConfiguredItemAttributes(BusinessSystemSession session, decimal configId, HashSet<string> atnms)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetConfiguredItemAttributesById"];
                cmd.Parameters[0].Value = configId;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var name = reader["ATNM"].ToString().Trim();
                        if (atnms.Contains(name))
                        {

                            if (!attributes.Contains(name))
                            {
                                var attribute = new ItemAttribute();
                                var amsy = reader["AMSY"].ToString().Trim();
                                var amrt = reader["AMRT"].ToString().Trim();
                                var sgvl = reader["SGVL"].ToString().Trim();
                                if (amsy == "57" && amrt == "BR")
                                {
                                    var udcItem = _udcItemManager.GetUdcItem(session, "57", "BR", sgvl.PadLeft(5, '0'));
                                    attribute.Name = name;
                                    attribute.Value = reader["SGVL"].ToString().Trim();
                                    attribute.Desc1 = udcItem.Description1;
                                    attribute.Desc2 = udcItem.Description2;
                                    attribute.Desc3 = udcItem.SpecialHandlingCode;
                                }
                                else
                                {
                                    attribute.Name = name;
                                    attribute.Value = reader["SGVL"].ToString().Trim();
                                    attribute.Desc1 = reader["DL01"].ToString().Trim();
                                    attribute.Desc2 = reader["DL02"].ToString().Trim();
                                    attribute.Desc3 = reader["SPHD"].ToString().Trim();
                                }

                                attributes.Add(attribute);
                            }
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }

        public ItemAttributes GetConfiguredItemArchiveAttributes(BusinessSystemSession session, decimal configId)
        {
            var attributes = new ItemAttributes();
            try
            {
                var cmd = session.DbCommands["ItemAttributeManager.GetConfiguredItemArchiveAttributesById"];
                cmd.Parameters[0].Value = configId;
                cmd.Connection = session.DbConnection;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
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
                        attributes.Add(attribute);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new Exception(session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return attributes;
        }
    }
}
