using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstMeasurementConfiguration : IEntityTypeConfiguration<MstMeasurement>
    {
        public void Configure(EntityTypeBuilder<MstMeasurement> entity)
        {
            entity.ToTable("MstMeasurement", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.MeasurementCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.MeasurementName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.MeasurementSymbol)
                .HasMaxLength(50);

            entity.Property(x => x.MeasurementType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.MeasurementGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.IsBaseUnit)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDecimalAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.DecimalPrecision)
                .HasDefaultValue(2);

            entity.Property(x => x.IsForDrug)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForLaboratory)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForVitalSign)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForGeneralUse)
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

            entity.HasIndex(x => x.MeasurementCode)
                .IsUnique();

            entity.HasIndex(x => x.MeasurementName);

            entity.HasIndex(x => x.MeasurementSymbol);

            entity.HasIndex(x => x.MeasurementType);

            entity.HasIndex(x => new
            {
                x.MeasurementType,
                x.MeasurementGroupName,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsForDrug,
                x.IsForLaboratory,
                x.IsForVitalSign,
                x.IsForGeneralUse,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}