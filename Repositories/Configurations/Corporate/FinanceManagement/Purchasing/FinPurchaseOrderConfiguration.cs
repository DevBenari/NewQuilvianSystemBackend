using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchaseOrderConfiguration : IEntityTypeConfiguration<FinPurchaseOrder>
{
    public void Configure(EntityTypeBuilder<FinPurchaseOrder> entity)
    {
        entity.ToTable("FinPurchaseOrder", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchaseOrder_Status",
                "\"Status\" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED','PARTIALLY_RECEIVED','FULLY_RECEIVED','CLOSED')");
            table.HasCheckConstraint("CK_FinPurchaseOrder_ApprovalTier", "\"ApprovalTier\" IN ('TIER_1','TIER_2')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PONumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinPurchaseOrderStatuses.Draft);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.ApprovalTier).HasMaxLength(10).IsRequired();
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DEC-014: SupplierId merujuk MstSupplier existing milik Administrator.
        entity.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PONumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinPurchaseOrder_PONumber");
        entity.HasIndex(x => x.SupplierId).HasDatabaseName("IX_FinPurchaseOrder_SupplierId");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinPurchaseOrder_Status");
    }
}
