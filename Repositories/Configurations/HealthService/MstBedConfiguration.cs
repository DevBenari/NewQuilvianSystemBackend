using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstBedConfiguration : IEntityTypeConfiguration<MstBed>
    {
        public void Configure(EntityTypeBuilder<MstBed> entity)
        {
            entity.ToTable("MstBed", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoomId)
                .IsRequired();

            entity.Property(x => x.BedCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.BedName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.BedNumber)
                .HasMaxLength(50);

            entity.Property(x => x.BedStatus)
                .HasConversion<int>()
                .HasDefaultValue(BedStatus.Available)
                .IsRequired();

            entity.Property(x => x.IsForMale)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForFemale)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForNewborn)
                .HasDefaultValue(false);

            entity.Property(x => x.IsIsolationBed)
                .HasDefaultValue(false);

            entity.Property(x => x.IsIntensiveCareBed)
                .HasDefaultValue(false);

            entity.Property(x => x.IsOdcBed)
                .HasDefaultValue(false);

            entity.Property(x => x.IsReservable)
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

            entity.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.BedCode)
                .IsUnique();

            entity.HasIndex(x => x.RoomId);

            entity.HasIndex(x => new
            {
                x.RoomId,
                x.BedNumber
            }).IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.RoomId,
                x.BedStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.BedStatus,
                x.IsReservable,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
