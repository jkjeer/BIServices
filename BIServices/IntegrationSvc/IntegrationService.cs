using System;
using System.Collections.Generic;
using BIServices.Common.Types.Enterprise;
using BIServices.Common.Types.Response;
using SoaCore.Wcf;

namespace BIServices.IntegrationSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class IntegrationService : IIntegrationService
    {
        public Common.Types.Integration.Subscriptions GetSubscriptions(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }

            try
            {
                return IntegrationProcessor.GetSubscriptions(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public List<String> GetPlantsWithInactiveSubscriptions()
        {
            try
            {
                return IntegrationProcessor.GetPlantsWithInactiveSubscriptions();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public SqlDmlResponse ActivateSubscriptions(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }

            try
            {
                return IntegrationProcessor.ActivateSubscriptions(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public SqlDmlResponse InactivateSubscriptions(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }

            try
            {
                return IntegrationProcessor.InactivateSubscriptions(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public String GetScheduleAgentStatus()
        {
            try
            {
                return IntegrationProcessor.GetScheduleAgentStatus();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public PingResults GetPingResults(string plantCode, string startDate, string endDate)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }

            try
            {
                return IntegrationProcessor.GetPingResults(plantCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationSFUKID()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationSFUKID();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationBIUKID()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationBIUKID();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationQAUKID()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationQAUKID();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationBIEDBT()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationBIEDBT();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationSFEDBT()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationSFEDBT();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYHighWaterMarks()
        {
            try
            {
                return IntegrationProcessor.ResetPYHighWaterMarks();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class IntegrationRestService : IIntegrationRestService
    {
        public Common.Types.Integration.Subscriptions GetSubscriptions(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            try
            {
                return IntegrationProcessor.GetSubscriptions(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public List<String> GetPlantsWithInactiveSubscriptions()
        {
            try
            {
                return IntegrationProcessor.GetPlantsWithInactiveSubscriptions();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public SqlDmlResponse ActivateSubscriptions(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            try
            {
                return IntegrationProcessor.ActivateSubscriptions(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public SqlDmlResponse InactivateSubscriptions(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            try
            {
                return IntegrationProcessor.InactivateSubscriptions(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public String GetScheduleAgentStatus()
        {
            try
            {
                return IntegrationProcessor.GetScheduleAgentStatus();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public Boolean StartScheduleAgent()
        {
            try
            {
                return IntegrationProcessor.StartScheduleAgent();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public Boolean StopScheduleAgent()
        {
            try
            {
                return IntegrationProcessor.StopScheduleAgent();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public PingResults GetPingResults(string plantCode, string startDate, string endDate)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new ApplicationException("Plant Code is REQUIRED.");
            }
            try
            {
                return IntegrationProcessor.GetPingResults(plantCode, startDate, endDate);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationSFUKID()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationSFUKID();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationBIUKID()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationBIUKID();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationQAUKID()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationQAUKID();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationBIEDBT()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationBIEDBT();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse ResetPYIntegrationSFEDBT()
        {
            try
            {
                return IntegrationProcessor.ResetPYIntegrationSFEDBT();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
        public ProcessResponse ResetPYHighWaterMarks()
        {
            try
            {
                return IntegrationProcessor.ResetPYHighWaterMarks();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
