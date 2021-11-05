using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BIServices.Common.Types.Response;
using BIServices.Common.Utilities;
using NLog;
// ReSharper disable InconsistentNaming

namespace BIServices.Common.Managers.Enterprise
{
    public class SSISLogManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SSISLogErrors GetSSISLogErrors(SqlConnection cnSSIS, int lookBackSeconds)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var ssisLogErrors = new SSISLogErrors();
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();
            try
            {
                var lookBackDateTime = DateTime.Now.AddSeconds(-lookBackSeconds);
                filters.Add("[EVENT_DATE] >= @lookBackDateTime");
                //parameters.Add(new SqlParameter("@lookBackDateTime", SqlDbType.DateTime));
                //parameters.Last().Value = lookBackDateTime;
                var cmd = GetSSISLogErrors(lookBackDateTime);
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = cnSSIS;
                Logger.Trace(Logging.DbCmdString(cmd));
                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var ssisLogError = new SSISLogError();
                        ssisLogError.Caller = CastDbNull.To(reader["caller"], "");
                        ssisLogError.Package = CastDbNull.To(reader["package"], "");
                        ssisLogError.ExecuTableName = CastDbNull.To(reader["executable_name"], "");
                        var messageTime = CastDbNull.To<DateTimeOffset>(reader["message_time"], default(DateTimeOffset));
                        ssisLogError.MessageTime = messageTime.DateTime;
                        ssisLogError.Information = CastDbNull.To(reader["information"], "");
                        ssisLogError.ExecutionID = CastDbNull.To<long>(reader["execution_id"], 0);
                        ssisLogErrors.Add(ssisLogError);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: {ssisLogErrors.Count}");
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnSSIS.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnSSIS.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return ssisLogErrors;
        }

