using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxBusinessTripAttendanceConfiguration : IEntityTypeConfiguration<TrxBusinessTripAttendance>
    {
        public void Configure(EntityTypeBuilder<TrxBusinessTripAttendance> builder)
        {
            builder.ToTable("TrxBusinessTripAttendance", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ReferenceType).HasMaxLength(50);
            builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
            builder.Property(x => x.AttendanceDate).HasColumnType("date");
            builder.Property(x => x.PlannedStartAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.PlannedEndAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualStartAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualEndAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Origin).HasMaxLength(250);
            builder.Property(x => x.Destination).HasMaxLength(250);
            builder.Property(x => x.ActivityDescription).HasMaxLength(500);
            builder.Property(x => x.AttendanceStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EvidenceFilePath).HasMaxLength(500);
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceLocation).WithMany().HasForeignKey(x => x.AttendanceLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate }).IsUnique().HasFilter("\"IsDelete\" = false AND \"AttendanceStatus\" <> 'Cancelled'");
            builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
            builder.HasIndex(x => new { x.AttendanceStatus, x.AttendanceDate });
        }
    }
}
