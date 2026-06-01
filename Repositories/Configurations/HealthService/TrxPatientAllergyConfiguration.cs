using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Configurations
{
    public class TrxPatientAllergyConfiguration : IEntityTypeConfiguration<TrxPatientAllergy>
    {
        public void Configure(EntityTypeBuilder<TrxPatientAllergy> builder)
        {
            builder.ToTable("TrxPatientAllergy", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.AllergyRecordNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.AllergenCode)
                .HasMaxLength(100);

            builder.Property(x => x.AllergenName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.AllergenGroupName)
                .HasMaxLength(250);

            builder.Property(x => x.ReactionType)
                .HasMaxLength(100);

            builder.Property(x => x.ReactionDescription)
                .HasMaxLength(1000);

            builder.Property(x => x.SourceOfInformation)
                .HasMaxLength(100);

            builder.Property(x => x.PatientSafetyNote)
                .HasMaxLength(1000);

            builder.Property(x => x.ClinicalNote)
                .HasMaxLength(1000);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.ResolvedReason)
                .HasMaxLength(250);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(250);

            builder.Property(x => x.IsHighRisk)
                .HasDefaultValue(false);

            builder.Property(x => x.IsLifeThreatening)
                .HasDefaultValue(false);

            builder.Property(x => x.IsAlertEnabled)
                .HasDefaultValue(true);

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

            builder.HasIndex(x => x.AllergyRecordNumber)
                .IsUnique();

            builder.HasIndex(x => x.PatientId);

            builder.HasIndex(x => x.EncounterId);

            builder.HasIndex(x => x.ConsultationId);

            builder.HasIndex(x => x.AssessmentId);

            builder.HasIndex(x => x.DoctorId);

            builder.HasIndex(x => x.DrugId);

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.AllergyStatus,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.AllergyCategory,
                x.AllergenName
            });

            builder.HasIndex(x => new
            {
                x.PatientId,
                x.IsAlertEnabled,
                x.IsHighRisk,
                x.IsLifeThreatening
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

            builder.HasOne(x => x.Drug)
                .WithMany()
                .HasForeignKey(x => x.DrugId)
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