using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;

namespace BIServices.QuerySvc
{
    public class ClosedRackManager
    {

        public ClosedRackManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            hash.Add("ClosedRacksManager.GetClosedRacks", GetClosedRacks());
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
        public iDB2Command GetClosedRacks()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT 
		        RHMCU,
		        RHZ58RID, 
		        RHZ58RT,
                RHLOCN,
		        RHZ58BOX,
		        RHZ58HST,
		        RHUPMJ,
		        RHTDAY,
		        S1Z58SCN ,
		        S1Z58SCTYP,
                S5DRQJ,
                S5SHAN,
                RHAN8,
                AB1.ABALPH AS ABALPH1,
                AB2.ABALPH AS ABALPH2,
		        SUM(RDPQOH/1000) AS RDPQOH 
	        FROM 
		        F580055 
		        INNER JOIN F580056 
		        ON	RDZ58RID = RHZ58RID 
		        INNER JOIN F5834A11 
		        INNER JOIN F5834A01
		        ON	S1MCU = S5MCU AND
			        S1Z58SCN = S5Z58SCN AND
			        S1Z58SCD = S5Z58SCD
		        ON	S5DOCO = RDDOC1 AND
		            RDDOC1 > 0
                INNER JOIN F0101 AB1
                ON AB1.ABAN8 = RHAN8
                INNER JOIN F0101 AB2
                ON AB2.ABAN8 = S5SHAN
	        WHERE 
		        RHMCU = ? AND
		        RHZ58HST = '4'
	        GROUP BY 
		        RHMCU,
		        RHZ58RID,
		        RHZ58RT,
                RHLOCN,
		        RHZ58BOX,
		        RHZ58HST,
		        RHUPMJ,
		        RHTDAY,
		        S1Z58SCN ,
		        S1Z58SCTYP,
                S5DRQJ,
                S5SHAN,
                RHAN8,
                AB1.ABALPH,
                AB2.ABALPH";
            cmd.Parameters.Add(new iDB2Parameter("PlantId", iDB2DbType.iDB2Graphic, 12));
            return cmd;
        }

        public ClosedRacks GetClosedRacks(BusinessSystemSession session, SqlConnection cn,  Plant plant)
        {
            var racks = new ClosedRacks();
            try
            {
                var cmd = (session.DbCommands.ContainsKey("ClosedRacksManager.GetClosedRacks")) ? session.DbCommands["ClosedRacksManager.GetClosedRacks"] : GetClosedRacks();
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rack = new ClosedRack();
                        rack.Plant = CastDbNull.To(reader["RHMCU"], String.Empty).Trim();
                        rack.RackId = CastDbNull.To<Decimal>(reader["RHZ58RID"], 0);
                        rack.RackType = CastDbNull.To(reader["RHZ58RT"], String.Empty).Trim();
                        rack.Location = CastDbNull.To(reader["RHLOCN"], String.Empty).Trim();
                        rack.BoxNum = CastDbNull.To<Decimal>(reader["RHZ58BOX"], 0);
                        rack.Status = CastDbNull.To(reader["RHZ58HST"], String.Empty).Trim();
                        rack.ScheduleNum = CastDbNull.To<Decimal>(reader["S1Z58SCN"], 0);
                        rack.ScheduleType = CastDbNull.To(reader["S1Z58SCTYP"], String.Empty).Trim();
                        var dateId = CastDbNull.To<Decimal>(reader["RHUPMJ"], 0);
                        var timeId = CastDbNull.To<Decimal>(reader["RHTDAY"], 0);
                        rack.RackDate = Common.Utilities.Common.CastToDateTime(dateId, timeId).AddHours(plant.TimeOffset);
                        var dateId1 = CastDbNull.To<Decimal>(reader["S5DRQJ"], 0);
                        rack.RequestedDate = Common.Utilities.Common.CastToDateTime(dateId1);
                        rack.CustomerNum = CastDbNull.To<Decimal>(reader["RHAN8"], 0);
                        rack.CustomerName = CastDbNull.To(reader["ABALPH1"], String.Empty).Trim();
                        rack.CustomerShipNum = CastDbNull.To<Decimal>(reader["S5SHAN"], 0);
                        rack.CustomerShipName = CastDbNull.To(reader["ABALPH2"], String.Empty).Trim();
                        rack.OnHand = Convert.ToInt16(CastDbNull.To<Decimal>(reader["RDPQOH"], 0));
                        //GetLineData(rack, cn);
                        racks.Add(rack);
                    }

                    reader.Close();
                }

