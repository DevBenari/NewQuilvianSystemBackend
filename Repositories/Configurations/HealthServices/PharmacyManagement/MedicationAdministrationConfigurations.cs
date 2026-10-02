using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement
{
    /// <summary>
    /// Bentuk tabel <c>PhmMedicationAdministration</c> — migration <c>K4</c>.
    /// </summary>
    public class PhmMedicationAdministrationConfiguration : IEntityTypeConfiguration<PhmMedicationAdministration>
    {
        public void Configure(EntityTypeBuilder<PhmMedicationAdministration> builder)
        {
            builder.ToTable("PhmMedicationAdministration", "public", table =>
            {
                table.HasCheckConstraint("CK_PhmMedicationAdministration_ScheduledHasTime", "\"DoseSource\" <> 1 OR \"ScheduledAt\" IS NOT NULL");
                table.HasCheckConstraint("CK_PhmMedicationAdministration_DoubleCheckerDiffers", "\"DoubleCheckedByUserId\" IS NULL OR \"DoubleCheckedByUserId\" <> \"RecordedByUserId\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AdministrationNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.PrescriptionId)
                .IsRequired();

            builder.Property(x => x.PrescriptionItemId)
                .IsRequired();

            builder.Property(x => x.EncounterId)
                .IsRequired();

            builder.Property(x => x.InpEpisodeId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.DrugId)
                .IsRequired();

            builder.Property(x => x.DoseSource)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ScheduledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DoseStatus)
                .HasConversion<int>()
                .HasDefaultValue(MedicationDoseStatus.Due)
                .IsRequired();

            builder.Property(x => x.StatusReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.PlannedDose)
                .HasColumnType("numeric(12,4)")
                .IsRequired(false);

            builder.Property(x => x.PlannedDoseUnitSnapshot)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.ActualDose)
                .HasColumnType("numeric(12,4)")
                .IsRequired(false);

            builder.Property(x => x.ActualDoseUnitSnapshot)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.ActualRouteSnapshot)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.AdministeredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.RecordedByEmployeeId)
                .IsRequired(false);

            builder.Property(x => x.RecordedByUserId)
                .IsRequired(false);

            builder.Property(x => x.RecordedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DeviationNote)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.IsHighAlertSnapshot)
                .IsRequired();

            builder.Property(x => x.DoubleCheckStatus)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.DoubleCheckedByEmployeeId)
                .IsRequired(false);

            builder.Property(x => x.DoubleCheckedByUserId)
                .IsRequired(false);

            builder.Property(x => x.DoubleCheckedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DoubleCheckNote)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.PrnIndication)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.PrnEvaluationDueAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.PrnEvaluationNote)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.PrnEvaluatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.PrnEvaluatedByUserId)
                .IsRequired(false);

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired(false);

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<PhmPrescription>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .HasConstraintName("FK_PhmMedicationAdministration_PrescriptionId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PhmPrescriptionItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionItemId)
                .HasConstraintName("FK_PhmMedicationAdministration_PrescriptionItemId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .HasConstraintName("FK_PhmMedicationAdministration_EncounterId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .HasConstraintName("FK_PhmMedicationAdministration_InpEpisodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .HasConstraintName("FK_PhmMedicationAdministration_PatientId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstDrug>()
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .HasConstraintName("FK_PhmMedicationAdministration_DrugId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByEmployeeId)
                .HasConstraintName("FK_PhmMedicationAdministration_RecordedByEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .HasConstraintName("FK_PhmMedicationAdministration_RecordedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.DoubleCheckedByEmployeeId)
                .HasConstraintName("FK_PhmMedicationAdministration_DoubleCheckedByEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.DoubleCheckedByUserId)
                .HasConstraintName("FK_PhmMedicationAdministration_DoubleCheckedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PrnEvaluatedByUserId)
                .HasConstraintName("FK_PhmMedicationAdministration_PrnEvaluatedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AdministrationNumber, "UX_PhmMedicationAdministration_Number")
                .IsUnique();

            builder.HasIndex(x => new { x.PrescriptionItemId, x.ScheduledAt }, "UX_PhmMedicationAdministration_Item_ScheduledAt")
                .IsUnique()
                .HasFilter("\"DoseSource\" = 1 AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.InpEpisodeId, x.ScheduledAt }, "IX_PhmMedicationAdministration_Episode_ScheduledAt");

            builder.HasIndex(x => new { x.InpEpisodeId, x.DoseStatus }, "IX_PhmMedicationAdministration_Episode_Status");

            builder.HasIndex(x => x.InpEpisodeId, "IX_PhmMedicationAdministration_Episode_DoubleCheck")
                .HasFilter("\"DoubleCheckStatus\" = 1");

            builder.HasIndex(x => x.IdempotencyKey, "UX_PhmMedicationAdministration_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            builder.HasIndex(x => x.PrescriptionId, "IX_PhmMedicationAdministration_PrescriptionId");

            builder.HasIndex(x => x.EncounterId, "IX_PhmMedicationAdministration_EncounterId");

            builder.HasIndex(x => x.PatientId, "IX_PhmMedicationAdministration_PatientId");

            builder.HasIndex(x => x.DrugId, "IX_PhmMedicationAdministration_DrugId");

            builder.HasIndex(x => x.RecordedByEmployeeId, "IX_PhmMedicationAdministration_RecordedByEmployeeId");

            builder.HasIndex(x => x.RecordedByUserId, "IX_PhmMedicationAdministration_RecordedByUserId");

            builder.HasIndex(x => x.DoubleCheckedByEmployeeId, "IX_PhmMedicationAdministration_DoubleCheckedByEmployeeId");

            builder.HasIndex(x => x.DoubleCheckedByUserId, "IX_PhmMedicationAdministration_DoubleCheckedByUserId");

            builder.HasIndex(x => x.PrnEvaluatedByUserId, "IX_PhmMedicationAdministration_PrnEvaluatedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>PhmMedicationAdministrationRevision</c> — migration <c>K4</c>.
    /// </summary>
    public class PhmMedicationAdministrationRevisionConfiguration : IEntityTypeConfiguration<PhmMedicationAdministrationRevision>
    {
        public void Configure(EntityTypeBuilder<PhmMedicationAdministrationRevision> builder)
        {
            builder.ToTable("PhmMedicationAdministrationRevision", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AdministrationId)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.RevisionKind)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.PreviousDoseStatus)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.PreviousActualDose)
                .HasColumnType("numeric(12,4)")
                .IsRequired(false);

            builder.Property(x => x.PreviousActualRouteSnapshot)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.PreviousAdministeredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.PreviousStatusReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.PreviousDeviationNote)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.PreviousRecordedByUserId)
                .IsRequired(false);

            builder.Property(x => x.CorrectionReason)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CorrectedByUserId)
                .IsRequired();

            builder.Property(x => x.CorrectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<PhmMedicationAdministration>()
                .WithMany()
                .HasForeignKey(x => x.AdministrationId)
                .HasConstraintName("FK_PhmMedicationAdministrationRevision_AdministrationId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CorrectedByUserId)
                .HasConstraintName("FK_PhmMedicationAdministrationRevision_CorrectedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AdministrationId, x.RevisionNumber }, "UX_PhmMedicationAdministrationRevision_Admin_Number")
                .IsUnique();

            builder.HasIndex(x => x.CorrectedByUserId, "IX_PhmMedicationAdministrationRevision_CorrectedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>PhmMedicationScheduleTime</c> — migration <c>K4</c>.
    /// </summary>
    public class PhmMedicationScheduleTimeConfiguration : IEntityTypeConfiguration<PhmMedicationScheduleTime>
    {
        public void Configure(EntityTypeBuilder<PhmMedicationScheduleTime> builder)
        {
            builder.ToTable("PhmMedicationScheduleTime", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FrequencyCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            builder.Property(x => x.SlotNumber)
                .IsRequired();

            builder.Property(x => x.TimeOfDay)
                .HasColumnType("time without time zone")
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<MstServiceUnit>()
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .HasConstraintName("FK_PhmMedicationScheduleTime_ServiceUnitId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.FrequencyCode, x.ServiceUnitId, x.SlotNumber }, "UX_PhmMedicationScheduleTime_Code_Unit_Slot")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .AreNullsDistinct(false);

            builder.HasIndex(x => x.ServiceUnitId, "IX_PhmMedicationScheduleTime_ServiceUnitId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>PhmMedicationAdministrationSetting</c> — migration <c>K4</c>.
    /// </summary>
    public class PhmMedicationAdministrationSettingConfiguration : IEntityTypeConfiguration<PhmMedicationAdministrationSetting>
    {
        public void Configure(EntityTypeBuilder<PhmMedicationAdministrationSetting> builder)
        {
            builder.ToTable("PhmMedicationAdministrationSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DoseGenerationHorizonHours)
                .IsRequired();

            builder.Property(x => x.MissedAfterMinutes)
                .IsRequired(false);

            builder.Property(x => x.PrnEvaluationMinutes)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasIndex(x => x.IsActive, "UX_PhmMedicationAdministrationSetting_Active")
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
