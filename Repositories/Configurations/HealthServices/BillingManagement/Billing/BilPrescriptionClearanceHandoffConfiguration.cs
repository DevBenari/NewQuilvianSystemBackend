using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing;

public sealed class BilPrescriptionClearanceHandoffConfiguration : IEntityTypeConfiguration<BilPrescriptionClearanceHandoff>
{
    public void Configure(EntityTypeBuilder<BilPrescriptionClearanceHandoff> entity)
    {
        entity.ToTable("BilPrescriptionClearanceHandoff", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilPrescriptionClearanceHandoff_ClearanceStatus",
                "\"ClearanceStatus\" IN ('CLEARED', 'REVOKED')");
            table.HasCheckConstraint(
                "CK_BilPrescriptionClearanceHandoff_Status",
                "\"Status\" IN ('CREATED', 'ACKNOWLEDGED')");
            table.HasCheckConstraint(
                "CK_BilPrescriptionClearanceHandoff_FinancialOutcome",
                "\"FinancialOutcome\" IS NULL OR \"FinancialOutcome\" IN ('PAID', 'INSURANCE_APPROVED', 'PAYMENT_WAIVED')");
            table.HasCheckConstraint(
                "CK_BilPrescriptionClearanceHandoff_ReasonCode",
                "\"ReasonCode\" IN ('INVOICE_SETTLED', 'INVOICE_WRITTEN_OFF', 'PRESCRIPTION_CHARGE_INCREASED', 'PAYMENT_REVERSED', 'WRITE_OFF_REVERSED', 'PAYER_COVERAGE_REVERSED')");
            table.HasCheckConstraint(
                "CK_BilPrescriptionClearanceHandoff_FinancialVersion",
                "\"FinancialVersion\" > 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.ClearanceStatus).HasMaxLength(20).IsRequired();
        entity.Property(x => x.FinancialOutcome).HasMaxLength(30);
        entity.Property(x => x.ReasonCode).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("CREATED");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.HasIndex(x => new { x.PrescriptionId, x.FinancialVersion })
            .IsUnique()
            .HasDatabaseName("IX_BilPrescriptionClearanceHandoff_Prescription_Version");

        entity.HasIndex(x => x.Status)
            .HasDatabaseName("IX_BilPrescriptionClearanceHandoff_Status");

        entity.HasIndex(x => x.InvoiceId)
            .HasDatabaseName("IX_BilPrescriptionClearanceHandoff_Invoice");

        entity.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
