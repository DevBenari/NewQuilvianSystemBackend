using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientFamilyHistoryConfiguration : IEntityTypeConfiguration<TrxPatientFamilyHistory>
    {
        public void Configure(EntityTypeBuilder<TrxPatientFamilyHistory> builder)
        {
            builder.ToTable("TrxPatientFamilyHistory", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.FamilyHistoryRecordNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FamilyMemberNameSnapshot)
                .HasMaxLength(200);

            builder.Property(x => x.RelationshipDescription)
                .HasMaxLength(100);

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

            builder.Property(x => x.CauseOfDeath)
                .HasMaxLength(250);

            builder.Property(x => x.SourceOfInformation)
                .HasMaxLength(100);

            builder.Property(x => x.ClinicalNote)
                .HasMaxLength(1000);

            builder.Property(x => x.RiskNote)
                .HasMaxLength(1000);

            builder.Property(x => x.ScreeningRecommendation)
                .HasMaxLength(1000);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.ResolvedReason)
                .HasMaxLength(250);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            builder.Property(x => x.IsFromMasterDiagnosis)
                .HasDefaultValue(false);

            builder.Property(x => x.IsFirstDegreeRelative)
                .HasDefaultValue(false);

            builder.Property(x => x.IsSecondDegreeRelative)
                .HasDefaultValue(false);

            builder.Property(x => x.IsSameHousehold)
                .HasDefaultValue(false);

            builder.Property(x => x.IsHereditaryDisease)
                .HasDefaultValue(false);

            builder.Property(x => x.IsGeneticRisk)
                .HasDefaultValue(false);

            builder.Property(x => x.IsChronicDisease)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCancerRelated)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCardiovascularRisk)
                .HasDefaultValue(false);

            builder.Property(x => x.IsMetabolicRisk)
                .HasDefaultValue(false);

            builder.Property(x => x.IsMentalHealthRelated)
                .HasDefaultValue(false);

            builder.Property(x => x.IsInfectiousDisease)
                .HasDefaultValue(false);

            builder.Property(x => x.IsHighRisk)
                .HasDefaultValue(false);

            builder.Property(x => x.IsScreeningRecommended)
                .HasDefaultValue(false);

            builder.Property(x => x.IsAlertEnabled)
                .HasDefaultValue(false);

            builder.Property(x => x.IsFamilyMemberDeceased)
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

            builder.HasIndex(x => x.FamilyHistoryRecordNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.FamilyMemberPatientId);

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
                x.FamilyHistoryStatus,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.RelationshipType,
                x.ConditionName
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.RelationshipSide,
                x.RelationshipType
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsHereditaryDisease,
                x.IsGeneticRisk,
                x.IsHighRisk
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsAlertEnabled,
                x.IsScreeningRecommended
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

            builder.HasOne(x => x.FamilyMemberPatient)
                .WithMany()
                .HasForeignKey(x => x.FamilyMemberPatientId)
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