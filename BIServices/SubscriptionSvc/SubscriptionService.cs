using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoaCore.Wcf;

namespace BIServices.SubscriptionSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class SubscriptionService : ISubscriptionService
    {
        public string GetSubscription(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            try
            {
                return SubscriptionProcessor.GetSubscription(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class SubscriptionRestService : ISubscriptionRestService
    {
        public string GetSubscription(string plantCode)
        {
            if (string.IsNullOrEmpty(plantCode))
            {
                throw new Exception("Plant Code is REQUIRED.");
            }
            try
            {
                return SubscriptionProcessor.GetSubscription(plantCode);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
