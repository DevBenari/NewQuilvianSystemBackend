using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxEmployeeOnboardingTaskConfiguration : IEntityTypeConfiguration<TrxEmployeeOnboardingTask>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeOnboardingTask> builder)
        {
            builder.ToTable("TrxEmployeeOnboardingTask", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.TaskCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TaskName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.TaskCategory).HasMaxLength(50);
            builder.Property(x => x.DueDate).HasColumnType("date");
            builder.Property(x => x.TaskStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DocumentPath).HasMaxLength(500);
            builder.Property(x => x.OriginalFileName).HasMaxLength(250);
            builder.Property(x => x.ContentType).HasMaxLength(150);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.EmployeeOnboarding).WithMany(x => x.Tasks).HasForeignKey(x => x.EmployeeOnboardingId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OnboardingTemplateTask).WithMany().HasForeignKey(x => x.OnboardingTemplateTaskId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedToWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedToWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.EmployeeOnboardingId, x.TaskCode }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.AssignedToUserId, x.TaskStatus, x.DueDate });
        }
    }
}
