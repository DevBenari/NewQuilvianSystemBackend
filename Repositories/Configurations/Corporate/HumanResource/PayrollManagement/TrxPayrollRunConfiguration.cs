using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollRunConfiguration : IEntityTypeConfiguration<TrxPayrollRun>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollRun> entity)
        {

            entity.ToTable("TrxPayrollRun", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RunNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RunType).HasMaxLength(30).HasDefaultValue("Regular").IsRequired();
            entity.Property(x => x.RunStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.PeriodStartDateSnapshot).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PeriodEndDateSnapshot).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AttendanceCutoffDateSnapshot).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VariableInputCutoffDateSnapshot).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaymentDateSnapshot).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.TotalBaseSalary).HasPrecision(18, 2);
            entity.Property(x => x.TotalEarning).HasPrecision(18, 2);
            entity.Property(x => x.TotalDeduction).HasPrecision(18, 2);
            entity.Property(x => x.TotalTax).HasPrecision(18, 2);
            entity.Property(x => x.TotalEmployeeContribution).HasPrecision(18, 2);
            entity.Property(x => x.TotalEmployerContribution).HasPrecision(18, 2);
            entity.Property(x => x.TotalGrossPay).HasPrecision(18, 2);
            entity.Property(x => x.TotalNetPay).HasPrecision(18, 2);
            entity.Property(x => x.TotalPaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CalculationStartedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PolicySnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ConfigurationSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidationSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LockedByUser).WithMany().HasForeignKey(x => x.LockedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CalculatedByUser).WithMany().HasForeignKey(x => x.CalculatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RunNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollPeriodId, x.RunType, x.IsDelete });
            entity.HasIndex(x => new { x.RunStatus, x.IsLocked, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.FinancePaymentBatchId, x.GlHeaderId });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
