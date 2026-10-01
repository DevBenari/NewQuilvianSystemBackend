using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableInvoiceBatchItemConfiguration : IEntityTypeConfiguration<FinReceivableInvoiceBatchItem>
{
    public void Configure(EntityTypeBuilder<FinReceivableInvoiceBatchItem> entity)
    {
        entity.ToTable("FinReceivableInvoiceBatchItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Batch)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Receivable)
            .WithMany()
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.BatchId).HasDatabaseName("IX_FinReceivableInvoiceBatchItem_BatchId");

        // erd/data-dictionary.md §C.14/§C.16: satu FinReceivable hanya boleh aktif di satu batch.
        // Filter murni IsDelete=false (bukan Status) — pembebasan piutang saat batch CANCELLED
        // adalah keputusan service yang menyusul, bukan cakupan skema ini.
        entity.HasIndex(x => x.ReceivableId)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinReceivableInvoiceBatchItem_ActiveReceivable");
    }
}
