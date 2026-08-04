using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxWorkflowApproverAssignmentConfiguration
        : IEntityTypeConfiguration<TrxWorkflowApproverAssignment>
    {
        public void Configure(EntityTypeBuilder<TrxWorkflowApproverAssignment> entity)
        {
            entity.ToTable("TrxWorkflowApproverAssignment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssignedApproverRoleCode)
                .HasMaxLength(100);

            entity.Property(x => x.ApproverSourceSnapshot)
                .HasMaxLength(50)
                .HasDefaultValue(WorkflowValueConstants.ApproverSource.RequesterManager)
                .IsRequired();

            entity.Property(x => x.AssignmentOrder)
                .HasDefaultValue(1);

            entity.Property(x => x.AssignmentStatus)
                .HasMaxLength(40)
                .HasDefaultValue(WorkflowValueConstants.AssignmentStatus.Pending)
                .IsRequired();

            entity.Property(x => x.AssignedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.AvailableAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.StartedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DueAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DelegatedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.IsRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsCurrentAssignment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDelegated)
                .HasDefaultValue(false);

            entity.Property(x => x.ResolutionSnapshotJson)
                .HasColumnType("jsonb");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowInstance)
                .WithMany()
                .HasForeignKey(x => x.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowStepInstance)
                .WithMany(x => x.ApproverAssignments)
                .HasForeignKey(x => x.WorkflowStepInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovalMatrix)
                .WithMany()
                .HasForeignKey(x => x.ApprovalMatrixId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovalDelegation)
                .WithMany(x => x.ApproverAssignments)
                .HasForeignKey(x => x.ApprovalDelegationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedApproverUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedApproverWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.AssignedApproverWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OriginalApproverUser)
                .WithMany()
                .HasForeignKey(x => x.OriginalApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OriginalApproverWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.OriginalApproverWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.WorkflowStepInstanceId,
                x.AssignmentOrder
            })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.WorkflowStepInstanceId,
                x.AssignedApproverUserId
            })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.AssignedApproverUserId,
                x.AssignmentStatus,
                x.AvailableAt,
                x.DueAt
            });

            entity.HasIndex(x => new
            {
                x.AssignedApproverWorkforceProfileId,
                x.AssignmentStatus
            });

            entity.HasIndex(x => new
            {
                x.WorkflowInstanceId,
                x.AssignmentStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ApprovalDelegationId,
                x.IsDelegated
            });

            entity.HasCheckConstraint(
                "CK_TrxWorkflowApproverAssignment_AssignmentOrder",
                "\"AssignmentOrder\" > 0");

            entity.HasCheckConstraint(
                "CK_TrxWorkflowApproverAssignment_CompletedAt",
                "\"CompletedAt\" IS NULL OR \"CompletedAt\" >= \"AssignedAt\"");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(
            EntityTypeBuilder<TrxWorkflowApproverAssignment> entity)
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
