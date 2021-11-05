using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using IBM.Data.DB2.iSeries;

namespace BIServices.Common.Types.Response
{
    [CollectionDataContract(Namespace = "")]
    public class SqlDmlResponses : List<SqlDmlResponse>
    {
    }

    [DataContract(Namespace = "")]
    public class SqlDmlResponse
    {
        public SqlDmlResponse()
        {
            Errors = new Errors();
        }

        public SqlDmlResponse(SqlCommand cmd)
        {
            ConnectionString = cmd.Connection.ConnectionString;
            Statement = Regex.Replace(Regex.Replace(Regex.Replace(cmd.CommandText, " +", " "), "\r\n", ""), "\t", "").Trim();
            HandlerClass = cmd.GetType().ToString();
            Parameters = new List<Parameter>();
            foreach (SqlParameter iParameter in cmd.Parameters)
            {
                var parameter = new Parameter
                {
                    Name = iParameter.ParameterName
                };
                if (iParameter.Value != null) parameter.Value = iParameter.Value.ToString();
                Parameters.Add(parameter);
            }
            StartTime = DateTime.Now.ToLocalTime();
        }

        public SqlDmlResponse(iDB2Command cmd)
        {
            ConnectionString = cmd.Connection.ConnectionString;
            Statement = Regex.Replace(Regex.Replace(Regex.Replace(cmd.CommandText, " +", " "), "\r\n", ""), "\t", "").Trim();
            HandlerClass = cmd.GetType().ToString();
            Parameters = new List<Parameter>();
            foreach (iDB2Parameter iParameter in cmd.Parameters)
            {
                var parameter = new Parameter
                {
                    Name = iParameter.ParameterName,
                };
                if (iParameter.Value != null) parameter.Value = iParameter.Value.ToString();
                Parameters.Add(parameter);
            }
            StartTime = DateTime.Now.ToLocalTime();
        }

        [DataMember(Order = 10)]
        public string ConnectionString { get; set; }
        [DataMember(Order = 20)]
        public string Name { get; set; }
        [DataMember(Order = 50)]
        public string Statement { get; set; }
        [DataMember(Order = 55)]
        public List<Parameter> Parameters { get; set; }
        [DataMember(Order = 60)]
        public int RowsAffected { get; set; }
        [DataMember(Order = 70)]
        public Errors Errors { get; set; }

        [DataMember(Order = 100)]
        public DateTime StartTime { get; set; }
        [DataMember(Order = 110)]
        public DateTime EndTime { get; set; }
        [DataMember(Order = 112)]
        public decimal ElapsedSeconds { get; set; }

        [DataMember(Order = 210)]
        public string Message { get; set; }

        [DataMember(Order = 300)]
        public string HandlerClass { get; set; }

        public void Init(SqlCommand cmd)
        {
            StartTime = DateTime.Now.ToLocalTime();
            ConnectionString = cmd.Connection.ConnectionString;
            Statement = Regex.Replace(Regex.Replace(Regex.Replace(cmd.CommandText, " +", " "), "\r\n", ""), "\t", "").Trim();
            HandlerClass = cmd.GetType().ToString();
            Parameters = new List<Parameter>();
            foreach (SqlParameter iParameter in cmd.Parameters)
            {
                var parameter = new Parameter
                {
                    Name = iParameter.ParameterName
                };
                if (iParameter.Value != null) parameter.Value = iParameter.Value.ToString();
                Parameters.Add(parameter);
            }
        }

        public void Init(iDB2Command cmd)
        {
            StartTime = DateTime.Now.ToLocalTime();
            ConnectionString = cmd.Connection.ConnectionString;
            Statement = Regex.Replace(Regex.Replace(Regex.Replace(cmd.CommandText, " +", " "), "\r\n", ""), "\t", "").Trim();
            HandlerClass = cmd.GetType().ToString();
            Parameters = new List<Parameter>();
            foreach (iDB2Parameter iParameter in cmd.Parameters)
            {
                var parameter = new Parameter
                {
                    Name = iParameter.ParameterName,
                };
                if (iParameter.Value != null) parameter.Value = iParameter.Value.ToString();
                Parameters.Add(parameter);
            }
        }

        public void Done()
        {
            EndTime = DateTime.Now;
            ElapsedSeconds = (decimal)(EndTime - StartTime).TotalSeconds;
        }

        public void Done(int rowsAffected)
        {
            RowsAffected = rowsAffected;
            EndTime = DateTime.Now;
            ElapsedSeconds = (decimal)(EndTime - StartTime).TotalSeconds;
        }

        public bool HasErrors()
        {
            return Errors.Any();
        }
    }

    public class Parameter
    {
        [DataMember(Order = 10)]
        public string Name { get; set; }
        [DataMember(Order = 20)]
        public string Value { get; set; }
    }
}
