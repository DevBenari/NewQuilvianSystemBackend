using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionConfiguration : IEntityTypeConfiguration<TrxPrescription>
    {
        public void Configure(EntityTypeBuilder<TrxPrescription> entity)
        {
            entity.ToTable("TrxPrescription", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EncounterId).IsRequired();
            entity.Property(x => x.ConsultationId).IsRequired();
            entity.Property(x => x.PatientId).IsRequired();
            entity.Property(x => x.DoctorId).IsRequired();
            entity.Property(x => x.ServiceUnitId).IsRequired();
            entity.Property(x => x.ClinicId).IsRequired(false);
            entity.Property(x => x.PaymentSourceId).IsRequired(false);
            entity.Property(x => x.PatientInsuranceId).IsRequired(false);
            entity.Property(x => x.InsuranceProviderId).IsRequired(false);

            entity.Property(x => x.PaymentTypeSnapshot).HasConversion<int>().IsRequired();
            entity.Property(x => x.PatientClassNameSnapshot).HasMaxLength(100);
            entity.Property(x => x.PaymentSourceNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.InsuranceProviderNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.PolicyNumberSnapshot).HasMaxLength(100);
            entity.Property(x => x.BenefitPlanCodeSnapshot).HasMaxLength(100);
            entity.Property(x => x.BenefitPlanNameSnapshot).HasMaxLength(150);

            entity.Property(x => x.PrescriptionStatus)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionStatus.Draft)
                .IsRequired();

            entity.Property(x => x.PaymentStatus)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionPaymentStatus.NotBilled)
                .IsRequired();

            entity.Property(x => x.FulfillmentStatus)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionFulfillmentStatus.WaitingForClinicalFinalization)
                .IsRequired();

            entity.Property(x => x.PrescriptionDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            ConfigureNullableTimestamp(entity, x => x.SubmittedAt);
            ConfigureNullableTimestamp(entity, x => x.BillingGeneratedAt);
            ConfigureNullableTimestamp(entity, x => x.PaymentCompletedAt);
            ConfigureNullableTimestamp(entity, x => x.ReadyForPharmacyAt);
            ConfigureNullableTimestamp(entity, x => x.PharmacyQueuedAt);
            ConfigureNullableTimestamp(entity, x => x.PharmacyVerifiedAt);
            ConfigureNullableTimestamp(entity, x => x.PreparationStartedAt);
            ConfigureNullableTimestamp(entity, x => x.ReadyToDispenseAt);
            ConfigureNullableTimestamp(entity, x => x.DispensedAt);
            ConfigureNullableTimestamp(entity, x => x.CancelledAt);

            entity.Property(x => x.ClinicalNote).HasMaxLength(1000);
            entity.Property(x => x.DoctorInstruction).HasMaxLength(1000);
            entity.Property(x => x.PharmacyNote).HasMaxLength(1000);
            entity.Property(x => x.CancelReason).HasMaxLength(250);

            entity.Property(x => x.RegularItemCount).HasDefaultValue(0);
            entity.Property(x => x.CompoundCount).HasDefaultValue(0);
            entity.Property(x => x.CompoundIngredientCount).HasDefaultValue(0);
            entity.Property(x => x.TotalItemCount).HasDefaultValue(0);
            entity.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.CoveredAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.PatientPayAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.IsNeedApproval).HasDefaultValue(false);
            entity.Property(x => x.IsApproved).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Consultation).WithMany().HasForeignKey(x => x.ConsultationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ServiceUnit).WithMany().HasForeignKey(x => x.ServiceUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Clinic).WithMany().HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSource).WithMany().HasForeignKey(x => x.PaymentSourceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PatientInsurance).WithMany().HasForeignKey(x => x.PatientInsuranceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InsuranceProvider).WithMany().HasForeignKey(x => x.InsuranceProviderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentCompletedByUser).WithMany().HasForeignKey(x => x.PaymentCompletedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PharmacyVerifiedByUser).WithMany().HasForeignKey(x => x.PharmacyVerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DispensedByUser).WithMany().HasForeignKey(x => x.DispensedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PrescriptionNumber).IsUnique();
            entity.HasIndex(x => x.ConsultationId).IsUnique().HasFilter("\"IsDelete\" = false AND \"IsCancel\" = false");
            entity.HasIndex(x => x.EncounterId);
            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.DoctorId);
            entity.HasIndex(x => x.ServiceUnitId);
            entity.HasIndex(x => x.ClinicId);
            entity.HasIndex(x => x.PaymentSourceId);
            entity.HasIndex(x => x.PatientInsuranceId);
            entity.HasIndex(x => x.InsuranceProviderId);
            entity.HasIndex(x => x.BillingId);
            entity.HasIndex(x => x.PharmacyQueueId);
            entity.HasIndex(x => new { x.PrescriptionStatus, x.PaymentStatus, x.FulfillmentStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PatientId, x.PrescriptionDateTime, x.IsDelete });
            entity.HasIndex(x => new { x.DoctorId, x.PrescriptionDateTime, x.PrescriptionStatus, x.IsDelete });
            entity.HasIndex(x => new { x.EncounterId, x.PrescriptionStatus, x.PaymentStatus, x.IsDelete });
        }

        private static void ConfigureNullableTimestamp(
            EntityTypeBuilder<TrxPrescription> entity,
            System.Linq.Expressions.Expression<Func<TrxPrescription, DateTime?>> property)
        {
            entity.Property(property)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }
    }
}
