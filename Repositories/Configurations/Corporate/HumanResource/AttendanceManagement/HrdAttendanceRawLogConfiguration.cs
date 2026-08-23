using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceRawLogConfiguration
        : IEntityTypeConfiguration<HrdAttendanceRawLog>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceRawLog> builder)
        {
            builder.ToTable("HrdAttendanceRawLog", "public");
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

            builder.Property(x => x.EventAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.ReceivedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.ProcessedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(x => x.UserType)
                .HasConversion<int?>();

            builder.Property(x => x.ExternalLogId).HasMaxLength(100);
            builder.Property(x => x.ExternalDeviceId).HasMaxLength(100);
            builder.Property(x => x.DeviceUserKey).HasMaxLength(100);

            builder.Property(x => x.EventType)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.RawLogEventType.Unknown)
                .IsRequired();

            builder.Property(x => x.SourceType)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.RawLogSourceType.Device)
                .IsRequired();

            builder.Property(x => x.Latitude).HasPrecision(10, 7);
            builder.Property(x => x.Longitude).HasPrecision(10, 7);
            builder.Property(x => x.AccuracyMeters).HasPrecision(12, 2);
            builder.Property(x => x.DistanceMeters).HasPrecision(12, 2);
            builder.Property(x => x.IpAddress).HasMaxLength(100);
            builder.Property(x => x.UserAgent).HasMaxLength(500);
            builder.Property(x => x.EventHash).HasMaxLength(128);
            builder.Property(x => x.RawPayloadJson).HasColumnType("jsonb");

            builder.Property(x => x.ProcessingStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.RawLogProcessingStatus.Pending)
                .IsRequired();

            builder.Property(x => x.ProcessingMessage).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

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

            builder.HasOne(x => x.AttendanceDevice)
                .WithMany()
                .HasForeignKey(x => x.AttendanceDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AttendanceLocation)
                .WithMany()
                .HasForeignKey(x => x.AttendanceLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProcessedAttendance)
                .WithMany()
                .HasForeignKey(x => x.ProcessedAttendanceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProcessedAttendanceDaily)
                .WithMany(x => x.RawLogs)
                .HasForeignKey(x => x.ProcessedAttendanceDailyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttendanceDeviceId, x.ExternalLogId })
                .IsUnique()
                .HasFilter("\"AttendanceDeviceId\" IS NOT NULL AND \"ExternalLogId\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => x.EventHash)
                .IsUnique()
                .HasFilter("\"EventHash\" IS NOT NULL AND \"IsDelete\" = false");

            builder.HasIndex(x => new { x.ProcessingStatus, x.ReceivedAt });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EventAt });
            builder.HasIndex(x => new { x.DeviceUserKey, x.EventAt });
            builder.HasIndex(x => new { x.SourceType, x.EventAt });
        }
    }
}
