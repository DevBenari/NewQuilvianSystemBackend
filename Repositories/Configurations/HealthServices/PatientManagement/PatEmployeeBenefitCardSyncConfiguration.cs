using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PatientManagement
{
    /// <summary>
    /// Konfigurasi EF Core untuk entitas PatEmployeeBenefitCardSync (FIN-DEC-239, kamus data bagian 12).
    /// </summary>
    public sealed class PatEmployeeBenefitCardSyncConfiguration : IEntityTypeConfiguration<PatEmployeeBenefitCardSync>
    {
        public void Configure(EntityTypeBuilder<PatEmployeeBenefitCardSync> builder)
        {
            builder.ToTable("PatEmployeeBenefitCardSync", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_PatEmployeeBenefitCardSync_Status",
                    "\"SyncStatus\" IN ('SYNCED','WAITING_PATIENT','ON_HOLD','FAILED_RETRYABLE','REVOKED')");

                table.HasCheckConstraint(
                    "CK_PatEmployeeBenefitCardSync_TriggerSource",
                    "\"LastTriggerSource\" IN ('ACTIVATION','SCHEDULER','MANUAL_RESYNC','CANCEL')");

                table.HasCheckConstraint(
                    "CK_PatEmployeeBenefitCardSync_CardLink",
                    "(\"SyncStatus\" IN ('SYNCED','REVOKED')) = (\"PatientCompanyGuarantorId\" IS NOT NULL AND \"PatientId\" IS NOT NULL)");

                table.HasCheckConstraint(
                    "CK_PatEmployeeBenefitCardSync_Reason",
                    "(\"SyncStatus\" = 'SYNCED') = (\"ReasonCode\" IS NULL)");

                table.HasCheckConstraint(
                    "CK_PatEmployeeBenefitCardSync_AttemptCount",
                    "\"AttemptCount\" >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmployeeBenefitEnrollmentId)
                .IsRequired();

            builder.Property(x => x.WorkforceProfileId)
                .IsRequired();

            builder.Property(x => x.SyncStatus)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ReasonCode)
                .HasMaxLength(50);

            builder.Property(x => x.AttemptCount)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.LastAttemptAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.NextAttemptAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.PatientId);

            builder.Property(x => x.PatientCompanyGuarantorId);

            builder.Property(x => x.SyncedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.RevokedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.LastTriggerSource)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.LastTriggeredByUserId);

            builder.Property(x => x.RowVersion)
                .IsRequired();

            // IdentityModel properties
            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone");

            // Foreign Keys with DeleteBehavior.Restrict
            builder.HasOne(x => x.EmployeeBenefitEnrollment)
                .WithMany()
                .HasForeignKey(x => x.EmployeeBenefitEnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientCompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.PatientCompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes and Unique Constraints
            builder.HasIndex(x => x.EmployeeBenefitEnrollmentId)
                .IsUnique()
                .HasDatabaseName("UX_PatEmployeeBenefitCardSync_Enrollment")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.PatientCompanyGuarantorId)
                .IsUnique()
                .HasDatabaseName("UX_PatEmployeeBenefitCardSync_Card")
                .HasFilter("\"PatientCompanyGuarantorId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.SyncStatus, x.NextAttemptAt })
                .HasDatabaseName("IX_PatEmployeeBenefitCardSync_Due");

            builder.HasIndex(x => x.WorkforceProfileId)
                .HasDatabaseName("IX_PatEmployeeBenefitCardSync_WorkforceProfile");

            builder.HasIndex(x => x.PatientId)
                .HasDatabaseName("IX_PatEmployeeBenefitCardSync_Patient")
                .HasFilter("\"PatientId\" IS NOT NULL");
        }
    }
}
