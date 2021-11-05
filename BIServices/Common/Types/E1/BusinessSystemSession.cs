using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.ServiceModel;
using BIServices.Common.Types.Enterprise;
using IBM.Data.DB2.iSeries;

namespace BIServices.Common.Types.E1
{
    [DataContract(Namespace = "")]
    public class BusinessSystemSession
    {
        [DataMember(Order = 10)]
        public iDB2Connection DbConnection;
        [DataMember(Order = 20)]
        public Dictionary<string, iDB2Command> DbCommands;
        [DataMember(Order = 30)]
        public Dictionary<string, object> Managers;
        [DataMember(Order = 40)]
        public PlantMap PlantMap;
        [DataMember(Order = 50)]
        public bool IsOpen;

        public DateTime GetCurrentTimestamp()
        {
            var dt = new DateTime();
            try
            {
                var cmd = GetCurrentTimestampStmt();
                cmd.Connection = this.DbConnection;
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    while (reader.Read())
                    {
                        try
                        {
                            dt = Utilities.CastDbNull.To<DateTime>(reader["TS"], new DateTime(1900, 1, 1));
                        }
                        catch (Exception e)
                        {
                            throw new ApplicationException(e.ToString());
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }

            return dt;
        }

        public iDB2Command GetCurrentTimestampStmt()
        {
            var cmd = new iDB2Command();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
            SELECT
              CURRENT TIMESTAMP AS TS
            FROM
              SYSIBM.SYSDUMMY1";
            return cmd;
        }


        public void Close()
        {
            try
            {
                DbConnection.Close();
                IsOpen = false;
            }
            catch (ApplicationException e)
            {
                throw new FaultException(e.Message);
            }
        }

    }
}
