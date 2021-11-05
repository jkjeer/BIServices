using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.QuerySvc
{
    public class RackEventManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RackEventManager(BusinessSystemSession session)
        {
            var hash = new Dictionary<string, iDB2Command>();
            //hash.Add("RackEventsManager.GetRackEventsByHour", GetRackEventsByHour());
            foreach (string keyName in hash.Keys)
            {
                if (!session.DbCommands.ContainsKey(keyName))
                {
                    session.DbCommands.Add(keyName, hash[keyName]);
                }
            }
        }

        /*********************************************************************************
         * Rack Events By Hour
         *******************************************************************************/
        public iDB2Command GetRackEventsByHour()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT 
	            A.HLMCU,
	            A.HLZ58RID,
	            A.HLZ58HST,
	            A.HLPSN,
	            A.HLUKID,
	            A.HLUPMJ,
	            A.HLTDAY
            FROM
	            F580559 A
	            INNER JOIN 
	            (SELECT
		            HLMCU,
		            HLZ58RID ,
		            HLZ58HST ,
		            HLPSN ,
		            MIN(HLUKID) AS HLUKID
	            FROM
		            F580559
	            WHERE
		            HLMCU = ? AND 
                    (
				        (HLUPMJ = ? AND HLUPMT >= ?) OR  
				        (HLUPMJ >= ? AND HLUPMJ <= ?) OR
				        (HLUPMJ = ? AND HLUPMT <= ?)
                    ) AND
		            HLZ58HST IN ('4', '5')
	            GROUP BY 
		            HLMCU,
		            HLZ58RID ,
		            HLZ58HST ,
		            HLPSN) B
		            ON B.HLUKID = A.HLUKID
            ORDER BY A.HLUPMJ, A.HLTDAY";
            cmd.Parameters.Add(new iDB2Parameter("PlantId", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("StartDateId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["StartDateId"].Precision = 6;
            cmd.Parameters["StartDateId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("StartTimeId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["StartTimeId"].Precision = 6;
            cmd.Parameters["StartTimeId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MidDateId1", iDB2DbType.iDB2Decimal));
            cmd.Parameters["MidDateId1"].Precision = 6;
            cmd.Parameters["MidDateId1"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MidDateId2", iDB2DbType.iDB2Decimal));
            cmd.Parameters["MidDateId2"].Precision = 6;
            cmd.Parameters["MidDateId2"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EndDateId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EndDateId"].Precision = 6;
            cmd.Parameters["EndDateId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EndTimeId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EndTimeId"].Precision = 6;
            cmd.Parameters["EndTimeId"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetClosedRackEventsByHour()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT * FROM (
                SELECT 
	                HLMCU,
	                HLZ58RID,
	                HLZ58HST,
	                HLPSN,
	                HLUKID,
	                HLUPMJ,
	                HLTDAY,
				    HLZ58SCN,
				    ROW_NUMBER() OVER (PARTITION BY HLMCU, HLZ58RID, HLZ58HST ORDER BY HLUKID ASC) AS RN
                FROM
	                F580559 A
	            WHERE
		            HLMCU = ? AND 
                    (
				        (HLUPMJ = ? AND HLUPMT >= ?) OR  
				        (HLUPMJ >= ? AND HLUPMJ <= ?) OR
				        (HLUPMJ = ? AND HLUPMT <= ?)
                    ) AND
				    HLZ58HST IN ('4') AND
				    NOT EXISTS (SELECT 1 FROM F580056 WHERE RDZ58RID = HLZ58RID AND RDMCU = HLMCU AND RDXCTO = 'OT')
		    ) A
		    WHERE A.RN = 1";
            cmd.Parameters.Add(new iDB2Parameter("PlantId", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("StartDateId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["StartDateId"].Precision = 6;
            cmd.Parameters["StartDateId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("StartTimeId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["StartTimeId"].Precision = 6;
            cmd.Parameters["StartTimeId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MidDateId1", iDB2DbType.iDB2Decimal));
            cmd.Parameters["MidDateId1"].Precision = 6;
            cmd.Parameters["MidDateId1"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MidDateId2", iDB2DbType.iDB2Decimal));
            cmd.Parameters["MidDateId2"].Precision = 6;
            cmd.Parameters["MidDateId2"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EndDateId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EndDateId"].Precision = 6;
            cmd.Parameters["EndDateId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EndTimeId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EndTimeId"].Precision = 6;
            cmd.Parameters["EndTimeId"].Scale = 0;
            return cmd;
        }

        public iDB2Command GetLockedRackEventsByHour()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
	        SELECT * FROM (
                SELECT 
	                HLMCU,
	                HLZ58RID,
	                HLZ58HST,
	                HLPSN,
	                HLUKID,
	                HLUPMJ,
	                HLTDAY,
				    HLZ58SCN,
				    ROW_NUMBER() OVER (PARTITION BY HLMCU, HLZ58RID, HLZ58HST, HLPSN ORDER BY HLUKID ASC) AS RN
                FROM
	                F580559 A
	            WHERE
		            HLMCU = ? AND 
                    (
				        (HLUPMJ = ? AND HLUPMT >= ?) OR  
				        (HLUPMJ >= ? AND HLUPMJ <= ?) OR
				        (HLUPMJ = ? AND HLUPMT <= ?)
                    ) AND
				    HLZ58HST IN ('5') AND
				    NOT EXISTS (SELECT 1 FROM F580056 WHERE RDZ58RID = HLZ58RID AND RDMCU = HLMCU AND RDXCTO = 'OT')
		    ) B
		    WHERE B.RN = 1";
            cmd.Parameters.Add(new iDB2Parameter("PlantId", iDB2DbType.iDB2Graphic, 12));
            cmd.Parameters.Add(new iDB2Parameter("StartDateId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["StartDateId"].Precision = 6;
            cmd.Parameters["StartDateId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("StartTimeId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["StartTimeId"].Precision = 6;
            cmd.Parameters["StartTimeId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MidDateId1", iDB2DbType.iDB2Decimal));
            cmd.Parameters["MidDateId1"].Precision = 6;
            cmd.Parameters["MidDateId1"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("MidDateId2", iDB2DbType.iDB2Decimal));
            cmd.Parameters["MidDateId2"].Precision = 6;
            cmd.Parameters["MidDateId2"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EndDateId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EndDateId"].Precision = 6;
            cmd.Parameters["EndDateId"].Scale = 0;
            cmd.Parameters.Add(new iDB2Parameter("EndTimeId", iDB2DbType.iDB2Decimal));
            cmd.Parameters["EndTimeId"].Precision = 6;
            cmd.Parameters["EndTimeId"].Scale = 0;
            return cmd;
        }

        public RackEventHourSummarySet GetRackEventsByHour(BusinessSystemSession session, SqlConnection cn, Plant plant, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var rackEvents = new RackEvents();
            var rackHourSummarySet = new RackEventHourSummarySet();
            try
            {
                Hashtable dateRange = new Hashtable(); ;
                if (dateCode.ToUpper().Equals("RANGE"))
                {
                    dateRange.Add("StartDate", (DateTime)startDate);
                    dateRange.Add("EndDate", (DateTime)endDate);
                }
                else
                {
                    dateRange = GetDateCodeRange(cn, dateCode);
                }
                var enterpriseStartDate = (DateTime)dateRange["StartDate"];
                var enterpriseEndDate = (DateTime)dateRange["EndDate"];

                var plantStartDateTime = enterpriseStartDate.AddHours(-plant.TimeOffset);
                var plantEndDateTime = enterpriseEndDate.AddDays(1).AddHours(-plant.TimeOffset);

                // Closed Events
                var cmd1 = GetClosedRackEventsByHour();
                cmd1.Connection = session.DbConnection;
                cmd1.Parameters[0].Value = plant.PlantNum.PadLeft(12);

                var dateParms = GetE1DateParms(plantStartDateTime, plantEndDateTime);
                cmd1.Parameters[1].Value = (decimal)dateParms["StartDate"];
                cmd1.Parameters[2].Value = (decimal)dateParms["StartTime"];
                cmd1.Parameters[3].Value = (decimal)dateParms["MidDate1"];
                cmd1.Parameters[4].Value = (decimal)dateParms["MidDate2"];
                cmd1.Parameters[5].Value = (decimal)dateParms["EndDate"];
                cmd1.Parameters[6].Value = (decimal)dateParms["EndTime"];

                Logger.Trace(Logging.DbCmdString(cmd1));
                using (IDataReader reader1 = cmd1.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader1.Read())
                    {
                        var rackEvent = new RackEvent();
                        rackEvent.Plant = CastDbNull.To<String>(reader1["HLMCU"], String.Empty).Trim();
                        rackEvent.RackId = CastDbNull.To<Decimal>(reader1["HLZ58RID"], 0);
                        rackEvent.EventType = "CLOS";
                        var dateId = CastDbNull.To<Decimal>(reader1["HLUPMJ"], 0);
                        var timeId = CastDbNull.To<Decimal>(reader1["HLTDAY"], 0);
                        rackEvent.EventDateTime = Common.Utilities.Common.CastToDateTime(dateId, timeId).AddHours(plant.TimeOffset);
                        rackEvent.EventDate = rackEvent.EventDateTime.Date;
                        rackEvent.EventHour = rackEvent.EventDateTime.Hour;
                        rackEvent.Quantity = 1;
                        rackEvents.Add(rackEvent);
                    }

                    reader1.Close();
                }

                // Locked Events
                var cmd2 = GetLockedRackEventsByHour();
                cmd2.Connection = session.DbConnection;
                cmd2.Parameters[0].Value = plant.PlantNum.PadLeft(12);

                cmd2.Parameters[1].Value = (decimal)dateParms["StartDate"];
                cmd2.Parameters[2].Value = (decimal)dateParms["StartTime"];
                cmd2.Parameters[3].Value = (decimal)dateParms["MidDate1"];
                cmd2.Parameters[4].Value = (decimal)dateParms["MidDate2"];
                cmd2.Parameters[5].Value = (decimal)dateParms["EndDate"];
                cmd2.Parameters[6].Value = (decimal)dateParms["EndTime"];

                Logger.Trace(Logging.DbCmdString(cmd2));
                using (IDataReader reader2 = cmd2.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader2.Read())
                    {
                        var rackEvent = new RackEvent();
                        rackEvent.Plant = CastDbNull.To<String>(reader2["HLMCU"], String.Empty).Trim();
                        rackEvent.RackId = CastDbNull.To<Decimal>(reader2["HLZ58RID"], 0);
                        var status = CastDbNull.To<String>(reader2["HLZ58HST"], String.Empty).Trim();
                        var loadNum = CastDbNull.To<Decimal>(reader2["HLPSN"], 0);
                        if (status == "5" && loadNum == 0)
                        {
                            rackEvent.EventType = "LOCK";
                        }
                        else
                        {
                            rackEvent.EventType = "LCKL";
                        }

                        var dateId = CastDbNull.To<Decimal>(reader2["HLUPMJ"], 0);
                        var timeId = CastDbNull.To<Decimal>(reader2["HLTDAY"], 0);
                        rackEvent.EventDateTime = Common.Utilities.Common.CastToDateTime(dateId, timeId).AddHours(plant.TimeOffset);
                        rackEvent.EventDate = rackEvent.EventDateTime.Date;
                        rackEvent.EventHour = rackEvent.EventDateTime.Hour;
                        rackEvent.Quantity = 1;
                        rackEvents.Add(rackEvent);
                    }

                    reader2.Close();
                }

                DateTime i = enterpriseStartDate;
                while (i <= enterpriseEndDate)
                {
                    int j = 0;
                    while (j <= 23)
                    {
                        var summary = new RackEventHourSummary();
                        summary.Plant = plant.PlantNum;
                        summary.EventDate = i;
                        summary.EventHour = j;
                        summary.ClosedRacks = rackEvents.Count(s => s.EventDate == i && s.EventHour == j && s.EventType == "CLOS");
                        summary.LockedRacks = rackEvents.Count(s => s.EventDate == i && s.EventHour == j && s.EventType == "LOCK");
                        summary.LockedLoadRacks = rackEvents.Count(s => s.EventDate == i && s.EventHour == j && s.EventType == "LCKL");
                        rackHourSummarySet.Add(summary);
                        j++;
                    }
                    i = i.AddDays(1);
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return rackHourSummarySet;
        }

        public RackEventHourSummarySet GetRackEventsByHourOLD(BusinessSystemSession session, SqlConnection cn, Plant plant, string dateCode, DateTime? startDate, DateTime? endDate)
        {
            var rackEvents = new RackEvents();
            var rackHourSummarySet = new RackEventHourSummarySet();
            try
            {
                Hashtable dateRange = new Hashtable(); ;
                if (dateCode.ToUpper().Equals("RANGE"))
                {
                    dateRange.Add("StartDate", (DateTime)startDate);
                    dateRange.Add("EndDate", (DateTime)endDate);
                }
                else
                {
                    dateRange = GetDateCodeRange(cn, dateCode);
                }
                var enterpriseStartDate = (DateTime)dateRange["StartDate"];
                var enterpriseEndDate = (DateTime)dateRange["EndDate"];

                var plantStartDateTime = enterpriseStartDate.AddHours(-plant.TimeOffset);
                var plantEndDateTime = enterpriseEndDate.AddDays(1).AddHours(-plant.TimeOffset);

                var cmd = GetRackEventsByHour();
                cmd.Connection = session.DbConnection;
                cmd.Parameters[0].Value = plant.PlantNum.PadLeft(12);

                var dateParms = GetE1DateParms(plantStartDateTime, plantEndDateTime);
                cmd.Parameters[1].Value = (decimal)dateParms["StartDate"];
                cmd.Parameters[2].Value = (decimal)dateParms["StartTime"];
                cmd.Parameters[3].Value = (decimal)dateParms["MidDate1"];
                cmd.Parameters[4].Value = (decimal)dateParms["MidDate2"];
                cmd.Parameters[5].Value = (decimal)dateParms["EndDate"];
                cmd.Parameters[6].Value = (decimal)dateParms["EndTime"];

                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var rackEvent = new RackEvent();
                        rackEvent.Plant = CastDbNull.To<String>(reader["HLMCU"], String.Empty).Trim();
                        rackEvent.RackId = CastDbNull.To<Decimal>(reader["HLZ58RID"], 0);
                        var status = CastDbNull.To<String>(reader["HLZ58HST"], String.Empty).Trim();
                        var loadNum = CastDbNull.To<Decimal>(reader["HLPSN"], 0);
                        if (status == "4")
                        {
                            rackEvent.EventType = "CLOS";
                        }
                        else
                        {
                            if (status == "5" && loadNum == 0)
                            {
                                rackEvent.EventType = "LOCK";
                            }
                            else
                            {
                                rackEvent.EventType = "LCKL";
                            }
                        }

                        var dateId = CastDbNull.To<Decimal>(reader["HLUPMJ"], 0);
                        var timeId = CastDbNull.To<Decimal>(reader["HLTDAY"], 0);
                        rackEvent.EventDateTime = Common.Utilities.Common.CastToDateTime(dateId, timeId).AddHours(plant.TimeOffset);
                        rackEvent.EventDate = rackEvent.EventDateTime.Date;
                        rackEvent.EventHour = rackEvent.EventDateTime.Hour;
                        rackEvent.Quantity = 1;
                        rackEvents.Add(rackEvent);
                    }

                    reader.Close();
                }

                DateTime i = enterpriseStartDate;
                while (i <= enterpriseEndDate)
                {
                    int j = 0;
                    while (j <= 23)
                    {
                        var summary = new RackEventHourSummary();
                        summary.Plant = plant.PlantNum;
                        summary.EventDate = i;
                        summary.EventHour = j;
                        summary.ClosedRacks = rackEvents.Count(s => s.EventDate == i && s.EventHour == j && s.EventType == "CLOS");
                        summary.LockedRacks = rackEvents.Count(s => s.EventDate == i && s.EventHour == j && s.EventType == "LOCK");
                        summary.LockedLoadRacks = rackEvents.Count(s => s.EventDate == i && s.EventHour == j && s.EventType == "LCKL");
                        rackHourSummarySet.Add(summary);
                        j++;
                    }
                    i = i.AddDays(1);
                }

            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return rackHourSummarySet;
        }

        /*********************************************************************************
     * Date Range
     *******************************************************************************/
        public SqlCommand GetDateCodeRange()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              MIN(CAL_DATE) AS START_DATE ,
              MAX(CAL_DATE) AS END_DATE
            FROM
              DW.CALENDAR_DATE_ATTRIBUTES CDA
              INNER JOIN DW.CALENDAR_DATE_TYPE CDT
              ON CDT.DATE_TYPE_ID = CDA.CAL_DATE_TYPE_ID
            WHERE
              CDT.DATE_TYPE_CD = @DateCode;";
            cmd.Parameters.Add(new SqlParameter("@DateCode", SqlDbType.VarChar, 12));
            return cmd;
        }

        public Hashtable GetDateCodeRange(SqlConnection cn, string dateCode)
        {
            var hash = new Hashtable();
            try
            {
                var cmd = GetDateCodeRange();
                cmd.Connection = cn;
                cmd.Parameters[0].Value = dateCode;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        hash.Add("StartDate", CastDbNull.To<DateTime>(reader["START_DATE"], new DateTime(1900, 1, 1)));
                        hash.Add("EndDate", CastDbNull.To<DateTime>(reader["END_DATE"], new DateTime(1900, 1, 1)));
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }

            return hash;
        }

        public Hashtable GetE1DateParms(DateTime startDateTime, DateTime endDateTime)
        {
            var hash = new Hashtable();
            try
            {
                if (startDateTime.Date == endDateTime.Date)
                {
                    hash.Add("StartDate", Common.Utilities.Common.CastToE1Date(startDateTime));
                    hash.Add("StartTime", Common.Utilities.Common.CastToE1Time(startDateTime));
                    hash.Add("MidDate1", (decimal)-1);
                    hash.Add("MidDate2", (decimal)-1);
                    hash.Add("EndDate", (decimal)-1);
                    hash.Add("EndTime", (decimal)-1);
                }
                else
                {
                    if ((endDateTime.Date - startDateTime.Date).TotalDays == 1)
                    {
                        hash.Add("StartDate", Common.Utilities.Common.CastToE1Date(startDateTime));
                        hash.Add("StartTime", Common.Utilities.Common.CastToE1Time(startDateTime));
                        hash.Add("MidDate1", (decimal)-1);
                        hash.Add("MidDate2", (decimal)-1);
                        hash.Add("EndDate", Common.Utilities.Common.CastToE1Date(endDateTime));
                        hash.Add("EndTime", Common.Utilities.Common.CastToE1Time(endDateTime));
                    }
                    else
                    {
                        hash.Add("StartDate", Common.Utilities.Common.CastToE1Date(startDateTime));
                        hash.Add("StartTime", Common.Utilities.Common.CastToE1Time(startDateTime));
                        hash.Add("MidDate1", Common.Utilities.Common.CastToE1Date(startDateTime.Date.AddDays(1)));
                        hash.Add("MidDate2", Common.Utilities.Common.CastToE1Date(endDateTime.Date.AddDays(-1)));
                        hash.Add("EndDate", Common.Utilities.Common.CastToE1Date(endDateTime));
                        hash.Add("EndTime", Common.Utilities.Common.CastToE1Time(endDateTime));                       
                    }
                }
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            return hash;
        }
    }
}
