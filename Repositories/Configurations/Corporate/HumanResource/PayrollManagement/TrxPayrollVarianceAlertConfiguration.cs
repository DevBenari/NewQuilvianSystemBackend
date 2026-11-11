using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollVarianceAlertConfiguration : IEntityTypeConfiguration<TrxPayrollVarianceAlert>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollVarianceAlert> builder)
        {
            builder.ToTable("TrxPayrollVarianceAlert", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PreviousNetPay).HasPrecision(18, 2);
            builder.Property(x => x.CurrentNetPay).HasPrecision(18, 2);
            builder.Property(x => x.VarianceAmount).HasPrecision(18, 2);
            builder.Property(x => x.VariancePercentage).HasPrecision(5, 2);

            builder.Property(x => x.AlertSeverity).HasMaxLength(30).IsRequired();
            builder.Property(x => x.AlertReason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.ResolutionStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.JustificationNotes).HasMaxLength(2000);

            builder.HasOne(x => x.PayrollRun)
                .WithMany()
                .HasForeignKey(x => x.PayrollRunId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResolvedByUser)
                .WithMany()
                .HasForeignKey(x => x.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PayrollRunId, x.WorkforceProfileId })
                .HasDatabaseName("idx_payroll_variance_run_emp");

            builder.HasIndex(x => x.ResolutionStatus)
                .HasDatabaseName("idx_payroll_variance_status");
        }
    }
}
