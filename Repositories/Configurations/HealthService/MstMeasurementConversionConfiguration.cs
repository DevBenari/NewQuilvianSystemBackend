using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstMeasurementConversionConfiguration : IEntityTypeConfiguration<MstMeasurementConversion>
    {
        public void Configure(EntityTypeBuilder<MstMeasurementConversion> entity)
        {
            entity.ToTable("MstMeasurementConversion", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FromMeasurementId)
                .IsRequired();

            entity.Property(x => x.ToMeasurementId)
                .IsRequired();

            entity.Property(x => x.ConversionFactor)
                .HasColumnType("numeric(18,6)")
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.IsBidirectional)
                .HasDefaultValue(true);

            entity.Property(x => x.IsStandardConversion)
                .HasDefaultValue(true);

            entity.Property(x => x.ConversionGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.FormulaNote)
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

            entity.HasOne(x => x.FromMeasurement)
                .WithMany()
                .HasForeignKey(x => x.FromMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToMeasurement)
                .WithMany()
                .HasForeignKey(x => x.ToMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.FromMeasurementId);

            entity.HasIndex(x => x.ToMeasurementId);

            entity.HasIndex(x => new
            {
                x.FromMeasurementId,
                x.ToMeasurementId
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.ConversionGroupName,
                x.IsStandardConversion,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}