        public static SqlCommand GetSSISLogErrors(DateTime lookBackDateTime)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.usp_GET_EXECUTION_ERRORS_BY_DATE";
                cmd.Parameters.Add(new SqlParameter("@DateVal", SqlDbType.DateTime));
                cmd.Parameters["@DateVal"].Value = lookBackDateTime;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }

        public static SSISLogExecutions GetSSISLogExecutions(SqlConnection cnSSIS, int lookBackSeconds)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            Logger.Info($"lookBackSeconds: {lookBackSeconds}");
            var ssisLogExecutions = new SSISLogExecutions();
            try
            {
                var lookBackDateTime = DateTime.Now.AddSeconds(-lookBackSeconds);
                var cmd = GetSSISLogRecentExecutions(lookBackDateTime);
                cmd.Connection = cnSSIS;
                Logger.Trace(Logging.DbCmdString(cmd));

                using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (reader.Read())
                    {
                        var ssisLogExecution = new SSISLogExecution();
                        ssisLogExecution.Package = CastDbNull.To(reader["package_name"], "");
                        ssisLogExecution.Result = CastDbNull.To(reader["result"], "");
                        var executionStartTime = CastDbNull.To(reader["start_time"], default(DateTimeOffset));
                        ssisLogExecution.ExecutionStartTime = executionStartTime.DateTime;
                        ssisLogExecution.ExecutionID = CastDbNull.To<long>(reader["execution_id"], 0);

                        ssisLogExecutions.Add(ssisLogExecution);
                    }

                    reader.Close();
                }

                Logger.Trace($"ROWS: {ssisLogExecutions.Count}");

                foreach (var exec in ssisLogExecutions)
                {
                    var paramCmd = GetSSISLogExecutionParameters(exec.ExecutionID);
                    paramCmd.Connection = cnSSIS;
                    Logger.Trace(Logging.DbCmdString(paramCmd));

                    using (IDataReader param = paramCmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        while (param.Read())
                        {
                            exec.UseHighWaterMarks = CastDbNull.To(param["usehighwatermarks"], false);
                            exec.HighWaterOffsetMinutes = CastDbNull.To(param["highwateroffsetminutes"], 0);
                            exec.StartDate = CastDbNull.To(param["startdate"], "");
                            exec.EndDate = CastDbNull.To(param["enddate"], "");
                            exec.StartRelativeDays = CastDbNull.To(param["startrelativedays"], 0);
                            exec.EndRelativeDays = CastDbNull.To(param["endrelativedays"], 0);
                        }

                        param.Close();
                    }

                    var summaryCmd = GetSSISLogExecutionSummary(exec.ExecutionID);
                    summaryCmd.Connection = cnSSIS;
                    Logger.Trace(Logging.DbCmdString(summaryCmd));

                    exec.Summary = new SSISLogExecutionSteps();
                    exec.Detail = new SSISLogExecutionPackages();
                    using (IDataReader summary = summaryCmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (summary.Read())
                        {
                            var step = new SSISLogExecutionStep();
                            step.Path = CastDbNull.To(summary["execution_path"], "");
                            step.Result = CastDbNull.To(summary["result"], "");
                            var stepStartTime = CastDbNull.To(summary["start_time"], default(DateTimeOffset));
                            step.StepStartTime = stepStartTime.DateTime;
                            var stepEndTime = CastDbNull.To(summary["end_time"], default(DateTimeOffset));
                            step.StepEndTime = stepEndTime.DateTime;
                            step.DurationSeconds = CastDbNull.To<decimal>(summary["duration_seconds"], 0m);
                            step.StepNum = CastDbNull.To(summary["step_num"], 0);
                            exec.Summary.Add(step);

                            var pkg = new SSISLogExecutionPackage();
                            pkg.PackagePath = step.Path;
                            pkg.StepNum = step.StepNum;
                            pkg.Steps = new SSISLogExecutionSteps();
                            exec.Detail.Add(pkg);
                        }

                        summary.Close();
                    }

                    Logger.Trace($"SUMMARY ROWS: {exec.Summary.Count}");

                    var detailCmd = GetSSISLogExecutionDetail(exec.ExecutionID);
                    detailCmd.Connection = cnSSIS;
                    Logger.Trace(Logging.DbCmdString(detailCmd));

                    using (IDataReader detail = detailCmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (detail.Read())
                        {
                            var step = new SSISLogExecutionStep();
                            step.Path = CastDbNull.To(detail["execution_path"], "");
                            step.StepName = CastDbNull.To(detail["executable_name"], "");
                            step.Result = CastDbNull.To(detail["result"], "");
                            var stepStartTime = CastDbNull.To(detail["start_time"], default(DateTimeOffset));
                            step.StepStartTime = stepStartTime.DateTime;
                            var stepEndTime = CastDbNull.To(detail["end_time"], default(DateTimeOffset));
                            step.StepEndTime = stepEndTime.DateTime;
                            step.DurationSeconds = CastDbNull.To<decimal>(detail["duration_seconds"], 0);
                            step.StepInfo = CastDbNull.To(detail["information"], "");

                            if (!exec.Detail.Exists(d => d.PackagePath == step.Path))
                            {
                                // only necessary in cases where package terminated unexpectedly and there is missing log data
                                exec.Detail.Add(new SSISLogExecutionPackage()
                                {
                                    PackagePath = step.Path,
                                    StepNum = 9999,
                                    Steps = new SSISLogExecutionSteps()
                                });
                            }
                            exec.Detail.Where(d => d.PackagePath == step.Path).First().Steps.Add(step);
                        }

                        detail.Close();
                    }

                    Logger.Trace($"DETAIL ROWS: {exec.Detail.Count}");
                }
                Logger.Trace($"EXECUTION ROWS: {ssisLogExecutions.Count}");
            }
            catch (Exception e)
            {
                Logger.Error("ERROR: " + cnSSIS.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
                throw new ApplicationException(cnSSIS.ConnectionString + "  " + e.Message + ": " + e.StackTrace);
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return ssisLogExecutions;
        }

        public static SqlCommand GetSSISLogRecentExecutions(DateTime lookBackDateTime)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.usp_GET_EXECUTIONS_COMPLETED_AFTER";
                cmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.DateTime));
                cmd.Parameters["@Date"].Value = lookBackDateTime;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }

        // This is intended mostly for testing purposes and is not used directly by the process
        public static SqlCommand GetSSISLogExecution(int executionId)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.usp_GET_EXECUTION_BY_ID";
                cmd.Parameters.Add(new SqlParameter("@ExecutionId", SqlDbType.Int));
                cmd.Parameters["@ExecutionId"].Value = executionId;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }

        public static SqlCommand GetSSISLogExecutionParameters(long executionID)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.usp_GET_EXECUTION_PARAMETERS";
                cmd.Parameters.Add(new SqlParameter("@ExecutionID", SqlDbType.BigInt));
                cmd.Parameters["@ExecutionID"].Value = executionID;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }

        public static SqlCommand GetSSISLogExecutionSummary(long executionID)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.usp_GET_EXECUTION_SUMMARY";
                cmd.Parameters.Add(new SqlParameter("@ExecutionID", SqlDbType.BigInt));
                cmd.Parameters["@ExecutionID"].Value = executionID;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }

        public static SqlCommand GetSSISLogExecutionDetail(long executionID)
        {
            var cmd = new SqlCommand();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.usp_GET_EXECUTION_DETAIL";
                cmd.Parameters.Add(new SqlParameter("@ExecutionID", SqlDbType.BigInt));
                cmd.Parameters["@ExecutionID"].Value = executionID;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            return cmd;
        }
    }
}
