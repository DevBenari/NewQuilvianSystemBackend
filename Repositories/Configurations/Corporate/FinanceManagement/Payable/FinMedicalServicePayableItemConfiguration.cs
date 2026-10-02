using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinMedicalServicePayableItemConfiguration : IEntityTypeConfiguration<FinMedicalServicePayableItem>
{
    public void Configure(EntityTypeBuilder<FinMedicalServicePayableItem> entity)
    {
        entity.ToTable("FinMedicalServicePayableItem", "public");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
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

        entity.HasIndex(x => x.PayableId)
            .HasDatabaseName("IX_FinMedicalServicePayableItem_PayableId");

        entity.HasIndex(x => x.SourceServiceFeeDetailId)
            .HasDatabaseName("IX_FinMedicalServicePayableItem_SourceServiceFeeDetailId");
    }
}
