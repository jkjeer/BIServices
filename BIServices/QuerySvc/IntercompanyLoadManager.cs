using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;

namespace BIServices.QuerySvc
{
    public class IntercompanyLoadManager
    {

        public IntercompanyLoadManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("IntercompanyLoadManager.GetLoads", GetLoads());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
          * Get Closed Racks From E1
          *******************************************************************************/
        public iDB2Command GetLoads()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            LHMCU AS SUPP_MCU,
	            AB0.ABALPH AS SUPP_ALPH,
	            AB0.ABAN8 AS SUPP_AN8,
	            AB1.ABAC30 AS RECD_MCU,
	            AB1.ABALPH AS RECD_ALPH,
	            LHAN8 AS RECD_AN8,
	            LHLDNM,
	            LHLOAD,
	            LHADDJ,
	            LHADTM,
	            LHUPMJ,
	            LHTDAY,
	            LHZ58LDS,
	            LHZ58RKN,
	            RHZ58RID,
	            RHZ58HST,
	            RHZ58RT,
	            RHZ58GWTT / 100 AS RHZ58GWTT,
	            RHZ58RTWT / 100 AS RHZ58RTWT,
	            RHUWUM,
	            IBSTKT,
	            SUM(RDPQOH/1000) AS RDPQOH
            FROM 
	            F580150 
	            INNER JOIN F0101 AB0
	            ON TRIM(AB0.ABAC30) = TRIM(LHMCU) AND
		            AB0.ABAT1 = 'F'
	            INNER JOIN F0101 AB1
	            ON AB1.ABAN8 = LHAN8
	            INNER JOIN F580055
	            ON RHMCU = LHMCU AND 
		            RHPSN = LHLDNM AND 
		            RHZ58HST IN ('7', '8')
	            INNER JOIN F580056
	            ON RDZ58RID = RHZ58RID
	            INNER JOIN F4102
	            ON IBMCU = RDMCU AND
		            IBITM = RDITM
            WHERE 
	            TRIM(AB1.ABAC30) NOT IN ('','08','29','37','92','98') AND
	            AB1.ABAT1 = 'F' AND
	            TRIM(LHMCU) != TRIM(AB1.ABAC30) AND
	            TRIM(AB1.ABAC30) = ? AND
	            LHUPMJ > ? 
            GROUP BY
	            LHMCU,
	            AB0.ABALPH,
	            AB0.ABAN8,
	            LHAN8,
	            AB1.ABAC30,
	            AB1.ABALPH,
	            LHLDNM,
	            LHLOAD,
	            LHADDJ,
	            LHADTM,
	            LHUPMJ,
	            LHTDAY,
	            LHZ58LDS,
	            LHZ58RKN,
	            RHZ58RID,
	            RHZ58HST,
	            RHZ58RT,
	            RHZ58GWTT,
	            RHZ58RTWT,
	            RHUWUM,
	            IBSTKT
            ORDER BY 
	            LHLDNM";
            cmd.Parameters.Add(new iDB2Parameter("MCU", iDB2DbType.iDB2Graphic, 6));
            cmd.Parameters.Add(new iDB2Parameter("UPMJ", iDB2DbType.iDB2Decimal));
            cmd.Parameters["UPMJ"].Precision = 6;
            cmd.Parameters["UPMJ"].Scale = 0;
            return cmd;
        }

        public IntercompanyLoads GetLoads(BusinessSystemSession session, Plant plant)
        {
            var loads = new IntercompanyLoads();
            try
            {
                var cmd = (session.DbCommands.ContainsKey("IntercompanyLoads.GetLoads")) ? session.DbCommands["IntercompanyLoads.GetLoads"] : GetLoads();
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = plant.PlantNum;
                cmd.Parameters[1].Value = Timestamp.CastToE1Date(DateTime.Now.AddDays(-7));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var load = new IntercompanyLoad();
                        load.SupplierPlant = CastDbNull.To<String>(reader["SUPP_MCU"], String.Empty).Trim();
                        load.SupplierPlantName = CastDbNull.To<String>(reader["SUPP_ALPH"], String.Empty).Trim();
                        load.SupplierPlantNum = CastDbNull.To<Decimal>(reader["SUPP_AN8"], 0);
                        load.ReceivingPlant = CastDbNull.To<String>(reader["RECD_MCU"], String.Empty).Trim();
                        load.ReceivingPlantName = CastDbNull.To<String>(reader["RECD_ALPH"], String.Empty).Trim();
                        load.ReceivingPlantNum = CastDbNull.To<Decimal>(reader["RECD_AN8"], 0);
                        load.LoadNum = CastDbNull.To<Decimal>(reader["LHLDNM"], 0);
                        var loadDateId = CastDbNull.To<Decimal>(reader["LHADDJ"], 0);
                        var loadTimeId = CastDbNull.To<Decimal>(reader["LHADTM"], 0);
                        load.LoadDateTime = Timestamp.CastToDateTime(loadDateId, loadTimeId);
                        load.LoadDateTimeLocal = load.LoadDateTime.AddHours(plant.TimeOffset);
                        load.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
                        load.RackType = CastDbNull.To<String>(reader["RHZ58RT"], String.Empty).Trim();
                        load.GlassWeight = Math.Round(CastDbNull.To<Decimal>(reader["RHZ58GWTT"], 0), 2);
                        load.RackWeight = Math.Round(CastDbNull.To<Decimal>(reader["RHZ58RTWT"], 0), 2);
                        load.StockType = CastDbNull.To<String>(reader["IBSTKT"], String.Empty).Trim();
                        load.Racks = 1;
                        load.Units = Math.Round(CastDbNull.To<Decimal>(reader["RDPQOH"], 0), 0);
                        loads.Add(load);
                    }

                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }

            return loads;
        }
    }
}



