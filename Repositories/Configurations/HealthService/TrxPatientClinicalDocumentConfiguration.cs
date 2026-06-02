using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Configurations
{
    public class TrxPatientClinicalDocumentConfiguration : IEntityTypeConfiguration<TrxPatientClinicalDocument>
    {
        public void Configure(EntityTypeBuilder<TrxPatientClinicalDocument> builder)
        {
            builder.ToTable("TrxPatientClinicalDocument", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.ClinicalDocumentNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.DocumentTitle)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.DocumentCode)
                .HasMaxLength(100);

            builder.Property(x => x.DocumentCategoryName)
                .HasMaxLength(250);

            builder.Property(x => x.ExternalDocumentNumber)
                .HasMaxLength(250);

            builder.Property(x => x.ExternalProviderName)
                .HasMaxLength(250);

            builder.Property(x => x.ExternalDoctorName)
                .HasMaxLength(250);

            builder.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.FileName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.OriginalFileName)
                .HasMaxLength(250);

            builder.Property(x => x.FileExtension)
                .HasMaxLength(100);

            builder.Property(x => x.MimeType)
                .HasMaxLength(150);

            builder.Property(x => x.FileHash)
                .HasMaxLength(256);

            builder.Property(x => x.StorageProvider)
                .HasMaxLength(100);

            builder.Property(x => x.ThumbnailPath)
                .HasMaxLength(500);

            builder.Property(x => x.PreviewPath)
                .HasMaxLength(500);

            builder.Property(x => x.DocumentSummary)
                .HasMaxLength(1000);

            builder.Property(x => x.ClinicalFindingSummary)
                .HasMaxLength(1000);

            builder.Property(x => x.Impression)
                .HasMaxLength(1000);

            builder.Property(x => x.Recommendation)
                .HasMaxLength(1000);

            builder.Property(x => x.Keywords)
                .HasMaxLength(500);

            builder.Property(x => x.ReviewNote)
                .HasMaxLength(500);

            builder.Property(x => x.VerificationNote)
                .HasMaxLength(500);

            builder.Property(x => x.ApprovalNote)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.ArchiveReason)
                .HasMaxLength(250);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            // =========================
            // DEFAULT VALUES
            // =========================

            builder.Property(x => x.IsConfidential)
                .HasDefaultValue(false);

            builder.Property(x => x.IsPatientVisible)
                .HasDefaultValue(false);

            builder.Property(x => x.IsShareable)
                .HasDefaultValue(false);

            builder.Property(x => x.IsExternalDocument)
                .HasDefaultValue(false);

            builder.Property(x => x.IsPartOfMedicalRecord)
                .HasDefaultValue(true);

            builder.Property(x => x.IsLegalDocument)
                .HasDefaultValue(false);

            builder.Property(x => x.IsNeedReview)
                .HasDefaultValue(false);

            builder.Property(x => x.IsReviewed)
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            builder.Property(x => x.IsApproved)
                .HasDefaultValue(false);

            builder.Property(x => x.IsArchived)
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

            builder.HasIndex(x => x.ClinicalDocumentNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.QueueId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.PatientDiagnosisId);

            builder.HasIndex(x => x.PatientProcedureId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.ServiceUnitId);

            builder.HasIndex(x => x.ClinicId);

            builder.HasIndex(x => x.UploadedByUserId);

            builder.HasIndex(x => x.FileHash);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.DocumentType,
                x.DocumentStatus
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.DocumentDateTime
            });

            builder.HasIndex(x => new
            {
                x.EncounterId,
                x.DocumentDateTime
            });

            builder.HasIndex(x => new
            {
                x.ConsultationId,
                x.DocumentDateTime
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsPartOfMedicalRecord,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsConfidential,
                x.ConfidentialityLevel
            });

            builder.HasIndex(x => new
            {
                x.IsNeedReview,
                x.IsReviewed
            });

            builder.HasIndex(x => new
            {
                x.IsVerified,
                x.IsApproved
            });

            builder.HasIndex(x => new
            {
                x.IsArchived,
                x.IsActive
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

            builder.HasOne(x => x.Queue)
                .WithMany()
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Assessment)
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientDiagnosis)
                .WithMany()
                .HasForeignKey(x => x.PatientDiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientProcedure)
                .WithMany()
                .HasForeignKey(x => x.PatientProcedureId)
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

            builder.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ArchivedByUser)
                .WithMany()
                .HasForeignKey(x => x.ArchivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}