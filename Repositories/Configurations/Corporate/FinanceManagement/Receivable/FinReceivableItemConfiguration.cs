using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableItemConfiguration : IEntityTypeConfiguration<FinReceivableItem>
{
    public void Configure(EntityTypeBuilder<FinReceivableItem> entity)
    {
        entity.ToTable("FinReceivableItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receivable)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
