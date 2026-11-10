using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingInboundMessageConfiguration : IEntityTypeConfiguration<FinPurchasingInboundMessage>
{
    public void Configure(EntityTypeBuilder<FinPurchasingInboundMessage> entity)
    {
        entity.ToTable("FinPurchasingInboundMessage", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchasingInboundMessage_SourceSystem", "\"SourceSystem\" IN ('BLM')");
            table.HasCheckConstraint("CK_FinPurchasingInboundMessage_DocumentType", "\"DocumentType\" IN ('PURCHASE_ORDER','GOODS_RECEIPT')");
            table.HasCheckConstraint("CK_FinPurchasingInboundMessage_ProcessingStatus",
                "\"ProcessingStatus\" IN ('RECEIVED','APPLIED','WAITING_MAPPING','CONFLICT','REJECTED','SUPERSEDED')");
            table.HasCheckConstraint("CK_FinPurchasingInboundMessage_AttemptCount", "\"AttemptCount\" >= 0");
            // FIN-VAL-309: APPLIED => tepat satu sasaran terisi, sesuai DocumentType.
            // EventType sengaja TANPA check constraint (kontrak BLM belum pasti, FIN-OQ-141).
            table.HasCheckConstraint("CK_FinPurchasingInboundMessage_Applied",
                "\"ProcessingStatus\" <> 'APPLIED' OR " +
                "(\"DocumentType\" = 'PURCHASE_ORDER' AND \"AppliedPurchaseOrderId\" IS NOT NULL AND \"AppliedGoodsReceiptId\" IS NULL) OR " +
                "(\"DocumentType\" = 'GOODS_RECEIPT' AND \"AppliedGoodsReceiptId\" IS NOT NULL AND \"AppliedPurchaseOrderId\" IS NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SourceSystem).HasMaxLength(20).IsRequired().HasDefaultValue(FinPurchasingSourceSystems.Blm);
        entity.Property(x => x.EventId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DocumentType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.ExternalDocumentNumber).HasMaxLength(50);
        entity.Property(x => x.SourceVersion).HasMaxLength(50);
        entity.Property(x => x.SourceOccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.ProcessingStatus).HasMaxLength(30).IsRequired().HasDefaultValue(FinPurchasingInboundProcessingStatuses.Received);
        entity.Property(x => x.AttemptCount).HasDefaultValue(0);
        entity.Property(x => x.LastAttemptAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.NextAttemptAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.LastErrorCode).HasMaxLength(50);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.AppliedPurchaseOrder)
            .WithMany()
            .HasForeignKey(x => x.AppliedPurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.AppliedGoodsReceipt)
            .WithMany()
            .HasForeignKey(x => x.AppliedGoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIN-VAL-308: idempotensi. Tanpa filter IsDelete: nomor kejadian yang pernah diterima tidak boleh masuk lagi.
        entity.HasIndex(x => new { x.SourceSystem, x.EventId })
            .IsUnique()
            .HasDatabaseName("IX_FinPurchasingInboundMessage_SourceSystem_EventId");
        entity.HasIndex(x => new { x.DocumentType, x.ExternalDocumentId, x.SourceOccurredAt })
            .HasDatabaseName("IX_FinPurchasingInboundMessage_Document");
        entity.HasIndex(x => new { x.ProcessingStatus, x.NextAttemptAt })
            .HasDatabaseName("IX_FinPurchasingInboundMessage_ProcessingStatus");
        entity.HasIndex(x => x.AppliedPurchaseOrderId)
            .HasFilter("\"AppliedPurchaseOrderId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchasingInboundMessage_AppliedPurchaseOrderId");
        entity.HasIndex(x => x.AppliedGoodsReceiptId)
            .HasFilter("\"AppliedGoodsReceiptId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchasingInboundMessage_AppliedGoodsReceiptId");
    }
}
