using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class WfpPerformanceReviewConfiguration : IEntityTypeConfiguration<WfpPerformanceReview>
    {
        public void Configure(EntityTypeBuilder<WfpPerformanceReview> entity)
        {
            entity.ToTable("WfpPerformanceReview", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PeriodStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PeriodEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReviewDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.OverallScore).HasPrecision(18, 4);
            entity.Property(x => x.FinalScore).HasPrecision(18, 4);
            entity.Property(x => x.OverallScore).HasDefaultValue(0m);
            entity.Property(x => x.FinalScore).HasDefaultValue(0m);
            entity.Property(x => x.IsAcknowledged).HasDefaultValue(false);
            entity.Property(x => x.IsFinalized).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.PerformanceReviews)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(x => x.PerformanceReviews)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MasterPerformanceCycle)
                .WithMany()
                .HasForeignKey(x => x.MasterPerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceTemplate)
                .WithMany()
                .HasForeignKey(x => x.PerformanceTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RatingScale)
                .WithMany()
                .HasForeignKey(x => x.RatingScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ManagerUser)
                .WithMany()
                .HasForeignKey(x => x.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FinalizedByUser)
                .WithMany()
                .HasForeignKey(x => x.FinalizedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReviewNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.PeriodStartDate, x.PeriodEndDate });

            entity.HasIndex(x => new { x.PerformanceCycleId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"PerformanceCycleId\" IS NOT NULL AND \"IsDelete\" = false");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpPerformanceReview> entity)
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
