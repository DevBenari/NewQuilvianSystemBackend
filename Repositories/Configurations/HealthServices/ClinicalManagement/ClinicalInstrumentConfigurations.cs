using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel <c>CliClinicalInstrument</c> — migration <c>K1</c>.
    /// </summary>
    public class CliClinicalInstrumentConfiguration : IEntityTypeConfiguration<CliClinicalInstrument>
    {
        public void Configure(EntityTypeBuilder<CliClinicalInstrument> builder)
        {
            builder.ToTable("CliClinicalInstrument", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.InstrumentKind)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.TargetMinAgeMonths)
                .IsRequired(false);

            builder.Property(x => x.TargetMaxAgeMonths)
                .IsRequired(false);

            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasIndex(x => x.Code, "UX_CliClinicalInstrument_Code")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.InstrumentKind, "IX_CliClinicalInstrument_Kind");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliClinicalInstrumentVersion</c> — migration <c>K1</c>.
    /// </summary>
    public class CliClinicalInstrumentVersionConfiguration : IEntityTypeConfiguration<CliClinicalInstrumentVersion>
    {
        public void Configure(EntityTypeBuilder<CliClinicalInstrumentVersion> builder)
        {
            builder.ToTable("CliClinicalInstrumentVersion", "public", table =>
            {
                table.HasCheckConstraint("CK_CliClinicalInstrumentVersion_ApproverDiffers", "\"ApprovedByUserId\" IS NULL OR \"ApprovedByUserId\" <> \"LastModifiedByUserId\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InstrumentId)
                .IsRequired();

            builder.Property(x => x.VersionNumber)
                .IsRequired();

            builder.Property(x => x.VersionStatus)
                .HasConversion<int>()
                .HasDefaultValue(ClinicalInstrumentVersionStatus.Draft)
                .IsRequired();

            builder.Property(x => x.DefinitionJson)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(x => x.DefinitionHash)
                .HasMaxLength(64)
                .IsFixedLength()
                .IsRequired();

            builder.Property(x => x.LastModifiedByUserId)
                .IsRequired();

            builder.Property(x => x.LastModifiedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            builder.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.ApprovalNote)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.RetiredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.RetiredByUserId)
                .IsRequired(false);

            builder.Property(x => x.RetireReason)
                .HasMaxLength(500)
                .IsRequired(false);

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<CliClinicalInstrument>()
                .WithMany()
                .HasForeignKey(x => x.InstrumentId)
                .HasConstraintName("FK_CliClinicalInstrumentVersion_InstrumentId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.LastModifiedByUserId)
                .HasConstraintName("FK_CliClinicalInstrumentVersion_LastModifiedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .HasConstraintName("FK_CliClinicalInstrumentVersion_ApprovedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RetiredByUserId)
                .HasConstraintName("FK_CliClinicalInstrumentVersion_RetiredByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.InstrumentId, x.VersionNumber }, "UX_CliClinicalInstrumentVersion_Instrument_Version")
                .IsUnique();

            builder.HasIndex(x => x.InstrumentId, "UX_CliClinicalInstrumentVersion_Approved")
                .IsUnique()
                .HasFilter("\"VersionStatus\" = 2 AND \"IsDelete\" = false");

            builder.HasIndex(x => x.LastModifiedByUserId, "IX_CliClinicalInstrumentVersion_LastModifiedByUserId");

            builder.HasIndex(x => x.ApprovedByUserId, "IX_CliClinicalInstrumentVersion_ApprovedByUserId");

            builder.HasIndex(x => x.RetiredByUserId, "IX_CliClinicalInstrumentVersion_RetiredByUserId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliAssessmentInstrumentResponse</c> — migration <c>K1</c>.
    /// </summary>
    public class CliAssessmentInstrumentResponseConfiguration : IEntityTypeConfiguration<CliAssessmentInstrumentResponse>
    {
        public void Configure(EntityTypeBuilder<CliAssessmentInstrumentResponse> builder)
        {
            builder.ToTable("CliAssessmentInstrumentResponse", "public", table =>
            {
                table.HasCheckConstraint("CK_CliAssessmentInstrumentResponse_OneOwner", "num_nonnulls(\"AssessmentId\", \"CaseManagementEvaluationId\") = 1");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssessmentId)
                .IsRequired(false);

            builder.Property(x => x.CaseManagementEvaluationId)
                .IsRequired(false);

            builder.Property(x => x.InstrumentVersionId)
                .IsRequired();

            builder.Property(x => x.DefinitionHashSnapshot)
                .HasMaxLength(64)
                .IsFixedLength()
                .IsRequired();

            builder.Property(x => x.ResponsesJson)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'::jsonb")
                .IsRequired();

            builder.Property(x => x.TotalScore)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            builder.Property(x => x.BandCode)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.BandLabelSnapshot)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.IsAlertBand)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.ComputedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<TrxPatientAssessment>()
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .HasConstraintName("FK_CliAssessmentInstrumentResponse_AssessmentId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CliCaseManagementEvaluation>()
                .WithMany()
                .HasForeignKey(x => x.CaseManagementEvaluationId)
                .HasConstraintName("FK_CliAssessmentInstrumentResponse_CaseManagementEvaluationId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CliClinicalInstrumentVersion>()
                .WithMany()
                .HasForeignKey(x => x.InstrumentVersionId)
                .HasConstraintName("FK_CliAssessmentInstrumentResponse_InstrumentVersionId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AssessmentId, x.InstrumentVersionId }, "UX_CliAssessmentInstrumentResponse_Assessment_Version")
                .IsUnique()
                .HasFilter("\"AssessmentId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.CaseManagementEvaluationId, x.InstrumentVersionId }, "UX_CliAssessmentInstrumentResponse_Evaluation_Version")
                .IsUnique()
                .HasFilter("\"CaseManagementEvaluationId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => x.InstrumentVersionId, "IX_CliAssessmentInstrumentResponse_InstrumentVersionId");
        }
    }

    /// <summary>
    /// Bentuk tabel <c>CliCaseManagementEvaluation</c> — migration <c>K3</c>.
    /// </summary>
    public class CliCaseManagementEvaluationConfiguration : IEntityTypeConfiguration<CliCaseManagementEvaluation>
    {
        public void Configure(EntityTypeBuilder<CliCaseManagementEvaluation> builder)
        {
            builder.ToTable("CliCaseManagementEvaluation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EvaluationNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.EncounterId)
                .IsRequired();

            builder.Property(x => x.InpEpisodeId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.ServiceUnitIdSnapshot)
                .IsRequired();

            builder.Property(x => x.EvaluationStatus)
                .HasConversion<int>()
                .HasDefaultValue(CaseManagementEvaluationStatus.Draft)
                .IsRequired();

            builder.Property(x => x.AuthorEmployeeId)
                .IsRequired();

            builder.Property(x => x.AuthorUserId)
                .IsRequired();

            builder.Property(x => x.ClinicalDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.CompletedByUserId)
                .IsRequired(false);

            builder.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.CancelledByUserId)
                .IsRequired(false);

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
                .HasConstraintName("FK_CliCaseManagementEvaluation_EncounterId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .HasConstraintName("FK_CliCaseManagementEvaluation_InpEpisodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .HasConstraintName("FK_CliCaseManagementEvaluation_PatientId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.AuthorEmployeeId)
                .HasConstraintName("FK_CliCaseManagementEvaluation_AuthorEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.AuthorUserId)
                .HasConstraintName("FK_CliCaseManagementEvaluation_AuthorUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CompletedByUserId)
                .HasConstraintName("FK_CliCaseManagementEvaluation_CompletedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .HasConstraintName("FK_CliCaseManagementEvaluation_CancelledByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.EvaluationNumber, "UX_CliCaseManagementEvaluation_Number")
                .IsUnique();

            builder.HasIndex(x => x.InpEpisodeId, "UX_CliCaseManagementEvaluation_Episode_Active")
                .IsUnique()
                .HasFilter("\"EvaluationStatus\" IN (1, 2) AND \"IsDelete\" = false");

            builder.HasIndex(x => x.IdempotencyKey, "UX_CliCaseManagementEvaluation_IdempotencyKey")
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            builder.HasIndex(x => x.EncounterId, "IX_CliCaseManagementEvaluation_EncounterId");

            builder.HasIndex(x => x.PatientId, "IX_CliCaseManagementEvaluation_PatientId");

            builder.HasIndex(x => x.AuthorEmployeeId, "IX_CliCaseManagementEvaluation_AuthorEmployeeId");

            builder.HasIndex(x => x.AuthorUserId, "IX_CliCaseManagementEvaluation_AuthorUserId");

            builder.HasIndex(x => x.CompletedByUserId, "IX_CliCaseManagementEvaluation_CompletedByUserId");

            builder.HasIndex(x => x.CancelledByUserId, "IX_CliCaseManagementEvaluation_CancelledByUserId");
        }
    }
}
