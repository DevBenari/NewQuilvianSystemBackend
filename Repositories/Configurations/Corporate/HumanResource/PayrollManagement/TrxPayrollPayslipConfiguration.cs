using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollPayslipConfiguration : IEntityTypeConfiguration<TrxPayrollPayslip>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollPayslip> entity)
        {

            entity.ToTable("TrxPayrollPayslip", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PayslipNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PayslipStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.PeriodStartDateSnapshot).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PeriodEndDateSnapshot).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EmployeeNumberSnapshot).HasMaxLength(50);
            entity.Property(x => x.EmployeeNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DepartmentNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.PositionNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.BaseSalary).HasPrecision(18, 2);
            entity.Property(x => x.TotalEarning).HasPrecision(18, 2);
            entity.Property(x => x.TotalDeduction).HasPrecision(18, 2);
            entity.Property(x => x.TotalTax).HasPrecision(18, 2);
            entity.Property(x => x.GrossPay).HasPrecision(18, 2);
            entity.Property(x => x.NetPay).HasPrecision(18, 2);
            entity.Property(x => x.FilePath).HasMaxLength(500);
            entity.Property(x => x.FileName).HasMaxLength(255);
            entity.Property(x => x.ContentType).HasMaxLength(100);
            entity.Property(x => x.FileChecksum).HasMaxLength(128);
            entity.Property(x => x.PayslipSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.GeneratedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.FirstDownloadedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.LastDownloadedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRun).WithMany().HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRunEmployee).WithOne(x => x.Payslip).HasForeignKey<TrxPayrollPayslip>(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.GeneratedByUser).WithMany().HasForeignKey(x => x.GeneratedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PublishedByUser).WithMany().HasForeignKey(x => x.PublishedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PayslipNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.PayrollRunEmployeeId).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollRunId, x.PayslipStatus, x.IsEmployeeVisible, x.IsDelete });
            entity.HasIndex(x => new { x.FileChecksum, x.IsDelete });
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
