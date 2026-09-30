using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinSupplierReturnDepositConfiguration : IEntityTypeConfiguration<FinSupplierReturnDeposit>
{
    public void Configure(EntityTypeBuilder<FinSupplierReturnDeposit> entity)
    {
        entity.ToTable("FinSupplierReturnDeposit", "public", table =>
        {
            table.HasCheckConstraint("CK_FinSupplierReturnDeposit_Available", "\"AvailableAmount\" >= 0");
            table.HasCheckConstraint("CK_FinSupplierReturnDeposit_Status", "\"Status\" IN ('AVAILABLE','EXHAUSTED','CANCELLED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        entity.Property(x => x.AvailableAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinSupplierReturnDepositStatuses.Available);
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

        // FIN-DEC-047: satu retur = satu deposit.
        entity.HasOne(x => x.SourceReturn)
            .WithOne(x => x.Deposit)
            .HasForeignKey<FinSupplierReturnDeposit>(x => x.SourceReturnId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.SourceReturnId).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinSupplierReturnDeposit_SourceReturnId");
        entity.HasIndex(x => new { x.SupplierId, x.Status }).HasDatabaseName("IX_FinSupplierReturnDeposit_Supplier_Status");
    }
}
