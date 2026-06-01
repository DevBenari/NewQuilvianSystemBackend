using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstRoomConfiguration : IEntityTypeConfiguration<MstRoom>
    {
        public void Configure(EntityTypeBuilder<MstRoom> entity)
        {
            entity.ToTable("MstRoom", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.RoomCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RoomName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.RoomType)
                .HasConversion<int>()
                .HasDefaultValue(RoomType.Unknown)
                .IsRequired();

            entity.Property(x => x.RoomNumber)
                .HasMaxLength(50);

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.Capacity)
                .HasDefaultValue(1);

            entity.Property(x => x.IsForMale)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForFemale)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForNewborn)
                .HasDefaultValue(false);

            entity.Property(x => x.IsIsolationRoom)
                .HasDefaultValue(false);

            entity.Property(x => x.IsIntensiveCare)
                .HasDefaultValue(false);

            entity.Property(x => x.IsOdcRoom)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAvailableForAdmission)
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

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RoomCode)
                .IsUnique();

            entity.HasIndex(x => x.RoomName);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.PatientClassId);

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.PatientClassId,
                x.RoomType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsIsolationRoom,
                x.IsIntensiveCare,
                x.IsOdcRoom,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
