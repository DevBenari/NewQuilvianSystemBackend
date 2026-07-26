using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class TrxRosterAssignmentConfiguration : IEntityTypeConfiguration<TrxRosterAssignment>
    {
        public void Configure(EntityTypeBuilder<TrxRosterAssignment> entity)
        {
            entity.ToTable("TrxRosterAssignment", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssignmentStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.ValidatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.RosterPeriod).WithMany(x => x.RosterAssignments).HasForeignKey(x => x.RosterPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkScheduleAssignment).WithMany().HasForeignKey(x => x.WorkScheduleAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ShiftGroup).WithMany().HasForeignKey(x => x.ShiftGroupId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.RosterPeriodId, x.WorkforceProfileId }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.AssignmentStatus, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.HasConflict, x.IsValidationPassed, x.IsDelete });
        }
    }
}
