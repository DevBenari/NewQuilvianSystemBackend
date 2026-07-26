using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxShiftAssignmentConfiguration : IEntityTypeConfiguration<TrxShiftAssignment>
    {
        public void Configure(EntityTypeBuilder<TrxShiftAssignment> entity)
        {
            entity.ToTable("TrxShiftAssignment", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ShiftDate).HasColumnType("date");
            entity.Property(x => x.ScheduledStartAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ScheduledEndAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AssignmentType).HasMaxLength(30).HasDefaultValue("Regular").IsRequired();
            entity.Property(x => x.AssignmentStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.AssignmentSource).HasMaxLength(30).HasDefaultValue("Roster");
            entity.Property(x => x.ValidatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.OverrideReason).HasMaxLength(1000);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.RosterAssignment).WithMany(x => x.ShiftAssignments).HasForeignKey(x => x.RosterAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DailyStaffingRequirement).WithMany().HasForeignKey(x => x.DailyStaffingRequirementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftSkillRequirement).WithMany().HasForeignKey(x => x.ShiftSkillRequirementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ScheduleChangeRequest).WithMany().HasForeignKey(x => x.ScheduleChangeRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftSwapRequest).WithMany().HasForeignKey(x => x.ShiftSwapRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ValidatedByUser).WithMany().HasForeignKey(x => x.ValidatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WorkforceProfileId, x.ScheduledStartAt, x.ScheduledEndAt, x.IsDelete });
            entity.HasIndex(x => new { x.RosterAssignmentId, x.ShiftDate, x.AssignmentStatus });
            entity.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.ShiftDate, x.ShiftId });
            entity.HasIndex(x => new { x.HasBlockingConflict, x.IsValidationPassed, x.IsDelete });
        }
    }
}
