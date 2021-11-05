using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using SoaCore.Wcf;
// ReSharper disable InconsistentNaming

namespace BIServices.IntegrationSvc
{
    [ServiceContract]
    public interface IIntegrationService
    {
        [OperationContract, OperationInvokerLogger]
        Common.Types.Integration.Subscriptions GetSubscriptions(string plantCode);

        [OperationContract, OperationInvokerLogger]
        List<String> GetPlantsWithInactiveSubscriptions();

        [OperationContract, OperationInvokerLogger]
        SqlDmlResponse ActivateSubscriptions(string plantCode);

        [OperationContract, OperationInvokerLogger]
        SqlDmlResponse InactivateSubscriptions(string plantCode);

        [OperationContract, OperationInvokerLogger]
        String GetScheduleAgentStatus();

        [OperationContract, OperationInvokerLogger]
        PingResults GetPingResults(string plantCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ResetPYIntegrationSFUKID();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ResetPYIntegrationBIUKID();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ResetPYIntegrationQAUKID();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ResetPYIntegrationBIEDBT();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ResetPYIntegrationSFEDBT();

        [OperationContract, OperationInvokerLogger]
        ProcessResponse ResetPYHighWaterMarks();
    }

    [ServiceContract]
    public interface IIntegrationRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetSubscriptions?PlantCode={plantCode}")]
        Common.Types.Integration.Subscriptions GetSubscriptions(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetPlantsWithInactiveSubscriptions")]
        List<String> GetPlantsWithInactiveSubscriptions();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ActivateSubscriptions?PlantCode={plantCode}")]
        SqlDmlResponse ActivateSubscriptions(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/InactivateSubscriptions?PlantCode={plantCode}")]
        SqlDmlResponse InactivateSubscriptions(string plantCode);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetScheduleAgentStatus")]
        String GetScheduleAgentStatus();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/StartScheduleAgent")]
        Boolean StartScheduleAgent();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/StopScheduleAgent")]
        Boolean StopScheduleAgent();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetPingResults?Plant={plantCode}&StartDate={startDate}&EndDate={endDate}")]
        PingResults GetPingResults(string plantCode, string startDate, string endDate);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ResetPYIntegrationSFUKID")]
        ProcessResponse ResetPYIntegrationSFUKID();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ResetPYIntegrationBIUKID")]
        ProcessResponse ResetPYIntegrationBIUKID();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ResetPYIntegrationQAUKID")]
        ProcessResponse ResetPYIntegrationQAUKID();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ResetPYIntegrationBIEDBT")]
        ProcessResponse ResetPYIntegrationBIEDBT();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ResetPYIntegrationSFEDBT")]
        ProcessResponse ResetPYIntegrationSFEDBT();

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/ResetPYHighWaterMarks")]
        ProcessResponse ResetPYHighWaterMarks();
    }
}
