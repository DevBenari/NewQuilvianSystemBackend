using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    /// <summary>Gambar radiologi pesanan — <c>BE-RAD-22</c>.</summary>
    public class RadOrderImageConfiguration : IEntityTypeConfiguration<RadOrderImage>
    {
        public void Configure(EntityTypeBuilder<RadOrderImage> builder)
        {
            builder.ToTable("RadOrderImage", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();

            builder.HasOne(x => x.RadOrder)
                .WithMany()
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RadStudy>()
                .WithMany()
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.RadOrderId, x.IsDelete, x.SortOrder });
            builder.HasIndex(x => x.RadStudyId);
        }
    }
}
