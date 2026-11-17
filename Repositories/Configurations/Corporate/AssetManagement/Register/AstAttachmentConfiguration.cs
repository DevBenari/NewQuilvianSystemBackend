using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAttachment</c> — data-dictionary.md bagian 3.7 (BE-AST-003).</summary>
public sealed class AstAttachmentConfiguration : IEntityTypeConfiguration<AstAttachment>
{
    public void Configure(EntityTypeBuilder<AstAttachment> entity)
    {
        entity.ToTable("AstAttachment", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        entity.Property(x => x.StoredPath).HasMaxLength(500).IsRequired();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.OwnerType, x.OwnerId })
            .HasDatabaseName("IX_AstAttachment_Owner");

        entity.HasIndex(x => x.Sha256);
    }
}
