using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinPaymentConfiguration : IEntityTypeConfiguration<FinPayment>
{
    public void Configure(EntityTypeBuilder<FinPayment> entity)
    {
        entity.ToTable("FinPayment", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPayment_PaymentType", "\"PaymentType\" IN ('SUPPLIER','MEDICAL_SERVICE')");
            table.HasCheckConstraint("CK_FinPayment_Status", "\"Status\" IN ('DRAFT','SUBMITTED','APPROVED','PAID','REJECTED','CANCELLED')");
            table.HasCheckConstraint("CK_FinPayment_PaymentMethod", "\"PaymentMethod\" IN ('TRANSFER','CASH','CHEQUE')");
            table.HasCheckConstraint("CK_FinPayment_Total", "\"TotalAmount\" > 0");
            table.HasCheckConstraint("CK_FinPayment_NetTransfer",
                "\"NetTransferAmount\" = \"TotalAmount\" - \"DeductionAmount\" + \"AdditionAmount\"");
            table.HasCheckConstraint("CK_FinPayment_NetTransferNonNegative", "\"NetTransferAmount\" >= 0");
            // Maker-checker: pengaju tidak boleh menyetujui pembayarannya sendiri (FIN-VAL-051).
            table.HasCheckConstraint("CK_FinPayment_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
            // Pembayaran yang berstatus PAID wajib teralokasi penuh ke utang (FIN-VAL-050).
            table.HasCheckConstraint("CK_FinPayment_FullyAllocatedWhenPaid", "\"Status\" <> 'PAID' OR \"AllocatedAmount\" = \"TotalAmount\"");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PaymentNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.PaymentType).HasMaxLength(30).IsRequired().HasDefaultValue(FinPaymentTypes.Supplier);
        entity.Property(x => x.PaymentMethod).HasMaxLength(30).IsRequired().HasDefaultValue(FinPaymentMethods.Transfer);
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DeductionAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.AdditionAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.NetTransferAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinPaymentStatuses.Draft);
        entity.Property(x => x.ApprovalTier).HasMaxLength(30);
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReferenceNumber).HasMaxLength(150);
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Allocations)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Deductions)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.PaymentNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinPayment_PaymentNumber");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinPayment_Status");
        entity.HasIndex(x => x.PaymentType).HasDatabaseName("IX_FinPayment_PaymentType");
        entity.HasIndex(x => x.PayeeReferenceId).HasDatabaseName("IX_FinPayment_PayeeReferenceId");
        entity.HasIndex(x => x.BankAccountId).HasDatabaseName("IX_FinPayment_BankAccountId");
        entity.HasIndex(x => x.PaidAt).HasDatabaseName("IX_FinPayment_PaidAt");
        entity.HasIndex(x => x.RequestedBy).HasDatabaseName("IX_FinPayment_RequestedBy");
    }
}
