using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Purchasing;

public sealed class FinSupplierReturnItemConfiguration : IEntityTypeConfiguration<FinSupplierReturnItem>
{
    public void Configure(EntityTypeBuilder<FinSupplierReturnItem> entity)
    {
        entity.ToTable("FinSupplierReturnItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Quantity).HasPrecision(18, 2);
        entity.Property(x => x.LineTotal).HasPrecision(18, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.SupplierReturn)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SupplierReturnId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.SupplierReturnId).HasDatabaseName("IX_FinSupplierReturnItem_SupplierReturnId");
    }
}
