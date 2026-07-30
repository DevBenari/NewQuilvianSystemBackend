using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxWorkflowStepInstanceConfiguration
        : IEntityTypeConfiguration<TrxWorkflowStepInstance>
    {
        public void Configure(EntityTypeBuilder<TrxWorkflowStepInstance> entity)
        {
            entity.ToTable("TrxWorkflowStepInstance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StepCodeSnapshot)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.StepNameSnapshot)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.StepTypeSnapshot)
                .HasMaxLength(50)
                .HasDefaultValue(WorkflowValueConstants.StepType.Approval)
                .IsRequired();

            entity.Property(x => x.ApprovalModeSnapshot)
                .HasMaxLength(50)
                .HasDefaultValue(WorkflowValueConstants.ApprovalMode.Any)
                .IsRequired();

            entity.Property(x => x.ApproverSourceSnapshot)
                .HasMaxLength(50)
                .HasDefaultValue(WorkflowValueConstants.ApproverSource.RequesterManager)
                .IsRequired();

            entity.Property(x => x.RequiredApprovalPercentage)
                .HasPrecision(5, 2);

            entity.Property(x => x.AvailableAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.StartedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DueAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.SkippedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.RequiredApprovalCount)
                .HasDefaultValue(1);

            entity.Property(x => x.TotalAssignmentCount)
                .HasDefaultValue(0);

            entity.Property(x => x.ApprovedActionCount)
                .HasDefaultValue(0);

            entity.Property(x => x.RejectedActionCount)
                .HasDefaultValue(0);

            entity.Property(x => x.StepStatus)
                .HasMaxLength(40)
                .HasDefaultValue(WorkflowValueConstants.StepStatus.Pending)
                .IsRequired();

            entity.Property(x => x.IsCurrentStep)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDelegationAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAutoAction)
                .HasDefaultValue(false);

            entity.Property(x => x.InstructionsSnapshot)
                .HasMaxLength(1000);

            entity.Property(x => x.AssignmentResolutionJson)
                .HasColumnType("jsonb");

            entity.Property(x => x.StepConditionSnapshotJson)
                .HasColumnType("jsonb");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowInstance)
                .WithMany(x => x.StepInstances)
                .HasForeignKey(x => x.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowStep)
                .WithMany()
                .HasForeignKey(x => x.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovalMatrix)
                .WithMany()
                .HasForeignKey(x => x.ApprovalMatrixId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.WorkflowInstanceId,
                x.StepCodeSnapshot
            })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.WorkflowInstanceId,
                x.StepOrder,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkflowInstanceId,
                x.IsCurrentStep,
                x.StepStatus
            });

            entity.HasIndex(x => new
            {
                x.ApprovalMatrixId,
                x.StepStatus
            });

            entity.HasIndex(x => x.DueAt);

            entity.HasCheckConstraint(
                "CK_TrxWorkflowStepInstance_StepOrder",
                "\"StepOrder\" > 0");

            entity.HasCheckConstraint(
                "CK_TrxWorkflowStepInstance_RequiredApprovalCount",
                "\"RequiredApprovalCount\" > 0");

            entity.HasCheckConstraint(
                "CK_TrxWorkflowStepInstance_RequiredApprovalPercentage",
                "\"RequiredApprovalPercentage\" IS NULL OR " +
                "(\"RequiredApprovalPercentage\" > 0 AND \"RequiredApprovalPercentage\" <= 100)");

            entity.HasCheckConstraint(
                "CK_TrxWorkflowStepInstance_ActionCounters",
                "\"TotalAssignmentCount\" >= 0 AND " +
                "\"ApprovedActionCount\" >= 0 AND " +
                "\"RejectedActionCount\" >= 0");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(
            EntityTypeBuilder<TrxWorkflowStepInstance> entity)
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
