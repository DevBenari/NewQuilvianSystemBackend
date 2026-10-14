using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstPatientMembershipConfiguration : IEntityTypeConfiguration<MstPatientMembership>
    {
        public void Configure(EntityTypeBuilder<MstPatientMembership> entity)
        {
            entity.ToTable("MstPatientMembership", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.MembershipTierId)
                .IsRequired();

            entity.Property(x => x.MemberNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.MembershipStatus)
                .HasConversion<int>()
                .HasDefaultValue(MembershipStatus.PendingPayment)
                .IsRequired();

            entity.Property(x => x.RegistrationType)
                .HasConversion<int>()
                .HasDefaultValue(MembershipRegistrationType.New)
                .IsRequired();

            entity.Property(x => x.JoinDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.PeriodStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ExpiredDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.PaymentDueAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ActivatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.FeeAmountSnapshot)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m);

            entity.Property(x => x.CancellationCategory)
                .HasConversion<int?>()
                .IsRequired(false);

            entity.Property(x => x.CancellationReason)
                .HasMaxLength(500);

            entity.Property(x => x.FeeChargeReferenceId)
                .IsRequired(false);

            entity.Property(x => x.RowVersion)
                .IsConcurrencyToken();

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAutoCreated)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCreatedFromKiosk)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCreatedFromAdmission)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCreatedByMarketing)
                .HasDefaultValue(false);

            entity.Property(x => x.PointBalance)
                .HasDefaultValue(0);

            entity.Property(x => x.TotalSpendAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.LastUpgradeDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastDowngradeDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.UpgradeDowngradeReason)
                .HasMaxLength(250);

            entity.Property(x => x.Notes)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MembershipTier)
                .WithMany()
                .HasForeignKey(x => x.MembershipTierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PreviousPatientMembership)
                .WithMany()
                .HasForeignKey(x => x.PreviousPatientMembershipId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PreviousPatientMembershipId);

            // Dibaca proses terjadwal: kedaluwarsa, mulai periode Terjadwal, batal tidak dibayar.
            entity.HasIndex(x => new { x.MembershipStatus, x.ExpiredDate });

            entity.HasIndex(x => new { x.MembershipStatus, x.PeriodStartDate });

            entity.HasIndex(x => new { x.MembershipStatus, x.PaymentDueAt });

            entity.HasIndex(x => x.MemberNumber)
                .IsUnique();

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.MembershipTierId);

            // Per pasien paling banyak satu Aktif (1), satu Menunggu Pembayaran (7), dan satu
            // Terjadwal (8) — INV-01 s.d. INV-03 crm-membership. Menggantikan unik IsPrimary lama.
            entity.HasIndex(x => x.PatientId, "UX_MstPatientMembership_Patient_Active")
                .IsUnique()
                .HasFilter("\"MembershipStatus\" = 1 AND \"IsDelete\" = false");

            entity.HasIndex(x => x.PatientId, "UX_MstPatientMembership_Patient_Pending")
                .IsUnique()
                .HasFilter("\"MembershipStatus\" = 7 AND \"IsDelete\" = false");

            entity.HasIndex(x => x.PatientId, "UX_MstPatientMembership_Patient_Scheduled")
                .IsUnique()
                .HasFilter("\"MembershipStatus\" = 8 AND \"IsDelete\" = false");

            entity.HasIndex(x => x.FeeChargeReferenceId, "UX_MstPatientMembership_FeeChargeReferenceId")
                .IsUnique()
                .HasFilter("\"FeeChargeReferenceId\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.MembershipStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.MembershipTierId,
                x.MembershipStatus,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
