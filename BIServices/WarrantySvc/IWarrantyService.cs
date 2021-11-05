using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using BIServices.Common.Types.Enterprise;
using BIServices.HvMtoSvc;
using SoaCore.Wcf;

namespace BIServices.WarrantySvc
{

    [ServiceContract]
    public interface IWarrantyService
    {
        [OperationContract, OperationInvokerLogger]
        ProducedUnits GetLvUnit(string plant, decimal unitId, DateTime startDate, DateTime endDate);
    }

    [ServiceContract]
    public interface IWarrantyRestService
    {
        [OperationContract, OperationInvokerLogger]
        [WebGet(UriTemplate = "/GetLvUnit?Plant={plant}&UnitId={unitId}&StartDate={startDate}&EndDate={endDate}")]
        ProducedUnits GetLvUnit(string plant, decimal unitId, DateTime startDate, DateTime endDate);
    }

}
