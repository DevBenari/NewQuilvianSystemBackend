using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstValuationParameterConfiguration : IEntityTypeConfiguration<AstValuationParameter>
{
    public void Configure(EntityTypeBuilder<AstValuationParameter> entity)
    {
        entity.ToTable("AstValuationParameter", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EffectiveFromPeriod).HasColumnType("date");
        entity.Property(x => x.CapitalizationThreshold).HasPrecision(18, 2);
        entity.Property(x => x.DisposalDirectorThreshold).HasPrecision(18, 2);
        entity.Property(x => x.UtilizationDirectorThreshold).HasPrecision(18, 2);
        entity.Property(x => x.Note).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.LegalEntityId, x.EffectiveFromPeriod })
            .IsUnique()
            .HasDatabaseName("IX_AstValuationParameter_Entity_Period");
    }
}
