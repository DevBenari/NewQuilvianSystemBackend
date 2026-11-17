using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstAccountingOutboxConfiguration : IEntityTypeConfiguration<AstAccountingOutbox>
{
    public void Configure(EntityTypeBuilder<AstAccountingOutbox> entity)
    {
        entity.ToTable("AstAccountingOutbox", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.EventTypeCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.SourceTransactionId).HasMaxLength(150).IsRequired();
        entity.Property(x => x.SourceVersion).HasMaxLength(10).IsRequired();
        entity.Property(x => x.AccountingDate).HasColumnType("date");
        entity.Property(x => x.DocumentDate).HasColumnType("date");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.ComponentsJson).HasColumnType("text");
        entity.Property(x => x.SubledgerJson).HasColumnType("text");
        entity.Property(x => x.SourceDocumentType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.AttemptCount).HasDefaultValue(0);
        entity.Property(x => x.LastError).HasMaxLength(1000);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.SourceTransactionId, x.SourceVersion })
            .IsUnique()
            .HasDatabaseName("IX_AstAccountingOutbox_Source");

        entity.HasIndex(x => x.EventTypeCode);
        entity.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId });
        entity.HasIndex(x => new { x.Status, x.CreateDateTime })
            .HasDatabaseName("IX_AstAccountingOutbox_Status_Created");
    }
}
