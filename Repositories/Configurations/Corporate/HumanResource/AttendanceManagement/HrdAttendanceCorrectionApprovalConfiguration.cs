using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceCorrectionApprovalConfiguration : IEntityTypeConfiguration<HrdAttendanceCorrectionApproval>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceCorrectionApproval> builder)
        {
            builder.ToTable("HrdAttendanceCorrectionApproval", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ApprovalStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ActionType).HasMaxLength(30);
            builder.Property(x => x.ActionAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Comments).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AttendanceCorrectionRequest).WithMany(x => x.Approvals).HasForeignKey(x => x.AttendanceCorrectionRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowStep).WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedApproverUser).WithMany().HasForeignKey(x => x.AssignedApproverUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedApproverWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedApproverWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ActualActionByUser).WithMany().HasForeignKey(x => x.ActualActionByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ActualActionByWorkforceProfile).WithMany().HasForeignKey(x => x.ActualActionByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DelegatedFromUser).WithMany().HasForeignKey(x => x.DelegatedFromUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttendanceCorrectionRequestId, x.StepOrder }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.AssignedApproverUserId, x.ApprovalStatus });
            builder.HasIndex(x => new { x.AssignedApproverWorkforceProfileId, x.ApprovalStatus });
        }
    }
}
