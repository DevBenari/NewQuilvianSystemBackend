using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workflow
{
    public class MstWorkflowDefinitionConfiguration : IEntityTypeConfiguration<MstWorkflowDefinition>
    {
        public void Configure(EntityTypeBuilder<MstWorkflowDefinition> entity)
        {
            entity.ToTable("MstWorkflowDefinition", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkflowCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WorkflowName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RequestType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.WorkflowCategory).HasMaxLength(100).HasDefaultValue("HumanResource").IsRequired();
            entity.Property(x => x.Version).HasDefaultValue(1);
            entity.Property(x => x.WorkflowStatus).HasMaxLength(50).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.AllowRequesterCancel).HasDefaultValue(true);
            entity.Property(x => x.AllowRequesterWithdraw).HasDefaultValue(true);
            entity.Property(x => x.AllowParallelApproval).HasDefaultValue(false);
            entity.Property(x => x.AllowStepSkip).HasDefaultValue(false);
            entity.Property(x => x.StopOnRejection).HasDefaultValue(true);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
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

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkflowCode, x.Version })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.WorkflowName);
            entity.HasIndex(x => new { x.RequestType, x.WorkflowStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }
    }
}
