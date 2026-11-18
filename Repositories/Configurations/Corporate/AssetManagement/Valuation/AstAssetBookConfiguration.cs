using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstAssetBookConfiguration : IEntityTypeConfiguration<AstAssetBook>
{
    public void Configure(EntityTypeBuilder<AstAssetBook> entity)
    {
        entity.ToTable("AstAssetBook", "public", table =>
        {
            table.HasCheckConstraint("CK_AstAssetBook_NotBelowResidual",
                "\"CapitalizedCost\" - \"AccumulatedDepreciation\" - \"AccumulatedImpairment\" >= \"ResidualValue\" OR \"IsCapitalized\" = false");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AcquisitionCost).HasPrecision(18, 2);
        entity.Property(x => x.CapitalizedCost).HasPrecision(18, 2);
        entity.Property(x => x.ResidualValue).HasPrecision(18, 2);
        entity.Property(x => x.DepreciatedMonths).HasDefaultValue(0);
        entity.Property(x => x.DepreciationStartPeriod).HasColumnType("date");
        entity.Property(x => x.LastDepreciatedPeriod).HasColumnType("date");
        entity.Property(x => x.AccumulatedDepreciation).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.AccumulatedImpairment).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.BookStatus).HasConversion<int>().IsRequired();
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.DepreciationPolicy).WithMany().HasForeignKey(x => x.DepreciationPolicyId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasDatabaseName("IX_AstAssetBook_AssetId");

        entity.HasIndex(x => x.IsCapitalized);
        entity.HasIndex(x => x.DepreciationStartPeriod);
        entity.HasIndex(x => x.BookStatus);
        entity.HasIndex(x => x.DepreciationPolicyId);
    }
}
