using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstDepreciationRunConfiguration : IEntityTypeConfiguration<AstDepreciationRun>
{
    public void Configure(EntityTypeBuilder<AstDepreciationRun> entity)
    {
        entity.ToTable("AstDepreciationRun", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Period).HasColumnType("date");
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.AdjustmentAmount).HasPrecision(18, 2);
        entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.LegalEntityId, x.Period })
            .IsUnique()
            .HasDatabaseName("IX_AstDepreciationRun_Entity_Period");
    }
}
