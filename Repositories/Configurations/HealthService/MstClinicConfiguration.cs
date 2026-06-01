using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstClinicConfiguration : IEntityTypeConfiguration<MstClinic>
    {
        public void Configure(EntityTypeBuilder<MstClinic> entity)
        {
            entity.ToTable("MstClinic", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClinicCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ClinicName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ClinicType)
                .HasConversion<int>()
                .HasDefaultValue(ClinicType.Unknown)
                .IsRequired();

            entity.Property(x => x.ShortName)
                .HasMaxLength(50);

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.RoomName)
                .HasMaxLength(50);

            entity.Property(x => x.IsAvailableForRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForKiosk)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForAppointment)
                .HasDefaultValue(true);

            entity.Property(x => x.IsDoctorRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsScreeningRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsQueueRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.DefaultEstimatedServiceMinutes)
                .HasDefaultValue(15);

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

            entity.HasIndex(x => x.ClinicCode)
                .IsUnique();

            entity.HasIndex(x => x.ClinicName);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicName
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAvailableForRegistration,
                x.IsAvailableForKiosk,
                x.IsAvailableForAppointment,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
