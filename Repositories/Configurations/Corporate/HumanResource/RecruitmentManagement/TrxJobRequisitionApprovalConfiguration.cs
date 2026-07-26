using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxJobRequisitionApprovalConfiguration : IEntityTypeConfiguration<TrxJobRequisitionApproval>
    {
        public void Configure(EntityTypeBuilder<TrxJobRequisitionApproval> builder)
        {
            builder.ToTable("TrxJobRequisitionApproval", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ApprovalStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ActionType).HasMaxLength(30);
            builder.Property(x => x.AssignedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActionAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Comment).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.JobRequisition).WithMany().HasForeignKey(x => x.JobRequisitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowStep).WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedApproverUser).WithMany().HasForeignKey(x => x.AssignedApproverUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AssignedApproverWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedApproverWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ActualActionByUser).WithMany().HasForeignKey(x => x.ActualActionByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ActualActionByWorkforceProfile).WithMany().HasForeignKey(x => x.ActualActionByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DelegatedFromUser).WithMany().HasForeignKey(x => x.DelegatedFromUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.JobRequisitionId, x.StepOrder, x.ApprovalStatus });
            builder.HasIndex(x => new { x.AssignedApproverUserId, x.ApprovalStatus, x.IsActive });
        }
    }
}
