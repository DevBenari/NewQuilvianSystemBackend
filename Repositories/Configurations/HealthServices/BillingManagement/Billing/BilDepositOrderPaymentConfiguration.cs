using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilDepositOrderPaymentConfiguration : IEntityTypeConfiguration<BilDepositOrderPayment>
{
    public void Configure(EntityTypeBuilder<BilDepositOrderPayment> entity)
    {
        entity.ToTable("BilDepositOrderPayment", "public", table =>
        {
            table.HasCheckConstraint("CK_BilDepositOrderPayment_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_BilDepositOrderPayment_Status", "\"Status\" IN ('SUCCESS','REVERSED')");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.DepositOrderId);
        entity.HasIndex(x => x.DepositMovementId);
        entity.HasOne(x => x.DepositMovement).WithMany().HasForeignKey(x => x.DepositMovementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
