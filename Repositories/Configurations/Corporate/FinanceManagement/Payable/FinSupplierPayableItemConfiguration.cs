using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinSupplierPayableItemConfiguration : IEntityTypeConfiguration<FinSupplierPayableItem>
{
    public void Configure(EntityTypeBuilder<FinSupplierPayableItem> entity)
    {
        entity.ToTable("FinSupplierPayableItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Quantity).HasPrecision(18, 2).HasDefaultValue(1m);
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        entity.Property(x => x.Amount).HasPrecision(18, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Payable)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PayableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
