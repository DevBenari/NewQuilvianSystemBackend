using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxApprovalActionConfiguration : IEntityTypeConfiguration<TrxApprovalAction>
    {
        public void Configure(EntityTypeBuilder<TrxApprovalAction> entity)
        {
            entity.ToTable("TrxApprovalAction", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelegated).HasDefaultValue(false);
            entity.Property(x => x.IsSystemAction).HasDefaultValue(false);
            entity.Property(x => x.ActionContextJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowInstance).WithMany(x => x.ApprovalActions).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStepInstance).WithMany(x => x.ApprovalActions).HasForeignKey(x => x.WorkflowStepInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovalDelegation).WithMany(x => x.ApprovalActions).HasForeignKey(x => x.ApprovalDelegationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedApproverUser).WithMany().HasForeignKey(x => x.AssignedApproverUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedApproverWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedApproverWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActualActionByUser).WithMany().HasForeignKey(x => x.ActualActionByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ActualActionByWorkforceProfile).WithMany().HasForeignKey(x => x.ActualActionByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DelegatedFromUser).WithMany().HasForeignKey(x => x.DelegatedFromUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkflowInstanceId, x.ActionAt });
            entity.HasIndex(x => new { x.WorkflowStepInstanceId, x.ActionType, x.ActionAt });
            entity.HasIndex(x => new { x.ActualActionByUserId, x.ActionAt });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxApprovalAction> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
