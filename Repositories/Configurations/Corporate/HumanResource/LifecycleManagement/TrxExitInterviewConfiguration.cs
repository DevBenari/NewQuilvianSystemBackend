using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxExitInterviewConfiguration : IEntityTypeConfiguration<TrxExitInterview>
    {
        public void Configure(EntityTypeBuilder<TrxExitInterview> builder)
        {
            builder.ToTable("TrxExitInterview", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.InterviewNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ScheduledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ConductedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.InterviewMode).HasMaxLength(50);
            builder.Property(x => x.InterviewStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.OverallSatisfactionScore).HasPrecision(10, 2);
            builder.Property(x => x.PrimaryReasonForLeaving).HasMaxLength(2000);
            builder.Property(x => x.PositiveFeedback).HasMaxLength(3000);
            builder.Property(x => x.ImprovementFeedback).HasMaxLength(3000);
            builder.Property(x => x.ManagerFeedback).HasMaxLength(2000);
            builder.Property(x => x.WorkplaceFeedback).HasMaxLength(2000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.EmployeeSeparation).WithMany().HasForeignKey(x => x.EmployeeSeparationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.InterviewerUser).WithMany().HasForeignKey(x => x.InterviewerUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.InterviewerWorkforceProfile).WithMany().HasForeignKey(x => x.InterviewerWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.InterviewNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.EmployeeSeparationId, x.InterviewStatus });
        }
    }
}
