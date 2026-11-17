using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstUtilizationItem</c> — data-dictionary.md bagian 3.8 (BE-AST-003).</summary>
public sealed class AstUtilizationItemConfiguration : IEntityTypeConfiguration<AstUtilizationItem>
{
    public void Configure(EntityTypeBuilder<AstUtilizationItem> entity)
    {
        entity.ToTable("AstUtilizationItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Utilization).WithMany(x => x.Items).HasForeignKey(x => x.UtilizationId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.UtilizationId, x.AssetId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstUtilizationItem_Utilization_Asset");

        entity.HasIndex(x => x.AssetId);
    }
}
