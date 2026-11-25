using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.PhysicalInventory;

public sealed class AstStockTakeResultConfiguration : IEntityTypeConfiguration<AstStockTakeResult>
{
    public void Configure(EntityTypeBuilder<AstStockTakeResult> entity)
    {
        entity.ToTable("AstStockTakeResult", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Category).HasConversion<int>().IsRequired();
        entity.Property(x => x.IsLocationMismatch).HasDefaultValue(false);
        entity.Property(x => x.IsConditionMismatch).HasDefaultValue(false);
        entity.Property(x => x.IsForeignAsset).HasDefaultValue(false);
        entity.Property(x => x.ExpectedCondition).HasConversion<int>();
        entity.Property(x => x.ObservedCondition).HasConversion<int>();
        entity.Property(x => x.TeamNote).HasMaxLength(1000);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Session).WithMany(x => x.Results).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Scan).WithMany().HasForeignKey(x => x.ScanId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ExpectedLocation).WithMany().HasForeignKey(x => x.ExpectedLocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.FoundLocation).WithMany().HasForeignKey(x => x.FoundLocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.SessionId, x.Category })
            .HasDatabaseName("IX_AstStockTakeResult_Session_Category");

        entity.HasIndex(x => x.AssetId);
        entity.HasIndex(x => x.ScanId);
    }
}
