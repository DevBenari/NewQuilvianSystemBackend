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

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.MacAddress)
                .HasMaxLength(100);

            entity.Property(x => x.SerialNumber)
                .HasMaxLength(100);

            entity.Property(x => x.DeviceModel)
                .HasMaxLength(100);

            entity.Property(x => x.VendorName)
                .HasMaxLength(100);

            entity.Property(x => x.IsAvailableForRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForCheckIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForPayment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAllowWalkIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowAppointment)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowInsuranceRegistration)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DefaultScannerProfile)
                .WithMany()
                .HasForeignKey(x => x.DefaultScannerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DeviceCode)
                .IsUnique();

            entity.HasIndex(x => x.DeviceName);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.DefaultScannerProfileId);

            entity.HasIndex(x => x.SerialNumber)
                .HasFilter("\"SerialNumber\" IS NOT NULL");

            entity.HasIndex(x => x.IpAddress)
                .HasFilter("\"IpAddress\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DeviceType,
                x.DeviceStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.IsAvailableForRegistration,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
