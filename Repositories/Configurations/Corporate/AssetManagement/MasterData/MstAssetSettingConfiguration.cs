using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.MasterData;

/// <summary>Pemetaan <c>MstAssetSetting</c> — data-dictionary.md bagian 2.8 (BE-AST-002).</summary>
/// <remarks>Unik <c>LegalEntityId</c>: satu baris pengaturan per badan hukum.</remarks>
public sealed class MstAssetSettingConfiguration : IEntityTypeConfiguration<MstAssetSetting>
{
    public void Configure(EntityTypeBuilder<MstAssetSetting> entity)
    {
        entity.ToTable("MstAssetSetting", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.IdleThresholdDays).HasDefaultValue(90);
        entity.Property(x => x.DocumentExpiryWarningDays).HasDefaultValue(30);
        entity.Property(x => x.UtilizationMaxMonthsForAssetHead).HasDefaultValue(12);
        entity.Property(x => x.CutoverDate).HasColumnType("date");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.LegalEntity)
            .WithMany()
            .HasForeignKey(x => x.LegalEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.HospitalSite)
            .WithMany()
            .HasForeignKey(x => x.HospitalSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.LegalEntityId)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstAssetSetting_LegalEntityId");
    }
}
