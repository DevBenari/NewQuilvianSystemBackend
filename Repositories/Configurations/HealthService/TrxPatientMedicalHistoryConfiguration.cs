using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientMedicalHistoryConfiguration : IEntityTypeConfiguration<TrxPatientMedicalHistory>
    {
        public void Configure(EntityTypeBuilder<TrxPatientMedicalHistory> builder)
        {
            builder.ToTable("TrxPatientMedicalHistory", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.MedicalHistoryRecordNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ConditionCode)
                .HasMaxLength(50);

            builder.Property(x => x.ConditionName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ConditionGroupName)
                .HasMaxLength(250);

            builder.Property(x => x.ConditionMasterType)
                .HasMaxLength(50)
                .HasDefaultValue("Manual");

            builder.Property(x => x.IcdVersion)
                .HasMaxLength(100);

            builder.Property(x => x.SourceOfInformation)
                .HasMaxLength(100);

            builder.Property(x => x.TreatmentHistory)
                .HasMaxLength(1000);

            builder.Property(x => x.MedicationHistory)
                .HasMaxLength(1000);

            builder.Property(x => x.SurgeryHistory)
                .HasMaxLength(1000);

            builder.Property(x => x.HospitalizationHistory)
                .HasMaxLength(1000);

            builder.Property(x => x.ComplicationNote)
                .HasMaxLength(1000);

            builder.Property(x => x.ClinicalNote)
                .HasMaxLength(1000);

            builder.Property(x => x.RiskNote)
                .HasMaxLength(1000);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.ResolvedReason)
                .HasMaxLength(250);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            builder.Property(x => x.IsFromMasterDiagnosis)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCurrentProblem)
                .HasDefaultValue(false);

            builder.Property(x => x.IsChronic)
                .HasDefaultValue(false);

            builder.Property(x => x.IsComorbidity)
                .HasDefaultValue(false);

            builder.Property(x => x.IsUnderTreatment)
                .HasDefaultValue(false);

            builder.Property(x => x.IsControlled)
                .HasDefaultValue(false);

            builder.Property(x => x.IsInfectiousDisease)
                .HasDefaultValue(false);

            builder.Property(x => x.IsHereditaryRelated)
                .HasDefaultValue(false);

            builder.Property(x => x.IsMentalHealthRelated)
                .HasDefaultValue(false);

            builder.Property(x => x.IsPregnancyRelated)
                .HasDefaultValue(false);

            builder.Property(x => x.IsSurgicalHistory)
                .HasDefaultValue(false);

            builder.Property(x => x.IsHospitalizationHistory)
                .HasDefaultValue(false);

            builder.Property(x => x.IsHighRisk)
                .HasDefaultValue(false);

            builder.Property(x => x.IsAlertEnabled)
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            // =========================
            // INDEXES
            // =========================

            builder.HasIndex(x => x.MedicalHistoryRecordNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.ServiceUnitId);

            builder.HasIndex(x => x.ClinicId);

            builder.HasIndex(x => x.DiagnosisId);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.HistoryStatus,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.HistoryType,
                x.ConditionName
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsCurrentProblem,
                x.IsChronic,
                x.IsComorbidity
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsAlertEnabled,
                x.IsHighRisk
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.RecordedDateTime
            });

            // =========================
            // RELATIONS
            // =========================

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Assessment)
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Diagnosis)
                .WithMany()
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResolvedByUser)
                .WithMany()
                .HasForeignKey(x => x.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
