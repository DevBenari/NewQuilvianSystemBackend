using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilDepositMovementConfiguration : IEntityTypeConfiguration<BilDepositMovement>
{
    public void Configure(EntityTypeBuilder<BilDepositMovement> entity)
    {
        entity.ToTable("BilDepositMovement", "public", table =>
        {
            table.HasCheckConstraint("CK_BilDepositMovement_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_BilDepositMovement_Type", "\"MovementType\" IN ('TOP_UP','ALLOCATION','RELEASE','REVERSAL')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MovementType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => new { x.DepositAccountId, x.OccurredAt });
        entity.HasIndex(x => x.SettlementId);
        entity.HasIndex(x => x.CashierShiftId);
        entity.HasOne<MstPaymentMethod>().WithMany().HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilSettlement>().WithMany().HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilCashierShift>().WithMany().HasForeignKey(x => x.CashierShiftId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilDepositMovement>().WithMany().HasForeignKey(x => x.ReversesMovementId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(x => x.ReversesMovementId).IsUnique();
    }
}
