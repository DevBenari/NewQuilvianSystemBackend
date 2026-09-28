using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

/// <summary>
/// BENTUK REVISI 5 (02-backend-architecture.md §D.2) — menggantikan rancangan REVISI 4 yang
/// menunjuk PurchasingInvoiceId (erd/data-dictionary.md bagian C.11, DIGANTIKAN).
/// </summary>
public sealed class FinSupplierReturnDepositUsageConfiguration : IEntityTypeConfiguration<FinSupplierReturnDepositUsage>
{
    public void Configure(EntityTypeBuilder<FinSupplierReturnDepositUsage> entity)
    {
        entity.ToTable("FinSupplierReturnDepositUsage", "public", table =>
        {
            table.HasCheckConstraint("CK_FinSupplierReturnDepositUsage_Amount", "\"UsedAmount\" > 0");
            table.HasCheckConstraint("CK_FinSupplierReturnDepositUsage_Status", "\"Status\" IN ('RESERVED','APPLIED','RELEASED')");
            table.HasCheckConstraint("CK_FinSupplierReturnDepositUsage_ReleasedAt",
                "(\"Status\" = 'RELEASED') = (\"ReleasedAt\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.UsedAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinSupplierReturnDepositUsageStatuses.Reserved);
        entity.Property(x => x.UsedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReleasedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.SupplierReturnDeposit)
            .WithMany(x => x.Usages)
            .HasForeignKey(x => x.SupplierReturnDepositId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Payment)
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIN-DES-046: deposit yang sama tidak boleh aktif dua kali dalam satu pembayaran.
        entity.HasIndex(x => new { x.PaymentId, x.SupplierReturnDepositId })
            .IsUnique()
            .HasFilter("\"Status\" <> 'RELEASED' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinSupplierReturnDepositUsage_ActivePerPayment");
        entity.HasIndex(x => x.SupplierReturnDepositId).HasDatabaseName("IX_FinSupplierReturnDepositUsage_SupplierReturnDepositId");
        entity.HasIndex(x => x.PaymentId).HasDatabaseName("IX_FinSupplierReturnDepositUsage_PaymentId");
    }
}
