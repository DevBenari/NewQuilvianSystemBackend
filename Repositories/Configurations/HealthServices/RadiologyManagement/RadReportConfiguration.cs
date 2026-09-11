using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadReportConfiguration : IEntityTypeConfiguration<RadReport>
    {
        public void Configure(EntityTypeBuilder<RadReport> builder)
        {
            builder.ToTable("RadReport", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReportNumber).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ReportStatus).HasConversion<int>().IsRequired();

            // Satu study paling banyak satu bacaan. Dijaga database, bukan hanya service:
            // dua permintaan bersamaan atas study yang sama sama-sama lolos pemeriksaan di
            // memori, dan yang kedua harus ditolak di sini.
            builder.HasIndex(x => x.RadStudyId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.ReportNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.ReportStatus);

            builder.HasOne(x => x.RadStudy)
                .WithMany()
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RadOrder)
                .WithMany()
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // EncounterId sengaja TANPA foreign key. Pemilik kunjungan adalah Registration
            // Management; kolom ini salinan untuk pencarian, sebagaimana ditetapkan
            // RAD-ERD-DICT-001 bagian 1.

            builder.HasMany(x => x.Versions)
                .WithOne(x => x.RadReport)
                .HasForeignKey(x => x.RadReportId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
