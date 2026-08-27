using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilTenderConfiguration : IEntityTypeConfiguration<BilTender>
{
    public void Configure(EntityTypeBuilder<BilTender> entity)
    {
        entity.ToTable("BilTender", "public", table =>
        {
            table.HasCheckConstraint("CK_BilTender_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint(
                "CK_BilTender_Status",
                "\"Status\" IN ('CREATED','PENDING','SUCCEEDED','FAILED','EXPIRED','REVERSED')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.ProviderReference).HasMaxLength(150);
        entity.Property(x => x.ProviderStatusCode).HasMaxLength(50);
        entity.Property(x => x.CashierReferenceNote).HasMaxLength(150);
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.LastProviderEventId).HasMaxLength(100);
        entity.Property(x => x.LastProviderPayloadHash).HasMaxLength(64);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.AttemptedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.SettledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ProviderOccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => x.ProviderReference).IsUnique()
            .HasFilter("\"ProviderReference\" IS NOT NULL");
        entity.HasIndex(x => new { x.SettlementId, x.Status });
        entity.HasIndex(x => x.CashierShiftId);
        entity.HasOne<MstPaymentMethod>().WithMany().HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilCashierShift>().WithMany().HasForeignKey(x => x.CashierShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
