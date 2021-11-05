using System;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Text;

namespace VelocityApp.Common.Managers
{
    class ConnectionManager
    {
        public static SqlConnection GetSqlConnection(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!builder.MultiSubnetFailover) IsPingable(builder.DataSource);
            var sqlCn = new SqlConnection { ConnectionString = connectionString };
            try
            {
                sqlCn.ConnectionString = connectionString;
                return sqlCn;
            }
            catch (Exception e)
            {
                var eMsg = e.Message;
                var message = "ERROR using: [" + connectionString + "]";
                throw new Exception(message);
            }
        }

        public static void IsPingable(string hostName)
        {
            var pingSender = new Ping();
            var options = new PingOptions { DontFragment = true };
            const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var buffer = Encoding.ASCII.GetBytes(data);
            const int timeout = 30;
            try
            {
                var reply = pingSender.Send(hostName, timeout, buffer, options);
            }
            catch (Exception e)
            {
                var eMsg = e.Message;
                var message = hostName + " NOT Pingable";
                throw new Exception(message);
            }
        }

    }
}
