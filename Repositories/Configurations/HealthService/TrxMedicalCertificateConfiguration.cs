using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxMedicalCertificateConfiguration : IEntityTypeConfiguration<TrxMedicalCertificate>
    {
        public void Configure(EntityTypeBuilder<TrxMedicalCertificate> builder)
        {
            builder.ToTable("TrxMedicalCertificate", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.MedicalCertificateNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CertificateTitle)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.CertificateCode)
                .HasMaxLength(100);

            builder.Property(x => x.CertificateCategoryName)
                .HasMaxLength(250);

            builder.Property(x => x.CertificatePurpose)
                .HasMaxLength(250);

            builder.Property(x => x.DiagnosisCodeSnapshot)
                .HasMaxLength(50);

            builder.Property(x => x.DiagnosisNameSnapshot)
                .HasMaxLength(500);

            builder.Property(x => x.DiagnosisMasterType)
                .HasMaxLength(50)
                .HasDefaultValue("Manual");

            builder.Property(x => x.IcdVersion)
                .HasMaxLength(100);

            builder.Property(x => x.ClinicalSummary)
                .HasMaxLength(1000);

            builder.Property(x => x.MedicalRecommendation)
                .HasMaxLength(1000);

            builder.Property(x => x.CertificateStatement)
                .HasMaxLength(4000);

            builder.Property(x => x.AdditionalStatement)
                .HasMaxLength(2000);

            builder.Property(x => x.RestrictionNote)
                .HasMaxLength(1000);

            builder.Property(x => x.FollowUpInstruction)
                .HasMaxLength(1000);

            builder.Property(x => x.SickLeaveReason)
                .HasMaxLength(250);

            builder.Property(x => x.ControlClinicName)
                .HasMaxLength(250);

            builder.Property(x => x.ReferralToProviderName)
                .HasMaxLength(250);

            builder.Property(x => x.ReferralToDepartmentName)
                .HasMaxLength(250);

            builder.Property(x => x.ReferralReason)
                .HasMaxLength(1000);

            builder.Property(x => x.CauseOfDeath)
                .HasMaxLength(500);

            builder.Property(x => x.FitnessAssessmentNote)
                .HasMaxLength(1000);

            builder.Property(x => x.WorkRestrictionNote)
                .HasMaxLength(1000);

            builder.Property(x => x.RequestedByName)
                .HasMaxLength(200);

            builder.Property(x => x.RequestedByRelationship)
                .HasMaxLength(100);

            builder.Property(x => x.RecipientName)
                .HasMaxLength(250);

            builder.Property(x => x.RecipientInstitutionName)
                .HasMaxLength(250);

            builder.Property(x => x.RecipientAddress)
                .HasMaxLength(500);

            builder.Property(x => x.IssuePlace)
                .HasMaxLength(250);

            builder.Property(x => x.CertificateFilePath)
                .HasMaxLength(500);

            builder.Property(x => x.CertificateFileName)
                .HasMaxLength(250);

            builder.Property(x => x.CertificateMimeType)
                .HasMaxLength(150);

            builder.Property(x => x.CertificateFileHash)
                .HasMaxLength(256);

            builder.Property(x => x.QrCodePath)
                .HasMaxLength(500);

            builder.Property(x => x.VerificationCode)
                .HasMaxLength(250);

            builder.Property(x => x.VerificationUrl)
                .HasMaxLength(500);

            builder.Property(x => x.VerificationNote)
                .HasMaxLength(500);

            builder.Property(x => x.ApprovalNote)
                .HasMaxLength(500);

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            builder.Property(x => x.RevocationReason)
                .HasMaxLength(500);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            // =========================
            // DEFAULT VALUES
            // =========================

            builder.Property(x => x.IsIssued)
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            builder.Property(x => x.IsApproved)
                .HasDefaultValue(false);

            builder.Property(x => x.IsRejected)
                .HasDefaultValue(false);

            builder.Property(x => x.IsRevoked)
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

            builder.HasIndex(x => x.MedicalCertificateNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.QueueId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.PatientDiagnosisId);

            builder.HasIndex(x => x.DiagnosisId);

            builder.HasIndex(x => x.ClinicalDocumentId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.ServiceUnitId);

            builder.HasIndex(x => x.ClinicId);

            builder.HasIndex(x => x.IssuedByDoctorId);

            builder.HasIndex(x => x.IssuedByUserId);

            builder.HasIndex(x => x.VerifiedByUserId);

            builder.HasIndex(x => x.ApprovedByUserId);

            builder.HasIndex(x => x.CertificateFileHash);

            builder.HasIndex(x => x.VerificationCode);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.CertificateType,
                x.CertificateStatus
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.CertificateDateTime
            });

            builder.HasIndex(x => new
            {
                x.EncounterId,
                x.CertificateDateTime
            });

            builder.HasIndex(x => new
            {
                x.ConsultationId,
                x.CertificateDateTime
            });

            builder.HasIndex(x => new
            {
                x.CertificateType,
                x.IssuedAt
            });

            builder.HasIndex(x => new
            {
                x.IsIssued,
                x.IsVerified,
                x.IsApproved
            });

            builder.HasIndex(x => new
            {
                x.IsRejected,
                x.IsRevoked,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.ExpiredDate,
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

            builder.HasOne(x => x.Diagnosis)
                .WithMany()
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ClinicalDocument)
                .WithMany()
                .HasForeignKey(x => x.ClinicalDocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.IssuedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.IssuedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.IssuedByUser)
                .WithMany()
                .HasForeignKey(x => x.IssuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RevokedByUser)
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
