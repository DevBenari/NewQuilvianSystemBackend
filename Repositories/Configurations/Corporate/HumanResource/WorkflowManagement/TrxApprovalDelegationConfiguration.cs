using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxApprovalDelegationConfiguration
        : IEntityTypeConfiguration<TrxApprovalDelegation>
    {
        public void Configure(EntityTypeBuilder<TrxApprovalDelegation> entity)
        {
            entity.ToTable("TrxApprovalDelegation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DelegationNumber)
                .HasMaxLength(60)
                .IsRequired();

            entity.Property(x => x.DelegationStatus)
                .HasMaxLength(40)
                .HasDefaultValue(WorkflowValueConstants.DelegationStatus.Draft)
                .IsRequired();

            entity.Property(x => x.EffectiveStartAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.EffectiveEndAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DelegationReason)
                .HasMaxLength(1000);

            entity.Property(x => x.SubmittedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.RevokedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.RevocationReason)
                .HasMaxLength(1000);

            entity.Property(x => x.AppliesToAllWorkflows)
                .HasDefaultValue(false);

            entity.Property(x => x.AllowSubDelegation)
                .HasDefaultValue(false);

            entity.Property(x => x.PreserveDelegatorAccountability)
                .HasDefaultValue(true);

            entity.Property(x => x.ScopeDefinitionJson)
                .HasColumnType("jsonb");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(x => x.DelegatorUser)
                .WithMany()
                .HasForeignKey(x => x.DelegatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DelegatorWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.DelegatorWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DelegateUser)
                .WithMany()
                .HasForeignKey(x => x.DelegateUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DelegateWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.DelegateWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovalDelegationPolicy)
                .WithMany()
                .HasForeignKey(x => x.ApprovalDelegationPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowStep)
                .WithMany()
                .HasForeignKey(x => x.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RevokedByUser)
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DelegationNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DelegatorUserId,
                x.DelegationStatus,
                x.EffectiveStartAt,
                x.EffectiveEndAt
            });

            entity.HasIndex(x => new
            {
                x.DelegateUserId,
                x.DelegationStatus,
                x.EffectiveStartAt,
                x.EffectiveEndAt
            });

            entity.HasIndex(x => new
            {
                x.WorkflowDefinitionId,
                x.WorkflowStepId,
                x.DelegationStatus
            });

            entity.HasIndex(x => new
            {
                x.ApprovalDelegationPolicyId,
                x.DelegationStatus
            });

            entity.HasCheckConstraint(
                "CK_TrxApprovalDelegation_EffectivePeriod",
                "\"EffectiveEndAt\" > \"EffectiveStartAt\"");

            entity.HasCheckConstraint(
                "CK_TrxApprovalDelegation_DifferentUser",
                "\"DelegatorUserId\" <> \"DelegateUserId\"");

            entity.HasCheckConstraint(
                "CK_TrxApprovalDelegation_StepRequiresDefinition",
                "\"WorkflowStepId\" IS NULL OR \"WorkflowDefinitionId\" IS NOT NULL");

            entity.HasCheckConstraint(
                "CK_TrxApprovalDelegation_WorkflowScope",
                "\"AppliesToAllWorkflows\" = true OR " +
                "\"WorkflowDefinitionId\" IS NOT NULL OR " +
                "\"ScopeDefinitionJson\" IS NOT NULL");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(
            EntityTypeBuilder<TrxApprovalDelegation> entity)
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
