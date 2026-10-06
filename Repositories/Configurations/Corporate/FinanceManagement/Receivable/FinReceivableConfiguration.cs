using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableConfiguration : IEntityTypeConfiguration<FinReceivable>
{
    public void Configure(EntityTypeBuilder<FinReceivable> entity)
    {
        entity.ToTable("FinReceivable", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivable_DebtorType", "\"DebtorType\" IN ('PAYER','PATIENT_GUARANTOR','EMPLOYEE_BENEFIT')");
            table.HasCheckConstraint("CK_FinReceivable_Status", "\"Status\" IN ('OUTSTANDING','PARTIAL','SETTLED','WRITTEN_OFF','CANCELLED')");
            table.HasCheckConstraint("CK_FinReceivable_Outstanding", "\"OutstandingAmount\" >= 0");
            // FIN-VAL-011: nilai asli piutang selalu seimbang.
            table.HasCheckConstraint("CK_FinReceivable_Balance",
                "\"OriginalAmount\" = \"OutstandingAmount\" + \"AllocatedAmount\" + \"AdjustedAmount\" + \"WrittenOffAmount\"");
            table.HasCheckConstraint("CK_FinReceivable_BenefitOwner",
                "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NOT NULL) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL)");
            // BE-FIN-079, FIN-DES-093: baris Billing (tiga kolom asal) XOR baris migrasi (OpeningItemBatchId).
            table.HasCheckConstraint("CK_FinReceivable_OpeningItem",
                "(\"SourceHandoffKey\" IS NOT NULL AND \"SourceHandoffId\" IS NOT NULL AND \"InvoiceId\" IS NOT NULL AND \"OpeningItemBatchId\" IS NULL) " +
                "OR (\"SourceHandoffKey\" IS NULL AND \"SourceHandoffId\" IS NULL AND \"InvoiceId\" IS NULL AND \"OpeningItemBatchId\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ReceivableNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DebtorType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.BenefitRelationship).HasMaxLength(30);
        entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.AllocatedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.AdjustedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.WrittenOffAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.DueDate).HasColumnType("date");
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableStatuses.Outstanding);
        entity.Property(x => x.ClaimStatus).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableClaimStatuses.NotRequired);
        entity.Property(x => x.RecognizedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // BE-FIN-079: FinOpeningItemBatch berada di submodule AccountingIntegration — satu bounded
        // context, sehingga FK sungguhan dipakai (berbeda dari SourceHandoffId/InvoiceId di atas
        // yang lintas bounded context dengan Billing).
        entity.HasOne(x => x.OpeningItemBatch)
            .WithMany()
            .HasForeignKey(x => x.OpeningItemBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.ReceivableNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinReceivable_ReceivableNumber");
        // BE-FIN-079: filter ditambah "SourceHandoffKey IS NOT NULL" supaya baris migrasi (kolom ini
        // NULL) tidak ikut diuji keunikannya — idempotensi intake Billing tetap terjaga.
        entity.HasIndex(x => x.SourceHandoffKey).IsUnique().HasFilter("\"IsDelete\" = false AND \"SourceHandoffKey\" IS NOT NULL").HasDatabaseName("IX_FinReceivable_SourceHandoffKey");
        entity.HasIndex(x => new { x.Status, x.DueDate }).HasDatabaseName("IX_FinReceivable_Status_DueDate");
        entity.HasIndex(x => x.OpeningItemBatchId).HasDatabaseName("IX_FinReceivable_OpeningItemBatchId");
    }
}
