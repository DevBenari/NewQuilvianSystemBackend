using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingSyncConflictConfiguration : IEntityTypeConfiguration<FinPurchasingSyncConflict>
{
    public void Configure(EntityTypeBuilder<FinPurchasingSyncConflict> entity)
    {
        entity.ToTable("FinPurchasingSyncConflict", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_ConflictType",
                "\"ConflictType\" IN ('GR_CANCELLED_WITH_DEPENDENTS','PO_CANCELLED_WITH_DEPENDENTS','NUMBER_COLLISION','PAYLOAD_MISMATCH','CONVERSION_UNRESOLVED')");
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_Status", "\"Status\" IN ('OPEN','RESOLVED')");
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_DependentType",
                "\"DependentDocumentType\" IS NULL OR \"DependentDocumentType\" IN ('INVOICE_EXCHANGE','PURCHASING_INVOICE','SUPPLIER_PAYABLE','PAYMENT')");
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_ResolutionCode",
                "\"ResolutionCode\" IS NULL OR \"ResolutionCode\" IN ('ADJUSTMENT','REVERSAL','SUPPLIER_RETURN','SUPPLIER_CREDIT','NO_FINANCIAL_IMPACT')");
            // FIN-VAL-312
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_Target", "\"PurchaseOrderId\" IS NOT NULL OR \"GoodsReceiptId\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_DependentId", "\"DependentDocumentType\" IS NULL OR \"DependentDocumentId\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinPurchasingSyncConflict_Resolved",
                "\"Status\" <> 'RESOLVED' OR (\"ResolutionCode\" IS NOT NULL AND \"ResolvedByUserId\" IS NOT NULL AND \"ResolvedAt\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ConflictType).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DependentDocumentType).HasMaxLength(30);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinPurchasingSyncConflictStatuses.Open);
        entity.Property(x => x.ResolutionCode).HasMaxLength(30);
        entity.Property(x => x.ResolutionNote).HasMaxLength(1000);
        entity.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.InboundMessage)
            .WithMany()
            .HasForeignKey(x => x.InboundMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.PurchaseOrder)
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.GoodsReceipt)
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIN-VAL-312: paling banyak satu konflik TERBUKA per (kiriman, jenis).
        entity.HasIndex(x => new { x.InboundMessageId, x.ConflictType })
            .IsUnique()
            .HasFilter("\"Status\" = 'OPEN' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinPurchasingSyncConflict_OpenPerMessageType");
        entity.HasIndex(x => new { x.Status, x.ConflictType })
            .HasDatabaseName("IX_FinPurchasingSyncConflict_StatusConflictType");
        entity.HasIndex(x => x.PurchaseOrderId)
            .HasFilter("\"PurchaseOrderId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchasingSyncConflict_PurchaseOrderId");
        entity.HasIndex(x => x.GoodsReceiptId)
            .HasFilter("\"GoodsReceiptId\" IS NOT NULL")
            .HasDatabaseName("IX_FinPurchasingSyncConflict_GoodsReceiptId");
    }
}
