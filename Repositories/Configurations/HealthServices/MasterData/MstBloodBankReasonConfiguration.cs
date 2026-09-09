using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Pemetaan daftar alasan terkendali Bank Darah.
    /// </summary>
    /// <remarks>
    /// Dua index, keduanya punya alasan operasional:
    ///
    /// 1. Index unik pada <c>ReasonCode</c> — penjaga terakhir ketika dua petugas menyimpan kode
    ///    yang sama pada saat hampir bersamaan.
    /// 2. Index gabungan <c>ReasonCategory</c> + <c>IsActive</c> — jalur baca terpanas daftar ini
    ///    adalah "ambil alasan aktif untuk kategori tertentu", yang dipanggil setiap kali petugas
    ///    membuka kotak pilihan alasan pada layar pembatalan, pengalihan, atau jalur darurat.
    /// </remarks>
    public class MstBloodBankReasonConfiguration : IEntityTypeConfiguration<MstBloodBankReason>
    {
        public void Configure(EntityTypeBuilder<MstBloodBankReason> builder)
        {
            builder.ToTable("MstBloodBankReason", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReasonCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ReasonText).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ReasonCategory).HasMaxLength(40).IsRequired();

            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.ReasonCode).IsUnique();
            builder.HasIndex(x => new { x.ReasonCategory, x.IsActive });
        }
    }
}
