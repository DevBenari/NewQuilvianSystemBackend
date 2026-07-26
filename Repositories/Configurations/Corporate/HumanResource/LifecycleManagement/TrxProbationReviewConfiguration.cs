using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxProbationReviewConfiguration : IEntityTypeConfiguration<TrxProbationReview>
    {
        public void Configure(EntityTypeBuilder<TrxProbationReview> builder)
        {
            builder.ToTable("TrxProbationReview", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.ReviewNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProbationStartDate).HasColumnType("date");
            builder.Property(x => x.ProbationEndDate).HasColumnType("date");
            builder.Property(x => x.ReviewDate).HasColumnType("date");
            builder.Property(x => x.PerformanceScore).HasPrecision(10, 2);
            builder.Property(x => x.CompetencyScore).HasPrecision(10, 2);
            builder.Property(x => x.AttendanceScore).HasPrecision(10, 2);
            builder.Property(x => x.OverallScore).HasPrecision(10, 2);
            builder.Property(x => x.ReviewResult).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ExtendedProbationEndDate).HasColumnType("date");
            builder.Property(x => x.Strengths).HasMaxLength(2000);
            builder.Property(x => x.ImprovementAreas).HasMaxLength(2000);
            builder.Property(x => x.Recommendation).HasMaxLength(2000);
            builder.Property(x => x.ReviewStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeOnboarding).WithMany().HasForeignKey(x => x.EmployeeOnboardingId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewerWorkforceProfile).WithMany().HasForeignKey(x => x.ReviewerWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewerUser).WithMany().HasForeignKey(x => x.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PerformanceRatingScale).WithMany().HasForeignKey(x => x.PerformanceRatingScaleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.ReviewNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.ProbationEndDate, x.ReviewStatus });
        }
    }
}
