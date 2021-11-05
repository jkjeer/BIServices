using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class IncidentDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadIncident(SqlConnection cnDW, KMIEHS context, DateTime lookback)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving Incident records...");
                TruncateIncident(cnDW);
                var query = from i in context.Incident.Expand("History")
                            where i.History.LastEdited > new DateTime(lookback.Year, lookback.Month, lookback.Day)
                            select i;
                foreach (var o in query)
                {
                    //Console.WriteLine($"Incident ID: {o.IncidentId}");
                    Console.Write(".");
                    rowCount += InsertIncidentEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing Incident records");
                LoadIncident(cnDW);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int TruncateIncident(SqlConnection cnDW)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            int rowCount;
            try
            {
                var cmdTruncate = new SqlCommand
                {
                    Connection = cnDW,
                    CommandType = CommandType.Text,
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_INCIDENT]"
                };
                rowCount = cmdTruncate.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int InsertIncidentEntity(SqlConnection cnDW, IncidentEntity incidentEntity)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            int rowCount;
            try
            {
                var cmd = new SqlCommand
                {
                    Connection = cnDW,
                    CommandType = CommandType.Text,
                    CommandText = @"
                        INSERT INTO [source].[VELOCITY_INCIDENT]
                            ([IncidentId]
                              ,[ClientId]
                              ,[IncidentCategoryId]
                              ,[IncidentStatusId]
                              ,[LocationId]
                              ,[OffsiteLocationId]
                              ,[ContractingCompanyId]
                              ,[AddressId]
                              ,[ProcessId]
                              ,[ActivityId]
                              ,[OnPremises]
                              ,[Downtime]
                              ,[Ppeworn]
                              ,[InsuranceNotified]
                              ,[DrugTestPerformed]
                              ,[AlcoholTestPerformed]
                              ,[Rcaperformed]
                              ,[Preventable]
                              ,[Recurrence]
                              ,[ProcessSafety]
                              ,[WorkRelated]
                              ,[Litigable]
                              ,[Sensitive]
                              ,[AgencySensitive]
                              ,[Media]
                              ,[TitleV]
                              ,[IncludeInStatistics]
                              ,[IncidentDate]
                              ,[ReportDate]
                              ,[InvestigationStartDate]
                              ,[EndDate]
                              ,[InsuranceNotifiedDate]
                              ,[DrugTestDate]
                              ,[AlcoholTestDate]
                              ,[DowntimeAmount]
                              ,[Latitude]
                              ,[Longitude]
                              ,[WindSpeed]
                              ,[TimeToExtinguish]
                              ,[DowntimeUnitCode]
                              ,[WindSpeedUnitCode]
                              ,[TimeToExtinguishUnitCode]
                              ,[IncidentNumber]
                              ,[Title]
                              ,[ReportedBy]
                              ,[ReporterPhone]
                              ,[InsuranceClaim]
                              ,[Equipment]
                              ,[Vehicle]
                              ,[OffPremisesLocation]
                              ,[OtherLocation]
                              ,[OtherContractingCompany]
                              ,[OtherProcess]
                              ,[OtherActivity]
                              ,[OtherPpe]
                              ,[OtherWeather]
                              ,[Description]
                              ,[ActivityDuringIncident]
                              ,[ImmediateAction]
                              ,[ImmediateCauses]
                              ,[CausalFactors]
                              ,[BestPractices]
                              ,[RiskAction]
                              ,[RiskCondition]
                              ,[InsuranceInfo]
                              ,[Active]
                              ,[Archived]
                              ,[HistoryId])
                        VALUES
                            ( @IncidentId
                              ,@ClientId
                              ,@IncidentCategoryId
                              ,@IncidentStatusId
                              ,@LocationId
                              ,@OffsiteLocationId
                              ,@ContractingCompanyId
                              ,@AddressId
                              ,@ProcessId
                              ,@ActivityId
                              ,@OnPremises
                              ,@Downtime
                              ,@Ppeworn
                              ,@InsuranceNotified
                              ,@DrugTestPerformed
                              ,@AlcoholTestPerformed
                              ,@Rcaperformed
                              ,@Preventable
                              ,@Recurrence
                              ,@ProcessSafety
                              ,@WorkRelated
                              ,@Litigable
                              ,@Sensitive
                              ,@AgencySensitive
                              ,@Media
                              ,@TitleV
                              ,@IncludeInStatistics
                              ,@IncidentDate
                              ,@ReportDate
                              ,@InvestigationStartDate
                              ,@EndDate
                              ,@InsuranceNotifiedDate
                              ,@DrugTestDate
                              ,@AlcoholTestDate
                              ,@DowntimeAmount
                              ,@Latitude
                              ,@Longitude
                              ,@WindSpeed
                              ,@TimeToExtinguish
                              ,@DowntimeUnitCode
                              ,@WindSpeedUnitCode
                              ,@TimeToExtinguishUnitCode
                              ,@IncidentNumber
                              ,@Title
                              ,@ReportedBy
                              ,@ReporterPhone
                              ,@InsuranceClaim
                              ,@Equipment
                              ,@Vehicle
                              ,@OffPremisesLocation
                              ,@OtherLocation
                              ,@OtherContractingCompany
                              ,@OtherProcess
                              ,@OtherActivity
                              ,@OtherPpe
                              ,@OtherWeather
                              ,@Description
                              ,@ActivityDuringIncident
                              ,@ImmediateAction
                              ,@ImmediateCauses
                              ,@CausalFactors
                              ,@BestPractices
                              ,@RiskAction
                              ,@RiskCondition
                              ,@InsuranceInfo
                              ,@Active
                              ,@Archived
                              ,@HistoryId)"
                };
                cmd.Parameters.Add("@IncidentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ClientId", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentCategoryId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentStatusId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LocationId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OffsiteLocationId", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ContractingCompanyId", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AddressId", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ProcessId", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ActivityId", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OnPremises", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Downtime", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Ppeworn", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@InsuranceNotified", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DrugTestPerformed", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AlcoholTestPerformed", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Rcaperformed", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Preventable", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Recurrence", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ProcessSafety", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@WorkRelated", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Litigable", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Sensitive", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AgencySensitive", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Media", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@TitleV", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncludeInStatistics", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ReportDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@InvestigationStartDate", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@EndDate", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@InsuranceNotifiedDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DrugTestDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@AlcoholTestDate", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DowntimeAmount", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Latitude", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Longitude", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@WindSpeed", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@TimeToExtinguish", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DowntimeUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@WindSpeedUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@TimeToExtinguishUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentNumber", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Title", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ReportedBy", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ReporterPhone", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@InsuranceClaim", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Equipment", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Vehicle", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OffPremisesLocation", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherLocation", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherContractingCompany", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherProcess", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherActivity", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherPpe", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherWeather", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Description", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ActivityDuringIncident", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ImmediateAction", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ImmediateCauses", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@CausalFactors", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@BestPractices", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RiskAction", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RiskCondition", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@InsuranceInfo", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Active", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Archived", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@HistoryId", SqlDbType.Int).Direction = ParameterDirection.Input;

                cmd.Parameters["@IncidentId"].Value = incidentEntity.IncidentId;
                cmd.Parameters["@ClientId"].Value = incidentEntity.ClientId;
                cmd.Parameters["@IncidentCategoryId"].Value = incidentEntity.IncidentCategoryId;
                cmd.Parameters["@IncidentStatusId"].Value = incidentEntity.IncidentStatusId;
                cmd.Parameters["@LocationId"].Value = incidentEntity.LocationId;
                cmd.Parameters["@OffsiteLocationId"].Value = incidentEntity.OffsiteLocationId ?? (object)DBNull.Value;
                cmd.Parameters["@ContractingCompanyId"].Value = incidentEntity.ContractingCompanyId ?? (object)DBNull.Value;
                cmd.Parameters["@AddressId"].Value = incidentEntity.AddressId ?? (object)DBNull.Value;
                cmd.Parameters["@ProcessId"].Value = incidentEntity.ProcessId ?? (object)DBNull.Value;
                cmd.Parameters["@ActivityId"].Value = incidentEntity.ActivityId ?? (object)DBNull.Value;
                cmd.Parameters["@OnPremises"].Value = incidentEntity.OnPremises ?? (object)DBNull.Value;
                cmd.Parameters["@Downtime"].Value = incidentEntity.Downtime ?? (object)DBNull.Value;
                cmd.Parameters["@Ppeworn"].Value = incidentEntity.Ppeworn ?? (object)DBNull.Value;
                cmd.Parameters["@InsuranceNotified"].Value = incidentEntity.InsuranceNotified ?? (object)DBNull.Value;
                cmd.Parameters["@DrugTestPerformed"].Value = incidentEntity.DrugTestPerformed ?? (object)DBNull.Value;
                cmd.Parameters["@AlcoholTestPerformed"].Value = incidentEntity.AlcoholTestPerformed ?? (object)DBNull.Value;
                cmd.Parameters["@Rcaperformed"].Value = incidentEntity.Rcaperformed ?? (object)DBNull.Value;
                cmd.Parameters["@Preventable"].Value = incidentEntity.Preventable ?? (object)DBNull.Value;
                cmd.Parameters["@Recurrence"].Value = incidentEntity.Recurrence ?? (object)DBNull.Value;
                cmd.Parameters["@ProcessSafety"].Value = incidentEntity.ProcessSafety ?? (object)DBNull.Value;
                cmd.Parameters["@WorkRelated"].Value = incidentEntity.WorkRelated ?? (object)DBNull.Value;
                cmd.Parameters["@Litigable"].Value = incidentEntity.Litigable ?? (object)DBNull.Value;
                cmd.Parameters["@Sensitive"].Value = incidentEntity.Sensitive ?? (object)DBNull.Value;
                cmd.Parameters["@AgencySensitive"].Value = incidentEntity.AgencySensitive ?? (object)DBNull.Value;
                cmd.Parameters["@Media"].Value = incidentEntity.Media ?? (object)DBNull.Value;
                cmd.Parameters["@TitleV"].Value = incidentEntity.TitleV ?? (object)DBNull.Value;
                cmd.Parameters["@IncludeInStatistics"].Value = incidentEntity.IncludeInStatistics;
                cmd.Parameters["@IncidentDate"].Value = incidentEntity.IncidentDate ?? (object)DBNull.Value;
                cmd.Parameters["@ReportDate"].Value = incidentEntity.ReportDate ?? (object)DBNull.Value;
                cmd.Parameters["@InvestigationStartDate"].Value = incidentEntity.InvestigationStartDate ?? (object)DBNull.Value;
                cmd.Parameters["@EndDate"].Value = incidentEntity.EndDate ?? (object)DBNull.Value;
                cmd.Parameters["@InsuranceNotifiedDate"].Value = incidentEntity.InsuranceNotifiedDate ?? (object)DBNull.Value;
                cmd.Parameters["@DrugTestDate"].Value = incidentEntity.DrugTestDate ?? (object)DBNull.Value;
                cmd.Parameters["@AlcoholTestDate"].Value = incidentEntity.AlcoholTestDate ?? (object)DBNull.Value;
                cmd.Parameters["@DowntimeAmount"].Value = incidentEntity.DowntimeAmount ?? (object)DBNull.Value;
                cmd.Parameters["@Latitude"].Value = incidentEntity.Latitude ?? (object)DBNull.Value;
                cmd.Parameters["@Longitude"].Value = incidentEntity.Longitude ?? (object)DBNull.Value;
                cmd.Parameters["@WindSpeed"].Value = incidentEntity.WindSpeed ?? (object)DBNull.Value;
                cmd.Parameters["@TimeToExtinguish"].Value = incidentEntity.TimeToExtinguish ?? (object)DBNull.Value;
                cmd.Parameters["@DowntimeUnitCode"].Value = incidentEntity.DowntimeUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@WindSpeedUnitCode"].Value = incidentEntity.WindSpeedUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@TimeToExtinguishUnitCode"].Value = incidentEntity.TimeToExtinguishUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@IncidentNumber"].Value = incidentEntity.IncidentNumber ?? (object)DBNull.Value;
                cmd.Parameters["@Title"].Value = incidentEntity.Title ?? (object)DBNull.Value;
                cmd.Parameters["@ReportedBy"].Value = incidentEntity.ReportedBy ?? (object)DBNull.Value;
                cmd.Parameters["@ReporterPhone"].Value = incidentEntity.ReporterPhone ?? (object)DBNull.Value;
                cmd.Parameters["@InsuranceClaim"].Value = incidentEntity.InsuranceClaim ?? (object)DBNull.Value;
                cmd.Parameters["@Equipment"].Value = incidentEntity.Equipment ?? (object)DBNull.Value;
                cmd.Parameters["@Vehicle"].Value = incidentEntity.Vehicle ?? (object)DBNull.Value;
                cmd.Parameters["@OffPremisesLocation"].Value = incidentEntity.OffPremisesLocation ?? (object)DBNull.Value;
                cmd.Parameters["@OtherLocation"].Value = incidentEntity.OtherLocation ?? (object)DBNull.Value;
                cmd.Parameters["@OtherContractingCompany"].Value = incidentEntity.OtherContractingCompany ?? (object)DBNull.Value;
                cmd.Parameters["@OtherProcess"].Value = incidentEntity.OtherProcess ?? (object)DBNull.Value;
                cmd.Parameters["@OtherActivity"].Value = incidentEntity.OtherActivity ?? (object)DBNull.Value;
                cmd.Parameters["@OtherPpe"].Value = incidentEntity.OtherPpe ?? (object)DBNull.Value;
                cmd.Parameters["@OtherWeather"].Value = incidentEntity.OtherWeather ?? (object)DBNull.Value;
                cmd.Parameters["@Description"].Value = incidentEntity.Description ?? (object)DBNull.Value;
                cmd.Parameters["@ActivityDuringIncident"].Value = incidentEntity.ActivityDuringIncident ?? (object)DBNull.Value;
                cmd.Parameters["@ImmediateAction"].Value = incidentEntity.ImmediateAction ?? (object)DBNull.Value;
                cmd.Parameters["@ImmediateCauses"].Value = incidentEntity.ImmediateCauses ?? (object)DBNull.Value;
                cmd.Parameters["@CausalFactors"].Value = incidentEntity.CausalFactors ?? (object)DBNull.Value;
                cmd.Parameters["@BestPractices"].Value = incidentEntity.BestPractices ?? (object)DBNull.Value;
                cmd.Parameters["@RiskAction"].Value = incidentEntity.RiskAction ?? (object)DBNull.Value;
                cmd.Parameters["@RiskCondition"].Value = incidentEntity.RiskCondition ?? (object)DBNull.Value;
                cmd.Parameters["@InsuranceInfo"].Value = incidentEntity.InsuranceInfo ?? (object)DBNull.Value;
                cmd.Parameters["@Active"].Value = incidentEntity.Active;
                cmd.Parameters["@Archived"].Value = incidentEntity.Archived;
                cmd.Parameters["@HistoryId"].Value = incidentEntity.HistoryId;

                Logger.Trace("CN: " + cnDW.ConnectionString);
                Logger.Trace("SQL: " + Environment.NewLine + cmd.CommandText);
                Logger.Trace("PARMS: " + string.Join(", ", cmd.Parameters.Cast<SqlParameter>().AsQueryable().Select(parm => parm.Value)));
                rowCount = cmd.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }

        public static int LoadIncident(SqlConnection cnDW)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            int rowCount;
            try
            {
                var cmdTruncate = new SqlCommand
                {
                    Connection = cnDW,
                    CommandType = CommandType.Text,
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_INCIDENT]"
                };
                rowCount = cmdTruncate.ExecuteNonQuery();
                Logger.Trace("ROWS: " + rowCount);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.ToString());
            }
            finally
            {
                Logger.Info("END: " + Convert.ToDecimal((DateTime.Now.ToLocalTime() - startTime).TotalSeconds) + " SECONDS");
            }
            return rowCount;
        }
    }
}
