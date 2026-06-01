using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpPerformanceReviewConfiguration : IEntityTypeConfiguration<WfpPerformanceReview>
    {
        public void Configure(EntityTypeBuilder<WfpPerformanceReview> entity)
        {
            entity.ToTable("WfpPerformanceReview", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.ReviewPeriod)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ReviewDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.ReviewerUserId)
                .IsRequired();

            entity.Property(x => x.ReviewType)
                .HasConversion<int>()
                .HasDefaultValue(PerformanceReviewType.Unknown)
                .IsRequired();

            entity.Property(x => x.TotalScore)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.FinalRating)
                .HasConversion<int>()
                .HasDefaultValue(PerformanceFinalRating.NotRated)
                .IsRequired();

            entity.Property(x => x.ReviewStatus)
                .HasConversion<int>()
                .HasDefaultValue(PerformanceReviewStatus.Draft)
                .IsRequired();

            entity.Property(x => x.StrengthNotes)
                .HasMaxLength(1000);

            entity.Property(x => x.ImprovementNotes)
                .HasMaxLength(1000);

            entity.Property(x => x.RecommendationNotes)
                .HasMaxLength(1000);

            entity.Property(x => x.IsFinalized)
                .HasDefaultValue(false);

            entity.Property(x => x.FinalizedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.PerformanceReviews)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.ReviewerUserId);

            entity.HasIndex(x => x.ReviewPeriod);

            entity.HasIndex(x => x.ReviewDate);

            entity.HasIndex(x => x.ReviewType);

            entity.HasIndex(x => x.ReviewStatus);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ReviewPeriod,
                x.ReviewType,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ReviewStatus,
                x.IsFinalized,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ReviewerUserId,
                x.ReviewDate,
                x.IsDelete
            });
        }
    }
}
