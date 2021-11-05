using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BIServices.Common.Types;
using BIServices.Common.Types.Configuration;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using Plant = BIServices.Common.Types.Plant;

namespace BIServices.Common.Managers
{
    public class PlantManager
    {
        public PlantManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("PlantManager.GetPlantByNum", GetPlantByNum());
            hash.Add("PlantManager.GetPlantByAddressNum", GetPlantByAddressNum());
            hash.Add("PlantManager.GetPlantAddress", GetPlantAddress());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /******************************************************************
        * Plant
        ******************************************************************/
        static public Plant GetPlant(string code)
        {
            var plant = new Plant();
            try
            {
                var plantConfig = (PlantConfiguration)ConfigurationManager.GetSection("PlantSection");
                var plantx = plantConfig.Plants[plantConfig.Xrefs[code].Value];
                plant.PlantCode = plantx.Code;
                plant.PlantNum = plantx.Name;
                plant.NumericValue = Convert.ToInt16(plantx.Numeric);
                plant.TimeOffset = Convert.ToInt16(plantx.Offset);
                plant.AddressNum = Convert.ToDecimal(plantx.AddressNum);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message + ": " + e.StackTrace);
            }
            return plant;
        }

        /******************************************************************
        * Plant
        ******************************************************************/
        public iDB2Command GetPlantByNum()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText =
            @"SELECT 
	            ABAN8,
                ABALPH,
                ABALKY,
                ABAC30,
                ABAC29,
                ABAT1              
            FROM 
	            F0101
            WHERE 
                ABAT1 = 'F' AND
                ABAC30 = ?";
            cmd.Parameters.Add(new iDB2Parameter("PlantNum", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public iDB2Command GetPlantByAddressNum()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText =
            @"SELECT 
	            ABAN8,
                ABALPH,
                ABALKY,
                ABAC30,
                ABAC29,
                ABAT1              
            FROM 
	            F0101
            WHERE 
                ABAT1 = 'F' AND
                ABAN8 = ?";
            cmd.Parameters.Add(new iDB2Parameter("AddressNum", iDB2DbType.iDB2Decimal));
            cmd.Parameters["AddressNum"].Precision = 8;
            cmd.Parameters["AddressNum"].Scale = 0;
            return cmd;
        }

        public Plant GetPlant(BusinessSystemSession session, decimal addressNum = 0, string plantNum = null, Boolean includeAddress = false)
        {
            var plant = new Plant();
            var cmd = new iDB2Command();
            try
            {
                if (addressNum != 0)
                {
                    cmd = (session.DbCommands.ContainsKey("GetPlantByAddressNum")) ? session.DbCommands["GetPlantByAddressNum"] : GetPlantByAddressNum();
                    cmd.Parameters[0].Value = addressNum;                    
                }
                if (plantNum != null)
                {
                    cmd = (session.DbCommands.ContainsKey("GetPlantByNum")) ? session.DbCommands["GetPlantByNum"] : GetPlantByNum();
                    cmd.Parameters[0].Value = plantNum;
                }
                cmd.Connection = session.DbConnection;
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    plant.PlantNum = CastDbNull.To<String>(reader["ABAC30"], String.Empty).Trim();
                    plant.PlantCode = session.PlantMap.GetAttributesByNum(plant.PlantNum).Code;
                    plant.ShortCompanyNum = CastDbNull.To<String>(reader["ABAC29"], String.Empty).Trim();
                    plant.CompanyNum = CastDbNull.To<String>(reader["ABAC29"], String.Empty).Trim().PadLeft(5).Replace(' ', '0');
                    plant.AddressNum = CastDbNull.To<Decimal>(reader["ABAN8"], 0);
                    plant.LegacyAddressNum = CastDbNull.To<String>(reader["ABALKY"], String.Empty).Trim();
                    plant.Name = CastDbNull.To<String>(reader["ABALPH"], String.Empty).Trim();
                    if (includeAddress)
                    {
                        plant.Address = GetPlantAddress(session, addressNum);
                    }
                }
                reader.Close();
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return plant;
        }

        /******************************************************************
        * Plant
        ******************************************************************/
        public iDB2Command GetPlantAddress()
        {
            var cmd = new iDB2Command();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                @"SELECT
                    ABAN8,
                    ABAT1,
                    ABALPH,
                    ALADD1,
                    ALADD2,
                    ALADD3,
                    ALADD4,
                    ALADDZ,
                    ALCTY1,
                    ALADDS,
                    ALCTR
                FROM 
                    F0101
	                LEFT OUTER JOIN F0116 
                    ON  ALAN8 = ABAN8 AND
						ALEFTB = ABEFTB
                WHERE 
                    ABAN8 = ?";
                cmd.Parameters.Add(new iDB2Parameter("AddressBookNum", iDB2DbType.iDB2Decimal));
                cmd.Parameters["AddressBookNum"].Precision = 8;
                cmd.Parameters["AddressBookNum"].Scale = 0;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return cmd;
        }

        public Address GetPlantAddress(BusinessSystemSession session, Decimal addressNum)
        {
            var address = new Address();
            try
            {
                var cmd = (session.DbCommands.ContainsKey("GetPlantAddress")) ? session.DbCommands["GetPlantAddress"] : GetPlantAddress();
                cmd.Parameters[0].Value = addressNum;
                cmd.Connection = session.DbConnection;
                IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                while (reader.Read())
                {
                    address.AddressNum = CastDbNull.To<Decimal>(reader["ABAN8"], 0);
                    address.Name = CastDbNull.To<String>(reader["ABALPH"], String.Empty).Trim();
                    address.Address1 = CastDbNull.To<String>(reader["ALADD1"], String.Empty).Trim();
                    address.Address2 = CastDbNull.To<String>(reader["ALADD2"], String.Empty).Trim();
                    address.Address3 = CastDbNull.To<String>(reader["ALADD3"], String.Empty).Trim();
                    address.Address4 = CastDbNull.To<String>(reader["ALADD4"], String.Empty).Trim();
                    address.City = CastDbNull.To<String>(reader["ALCTY1"], String.Empty).Trim();
                    address.State = CastDbNull.To<String>(reader["ALADDS"], String.Empty).Trim();
                    address.PostalCode = CastDbNull.To<String>(reader["ALADDZ"], String.Empty).Trim();
                    address.Country = CastDbNull.To<String>(reader["ALCTR"], String.Empty).Trim();
                }
                reader.Close();
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return address;
        }
    }
}
