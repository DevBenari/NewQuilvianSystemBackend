using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workflow
{
    public class MstWorkflowStepConfiguration : IEntityTypeConfiguration<MstWorkflowStep>
    {
        public void Configure(EntityTypeBuilder<MstWorkflowStep> entity)
        {
            entity.ToTable("MstWorkflowStep", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkflowDefinitionId).IsRequired();
            entity.Property(x => x.StepCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.StepName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.StepOrder).HasDefaultValue(1);
            entity.Property(x => x.StepType).HasMaxLength(50).HasDefaultValue("Approval").IsRequired();
            entity.Property(x => x.ApprovalMode).HasMaxLength(50).HasDefaultValue("Any").IsRequired();
            entity.Property(x => x.RequiredApprovalCount).HasDefaultValue(1);
            entity.Property(x => x.RequiredApprovalPercentage).HasPrecision(5, 2).IsRequired(false);
            entity.Property(x => x.ApproverSourceType).HasMaxLength(50).HasDefaultValue("RequesterManager").IsRequired();
            entity.Property(x => x.ApproverRoleCode).HasMaxLength(100);
            entity.Property(x => x.IsRequired).HasDefaultValue(true);
            entity.Property(x => x.IsParallel).HasDefaultValue(false);
            entity.Property(x => x.AllowDelegation).HasDefaultValue(true);
            entity.Property(x => x.AllowSelfApproval).HasDefaultValue(false);
            entity.Property(x => x.OnApproveNextStepCode).HasMaxLength(50);
            entity.Property(x => x.OnRejectStepCode).HasMaxLength(50);
            entity.Property(x => x.Instructions).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany(x => x.WorkflowSteps)
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApproverPosition)
                .WithMany()
                .HasForeignKey(x => x.ApproverPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApproverOrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.ApproverOrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.StepCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.StepOrder, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.StepType, x.ApprovalMode, x.ApproverSourceType });
            entity.HasIndex(x => new { x.ApproverPositionId, x.ApproverOrganizationUnitId, x.SpecificApproverUserId });
            entity.HasIndex(x => x.ApproverRoleCode);
        }
    }
}
