using BIServices.Common.Types.Response;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BIServices.Common.Utilities
{
    public static class Sql
    {
        public static SqlDmlResponses SqlBatchExecution(List<string> statements, short batchSize, SqlConnection cn, SqlTransaction txn = null)
        {
            var responses = new SqlDmlResponses();
            var cmd = new SqlCommand();
            cmd.Connection = cn;
            if (txn != null)
            {
                cmd.Transaction = txn;
            }

            var sb = new StringBuilder();
            int i = 0;
            try
            {
                foreach (var statement in statements)
                {
                    i++;
                    sb.Append(statement);
                    if (i % batchSize == 0 || i == statements.Count)
                    {
                        var dmlResponse = new SqlDmlResponse();
                        cmd.CommandText = sb.ToString();
                        dmlResponse.Init(cmd);
                        int rows = cmd.ExecuteNonQuery();
                        dmlResponse.RowsAffected = rows;
                        responses.Add(dmlResponse);
                        sb.Clear();
                    }
                }
                txn?.Commit();
            }
            catch (Exception e)
            {
                txn?.Rollback();
                throw new Exception(cn.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            return responses;
        }

        public class SqlFilterSet : List<SqlFilter>
        {
            public override string ToString()
            {
                var filterString = new StringBuilder();
                foreach (SqlFilter filter in this)
                {
                    if (filter == this.Last())
                    {
                        filterString.Append($"{filter.Text}");
                    }
                    else
                    {
                        filterString.Append($"{filter.Text} {filter.Operator} ");
                    }
                }
                return filterString.ToString();
            }

            public string ToEnclosedString()
            {
                var filterString = new StringBuilder();
                filterString.Append("(");
                foreach (SqlFilter filter in this)
                {
                    if (filter == this.Last())
                    {
                        filterString.Append($"{filter.Text}");
                    }
                    else
                    {
                        filterString.Append($"{filter.Text} {filter.Operator} ");
                    }
                }
                filterString.Append(")");
                return filterString.ToString();
            }
        }

        public class SqlFilters : List<SqlFilter>
        {
            public override string ToString()
            {
                var filterString = new StringBuilder();
                if (this.Count == 0)
                {
                    filterString.Append("1=1");
                }
                else
                {
                    foreach (SqlFilter filter in this)
                    {
                        if (filter == this.Last())
                        {
                            filterString.Append($"{filter.Text}");
                        }
                        else
                        {
                            filterString.Append($"{filter.Text} {filter.Operator} ");
                        }
                    }
                }
                return filterString.ToString();
            }
        }

        public class SqlFilter
        {
            public SqlFilter(string text, string operation)
            {
                Text = text;
                Operator = operation;
            }

            public string Text { get; set; }
            public string Operator { get; set; }
        }

        public static string GetSqlFilterTokens(int count)
        {
            var sb = new StringBuilder();
            sb.Insert(0, "?,", count - 1);
            sb.Append("?");
            return sb.ToString();
        }


    }
}
