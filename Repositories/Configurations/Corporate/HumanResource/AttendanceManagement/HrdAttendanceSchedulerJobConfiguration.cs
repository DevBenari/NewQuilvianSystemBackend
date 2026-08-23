using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceSchedulerJobConfiguration
        : IEntityTypeConfiguration<HrdAttendanceSchedulerJob>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceSchedulerJob> builder)
        {
            builder.ToTable("HrdAttendanceSchedulerJob", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.JobNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.JobType)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendanceSchedulerJobType.ProcessRange)
                .IsRequired();
            builder.Property(x => x.JobStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending)
                .IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.Priority).HasDefaultValue(100);
            builder.Property(x => x.RetryCount).HasDefaultValue(0);
            builder.Property(x => x.MaxRetryCount).HasDefaultValue(3);
            builder.Property(x => x.ScheduledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AvailableAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.HeartbeatAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.FailedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.NextRetryAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.WorkerInstanceId).HasMaxLength(200);
            builder.Property(x => x.CorrelationId).HasMaxLength(100);
            builder.Property(x => x.ParametersJson).HasColumnType("jsonb");
            builder.Property(x => x.LastError).HasMaxLength(4000);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AttendancePeriod).WithMany(x => x.SchedulerJobs).HasForeignKey(x => x.AttendancePeriodId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ProcessingRun).WithMany().HasForeignKey(x => x.ProcessingRunId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TriggeredByUser).WithMany().HasForeignKey(x => x.TriggeredByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.JobNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.CorrelationId)
                .IsUnique()
                .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.JobStatus, x.AvailableAt, x.Priority });
            builder.HasIndex(x => new { x.AttendancePeriodId, x.JobStatus });
            builder.HasIndex(x => new { x.StartDate, x.EndDate, x.JobType });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId });
        }
    }
}
