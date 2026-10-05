using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.AccountingIntegration;

public sealed class FinOpeningItemBatchConfiguration : IEntityTypeConfiguration<FinOpeningItemBatch>
{
    public void Configure(EntityTypeBuilder<FinOpeningItemBatch> entity)
    {
        entity.ToTable("FinOpeningItemBatch", "public", table =>
        {
            table.HasCheckConstraint("CK_FinOpeningItemBatch_ItemKind",
                "\"ItemKind\" IN ('RECEIVABLE','SUPPLIER_PAYABLE')");
            table.HasCheckConstraint("CK_FinOpeningItemBatch_Status",
                "\"Status\" IN ('DRAFT','VALIDATED','APPROVED','LOCKED','REJECTED')");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ItemKind).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinOpeningItemBatchStatuses.Draft);
        entity.Property(x => x.CutoverDate).HasColumnType("date").IsRequired();
        entity.Property(x => x.TotalItemCount).HasDefaultValue(0);
        entity.Property(x => x.TotalOutstandingAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DeclaredAccountingOpeningAmount).HasPrecision(18, 2);
        entity.Property(x => x.AccountingReferenceDocument).HasMaxLength(200).IsRequired();
        entity.Property(x => x.UploadedFileName).HasMaxLength(260).IsRequired();
        entity.Property(x => x.SourceFormat).HasMaxLength(10).IsRequired();
        entity.Property(x => x.ValidationSummaryJson).HasColumnType("text");
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.BatchNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinOpeningItemBatch_BatchNumber");

        entity.HasIndex(x => x.ItemKind).HasDatabaseName("IX_FinOpeningItemBatch_ItemKind");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinOpeningItemBatch_Status");
    }
}
