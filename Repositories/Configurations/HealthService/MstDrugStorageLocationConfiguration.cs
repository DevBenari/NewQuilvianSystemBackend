using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDrugStorageLocationConfiguration : IEntityTypeConfiguration<MstDrugStorageLocation>
    {
        public void Configure(EntityTypeBuilder<MstDrugStorageLocation> entity)
        {
            entity.ToTable("MstDrugStorageLocation", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ParentStorageLocationId)
                .IsRequired(false);

            entity.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.RoomId)
                .IsRequired(false);

            entity.Property(x => x.StorageLocationCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.StorageLocationName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.StorageLocationType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.LocationGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(100);

            entity.Property(x => x.RoomName)
                .HasMaxLength(100);

            entity.Property(x => x.RackCode)
                .HasMaxLength(100);

            entity.Property(x => x.ShelfCode)
                .HasMaxLength(100);

            entity.Property(x => x.BinCode)
                .HasMaxLength(100);

            entity.Property(x => x.MinimumTemperatureCelsius)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.MaximumTemperatureCelsius)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.MinimumHumidityPercent)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.MaximumHumidityPercent)
                .HasColumnType("numeric(8,2)")
                .IsRequired(false);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.IsMainWarehouse)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPharmacyLocation)
                .HasDefaultValue(true);

            entity.Property(x => x.IsColdChain)
                .HasDefaultValue(false);

            entity.Property(x => x.IsControlledDrugStorage)
                .HasDefaultValue(false);

            entity.Property(x => x.IsHighAlertStorage)
                .HasDefaultValue(false);

            entity.Property(x => x.IsQuarantineLocation)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAllowReceiving)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowDispensing)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowTransferIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowTransferOut)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.ParentStorageLocation)
                .WithMany(x => x.ChildStorageLocations)
                .HasForeignKey(x => x.ParentStorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ParentStorageLocationId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.RoomId);

            entity.HasIndex(x => x.StorageLocationCode)
                .IsUnique();

            entity.HasIndex(x => x.StorageLocationName);

            entity.HasIndex(x => x.StorageLocationType);

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.StorageLocationType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ParentStorageLocationId,
                x.StorageLocationName,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsDefault,
                x.IsMainWarehouse,
                x.IsPharmacyLocation,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsColdChain,
                x.IsControlledDrugStorage,
                x.IsHighAlertStorage,
                x.IsQuarantineLocation,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAllowReceiving,
                x.IsAllowDispensing,
                x.IsAllowTransferIn,
                x.IsAllowTransferOut,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}