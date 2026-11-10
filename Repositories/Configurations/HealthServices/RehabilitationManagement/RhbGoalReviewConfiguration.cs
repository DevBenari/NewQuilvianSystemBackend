using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan capaian tujuan per reasesmen — <c>data-dictionary.md</c> bagian 2.15.</summary>
    public class RhbGoalReviewConfiguration : IEntityTypeConfiguration<RhbGoalReview>
    {
        public void Configure(EntityTypeBuilder<RhbGoalReview> builder)
        {
            builder.ToTable("RhbGoalReview", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AchievementStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.NoteText).HasMaxLength(500);

            builder.HasIndex(x => new { x.ProgressReviewId, x.GoalId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.GoalId);

            builder.HasOne(x => x.ProgressReview)
                .WithMany(x => x.GoalReviews)
                .HasForeignKey(x => x.ProgressReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Goal)
                .WithMany()
                .HasForeignKey(x => x.GoalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
