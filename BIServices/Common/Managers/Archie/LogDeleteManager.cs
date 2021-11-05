using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BIServices.Common.Managers.Enterprise;
using BIServices.Common.Types.Archie;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;

namespace BIServices.Common.Managers.Archie
{
    public static class LogDeleteManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Servers GetPlantConfig(SqlConnection cnArchCfg, string plantName)
        {
            var servers = new Servers();
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();
            try
            {
                filters.Add("[Plant] = @plant");
                parameters.Add(new SqlParameter("@plant", SqlDbType.VarChar));
                parameters.Last().Value = plantName;
                if (filters.Any())
                {
                    var cmd = GetPlantConfig(filters);
                    foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                    cmd.Connection = cnArchCfg;
                    //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            var server = new Server();
                            server.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                            server.NetBiosName = CastDbNull.To(reader["NetBiosName"], string.Empty).Trim();
                            server.WorkloadType = CastDbNull.To(reader["WorkloadType"], string.Empty).Trim();
                            server.ServerFunction = CastDbNull.To(reader["ServerFunction"], string.Empty).Trim();
                            server.DeploymentMode = CastDbNull.To(reader["DeploymentMode"], string.Empty).Trim();
                            server.ServerType = CastDbNull.To(reader["ServerType"], string.Empty).Trim();
                            server.IsActive = CastDbNull.To(reader["IsActive"], false);
                            server.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                            server.IsSQL = CastDbNull.To(reader["IsSQL"], false);
                            server.DeleteLogFiles = CastDbNull.To(reader["DeleteLogFiles"], false);
                            server.LogFileRetentionDays = CastDbNull.To(reader["LogFileRetentionDays"], 0);
                            server.Plant = PlantManager.GetPlant(CastDbNull.To(reader["Plant"], string.Empty).Trim());
                            servers.Add(server);
                        }

                        Logger.Trace($"Rows: {servers.Count}");
                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new Exception($"{cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
            }

            return servers;
        }

        private static SqlCommand GetPlantConfig(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters) filterString.Append(filter);
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                SELECT
                    [ID]
                    ,[Plant]
                    ,[ServerName]
                    ,[NetBiosName]
                    ,[WorkloadType]
                    ,[ServerFunction]
                    ,[DeploymentMode]
                    ,[ServerType]
                    ,[IsActive]
                    ,[IsEnabled]
                    ,[IsSQL]
                    ,[DeleteLogFiles]
                    ,[LogFileRetentionDays]
                    ,[BITimeStamp]
                    ,[PlantCode]                
                FROM
                    [dbo].[cfgServersLogDelete]
                WHERE
                     {filterString} 
                ";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

            return cmd;
        }

        public static Server GetServerConfig(SqlConnection cnArchCfg, string serverName)
        {
            var server = new Server();
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();
            try
            {
                filters.Add("[ServerName] = @serverName");
                parameters.Add(new SqlParameter("@serverName", SqlDbType.VarChar));
                parameters.Last().Value = serverName;
                if (filters.Any())
                {
                    var cmd = GetServerConfig(filters);
                    foreach (var parameter in parameters) cmd.Parameters.Add(parameter);
                    cmd.Connection = cnArchCfg;
                    //cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["BoxingQueryTimeout"]);
                    Logger.Trace(Logging.DbCmdString(cmd));
                    using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            server.ServerName = CastDbNull.To(reader["ServerName"], string.Empty).Trim();
                            server.NetBiosName = CastDbNull.To(reader["NetBiosName"], string.Empty).Trim();
                            server.WorkloadType = CastDbNull.To(reader["WorkloadType"], string.Empty).Trim();
                            server.ServerFunction = CastDbNull.To(reader["ServerFunction"], string.Empty).Trim();
                            server.DeploymentMode = CastDbNull.To(reader["DeploymentMode"], string.Empty).Trim();
                            server.ServerType = CastDbNull.To(reader["ServerType"], string.Empty).Trim();
                            server.IsActive = CastDbNull.To(reader["IsActive"], false);
                            server.IsEnabled = CastDbNull.To(reader["IsEnabled"], false);
                            server.IsSQL = CastDbNull.To(reader["IsSQL"], false);
                            server.DeleteLogFiles = CastDbNull.To(reader["DeleteLogFiles"], false);
                            server.LogFileRetentionDays = CastDbNull.To(reader["LogFileRetentionDays"], 0);
                            server.Plant = PlantManager.GetPlant(CastDbNull.To(reader["Plant"], string.Empty).Trim());
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"ERROR: {cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
                throw new Exception($"{cnArchCfg.ConnectionString}  {e.Message}: {e.StackTrace}");
            }

            return server;
        }

        private static SqlCommand GetServerConfig(List<string> filters)
        {
            var filterString = new StringBuilder();
            foreach (var filter in filters) filterString.Append(filter);
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $@"
                SELECT
                    [ID]
                    ,[Plant]
                    ,[ServerName]
                    ,[NetBiosName]
                    ,[WorkloadType]
                    ,[ServerFunction]
                    ,[DeploymentMode]
                    ,[ServerType]
                    ,[IsActive]
                    ,[IsEnabled]
                    ,[IsSQL]
                    ,[DeleteLogFiles]
                    ,[LogFileRetentionDays]
                    ,[BITimeStamp]
                    ,[PlantCode]                
                FROM
                    [dbo].[cfgServersLogDelete]
                WHERE
                     {filterString} 
                ";
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

            return cmd;
        }
        
        public static ValidationErrors Validate(Server server, int requestedRetentionDays)
        {
            var messages = new ValidationErrors();
            var minRetentionDays = Convert.ToInt32(ConfigurationManager.AppSettings["MinLogRetentionDays"]);
            if (requestedRetentionDays < server.LogFileRetentionDays)
            {
                var err = new ValidationError($"Server {server.ServerName} Requested Retention Days {requestedRetentionDays} is less than server {server.ServerName} is configuration.", "Validation", "Fatal", true);
                messages.Add(err);
            }

            if (requestedRetentionDays < minRetentionDays)
            {
                var err = new ValidationError($"Server {server.ServerName} Requested Retention Days {requestedRetentionDays} is less than Minimum Retention Days.", "Validation", "Fatal", true);
                messages.Add(err);
            }

            if (!server.DeleteLogFiles)
            {
                var err = new ValidationError($"Server {server.ServerName} is not configured for DeleteLogFiles.", "Validation", "Fatal", true);
                messages.Add(err);
            }

            if (!server.IsActive)
            {
                var err = new ValidationError($"Server {server.ServerName} is Inactive.", "Validation", "Fatal", true);
                messages.Add(err);
            }

            if (!server.IsEnabled)
            {
                var err = new ValidationError($"Server {server.ServerName} is Disabled.", "Validation", "Fatal", true);
                messages.Add(err);
            }

            return messages;
        }
    }
}