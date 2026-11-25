using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstUnitCostCenterConfiguration : IEntityTypeConfiguration<AstUnitCostCenter>
{
    public void Configure(EntityTypeBuilder<AstUnitCostCenter> entity)
    {
        entity.ToTable("AstUnitCostCenter", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.OrganizationUnitId)
            .IsUnique()
            .HasDatabaseName("IX_AstUnitCostCenter_OrganizationUnitId");

        entity.HasIndex(x => x.CostCenterId);
    }
}
