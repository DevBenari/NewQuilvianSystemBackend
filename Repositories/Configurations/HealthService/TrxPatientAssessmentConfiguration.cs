using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientAssessmentConfiguration : IEntityTypeConfiguration<TrxPatientAssessment>
    {
        public void Configure(EntityTypeBuilder<TrxPatientAssessment> entity)
        {
            entity.ToTable("TrxPatientAssessment", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssessmentNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.QueueId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.DoctorId)
                .IsRequired(false);

            entity.Property(x => x.AssessmentDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.AssessmentStatus)
                .HasConversion<int>()
                .HasDefaultValue(PatientAssessmentStatus.Draft)
                .IsRequired();

            entity.Property(x => x.AssessmentByUserId)
                .IsRequired(false);

            entity.Property(x => x.ChiefComplaint)
                .HasMaxLength(500);

            entity.Property(x => x.CurrentIllnessHistory)
                .HasMaxLength(1000);

            entity.Property(x => x.MedicationHistory)
                .HasMaxLength(1000);

            entity.Property(x => x.BloodPressureSystolic)
                .IsRequired(false);

            entity.Property(x => x.BloodPressureDiastolic)
                .IsRequired(false);

            entity.Property(x => x.PulseRate)
                .IsRequired(false);

            entity.Property(x => x.IsPulseReadable)
                .HasDefaultValue(true);

            entity.Property(x => x.RespiratoryRate)
                .IsRequired(false);

            entity.Property(x => x.Temperature)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.OxygenSaturation)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.IsUsingOxygen)
                .HasDefaultValue(false);

            entity.Property(x => x.OxygenSupportType)
                .HasConversion<int>()
                .HasDefaultValue(OxygenSupportType.None)
                .IsRequired();

            entity.Property(x => x.OxygenFlowRate)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.OxygenSupportNote)
                .HasMaxLength(100);

            entity.Property(x => x.ConsciousnessStatus)
                .HasConversion<int>()
                .HasDefaultValue(ConsciousnessStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.Weight)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.Height)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.BMI)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.MeanArterialPressure)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.MapStatus)
                .HasConversion<int>()
                .HasDefaultValue(MapStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.EarlyWarningScore)
                .IsRequired(false);

            entity.Property(x => x.EwsRiskLevel)
                .HasConversion<int>()
                .HasDefaultValue(EwsRiskLevel.Unknown)
                .IsRequired();

            entity.Property(x => x.EwsMonitoringRecommendation)
                .HasMaxLength(250);

            entity.Property(x => x.HasPain)
                .HasDefaultValue(false);

            entity.Property(x => x.PainScale)
                .IsRequired(false);

            entity.Property(x => x.PainTrigger)
                .HasMaxLength(250);

            entity.Property(x => x.PainQuality)
                .HasMaxLength(250);

            entity.Property(x => x.PainLocation)
                .HasMaxLength(250);

            entity.Property(x => x.PainFrequency)
                .HasMaxLength(250);

            entity.Property(x => x.PainManagement)
                .HasMaxLength(250);

            entity.Property(x => x.PainNote)
                .HasMaxLength(500);

            entity.Property(x => x.HasHereditaryDisease)
                .HasDefaultValue(false);

            entity.Property(x => x.HereditaryDiseaseNote)
                .HasMaxLength(500);

            entity.Property(x => x.HasAllergy)
                .HasDefaultValue(false);

            entity.Property(x => x.AllergyType)
                .HasMaxLength(250);

            entity.Property(x => x.AllergyNote)
                .HasMaxLength(500);

            entity.Property(x => x.HasBcgImmunization)
                .HasDefaultValue(false);

            entity.Property(x => x.HasHepatitisBImmunization)
                .HasDefaultValue(false);

            entity.Property(x => x.HasPolioImmunization)
                .HasDefaultValue(false);

            entity.Property(x => x.HasDptImmunization)
                .HasDefaultValue(false);

            entity.Property(x => x.HasMeaslesImmunization)
                .HasDefaultValue(false);

            entity.Property(x => x.ImmunizationNote)
                .HasMaxLength(500);

            entity.Property(x => x.AppetiteStatus)
                .HasConversion<int>()
                .HasDefaultValue(AppetiteStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.HasNausea)
                .HasDefaultValue(false);

            entity.Property(x => x.HasVomiting)
                .HasDefaultValue(false);

            entity.Property(x => x.NutritionRiskStatus)
                .HasConversion<int>()
                .HasDefaultValue(NutritionRiskStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.NutritionRiskScore)
                .IsRequired(false);

            entity.Property(x => x.NutritionNote)
                .HasMaxLength(500);

            entity.Property(x => x.HasFallRisk)
                .HasDefaultValue(false);

            entity.Property(x => x.HasAtaxia)
                .HasDefaultValue(false);

            entity.Property(x => x.HasPosturalInstability)
                .HasDefaultValue(false);

            entity.Property(x => x.FallRiskStatus)
                .HasConversion<int>()
                .HasDefaultValue(FallRiskStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.FallRiskScore)
                .IsRequired(false);

            entity.Property(x => x.FallRiskNote)
                .HasMaxLength(500);

            entity.Property(x => x.FunctionalStatus)
                .HasConversion<int>()
                .HasDefaultValue(FunctionalStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.FunctionalNote)
                .HasMaxLength(500);

            entity.Property(x => x.PsychosocialNote)
                .HasMaxLength(500);

            entity.Property(x => x.EducationNote)
                .HasMaxLength(500);

            entity.Property(x => x.NurseNote)
                .HasMaxLength(500);

            entity.Property(x => x.StartedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Queue)
                .WithMany()
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssessmentByUser)
                .WithMany()
                .HasForeignKey(x => x.AssessmentByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AssessmentNumber)
                .IsUnique();

            entity.HasIndex(x => x.EncounterId)
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.QueueId)
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.ServiceUnitId);
            entity.HasIndex(x => x.ClinicId);
            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.AssessmentDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.AssessmentStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.AssessmentDateTime,
                x.AssessmentStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.AssessmentStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.FallRiskStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EwsRiskLevel,
                x.IsDelete
            });

            entity.HasIndex(x => x.AssessmentByUserId);
            entity.HasIndex(x => x.CompletedByUserId);
            entity.HasIndex(x => x.CancelledByUserId);
        }
    }
}
