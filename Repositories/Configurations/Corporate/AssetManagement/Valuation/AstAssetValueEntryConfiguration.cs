using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstAssetValueEntryConfiguration : IEntityTypeConfiguration<AstAssetValueEntry>
{
    public void Configure(EntityTypeBuilder<AstAssetValueEntry> entity)
    {
        entity.ToTable("AstAssetValueEntry", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EntryType).HasConversion<int>().IsRequired();
        entity.Property(x => x.Period).HasColumnType("date");
        entity.Property(x => x.CostDelta).HasPrecision(18, 2);
        entity.Property(x => x.AccumulatedDepreciationDelta).HasPrecision(18, 2);
        entity.Property(x => x.AccumulatedImpairmentDelta).HasPrecision(18, 2);
        entity.Property(x => x.SourceDocumentType).HasMaxLength(40);
        entity.Property(x => x.SourceDocumentNumber).HasMaxLength(30);
        entity.Property(x => x.Note).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.AssetId, x.Period })
            .HasDatabaseName("IX_AstAssetValueEntry_Asset_Period");

        entity.HasIndex(x => x.EntryType);
        entity.HasIndex(x => x.SourceDocumentId);
        entity.HasIndex(x => x.CostCenterId);
    }
}
