using System;

namespace BIServices.Common.Utilities
{
    public class E1TimeSpan
    {
        public decimal DateComponent1 { get; set; }
        public decimal TimeComponent1 { get; set; }
        public decimal DateComponent2 { get; set; }
        public decimal DateComponent3 { get; set; }

        public E1TimeSpan()
        {
            DateComponent1 = 0;
            TimeComponent1 = 0;
            DateComponent2 = 0;
            DateComponent3 = 0;
        }

        public void SetTimeSpan(TimeSpan ts)
        {
            DateTime nd = DateTime.Now.Subtract(ts);
            DateComponent1 = CastToE1Date(nd);
            TimeComponent1 = nd.Hour * 10000 + nd.Minute * 100 + nd.Second;
            DateComponent2 = CastToE1Date(nd.AddDays(1));
            DateComponent3 = CastToE1Date(DateTime.Now);
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
