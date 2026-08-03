using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveAttendanceIntegrationConfiguration : IEntityTypeConfiguration<TrxLeaveAttendanceIntegration>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveAttendanceIntegration> builder)
        {
            builder.ToTable("TrxLeaveAttendanceIntegration", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LeaveDate).HasColumnType("date");
            builder.Property(x => x.RequestedLeaveDays).HasPrecision(18, 4);
            builder.Property(x => x.IntegrationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.AttendanceStatusBefore).HasMaxLength(50);
            builder.Property(x => x.AttendanceStatusAfter).HasMaxLength(50);
            builder.Property(x => x.ProcessingStatusBefore).HasMaxLength(30);
            builder.Property(x => x.ProcessingStatusAfter).HasMaxLength(30);
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IdempotencyKey).HasMaxLength(160);
            builder.Property(x => x.ScheduleSnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.ResultSnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.HasOne(x => x.LeaveExecution).WithMany(x => x.AttendanceIntegrations).HasForeignKey(x => x.LeaveExecutionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LeaveRequest).WithMany().HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AppliedByUser).WithMany().HasForeignKey(x => x.AppliedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.LeaveRequestId, x.LeaveDate }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.IntegrationStatus, x.LeaveDate });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.LeaveDate });
            builder.HasIndex(x => x.AttendanceDailyId);
        }
    }
}
