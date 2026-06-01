using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpLeaveRequestConfiguration : IEntityTypeConfiguration<WfpLeaveRequest>
    {
        public void Configure(EntityTypeBuilder<WfpLeaveRequest> entity)
        {
            entity.ToTable("WfpLeaveRequest", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.LeaveBalanceId)
                .IsRequired(false);

            entity.Property(x => x.LeaveType)
                .HasConversion<int>()
                .HasDefaultValue(LeaveType.AnnualLeave)
                .IsRequired();

            entity.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.TotalDays)
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.IsHalfDay)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDeductBalance)
                .HasDefaultValue(true);

            entity.Property(x => x.Reason)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(LeaveApprovalStatus.PendingApproval)
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
                .WithMany(x => x.LeaveRequests)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeaveBalance)
                .WithMany(x => x.LeaveRequests)
                .HasForeignKey(x => x.LeaveBalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.LeaveBalanceId);

            entity.HasIndex(x => x.ApprovedByUserId);

            entity.HasIndex(x => x.RejectedByUserId);

            entity.HasIndex(x => x.CancelledByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.LeaveType,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.StartDate,
                x.EndDate,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.LeaveType,
                x.ApprovalStatus,
                x.StartDate,
                x.EndDate
            });

            entity.HasIndex(x => new
            {
                x.RequestedAt,
                x.ApprovalStatus,
                x.IsDelete
            });
        }
    }
}
