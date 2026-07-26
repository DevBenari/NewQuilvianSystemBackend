using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class WfpShiftSwapRequestConfiguration : IEntityTypeConfiguration<WfpShiftSwapRequest>
    {
        public void Configure(EntityTypeBuilder<WfpShiftSwapRequest> entity)
        {
            entity.ToTable("WfpShiftSwapRequest", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequesterShiftDate).HasColumnType("date");
            entity.Property(x => x.TargetShiftDate).HasColumnType("date");
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.AttachmentPath).HasMaxLength(500);
            entity.Property(x => x.RequestStatus).HasMaxLength(30).HasDefaultValue("PendingTarget").IsRequired();
            entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.TargetRespondedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.TargetResponseNotes).HasMaxLength(1000);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.RequesterWorkforceProfile).WithMany().HasForeignKey(x => x.RequesterWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TargetWorkforceProfile).WithMany().HasForeignKey(x => x.TargetWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RosterPeriod).WithMany().HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequesterShiftAssignment).WithMany().HasForeignKey(x => x.RequesterShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TargetShiftAssignment).WithMany().HasForeignKey(x => x.TargetShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.RequesterWorkforceProfileId, x.RequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.TargetWorkforceProfileId, x.RequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.RequesterShiftAssignmentId, x.TargetShiftAssignmentId, x.IsDelete });
        }
    }
}
