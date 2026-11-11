using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchaseOrderItemConfiguration : IEntityTypeConfiguration<FinPurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<FinPurchaseOrderItem> entity)
    {
        entity.ToTable("FinPurchaseOrderItem", "public", table =>
        {
            // FIN-VAL-306: keenam kolom BLM wajib semuanya kosong (item lama) atau semuanya terisi (item BLM).
            table.HasCheckConstraint("CK_FinPurchaseOrderItem_BlmFields",
                "(\"ExternalPurchaseOrderItemId\" IS NULL AND \"ExternalProductId\" IS NULL AND \"SourceUomCode\" IS NULL AND \"QuantityPerBaseUnit\" IS NULL AND \"BaseUomCode\" IS NULL AND \"BaseQuantity\" IS NULL) OR " +
                "(\"ExternalPurchaseOrderItemId\" IS NOT NULL AND \"ExternalProductId\" IS NOT NULL AND \"SourceUomCode\" IS NOT NULL AND \"QuantityPerBaseUnit\" IS NOT NULL AND \"BaseUomCode\" IS NOT NULL AND \"BaseQuantity\" IS NOT NULL)");
            // FIN-DEC-276: isi per satuan harus positif; tidak ada nilai bawaan 1.
            table.HasCheckConstraint("CK_FinPurchaseOrderItem_QuantityPerBaseUnit",
                "\"QuantityPerBaseUnit\" IS NULL OR \"QuantityPerBaseUnit\" > 0");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ProductCategory).HasMaxLength(100).IsRequired();
        entity.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Unit).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Quantity).HasPrecision(18, 2);
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        entity.Property(x => x.LineTotal).HasPrecision(18, 2);

        // Penampung data BLM (BE-FIN-142). Kuantitas satuan dasar numeric(18,6): tanpa pembulatan dini (FIN-DEC-276).
        entity.Property(x => x.ExternalProductCode).HasMaxLength(100);
        entity.Property(x => x.SourceUomCode).HasMaxLength(30);
        entity.Property(x => x.QuantityPerBaseUnit).HasPrecision(18, 6);
        entity.Property(x => x.BaseUomCode).HasMaxLength(30);
        entity.Property(x => x.BaseQuantity).HasPrecision(18, 6);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PurchaseOrderId).HasDatabaseName("IX_FinPurchaseOrderItem_PurchaseOrderId");
        entity.HasIndex(x => x.ExternalPurchaseOrderItemId)
            .IsUnique()
            .HasFilter("\"ExternalPurchaseOrderItemId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinPurchaseOrderItem_ExternalPurchaseOrderItemId");
        entity.HasIndex(x => x.ExternalProductId)
            .HasFilter("\"ExternalProductId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchaseOrderItem_ExternalProductId");
    }
}
