using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDrugSupplierConfiguration : IEntityTypeConfiguration<MstDrugSupplier>
    {
        public void Configure(EntityTypeBuilder<MstDrugSupplier> entity)
        {
            entity.ToTable("MstDrugSupplier", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DrugId)
                .IsRequired();

            entity.Property(x => x.SupplierId)
                .IsRequired();

            entity.Property(x => x.DrugSupplierCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SupplierDrugCode)
                .HasMaxLength(50);

            entity.Property(x => x.SupplierDrugName)
                .HasMaxLength(200);

            entity.Property(x => x.PurchaseUnitMeasurementId)
                .IsRequired(false);

            entity.Property(x => x.MinimumOrderQuantity)
                .HasColumnType("numeric(18,6)")
                .HasDefaultValue(1);

            entity.Property(x => x.OrderMultipleQuantity)
                .HasColumnType("numeric(18,6)")
                .HasDefaultValue(1);

            entity.Property(x => x.MaximumOrderQuantity)
                .HasColumnType("numeric(18,6)")
                .IsRequired(false);

            entity.Property(x => x.MinimumPurchaseAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.DefaultPurchasePrice)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.LastPurchasePrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.ContractPurchasePrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.DiscountPercent)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.TaxPercent)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.LeadTimeDays)
                .HasDefaultValue(0);

            entity.Property(x => x.IsPreferredSupplier)
                .HasDefaultValue(false);

            entity.Property(x => x.IsContractSupplier)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDefaultForPurchase)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAllowPurchase)
                .HasDefaultValue(true);

            entity.Property(x => x.IsRequireQuotation)
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

            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.DrugSuppliers)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PurchaseUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.PurchaseUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DrugSupplierCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.DrugId);

            entity.HasIndex(x => x.SupplierId);

            entity.HasIndex(x => x.PurchaseUnitMeasurementId);

            entity.HasIndex(x => x.SupplierDrugCode);

            entity.HasIndex(x => new
            {
                x.SupplierId,
                x.SupplierDrugCode
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false AND \"SupplierDrugCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.SupplierId
            })
            .IsUnique()
            .HasDatabaseName("IX_MstDrugSupplier_Drug_Supplier_NullPurchaseUnit")
            .HasFilter("\"IsDelete\" = false AND \"PurchaseUnitMeasurementId\" IS NULL");

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.SupplierId,
                x.PurchaseUnitMeasurementId
            })
            .IsUnique()
            .HasDatabaseName("IX_MstDrugSupplier_Drug_Supplier_PurchaseUnit")
            .HasFilter("\"IsDelete\" = false AND \"PurchaseUnitMeasurementId\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.IsPreferredSupplier,
                x.IsDefaultForPurchase,
                x.IsAllowPurchase,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.SupplierId,
                x.IsContractSupplier,
                x.IsAllowPurchase,
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
