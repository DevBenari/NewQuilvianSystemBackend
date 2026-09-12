using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Platform.NumberSeriesManagement
{
    /// <summary>
    /// Pemetaan pencacah deret nomor bersama.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Index unik <c>(SequenceKey, ScopeKey)</c> adalah penjaga terakhir modul ini.</b> Kunci
    /// penasihat di dalam transaksi alokasi menyerialkan urutan pengambilan nomor, tetapi kunci
    /// itu hidup di dalam aplikasi. Index ini hidup di database, dan ia yang menahan ketika ada
    /// jalur yang lupa mengunci — termasuk jalur yang belum ditulis hari ini.
    /// </para>
    ///
    /// <para>
    /// <b>Dua check constraint, keduanya menjaga hal yang berbeda.</b> <c>CurrentValue &gt; 0</c>
    /// menahan baris yang mengaku ada tetapi belum pernah menerbitkan apa pun.
    /// <c>ResetPolicy IN (...)</c> menahan kebijakan di luar keempat nilai sah — nilai asing di
    /// sana membuat periode pencacah tidak dapat dihitung, dan alokasi berikutnya akan memilih
    /// baris yang salah.
    /// </para>
    ///
    /// <para>
    /// <b>Nol relasi keluar, dan itu disengaja.</b> Pencacah tidak menunjuk modul, pengguna,
    /// maupun catatan mana pun. Menambahkan foreign key ke modul pemakai akan membuat platform
    /// mengenal konsumennya satu per satu — persis kebalikan dari pembagian kewenangan
    /// <c>DEC-PLT-005</c>.
    /// </para>
    ///
    /// <para>
    /// Bentuknya mengikuti <c>BilNumberSeriesConfiguration</c> supaya pemindahan empat deret
    /// Billing di <c>PLT-SLICE-02</c> menjadi penyalinan baris.
    /// </para>
    /// </remarks>
    public class NumNumberSeriesConfiguration : IEntityTypeConfiguration<NumNumberSeries>
    {
        public void Configure(EntityTypeBuilder<NumNumberSeries> builder)
        {
            builder.ToTable("NumNumberSeries", "public", table =>
            {
                table.HasCheckConstraint("CK_NumNumberSeries_CurrentValue", "\"CurrentValue\" > 0");
                table.HasCheckConstraint(
                    "CK_NumNumberSeries_ResetPolicy",
                    "\"ResetPolicy\" IN ('NEVER','YEARLY','MONTHLY','DAILY')");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SequenceKey).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ScopeKey).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ResetPolicy).HasMaxLength(20).IsRequired();
            builder.Property(x => x.LastAllocatedAt).HasColumnType("timestamp with time zone");

            builder.HasIndex(x => new { x.SequenceKey, x.ScopeKey }).IsUnique();
        }
    }
}
