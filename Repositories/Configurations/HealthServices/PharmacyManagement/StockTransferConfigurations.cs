using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class TrxStockTransferConfiguration : IEntityTypeConfiguration<TrxStockTransfer>
{
    public void Configure(EntityTypeBuilder<TrxStockTransfer> builder)
    {
        builder.ToTable("TrxStockTransfer", "public", table =>
        {
            // Memindahkan barang ke lokasi yang sama dengan asalnya bukan perpindahan apa pun,
            // tetapi tetap akan menulis dua baris kartu stok yang saling meniadakan dan
            // mengaburkan pembacaan.
            table.HasCheckConstraint("CK_TrxStockTransfer_SourceNotDestination",
                "\"SourceStorageLocationId\" <> \"DestinationStorageLocationId\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransferNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.DecisionReason).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.TransferNumber).IsUnique();
        builder.HasIndex(x => new { x.SourceStorageLocationId, x.Status, x.RequestedAt });
        builder.HasIndex(x => new { x.DestinationStorageLocationId, x.Status, x.RequestedAt });
        builder.HasIndex(x => x.RequestedAt);

        builder.HasOne(x => x.SourceStorageLocation).WithMany()
            .HasForeignKey(x => x.SourceStorageLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DestinationStorageLocation).WithMany()
            .HasForeignKey(x => x.DestinationStorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxStockTransferItemConfiguration : IEntityTypeConfiguration<TrxStockTransferItem>
{
    public void Configure(EntityTypeBuilder<TrxStockTransferItem> builder)
    {
        builder.ToTable("TrxStockTransferItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DrugCodeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.RequestedQuantity).HasColumnType("numeric(18,3)");
        builder.Property(x => x.IssuedQuantity).HasColumnType("numeric(18,3)");
        builder.Property(x => x.ReceivedQuantity).HasColumnType("numeric(18,3)");

        // Satu obat hanya boleh muncul sekali dalam satu transfer. Dua baris obat yang sama
        // membuat penyiapan dikerjakan dua kali untuk satu kebutuhan.
        builder.HasIndex(x => new { x.StockTransferId, x.DrugId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        builder.HasOne(x => x.StockTransfer).WithMany(x => x.Items)
            .HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxStockTransferAllocationConfiguration
    : IEntityTypeConfiguration<TrxStockTransferAllocation>
{
    public void Configure(EntityTypeBuilder<TrxStockTransferAllocation> builder)
    {
        builder.ToTable("TrxStockTransferAllocation", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasColumnType("numeric(18,3)");

        builder.HasIndex(x => new { x.StockTransferItemId, x.SequenceNumber });
        builder.HasIndex(x => x.DrugBatchId);

        builder.HasOne(x => x.StockTransferItem).WithMany(x => x.Allocations)
            .HasForeignKey(x => x.StockTransferItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.DrugBatch).WithMany()
            .HasForeignKey(x => x.DrugBatchId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxStockTransferHistoryConfiguration
    : IEntityTypeConfiguration<TrxStockTransferHistory>
{
    public void Configure(EntityTypeBuilder<TrxStockTransferHistory> builder)
    {
        builder.ToTable("TrxStockTransferHistory", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.HasIndex(x => new { x.StockTransferId, x.OccurredAt });

        // Perintah yang sama tidak boleh dijalankan dua kali.
        builder.HasIndex(x => new { x.Action, x.CorrelationId })
            .IsUnique()
            .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasOne(x => x.StockTransfer).WithMany(x => x.Histories)
            .HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
    }
}
