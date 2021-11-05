using System;
using System.Data;
using System.Data.SqlClient;
using BIServices.CgiisSvc;
using BIServices.Common.Types.Response;

namespace BIServices.Common.Managers.Cgiis
{
    public class CgiisManager
    {
        public CgiisManager()
        {
        }

        /**********************************************************************
         *  Jobs
         *********************************************************************/
        public SqlCommand GetJob()
        {
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              bu.MCU ,
              tfr.E1TransferTypeName ,
              sub.E1MappingSubscriptionId ,
              sub.E1MappingTableId ,
              tbl.MappingTableDesc ,
              ds.E1MappingDataSourceId ,
              ds.ConnectionString ,
              sub.TableName ,
              tbl.E1StagingTableName ,
              tbl.E1StagingTableView ,
              sub.SelectionCriteria ,
              sub.Active ,
              sub.IsEnabled
            FROM
              E1MappingSubscription sub
              LEFT OUTER JOIN E1MappingTable tbl
              ON tbl.E1MappingTableId = sub.E1MappingTableId
              LEFT OUTER JOIN E1BusinessUnit bu
              ON bu.E1BusinessUnitId = sub.E1BusinessUnitId
              LEFT OUTER JOIN E1MappingDataSource ds
              ON ds.E1MappingDataSourceId = sub.E1MappingDataSourceId
              LEFT OUTER JOIN E1MappingDataSource ds1
              ON ds1.E1MappingDataSourceId = tbl.E1MappingDataSourceId
              LEFT OUTER JOIN E1TransferType tfr
              ON tfr.E1TransferTypeId = tbl.E1TransferTypeId
            WHERE
              bu.MCU = @JobName";
            cmd.Parameters.Add(new SqlParameter("@JobName", SqlDbType.VarChar, 30));
            return cmd;
        }


        public SubmitResponse SubmitJob(string jobName)
        {
            var response = new SubmitResponse();
            try
            {
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
            finally
            {
            }
            return response;
        }
    }
}
