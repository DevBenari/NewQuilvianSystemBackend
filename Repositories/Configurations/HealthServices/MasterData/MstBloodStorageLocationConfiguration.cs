using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Pemetaan master lokasi penyimpanan darah.
    /// </summary>
    /// <remarks>
    /// Dua index, keduanya punya alasan operasional:
    ///
    /// 1. Index unik pada <c>StorageLocationCode</c> — penjaga terakhir ketika dua petugas
    ///    menyimpan kode yang sama pada saat hampir bersamaan.
    /// 2. Index pada <c>IsActive</c> — jalur baca terpanas master ini adalah "ambil lokasi
    ///    yang sedang aktif", yang dipanggil setiap kali petugas menyimpan atau memindahkan
    ///    kantong.
    /// </remarks>
    public class MstBloodStorageLocationConfiguration
        : IEntityTypeConfiguration<MstBloodStorageLocation>
    {
        public void Configure(EntityTypeBuilder<MstBloodStorageLocation> builder)
        {
            builder.ToTable("MstBloodStorageLocation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StorageLocationCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.StorageLocationName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.StorageLocationCode).IsUnique();
            builder.HasIndex(x => x.IsActive);
        }
    }
}
