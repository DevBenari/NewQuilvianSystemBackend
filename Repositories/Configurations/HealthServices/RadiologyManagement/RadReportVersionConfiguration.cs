using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadReportVersionConfiguration : IEntityTypeConfiguration<RadReportVersion>
    {
        public void Configure(EntityTypeBuilder<RadReportVersion> builder)
        {
            builder.ToTable("RadReportVersion", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Findings).HasMaxLength(8000);
            builder.Property(x => x.Impression).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.Recommendation).HasMaxLength(2000);
            builder.Property(x => x.AmendmentReason).HasMaxLength(1000);

            builder.Property(x => x.VersionStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.AuthorRoleSnapshot).HasConversion<int>().IsRequired();

            // Nomor versi tidak boleh kembar dalam satu bacaan. Tanpa penjaga ini, dua
            // amandemen yang berjalan hampir bersamaan dapat sama-sama menjadi "versi 2", dan
            // riwayat klinisnya bercabang tanpa ada yang tahu mana yang berlaku.
            builder.HasIndex(x => new { x.RadReportId, x.VersionNumber })
                .IsUnique();

            builder.HasIndex(x => x.PreviousVersionId);
            builder.HasIndex(x => x.AuthorUserId);

            // Rantai koreksi: setiap versi menunjuk versi yang digantikannya. Restrict, karena
            // menghapus satu mata rantai akan memutus riwayat yang justru wajib utuh.
            builder.HasOne(x => x.PreviousVersion)
                .WithMany()
                .HasForeignKey(x => x.PreviousVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
