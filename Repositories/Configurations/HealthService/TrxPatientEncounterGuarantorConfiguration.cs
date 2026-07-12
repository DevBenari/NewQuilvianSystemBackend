using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientEncounterGuarantorConfiguration : IEntityTypeConfiguration<TrxPatientEncounterGuarantor>
    {
        public void Configure(EntityTypeBuilder<TrxPatientEncounterGuarantor> entity)
        {
            entity.ToTable("TrxPatientEncounterGuarantor", "public");

            entity.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            entity.Property(x => x.PaymentSourceNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.PaymentType)
                .HasConversion<int>()
                .HasDefaultValue(EncounterPaymentType.Cash)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            // =========================
            // PAYMENT REFERENCES
            // =========================

            entity.Property(x => x.PaymentMethodId)
                .IsRequired(false);

            entity.Property(x => x.PatientInsuranceId)
                .IsRequired(false);

            entity.Property(x => x.InsuranceProviderId)
                .IsRequired(false);

            // =========================
            // REGISTRATION SNAPSHOT
            // =========================

            entity.Property(x => x.PaymentSourceNameSnapshot)
                .HasMaxLength(250);

            entity.Property(x => x.PolicyNumberSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.CardNumberSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.MemberNumberSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.PlanNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.ClassNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.BenefitPlanCodeSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.EffectiveStartDateSnapshot)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDateSnapshot)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsEligible)
                .HasDefaultValue(true);

            entity.Property(x => x.IsPolicyActive)
                .HasDefaultValue(false);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            // =========================
            // IDENTITY MODEL
            // =========================

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

            // =========================
            // RELATIONS
            // =========================

            entity.HasOne(x => x.Encounter)
                .WithOne(x => x.PaymentSource)
                .HasForeignKey<TrxPatientEncounterGuarantor>(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PaymentMethod)
                .WithMany()
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientInsurance)
                .WithMany()
                .HasForeignKey(x => x.PatientInsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InsuranceProvider)
                .WithMany()
                .HasForeignKey(x => x.InsuranceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // INDEXES
            // =========================

            entity.HasIndex(x => x.PaymentSourceNumber)
                .IsUnique();

            // Menjamin satu encounter hanya mempunyai satu sumber pembayaran.
            entity.HasIndex(x => x.EncounterId)
                .IsUnique();

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.PaymentMethodId);

            entity.HasIndex(x => x.PatientInsuranceId);

            entity.HasIndex(x => x.InsuranceProviderId);

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.PaymentType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.InsuranceProviderId,
                x.BenefitPlanCodeSnapshot,
                x.IsPolicyActive,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PolicyNumberSnapshot,
                x.CardNumberSnapshot,
                x.MemberNumberSnapshot
            });
        }
    }
}
