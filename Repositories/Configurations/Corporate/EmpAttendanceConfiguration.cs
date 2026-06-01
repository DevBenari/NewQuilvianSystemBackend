using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class EmpAttendanceConfiguration : IEntityTypeConfiguration<EmpAttendance>
    {
        public void Configure(EntityTypeBuilder<EmpAttendance> entity)
        {
            entity.ToTable("EmpAttendance", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired(false);

            entity.Property(x => x.WorkScheduleId)
                .IsRequired(false);

            entity.Property(x => x.WorkScheduleAssignmentId)
                .IsRequired(false);

            entity.Property(x => x.AttendanceDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.CheckInAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.CheckOutAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.WorkStartTime)
                .HasColumnType("time without time zone")
                .IsRequired(false);

            entity.Property(x => x.WorkEndTime)
                .HasColumnType("time without time zone")
                .IsRequired(false);

            entity.Property(x => x.IsOvernightSchedule)
                .HasDefaultValue(false);

            entity.Property(x => x.ScheduledCheckInAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ScheduledCheckOutAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CheckInToleranceMinutes)
                .HasDefaultValue(0);

            entity.Property(x => x.CheckOutToleranceMinutes)
                .HasDefaultValue(0);

            entity.Property(x => x.IsLate)
                .HasDefaultValue(false);

            entity.Property(x => x.LateMinutes)
                .HasDefaultValue(0);

            entity.Property(x => x.AttendanceStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Present")
                .IsRequired();

            entity.Property(x => x.UserType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.CheckInSource)
                .HasMaxLength(50)
                .HasDefaultValue("Login")
                .IsRequired();

            entity.Property(x => x.CheckOutSource)
                .HasMaxLength(50);

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .HasDefaultValue("CheckedIn")
                .IsRequired();

            entity.Property(x => x.GeofenceBypassReason)
                .HasMaxLength(250);

            entity.Property(x => x.CheckInIpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.CheckOutIpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.CheckInUserAgent)
                .HasMaxLength(500);

            entity.Property(x => x.CheckOutUserAgent)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkSchedule)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkScheduleAssignment)
                .WithMany()
                .HasForeignKey(x => x.WorkScheduleAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.AttendanceDate
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EmployeeId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.WorkScheduleId);

            entity.HasIndex(x => x.WorkScheduleAssignmentId);

            entity.HasIndex(x => x.AttendanceDate);

            entity.HasIndex(x => x.Status);

            entity.HasIndex(x => x.AttendanceStatus);

            entity.HasIndex(x => x.IsLate);

            entity.HasIndex(x => x.IsOvernightSchedule);

            entity.HasIndex(x => x.IsGeofenceBypassed);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.AttendanceDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkScheduleId,
                x.AttendanceDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkScheduleAssignmentId,
                x.AttendanceDate,
                x.IsDelete
            });
        }
    }
}
