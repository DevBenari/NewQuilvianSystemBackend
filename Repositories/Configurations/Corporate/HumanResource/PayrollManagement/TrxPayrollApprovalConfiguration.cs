using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollApprovalConfiguration : IEntityTypeConfiguration<TrxPayrollApproval>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollApproval> entity)
        {

            entity.ToTable("TrxPayrollApproval", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ApprovalLevel).HasMaxLength(50).HasDefaultValue("Payroll").IsRequired();
            entity.Property(x => x.ApprovalStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.ActionType).HasMaxLength(30);
            entity.Property(x => x.ActionAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Comments).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRun).WithMany(x => x.Approvals).HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStep).WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedApproverUser).WithMany().HasForeignKey(x => x.AssignedApproverUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedApproverWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedApproverWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActualActionByUser).WithMany().HasForeignKey(x => x.ActualActionByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActualActionByWorkforceProfile).WithMany().HasForeignKey(x => x.ActualActionByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DelegatedFromUser).WithMany().HasForeignKey(x => x.DelegatedFromUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PayrollRunId, x.StepOrder, x.AssignedApproverUserId, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollRunId, x.ApprovalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.AssignedApproverUserId, x.ApprovalStatus, x.IsActive, x.IsDelete });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
