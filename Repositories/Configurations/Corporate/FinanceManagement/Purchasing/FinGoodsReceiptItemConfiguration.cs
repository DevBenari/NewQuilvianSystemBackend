using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinGoodsReceiptItemConfiguration : IEntityTypeConfiguration<FinGoodsReceiptItem>
{
    public void Configure(EntityTypeBuilder<FinGoodsReceiptItem> entity)
    {
        entity.ToTable("FinGoodsReceiptItem", "public", table =>
        {
            // FIN-VAL-306: kedelapan kolom BLM wajib semuanya kosong (baris lama) atau semuanya terisi (baris BLM).
            table.HasCheckConstraint("CK_FinGoodsReceiptItem_BlmFields",
                "(\"ExternalReceiveOrderItemId\" IS NULL AND \"ConditionCode\" IS NULL AND \"SourceQuantity\" IS NULL AND \"SourceUomCode\" IS NULL AND \"QuantityPerBaseUnit\" IS NULL AND \"BaseUomCode\" IS NULL AND \"BaseQuantity\" IS NULL AND \"AcceptedBaseQuantity\" IS NULL) OR " +
                "(\"ExternalReceiveOrderItemId\" IS NOT NULL AND \"ConditionCode\" IS NOT NULL AND \"SourceQuantity\" IS NOT NULL AND \"SourceUomCode\" IS NOT NULL AND \"QuantityPerBaseUnit\" IS NOT NULL AND \"BaseUomCode\" IS NOT NULL AND \"BaseQuantity\" IS NOT NULL AND \"AcceptedBaseQuantity\" IS NOT NULL)");
            // FIN-VAL-304
            table.HasCheckConstraint("CK_FinGoodsReceiptItem_ConditionCode",
                "\"ConditionCode\" IS NULL OR \"ConditionCode\" IN ('GOOD','DAMAGED','PENDING_INSPECTION')");
            // FIN-DEC-276: isi per satuan harus positif; tidak ada nilai bawaan 1.
            table.HasCheckConstraint("CK_FinGoodsReceiptItem_QuantityPerBaseUnit",
                "\"QuantityPerBaseUnit\" IS NULL OR \"QuantityPerBaseUnit\" > 0");
            // FIN-VAL-305: sah tidak boleh melebihi fisik, dan nol bila kondisi bukan GOOD.
            table.HasCheckConstraint("CK_FinGoodsReceiptItem_AcceptedBaseQuantity",
                "\"AcceptedBaseQuantity\" IS NULL OR (\"AcceptedBaseQuantity\" >= 0 AND \"AcceptedBaseQuantity\" <= \"BaseQuantity\" AND (\"ConditionCode\" = 'GOOD' OR \"AcceptedBaseQuantity\" = 0))");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 2);
        entity.Property(x => x.BatchNumber).HasMaxLength(100);
        entity.Property(x => x.ExpiredDate).HasColumnType("date");
        entity.Property(x => x.Notes).HasMaxLength(500);

        // Penampung data BLM (BE-FIN-142). Kuantitas satuan dasar numeric(18,6): tanpa pembulatan dini (FIN-DEC-276).
        // ReceivedQuantity di atas tetap numeric(18,2) dan artinya tidak berubah (FIN-DES-140).
        entity.Property(x => x.ConditionCode).HasMaxLength(30);
        entity.Property(x => x.SourceQuantity).HasPrecision(18, 6);
        entity.Property(x => x.SourceUomCode).HasMaxLength(30);
        entity.Property(x => x.QuantityPerBaseUnit).HasPrecision(18, 6);
        entity.Property(x => x.BaseUomCode).HasMaxLength(30);
        entity.Property(x => x.BaseQuantity).HasPrecision(18, 6);
        entity.Property(x => x.AcceptedBaseQuantity).HasPrecision(18, 6);

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
        entity.HasIndex(x => x.ExternalReceiveOrderItemId)
            .IsUnique()
            .HasFilter("\"ExternalReceiveOrderItemId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinGoodsReceiptItem_ExternalReceiveOrderItemId");
    }
}
