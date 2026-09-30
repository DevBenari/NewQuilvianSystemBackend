using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingIdempotencyRecordConfiguration : IEntityTypeConfiguration<FinPurchasingIdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<FinPurchasingIdempotencyRecord> entity)
    {
        entity.ToTable("FinPurchasingIdempotencyRecord", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_FinPurchasingIdempotencyRecord_EntityType",
                "\"EntityType\" IN ('PurchaseOrder','GoodsReceipt','InvoiceExchange','PurchasingInvoice','SupplierReturn')");
            table.HasCheckConstraint(
                "CK_FinPurchasingIdempotencyRecord_Action",
                "\"Action\" IN ('Create','Submit','Approve','Cancel','Confirm')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EntityType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Action).HasMaxLength(20).IsRequired();
        entity.Property(x => x.ResponseBody).HasColumnType("text").IsRequired();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Baris dedup utama — satu IdempotencyKey hanya pernah punya satu hasil.
        entity.HasIndex(x => x.IdempotencyKey).IsUnique().HasDatabaseName("IX_FinPurchasingIdempotencyRecord_IdempotencyKey");
        entity.HasIndex(x => new { x.EntityType, x.EntityId }).HasDatabaseName("IX_FinPurchasingIdempotencyRecord_EntityTypeEntityId");
    }
}
