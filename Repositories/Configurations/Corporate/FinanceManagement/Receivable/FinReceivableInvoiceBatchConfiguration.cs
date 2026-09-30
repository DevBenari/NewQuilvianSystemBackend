using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableInvoiceBatchConfiguration : IEntityTypeConfiguration<FinReceivableInvoiceBatch>
{
    public void Configure(EntityTypeBuilder<FinReceivableInvoiceBatch> entity)
    {
        entity.ToTable("FinReceivableInvoiceBatch", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivableInvoiceBatch_DebtorType", "\"DebtorType\" = 'PAYER'");
            table.HasCheckConstraint("CK_FinReceivableInvoiceBatch_Status", "\"Status\" IN ('DRAFT','ISSUED','PARTIALLY_PAID','PAID','CANCELLED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DebtorType).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableInvoiceBatchDebtorTypes.Payer);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinReceivableInvoiceBatchStatuses.Draft);
        entity.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.BatchNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinReceivableInvoiceBatch_BatchNumber");
        entity.HasIndex(x => x.DebtorType).HasDatabaseName("IX_FinReceivableInvoiceBatch_DebtorType");
        entity.HasIndex(x => x.DebtorReferenceId).HasDatabaseName("IX_FinReceivableInvoiceBatch_DebtorReferenceId");
        entity.HasIndex(x => x.PeriodStart).HasDatabaseName("IX_FinReceivableInvoiceBatch_PeriodStart");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinReceivableInvoiceBatch_Status");
    }
}
