using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetLocation</c> — data-dictionary.md bagian 2.7 (BE-AST-002).</summary>
/// <remarks>
/// FK ke <c>MstOrganizationUnit</c> (HR) dan <c>MstRoom</c> (HealthServices) memakai <c>WithMany()</c> tanpa
/// koleksi balik agar model milik modul lain tidak disentuh.
/// </remarks>
public sealed class MstAssetLocationConfiguration : IEntityTypeConfiguration<MstAssetLocation>
{
    public void Configure(EntityTypeBuilder<MstAssetLocation> entity)
    {
        entity.ToTable("MstAssetLocation", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.LocationCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.LocationName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.BuildingName).HasMaxLength(100);
        entity.Property(x => x.FloorName).HasMaxLength(50);
        entity.Property(x => x.RoomName).HasMaxLength(100);
        entity.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.OrganizationUnit)
            .WithMany()
            .HasForeignKey(x => x.OrganizationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.OrganizationUnitId);
        entity.HasIndex(x => x.RoomId);

        entity.HasIndex(x => x.LocationCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetLocation_LocationCode");
    }
}
