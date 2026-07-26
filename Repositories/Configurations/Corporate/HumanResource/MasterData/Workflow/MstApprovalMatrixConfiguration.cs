using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workflow
{
    public class MstApprovalMatrixConfiguration : IEntityTypeConfiguration<MstApprovalMatrix>
    {
        public void Configure(EntityTypeBuilder<MstApprovalMatrix> entity)
        {
            entity.ToTable("MstApprovalMatrix", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkflowDefinitionId).IsRequired();
            entity.Property(x => x.WorkflowStepId).IsRequired();
            entity.Property(x => x.ApprovalMatrixCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ApprovalMatrixName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MinimumAmount).HasPrecision(18, 2).IsRequired(false);
            entity.Property(x => x.MaximumAmount).HasPrecision(18, 2).IsRequired(false);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR");
            entity.Property(x => x.MinimumDurationHours).HasPrecision(10, 2).IsRequired(false);
            entity.Property(x => x.MaximumDurationHours).HasPrecision(10, 2).IsRequired(false);
            entity.Property(x => x.ApproverSourceType).HasMaxLength(50).HasDefaultValue("RequesterManager").IsRequired();
            entity.Property(x => x.ApproverRoleCode).HasMaxLength(100);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.IsFallback).HasDefaultValue(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ConditionDefinitionJson).HasColumnType("jsonb").IsRequired(false);
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
                .WithMany(x => x.ApprovalMatrices)
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowStep)
                .WithMany(x => x.ApprovalMatrices)
                .HasForeignKey(x => x.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequesterPosition).WithMany().HasForeignKey(x => x.RequesterPositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApproverPosition).WithMany().HasForeignKey(x => x.ApproverPositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApproverOrganizationUnit).WithMany().HasForeignKey(x => x.ApproverOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ApprovalMatrixCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.WorkflowStepId, x.Priority });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId });
            entity.HasIndex(x => new { x.RequesterPositionId, x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.MinimumAmount, x.MaximumAmount, x.CurrencyCode });
            entity.HasIndex(x => new { x.MinimumDurationDays, x.MaximumDurationDays });
            entity.HasIndex(x => new { x.ApproverSourceType, x.ApproverPositionId, x.ApproverOrganizationUnitId });
            entity.HasIndex(x => new { x.IsFallback, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }
    }
}
