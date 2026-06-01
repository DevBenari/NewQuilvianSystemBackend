using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstServiceUnitConfiguration : IEntityTypeConfiguration<MstServiceUnit>
    {
        public void Configure(EntityTypeBuilder<MstServiceUnit> entity)
        {
            entity.ToTable("MstServiceUnit", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ServiceUnitCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ServiceUnitName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ServiceUnitType)
                .HasConversion<int>()
                .HasDefaultValue(ServiceUnitType.Unknown)
                .IsRequired();

            entity.Property(x => x.ShortName)
                .HasMaxLength(50);

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.IsAvailableForRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForKiosk)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAvailableForAppointment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsQueueRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsDoctorRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsScreeningRequired)
                .HasDefaultValue(false);

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

            entity.HasIndex(x => x.ServiceUnitCode)
                .IsUnique();

            entity.HasIndex(x => x.ServiceUnitName);

            entity.HasIndex(x => x.ServiceUnitType);

            entity.HasIndex(x => new
            {
                x.ServiceUnitType,
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
