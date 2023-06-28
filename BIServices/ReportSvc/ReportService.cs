using System;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ReportSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class ReportService : IReportService
    {
        public ProcessResponse UpdateReportParameter(string workspaceName, string reportName, string parameterName, string newValue)
        {
            if (string.IsNullOrEmpty(workspaceName))
            {
                throw new ApplicationException("Workspace Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportName))
            {
                throw new ApplicationException("Report Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ApplicationException("Parameter Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(newValue))
            {
                throw new ApplicationException("New Value is REQUIRED.");
            }
            var processor = new ReportProcessor();
            try
            {
                return processor.UpdateReportParameter(workspaceName, reportName, parameterName, newValue);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse PublishReport(string workspaceNameSource, string reportNameSource, string workspaceTarget, string reportNameTarget, string parameterName, string newValue)
        {
            if (string.IsNullOrEmpty(workspaceNameSource))
            {
                throw new ApplicationException("Workspace Name Source is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportNameSource))
            {
                throw new ApplicationException("Report Name Source is REQUIRED.");
            }
            if (string.IsNullOrEmpty(workspaceTarget))
            {
                throw new ApplicationException("Workspace Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportNameTarget))
            {
                throw new ApplicationException("Report Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ApplicationException("Parameter Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(newValue))
            {
                throw new ApplicationException("New Value is REQUIRED.");
            }
            var processor = new ReportProcessor();
            try
            {
                return processor.PublishReport(workspaceNameSource, reportNameSource, workspaceTarget, reportNameTarget, parameterName, newValue);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse PublishFile(string folderName, string fileName, string workspaceTarget, string reportNameTarget, string parameterName, string newValue)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ApplicationException("Folder Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ApplicationException("File Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(workspaceTarget))
            {
                throw new ApplicationException("Workspace Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportNameTarget))
            {
                throw new ApplicationException("Report Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ApplicationException("Parameter Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(newValue))
            {
                throw new ApplicationException("New Value is REQUIRED.");
            }
            var processor = new ReportProcessor();
            try
            {
                return processor.PublishFile(folderName, fileName, workspaceTarget, reportNameTarget, parameterName, newValue);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse PBITest()
        {
            var processor = new ReportProcessor();
            try
            {
                return processor.PBITest();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class ReportRestService : IReportRestService
    {
        public ProcessResponse UpdateReportParameter(string workspaceName, string reportName, string parameterName, string newValue)
        {
            var processor = new ReportProcessor();
            if (string.IsNullOrEmpty(workspaceName))
            {
                throw new ApplicationException("Workspace Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportName))
            {
                throw new ApplicationException("Report Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ApplicationException("Parameter Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(newValue))
            {
                throw new ApplicationException("New Value is REQUIRED.");
            }
            try
            {
                return processor.UpdateReportParameter(workspaceName, reportName, parameterName, newValue);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse PublishReport(string workspaceNameSource, string reportNameSource, string workspaceTarget, string reportNameTarget, string parameterName, string newValue)
        {
            if (string.IsNullOrEmpty(workspaceNameSource))
            {
                throw new ApplicationException("Workspace Name Source is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportNameSource))
            {
                throw new ApplicationException("Report Name Source is REQUIRED.");
            }
            if (string.IsNullOrEmpty(workspaceTarget))
            {
                throw new ApplicationException("Workspace Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportNameTarget))
            {
                throw new ApplicationException("Report Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ApplicationException("Parameter Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(newValue))
            {
                throw new ApplicationException("New Value is REQUIRED.");
            }
            var processor = new ReportProcessor();
            try
            {
                return processor.PublishReport(workspaceNameSource, reportNameSource, workspaceTarget, reportNameTarget, parameterName, newValue);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse PublishFile(string folderName, string fileName, string workspaceTarget, string reportNameTarget, string parameterName, string newValue)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ApplicationException("Folder Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ApplicationException("File Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(workspaceTarget))
            {
                throw new ApplicationException("Workspace Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(reportNameTarget))
            {
                throw new ApplicationException("Report Name Target is REQUIRED.");
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ApplicationException("Parameter Name is REQUIRED.");
            }
            if (string.IsNullOrEmpty(newValue))
            {
                throw new ApplicationException("New Value is REQUIRED.");
            }
            var processor = new ReportProcessor();
            try
            {
                return processor.PublishFile(folderName, fileName, workspaceTarget, reportNameTarget, parameterName, newValue);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse PBITest()
        {
            var processor = new ReportProcessor();
            try
            {
                return processor.PBITest();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

    }

}
