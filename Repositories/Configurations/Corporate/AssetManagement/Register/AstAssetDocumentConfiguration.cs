using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAssetDocument</c> — data-dictionary.md bagian 3.6 (BE-AST-003).</summary>
public sealed class AstAssetDocumentConfiguration : IEntityTypeConfiguration<AstAssetDocument>
{
    public void Configure(EntityTypeBuilder<AstAssetDocument> entity)
    {
        entity.ToTable("AstAssetDocument", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.DocumentNumber).HasMaxLength(100).IsRequired();
        entity.Property(x => x.IssuedDate).HasColumnType("date");
        entity.Property(x => x.ExpiryDate).HasColumnType("date");
        entity.Property(x => x.HolderName).HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany(x => x.Documents).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.DocumentType).WithMany().HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.AssetId);
        entity.HasIndex(x => x.DocumentTypeId);
        entity.HasIndex(x => x.ExpiryDate);
    }
}
