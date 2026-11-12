using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetGroup</c> — data-dictionary.md bagian 2.3 dan DDL bagian 4 (BE-AST-002).</summary>
public sealed class MstAssetGroupConfiguration : IEntityTypeConfiguration<MstAssetGroup>
{
    public void Configure(EntityTypeBuilder<MstAssetGroup> entity)
    {
        entity.ToTable("MstAssetGroup", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.InventoryCode).HasMaxLength(11).IsRequired();
        entity.Property(x => x.GroupName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(500);
        entity.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Classification)
            .WithMany()
            .HasForeignKey(x => x.ClassificationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.CategoryId);
        entity.HasIndex(x => x.ClassificationId);

        entity.HasIndex(x => x.InventoryCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetGroup_InventoryCode");
    }
}
