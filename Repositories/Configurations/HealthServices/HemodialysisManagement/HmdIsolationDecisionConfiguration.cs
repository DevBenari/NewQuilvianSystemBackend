using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan keputusan isolasi — <c>data-dictionary.md</c> bagian 3.6.</summary>
    public class HmdIsolationDecisionConfiguration : IEntityTypeConfiguration<HmdIsolationDecision>
    {
        public void Configure(EntityTypeBuilder<HmdIsolationDecision> builder)
        {
            builder.ToTable("HmdIsolationDecision", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Requirement).HasConversion<int>().IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.Requirement);
            builder.HasIndex(x => x.EffectiveFrom);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.DecidedByUserId);
            builder.HasIndex(x => x.SerologyReviewId);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.IsolationDecisions)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SerologyReview)
                .WithMany()
                .HasForeignKey(x => x.SerologyReviewId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
