using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Movement;

public sealed class AstTransferItemConfiguration : IEntityTypeConfiguration<AstTransferItem>
{
    public void Configure(EntityTypeBuilder<AstTransferItem> entity)
    {
        entity.ToTable("AstTransferItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ConditionAtSubmit).HasConversion<int>().IsRequired();
        entity.Property(x => x.ConditionAtReceipt).HasConversion<int>();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Transfer).WithMany(x => x.Items).HasForeignKey(x => x.TransferId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.TransferId, x.AssetId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstTransferItem_Transfer_Asset");

        entity.HasIndex(x => x.AssetId);
    }
}
