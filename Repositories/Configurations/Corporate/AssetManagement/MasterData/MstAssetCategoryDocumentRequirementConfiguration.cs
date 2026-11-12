using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetCategoryDocumentRequirement</c> — data-dictionary.md bagian 2.6 (BE-AST-002).</summary>
public sealed class MstAssetCategoryDocumentRequirementConfiguration : IEntityTypeConfiguration<MstAssetCategoryDocumentRequirement>
{
    public void Configure(EntityTypeBuilder<MstAssetCategoryDocumentRequirement> entity)
    {
        entity.ToTable("MstAssetCategoryDocumentRequirement", "public");
        entity.HasKey(x => x.Id);

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

        entity.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // CategoryId menjadi kolom pertama index unik ini sehingga tidak butuh index tunggal sendiri.
        entity.HasIndex(x => new { x.CategoryId, x.DocumentTypeId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetCategoryDocumentRequirement_Category_DocumentType");

        entity.HasIndex(x => x.DocumentTypeId);
    }
}
