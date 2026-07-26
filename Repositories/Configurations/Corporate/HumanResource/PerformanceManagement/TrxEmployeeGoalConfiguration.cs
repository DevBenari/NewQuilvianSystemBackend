using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxEmployeeGoalConfiguration : IEntityTypeConfiguration<TrxEmployeeGoal>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeGoal> entity)
        {
            entity.ToTable("TrxEmployeeGoal", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Weight).HasPrecision(8, 4);
            entity.Property(x => x.TargetValue).HasPrecision(18, 4);
            entity.Property(x => x.ProgressPercentage).HasPrecision(5, 2);
            entity.Property(x => x.CurrentValue).HasPrecision(18, 4);
            entity.Property(x => x.EvidenceJson).HasColumnType("jsonb");
            entity.Property(x => x.Weight).HasDefaultValue(0m);
            entity.Property(x => x.ProgressPercentage).HasDefaultValue(0m);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany(x => x.EmployeeGoals)
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

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

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PerformanceCycleId, x.WorkforceProfileId, x.GoalCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.GoalStatus, x.DueDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeGoal> entity)
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
