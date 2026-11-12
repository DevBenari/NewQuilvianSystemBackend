using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAssetHandoverItem</c> — data-dictionary.md bagian 3.3 (BE-AST-003).</summary>
public sealed class AstAssetHandoverItemConfiguration : IEntityTypeConfiguration<AstAssetHandoverItem>
{
    public void Configure(EntityTypeBuilder<AstAssetHandoverItem> entity)
    {
        entity.ToTable("AstAssetHandoverItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ConditionAtHandover).HasConversion<int>().IsRequired();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Handover).WithMany(x => x.Items).HasForeignKey(x => x.HandoverId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.HandoverId, x.AssetId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstAssetHandoverItem_Handover_Asset");

        entity.HasIndex(x => x.AssetId);
    }
}
