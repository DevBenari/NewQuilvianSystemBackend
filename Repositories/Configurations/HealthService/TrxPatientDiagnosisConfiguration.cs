using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientDiagnosisConfiguration : IEntityTypeConfiguration<TrxPatientDiagnosis>
    {
        public void Configure(EntityTypeBuilder<TrxPatientDiagnosis> entity)
        {
            entity.ToTable("TrxPatientDiagnosis", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.ConsultationId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.DoctorId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.DiagnosisId)
                .IsRequired(false);

            entity.Property(x => x.DiagnosisCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DiagnosisName)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.DiagnosisMasterType)
                .HasMaxLength(50)
                .HasDefaultValue("ICD10")
                .IsRequired();

            entity.Property(x => x.IcdVersion)
                .HasMaxLength(100);

            entity.Property(x => x.DiagnosisType)
                .HasConversion<int>()
                .HasDefaultValue(PatientDiagnosisType.Secondary)
                .IsRequired();

            entity.Property(x => x.DiagnosisStatus)
                .HasConversion<int>()
                .HasDefaultValue(PatientDiagnosisStatus.Active)
                .IsRequired();

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsChronic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNewCase)
                .HasDefaultValue(true);

            entity.Property(x => x.IsConfirmed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsFromMasterDiagnosis)
                .HasDefaultValue(false);

            entity.Property(x => x.DiagnosisDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.OnsetDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ClinicalNote)
                .HasMaxLength(1000);

            entity.Property(x => x.AssessmentNote)
                .HasMaxLength(1000);

            entity.Property(x => x.PlanNote)
                .HasMaxLength(1000);

            entity.Property(x => x.DifferentialDiagnosisNote)
                .HasMaxLength(500);

            entity.Property(x => x.SupportingFindingNote)
                .HasMaxLength(500);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.ResolvedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ResolvedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ResolvedReason)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Diagnosis)
                .WithMany()
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ResolvedByUser)
                .WithMany()
                .HasForeignKey(x => x.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EncounterId);

            entity.HasIndex(x => x.ConsultationId);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.DiagnosisId);

            entity.HasIndex(x => x.DiagnosisCode);

            entity.HasIndex(x => x.DiagnosisName);

            entity.HasIndex(x => x.IcdVersion);

            entity.HasIndex(x => new
            {
                x.ConsultationId,
                x.DiagnosisCode,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ConsultationId,
                x.DiagnosisId,
                x.IsDelete
            })
            .HasFilter("\"DiagnosisId\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.ConsultationId,
                x.IsPrimary,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.DiagnosisCode,
                x.DiagnosisStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.DiagnosisId,
                x.DiagnosisStatus,
                x.IsDelete
            })
            .HasFilter("\"DiagnosisId\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.DiagnosisDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DiagnosisType,
                x.DiagnosisStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DiagnosisMasterType,
                x.IcdVersion,
                x.IsFromMasterDiagnosis,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsPrimary,
                x.IsConfirmed,
                x.IsChronic,
                x.IsNewCase,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.SortOrder,
                x.IsDelete
            });

            entity.HasIndex(x => x.ResolvedByUserId);

            entity.HasIndex(x => x.CancelledByUserId);
        }
    }
}
