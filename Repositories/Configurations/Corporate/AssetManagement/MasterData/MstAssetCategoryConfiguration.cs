using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetCategory</c> — data-dictionary.md bagian 2.1 (BE-AST-002).</summary>
public sealed class MstAssetCategoryConfiguration : IEntityTypeConfiguration<MstAssetCategory>
{
    public void Configure(EntityTypeBuilder<MstAssetCategory> entity)
    {
        entity.ToTable("MstAssetCategory", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CategoryCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(500);

        // Sentinel true: tanpa itu EF tidak mengirim false saat INSERT dan default kolom true selalu
        // menang, sehingga kategori Tanah tersimpan sebagai disusutkan (preseden MstPatient.IsMember,
        // BE-CRM-015). Skema kolom tidak berubah.
        entity.Property(x => x.IsDepreciable).HasDefaultValue(true).HasSentinel(true);
        entity.Property(x => x.IsActive).HasDefaultValue(true).HasSentinel(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.CategoryCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetCategory_CategoryCode");
    }
}
