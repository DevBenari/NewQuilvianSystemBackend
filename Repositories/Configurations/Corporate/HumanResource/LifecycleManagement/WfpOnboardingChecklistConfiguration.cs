using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class WfpOnboardingChecklistConfiguration : IEntityTypeConfiguration<WfpOnboardingChecklist>
    {
        public void Configure(EntityTypeBuilder<WfpOnboardingChecklist> builder)
        {
            builder.ToTable("WfpOnboardingChecklist", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.ChecklistNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.OnboardingType).HasMaxLength(50);
            builder.Property(x => x.PlannedStartDate).HasColumnType("date");
            builder.Property(x => x.ActualStartDate).HasColumnType("date");
            builder.Property(x => x.PlannedCompletionDate).HasColumnType("date");
            builder.Property(x => x.ActualCompletionDate).HasColumnType("date");
            builder.Property(x => x.ChecklistStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ProgressPercentage).HasPrecision(7, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OnboardingTemplate).WithMany().HasForeignKey(x => x.OnboardingTemplateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CandidateHiring).WithMany().HasForeignKey(x => x.CandidateHiringId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CoordinatorUser).WithMany().HasForeignKey(x => x.CoordinatorUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ManagerWorkforceProfile).WithMany().HasForeignKey(x => x.ManagerWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.ChecklistNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.ChecklistStatus, x.IsActive });
        }
    }
}
