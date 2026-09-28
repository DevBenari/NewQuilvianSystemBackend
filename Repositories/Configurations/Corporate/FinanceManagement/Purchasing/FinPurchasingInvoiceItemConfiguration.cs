using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinPurchasingInvoiceItemConfiguration : IEntityTypeConfiguration<FinPurchasingInvoiceItem>
{
    public void Configure(EntityTypeBuilder<FinPurchasingInvoiceItem> entity)
    {
        entity.ToTable("FinPurchasingInvoiceItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Quantity).HasPrecision(18, 2);
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        entity.Property(x => x.LineTotal).HasPrecision(18, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.PurchasingInvoice)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PurchasingInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PurchasingInvoiceId).HasDatabaseName("IX_FinPurchasingInvoiceItem_PurchasingInvoiceId");
    }
}
