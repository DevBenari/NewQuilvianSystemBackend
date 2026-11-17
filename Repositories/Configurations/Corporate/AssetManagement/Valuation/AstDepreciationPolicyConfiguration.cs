using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstDepreciationPolicyConfiguration : IEntityTypeConfiguration<AstDepreciationPolicy>
{
    public void Configure(EntityTypeBuilder<AstDepreciationPolicy> entity)
    {
        entity.ToTable("AstDepreciationPolicy", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EffectiveFromPeriod).HasColumnType("date");
        entity.Property(x => x.ResidualValuePercent).HasPrecision(5, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.AssetGroup).WithMany().HasForeignKey(x => x.AssetGroupId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.AssetGroupId, x.EffectiveFromPeriod })
            .IsUnique()
            .HasDatabaseName("IX_AstDepreciationPolicy_Group_Period");
    }
}
