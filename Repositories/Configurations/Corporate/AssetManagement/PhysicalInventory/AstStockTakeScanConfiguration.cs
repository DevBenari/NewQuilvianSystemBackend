using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.PhysicalInventory;

public sealed class AstStockTakeScanConfiguration : IEntityTypeConfiguration<AstStockTakeScan>
{
    public void Configure(EntityTypeBuilder<AstStockTakeScan> entity)
    {
        entity.ToTable("AstStockTakeScan", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ScannedCode).HasMaxLength(60);
        entity.Property(x => x.IsManualEntry).HasDefaultValue(false);
        entity.Property(x => x.IsUnregisteredFinding).HasDefaultValue(false);
        entity.Property(x => x.FindingDescription).HasMaxLength(500);
        entity.Property(x => x.ObservedCondition).HasConversion<int>().IsRequired();
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.ScannedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsLatest).HasDefaultValue(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Session).WithMany(x => x.Scans).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.FoundLocation).WithMany().HasForeignKey(x => x.FoundLocationId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.PhotoAttachment).WithMany().HasForeignKey(x => x.PhotoAttachmentId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ScannedByEmployee).WithMany().HasForeignKey(x => x.ScannedByEmployeeId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.SessionId, x.AssetId, x.IsLatest })
            .HasDatabaseName("IX_AstStockTakeScan_Session_Asset_Latest");

        entity.HasIndex(x => x.AssetId);
        entity.HasIndex(x => x.FoundLocationId);
        entity.HasIndex(x => x.ScannedByEmployeeId);
    }
}
