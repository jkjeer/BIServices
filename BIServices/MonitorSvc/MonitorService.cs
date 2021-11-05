using System;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.MonitorSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class MonitorService : IMonitorService
    {
        public string GetHostName(string serviceAliasName)
        {
            if (string.IsNullOrEmpty(serviceAliasName))
            {
                throw new ApplicationException("ServiceAliasName is REQUIRED.");
            }
            try
            {
                return MonitorProcessor.GetHostName(serviceAliasName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

        }

        public ProcessResponse GetProcessLogErrors(int lookBackMinutes = 0)
        {
            try
            {
                return MonitorProcessor.GetProcessLogErrors(lookBackMinutes);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse GetSSISLogErrors(int lookBackSeconds = 0)
        {
            try
            {
                return MonitorProcessor.GetSSISLogErrors(lookBackSeconds);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponses GetSSISLogExecutions(int lookBackSeconds = 0)
        {
            try
            {
                return MonitorProcessor.GetSSISLogExecutions(lookBackSeconds);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse CheckE1HALatency()
        {
            try
            {
                return MonitorProcessor.CheckE1HALatency();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DuplicateUdcAuditE1()
        {
            try
            {
                return MonitorProcessor.DuplicateUdcAuditE1();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DuplicateUdcAuditDw()
        {
            try
            {
                return MonitorProcessor.DuplicateUdcAuditDw();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class MonitorRestService : IMonitorRestService
    {
        public string GetHostName(string serviceAliasName)
        {
            if (string.IsNullOrEmpty(serviceAliasName))
            {
                throw new ApplicationException("ServiceAliasName is REQUIRED.");
            }
            try
            {
                return MonitorProcessor.GetHostName(serviceAliasName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse GetProcessLogErrors(int lookBackMinutes = 0)
        {
            try
            {
                return MonitorProcessor.GetProcessLogErrors(lookBackMinutes);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse GetSSISLogErrors(int lookBackSeconds = 0)
        {
            try
            {
                return MonitorProcessor.GetSSISLogErrors(lookBackSeconds);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponses GetSSISLogExecutions(int lookBackSeconds = 0)
        {
            try
            {
                return MonitorProcessor.GetSSISLogExecutions(lookBackSeconds);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse CheckE1HALatency()
        {
            try
            {
                return MonitorProcessor.CheckE1HALatency();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DuplicateUdcAuditE1()
        {
            try
            {
                return MonitorProcessor.DuplicateUdcAuditE1();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public ProcessResponse DuplicateUdcAuditDw()
        {
            try
            {
                return MonitorProcessor.DuplicateUdcAuditDw();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
