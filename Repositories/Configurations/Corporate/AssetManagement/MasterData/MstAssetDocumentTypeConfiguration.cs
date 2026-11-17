using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetDocumentType</c> — data-dictionary.md bagian 2.5 (BE-AST-002).</summary>
public sealed class MstAssetDocumentTypeConfiguration : IEntityTypeConfiguration<MstAssetDocumentType>
{
    public void Configure(EntityTypeBuilder<MstAssetDocumentType> entity)
    {
        entity.ToTable("MstAssetDocumentType", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.DocumentTypeCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.DocumentTypeName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.HasExpiry).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.DocumentTypeCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetDocumentType_DocumentTypeCode");
    }
}
