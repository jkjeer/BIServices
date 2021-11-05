using System;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;
using NLog;

namespace BIServices.ShippingSvc
{
    public class ShippingProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static Racks GetShippedRacks(string plants, decimal customerNum, string dateCode, string startDate, string endDate)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            try
            {
                plants = plants.Replace("'", "");
                dateCode = dateCode?.Replace("'", "");
                return ShippingManager.GetShippedRacks(session, plants, customerNum, dateCode, startDate, endDate);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
            }
        }

    }
}