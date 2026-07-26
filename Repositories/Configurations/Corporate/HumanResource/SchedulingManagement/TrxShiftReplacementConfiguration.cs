using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxShiftReplacementConfiguration : IEntityTypeConfiguration<TrxShiftReplacement>
    {
        public void Configure(EntityTypeBuilder<TrxShiftReplacement> entity)
        {
            entity.ToTable("TrxShiftReplacement", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReplacementType).HasMaxLength(40).HasDefaultValue("Replacement").IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ReplacementStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.ShiftAssignment).WithMany().HasForeignKey(x => x.ShiftAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OriginalWorkforceProfile).WithMany().HasForeignKey(x => x.OriginalWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReplacementWorkforceProfile).WithMany().HasForeignKey(x => x.ReplacementWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ShiftAssignmentId, x.ReplacementStatus, x.IsDelete });
            entity.HasIndex(x => new { x.ReplacementWorkforceProfileId, x.ReplacementStatus, x.IsActive, x.IsDelete });
        }
    }
}
