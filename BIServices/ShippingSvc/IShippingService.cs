using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using SoaCore.Wcf;

namespace BIServices.ShippingSvc
{
    [ServiceContract]
    public interface IShippingService
    {
        [OperationContract, OperationInvokerLogger]
        Racks GetShippedRacks(string plantCode, decimal customerNum, string dateCode, string startDate, string endDate);
    }

    [ServiceContract]
    public interface IShippingRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetShippedRacks?Plant={plantCode}&CustomerNum={customerNum}&DateCode={dateCode}&StartDate={startDate}&EndDate={endDate}")]
        Racks GetShippedRacks(string plantCode, decimal customerNum, string dateCode, string startDate, string endDate);
    }
}
