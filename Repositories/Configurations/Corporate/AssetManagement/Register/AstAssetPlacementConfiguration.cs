using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstAssetPlacement</c> — data-dictionary.md bagian 3.2 dan DDL bagian 4 (BE-AST-003).</summary>
public sealed class AstAssetPlacementConfiguration : IEntityTypeConfiguration<AstAssetPlacement>
{
    public void Configure(EntityTypeBuilder<AstAssetPlacement> entity)
    {
        entity.ToTable("AstAssetPlacement", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SourceType).HasMaxLength(30).IsRequired();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany(x => x.Placements).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CustodianEmployee).WithMany().HasForeignKey(x => x.CustodianEmployeeId).OnDelete(DeleteBehavior.Restrict);

        // Satu penempatan berjalan per aset.
        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasFilter("\"EndAt\" IS NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_AstAssetPlacement_Current");

        entity.HasIndex(x => x.OrganizationUnitId);
        entity.HasIndex(x => x.LocationId);
        entity.HasIndex(x => x.CustodianEmployeeId);
        entity.HasIndex(x => x.SourceDocumentId);
        entity.HasIndex(x => x.StartAt);
    }
}
