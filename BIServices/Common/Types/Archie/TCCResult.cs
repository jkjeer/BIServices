using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BIServices.Common.Types.Archie
{
    [CollectionDataContract(Namespace = "")]
    public class TCCResults : List<TCCResult>
    {
        public int ElepasedSeconds()
        {
            var elapsedSeconds = 0;
            try
            {
                DateTime startDate = this.FirstOrDefault().StartDate;
                DateTime endDate = this.LastOrDefault().EndDate;
                elapsedSeconds = (int) (endDate - startDate).TotalSeconds;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            return elapsedSeconds;
        }
    }

    [DataContract(Namespace = "")]

    public class TCCResult
    {
        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string Step { get; set; }
        [DataMember(Order = 20, EmitDefaultValue = false)]
        public int Selected { get; set; }
        [DataMember(Order = 30, EmitDefaultValue = false)]
        public int Updated { get; set; }
        [DataMember(Order = 40, EmitDefaultValue = false)]
        public int Deleted { get; set; }
        [DataMember(Order = 50, EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 60, EmitDefaultValue = false)]
        public DateTime EndDate { get; set; }
        [DataMember(Order = 70, EmitDefaultValue = false)]
        public string GUID { get; set; }
        [DataMember(Order = 80, EmitDefaultValue = false)]
        public int ElapsedSeconds { get; set; }
    }
}
