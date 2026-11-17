using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAssetHistory</c> — data-dictionary.md bagian 3.5 (BE-AST-003).</summary>
public sealed class AstAssetHistoryConfiguration : IEntityTypeConfiguration<AstAssetHistory>
{
    public void Configure(EntityTypeBuilder<AstAssetHistory> entity)
    {
        entity.ToTable("AstAssetHistory", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EventType).HasConversion<int>().IsRequired();
        entity.Property(x => x.SubModule).HasMaxLength(20).IsRequired();
        entity.Property(x => x.FromValue).HasMaxLength(250);
        entity.Property(x => x.ToValue).HasMaxLength(250);
        entity.Property(x => x.SourceDocumentType).HasMaxLength(40);
        entity.Property(x => x.SourceDocumentNumber).HasMaxLength(30);
        entity.Property(x => x.Reason).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);

        // AssetId menjadi kolom pertama index ini sehingga tidak butuh index tunggal sendiri.
        entity.HasIndex(x => new { x.AssetId, x.OccurredAt })
            .HasDatabaseName("IX_AstAssetHistory_Asset_OccurredAt");

        entity.HasIndex(x => x.EventType);
        entity.HasIndex(x => x.SubModule);
        entity.HasIndex(x => x.SourceDocumentId);
        entity.HasIndex(x => x.ActorUserId);
    }
}
