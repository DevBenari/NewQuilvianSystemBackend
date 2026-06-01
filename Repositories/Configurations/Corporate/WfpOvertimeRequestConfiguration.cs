using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpOvertimeRequestConfiguration : IEntityTypeConfiguration<WfpOvertimeRequest>
    {
        public void Configure(EntityTypeBuilder<WfpOvertimeRequest> entity)
        {
            entity.ToTable("WfpOvertimeRequest", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.AttendanceId)
                .IsRequired(false);

            entity.Property(x => x.WorkScheduleAssignmentId)
                .IsRequired(false);

            entity.Property(x => x.WorkScheduleId)
                .IsRequired(false);

            entity.Property(x => x.OvertimeDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.ScheduledStartTime)
                .HasColumnType("time without time zone")
                .IsRequired(false);

            entity.Property(x => x.ScheduledEndTime)
                .HasColumnType("time without time zone")
                .IsRequired(false);

            entity.Property(x => x.ActualCheckInAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ActualCheckOutAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.StartTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            entity.Property(x => x.EndTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            entity.Property(x => x.IsOvernight)
                .HasDefaultValue(false);

            entity.Property(x => x.TotalMinutes)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.Reason)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(OvertimeApprovalStatus.PendingApproval)
                .IsRequired();

            entity.Property(x => x.RequestedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ApprovalNote)
                .HasMaxLength(250);

            entity.Property(x => x.RejectedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RejectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedReason)
                .HasMaxLength(250);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(250);

            entity.Property(x => x.IsPayrollProcessed)
                .HasDefaultValue(false);

            entity.Property(x => x.PayrollProcessedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.PayrollProcessedByUserId)
                .IsRequired(false);

            entity.Property(x => x.PayrollPeriodCode)
                .HasMaxLength(50);

            entity.Property(x => x.AttachmentPath)
                .HasMaxLength(500);

            entity.Property(x => x.AttachmentContentType)
                .HasMaxLength(100);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.OvertimeRequests)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Attendance)
                .WithMany()
                .HasForeignKey(x => x.AttendanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkSchedule)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PayrollProcessedByUser)
                .WithMany()
                .HasForeignKey(x => x.PayrollProcessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.AttendanceId);

            entity.HasIndex(x => x.WorkScheduleAssignmentId);

            entity.HasIndex(x => x.WorkScheduleId);

            entity.HasIndex(x => x.ApprovedByUserId);

            entity.HasIndex(x => x.RejectedByUserId);

            entity.HasIndex(x => x.CancelledByUserId);

            entity.HasIndex(x => x.PayrollProcessedByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.OvertimeDate,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.OvertimeDate,
                x.StartTime,
                x.EndTime
            })
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.ApprovalStatus,
                x.IsPayrollProcessed,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PayrollPeriodCode,
                x.IsPayrollProcessed,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkScheduleId,
                x.OvertimeDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.AttendanceId,
                x.IsDelete
            });
        }
    }
}
