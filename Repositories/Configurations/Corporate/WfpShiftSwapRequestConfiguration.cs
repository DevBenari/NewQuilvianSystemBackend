using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpShiftSwapRequestConfiguration : IEntityTypeConfiguration<WfpShiftSwapRequest>
    {
        public void Configure(EntityTypeBuilder<WfpShiftSwapRequest> entity)
        {
            entity.ToTable("WfpShiftSwapRequest", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequesterWorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.TargetWorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.RequesterScheduleAssignmentId)
                .IsRequired();

            entity.Property(x => x.TargetScheduleAssignmentId)
                .IsRequired();

            entity.Property(x => x.Reason)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(ScheduleRequestApprovalStatus.Pending)
                .IsRequired();

            entity.Property(x => x.RequestedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RejectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedReason)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.RequesterWorkforceProfile)
                .WithMany(x => x.RequestedShiftSwapRequests)
                .HasForeignKey(x => x.RequesterWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TargetWorkforceProfile)
                .WithMany(x => x.TargetShiftSwapRequests)
                .HasForeignKey(x => x.TargetWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequesterScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.RequesterScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TargetScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.TargetScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RequesterWorkforceProfileId);

            entity.HasIndex(x => x.TargetWorkforceProfileId);

            entity.HasIndex(x => x.RequesterScheduleAssignmentId);

            entity.HasIndex(x => x.TargetScheduleAssignmentId);

            entity.HasIndex(x => x.ApprovalStatus);

            entity.HasIndex(x => x.RequestedAt);

            entity.HasIndex(x => x.ApprovedByUserId);

            entity.HasIndex(x => x.RejectedByUserId);

            entity.HasIndex(x => new
            {
                x.RequesterWorkforceProfileId,
                x.TargetWorkforceProfileId,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.RequesterScheduleAssignmentId,
                x.TargetScheduleAssignmentId,
                x.ApprovalStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.RequesterWorkforceProfileId,
                x.RequesterScheduleAssignmentId,
                x.ApprovalStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.TargetWorkforceProfileId,
                x.TargetScheduleAssignmentId,
                x.ApprovalStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.RequesterScheduleAssignmentId,
                x.TargetScheduleAssignmentId,
                x.ApprovalStatus
            })
            .HasFilter("\"ApprovalStatus\" = 1 AND \"IsDelete\" = false");
        }
    }
}
