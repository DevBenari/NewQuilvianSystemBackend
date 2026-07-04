using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstQueueDisplayDeviceConfiguration : IEntityTypeConfiguration<MstQueueDisplayDevice>
    {
        public void Configure(EntityTypeBuilder<MstQueueDisplayDevice> entity)
        {
            entity.ToTable("MstQueueDisplayDevice", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NurseStationClusterId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            entity.Property(x => x.DisplayCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.DisplayDeviceType)
                .HasConversion<int>()
                .HasDefaultValue(QueueDisplayDeviceType.TvDisplay)
                .IsRequired();

            entity.Property(x => x.LayoutType)
                .HasConversion<int>()
                .HasDefaultValue(QueueDisplayLayoutType.ClusterBoard)
                .IsRequired();

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.RoomName)
                .HasMaxLength(50);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(50);

            entity.Property(x => x.MacAddress)
                .HasMaxLength(100);

            entity.Property(x => x.PairingCode)
                .HasMaxLength(100);

            entity.Property(x => x.DeviceToken)
                .HasMaxLength(200);

            entity.Property(x => x.EnableVoiceCalling)
                .HasDefaultValue(true);

            entity.Property(x => x.ShowPatientName)
                .HasDefaultValue(false);

            entity.Property(x => x.ShowDoctorName)
                .HasDefaultValue(true);

            entity.Property(x => x.ShowClinicName)
                .HasDefaultValue(true);

            entity.Property(x => x.RefreshIntervalSeconds)
                .HasDefaultValue(5);

            entity.Property(x => x.SessionExpireMinutes)
                .IsRequired(false);

            entity.Property(x => x.LastOnlineDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastOfflineDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastErrorMessage)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.NurseStationCluster)
                .WithMany()
                .HasForeignKey(x => x.NurseStationClusterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DisplayCode)
                .IsUnique();

            entity.HasIndex(x => x.NurseStationClusterId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.PairingCode);

            entity.HasIndex(x => x.DeviceToken);

            entity.HasIndex(x => new
            {
                x.NurseStationClusterId,
                x.DisplayDeviceType,
                x.LayoutType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.SessionExpireMinutes);
        }
    }

}
