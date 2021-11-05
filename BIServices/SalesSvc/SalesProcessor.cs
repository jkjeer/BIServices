using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BIServices.Common.Managers.E1;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Managers.ShopFloor;
using BIServices.Common.Types.Enterprise;
using NLog;

namespace BIServices.SalesSvc
{
    public class SalesProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public SalesOrderLines GetInterlayerSales(string plantCodes, string customerSoldNums, string customerShipNums, string dateCode, string startDate, string endDate, string lastStatus, string nextStatus)
        {
            var session = BusinessSystemSessionManager.GetBusinessSession();
            var cnSf = new SqlConnection();
            var lines = new SalesOrderLines();
            try
            {
                var lastStatusCodes = new List<string>();
                var nextStatusCodes = new List<string>();
                if (!String.IsNullOrEmpty(lastStatus))
                {
                    lastStatusCodes = lastStatus.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(nextStatus))
                {
                    nextStatusCodes = nextStatus.Split(',').ToList();
                }
                var plantCodesClean = plantCodes.Replace("'", "");
                var plants = PlantManager.GetPlants(plantCodesClean);
                foreach (Plant plant in plants)
                {
                    try
                    {
                        cnSf = ConnectionManager.GetSqlConnectionFromKey(plant.PlantCode + ".ShopFloor");
                        cnSf.Open();
                        var mgr = new SalesManagerLG();
                        lines.AddRange(mgr.GetInterlayerSales(cnSf, plant, customerSoldNums, customerShipNums, dateCode, startDate, endDate, lastStatusCodes, nextStatusCodes));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.ToString());
                    }
                    finally
                    {
                        cnSf.Close();
                    }
                }
                return lines;
            }
            catch (Exception err)
            {
                throw new Exception(err.ToString());
            }
            finally
            {
                session.Close();
            }
        }


    }
}