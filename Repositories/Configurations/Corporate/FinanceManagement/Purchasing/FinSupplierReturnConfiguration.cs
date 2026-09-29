using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinSupplierReturnConfiguration : IEntityTypeConfiguration<FinSupplierReturn>
{
    public void Configure(EntityTypeBuilder<FinSupplierReturn> entity)
    {
        entity.ToTable("FinSupplierReturn", "public", table =>
        {
            table.HasCheckConstraint("CK_FinSupplierReturn_Status", "\"Status\" IN ('DRAFT','CONFIRMED','CANCELLED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinSupplierReturnStatuses.Draft);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.PurchasingInvoice)
            .WithMany()
            .HasForeignKey(x => x.PurchasingInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.ReturnNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinSupplierReturn_ReturnNumber");
        entity.HasIndex(x => x.PurchasingInvoiceId).HasDatabaseName("IX_FinSupplierReturn_PurchasingInvoiceId");
    }
}
