using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollRestitutionConfiguration : IEntityTypeConfiguration<TrxPayrollRestitution>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollRestitution> builder)
        {
            builder.ToTable("TrxPayrollRestitution", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RestitutionNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RestitutionType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Claim)
                .WithMany()
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.AffectedPayrollRun)
                .WithMany()
                .HasForeignKey(x => x.AffectedPayrollRunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AffectedPayslip)
                .WithMany()
                .HasForeignKey(x => x.AffectedPayslipId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TargetPayrollRun)
                .WithMany()
                .HasForeignKey(x => x.TargetPayrollRunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RestitutionNumber)
                .IsUnique()
                .HasDatabaseName("uq_payroll_restitution_number");

            builder.HasIndex(x => new { x.EmployeeId, x.TargetPayrollRunId })
                .HasDatabaseName("idx_payroll_restitution_emp_target");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("idx_payroll_restitution_status");
        }
    }
}
