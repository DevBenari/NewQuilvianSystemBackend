using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan tinjauan serologi — <c>data-dictionary.md</c> bagian 3.5.</summary>
    /// <remarks>
    /// <c>LabExaminationId</c> sengaja tanpa FK: hasil laboratorium milik modul lain.
    /// </remarks>
    public class HmdSerologyReviewConfiguration : IEntityTypeConfiguration<HmdSerologyReview>
    {
        public void Configure(EntityTypeBuilder<HmdSerologyReview> builder)
        {
            builder.ToTable("HmdSerologyReview", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TestType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ResultFlag).HasConversion<int>().IsRequired();
            builder.Property(x => x.ReviewStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ResultSummary).HasMaxLength(500);
            builder.Property(x => x.ReviewNote).HasMaxLength(1000);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.TestType);
            builder.HasIndex(x => x.LabExaminationId);
            builder.HasIndex(x => x.ResultDate);
            builder.HasIndex(x => x.ResultFlag);
            builder.HasIndex(x => x.ReviewStatus);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.SerologyReviews)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
