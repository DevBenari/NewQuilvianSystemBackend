using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workflow
{
    public class MstApprovalDelegationPolicyConfiguration : IEntityTypeConfiguration<MstApprovalDelegationPolicy>
    {
        public void Configure(EntityTypeBuilder<MstApprovalDelegationPolicy> entity)
        {
            entity.ToTable("MstApprovalDelegationPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DelegationPolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DelegationPolicyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DelegationType).HasMaxLength(50).HasDefaultValue("Temporary").IsRequired();
            entity.Property(x => x.MaximumDelegationDays).HasDefaultValue(30);
            entity.Property(x => x.MinimumNoticeHours).HasDefaultValue(0);
            entity.Property(x => x.RequireManagerApproval).HasDefaultValue(false);
            entity.Property(x => x.RequireHrVerification).HasDefaultValue(false);
            entity.Property(x => x.AllowCrossOrganizationUnit).HasDefaultValue(false);
            entity.Property(x => x.AllowCrossHospitalSite).HasDefaultValue(false);
            entity.Property(x => x.AllowCrossLegalEntity).HasDefaultValue(false);
            entity.Property(x => x.AllowSubDelegation).HasDefaultValue(false);
            entity.Property(x => x.AllowSelfDelegation).HasDefaultValue(false);
            entity.Property(x => x.PreserveDelegatorAccountability).HasDefaultValue(true);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
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

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany(x => x.DelegationPolicies)
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowStep)
                .WithMany(x => x.DelegationPolicies)
                .HasForeignKey(x => x.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DelegationPolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.WorkflowStepId });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.DelegationType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }
    }
}
