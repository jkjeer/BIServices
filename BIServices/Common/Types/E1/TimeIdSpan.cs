using System;

namespace BIServices.Common.Types.E1
{
    public class TimeIdSpan
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal StartDateId { get; set; }
        public decimal StartTimeId1 { get; set; }
        public decimal StartTimeId2 { get; set; }
        public decimal MidDateId1 { get; set; }
        public decimal MidDateId2 { get; set; }
        public decimal EndDateId { get; set; }
        public decimal EndTimeId { get; set; }

        public TimeIdSpan()
        {
            StartDateId = 0;
            StartTimeId1 = 0;
            StartTimeId2 = 0;
            MidDateId1 = 0;
            MidDateId2 = 0;
            EndDateId = 0;
            EndTimeId = 0;
        }
    }
}
