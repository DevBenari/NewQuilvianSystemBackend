using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class WfpScheduleChangeRequestConfiguration : IEntityTypeConfiguration<WfpScheduleChangeRequest>
    {
        public void Configure(EntityTypeBuilder<WfpScheduleChangeRequest> entity)
        {
            entity.ToTable("WfpScheduleChangeRequest", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequestType).HasMaxLength(40).HasDefaultValue("ScheduleChange").IsRequired();
            entity.Property(x => x.RequestedDate).HasColumnType("date");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.AttachmentPath).HasMaxLength(500);
            entity.Property(x => x.RequestStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkScheduleAssignment).WithMany().HasForeignKey(x => x.WorkScheduleAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RosterPeriod).WithMany().HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CurrentShiftAssignment).WithMany().HasForeignKey(x => x.CurrentShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedShiftAssignment).WithMany().HasForeignKey(x => x.RequestedShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CurrentWorkSchedule).WithMany().HasForeignKey(x => x.CurrentWorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedWorkSchedule).WithMany().HasForeignKey(x => x.RequestedWorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CurrentShift).WithMany().HasForeignKey(x => x.CurrentShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedShift).WithMany().HasForeignKey(x => x.RequestedShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.RequestStatus });
        }
    }
}
