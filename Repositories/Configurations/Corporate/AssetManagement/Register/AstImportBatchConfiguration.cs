using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Register;

/// <summary>Pemetaan <c>AstImportBatch</c> — data-dictionary.md bagian 3.9 (BE-AST-003).</summary>
public sealed class AstImportBatchConfiguration : IEntityTypeConfiguration<AstImportBatch>
{
    public void Configure(EntityTypeBuilder<AstImportBatch> entity)
    {
        entity.ToTable("AstImportBatch", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ImportNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.CutoverDate).HasColumnType("date");
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.RowCount).HasDefaultValue(0);
        entity.Property(x => x.ErrorCount).HasDefaultValue(0);
        entity.Property(x => x.TotalAcquisitionCost).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.TotalAccumulatedDepreciation).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.FileAttachment).WithMany().HasForeignKey(x => x.FileAttachmentId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.ImportNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstImportBatch_ImportNumber");

        entity.HasIndex(x => x.Status);
    }
}
