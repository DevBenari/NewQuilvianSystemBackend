using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstDepreciationRunLineConfiguration : IEntityTypeConfiguration<AstDepreciationRunLine>
{
    public void Configure(EntityTypeBuilder<AstDepreciationRunLine> entity)
    {
        entity.ToTable("AstDepreciationRunLine", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.AdjustmentReason).HasConversion<int>().IsRequired();
        entity.Property(x => x.AdjustmentPeriod).HasColumnType("date");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Run).WithMany(x => x.Lines).HasForeignKey(x => x.RunId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.AssetGroup).WithMany().HasForeignKey(x => x.AssetGroupId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.RunId, x.AssetId, x.AdjustmentReason })
            .IsUnique()
            .HasDatabaseName("IX_AstDepreciationRunLine_Run_Asset_Reason");

        entity.HasIndex(x => new { x.RunId, x.AssetGroupId, x.CostCenterId })
            .HasDatabaseName("IX_AstDepreciationRunLine_Run_Group_CostCenter");

        entity.HasIndex(x => x.AssetId);
    }
}
