using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinInvoiceExchangeConfiguration : IEntityTypeConfiguration<FinInvoiceExchange>
{
    public void Configure(EntityTypeBuilder<FinInvoiceExchange> entity)
    {
        entity.ToTable("FinInvoiceExchange", "public", table =>
        {
            table.HasCheckConstraint("CK_FinInvoiceExchange_Status", "\"Status\" IN ('RECEIVED','LINKED_TO_INVOICE','CANCELLED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ExchangeNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SupplierInvoiceNumber).HasMaxLength(100).IsRequired();
        entity.Property(x => x.SupplierInvoiceDate).HasColumnType("date");
        entity.Property(x => x.ReceivedDate).HasColumnType("date");
        entity.Property(x => x.EstimatedDueDate).HasColumnType("date");
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinInvoiceExchangeStatuses.Received);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIN-DEC-051: boleh tanpa PO/GR — SetNull, bukan Restrict/Cascade.
        entity.HasOne(x => x.PurchaseOrder)
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(x => x.GoodsReceipt)
            .WithMany(x => x.InvoiceExchanges)
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => x.ExchangeNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinInvoiceExchange_ExchangeNumber");
        entity.HasIndex(x => x.SupplierId).HasDatabaseName("IX_FinInvoiceExchange_SupplierId");
        entity.HasIndex(x => x.PurchaseOrderId).HasDatabaseName("IX_FinInvoiceExchange_PurchaseOrderId");
        entity.HasIndex(x => x.GoodsReceiptId).HasDatabaseName("IX_FinInvoiceExchange_GoodsReceiptId");
        entity.HasIndex(x => x.EstimatedDueDate).HasDatabaseName("IX_FinInvoiceExchange_EstimatedDueDate");
    }
}
