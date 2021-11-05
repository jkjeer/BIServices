using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.ServiceModel;
using NLog;

namespace VelocityApp.Common.Types
{
    [CollectionDataContract(Namespace = "")]
    public class Errors : List<Error>
    {
    }

    [DataContract(Namespace = "")]
    public class Error
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        public Error()
        {
        }

        public Error(string message, bool notify = true)
        {
            Message = message;
            if (notify)
            {
                LogEvent(new Exception(message));
            }
        }

        public Error(Exception e, bool notify = true)
        {
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
            if (notify)
            {
                LogEvent(e);
            }
        }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public string Message { get; set; }

        [DataMember(Order = 30, EmitDefaultValue = false)]
        public string Source { get; set; }

        [DataMember(Order = 40, EmitDefaultValue = false)]
        public string StackTrace { get; set; }

        private void LogEvent(Exception e)
        {
            if (OperationContext.Current == null)
            {
                OperationContext.Current = (OperationContext)CallContext.LogicalGetData("WcfOperationContext");
            }
            OperationContext operationContext = OperationContext.Current;

            LogEventInfo logEvent = new LogEventInfo
            {
                Level = LogLevel.Error,
                LoggerName = GetType().FullName
            };
            logEvent.Properties.Add("EVENT_DATE", DateTime.Now);
            logEvent.Properties.Add("EVENT_TYPE", "ERROR");
            logEvent.Properties.Add("LOG_INTERFACE", "");
            logEvent.Properties.Add("ARGUMENTS", "");
            logEvent.Properties.Add("OPERATION_NAME", "");
            try
            {
                logEvent.Properties.Add("USER_NAME", operationContext.ServiceSecurityContext.WindowsIdentity.Name);
            }
            catch
            {
                logEvent.Properties.Add("USER_NAME", "");
            }
            try
            {
                logEvent.Properties.Add("CONTRACT_NAME", operationContext.EndpointDispatcher.ContractName);
            }
            catch
            {
                logEvent.Properties.Add("CONTRACT_NAME", "");
            }
            try
            {
                logEvent.Properties.Add("HOST_NAME", operationContext.InstanceContext.Host.Description?.Name);
            }
            catch
            {
                logEvent.Properties.Add("HOST_NAME", "");
            }
            try
            {
                logEvent.Properties.Add("SCHEME", operationContext.EndpointDispatcher.EndpointAddress.Uri.Scheme);
            }
            catch
            {
                logEvent.Properties.Add("SCHEME", "");
            }
            try
            {
                logEvent.Properties.Add("PORT", operationContext.EndpointDispatcher.EndpointAddress.Uri.Port);
            }
            catch
            {
                logEvent.Properties.Add("PORT", "");
            }
            try
            {
                logEvent.Properties.Add("SERVER_ADDRESS", operationContext.Channel.LocalAddress.Uri.ToString());
            }
            catch
            {
                logEvent.Properties.Add("SERVER_ADDRESS", "");
            }
            try
            {
                logEvent.Properties.Add("SESSION_ID", operationContext.SessionId);
            }
            catch
            {
                logEvent.Properties.Add("SESSION_ID", "");
            }
            logEvent.Properties.Add("ELAPSED_MS", 0);
            logEvent.Properties.Add("COMMENTS", $"{e.Message} {e.StackTrace}");
            logger.Log(logEvent);
        }

    }
}
