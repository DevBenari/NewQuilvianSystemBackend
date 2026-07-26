using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxAttendanceConfiguration : IEntityTypeConfiguration<TrxAttendance>
    {
        public void Configure(EntityTypeBuilder<TrxAttendance> builder)
        {
            builder.ToTable("TrxAttendance", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AttendanceDate).HasColumnType("date");
            builder.Property(x => x.CheckInAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CheckOutAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.WorkStartTime).HasColumnType("time without time zone");
            builder.Property(x => x.WorkEndTime).HasColumnType("time without time zone");
            builder.Property(x => x.ScheduledCheckInAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ScheduledCheckOutAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UserType).HasConversion<int>();
            builder.Property(x => x.AttendanceStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.GeofenceBypassReason).HasMaxLength(250);
            builder.Property(x => x.CheckInSource).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CheckOutSource).HasMaxLength(50);
            builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CheckInIpAddress).HasMaxLength(100);
            builder.Property(x => x.CheckOutIpAddress).HasMaxLength(100);
            builder.Property(x => x.CheckInUserAgent).HasMaxLength(500);
            builder.Property(x => x.CheckOutUserAgent).HasMaxLength(500);
            builder.Property(x => x.ProcessingStatus).HasMaxLength(30).HasDefaultValue("Pending");
            builder.Property(x => x.ProcessingMessage).HasMaxLength(500);

            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkScheduleAssignment).WithMany().HasForeignKey(x => x.WorkScheduleAssignmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendancePolicy).WithMany().HasForeignKey(x => x.AttendancePolicyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.GracePeriodPolicy).WithMany().HasForeignKey(x => x.GracePeriodPolicyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceLocation).WithMany().HasForeignKey(x => x.AttendanceLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CheckInDevice).WithMany().HasForeignKey(x => x.CheckInDeviceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CheckOutDevice).WithMany().HasForeignKey(x => x.CheckOutDeviceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceDaily).WithOne(x => x.Attendance).HasForeignKey<TrxAttendance>(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserId, x.AttendanceDate }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate });
            builder.HasIndex(x => new { x.AttendanceStatus, x.AttendanceDate });
            builder.HasIndex(x => new { x.Status, x.IsProcessed, x.AttendanceDate });
            builder.HasIndex(x => x.AttendanceDailyId).IsUnique().HasFilter("\"AttendanceDailyId\" IS NOT NULL AND \"IsDelete\" = false");
        }
    }
}
