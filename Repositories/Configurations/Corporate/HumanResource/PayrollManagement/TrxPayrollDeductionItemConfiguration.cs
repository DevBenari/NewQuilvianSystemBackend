using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollDeductionItemConfiguration : IEntityTypeConfiguration<TrxPayrollDeductionItem>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollDeductionItem> builder)
        {
            builder.ToTable("TrxPayrollDeductionItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeductionCategory)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ScheduledAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.ActualDeductedAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.DeferredAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.ExecutionStatus)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.PayrollRun)
                .WithMany()
                .HasForeignKey(x => x.PayrollRunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DeductionSchedule)
                .WithMany(x => x.DeductionItems)
                .HasForeignKey(x => x.DeductionScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PayrollRunId, x.DeductionScheduleId })
                .IsUnique()
                .HasDatabaseName("uq_deduction_item_run_schedule");

            builder.HasIndex(x => new { x.EmployeeId, x.PriorityLevel })
                .HasDatabaseName("idx_deduction_item_emp_priority");

            builder.HasIndex(x => x.ExecutionStatus)
                .HasDatabaseName("idx_deduction_item_status");
        }
    }
}
