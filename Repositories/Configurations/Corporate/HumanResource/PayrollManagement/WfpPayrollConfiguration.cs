using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class WfpPayrollConfiguration : IEntityTypeConfiguration<WfpPayroll>
    {
        public void Configure(EntityTypeBuilder<WfpPayroll> entity)
        {

            entity.ToTable("WfpPayroll", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PayrollNumber).HasMaxLength(50);
            entity.Property(x => x.PayrollGroupCode).HasMaxLength(50);
            entity.Property(x => x.PayrollStatus).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.PaymentFrequency).HasMaxLength(30).HasDefaultValue("Monthly").IsRequired();
            entity.Property(x => x.PaymentMethod).HasMaxLength(30).HasDefaultValue("BankTransfer").IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.BaseSalary).HasPrecision(18, 2);
            entity.Property(x => x.TotalAllowance).HasPrecision(18, 2);
            entity.Property(x => x.TotalDeduction).HasPrecision(18, 2);
            entity.Property(x => x.GrossSalary).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.InsuranceAmount).HasPrecision(18, 2);
            entity.Property(x => x.NetSalary).HasPrecision(18, 2);
            entity.Property(x => x.LastCalculatedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryAssignment).WithMany().HasForeignKey(x => x.SalaryAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LastPayrollPeriod).WithMany().HasForeignKey(x => x.LastPayrollPeriodId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.PayrollNumber).IsUnique().HasFilter("\"PayrollNumber\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollStatus, x.IsPayrollEligible, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
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
