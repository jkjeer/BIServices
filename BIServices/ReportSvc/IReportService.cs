using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ReportSvc
{
    [ServiceContract]
    public interface IReportService
    {
        [OperationContract, OperationInvokerLogger]
        ProcessResponse UpdateReportParameter(string workspaceName, string reportName, string parameterName, string newValue);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse PublishReport(string workspaceNameSource, string reportNameSource, string workspaceNameTarget, string reportNameTarget, string parameterName, string newValue);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse PublishFile(string folderName, string fileName, string workspaceNameTarget, string reportNameTarget, string parameterName, string newValue);

        [OperationContract, OperationInvokerLogger]
        ProcessResponse PBITest();
    }

    [ServiceContract]
    public interface IReportRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/UpdateReportParameter?WorkspaceName={workspaceName}&ReportName={reportName}&ParameterName={parameterName}&NewValue={newValue}")]
        ProcessResponse UpdateReportParameter(string workspaceName, string reportName, string parameterName, string newValue);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/PublishReport?WorkspaceNameSource={workspaceNameSource}&ReportNameSource={reportNameSource}&WorkspaceNameTarget={workspaceNameTarget}&ReportNameTarget={reportNameTarget}&ParameterName={parameterName}&NewValue={newValue}")]
        ProcessResponse PublishReport(string workspaceNameSource, string reportNameSource, string workspaceNameTarget, string reportNameTarget, string parameterName, string newValue);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/PublishFile?FolderName={folderName}&FileName={fileName}&WorkspaceNameTarget={workspaceNameTarget}&ReportNameTarget={reportNameTarget}&ParameterName={parameterName}&NewValue={newValue}")]
        ProcessResponse PublishFile(string folderName, string fileName, string workspaceNameTarget, string reportNameTarget, string parameterName, string newValue);

        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/PBITest")]
        ProcessResponse PBITest();

    }

}
