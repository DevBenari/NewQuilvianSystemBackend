using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

/// <summary>
/// P04 — NOT ACTIVATED (QBE-CFG-001). Lihat <see cref="FinPurchasingInvoiceItemAllocation"/> dan
/// <see cref="FinInvoiceExchangeGoodsReceiptConfiguration"/> untuk syarat aktivasi dan catatan migration.
/// </summary>
public sealed class FinPurchasingInvoiceItemAllocationConfiguration : IEntityTypeConfiguration<FinPurchasingInvoiceItemAllocation>
{
    public void Configure(EntityTypeBuilder<FinPurchasingInvoiceItemAllocation> entity)
    {
        entity.ToTable("FinPurchasingInvoiceItemAllocation", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchasingInvoiceItemAllocation_Quantity", "\"AllocatedQuantity\" > 0");
            table.HasCheckConstraint("CK_FinPurchasingInvoiceItemAllocation_Amount", "\"AllocatedAmount\" >= 0");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AllocatedQuantity).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.PurchasingInvoiceItem)
            .WithMany()
            .HasForeignKey(x => x.PurchasingInvoiceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.GoodsReceiptItem)
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptItemId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.PurchasingInvoiceItemId, x.GoodsReceiptItemId }).IsUnique().HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinPurchasingInvoiceItemAllocation_InvoiceItem_GrItem");
        entity.HasIndex(x => x.GoodsReceiptItemId).HasDatabaseName("IX_FinPurchasingInvoiceItemAllocation_GoodsReceiptItemId");
    }
}
