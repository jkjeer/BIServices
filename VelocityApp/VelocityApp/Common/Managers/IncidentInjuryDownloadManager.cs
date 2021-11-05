using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using KMI.Orm.Linq;
using NLog;

namespace VelocityApp.Common.Managers
{
    class IncidentInjuryDownloadManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int DownloadIncidentInjury(SqlConnection cnDW, KMIEHS context)
        {
            var startTime = DateTime.Now.ToLocalTime();
            Logger.Info("BEGIN: " + DateTime.Now.ToLocalTime());
            var rowCount = 0;
            try
            {
                Console.WriteLine("Retrieving IncidentInjury records...");
                TruncateIncidentInjury(cnDW);
                var query = from i in context.IncidentInjury
                                //where task.DueDate > new DateTime(2016, 12, 15)
                            select i;
                foreach (var o in query)
                {
                    //Console.WriteLine($"IncidentInjuryId ID: {o.IncidentInjuryId}");
                    Console.Write(".");
                    rowCount += InsertIncidentInjuryEntity(cnDW, o);
                }
                Console.WriteLine("Finished Writing IncidentInjury records");
                LoadIncidentInjury(cnDW);
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

        public static int TruncateIncidentInjury(SqlConnection cnDW)
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
                    CommandText = @"TRUNCATE TABLE [SOURCE].[VELOCITY_INCIDENT_INJURY]"
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

        public static int InsertIncidentInjuryEntity(SqlConnection cnDW, IncidentInjuryEntity incidentInjuryEntity)
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
                        INSERT INTO [SOURCE].[VELOCITY_INCIDENT_INJURY]
                            ( [InjuryId]
                              ,[IncidentId]
                              ,[IncidentInvolvedId]
                              ,[LostTime]
                              ,[RestrictedTime]
                              ,[Fatality]
                              ,[EmergencyRoom]
                              ,[Inpatient]
                              ,[RepeatedMotion]
                              ,[LiftedFromFloor]
                              ,[LiftingAidsAvailable]
                              ,[LiftingAidsUsed]
                              ,[SuddenPain]
                              ,[NonPainEffect]
                              ,[FatalityDate]
                              ,[EffectNoticed]
                              ,[FallHeight]
                              ,[LostTimeEstimate]
                              ,[LostTimeActual]
                              ,[RestrictedTimeEstimate]
                              ,[RestrictedTimeActual]
                              ,[DistanceMoved]
                              ,[RepeatFrequency]
                              ,[ObjectSize]
                              ,[FallHeightUnitCode]
                              ,[DistanceMovedUnitCode]
                              ,[RepeatFrequencyUnitCode]
                              ,[ObjectSizeUnitCode]
                              ,[FirstAidResponder]
                              ,[OtherObjectResponsible]
                              ,[OtherInjuryType]
                              ,[OtherBodyPart]
                              ,[OtherInjuryCause]
                              ,[OtherTreatment]
                              ,[Restriction]
                              ,[Physician]
                              ,[MedicalFacility]
                              ,[Treatment]
                              ,[PhysicalEffect]
                              ,[EffectChanges]
                              ,[MaterialExposure]
                              ,[Allergies]
                              ,[IngestedPrior]
                              ,[PainCause])
                        VALUES
                            ( @InjuryId
                                ,@IncidentId
                                ,@IncidentInvolvedId
                                ,@LostTime
                                ,@RestrictedTime
                                ,@Fatality
                                ,@EmergencyRoom
                                ,@Inpatient
                                ,@RepeatedMotion
                                ,@LiftedFromFloor
                                ,@LiftingAidsAvailable
                                ,@LiftingAidsUsed
                                ,@SuddenPain
                                ,@NonPainEffect
                                ,@FatalityDate
                                ,@EffectNoticed
                                ,@FallHeight
                                ,@LostTimeEstimate
                                ,@LostTimeActual
                                ,@RestrictedTimeEstimate
                                ,@RestrictedTimeActual
                                ,@DistanceMoved
                                ,@RepeatFrequency
                                ,@ObjectSize
                                ,@FallHeightUnitCode
                                ,@DistanceMovedUnitCode
                                ,@RepeatFrequencyUnitCode
                                ,@ObjectSizeUnitCode
                                ,@FirstAidResponder
                                ,@OtherObjectResponsible
                                ,@OtherInjuryType
                                ,@OtherBodyPart
                                ,@OtherInjuryCause
                                ,@OtherTreatment
                                ,@Restriction
                                ,@Physician
                                ,@MedicalFacility
                                ,@Treatment
                                ,@PhysicalEffect
                                ,@EffectChanges
                                ,@MaterialExposure
                                ,@Allergies
                                ,@IngestedPrior
                                ,@PainCause)"
                };
                cmd.Parameters.Add("@InjuryId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IncidentInvolvedId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LostTime", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RestrictedTime", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Fatality", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@EmergencyRoom", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Inpatient", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RepeatedMotion", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LiftedFromFloor", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LiftingAidsAvailable", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LiftingAidsUsed", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@SuddenPain", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@NonPainEffect", SqlDbType.Bit).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@FatalityDate", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@EffectNoticed", SqlDbType.DateTime).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@FallHeight", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LostTimeEstimate", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@LostTimeActual", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RestrictedTimeEstimate", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RestrictedTimeActual", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DistanceMoved", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RepeatFrequency", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ObjectSize", SqlDbType.Float).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@FallHeightUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@DistanceMovedUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@RepeatFrequencyUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@ObjectSizeUnitCode", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@FirstAidResponder", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherObjectResponsible", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherInjuryType", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherBodyPart", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherInjuryCause", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@OtherTreatment", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Restriction", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Physician", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@MedicalFacility", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Treatment", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PhysicalEffect", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@EffectChanges", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@MaterialExposure", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@Allergies", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@IngestedPrior", SqlDbType.VarChar).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@PainCause", SqlDbType.VarChar).Direction = ParameterDirection.Input;

                cmd.Parameters["@InjuryId"].Value = incidentInjuryEntity.InjuryId;
                cmd.Parameters["@IncidentId"].Value = incidentInjuryEntity.IncidentId;
                if (incidentInjuryEntity.IncidentInvolvedId != null) { cmd.Parameters["@IncidentInvolvedId"].Value = incidentInjuryEntity.IncidentInvolvedId; } else { cmd.Parameters["@IncidentInvolvedId"].Value = DBNull.Value; }
                cmd.Parameters["@LostTime"].Value = incidentInjuryEntity.LostTime ?? (object)DBNull.Value;
                cmd.Parameters["@RestrictedTime"].Value = incidentInjuryEntity.RestrictedTime ?? (object)DBNull.Value;
                cmd.Parameters["@Fatality"].Value = incidentInjuryEntity.Fatality ?? (object)DBNull.Value;
                cmd.Parameters["@EmergencyRoom"].Value = incidentInjuryEntity.EmergencyRoom ?? (object)DBNull.Value;
                cmd.Parameters["@Inpatient"].Value = incidentInjuryEntity.Inpatient ?? (object)DBNull.Value;
                cmd.Parameters["@RepeatedMotion"].Value = incidentInjuryEntity.RepeatedMotion ?? (object)DBNull.Value;
                cmd.Parameters["@LiftedFromFloor"].Value = incidentInjuryEntity.LiftedFromFloor ?? (object)DBNull.Value;
                cmd.Parameters["@LiftingAidsAvailable"].Value = incidentInjuryEntity.LiftingAidsAvailable ?? (object)DBNull.Value;
                cmd.Parameters["@LiftingAidsUsed"].Value = incidentInjuryEntity.LiftingAidsUsed ?? (object)DBNull.Value;
                cmd.Parameters["@SuddenPain"].Value = incidentInjuryEntity.SuddenPain ?? (object)DBNull.Value;
                cmd.Parameters["@NonPainEffect"].Value = incidentInjuryEntity.NonPainEffect ?? (object)DBNull.Value;
                cmd.Parameters["@FatalityDate"].Value = incidentInjuryEntity.FatalityDate ?? (object)DBNull.Value;
                cmd.Parameters["@EffectNoticed"].Value = incidentInjuryEntity.EffectNoticed ?? (object)DBNull.Value;
                if (incidentInjuryEntity.FallHeight != null) { cmd.Parameters["@FallHeight"].Value = incidentInjuryEntity.FallHeight; } else { cmd.Parameters["@FallHeight"].Value = DBNull.Value; }
                if (incidentInjuryEntity.LostTimeEstimate != null) { cmd.Parameters["@LostTimeEstimate"].Value = incidentInjuryEntity.LostTimeEstimate; } else { cmd.Parameters["@LostTimeEstimate"].Value = DBNull.Value; }
                if (incidentInjuryEntity.LostTimeActual != null) { cmd.Parameters["@LostTimeActual"].Value = incidentInjuryEntity.LostTimeActual; } else { cmd.Parameters["@LostTimeActual"].Value = DBNull.Value; }
                if (incidentInjuryEntity.RestrictedTimeEstimate != null) { cmd.Parameters["@RestrictedTimeEstimate"].Value = incidentInjuryEntity.RestrictedTimeEstimate; } else { cmd.Parameters["@RestrictedTimeEstimate"].Value = DBNull.Value; }
                if (incidentInjuryEntity.RestrictedTimeActual != null) { cmd.Parameters["@RestrictedTimeActual"].Value = incidentInjuryEntity.RestrictedTimeActual; } else { cmd.Parameters["@RestrictedTimeActual"].Value = DBNull.Value; }
                if (incidentInjuryEntity.DistanceMoved != null) { cmd.Parameters["@DistanceMoved"].Value = incidentInjuryEntity.DistanceMoved; } else { cmd.Parameters["@DistanceMoved"].Value = DBNull.Value; }
                if (incidentInjuryEntity.RepeatFrequency != null) { cmd.Parameters["@RepeatFrequency"].Value = incidentInjuryEntity.RepeatFrequency; } else { cmd.Parameters["@RepeatFrequency"].Value = DBNull.Value; }
                if (incidentInjuryEntity.ObjectSize != null) { cmd.Parameters["@ObjectSize"].Value = incidentInjuryEntity.ObjectSize; } else { cmd.Parameters["@ObjectSize"].Value = DBNull.Value; }
                cmd.Parameters["@FallHeightUnitCode"].Value = incidentInjuryEntity.FallHeightUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@DistanceMovedUnitCode"].Value = incidentInjuryEntity.DistanceMovedUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@RepeatFrequencyUnitCode"].Value = incidentInjuryEntity.RepeatFrequencyUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@ObjectSizeUnitCode"].Value = incidentInjuryEntity.ObjectSizeUnitCode ?? (object)DBNull.Value;
                cmd.Parameters["@FirstAidResponder"].Value = incidentInjuryEntity.FirstAidResponder ?? (object)DBNull.Value;
                cmd.Parameters["@OtherObjectResponsible"].Value = incidentInjuryEntity.OtherObjectResponsible ?? (object)DBNull.Value;
                cmd.Parameters["@OtherInjuryType"].Value = incidentInjuryEntity.OtherInjuryType ?? (object)DBNull.Value;
                cmd.Parameters["@OtherBodyPart"].Value = incidentInjuryEntity.OtherBodyPart ?? (object)DBNull.Value;
                cmd.Parameters["@OtherInjuryCause"].Value = incidentInjuryEntity.OtherInjuryCause ?? (object)DBNull.Value;
                cmd.Parameters["@OtherTreatment"].Value = incidentInjuryEntity.OtherTreatment ?? (object)DBNull.Value;
                cmd.Parameters["@Restriction"].Value = incidentInjuryEntity.Restriction ?? (object)DBNull.Value;
                cmd.Parameters["@Physician"].Value = incidentInjuryEntity.Physician ?? (object)DBNull.Value;
                cmd.Parameters["@MedicalFacility"].Value = incidentInjuryEntity.MedicalFacility ?? (object)DBNull.Value;
                cmd.Parameters["@Treatment"].Value = incidentInjuryEntity.Treatment ?? (object)DBNull.Value;
                cmd.Parameters["@PhysicalEffect"].Value = incidentInjuryEntity.PhysicalEffect ?? (object)DBNull.Value;
                cmd.Parameters["@EffectChanges"].Value = incidentInjuryEntity.EffectChanges ?? (object)DBNull.Value;
                cmd.Parameters["@MaterialExposure"].Value = incidentInjuryEntity.MaterialExposure ?? (object)DBNull.Value;
                cmd.Parameters["@Allergies"].Value = incidentInjuryEntity.Allergies ?? (object)DBNull.Value;
                cmd.Parameters["@IngestedPrior"].Value = incidentInjuryEntity.IngestedPrior ?? (object)DBNull.Value;
                cmd.Parameters["@PainCause"].Value = incidentInjuryEntity.PainCause ?? (object)DBNull.Value;

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

        public static int LoadIncidentInjury(SqlConnection cnDW)
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
                    CommandText = @"EXEC [PROD].[usp_VELOCITY_INCIDENT_INJURY]"
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
