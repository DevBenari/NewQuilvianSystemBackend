using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxAttendanceDailyConfiguration : IEntityTypeConfiguration<TrxAttendanceDaily>
    {
        public void Configure(EntityTypeBuilder<TrxAttendanceDaily> builder)
        {
            builder.ToTable("TrxAttendanceDaily", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AttendanceDate)
                .HasColumnType("date");

            builder.Property(x => x.ScheduleSource)
                .HasMaxLength(40)
                .HasDefaultValue(AttendanceValueConstants.ScheduleSource.Unresolved)
                .IsRequired();

            builder.Property(x => x.ScheduleResolutionJson)
                .HasColumnType("jsonb")
                .IsRequired(false);

            builder.Property(x => x.ScheduledCheckInAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ScheduledCheckOutAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.FirstCheckInAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.LastCheckOutAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ProcessedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.PayrollProcessedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.UserType)
                .HasConversion<int>();

            builder.Property(x => x.AttendanceStatus)
                .HasMaxLength(50)
                .HasDefaultValue(AttendanceValueConstants.AttendanceStatus.Unprocessed)
                .IsRequired();

            builder.Property(x => x.ProcessingStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendanceProcessingStatus.Pending)
                .IsRequired();

            builder.Property(x => x.PayrollInputStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.PayrollInputStatus.Pending);

            builder.Property(x => x.ProcessingMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationAssignment)
                .WithMany()
                .HasForeignKey(x => x.OrganizationAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkLocation)
                .WithMany()
                .HasForeignKey(x => x.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkSchedule)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PrimaryShiftAssignment)
                .WithMany()
                .HasForeignKey(x => x.PrimaryShiftAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Shift)
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AttendancePolicy)
                .WithMany()
                .HasForeignKey(x => x.AttendancePolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.GracePeriodPolicy)
                .WithMany()
                .HasForeignKey(x => x.GracePeriodPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PayrollPeriod)
                .WithMany()
                .HasForeignKey(x => x.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AttendancePeriod)
                .WithMany(x => x.AttendanceDailies)
                .HasForeignKey(x => x.AttendancePeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Compatibility untuk record lama yang belum mempunyai WorkforceProfileId.
            builder.HasIndex(x => new { x.UserId, x.AttendanceDate })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Canonical unique key attendance HR.
            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate })
                .IsUnique()
                .HasFilter("\"WorkforceProfileId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => x.AttendancePeriodId)
                .HasFilter("\"AttendancePeriodId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => x.PrimaryShiftAssignmentId)
                .HasFilter("\"PrimaryShiftAssignmentId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.ScheduleSource, x.AttendanceDate });
            builder.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.AttendanceDate });
            builder.HasIndex(x => new { x.AttendanceStatus, x.AttendanceDate });
            builder.HasIndex(x => new { x.ProcessingStatus, x.PayrollInputStatus, x.AttendanceDate });
            builder.HasIndex(x => new { x.IsLate, x.IsEarlyLeave, x.HasMissingPunch, x.AttendanceDate });
        }
    }
}
