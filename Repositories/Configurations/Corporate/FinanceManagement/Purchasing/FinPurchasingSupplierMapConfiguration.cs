using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingSupplierMapConfiguration : IEntityTypeConfiguration<FinPurchasingSupplierMap>
{
    public void Configure(EntityTypeBuilder<FinPurchasingSupplierMap> entity)
    {
        entity.ToTable("FinPurchasingSupplierMap", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPurchasingSupplierMap_SourceSystem", "\"SourceSystem\" IN ('BLM')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SourceSystem).HasMaxLength(20).IsRequired().HasDefaultValue(FinPurchasingSourceSystems.Blm);
        entity.Property(x => x.ExternalSupplierName).HasMaxLength(300);
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DEC-014: SupplierId merujuk MstSupplier existing milik Administrator.
        entity.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIN-VAL-310: satu pemetaan aktif per supplier BLM; pemetaan lama dinonaktifkan, bukan ditimpa.
        entity.HasIndex(x => new { x.SourceSystem, x.ExternalSupplierId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinPurchasingSupplierMap_SourceSystem_ExternalSupplierId");
        entity.HasIndex(x => x.SupplierId).HasDatabaseName("IX_FinPurchasingSupplierMap_SupplierId");
    }
}
