using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class WfpWorkScheduleAssignmentConfiguration : IEntityTypeConfiguration<WfpWorkScheduleAssignment>
    {
        public void Configure(EntityTypeBuilder<WfpWorkScheduleAssignment> entity)
        {
            entity.ToTable("WfpWorkScheduleAssignment", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssignmentType).HasMaxLength(30).HasDefaultValue("Primary").IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.IsPrimary).HasDefaultValue(true);
            entity.Property(x => x.IsRotating).HasDefaultValue(false);
            entity.Property(x => x.IsTemporary).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.WorkforceProfile).WithMany(x => x.WorkScheduleAssignments).HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftGroup).WithMany().HasForeignKey(x => x.ShiftGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftPattern).WithMany().HasForeignKey(x => x.ShiftPatternId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RosterPolicy).WithMany().HasForeignKey(x => x.RosterPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MinimumRestPolicy).WithMany().HasForeignKey(x => x.MinimumRestPolicyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate });
            entity.HasIndex(x => new { x.WorkforceProfileId, x.IsPrimary, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.IsActive, x.IsDelete });
        }
    }
}
