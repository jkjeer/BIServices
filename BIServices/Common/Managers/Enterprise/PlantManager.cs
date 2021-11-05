using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using BIServices.Common.Types.Configuration;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;
using Plant = BIServices.Common.Types.Enterprise.Plant;

namespace BIServices.Common.Managers.Enterprise
{
    public class PlantManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        static public Plant GetPlant(string plantCode)
        {
            var plant = new Plant();
            try
            {
                var plants = GetPlants();
                plant = plants.Find(t => t.PlantNum == plantCode || string.Equals(t.PlantCode, plantCode, StringComparison.CurrentCultureIgnoreCase) || t.NumericValue.ToString() == plantCode);
                if (plant == null)
                {
                    throw new Exception($"Error: Plant {plantCode} not found.");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"{e.Message}: {e.StackTrace}");
            }
            return plant;
        }

        public static Plants GetPlants(string plantCode)
        {
            var plants = new Plants();
            try
            {
                var allPlants = GetPlants();
                var plantList = plantCode.ToUpper().Split(',').ToList();
                foreach (var plantItem in plantList)
                {
                    var plant = allPlants.FindAll(t => t.PlantNum == plantItem || string.Equals(t.PlantCode, plantItem, StringComparison.CurrentCultureIgnoreCase) || t.NumericValue.ToString() == plantItem);
                    if (plant.Count > 0)
                    {
                        plants.AddRange(plant);
                    }
                    else
                    {
                        throw new Exception($"Error: Plant {plantCode} not found.");
                    }
                    if (!plants.Any())
                    {
                        throw new Exception($"Error: Plant {plantCode} not found.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Plant: {plantCode} {e.Message}: {e.StackTrace}");
            }
            return plants;
        }

        public static Plants GetPlants()
        {
            ObjectCache cache = MemoryCache.Default;

            if (cache.Contains("Plants"))
                return (Plants)cache.Get("Plants");
            else
            {
                var cn = new SqlConnection();
                var plants = new Plants();
                try
                {
                    cn = ConnectionManager.GetSqlConnectionFromKey("CORP.INTEGRATION");
                    cn.Open();
                    plants = GetPlantsFromDb(cn);

                    // Store data in the cache
                    CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                    //cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(1.0);
                    cache.Add("Plants", plants, cacheItemPolicy);
                }
                catch (Exception e)
                {
                    var err = new Error(e);
                }
                finally
                {
                    cn.Close();
                }
                return plants;
            }
        }

        /******************************************************************
        * Plant Attributes
        ******************************************************************/
        public static SqlCommand GetPlantsFromDb()

        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT 
                PLANT_NUMERIC_CD, 
                PLANT_ALPHA_CD, 
                PLANT_NAME, 
                PLANT_NO, 
                CUSTOMER_NO, 
                TIME_ZONE_CD, 
                TIME_ZONE_OFFSET,
                COMPANY_CD,
                SHORT_COMPANY_CD,
                ORDER_CALENDAR_CD
            FROM 
	            SVCBI.PLANT");
            return cmd;
        }

        public static Plants GetPlantsFromDb(SqlConnection cn)
        {
            var plants = new Plants();
            try
            {
                var cmd = GetPlantsFromDb();
                cmd.Connection = cn;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var plant = new Plant();
                        plant.PlantNum = CastDbNull.To(reader["PLANT_NUMERIC_CD"], string.Empty).Trim();
                        plant.PlantCode = CastDbNull.To(reader["PLANT_ALPHA_CD"], string.Empty).Trim();
                        plant.Name = CastDbNull.To(reader["PLANT_NAME"], string.Empty).Trim();
                        plant.NumericValue = CastDbNull.To<int>(reader["PLANT_NO"], 0);
                        plant.AddressNum = CastDbNull.To<decimal>(reader["CUSTOMER_NO"], 0);
                        plant.TimeOffset = CastDbNull.To<int>(reader["TIME_ZONE_OFFSET"], 0);
                        plant.TimeZone = CastDbNull.To(reader["TIME_ZONE_CD"], string.Empty).Trim();
                        plant.CompanyNum = CastDbNull.To(reader["COMPANY_CD"], string.Empty).Trim();
                        plant.ShortCompanyNum = CastDbNull.To(reader["SHORT_COMPANY_CD"], string.Empty).Trim();
                        plant.OrderCalendarName = CastDbNull.To(reader["ORDER_CALENDAR_CD"], string.Empty).Trim();
                        plants.Add(plant);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cn.ConnectionString + " JobName: " + e.Message + ": " + e.StackTrace);
                throw new Exception(cn.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return plants;
        }

        /******************************************************************
        * Plant
        ******************************************************************/
        static public Plant GetPlantOld(string code)
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
                throw new Exception(e.Message + ": " + e.StackTrace);
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
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
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
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
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
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            return address;
        }
    }
}
