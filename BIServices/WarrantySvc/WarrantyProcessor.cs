using System;
using System.Data.SqlClient;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.E1;
using BIServices.Common.Types.Enterprise;

namespace BIServices.WarrantySvc
{
    public class WarrantyProcessor
    {
        public ProducedUnits GetLvUnit(string plant, decimal unitId, DateTime startDate, DateTime endDate)
        {
            var session = new BusinessSystemSession();
            var cn = new SqlConnection();
            try
            {
                cn = ConnectionManager.GetSqlConnectionFromKey("DW");
                cn.Open();
                session = BusinessSystemSessionManager.GetBusinessSession();
                var plantCode = plant.Replace("'", "");
                var plantObj = PlantManager.GetPlant(plantCode);
                var mgr = new WarrantyMtoManager(session);
                return mgr.GetLvUnit(session, cn, plantObj, unitId);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
                session.Close();
                cn.Close();
            }
        }

    }
}