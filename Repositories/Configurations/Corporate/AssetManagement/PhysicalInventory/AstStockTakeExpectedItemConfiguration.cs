using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.PhysicalInventory;

public sealed class AstStockTakeExpectedItemConfiguration : IEntityTypeConfiguration<AstStockTakeExpectedItem>
{
    public void Configure(EntityTypeBuilder<AstStockTakeExpectedItem> entity)
    {
        entity.ToTable("AstStockTakeExpectedItem", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ExpectedCondition).HasConversion<int>().IsRequired();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Session).WithMany(x => x.ExpectedItems).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ExpectedLocation).WithMany().HasForeignKey(x => x.ExpectedLocationId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ExpectedCustodianEmployee).WithMany().HasForeignKey(x => x.ExpectedCustodianEmployeeId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.LoanBorrowerOrganizationUnit).WithMany().HasForeignKey(x => x.LoanBorrowerOrganizationUnitId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.SessionId, x.AssetId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstStockTakeExpectedItem_Session_Asset");

        entity.HasIndex(x => x.AssetId);
    }
}
