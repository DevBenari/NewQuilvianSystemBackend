using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinGoodsReceiptItemConfiguration : IEntityTypeConfiguration<FinGoodsReceiptItem>
{
    public void Configure(EntityTypeBuilder<FinGoodsReceiptItem> entity)
    {
        entity.ToTable("FinGoodsReceiptItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 2);
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.GoodsReceipt)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.PurchaseOrderItem)
            .WithMany(x => x.GoodsReceiptItems)
            .HasForeignKey(x => x.PurchaseOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.GoodsReceiptId).HasDatabaseName("IX_FinGoodsReceiptItem_GoodsReceiptId");
        entity.HasIndex(x => x.PurchaseOrderItemId).HasDatabaseName("IX_FinGoodsReceiptItem_PurchaseOrderItemId");
    }
}
