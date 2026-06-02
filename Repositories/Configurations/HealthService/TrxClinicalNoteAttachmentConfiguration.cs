using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxClinicalNoteAttachmentConfiguration : IEntityTypeConfiguration<TrxClinicalNoteAttachment>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalNoteAttachment> builder)
        {
            builder.ToTable("TrxClinicalNoteAttachment", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.AttachmentNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.AttachmentTitle)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.AttachmentCode)
                .HasMaxLength(100);

            builder.Property(x => x.AttachmentCategoryName)
                .HasMaxLength(250);

            builder.Property(x => x.AttachmentDescription)
                .HasMaxLength(1000);

            builder.Property(x => x.NoteSectionName)
                .HasMaxLength(250);

            builder.Property(x => x.ClinicalNote)
                .HasMaxLength(1000);

            builder.Property(x => x.FindingNote)
                .HasMaxLength(1000);

            builder.Property(x => x.InterpretationNote)
                .HasMaxLength(1000);

            builder.Property(x => x.FollowUpNote)
                .HasMaxLength(1000);

            builder.Property(x => x.BodySite)
                .HasMaxLength(250);

            builder.Property(x => x.BodySiteDescription)
                .HasMaxLength(250);

            builder.Property(x => x.ViewPosition)
                .HasMaxLength(100);

            builder.Property(x => x.CaptureDeviceName)
                .HasMaxLength(100);

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

            builder.Property(x => x.ReviewNote)
                .HasMaxLength(500);

            builder.Property(x => x.VerificationNote)
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

            builder.Property(x => x.IsPartOfMedicalRecord)
                .HasDefaultValue(true);

            builder.Property(x => x.IsClinicalMedia)
                .HasDefaultValue(true);

            builder.Property(x => x.IsBeforeAfterComparison)
                .HasDefaultValue(false);

            builder.Property(x => x.IsNeedReview)
                .HasDefaultValue(false);

            builder.Property(x => x.IsReviewed)
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
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

            builder.HasIndex(x => x.AttachmentNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.QueueId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.PatientDiagnosisId);

            builder.HasIndex(x => x.PatientProcedureId);

            builder.HasIndex(x => x.ClinicalDocumentId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.ServiceUnitId);

            builder.HasIndex(x => x.ClinicId);

            builder.HasIndex(x => x.UploadedByUserId);

            builder.HasIndex(x => x.CapturedByUserId);

            builder.HasIndex(x => x.FileHash);

            builder.HasIndex(x => x.RelatedAttachmentId);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.AttachmentType,
                x.AttachmentStatus
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.AttachmentContext,
                x.UploadedAt
            });

            builder.HasIndex(x => new
            {
                x.EncounterId,
                x.UploadedAt
            });

            builder.HasIndex(x => new
            {
                x.ConsultationId,
                x.UploadedAt
            });

            builder.HasIndex(x => new
            {
                x.PatientDiagnosisId,
                x.UploadedAt
            });

            builder.HasIndex(x => new
            {
                x.PatientProcedureId,
                x.UploadedAt
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
                x.IsArchived
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

            builder.HasOne(x => x.ClinicalDocument)
                .WithMany()
                .HasForeignKey(x => x.ClinicalDocumentId)
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

            builder.HasOne(x => x.RelatedAttachment)
                .WithMany()
                .HasForeignKey(x => x.RelatedAttachmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CapturedByUser)
                .WithMany()
                .HasForeignKey(x => x.CapturedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
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
