using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VelocityApp.Common.Types
{
    [CollectionDataContract(Namespace = "")]
    public class ConnectionStrings : List<ConnectionString>
    {
    }

    [DataContract(Namespace = "")]
    public class ConnectionString
    {
        [DataMember(Order = 10)]
        public string Name { get; set; }
        [DataMember(Order = 20)]
        public string Value { get; set; }

        public ConnectionString()
        {
        }

        public ConnectionString(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
