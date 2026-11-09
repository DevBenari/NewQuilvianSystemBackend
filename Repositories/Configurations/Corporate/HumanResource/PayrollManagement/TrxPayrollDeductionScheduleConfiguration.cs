using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollDeductionScheduleConfiguration : IEntityTypeConfiguration<TrxPayrollDeductionSchedule>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollDeductionSchedule> builder)
        {
            builder.ToTable("TrxPayrollDeductionSchedule", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ScheduleNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DeductionCategory)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ExternalReferenceId)
                .HasMaxLength(100);

            builder.Property(x => x.TotalLoanPrincipal)
                .HasPrecision(18, 2);

            builder.Property(x => x.MonthlyInstallmentAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.RemainingPrincipal)
                .HasPrecision(18, 2);

            builder.Property(x => x.ScheduleStatus)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.DeductionItems)
                .WithOne(x => x.DeductionSchedule)
                .HasForeignKey(x => x.DeductionScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ScheduleNumber)
                .IsUnique()
                .HasDatabaseName("uq_payroll_deduction_schedule_number");

            builder.HasIndex(x => new { x.EmployeeId, x.PriorityLevel })
                .HasDatabaseName("idx_deduction_schedule_emp_priority");

            builder.HasIndex(x => x.ScheduleStatus)
                .HasDatabaseName("idx_deduction_schedule_status");
        }
    }
}
