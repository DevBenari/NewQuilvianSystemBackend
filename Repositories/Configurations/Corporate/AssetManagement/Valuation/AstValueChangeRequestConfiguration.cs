using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Valuation;

public sealed class AstValueChangeRequestConfiguration : IEntityTypeConfiguration<AstValueChangeRequest>
{
    public void Configure(EntityTypeBuilder<AstValueChangeRequest> entity)
    {
        entity.ToTable("AstValueChangeRequest", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.RequestNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.ChangeType).HasConversion<int>().IsRequired();
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.NewResidualValue).HasPrecision(18, 2);
        entity.Property(x => x.InvoiceNumber).HasMaxLength(60);
        entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.EvidenceAttachment).WithMany().HasForeignKey(x => x.EvidenceAttachmentId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.AppliedEntry).WithMany().HasForeignKey(x => x.AppliedEntryId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.RequestNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstValueChangeRequest_RequestNumber");

        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasFilter("\"Status\" = 1 AND \"IsDelete\" = false")
            .HasDatabaseName("IX_AstValueChangeRequest_OpenPerAsset");

        entity.HasIndex(x => x.ChangeType);
        entity.HasIndex(x => x.Status);
    }
}
