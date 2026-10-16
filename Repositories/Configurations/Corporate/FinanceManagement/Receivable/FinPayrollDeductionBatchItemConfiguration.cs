using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinPayrollDeductionBatchItemConfiguration : IEntityTypeConfiguration<FinPayrollDeductionBatchItem>
{
    public void Configure(EntityTypeBuilder<FinPayrollDeductionBatchItem> entity)
    {
        entity.ToTable("FinPayrollDeductionBatchItem", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPayrollDeductionBatchItem_Result",
                "\"ResultStatus\" IN ('BERHASIL','SEBAGIAN','GAGAL') AND \"DeductedAmount\" >= 0 AND (\"ResultStatus\" <> 'GAGAL' OR \"DeductedAmount\" = 0)");
            table.HasCheckConstraint("CK_FinPayrollDeductionBatchItem_Validation",
                "\"ValidationStatus\" IN ('VALID','INVALID')");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.RowNumber).IsRequired();
        entity.Property(x => x.InstallmentCode).HasMaxLength(60).IsRequired();
        entity.Property(x => x.EmployeeNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ResultStatus).HasMaxLength(10).IsRequired();
        entity.Property(x => x.DeductedAmount).HasPrecision(18, 2);
        entity.Property(x => x.DueAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.PreviousDeductedAmount).HasPrecision(18, 2);
        entity.Property(x => x.DeltaAmount).HasPrecision(18, 2);
        entity.Property(x => x.ValidationStatus).HasMaxLength(10).IsRequired().HasDefaultValue(FinPayrollDeductionItemValidationStatuses.Valid);
        entity.Property(x => x.ValidationMessage).HasMaxLength(500);
        entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Unik (BatchId, RowNumber)
        entity.HasIndex(x => new { x.BatchId, x.RowNumber }).IsUnique();

        // Unik parsial (BatchId, InstallmentId) untuk memastikan satu angsuran tidak muncul ganda dalam satu batch
        entity.HasIndex(x => new { x.BatchId, x.InstallmentId })
            .IsUnique()
            .HasFilter("\"InstallmentId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("UX_FinPayrollDeductionBatchItem_BatchInstallment");

        entity.HasOne(x => x.Batch)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Installment)
            .WithMany()
            .HasForeignKey(x => x.InstallmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

