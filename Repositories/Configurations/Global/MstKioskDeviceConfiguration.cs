using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstKioskDeviceConfiguration : IEntityTypeConfiguration<MstKioskDevice>
    {
        public void Configure(EntityTypeBuilder<MstKioskDevice> entity)
        {
            entity.ToTable("MstKioskDevice", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DeviceCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DeviceName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.DeviceType)
                .HasConversion<int>()
                .HasDefaultValue(KioskDeviceType.Unknown)
                .IsRequired();

            entity.Property(x => x.DeviceStatus)
                .HasConversion<int>()
                .HasDefaultValue(KioskDeviceStatus.Active)
                .IsRequired();

            entity.Property(x => x.DefaultScannerProfileId)
                .IsRequired(false);

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.MacAddress)
                .HasMaxLength(100);

            entity.Property(x => x.IsAllowWalkIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowAppointment)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowInsuranceRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.SessionExpireMinutes)
                .IsRequired(false);

            entity.Property(x => x.LastOnlineAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastOfflineAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastErrorMessage)
                .HasMaxLength(250);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.UpdateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.DefaultScannerProfile)
                .WithMany()
                .HasForeignKey(x => x.DefaultScannerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DeviceCode)
                .IsUnique();

            entity.HasIndex(x => x.DeviceName);

            entity.HasIndex(x => x.DefaultScannerProfileId);

            entity.HasIndex(x => x.IpAddress)
                .HasFilter("\"IpAddress\" IS NOT NULL");

            entity.HasIndex(x => x.MacAddress)
                .HasFilter("\"MacAddress\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DeviceType,
                x.DeviceStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAllowWalkIn,
                x.IsAllowAppointment,
                x.IsAllowInsuranceRegistration,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.SessionExpireMinutes);
        }
    }
}