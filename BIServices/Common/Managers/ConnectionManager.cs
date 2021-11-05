using System;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Text;
using IBM.Data.DB2.iSeries;

namespace BIServices.Common.Managers
{
    public static class ConnectionManager
    {

        public static OleDbConnection GetOleDbConnection(string connectionString)
        {
            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(connectionString);
            IsPingable(builder["Network Address"].ToString());
            OleDbConnection OleDbCn = new OleDbConnection();
            OleDbCn.ConnectionString = connectionString;
            try
            {
                OleDbCn.ConnectionString = connectionString;
                return OleDbCn;
            }
            catch (Exception e)
            {
                string message = "ERROR using: [" + connectionString + "]";
                throw new Exception(message);
            }
        }

        public static SqlConnection GetSqlConnection(string connectionString)
        {
            //OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder(connectionString);
            var builder = new SqlConnectionStringBuilder(connectionString);
            IsPingable(builder.DataSource);
            var sqlCn = new SqlConnection();
            sqlCn.ConnectionString = connectionString;
            try
            {
                sqlCn.ConnectionString = connectionString;
                return sqlCn;
            }
            catch (Exception e)
            {
                string message = "ERROR using: [" + connectionString + "]";
                throw new Exception(message);
            }
        }

        public static SqlConnection GetSqlConnectionFromKey(string keyValue)
        {
            var cn = new SqlConnection();
            var builder = new SqlConnectionStringBuilder();
            try
            {
                builder.ConnectionString = ConfigurationManager.ConnectionStrings[keyValue].ConnectionString;
                IsPingable(builder.DataSource);
                cn.ConnectionString = builder.ConnectionString;
            }
            catch (Exception e)
            {
                //string errorMsg = "Connection String Key not found for " + keyValue;
                string errorMsg = e.Message;
                throw new Exception(errorMsg);
            }
            return cn; 
        }


        public static iDB2Connection GetDb2Connection(string connectionString)
        {
            iDB2ConnectionStringBuilder builder = new iDB2ConnectionStringBuilder(connectionString);
            try
            {
                IsPingable(builder.DataSource);
                var sqlCn = new iDB2Connection();
                sqlCn.ConnectionString = connectionString;
                try
                {
                    sqlCn.ConnectionString = connectionString;
                    return sqlCn;
                }
                catch (Exception e)
                {
                    string message = "ERROR using: [" + connectionString + "]";
                    throw new Exception(message);
                }
            } 
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public static void IsPingable(String hostName)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 30;
            try
            {
                PingReply reply = pingSender.Send(hostName, timeout, buffer, options);
            }
            catch (Exception e)
            {
                string message = hostName + " NOT Pingable";
                throw new Exception(message);
            }
        }

        public static string GetE1ConnectionString()
        {
            String connectionString = "";
            String connectionKey = "";
            try
            {
                var sb = new StringBuilder();
                sb.Append(ConfigurationManager.AppSettings["Environment"]);
                sb.Append(".E1");
                connectionKey = sb.ToString();
                connectionString = ConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            }
            catch (Exception e)
            {
                string errorMsg = "Connection String Key not found for " + connectionKey;
                throw new Exception(errorMsg);
            }
            return connectionString;            
        }

        public static string GetSFConnectionString(String plantCode, String databaseName)
        {
            String connectionString = "";
            String connectionKey = "";
            try
            {
                var sb = new StringBuilder();
                sb.Append(ConfigurationManager.AppSettings["Environment"]);
                sb.Append(".");
                sb.Append(plantCode);
                sb.Append(".");
                sb.Append(databaseName);
                connectionKey = sb.ToString();
                connectionString = ConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            }
            catch (Exception e)
            {
                string errorMsg = "Connection String Key not found for " + connectionKey;
                throw new Exception(errorMsg);
            }
            return connectionString;      
        }

        public static string GetDb2ConnectionString()
        {
            String connectionString = "";
            String connectionKey = "";
            try
            {
                var sb = new StringBuilder();
                sb.Append(ConfigurationManager.AppSettings["Environment"]);
                sb.Append(".iSeries");
                connectionKey = sb.ToString();
                connectionString = ConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            }
            catch (Exception e)
            {
                string errorMsg = "Connection String Key not found for " + connectionKey;
                throw new Exception(errorMsg);
            }
            return connectionString;            
        }

        public static string GetConnectionString(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch (Exception e)
            {
                string errorMsg = "Connection String Key not found for " + name;
                throw new Exception(errorMsg);
            }
        }
    }
}
