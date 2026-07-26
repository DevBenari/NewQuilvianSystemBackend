using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxIndividualDevelopmentPlanConfiguration : IEntityTypeConfiguration<TrxIndividualDevelopmentPlan>
    {
        public void Configure(EntityTypeBuilder<TrxIndividualDevelopmentPlan> entity)
        {
            entity.ToTable("TrxIndividualDevelopmentPlan", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PlanStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PlanEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ProgressPercentage).HasPrecision(5, 2);
            entity.Property(x => x.DevelopmentActionsJson).HasColumnType("jsonb");
            entity.Property(x => x.ProgressPercentage).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
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

            entity.HasOne(x => x.ManagerUser)
                .WithMany()
                .HasForeignKey(x => x.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceReview)
                .WithMany()
                .HasForeignKey(x => x.PerformanceReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany()
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PlanNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.PlanStatus, x.PlanStartDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxIndividualDevelopmentPlan> entity)
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
