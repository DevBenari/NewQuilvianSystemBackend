using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxWorkflowInstanceConfiguration : IEntityTypeConfiguration<TrxWorkflowInstance>
    {
        public void Configure(EntityTypeBuilder<TrxWorkflowInstance> entity)
        {
            entity.ToTable("TrxWorkflowInstance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DueAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.LastActionAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.WithdrawnAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CurrentStepOrder).HasDefaultValue(0);
            entity.Property(x => x.WorkflowDefinitionSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.RequestContextJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByEmployee).WithMany().HasForeignKey(x => x.RequestedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
            entity.HasIndex(x => new { x.RequestedByWorkforceProfileId, x.WorkflowStatus, x.StartedAt });
            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.WorkflowStatus, x.CurrentStepOrder });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxWorkflowInstance> entity)
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
