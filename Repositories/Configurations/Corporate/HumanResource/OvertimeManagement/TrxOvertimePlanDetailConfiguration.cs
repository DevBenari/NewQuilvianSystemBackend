using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxOvertimePlanDetailConfiguration : IEntityTypeConfiguration<TrxOvertimePlanDetail>
    {
        public void Configure(EntityTypeBuilder<TrxOvertimePlanDetail> entity)
        {
            entity.ToTable("TrxOvertimePlanDetail", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OvertimeDate)
                .HasColumnType("date");

            entity.Property(x => x.PlannedEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.PlannedStartAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.PlannedEndAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DayType)
                .HasMaxLength(30)
                .HasDefaultValue(OvertimeValueConstants.DayType.Workday)
                .IsRequired();

            entity.Property(x => x.OvertimeCategory)
                .HasMaxLength(40)
                .HasDefaultValue(OvertimeValueConstants.OvertimeCategory.AfterShift)
                .IsRequired();

            entity.Property(x => x.WorkDescription)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.Property(x => x.ValidationResultJson)
                .HasColumnType("jsonb");

            entity.Property(x => x.DetailStatus)
                .HasMaxLength(40)
                .HasDefaultValue(OvertimeValueConstants.PlanDetailStatus.Draft)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkLocation)
                .WithMany()
                .HasForeignKey(x => x.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RosterPeriod)
                .WithMany()
                .HasForeignKey(x => x.RosterPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ShiftAssignment)
                .WithMany()
                .HasForeignKey(x => x.ShiftAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkSchedule)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Shift)
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OvertimePolicy)
                .WithMany()
                .HasForeignKey(x => x.OvertimePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.OvertimePlanId,
                x.SequenceNumber
            })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.OvertimePlanId,
                x.WorkforceProfileId,
                x.PlannedStartAt
            })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.OvertimeDate,
                x.DetailStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DepartmentId,
                x.OvertimeDate,
                x.DetailStatus,
                x.IsDelete
            });
        }
    }
}
