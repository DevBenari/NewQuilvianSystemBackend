using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinMedicalServicePayableConfiguration : IEntityTypeConfiguration<FinMedicalServicePayable>
{
    public void Configure(EntityTypeBuilder<FinMedicalServicePayable> entity)
    {
        entity.ToTable("FinMedicalServicePayable", "public", table =>
        {
            table.HasCheckConstraint("CK_FinMedicalServicePayable_PayeeType", "\"PayeeType\" IN ('DOCTOR','NURSE','OTHER_PRACTITIONER')");
            table.HasCheckConstraint("CK_FinMedicalServicePayable_Status", "\"Status\" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')");
            table.HasCheckConstraint("CK_FinMedicalServicePayable_Outstanding", "\"OutstandingAmount\" >= 0");
            table.HasCheckConstraint("CK_FinMedicalServicePayable_Balance",
                "\"OriginalAmount\" = \"OutstandingAmount\" + \"PaidAmount\" + \"AdjustedAmount\"");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PayableNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.PayeeType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.PayeeReferenceId).IsRequired();
        entity.Property(x => x.SourceMedicalServiceFeeId).IsRequired();
        entity.Property(x => x.SourceApHandoffId).IsRequired(false);
        entity.Property(x => x.PeriodCode).HasMaxLength(20).IsRequired();

        entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        entity.Property(x => x.OutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.PaidAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.AdjustedAmount).HasPrecision(18, 2).HasDefaultValue(0m);

        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinMedicalServicePayableStatuses.Outstanding);
        entity.Property(x => x.RecognizedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Index
        entity.HasIndex(x => x.PayableNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinMedicalServicePayable_PayableNumber");

        // Satu hasil jasa yang disetujui menghasilkan paling banyak satu utang (MF-DEC-005, data-dictionary.md §A.6)
        entity.HasIndex(x => x.SourceMedicalServiceFeeId)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinMedicalServicePayable_SourceFee");

        entity.HasIndex(x => new { x.PayeeType, x.PayeeReferenceId, x.PeriodCode })
            .HasDatabaseName("IX_FinMedicalServicePayable_Payee_Period");

        entity.HasIndex(x => x.SourceApHandoffId)
            .HasDatabaseName("IX_FinMedicalServicePayable_SourceApHandoffId");

        entity.HasIndex(x => x.Status)
            .HasDatabaseName("IX_FinMedicalServicePayable_Status");

        entity.HasIndex(x => x.OutstandingAmount)
            .HasDatabaseName("IX_FinMedicalServicePayable_Outstanding");
    }
}
