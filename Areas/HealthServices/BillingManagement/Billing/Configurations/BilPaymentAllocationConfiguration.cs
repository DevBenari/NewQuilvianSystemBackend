using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilPaymentAllocationConfiguration : IEntityTypeConfiguration<BilPaymentAllocation>
{
    public void Configure(EntityTypeBuilder<BilPaymentAllocation> entity)
    {
        entity.ToTable("BilPaymentAllocation", "public", table =>
        {
            table.HasCheckConstraint("CK_BilPaymentAllocation_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_BilPaymentAllocation_TargetType", "\"TargetType\" = 'INVOICE'");
            table.HasCheckConstraint(
                "CK_BilPaymentAllocation_CalculationVersion",
                "\"CalculationVersion\" IS NULL OR \"CalculationVersion\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TargetType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => new { x.SettlementId, x.AllocatedAt });
        entity.HasIndex(x => new { x.TargetType, x.TargetId, x.AllocatedAt });
        entity.HasIndex(x => x.ReversesAllocationId).IsUnique();
        entity.HasOne(x => x.Settlement).WithMany(x => x.Allocations)
            .HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilInvoice>().WithMany().HasForeignKey(x => x.TargetId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilPaymentAllocation>().WithMany().HasForeignKey(x => x.ReversesAllocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
