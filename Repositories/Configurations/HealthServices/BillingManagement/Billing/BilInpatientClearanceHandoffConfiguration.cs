using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.Billing;

public sealed class BilInpatientClearanceHandoffConfiguration : IEntityTypeConfiguration<BilInpatientClearanceHandoff>
{
    public void Configure(EntityTypeBuilder<BilInpatientClearanceHandoff> entity)
    {
        entity.ToTable("BilInpatientClearanceHandoff", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilInpatientClearanceHandoff_ClearanceStatus",
                "\"ClearanceStatus\" IN ('PENDING', 'BLOCKED', 'CLEARED', 'REVOKED')");
            table.HasCheckConstraint(
                "CK_BilInpatientClearanceHandoff_Status",
                "\"Status\" IN ('CREATED', 'ACKNOWLEDGED')");
            table.HasCheckConstraint(
                "CK_BilInpatientClearanceHandoff_FinancialOutcome",
                "\"FinancialOutcome\" IS NULL OR \"FinancialOutcome\" IN ('FULLY_PAID', 'INSURANCE_GUARANTEED', 'SETTLED_WITH_DEPOSIT', 'DISCHARGED_WITH_AR')");
            table.HasCheckConstraint(
                "CK_BilInpatientClearanceHandoff_ReasonCode",
                "\"ReasonCode\" IN ('INVOICE_SETTLED', 'GUARANTOR_APPROVED', 'DISCHARGE_ORDER_INITIATED', 'LATE_CHARGE_POSTED', 'PAYMENT_REVERSED', 'CORRECTION_APPLIED')");
            table.HasCheckConstraint(
                "CK_BilInpatientClearanceHandoff_FinancialVersion",
                "\"FinancialVersion\" > 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.ClearanceStatus).HasMaxLength(20).IsRequired();
        entity.Property(x => x.FinancialOutcome).HasMaxLength(30);
        entity.Property(x => x.OutstandingBalance).HasPrecision(18, 2);
        entity.Property(x => x.TotalPatientResponsibility).HasPrecision(18, 2);
        entity.Property(x => x.TotalPaidOrAllocated).HasPrecision(18, 2);
        entity.Property(x => x.ReasonCode).HasMaxLength(40).IsRequired();
        entity.Property(x => x.RevocationReason).HasMaxLength(100);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("CREATED");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.HasIndex(x => new { x.EncounterId, x.FinancialVersion })
            .IsUnique()
            .HasDatabaseName("IX_BilInpatientClearanceHandoff_Encounter_Version");

        entity.HasIndex(x => x.Status)
            .HasDatabaseName("IX_BilInpatientClearanceHandoff_Status");

        entity.HasIndex(x => x.InvoiceId)
            .HasDatabaseName("IX_BilInpatientClearanceHandoff_Invoice");

        entity.HasIndex(x => x.ClearanceStatus)
            .HasDatabaseName("IX_BilInpatientClearanceHandoff_ClearanceStatus");

        entity.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
