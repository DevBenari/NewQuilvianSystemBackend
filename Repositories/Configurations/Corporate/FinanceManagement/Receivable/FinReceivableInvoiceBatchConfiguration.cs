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
            table.HasCheckConstraint("CK_FinReceivableInvoiceBatch_ClaimStatus", "\"ClaimStatus\" IS NULL OR \"ClaimStatus\" IN ('SUBMITTED','PAYER_VERIFIED','APPROVED','CLOSED')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DebtorType).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableInvoiceBatchDebtorTypes.Payer);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinReceivableInvoiceBatchStatuses.Draft);
        entity.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        // Dokumen & Tenor tagihan
        entity.Property(x => x.InvoiceDate).HasColumnType("date");
        entity.Property(x => x.DueDate).HasColumnType("date");
        entity.Property(x => x.Note).HasMaxLength(500);
        entity.Property(x => x.VisitCode).HasMaxLength(10);
        entity.Property(x => x.TotalDiscount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DiscountType).HasMaxLength(20);
        entity.Property(x => x.DiscountPercent).HasPrecision(18, 2);
        entity.Property(x => x.DiscountNote).HasMaxLength(500);
        entity.Property(x => x.OtherReceiptAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.OtherReceiptNote).HasMaxLength(500);

        // Sumbu klaim penjamin (BE-FIN-052, FIN-DEC-097) — seluruhnya nullable, lihat FIN-DES-070.
        entity.Property(x => x.ClaimStatus).HasMaxLength(30);
        entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
        entity.Property(x => x.PayerClaimReference).HasMaxLength(100);
        entity.Property(x => x.ClaimNote).HasMaxLength(500);
        entity.Property(x => x.PayerVerifiedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ClaimApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ClaimClosedAt).HasColumnType("timestamp with time zone");

        // Workflow Canceled Invoice & Reissue
        entity.Property(x => x.CancelReason).HasMaxLength(500);
        entity.Property(x => x.ServiceType).HasMaxLength(20);
        entity.Property(x => x.ReissuedFromBatchId);
        entity.Property(x => x.ReissuedToBatchId);

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
        entity.HasIndex(x => x.ClaimStatus).HasDatabaseName("IX_FinReceivableInvoiceBatch_ClaimStatus");
        entity.HasIndex(x => x.InvoiceDate).HasDatabaseName("IX_FinReceivableInvoiceBatch_InvoiceDate");
        entity.HasIndex(x => x.CancelDateTime).HasDatabaseName("IX_FinReceivableInvoiceBatch_CancelDateTime");
        entity.HasIndex(x => x.ServiceType).HasDatabaseName("IX_FinReceivableInvoiceBatch_ServiceType");
        entity.HasIndex(x => x.ReissuedFromBatchId).IsUnique().HasFilter("\"IsDelete\" = false AND \"ReissuedFromBatchId\" IS NOT NULL").HasDatabaseName("IX_FinReceivableInvoiceBatch_ReissuedFromBatchId");
        entity.HasIndex(x => x.ReissuedToBatchId).HasDatabaseName("IX_FinReceivableInvoiceBatch_ReissuedToBatchId");
    }
}
