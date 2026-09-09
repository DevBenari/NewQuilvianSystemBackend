using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class PhmDrugBatchConfiguration : IEntityTypeConfiguration<PhmDrugBatch>
{
    public void Configure(EntityTypeBuilder<PhmDrugBatch> builder)
    {
        builder.ToTable("PhmDrugBatch", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PrincipalName).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);

        // Satu nomor batch pada satu obat hanya boleh ada sekali. Aturan ini sekaligus
        // memastikan satu batch tidak dapat memiliki dua tanggal kedaluwarsa yang berbeda.
        // Nomor batch yang sama pada obat berbeda tetap diizinkan, karena penomoran itu
        // milik pabrik masing-masing.
        builder.HasIndex(x => new { x.DrugId, x.BatchNumber })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        // Pemantauan mendekati kedaluwarsa dan penarikan obat keduanya menyapu menurut
        // tanggal, bukan menurut obat.
        builder.HasIndex(x => x.ExpiryDate);
        builder.HasIndex(x => x.BatchNumber);

        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Supplier).WithMany()
            .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PhmDrugStockBalanceConfiguration : IEntityTypeConfiguration<PhmDrugStockBalance>
{
    public void Configure(EntityTypeBuilder<PhmDrugStockBalance> builder)
    {
        builder.ToTable("PhmDrugStockBalance", "public", table =>
        {
            // Aturan "stok tidak boleh negatif" ditegakkan layanan, tetapi tetap dipasang di
            // basis data sebagai penjaga terakhir. Layanan dapat keliru atau dilewati skrip;
            // saldo minus yang terlanjur tersimpan jauh lebih mahal daripada satu transaksi
            // yang gagal.
            table.HasCheckConstraint("CK_PhmDrugStockBalance_OnHandNotNegative",
                "\"QuantityOnHand\" >= 0");

            // Menahan lebih banyak daripada yang ada di rak berarti dua proses menghitung
            // barang yang sama sebagai miliknya.
            table.HasCheckConstraint("CK_PhmDrugStockBalance_ReservedWithinOnHand",
                "\"QuantityReserved\" >= 0 AND \"QuantityReserved\" <= \"QuantityOnHand\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuantityOnHand).HasColumnType("numeric(18,3)");
        builder.Property(x => x.QuantityReserved).HasColumnType("numeric(18,3)");
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.Ignore(x => x.QuantityAvailable);

        // Satu baris saldo untuk setiap batch, lokasi, dan status. Tanpa keunikan ini, dua
        // baris untuk kombinasi yang sama akan membuat total stok terbaca ganda.
        builder.HasIndex(x => new { x.DrugBatchId, x.StorageLocationId, x.Status })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        // Pertanyaan yang paling sering diajukan: berapa stok obat ini di lokasi ini.
        builder.HasIndex(x => new { x.DrugId, x.StorageLocationId, x.Status });
        builder.HasIndex(x => x.StorageLocationId);

        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DrugBatch).WithMany(x => x.Balances)
            .HasForeignKey(x => x.DrugBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StorageLocation).WithMany()
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PhmDrugStockMutationConfiguration : IEntityTypeConfiguration<PhmDrugStockMutation>
{
    public void Configure(EntityTypeBuilder<PhmDrugStockMutation> builder)
    {
        builder.ToTable("PhmDrugStockMutation", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuantityChange).HasColumnType("numeric(18,3)");
        builder.Property(x => x.BalanceBefore).HasColumnType("numeric(18,3)");
        builder.Property(x => x.BalanceAfter).HasColumnType("numeric(18,3)");
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(50);
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        // Kartu stok dibaca sebagai lini masa satu batch di satu lokasi.
        builder.HasIndex(x => new { x.DrugBatchId, x.StorageLocationId, x.OccurredAt });
        builder.HasIndex(x => new { x.DrugId, x.StorageLocationId, x.OccurredAt });
        builder.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId });

        // Perintah yang sama tidak boleh menghasilkan dua pergerakan. Penyaring memastikan
        // baris tanpa kunci korelasi tidak saling bertabrakan.
        builder.HasIndex(x => new { x.SourceDocumentType, x.CorrelationId })
            .IsUnique()
            .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DrugBatch).WithMany()
            .HasForeignKey(x => x.DrugBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StorageLocation).WithMany()
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CorrectionOfMutation).WithMany()
            .HasForeignKey(x => x.CorrectionOfMutationId).OnDelete(DeleteBehavior.Restrict);
    }
}
