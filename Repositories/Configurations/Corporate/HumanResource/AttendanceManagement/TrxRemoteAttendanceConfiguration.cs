using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxRemoteAttendanceConfiguration : IEntityTypeConfiguration<TrxRemoteAttendance>
    {
        public void Configure(EntityTypeBuilder<TrxRemoteAttendance> builder)
        {
            builder.ToTable("TrxRemoteAttendance", "public");
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
            builder.Property(x => x.LocationDescription).HasMaxLength(500);
            builder.Property(x => x.CheckInLatitude).HasPrecision(10, 7);
            builder.Property(x => x.CheckInLongitude).HasPrecision(10, 7);
            builder.Property(x => x.CheckInAccuracyMeters).HasPrecision(12, 2);
            builder.Property(x => x.CheckOutLatitude).HasPrecision(10, 7);
            builder.Property(x => x.CheckOutLongitude).HasPrecision(10, 7);
            builder.Property(x => x.CheckOutAccuracyMeters).HasPrecision(12, 2);
            builder.Property(x => x.CheckInIpAddress).HasMaxLength(100);
            builder.Property(x => x.CheckOutIpAddress).HasMaxLength(100);
            builder.Property(x => x.CheckInUserAgent).HasMaxLength(500);
            builder.Property(x => x.CheckOutUserAgent).HasMaxLength(500);
            builder.Property(x => x.ApprovalStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.EvidenceFilePath).HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceLocation).WithMany().HasForeignKey(x => x.AttendanceLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendancePolicy).WithMany().HasForeignKey(x => x.AttendancePolicyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate }).IsUnique().HasFilter("\"IsDelete\" = false AND \"ApprovalStatus\" <> 'Cancelled'");
            builder.HasIndex(x => new { x.ApprovalStatus, x.AttendanceDate });
        }
    }
}
