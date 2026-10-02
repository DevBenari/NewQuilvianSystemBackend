using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPathologyReportConfiguration : IEntityTypeConfiguration<LabPathologyReport>
    {
        public void Configure(EntityTypeBuilder<LabPathologyReport> builder)
        {
            builder.ToTable("LabPathologyReport", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabOrderId).IsRequired();
            builder.Property(x => x.FindingStatus).HasConversion<int>();
            builder.Property(x => x.ReopenCount).HasDefaultValue(0);

            // INV-32 — satu pesanan tepat satu laporan. PARSIAL dengan alasan yang sama seperti
            // seluruh index unik modul ini: penghapusan bersifat penandaan, dan baris tertandai
            // hapus akan tetap menempati kuncinya sehingga laporan tidak dapat dibuat ulang.
            builder.HasIndex(x => x.LabOrderId)
                .IsUnique()
                .HasDatabaseName("IX_LabPathologyReport_LabOrderId")
                .HasFilter("\"IsDelete\" = false");

            // Dipakai menjawab "laporan mana yang sudah selesai ditulis". Ia menggantikan kolom
            // status yang sengaja tidak ada (INV-36), sehingga penyaringnya perlu terindeks.
            builder.HasIndex(x => x.FinalizedAt);

            // Restrict. Laporan diagnostik adalah bagian riwayat pesanan; pesanan yang masih
            // dirujuk tidak boleh hilang dari bawahnya dan memutus penelusurannya.
            builder.HasOne(x => x.LabOrder)
                .WithMany()
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // AnalystUserId dan FinalizedByUserId SENGAJA nol dikonfigurasikan sebagai relasi.
            // Keduanya Guid? telanjang tanpa navigation property, sehingga EF nol membentuk
            // foreign key — mengikuti ResultEnteredByUserId dan peringatan REG-ACTOR-FK.
            // Ketiadaan baris konfigurasi di sini adalah keputusan, bukan kelalaian.
        }
    }
}
