using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollHandoffBatchConfiguration : IEntityTypeConfiguration<TrxPayrollHandoffBatch>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollHandoffBatch> builder)
        {
            builder.ToTable("TrxPayrollHandoffBatch", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BatchNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.TotalDisbursementAmount).HasPrecision(18, 2);
            builder.Property(x => x.ChecksumSha256).HasMaxLength(64).IsRequired();
            builder.Property(x => x.HandoffStatus).HasMaxLength(40).IsRequired();
            builder.Property(x => x.FinanceReferenceNo).HasMaxLength(100);
            builder.Property(x => x.FinanceNotes).HasMaxLength(2000);

            builder.HasOne(x => x.PayrollRun)
                .WithMany()
                .HasForeignKey(x => x.PayrollRunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TransferredByUser)
                .WithMany()
                .HasForeignKey(x => x.TransferredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BatchNumber)
                .IsUnique()
                .HasDatabaseName("idx_payroll_handoff_batch_number");

            builder.HasIndex(x => new { x.PeriodYear, x.PeriodMonth })
                .HasDatabaseName("idx_payroll_handoff_period");
        }
    }
}
