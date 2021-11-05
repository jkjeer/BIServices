using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BIServices.Common.Types
{
    [CollectionDataContract(Namespace = "")]
    public class SqlDmlResponses : List<SqlDmlResponse>
    {
    }
}
