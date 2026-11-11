using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingExternalCancellationConfiguration : IEntityTypeConfiguration<FinPurchasingExternalCancellation>
{
    public void Configure(EntityTypeBuilder<FinPurchasingExternalCancellation> entity)
    {
        entity.ToTable("FinPurchasingExternalCancellation", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_SourceSystem", "\"SourceSystem\" IN ('BLM')");
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_Direction", "\"Direction\" IN ('OUTBOUND_REQUEST','INBOUND_NOTICE')");
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_DocumentType", "\"DocumentType\" IN ('PURCHASE_ORDER','GOODS_RECEIPT')");
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_Status",
                "\"Status\" IN ('REQUESTED','SENT','CONFIRMED','REJECTED','FAILED')");
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_AttemptCount", "\"AttemptCount\" >= 0");
            // FIN-VAL-313: FK lokal sesuai jenis dokumen.
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_Target",
                "(\"DocumentType\" = 'PURCHASE_ORDER' AND \"PurchaseOrderId\" IS NOT NULL) OR (\"DocumentType\" = 'GOODS_RECEIPT' AND \"GoodsReceiptId\" IS NOT NULL)");
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_Requester",
                "\"Direction\" <> 'OUTBOUND_REQUEST' OR \"RequestedByUserId\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinPurchasingExternalCancellation_Confirmed",
                "\"Status\" <> 'CONFIRMED' OR \"CancelledAt\" IS NOT NULL");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SourceSystem).HasMaxLength(20).IsRequired().HasDefaultValue(FinPurchasingSourceSystems.Blm);
        entity.Property(x => x.Direction).HasMaxLength(20).IsRequired();
        entity.Property(x => x.DocumentType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.EventId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.CategoryCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.OtherCategoryText).HasMaxLength(300);
        entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.CancelledBy).HasMaxLength(100);
        entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
        entity.Property(x => x.RejectionCode).HasMaxLength(50);
        entity.Property(x => x.AttemptCount).HasDefaultValue(0);
        entity.Property(x => x.NextAttemptAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.PurchaseOrder)
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.GoodsReceipt)
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.InboundMessage)
            .WithMany()
            .HasForeignKey(x => x.InboundMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIN-VAL-313: idempotensi permintaan dan pemberitahuan. Tanpa filter IsDelete.
        entity.HasIndex(x => new { x.SourceSystem, x.Direction, x.EventId })
            .IsUnique()
            .HasDatabaseName("IX_FinPurchasingExternalCancellation_SourceDirectionEvent");
        entity.HasIndex(x => new { x.DocumentType, x.ExternalDocumentId })
            .HasDatabaseName("IX_FinPurchasingExternalCancellation_Document");
        entity.HasIndex(x => new { x.Status, x.NextAttemptAt })
            .HasDatabaseName("IX_FinPurchasingExternalCancellation_Status");
        entity.HasIndex(x => x.PurchaseOrderId)
            .HasFilter("\"PurchaseOrderId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchasingExternalCancellation_PurchaseOrderId");
        entity.HasIndex(x => x.GoodsReceiptId)
            .HasFilter("\"GoodsReceiptId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchasingExternalCancellation_GoodsReceiptId");
    }
}
