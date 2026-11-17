using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstImportBatchRow</c> — data-dictionary.md bagian 3.9 (BE-AST-003).</summary>
public sealed class AstImportBatchRowConfiguration : IEntityTypeConfiguration<AstImportBatchRow>
{
    public void Configure(EntityTypeBuilder<AstImportBatchRow> entity)
    {
        entity.ToTable("AstImportBatchRow", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.RawJson).HasColumnType("text").IsRequired();
        entity.Property(x => x.ErrorMessages).HasMaxLength(2000);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.ImportBatch).WithMany(x => x.Rows).HasForeignKey(x => x.ImportBatchId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CreatedAsset).WithMany().HasForeignKey(x => x.CreatedAssetId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.ImportBatchId, x.RowNumber })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstImportBatchRow_Batch_RowNumber");

        entity.HasIndex(x => x.CreatedAssetId);
    }
}