                GetLastLineData(racks, cn);
                GetAggLineData(racks, cn);
                GetAttributes(racks, cn);
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return racks;
        }

        public SqlCommand GetLastLineData(ClosedRacks racks)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              RackId ,
              ProductionLineCode ,
              WrittenDate
            FROM
              ( SELECT
                  rt.RackId ,
                  PD.ProductionLineCode ,
                  PD.WrittenDate ,
                  ROW_NUMBER() OVER ( PARTITION BY rt.RackId ORDER BY PD.WrittenDate DESC ) AS ROW_NUM
                FROM
                  EBS.RackDetail rt WITH (NOLOCK)
                  INNER JOIN UnitBox ub WITH (NOLOCK)
                  ON ub.UnitIDtxt = rt.SerialNum
                  INNER JOIN ProdData PD WITH ( NOLOCK )
                  ON PD.UnitID = ub.UnitID AND
                     PD.OrderNum = ub.OrderItem
                WHERE
                  rt.RackId IN ({0})
              ) A
            WHERE
              A.ROW_NUM = 1", String.Join(",", racks.Select(s => s.RackId.ToString())));
            return cmd;
        }

        public void GetLastLineData(ClosedRacks racks, SqlConnection cn)
        {
            try
            {
                var cmd = GetLastLineData(racks);
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, object[]>();
                    while (reader.Read())
                    {
                        var rackId = CastDbNull.To<Decimal>(reader["RackID"], 0);
                        var lastLine = CastDbNull.To(reader["ProductionLineCode"], String.Empty).Trim();
                        var lastLineDate = CastDbNull.To(reader["WrittenDate"], new DateTime(1900, 1, 1));
                        dict.Add(rackId, new object[]
                        {
                            lastLine,
                            lastLineDate
                        });
                    }

                    reader.Close();

                    foreach (ClosedRack rack in racks)
                    {
                        if (dict.ContainsKey(rack.RackId))
                        {
                            rack.LastLine = (string) dict[rack.RackId][0];
                            rack.LastLineDate = (DateTime) dict[rack.RackId][1];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        public SqlCommand GetAggLineData(ClosedRacks racks)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              rt.RackID ,
              PD.ProductionLineCode ,
              COUNT(DISTINCT ub.UnitID) AS Units
            FROM
              EBS.RackDetail rt WITH (NOLOCK)
              INNER  JOIN UnitBox ub WITH (NOLOCK)
              ON ub.UnitIDtxt = rt.SerialNum
              INNER JOIN ProdData PD WITH ( NOLOCK )
              ON PD.UnitID = ub.UnitID AND
                 PD.OrderNum = ub.OrderItem
            WHERE
              rt.RackId IN ({0})
            GROUP BY
              rt.RackID ,
              PD.ProductionLineCode
            ORDER BY
              1;", String.Join(",", racks.Select(s => s.RackId.ToString())));
            return cmd;
        }

        public void GetAggLineData(ClosedRacks racks, SqlConnection cn)
        {
            try
            {
                var cmd = GetAggLineData(racks);
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, string>();

                    decimal prevRackId = -1;
                    StringBuilder sb = new StringBuilder();
                    while (reader.Read())
                    {
                        var rackId = CastDbNull.To<Decimal>(reader["RackID"], 0);
                        var lastLine = CastDbNull.To(reader["ProductionLineCode"], String.Empty).Trim();
                        var units = CastDbNull.To(reader["Units"], 0);
                        if (prevRackId != rackId && prevRackId != -1)
                        {
                            if (!dict.ContainsKey(prevRackId))
                            {
                                dict.Add(prevRackId, sb.ToString());
                                sb.Clear();
                            }
                        }

                        sb.Append(lastLine);
                        sb.Append("(");
                        sb.Append(units.ToString());
                        sb.Append(") ");
                        prevRackId = rackId;
                    }

                    reader.Close();
                    dict.Add(prevRackId, sb.ToString());
                    sb.Clear();

                    foreach (ClosedRack rack in racks)
                    {
                        if (dict.ContainsKey(rack.RackId))
                        {
                            rack.Lines = dict[rack.RackId];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        public SqlCommand GetAttributes(ClosedRacks racks)
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format(@"
            SELECT
              rt.RackID ,
              SUM(CASE WHEN ub.Tubes = 1 THEN 1 ELSE 0 END) AS Tubes,
              SUM(CASE WHEN ub.ShapeNum > 0 THEN 1 ELSE 0 END) AS Shapes,
              SUM(CASE WHEN ub.PreserveExt > 0 OR ub.PreserveInt > 0 THEN 1 ELSE 0 END) AS Preserve,                
              SUM(CASE WHEN ub.ScoreOpenings > 0 THEN 1 ELSE 0 END) AS Scoring,
              SUM(CASE WHEN ub.MCode > 0 THEN 1 ELSE 0 END) AS Bars,
              SUM(CASE WHEN ub.PickUnit > 0 THEN 1 ELSE 0 END) AS Picks
            FROM
              EBS.RackDetail rt
              INNER  JOIN UnitBox ub
              ON ub.UnitIDtxt = rt.SerialNum
            WHERE
              rt.RackId IN ({0})
            GROUP BY
              rt.RackID", String.Join(",", racks.Select(s => s.RackId.ToString())));
            return cmd;
        }

        public void GetAttributes(ClosedRacks racks, SqlConnection cn)
        {
            try
            {
                var cmd = GetAttributes(racks);
                cmd.Connection = cn;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var dict = new Dictionary<decimal, int[]>();
                    while (reader.Read())
                    {
                        var rackId = CastDbNull.To<Decimal>(reader["RackID"], 0);
                        var tubes = CastDbNull.To(reader["Tubes"], 0);
                        var shapes = CastDbNull.To(reader["Shapes"], 0);
                        var preserve = CastDbNull.To(reader["Preserve"], 0);
                        var scoring = CastDbNull.To(reader["Scoring"], 0);
                        var bars = CastDbNull.To(reader["Bars"], 0);
                        var picks = CastDbNull.To(reader["Picks"], 0);
                        dict.Add(rackId, new int[]
                        {
                            tubes,
                            shapes,
                            preserve,
                            scoring,
                            bars,
                            picks
                        });
                    }

                    reader.Close();

                    foreach (ClosedRack rack in racks)
                    {
                        if (dict.ContainsKey(rack.RackId))
                        {
                            rack.Tubes = dict[rack.RackId][0];
                            rack.Shapes = dict[rack.RackId][1];
                            rack.Preserve = dict[rack.RackId][2];
                            rack.Scoring = dict[rack.RackId][3];
                            rack.Bars = dict[rack.RackId][4];
                            rack.Picks = dict[rack.RackId][5];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }


    }
}
