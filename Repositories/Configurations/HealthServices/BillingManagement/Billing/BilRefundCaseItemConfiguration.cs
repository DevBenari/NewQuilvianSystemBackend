using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilRefundCaseItemConfiguration : IEntityTypeConfiguration<BilRefundCaseItem>
{
    public void Configure(EntityTypeBuilder<BilRefundCaseItem> entity)
    {
        entity.ToTable("BilRefundCaseItem", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilRefundCaseItem_Amount",
                "\"Amount\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        // Satu case menyebut satu baris paling banyak sekali.
        entity.HasIndex(x => new { x.RefundCaseId, x.InvoiceItemId }).IsUnique();
        // Akumulasi per baris dibaca lewat baris tagihan.
        entity.HasIndex(x => x.InvoiceItemId);
        entity.HasOne(x => x.RefundCase).WithMany(x => x.Items).HasForeignKey(x => x.RefundCaseId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.InvoiceItem).WithMany().HasForeignKey(x => x.InvoiceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
