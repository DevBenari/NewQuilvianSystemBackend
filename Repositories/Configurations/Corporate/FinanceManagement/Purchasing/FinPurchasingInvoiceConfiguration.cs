using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingInvoiceConfiguration : IEntityTypeConfiguration<FinPurchasingInvoice>
{
    public void Configure(EntityTypeBuilder<FinPurchasingInvoice> entity)
    {
        entity.ToTable("FinPurchasingInvoice", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchasingInvoice_Status",
                "\"Status\" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED')");
            table.HasCheckConstraint("CK_FinPurchasingInvoice_ApprovalTier", "\"ApprovalTier\" IN ('TIER_1','TIER_2')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SubtotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.PPNAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DownPaymentAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.OtherDeductionAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinPurchasingInvoiceStatuses.Draft);
        entity.Property(x => x.ApprovalTier).HasMaxLength(10).IsRequired();
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DEC-051: tepat satu Purchasing Invoice per Tukar Faktur.
        entity.HasOne(x => x.InvoiceExchange)
            .WithOne(x => x.PurchasingInvoice)
            .HasForeignKey<FinPurchasingInvoice>(x => x.InvoiceExchangeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.InvoiceNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinPurchasingInvoice_InvoiceNumber");
        entity.HasIndex(x => x.InvoiceExchangeId).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinPurchasingInvoice_InvoiceExchangeId");
        entity.HasIndex(x => x.SupplierId).HasDatabaseName("IX_FinPurchasingInvoice_SupplierId");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinPurchasingInvoice_Status");
    }
}
