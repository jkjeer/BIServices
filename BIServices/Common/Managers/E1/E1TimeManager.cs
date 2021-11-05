using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Utilities;
using IBM.Data.DB2.iSeries;
using NLog;

namespace BIServices.Common.Managers.E1
{
    public class E1TimeManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static DateTime GetE1TimeStamp(BusinessSystemSession e1Session, string dateAddStr = null)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var e1Timestamp = DateTime.Now;
            try
            {
                var cmd = new iDB2Command
                {
                    Connection = e1Session.DbConnection,
                    CommandType = CommandType.Text,
                    CommandText = $@"SELECT CURRENT TIMESTAMP {dateAddStr} AS T FROM sysibm.sysdummy1"
                };
                Logger.Trace("CN: " + e1Session.DbConnection.ConnectionString);
                Logger.Trace("SQL: " + Environment.NewLine + cmd.CommandText);
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var counter = 0;
                    while (reader.Read())
                    {
                        e1Timestamp = CastDbNull.To(reader["T"], DateTime.Now.ToLocalTime());
                        counter++;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + counter);
                }
            }
            catch (Exception e)
            {
                Logger.Trace("ERROR: " + e.Message);
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }

            Logger.Trace("CLOSE: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            return e1Timestamp;
        }

        public static string GetE1TimeStampString(BusinessSystemSession e1Session, string dateAddStr = null)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var e1Timestamp = "";
            try
            {
                var cmd = new iDB2Command
                {
                    Connection = e1Session.DbConnection,
                    CommandType = CommandType.Text,
                    CommandText = $@"SELECT CHAR(CURRENT TIMESTAMP {dateAddStr}) AS T FROM sysibm.sysdummy1"
                };
                Logger.Trace("CN: " + e1Session.DbConnection.ConnectionString);
                Logger.Trace("SQL: " + Environment.NewLine + cmd.CommandText);
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var counter = 0;
                    while (reader.Read())
                    {
                        e1Timestamp = CastDbNull.To(reader["T"], "");
                        counter++;
                    }

                    reader.Close();
                    Logger.Trace("ROWS: " + counter);
                }
            }
            catch (Exception e)
            {
                Logger.Trace("ERROR: " + e.Message);
                throw new ApplicationException(e1Session.DbConnection.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }

            Logger.Trace("CLOSE: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            return e1Timestamp;
        }

        public static DateTime GetE1Date(BusinessSystemSession e1Session, string dateAddStr = null)
        {
            var e1Date = GetE1TimeStamp(e1Session, dateAddStr);
            e1Date = Convert.ToDateTime(e1Date.Date);
            return e1Date;
        }

        public static TimeSpan GetE1Time(BusinessSystemSession e1Session, string dateAddStr = null)
        {
            var e1Time = GetE1TimeStamp(e1Session, dateAddStr);
            return e1Time.TimeOfDay;
        }

        public static decimal GetE1JulianDate(BusinessSystemSession e1Session, string dateAddStr = null)
        {
            var e1Date = GetE1TimeStamp(e1Session, dateAddStr);
            e1Date = Convert.ToDateTime(e1Date.Date);
            var e1JulianDate = CastToE1Date(e1Date);
            return e1JulianDate;
        }
        public static decimal GetE1JulianTime(BusinessSystemSession e1Session, string dateAddStr = null)
        {
            var e1Time = GetE1TimeStamp(e1Session, dateAddStr);
            var e1JulianTime = CastToE1Time(e1Time);
            return e1JulianTime;
        }

        public static decimal CastToE1Date(DateTime dateTime)
        {
            var e1Date = Utilities.Common.CastToE1Date(dateTime);
            return e1Date;
        }

        public static decimal CastToE1Time(DateTime dateTime)
        {
            var e1Time = Utilities.Common.CastToE1Time(dateTime);
            return e1Time;
        }

        public static E1TimeSpan GetE1TimeSpan(TimeSpan span)
        {
            var tspan = new E1TimeSpan();
            tspan.SetTimeSpan(span);
            return tspan;
        }

        public static TimeIdSpan GetE1TimeIdSpan(DateTime startDate, DateTime endDate)
        {
            var span = new TimeIdSpan
            {
                StartDateId = CastToE1DateId(startDate),
                StartTimeId1 = CastToE1TimeId(startDate),
                StartTimeId2 = ((endDate.Date - startDate.Date).Days == 0) ? CastToE1TimeId(endDate) : 235959,
                MidDateId1 = ((endDate.Date - startDate.Date).Days > 1) ? CastToE1DateId(startDate.AddDays(1)) : -1,
                MidDateId2 = ((endDate.Date - startDate.Date).Days > 1) ? CastToE1DateId(endDate.AddDays(-1)) : -1,
                EndDateId = ((endDate.Date - startDate.Date).Days == 0) ? -1 : CastToE1DateId(endDate),
                EndTimeId = ((endDate.Date - startDate.Date).Days == 0) ? -1 : CastToE1TimeId(endDate)
            };
            return span;
        }


        public static string GetE1DateTimeQueryString(string dateFieldName, string timeFieldName, TimeIdSpan span)
        {
            var strReturn = "";
            if (span.StartTimeId2 != 235959)
            {
                strReturn = string.Format($"({dateFieldName} = {span.StartDateId} AND {timeFieldName} > {span.StartTimeId1} AND {timeFieldName} <= {span.StartTimeId2})");
            }
            else
            {
                if (span.MidDateId1 == -1)
                {
                    strReturn = string.Format($"(({dateFieldName} = {span.StartDateId} AND {timeFieldName} > {span.StartTimeId1}) OR ({dateFieldName} = {span.EndDateId} AND {timeFieldName} <= {span.EndTimeId}))");
                }
                else
                {
                    strReturn = string.Format($"(({dateFieldName} = {span.StartDateId} AND {timeFieldName} > {span.StartTimeId1}) OR ({dateFieldName} >= {span.MidDateId1} AND {dateFieldName} <= {span.MidDateId2}) OR ({dateFieldName} = {span.EndDateId} AND {timeFieldName} <= {span.EndTimeId}))");
                }
            }
            Logger.Debug(strReturn);

            return strReturn;
        }


        public static string GetE1DateTimeQueryString(string dateFieldName, string timeFieldName, DateTime startDate, DateTime? endDate = null)
        {
            var startDateId = Timestamp.CastToE1Date(startDate);
            var startTimeId = Timestamp.CastToE1Time(startDate);
            var midDateId1 = Timestamp.CastToE1Date(startDate.AddDays(1));

            if (endDate == null)
            {
                return string.Format($"(({dateFieldName} = {startDateId} AND {timeFieldName} > {startTimeId}) OR {dateFieldName} > {midDateId1})");
            }
            else
            {
                var endDate1 = (DateTime)endDate;
                var midDateId2 = Timestamp.CastToE1Date(endDate1.AddDays(-1));
                var endDateId = Timestamp.CastToE1Date(endDate1);
                var endTimeId = Timestamp.CastToE1Time(endDate1);
                if ((endDate1 - startDate).Days == 0)
                {
                    return string.Format($"{dateFieldName} = {startDateId} AND {timeFieldName} > {startTimeId} AND {timeFieldName} <= {endTimeId}");
                }
                if ((endDate1 - startDate).Days == 1)
                {
                    return string.Format($"(({dateFieldName} = {startDateId} AND {timeFieldName} > {startTimeId}) OR ({dateFieldName} = {endDateId} AND {timeFieldName} <= {endTimeId}))");
                }
                return string.Format($"(({dateFieldName} = {startDateId} AND {timeFieldName} > {startTimeId}) OR ({dateFieldName} >= {midDateId1} AND {dateFieldName} <= {midDateId2}) OR ({dateFieldName} = {endDateId} AND {timeFieldName} <= {endTimeId}))");
            }
        }

        public static string GetE1DateTimeQueryString(List<string> dateFieldNames, List<string> timeFieldNames, DateTime startDate, DateTime? endDate = null)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            var i = 0;
            var startDateId = Timestamp.CastToE1Date(startDate);
            var startTimeId = Timestamp.CastToE1Time(startDate);
            var midDateId1 = Timestamp.CastToE1Date(startDate.AddDays(1));
            if (endDate == null)
            {
                foreach (var dateFieldName in dateFieldNames)
                {
                    var timeFieldName = timeFieldNames[i];
                    i++;
                    sb.Append($@"(({dateFieldName} = {startDateId} AND {timeFieldName} >= {startTimeId}) OR {dateFieldName} >= {midDateId1}) OR ");
                }
            }
            else
            {
                var endDate1 = (DateTime)endDate;
                var midDateId2 = Timestamp.CastToE1Date(endDate1.AddDays(-1));
                var endDateId = Timestamp.CastToE1Date(endDate1);
                var endTimeId = Timestamp.CastToE1Time(endDate1);
                switch ((endDate1 - startDate).Days)
                {
                    case 0:
                        foreach (var dateFieldName in dateFieldNames)
                        {
                            var timeFieldName = timeFieldNames[i];
                            i++;
                            sb.Append($@"({dateFieldName} = {startDateId} AND {timeFieldName} >= {startTimeId} AND {timeFieldName} <= {endTimeId}) OR ");
                        }
                        break;
                    case 1:
                        foreach (var dateFieldName in dateFieldNames)
                        {
                            var timeFieldName = timeFieldNames[i];
                            i++;
                            sb.Append($@"(({dateFieldName} = {startDateId} AND {timeFieldName} >= {startTimeId}) OR ({dateFieldName} = {endDateId} AND {timeFieldName} <= {endTimeId})) OR ");
                        }
                        break;
                    default:
                        foreach (var dateFieldName in dateFieldNames)
                        {
                            var timeFieldName = timeFieldNames[i];
                            i++;
                            sb.Append($@"(({dateFieldName} = {startDateId} AND {timeFieldName} >= {startTimeId}) OR ({dateFieldName} >= {midDateId1} AND {dateFieldName} <= {midDateId2}) OR ({dateFieldName} = {endDateId} AND {timeFieldName} <= {endTimeId})) OR ");
                        }
                        break;
                }
            }
            sb.Length = sb.Length - 4;
            sb.Append(")");
            return sb.ToString();
        }

        public static decimal CastToE1DateId(DateTime inputDate)
        {
            return (inputDate.Year - 1900) * 1000 + inputDate.DayOfYear;
        }

        public static decimal CastToE1TimeId(DateTime inputDate)
        {
            return (inputDate.Hour * 10000) + (inputDate.Minute * 100) + inputDate.Second;
        }

        public static DateTime ConvertE1Date(decimal dateId)
        {
            DateTime dateBase = new DateTime(1900, 1, 1);
            int dateDays = 0;
            int dateYears = 0;
            try
            {
                string dateString = Convert.ToString(dateId, CultureInfo.CurrentCulture);
                int dateStringLength = dateString.Length;
                if (dateStringLength == 5 | dateStringLength == 6)
                {
                    dateDays = Convert.ToInt16(dateString.Substring(dateStringLength - 3, 3)) - 1;
                    dateYears = Convert.ToInt16(dateString.Substring(0, dateStringLength - 3));
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            return dateBase.AddYears(dateYears).AddDays(dateDays);
        }

        public static DateTime CastToDateTime(decimal dateId, decimal timeId = 0)
        {
            DateTime dt = ConvertE1Date(dateId);
            Data.CastDbNull.To<decimal>(timeId, 0);
            try
            {
                var timeString = Convert.ToString(timeId, CultureInfo.CurrentCulture).PadLeft(6).Replace(" ", "0");
                var hours = Convert.ToInt16(timeString.Substring(0, 2));
                var minutes = Convert.ToInt16(timeString.Substring(2, 2));
                var seconds = Convert.ToInt16(timeString.Substring(4, 2));
                TimeSpan ts = new TimeSpan(hours, minutes, seconds);
                return dt.Add(ts);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public static DateTime CastSttmToDateTime(string sttm)
        {
            try
            {
                var years = Convert.ToInt16(sttm.Substring(0, 4));
                var months = Convert.ToInt16(sttm.Substring(5, 2));
                var days = Convert.ToInt16(sttm.Substring(8, 2));
                var hours = Convert.ToInt16(sttm.Substring(11, 2));
                var minutes = Convert.ToInt16(sttm.Substring(14, 2));
                var seconds = Convert.ToInt16(sttm.Substring(17, 2));
                return new DateTime(years, months, days, hours, minutes, seconds);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
