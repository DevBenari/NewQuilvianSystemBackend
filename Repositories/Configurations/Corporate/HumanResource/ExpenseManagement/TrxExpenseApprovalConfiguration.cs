using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.ExpenseManagement
{
    public class TrxExpenseApprovalConfiguration : IEntityTypeConfiguration<TrxExpenseApproval>
    {
        public void Configure(EntityTypeBuilder<TrxExpenseApproval> entity)
        {
            entity.ToTable("TrxExpenseApproval", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ApprovalLevel).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ApprovalStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.ActionType).HasMaxLength(30);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.ActionAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DueAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Comments).HasMaxLength(2000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.ExpenseClaim).WithMany(x => x.Approvals).HasForeignKey(x => x.ExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStep).WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedApproverUser).WithMany().HasForeignKey(x => x.AssignedApproverUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedApproverWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedApproverWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActualActionByUser).WithMany().HasForeignKey(x => x.ActualActionByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActualActionByWorkforceProfile).WithMany().HasForeignKey(x => x.ActualActionByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DelegatedFromUser).WithMany().HasForeignKey(x => x.DelegatedFromUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ExpenseClaimId, x.StepOrder }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.AssignedApproverUserId, x.ApprovalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.AssignedApproverWorkforceProfileId, x.ApprovalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.ApprovalLevel, x.ApprovalStatus, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxExpenseApproval> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
