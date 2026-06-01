using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpPerformanceReviewDetailConfiguration : IEntityTypeConfiguration<WfpPerformanceReviewDetail>
    {
        public void Configure(EntityTypeBuilder<WfpPerformanceReviewDetail> entity)
        {
            entity.ToTable("WfpPerformanceReviewDetail", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PerformanceReviewId)
                .IsRequired();

            entity.Property(x => x.CriteriaCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CriteriaName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Score)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.Weight)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.WeightedScore)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.PerformanceReview)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.PerformanceReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PerformanceReviewId);

            entity.HasIndex(x => x.CriteriaCode);

            entity.HasIndex(x => new
            {
                x.PerformanceReviewId,
                x.CriteriaCode
            })
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PerformanceReviewId,
                x.IsDelete
            });
        }
    }
}
