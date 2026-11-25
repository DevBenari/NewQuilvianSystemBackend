using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstSubledgerAccountSetupConfiguration : IEntityTypeConfiguration<AstSubledgerAccountSetup>
{
    public void Configure(EntityTypeBuilder<AstSubledgerAccountSetup> entity)
    {
        entity.ToTable("AstSubledgerAccountSetup", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CostControlAccountCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.AccumulatedDepreciationControlAccountCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.AccumulatedImpairmentControlAccountCode).HasMaxLength(50);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.AssetGroup).WithMany().HasForeignKey(x => x.AssetGroupId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.AssetGroupId)
            .IsUnique()
            .HasDatabaseName("IX_AstSubledgerAccountSetup_AssetGroupId");
    }
}
