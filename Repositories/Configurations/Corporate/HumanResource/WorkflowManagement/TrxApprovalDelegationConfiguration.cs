using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxApprovalDelegationConfiguration : IEntityTypeConfiguration<TrxApprovalDelegation>
    {
        public void Configure(EntityTypeBuilder<TrxApprovalDelegation> entity)
        {
            entity.ToTable("TrxApprovalDelegation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EffectiveStartAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AppliesToAllWorkflows).HasDefaultValue(false);
            entity.Property(x => x.AllowSubDelegation).HasDefaultValue(false);
            entity.Property(x => x.PreserveDelegatorAccountability).HasDefaultValue(true);
            entity.Property(x => x.ScopeDefinitionJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.DelegatorUser).WithMany().HasForeignKey(x => x.DelegatorUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DelegatorWorkforceProfile).WithMany().HasForeignKey(x => x.DelegatorWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DelegateUser).WithMany().HasForeignKey(x => x.DelegateUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DelegateWorkforceProfile).WithMany().HasForeignKey(x => x.DelegateWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovalDelegationPolicy).WithMany().HasForeignKey(x => x.ApprovalDelegationPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStep).WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RevokedByUser).WithMany().HasForeignKey(x => x.RevokedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DelegationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.DelegatorUserId, x.DelegationStatus, x.EffectiveStartAt, x.EffectiveEndAt });
            entity.HasIndex(x => new { x.DelegateUserId, x.DelegationStatus, x.EffectiveStartAt, x.EffectiveEndAt });
            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.WorkflowStepId });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxApprovalDelegation> entity)
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
