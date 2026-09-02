using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class TrxStockRequestConfiguration : IEntityTypeConfiguration<TrxStockRequest>
{
    public void Configure(EntityTypeBuilder<TrxStockRequest> builder)
    {
        builder.ToTable("TrxStockRequest", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.DecisionReason).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.RequestNumber).IsUnique();

        // Riwayat permintaan hampir selalu dibuka dengan saringan unit dan status, lalu
        // diurutkan menurut waktu. Indeks ini mengikuti bentuk pertanyaan itu.
        builder.HasIndex(x => new { x.RequestingServiceUnitId, x.Status, x.RequestedAt });
        builder.HasIndex(x => new { x.StorageLocationId, x.Status });
        builder.HasIndex(x => x.RequestedAt);

        builder.HasOne(x => x.RequestingServiceUnit).WithMany()
            .HasForeignKey(x => x.RequestingServiceUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StorageLocation).WithMany()
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequestedByWorkforce).WithMany()
            .HasForeignKey(x => x.RequestedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxStockRequestItemConfiguration : IEntityTypeConfiguration<TrxStockRequestItem>
{
    public void Configure(EntityTypeBuilder<TrxStockRequestItem> builder)
    {
        builder.ToTable("TrxStockRequestItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DrugCodeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MeasurementNameSnapshot).HasMaxLength(50);
        builder.Property(x => x.Note).HasMaxLength(500);

        // Satu obat hanya boleh muncul sekali pada satu permintaan. Tanpa ini, dua baris
        // obat yang sama membuat gudang menyiapkan dua kali untuk kebutuhan yang satu.
        builder.HasIndex(x => new { x.StockRequestId, x.DrugId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        builder.HasOne(x => x.StockRequest).WithMany(x => x.Items)
            .HasForeignKey(x => x.StockRequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Measurement).WithMany()
            .HasForeignKey(x => x.MeasurementId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxStockRequestHistoryConfiguration : IEntityTypeConfiguration<TrxStockRequestHistory>
{
    public void Configure(EntityTypeBuilder<TrxStockRequestHistory> builder)
    {
        builder.ToTable("TrxStockRequestHistory", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.HasIndex(x => new { x.StockRequestId, x.OccurredAt });

        // Kunci idempotensi: satu kunci hanya berlaku sekali per jenis aksi, sehingga
        // permintaan yang terkirim dua kali tidak menghasilkan dua tindakan.
        builder.HasIndex(x => new { x.Action, x.CorrelationId })
            .IsUnique()
            .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasOne(x => x.StockRequest).WithMany(x => x.Histories)
            .HasForeignKey(x => x.StockRequestId).OnDelete(DeleteBehavior.Restrict);
    }
}
