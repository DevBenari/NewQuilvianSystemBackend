using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilMembershipFeeChargeConfiguration : IEntityTypeConfiguration<BilMembershipFeeCharge>
{
    public void Configure(EntityTypeBuilder<BilMembershipFeeCharge> entity)
    {
        entity.ToTable("BilMembershipFeeCharge", "public", table =>
        {
            table.HasCheckConstraint("CK_BilMembershipFeeCharge_FeeAmount", "\"FeeAmount\" > 0");
            table.HasCheckConstraint(
                "CK_BilMembershipFeeCharge_PaidAmount",
                "\"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"FeeAmount\"");
            table.HasCheckConstraint(
                "CK_BilMembershipFeeCharge_Status",
                "\"Status\" IN ('UNPAID','PARTIALLY_PAID','PAID','CANCELLED')");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.ChargeNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.FeeAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.CancellationReason).HasMaxLength(500);
        entity.Property(x => x.PaymentDueAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.SettledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.MembershipConfirmedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Idempotency: satu membership paling banyak satu tagihan iuran.
        entity.HasIndex(x => x.MembershipId).IsUnique();
        entity.HasIndex(x => x.ChargeNumber).IsUnique();
        entity.HasIndex(x => x.PatientId);
        entity.HasIndex(x => new { x.Status, x.PaymentDueAt });

        // Histori finansial tidak boleh ikut terhapus bersama membership atau pasien.
        entity.HasOne<MstPatientMembership>().WithMany().HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<MstPatient>().WithMany().HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Allocations).WithOne(x => x.MembershipFeeCharge)
            .HasForeignKey(x => x.MembershipFeeChargeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BilMembershipFeeAllocationConfiguration : IEntityTypeConfiguration<BilMembershipFeeAllocation>
{
    public void Configure(EntityTypeBuilder<BilMembershipFeeAllocation> entity)
    {
        entity.ToTable("BilMembershipFeeAllocation", "public", table =>
        {
            table.HasCheckConstraint("CK_BilMembershipFeeAllocation_Amount", "\"Amount\" > 0");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.MembershipFeeChargeId, x.AllocatedAt });
        entity.HasIndex(x => new { x.SettlementId, x.AllocatedAt });
        // Satu pembalikan paling banyak untuk satu alokasi asal.
        entity.HasIndex(x => x.ReversesAllocationId).IsUnique();

        entity.HasOne<BilSettlement>().WithMany().HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilMembershipFeeAllocation>().WithMany().HasForeignKey(x => x.ReversesAllocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
