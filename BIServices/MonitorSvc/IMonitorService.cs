using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;
// ReSharper disable InconsistentNaming

namespace BIServices.MonitorSvc
{
    [ServiceContract]

    public interface IMonitorService
    {
        [OperationContract, OperationInvokerLogger]
        string GetHostName(string serviceAliasName);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse GetProcessLogErrors(int lookBackMinutes);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse GetSSISLogErrors(int lookBackSeconds);

        [OperationContract, OperationInvokerLogger]
        ProcessResponses GetSSISLogExecutions(int lookBackSeconds);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse CheckE1HALatency();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DuplicateUdcAuditE1();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse DuplicateUdcAuditDw();
    }

    [ServiceContract]
    public interface IMonitorRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetHostName?ServiceAliasName={serviceAliasName}")]
        string GetHostName(string serviceAliasName);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetProcessLogErrors?LookBackMinutes={lookBackMinutes}")]
        ProcessResponse GetProcessLogErrors(int lookBackMinutes);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSSISLogErrors?LookBackSeconds={lookBackSeconds}")]
        ProcessResponse GetSSISLogErrors(int lookBackSeconds);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSSISLogExecutions?LookBackSeconds={lookBackSeconds}")]
        ProcessResponses GetSSISLogExecutions(int lookBackSeconds);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/CheckE1HALatency")]
        ProcessResponse CheckE1HALatency();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DuplicateUdcAuditE1")]
        ProcessResponse DuplicateUdcAuditE1();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/DuplicateUdcAuditDw")]
        ProcessResponse DuplicateUdcAuditDw();
    }
}
