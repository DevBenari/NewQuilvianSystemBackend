using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class TrxWorkforceAllocationConfiguration : IEntityTypeConfiguration<TrxWorkforceAllocation>
    {
        public void Configure(EntityTypeBuilder<TrxWorkforceAllocation> builder)
        {
            builder.ToTable("TrxWorkforceAllocation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AllocationDate).HasColumnType("date");
            builder.Property(x => x.AllocationStartDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AllocationEndDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AllocationType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.AllocationSource).HasMaxLength(30).IsRequired();
            builder.Property(x => x.AllocationRole).HasMaxLength(100);
            builder.Property(x => x.AllocationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.DailyStaffingRequirement).WithMany(x => x.WorkforceAllocations).HasForeignKey(x => x.DailyStaffingRequirementId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.AllocationDate, x.ShiftId, x.IsActive });
            builder.HasIndex(x => new { x.DailyStaffingRequirementId, x.AllocationStatus });
            builder.HasIndex(x => new { x.OrganizationUnitId, x.DepartmentId, x.PositionId, x.AllocationDate });

        }
    }
}
