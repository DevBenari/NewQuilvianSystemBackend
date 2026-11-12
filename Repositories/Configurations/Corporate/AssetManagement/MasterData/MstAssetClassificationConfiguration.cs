using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetClassification</c> — data-dictionary.md bagian 2.2 (BE-AST-002).</summary>
public sealed class MstAssetClassificationConfiguration : IEntityTypeConfiguration<MstAssetClassification>
{
    public void Configure(EntityTypeBuilder<MstAssetClassification> entity)
    {
        entity.ToTable("MstAssetClassification", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ClassificationCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.ClassificationName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.IsMedical).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.ClassificationCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetClassification_ClassificationCode");
    }
}
