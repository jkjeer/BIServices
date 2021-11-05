using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Response;
using SoaCore.Wcf;

namespace BIServices.CgiisSvc
{
    [ServiceContract]
    public interface ICgiisService
    {
        [OperationContract, OperationInvokerLogger]
        SubmitResponse SubmitJob(string jobName);
    }

    [ServiceContract]
    public interface ICgiisRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/SubmitJob?JobName={jobName}")]
        SubmitResponse SubmitJob(string jobName);
    }
}

