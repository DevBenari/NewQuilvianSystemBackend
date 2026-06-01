using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDrugUnitConversionConfiguration : IEntityTypeConfiguration<MstDrugUnitConversion>
    {
        public void Configure(EntityTypeBuilder<MstDrugUnitConversion> entity)
        {
            entity.ToTable("MstDrugUnitConversion", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DrugId)
                .IsRequired();

            entity.Property(x => x.ConversionCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ConversionName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.FromMeasurementId)
                .IsRequired();

            entity.Property(x => x.ToMeasurementId)
                .IsRequired();

            entity.Property(x => x.FromQuantity)
                .HasColumnType("numeric(18,6)")
                .HasDefaultValue(1);

            entity.Property(x => x.ToQuantity)
                .HasColumnType("numeric(18,6)")
                .HasDefaultValue(1);

            entity.Property(x => x.ConversionFactor)
                .HasColumnType("numeric(18,6)")
                .HasDefaultValue(1);

            entity.Property(x => x.ConversionType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.IsBidirectional)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForPurchase)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForStock)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForDispensing)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForPrescription)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForCompound)
                .HasDefaultValue(false);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

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

            entity.HasOne(x => x.Drug)
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FromMeasurement)
                .WithMany()
                .HasForeignKey(x => x.FromMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToMeasurement)
                .WithMany()
                .HasForeignKey(x => x.ToMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DrugId);

            entity.HasIndex(x => x.FromMeasurementId);

            entity.HasIndex(x => x.ToMeasurementId);

            entity.HasIndex(x => x.ConversionCode)
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.FromMeasurementId,
                x.ToMeasurementId
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.ConversionType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.IsForPurchase,
                x.IsForStock,
                x.IsForDispensing,
                x.IsForPrescription,
                x.IsForCompound,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}