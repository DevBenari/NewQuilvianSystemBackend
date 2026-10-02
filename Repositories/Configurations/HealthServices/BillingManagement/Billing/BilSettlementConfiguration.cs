using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilSettlementConfiguration : IEntityTypeConfiguration<BilSettlement>
{
    public void Configure(EntityTypeBuilder<BilSettlement> entity)
    {
        entity.ToTable("BilSettlement", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilSettlement_Target",
                "((\"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL) OR (\"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL))");
            table.HasCheckConstraint(
                "CK_BilSettlement_Purpose",
                "\"Purpose\" IN ('DEPOSIT_TOP_UP','INVOICE_PAYMENT')");
            table.HasCheckConstraint(
                "CK_BilSettlement_PurposeTarget",
                "((\"Purpose\" = 'INVOICE_PAYMENT' AND \"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL) OR (\"Purpose\" = 'DEPOSIT_TOP_UP' AND \"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL))");
            table.HasCheckConstraint(
                "CK_BilSettlement_Status",
                "\"Status\" IN ('DRAFT','IN_PROGRESS','PARTIALLY_SETTLED','SETTLED','FAILED')");
            table.HasCheckConstraint(
                "CK_BilSettlement_Amounts",
                "\"RequestedAmount\" > 0 AND \"SuccessfulAmount\" >= 0 AND \"AllocatedAmount\" >= 0 AND \"AllocatedAmount\" <= \"SuccessfulAmount\" AND \"SuccessfulAmount\" <= \"RequestedAmount\"");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Purpose).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Note).HasMaxLength(500);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.RequestedAmount).HasPrecision(18, 2);
        entity.Property(x => x.SuccessfulAmount).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => new { x.InvoiceId, x.Status });
        entity.HasIndex(x => new { x.DepositAccountId, x.Status });
        entity.HasOne<BilInvoice>().WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<BilDepositAccount>().WithMany().HasForeignKey(x => x.DepositAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Tenders).WithOne(x => x.Settlement)
            .HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
    }
}
