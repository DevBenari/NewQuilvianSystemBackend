using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan akses vaskular — <c>data-dictionary.md</c> bagian 3.4.</summary>
    public class HmdVascularAccessConfiguration : IEntityTypeConfiguration<HmdVascularAccess>
    {
        public void Configure(EntityTypeBuilder<HmdVascularAccess> builder)
        {
            builder.ToTable("HmdVascularAccess", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AccessType).HasConversion<int>().IsRequired();
            builder.Property(x => x.AccessStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.AccessSite).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ConditionNote).HasMaxLength(1000);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.AccessType);
            builder.HasIndex(x => x.AccessStatus);
            builder.HasIndex(x => x.IsPrimary);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.VascularAccesses)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
