using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabResultCorrectionReasonConfiguration : IEntityTypeConfiguration<LabResultCorrectionReason>
    {
        public void Configure(EntityTypeBuilder<LabResultCorrectionReason> builder)
        {
            builder.ToTable("LabResultCorrectionReason", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReasonCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.ReasonName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256);

            builder.Property(x => x.RequiresNote).HasDefaultValue(false);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            // DEFAULT true di skema (kamus data 17.6), tetapi EF WAJIB selalu mengirim nilainya.
            // Tanpa ValueGeneratedNever, false adalah nilai "belum diisi" bagi EF: baris yang
            // sengaja dibuat nonaktif akan tersimpan AKTIF oleh default database, tanpa galat.
            builder.Property(x => x.IsActive).HasDefaultValue(true).ValueGeneratedNever();

            // PARSIAL: penghapusan di sistem ini bersifat penandaan, sehingga tanpa pembatas ini
            // kode yang pernah dipakai lalu dihapus nol akan pernah dapat dipakai lagi. Kelas
            // cacat yang sama dengan LAB-CONFLICT-005.
            builder.HasIndex(x => x.ReasonCode)
                .IsUnique()
                .HasDatabaseName("IX_LabResultCorrectionReason_ReasonCode")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
