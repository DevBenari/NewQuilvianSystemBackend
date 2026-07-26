using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollRunEmployeeConfiguration : IEntityTypeConfiguration<TrxPayrollRunEmployee>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollRunEmployee> entity)
        {

            entity.ToTable("TrxPayrollRunEmployee", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EmployeePayrollStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.EmployeeNumberSnapshot).HasMaxLength(50);
            entity.Property(x => x.EmployeeNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DepartmentNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.PositionNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.EmployeeGradeSnapshot).HasMaxLength(100);
            entity.Property(x => x.CostCenterCodeSnapshot).HasMaxLength(100);
            entity.Property(x => x.CostCenterNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.BankNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.BankAccountNumberSnapshot).HasMaxLength(100);
            entity.Property(x => x.BankAccountHolderSnapshot).HasMaxLength(200);
            entity.Property(x => x.TaxStatusSnapshot).HasMaxLength(50);
            entity.Property(x => x.NpwpNumberSnapshot).HasMaxLength(50);
            entity.Property(x => x.BaseSalary).HasPrecision(18, 2);
            entity.Property(x => x.TotalRecurringEarning).HasPrecision(18, 2);
            entity.Property(x => x.TotalVariableEarning).HasPrecision(18, 2);
            entity.Property(x => x.TotalOvertimeAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAttendanceAllowance).HasPrecision(18, 2);
            entity.Property(x => x.TotalTransportAllowance).HasPrecision(18, 2);
            entity.Property(x => x.TotalBenefit).HasPrecision(18, 2);
            entity.Property(x => x.TotalDeduction).HasPrecision(18, 2);
            entity.Property(x => x.TotalTax).HasPrecision(18, 2);
            entity.Property(x => x.TotalEmployeeInsuranceContribution).HasPrecision(18, 2);
            entity.Property(x => x.TotalEmployerInsuranceContribution).HasPrecision(18, 2);
            entity.Property(x => x.GrossPay).HasPrecision(18, 2);
            entity.Property(x => x.NetPay).HasPrecision(18, 2);
            entity.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidLeaveDays).HasPrecision(9, 2);
            entity.Property(x => x.UnpaidLeaveDays).HasPrecision(9, 2);
            entity.Property(x => x.AbsentDays).HasPrecision(9, 2);
            entity.Property(x => x.SnapshotFrozenAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EmployeeSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.SalarySnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.TaxSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.InsuranceSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.CalculationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.ValidationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRun).WithMany(x => x.Employees).HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollProfile).WithMany(x => x.PayrollRunEmployees).HasForeignKey(x => x.PayrollProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TaxProfile).WithMany().HasForeignKey(x => x.TaxProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InsuranceProfile).WithMany().HasForeignKey(x => x.InsuranceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryAssignment).WithMany().HasForeignKey(x => x.SalaryAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SnapshotFrozenByUser).WithMany().HasForeignKey(x => x.SnapshotFrozenByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinalizedByUser).WithMany().HasForeignKey(x => x.FinalizedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PayrollRunId, x.WorkforceProfileId }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollRunId, x.EmployeePayrollStatus, x.IsDelete });
            entity.HasIndex(x => new { x.WorkforceProfileId, x.IsFinalized, x.IsDelete });
            entity.HasIndex(x => new { x.CostCenterId, x.PayrollRunId, x.IsDelete });
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
