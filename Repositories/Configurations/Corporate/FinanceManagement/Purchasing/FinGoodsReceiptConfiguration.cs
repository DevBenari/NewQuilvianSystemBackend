using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinGoodsReceiptConfiguration : IEntityTypeConfiguration<FinGoodsReceipt>
{
    public void Configure(EntityTypeBuilder<FinGoodsReceipt> entity)
    {
        entity.ToTable("FinGoodsReceipt", "public", table =>
        {
            table.HasCheckConstraint("CK_FinGoodsReceipt_Status", "\"Status\" IN ('RECEIVED','CANCELLED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.GRNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ReceivedDate).HasColumnType("date");
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinGoodsReceiptStatuses.Received);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DES-037: GR selalu terhadap satu PO — Restrict, bukan SetNull/Cascade.
        entity.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.GoodsReceipts)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.GRNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinGoodsReceipt_GRNumber");
        entity.HasIndex(x => x.PurchaseOrderId).HasDatabaseName("IX_FinGoodsReceipt_PurchaseOrderId");
    }
}
