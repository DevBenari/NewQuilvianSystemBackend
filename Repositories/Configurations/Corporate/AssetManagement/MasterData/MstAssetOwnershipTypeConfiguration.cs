using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetOwnershipType</c> — data-dictionary.md bagian 2.4 (BE-AST-002).</summary>
public sealed class MstAssetOwnershipTypeConfiguration : IEntityTypeConfiguration<MstAssetOwnershipType>
{
    public void Configure(EntityTypeBuilder<MstAssetOwnershipType> entity)
    {
        entity.ToTable("MstAssetOwnershipType", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnershipCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.OwnershipName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.IsCapitalizable).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.OwnershipCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetOwnershipType_OwnershipCode");
    }
}
