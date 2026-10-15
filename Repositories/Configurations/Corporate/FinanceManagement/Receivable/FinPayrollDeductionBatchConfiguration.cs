using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinPayrollDeductionBatchConfiguration : IEntityTypeConfiguration<FinPayrollDeductionBatch>
{
    public void Configure(EntityTypeBuilder<FinPayrollDeductionBatch> entity)
    {
        entity.ToTable("FinPayrollDeductionBatch", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPayrollDeductionBatch_Type", "\"BatchType\" IN ('REGULER','KOREKSI')");
            table.HasCheckConstraint("CK_FinPayrollDeductionBatch_Status", "\"Status\" IN ('DRAFT','DIAJUKAN','DISAHKAN','DITOLAK','DIBATALKAN')");
            table.HasCheckConstraint("CK_FinPayrollDeductionBatch_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"SubmittedBy\"");
            table.HasCheckConstraint("CK_FinPayrollDeductionBatch_Correction", "\"BatchType\" <> 'KOREKSI' OR \"CorrectionReason\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinPayrollDeductionBatch_Period", "\"DeductionPeriod\" ~ '^[0-9]{4}-(0[1-9]|1[0-2])$'");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DeductionPeriod).HasMaxLength(7).IsRequired();
        entity.Property(x => x.BatchType).HasMaxLength(10).IsRequired().HasDefaultValue(FinPayrollDeductionBatchTypes.Reguler);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinPayrollDeductionBatchStatuses.Draft);
        entity.Property(x => x.ControlTotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalDeductedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.TotalRows).HasDefaultValue(0);
        entity.Property(x => x.InvalidRows).HasDefaultValue(0);
        entity.Property(x => x.PayrollReportNumber).HasMaxLength(100);
        entity.Property(x => x.CorrectionReason).HasMaxLength(500);
        entity.Property(x => x.SourceFileName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.BatchNumber).IsUnique();

        entity.HasIndex(x => new { x.DeductionPeriod, x.Status });

        // Hanya satu batch REGULER aktif (DRAFT, DIAJUKAN, DISAHKAN) per periode (FIN-VAL-282, UX_FinPayrollDeductionBatch_RegularPeriod)
        entity.HasIndex(x => x.DeductionPeriod)
            .IsUnique()
            .HasFilter("\"BatchType\" = 'REGULER' AND \"Status\" IN ('DRAFT','DIAJUKAN','DISAHKAN') AND \"IsDelete\" = false")
            .HasDatabaseName("UX_FinPayrollDeductionBatch_RegularPeriod");

        entity.HasOne(x => x.Proof)
            .WithMany()
            .HasForeignKey(x => x.ProofId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(x => x.Items)
            .WithOne(x => x.Batch)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

