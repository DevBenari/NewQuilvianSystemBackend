using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Configurations
{
    public class TrxPatientConsentConfiguration : IEntityTypeConfiguration<TrxPatientConsent>
    {
        public void Configure(EntityTypeBuilder<TrxPatientConsent> builder)
        {
            builder.ToTable("TrxPatientConsent", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.ConsentNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ConsentTitle)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.ConsentCode)
                .HasMaxLength(100);

            builder.Property(x => x.ConsentCategoryName)
                .HasMaxLength(250);

            builder.Property(x => x.ConsentDescription)
                .HasMaxLength(1000);

            builder.Property(x => x.ProcedureCodeSnapshot)
                .HasMaxLength(50);

            builder.Property(x => x.ProcedureNameSnapshot)
                .HasMaxLength(250);

            builder.Property(x => x.ProcedureTypeSnapshot)
                .HasMaxLength(100);

            builder.Property(x => x.ProcedureLocation)
                .HasMaxLength(250);

            builder.Property(x => x.DiagnosisExplanation)
                .HasMaxLength(2000);

            builder.Property(x => x.ProcedureExplanation)
                .HasMaxLength(2000);

            builder.Property(x => x.BenefitExplanation)
                .HasMaxLength(2000);

            builder.Property(x => x.RiskExplanation)
                .HasMaxLength(2000);

            builder.Property(x => x.AlternativeExplanation)
                .HasMaxLength(2000);

            builder.Property(x => x.ConsequenceExplanation)
                .HasMaxLength(2000);

            builder.Property(x => x.PatientQuestionNote)
                .HasMaxLength(1000);

            builder.Property(x => x.SignerName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.SignerRelationship)
                .HasMaxLength(100);

            builder.Property(x => x.SignerIdentityType)
                .HasMaxLength(50);

            builder.Property(x => x.SignerIdentityNumber)
                .HasMaxLength(100);

            builder.Property(x => x.SignerPhoneNumber)
                .HasMaxLength(30);

            builder.Property(x => x.SignerAddress)
                .HasMaxLength(500);

            builder.Property(x => x.WitnessName)
                .HasMaxLength(200);

            builder.Property(x => x.WitnessRelationship)
                .HasMaxLength(100);

            builder.Property(x => x.PatientSignaturePath)
                .HasMaxLength(500);

            builder.Property(x => x.SignerSignaturePath)
                .HasMaxLength(500);

            builder.Property(x => x.DoctorSignaturePath)
                .HasMaxLength(500);

            builder.Property(x => x.WitnessSignaturePath)
                .HasMaxLength(500);

            builder.Property(x => x.ConsentFilePath)
                .HasMaxLength(500);

            builder.Property(x => x.ConsentFileName)
                .HasMaxLength(250);

            builder.Property(x => x.ConsentMimeType)
                .HasMaxLength(150);

            builder.Property(x => x.ConsentFileHash)
                .HasMaxLength(256);

            builder.Property(x => x.VerificationNote)
                .HasMaxLength(500);

            builder.Property(x => x.ApprovalNote)
                .HasMaxLength(500);

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            builder.Property(x => x.WithdrawalReason)
                .HasMaxLength(500);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            // =========================
            // DEFAULT VALUES
            // =========================

            builder.Property(x => x.IsDiagnosisExplained)
                .HasDefaultValue(false);

            builder.Property(x => x.IsProcedureExplained)
                .HasDefaultValue(false);

            builder.Property(x => x.IsRiskExplained)
                .HasDefaultValue(false);

            builder.Property(x => x.IsAlternativeExplained)
                .HasDefaultValue(false);

            builder.Property(x => x.IsPatientUnderstood)
                .HasDefaultValue(false);

            builder.Property(x => x.IsPatientAgreed)
                .HasDefaultValue(false);

            builder.Property(x => x.IsEmergencyConsent)
                .HasDefaultValue(false);

            builder.Property(x => x.IsHighRiskConsent)
                .HasDefaultValue(false);

            builder.Property(x => x.IsLegalDocument)
                .HasDefaultValue(true);

            builder.Property(x => x.IsPartOfMedicalRecord)
                .HasDefaultValue(true);

            builder.Property(x => x.IsSignerPatient)
                .HasDefaultValue(true);

            builder.Property(x => x.IsSignerLegalRepresentative)
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            builder.Property(x => x.IsApproved)
                .HasDefaultValue(false);

            builder.Property(x => x.IsRejected)
                .HasDefaultValue(false);

            builder.Property(x => x.IsWithdrawn)
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

            builder.HasIndex(x => x.ConsentNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.QueueId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.PatientProcedureId);

            builder.HasIndex(x => x.ClinicalDocumentId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.ServiceUnitId);

            builder.HasIndex(x => x.ClinicId);

            builder.HasIndex(x => x.ExplainedByDoctorId);

            builder.HasIndex(x => x.ExplainedByUserId);

            builder.HasIndex(x => x.VerifiedByUserId);

            builder.HasIndex(x => x.ApprovedByUserId);

            builder.HasIndex(x => x.ConsentFileHash);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.ConsentType,
                x.ConsentStatus
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.ConsentDateTime
            });

            builder.HasIndex(x => new
            {
                x.EncounterId,
                x.ConsentDateTime
            });

            builder.HasIndex(x => new
            {
                x.ConsultationId,
                x.ConsentDateTime
            });

            builder.HasIndex(x => new
            {
                x.PatientProcedureId,
                x.ConsentStatus
            });

            builder.HasIndex(x => new
            {
                x.IsVerified,
                x.IsApproved,
                x.IsRejected
            });

            builder.HasIndex(x => new
            {
                x.IsWithdrawn,
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

            builder.HasOne(x => x.ExplainedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.ExplainedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExplainedByUser)
                .WithMany()
                .HasForeignKey(x => x.ExplainedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WitnessByUser)
                .WithMany()
                .HasForeignKey(x => x.WitnessByUserId)
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

            builder.HasOne(x => x.WithdrawnByUser)
                .WithMany()
                .HasForeignKey(x => x.WithdrawnByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}