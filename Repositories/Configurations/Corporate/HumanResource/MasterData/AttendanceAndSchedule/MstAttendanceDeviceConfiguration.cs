using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstAttendanceDeviceConfiguration : IEntityTypeConfiguration<MstAttendanceDevice>
    {
        public void Configure(EntityTypeBuilder<MstAttendanceDevice> entity)
        {
            entity.ToTable("MstAttendanceDevice", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttendanceDeviceCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AttendanceDeviceName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DeviceType).HasMaxLength(50).HasDefaultValue("Fingerprint").IsRequired();
            entity.Property(x => x.SerialNumber).HasMaxLength(100);
            entity.Property(x => x.Manufacturer).HasMaxLength(100);
            entity.Property(x => x.ModelName).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.MacAddress).HasMaxLength(50);
            entity.Property(x => x.IntegrationProvider).HasMaxLength(100);
            entity.Property(x => x.ExternalDeviceId).HasMaxLength(100);
            entity.Property(x => x.LastSyncAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsOnline).HasDefaultValue(false);
            entity.Property(x => x.IsPrimary).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.AttendanceLocation)
                .WithMany(x => x.AttendanceDevices)
                .HasForeignKey(x => x.AttendanceLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AttendanceDeviceCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.SerialNumber)
                .IsUnique()
                .HasFilter("\"SerialNumber\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.ExternalDeviceId)
                .HasFilter("\"ExternalDeviceId\" IS NOT NULL");

            entity.HasIndex(x => new { x.AttendanceLocationId, x.HospitalSiteId });
            entity.HasIndex(x => new { x.DeviceType, x.IsOnline, x.IsPrimary, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstAttendanceDevice> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
