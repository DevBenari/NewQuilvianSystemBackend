using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class TrxHrServiceRequestConfiguration : IEntityTypeConfiguration<TrxHrServiceRequest>
    {
        public void Configure(EntityTypeBuilder<TrxHrServiceRequest> entity)
        {
            entity.ToTable("TrxHrServiceRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SlaDueAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.FirstResponseAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.LastActivityAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsSlaBreached).HasDefaultValue(false);
            entity.Property(x => x.IsEmployeeVisible).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(false);
            entity.Property(x => x.RequestPayloadJson).HasColumnType("jsonb");
            entity.Property(x => x.ServiceSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.HrServiceCategory).WithMany(x => x.ServiceRequests).HasForeignKey(x => x.HrServiceCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HrServiceType).WithMany(x => x.ServiceRequests).HasForeignKey(x => x.HrServiceTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByEmployee).WithMany().HasForeignKey(x => x.RequestedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowInstance).WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedToWorkforceProfile).WithMany().HasForeignKey(x => x.AssignedToWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.RequestedByWorkforceProfileId, x.RequestStatus, x.RequestedAt });
            entity.HasIndex(x => new { x.AssignedToUserId, x.RequestStatus, x.SlaDueAt });
            entity.HasIndex(x => new { x.HrServiceCategoryId, x.HrServiceTypeId, x.RequestStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxHrServiceRequest> entity)
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
