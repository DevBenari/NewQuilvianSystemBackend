using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDrugStockPolicyConfiguration : IEntityTypeConfiguration<MstDrugStockPolicy>
    {
        public void Configure(EntityTypeBuilder<MstDrugStockPolicy> entity)
        {
            entity.ToTable("MstDrugStockPolicy", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DrugId)
                .IsRequired();

            entity.Property(x => x.StorageLocationId)
                .IsRequired(false);

            entity.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.StockUnitMeasurementId)
                .IsRequired();

            entity.Property(x => x.StockPolicyCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.StockPolicyName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.MinimumStockQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0);

            entity.Property(x => x.MaximumStockQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0);

            entity.Property(x => x.ReorderPointQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0);

            entity.Property(x => x.ReorderQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0);

            entity.Property(x => x.SafetyStockQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0);

            entity.Property(x => x.CriticalStockQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0);

            entity.Property(x => x.LeadTimeDays)
                .HasDefaultValue(0);

            entity.Property(x => x.ExpiryWarningDays)
                .HasDefaultValue(90);

            entity.Property(x => x.NearExpiryWarningDays)
                .HasDefaultValue(30);

            entity.Property(x => x.IsAutoReorderEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAllowNegativeStock)
                .HasDefaultValue(false);

            entity.Property(x => x.IsBatchRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsExpiryDateRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsStockOpnameRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.StockOpnameIntervalDays)
                .HasDefaultValue(30);

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

            entity.HasOne(x => x.StorageLocation)
                .WithMany()
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StockUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.StockUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DrugId);

            entity.HasIndex(x => x.StorageLocationId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.StockUnitMeasurementId);

            entity.HasIndex(x => x.StockPolicyCode)
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.StorageLocationId
            })
            .IsUnique()
            .HasFilter("\"StorageLocationId\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.DrugId)
                .IsUnique()
                .HasFilter("\"StorageLocationId\" IS NULL AND \"ServiceUnitId\" IS NULL AND \"ClinicId\" IS NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.ServiceUnitId,
                x.ClinicId
            })
            .IsUnique()
            .HasFilter("\"StorageLocationId\" IS NULL AND \"ClinicId\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.StockUnitMeasurementId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.MinimumStockQuantity,
                x.MaximumStockQuantity,
                x.ReorderPointQuantity,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAutoReorderEnabled,
                x.IsAllowNegativeStock,
                x.IsBatchRequired,
                x.IsExpiryDateRequired,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ExpiryWarningDays,
                x.NearExpiryWarningDays,
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