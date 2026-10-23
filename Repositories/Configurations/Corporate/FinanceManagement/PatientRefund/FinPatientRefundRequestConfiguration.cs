using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.PatientRefund;

public sealed class FinPatientRefundRequestConfiguration : IEntityTypeConfiguration<FinPatientRefundRequest>
{
    public void Configure(EntityTypeBuilder<FinPatientRefundRequest> entity)
    {
        entity.ToTable("FinPatientRefundRequest", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_FinPatientRefundRequest_Amounts",
                "\"RequestedAmount\" > 0 AND \"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"RequestedAmount\" AND (\"ApprovedAmount\" IS NULL OR (\"ApprovedAmount\" > 0 AND \"ApprovedAmount\" <= \"RequestedAmount\"))");
            table.HasCheckConstraint(
                "CK_FinPatientRefundRequest_Status",
                "\"Status\" IN ('PENDING','APPROVED','REJECTED','CANCELLED','PARTIALLY_PAID','PAID','FAILED')");
            table.HasCheckConstraint(
                "CK_FinPatientRefundRequest_MakerChecker",
                "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.RequestedAmount).HasPrecision(18, 2);
        entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.PaymentMethod).HasMaxLength(30);
        entity.Property(x => x.PayeeBankName).HasMaxLength(100);
        entity.Property(x => x.PayeeAccountNumber).HasMaxLength(50);
        entity.Property(x => x.PayeeAccountName).HasMaxLength(150);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.Property(x => x.ExecutionNotes).HasMaxLength(500);
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.FailedReason).HasMaxLength(500);
        entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.PatientId);
        entity.HasIndex(x => x.RefundableCreditId);
        entity.HasIndex(x => new { x.Status, x.SubmittedAt });

        entity.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.RefundableCredit).WithMany().HasForeignKey(x => x.RefundableCreditId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

