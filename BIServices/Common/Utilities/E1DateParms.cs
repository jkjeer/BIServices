using System;

namespace BIServices.Common.Utilities
{
    public class E1DateParms
    {
        public decimal DateComponent1 { get; set; }
        public decimal TimeComponent1 { get; set; }
        public decimal DateComponent2 { get; set; }
        public decimal DateComponent3 { get; set; }

        public E1DateParms()
        {
            DateComponent1 = 0;
            TimeComponent1 = 0;
            DateComponent2 = 0;
            DateComponent3 = 0;
        }

        public void SetParms(TimeSpan ts)
        {
            DateTime nd = DateTime.Now.Subtract(ts);
            if (nd.Date == DateTime.Now.Date)
            {
                DateComponent1 = CastToE1Date(nd);
                TimeComponent1 = CastToE1Time(nd);
                DateComponent2 = -1;
                DateComponent3 = -1;
            }
            else
            {
                DateComponent1 = CastToE1Date(nd);
                TimeComponent1 = CastToE1Time(nd);
                DateComponent2 = CastToE1Date(nd.Date.AddDays(1));
                DateComponent3 = CastToE1Date(DateTime.Now.Date);
            }
        }

        public void SetParms(DateTime nd)
        {
            if (nd.Date == DateTime.Now.Date)
            {
                DateComponent1 = CastToE1Date(nd);
                TimeComponent1 = CastToE1Time(nd);
                DateComponent2 = -1;
                DateComponent3 = -1;
            }
            else
            {
                DateComponent1 = CastToE1Date(nd);
                TimeComponent1 = CastToE1Time(nd);
                DateComponent2 = CastToE1Date(nd.Date.AddDays(1));
                DateComponent3 = CastToE1Date(DateTime.Now.Date);
            }
        }

        private Decimal CastToE1Date(DateTime inputDate)
        {
            return (inputDate.Year - 1900) * 1000 + inputDate.DayOfYear;
        }

        private Decimal CastToE1Time(DateTime inputDate)
        {
            return (inputDate.Hour * 10000) + (inputDate.Minute * 100) + inputDate.Second;
        }
    }
}
