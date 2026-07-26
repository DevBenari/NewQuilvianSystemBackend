using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class WfpPerformanceReviewDetailConfiguration : IEntityTypeConfiguration<WfpPerformanceReviewDetail>
    {
        public void Configure(EntityTypeBuilder<WfpPerformanceReviewDetail> entity)
        {
            entity.ToTable("WfpPerformanceReviewDetail", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Weight).HasPrecision(8, 4);
            entity.Property(x => x.TargetValue).HasPrecision(18, 4);
            entity.Property(x => x.ActualValue).HasPrecision(18, 4);
            entity.Property(x => x.SelfScore).HasPrecision(18, 4);
            entity.Property(x => x.ManagerScore).HasPrecision(18, 4);
            entity.Property(x => x.FinalScore).HasPrecision(18, 4);
            entity.Property(x => x.Score).HasPrecision(18, 4);
            entity.Property(x => x.Weight).HasDefaultValue(0m);
            entity.Property(x => x.Sequence).HasDefaultValue(1);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceReview)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.PerformanceReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.KpiCatalog)
                .WithMany()
                .HasForeignKey(x => x.KpiCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceTemplateDetail)
                .WithMany()
                .HasForeignKey(x => x.PerformanceTemplateDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PerformanceReviewId, x.Sequence })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.KpiCatalogId, x.PerformanceReviewId });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpPerformanceReviewDetail> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
