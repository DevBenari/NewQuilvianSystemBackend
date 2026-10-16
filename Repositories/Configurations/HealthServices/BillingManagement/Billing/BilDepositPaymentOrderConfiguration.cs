using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilDepositPaymentOrderConfiguration : IEntityTypeConfiguration<BilDepositPaymentOrder>
{
    public void Configure(EntityTypeBuilder<BilDepositPaymentOrder> entity)
    {
        entity.ToTable("BilDepositPaymentOrder", "public", table =>
        {
            table.HasCheckConstraint("CK_BilDepositPaymentOrder_RequestedAmount", "\"RequestedAmount\" > 0");
            table.HasCheckConstraint("CK_BilDepositPaymentOrder_PaidAmount", "\"PaidAmount\" >= 0");
            table.HasCheckConstraint("CK_BilDepositPaymentOrder_OutstandingAmount", "\"OutstandingAmount\" >= 0");
            table.HasCheckConstraint("CK_BilDepositPaymentOrder_Status", "\"Status\" IN ('UNPAID','PARTIALLY_PAID','PAID','CANCELLED','EXPIRED')");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.RequestedAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.OrderType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Remarks).HasMaxLength(500);
        entity.Property(x => x.CancellationReason).HasMaxLength(500);
        entity.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.OrderNumber).IsUnique();
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => x.EpisodeId);
        entity.HasIndex(x => x.EncounterId);
        entity.HasIndex(x => x.PatientId);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => new { x.EpisodeId, x.OrderType });

        entity.HasMany(x => x.Payments).WithOne(x => x.DepositOrder)
            .HasForeignKey(x => x.DepositOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
