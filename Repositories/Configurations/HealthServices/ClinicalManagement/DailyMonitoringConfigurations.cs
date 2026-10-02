using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel <c>CliFluidBalanceEntry</c> — migration <c>K5</c>.
    /// </summary>
    public class CliFluidBalanceEntryConfiguration : IEntityTypeConfiguration<CliFluidBalanceEntry>
    {
        public void Configure(EntityTypeBuilder<CliFluidBalanceEntry> builder)
        {
            builder.ToTable("CliFluidBalanceEntry", "public", table =>
            {
                table.HasCheckConstraint("CK_CliFluidBalanceEntry_Volume", "\"VolumeMl\" > 0 AND \"VolumeMl\" <= 10000");
                table.HasCheckConstraint("CK_CliFluidBalanceEntry_MedicationLink", "(\"SourceCategory\" = 5) = (\"MedicationAdministrationId\" IS NOT NULL)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EncounterId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.InpEpisodeId)
                .IsRequired();

            builder.Property(x => x.Direction)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.SourceCategory)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.SourceDetail)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(x => x.VolumeMl)
                .HasColumnType("numeric(9,2)")
                .IsRequired();

            builder.Property(x => x.EntryDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.RecordedByEmployeeId)
                .IsRequired();

            builder.Property(x => x.RecordedByUserId)
                .IsRequired();

            builder.Property(x => x.MedicationAdministrationId)
                .IsRequired(false);

            builder.Property(x => x.EntryStatus)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalMeasurementStatus.Active)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.CancelReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.DoseCorrectionFlaggedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired(false);

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .HasConstraintName("FK_CliFluidBalanceEntry_EncounterId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .HasConstraintName("FK_CliFluidBalanceEntry_PatientId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .HasConstraintName("FK_CliFluidBalanceEntry_InpEpisodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByEmployeeId)
                .HasConstraintName("FK_CliFluidBalanceEntry_RecordedByEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .HasConstraintName("FK_CliFluidBalanceEntry_RecordedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PhmMedicationAdministration>()
                .WithMany()
                .HasForeignKey(x => x.MedicationAdministrationId)
                .HasConstraintName("FK_CliFluidBalanceEntry_MedicationAdministrationId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.InpEpisodeId, x.EntryDateTime }, "IX_CliFluidBalanceEntry_Episode_EntryDateTime");

            builder.HasIndex(x => x.MedicationAdministrationId, "UX_CliFluidBalanceEntry_MedicationAdministration_Active")
                .IsUnique()
                .HasFilter("\"MedicationAdministrationId\" IS NOT NULL AND \"EntryStatus\" = 1 AND \"IsDelete\" = false");

            builder.HasIndex(x => x.IdempotencyKey, "UX_CliFluidBalanceEntry_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            builder.HasIndex(x => x.EncounterId, "IX_CliFluidBalanceEntry_EncounterId");

            builder.HasIndex(x => x.PatientId, "IX_CliFluidBalanceEntry_PatientId");

            builder.HasIndex(x => x.RecordedByEmployeeId, "IX_CliFluidBalanceEntry_RecordedByEmployeeId");

            builder.HasIndex(x => x.RecordedByUserId, "IX_CliFluidBalanceEntry_RecordedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliFluidBalanceEntryRevision</c> — migration <c>K5</c>.
    /// </summary>
    public class CliFluidBalanceEntryRevisionConfiguration : IEntityTypeConfiguration<CliFluidBalanceEntryRevision>
    {
        public void Configure(EntityTypeBuilder<CliFluidBalanceEntryRevision> builder)
        {
            builder.ToTable("CliFluidBalanceEntryRevision", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntryId)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.PreviousVolumeMl)
                .HasColumnType("numeric(9,2)")
                .IsRequired();

            builder.Property(x => x.PreviousEntryDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.PreviousSourceCategory)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.PreviousSourceDetail)
                .HasMaxLength(200)
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

            builder.HasOne<CliFluidBalanceEntry>()
                .WithMany()
                .HasForeignKey(x => x.EntryId)
                .HasConstraintName("FK_CliFluidBalanceEntryRevision_EntryId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CorrectedByUserId)
                .HasConstraintName("FK_CliFluidBalanceEntryRevision_CorrectedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EntryId, x.RevisionNumber }, "UX_CliFluidBalanceEntryRevision_Entry_Number")
                .IsUnique();

            builder.HasIndex(x => x.CorrectedByUserId, "IX_CliFluidBalanceEntryRevision_CorrectedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliBloodGlucoseReading</c> — migration <c>K5</c>.
    /// </summary>
    public class CliBloodGlucoseReadingConfiguration : IEntityTypeConfiguration<CliBloodGlucoseReading>
    {
        public void Configure(EntityTypeBuilder<CliBloodGlucoseReading> builder)
        {
            builder.ToTable("CliBloodGlucoseReading", "public", table =>
            {
                table.HasCheckConstraint("CK_CliBloodGlucoseReading_Unit", "\"GlucoseUnit\" IN (1, 2)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EncounterId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.InpEpisodeId)
                .IsRequired();

            builder.Property(x => x.MeasuredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.GlucoseValue)
                .HasColumnType("numeric(7,2)")
                .IsRequired();

            builder.Property(x => x.GlucoseUnit)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Method)
                .HasConversion<int>()
                .HasDefaultValue(BloodGlucoseMethod.WardGlucometer)
                .IsRequired();

            builder.Property(x => x.RecordedByEmployeeId)
                .IsRequired();

            builder.Property(x => x.RecordedByUserId)
                .IsRequired();

            builder.Property(x => x.ReadingStatus)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalMeasurementStatus.Active)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.CancelReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired(false);

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .HasConstraintName("FK_CliBloodGlucoseReading_EncounterId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .HasConstraintName("FK_CliBloodGlucoseReading_PatientId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .HasConstraintName("FK_CliBloodGlucoseReading_InpEpisodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByEmployeeId)
                .HasConstraintName("FK_CliBloodGlucoseReading_RecordedByEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .HasConstraintName("FK_CliBloodGlucoseReading_RecordedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.InpEpisodeId, x.MeasuredAt }, "IX_CliBloodGlucoseReading_Episode_MeasuredAt");

            builder.HasIndex(x => x.IdempotencyKey, "UX_CliBloodGlucoseReading_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            builder.HasIndex(x => x.EncounterId, "IX_CliBloodGlucoseReading_EncounterId");

            builder.HasIndex(x => x.PatientId, "IX_CliBloodGlucoseReading_PatientId");

            builder.HasIndex(x => x.RecordedByEmployeeId, "IX_CliBloodGlucoseReading_RecordedByEmployeeId");

            builder.HasIndex(x => x.RecordedByUserId, "IX_CliBloodGlucoseReading_RecordedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliBloodGlucoseReadingRevision</c> — migration <c>K5</c>.
    /// </summary>
    public class CliBloodGlucoseReadingRevisionConfiguration : IEntityTypeConfiguration<CliBloodGlucoseReadingRevision>
    {
        public void Configure(EntityTypeBuilder<CliBloodGlucoseReadingRevision> builder)
        {
            builder.ToTable("CliBloodGlucoseReadingRevision", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReadingId)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.PreviousValue)
                .HasColumnType("numeric(7,2)")
                .IsRequired();

            builder.Property(x => x.PreviousUnit)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.PreviousMeasuredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.CorrectionReason)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CorrectedByUserId)
                .IsRequired();

            builder.Property(x => x.CorrectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<CliBloodGlucoseReading>()
                .WithMany()
                .HasForeignKey(x => x.ReadingId)
                .HasConstraintName("FK_CliBloodGlucoseReadingRevision_ReadingId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CorrectedByUserId)
                .HasConstraintName("FK_CliBloodGlucoseReadingRevision_CorrectedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ReadingId, x.RevisionNumber }, "UX_CliBloodGlucoseReadingRevision_Reading_Number")
                .IsUnique();

            builder.HasIndex(x => x.CorrectedByUserId, "IX_CliBloodGlucoseReadingRevision_CorrectedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliDailyObservation</c> — migration <c>K5</c>.
    /// </summary>
    public class CliDailyObservationConfiguration : IEntityTypeConfiguration<CliDailyObservation>
    {
        public void Configure(EntityTypeBuilder<CliDailyObservation> builder)
        {
            builder.ToTable("CliDailyObservation", "public", table =>
            {
                table.HasCheckConstraint("CK_CliDailyObservation_DietPercent", "\"DietIntakePercent\" IS NULL OR (\"DietIntakePercent\" >= 0 AND \"DietIntakePercent\" <= 100)");
                table.HasCheckConstraint("CK_CliDailyObservation_Abdominal", "\"AbdominalCircumferenceCm\" IS NULL OR (\"AbdominalCircumferenceCm\" >= 20 AND \"AbdominalCircumferenceCm\" <= 250)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EncounterId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.InpEpisodeId)
                .IsRequired();

            builder.Property(x => x.ObservedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.DietIntakePercent)
                .IsRequired(false);

            builder.Property(x => x.DietNote)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.MobilizationLevel)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.AbdominalCircumferenceCm)
                .HasColumnType("numeric(5,1)")
                .IsRequired(false);

            builder.Property(x => x.IsAgitated)
                .IsRequired(false);

            builder.Property(x => x.Note)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.RecordedByEmployeeId)
                .IsRequired();

            builder.Property(x => x.RecordedByUserId)
                .IsRequired();

            builder.Property(x => x.ObservationStatus)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalMeasurementStatus.Active)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.CancelReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired(false);

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .HasConstraintName("FK_CliDailyObservation_EncounterId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .HasConstraintName("FK_CliDailyObservation_PatientId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .HasConstraintName("FK_CliDailyObservation_InpEpisodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByEmployeeId)
                .HasConstraintName("FK_CliDailyObservation_RecordedByEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .HasConstraintName("FK_CliDailyObservation_RecordedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.InpEpisodeId, x.ObservedAt }, "IX_CliDailyObservation_Episode_ObservedAt");

            builder.HasIndex(x => x.IdempotencyKey, "UX_CliDailyObservation_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            builder.HasIndex(x => x.EncounterId, "IX_CliDailyObservation_EncounterId");

            builder.HasIndex(x => x.PatientId, "IX_CliDailyObservation_PatientId");

            builder.HasIndex(x => x.RecordedByEmployeeId, "IX_CliDailyObservation_RecordedByEmployeeId");

            builder.HasIndex(x => x.RecordedByUserId, "IX_CliDailyObservation_RecordedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliDailyObservationRevision</c> — migration <c>K5</c>.
    /// </summary>
    public class CliDailyObservationRevisionConfiguration : IEntityTypeConfiguration<CliDailyObservationRevision>
    {
        public void Configure(EntityTypeBuilder<CliDailyObservationRevision> builder)
        {
            builder.ToTable("CliDailyObservationRevision", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ObservationId)
                .IsRequired();

            builder.Property(x => x.RevisionNumber)
                .IsRequired();

            builder.Property(x => x.PreviousValuesJson)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(x => x.CorrectionReason)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CorrectedByUserId)
                .IsRequired();

            builder.Property(x => x.CorrectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<CliDailyObservation>()
                .WithMany()
                .HasForeignKey(x => x.ObservationId)
                .HasConstraintName("FK_CliDailyObservationRevision_ObservationId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CorrectedByUserId)
                .HasConstraintName("FK_CliDailyObservationRevision_CorrectedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ObservationId, x.RevisionNumber }, "UX_CliDailyObservationRevision_Observation_Number")
                .IsUnique();

            builder.HasIndex(x => x.CorrectedByUserId, "IX_CliDailyObservationRevision_CorrectedByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliNursingShift</c> — migration <c>K5</c>.
    /// </summary>
    public class CliNursingShiftConfiguration : IEntityTypeConfiguration<CliNursingShift>
    {
        public void Configure(EntityTypeBuilder<CliNursingShift> builder)
        {
            builder.ToTable("CliNursingShift", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            builder.Property(x => x.ShiftCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.ShiftName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.StartTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            builder.Property(x => x.EndTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<MstServiceUnit>()
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .HasConstraintName("FK_CliNursingShift_ServiceUnitId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ServiceUnitId, x.ShiftCode }, "UX_CliNursingShift_Unit_Code")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .AreNullsDistinct(false);
        }
    }
}
