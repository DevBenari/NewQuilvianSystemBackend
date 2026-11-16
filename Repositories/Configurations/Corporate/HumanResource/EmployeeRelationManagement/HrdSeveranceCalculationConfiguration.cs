using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class HrdSeveranceCalculationConfiguration : IEntityTypeConfiguration<HrdSeveranceCalculation>
    {
        public void Configure(EntityTypeBuilder<HrdSeveranceCalculation> entity)
        {
            entity.ToTable("HrdSeveranceCalculation", "public");
            entity.HasKey(x => x.CalculationId);

            entity.Property(x => x.CalculationId).HasColumnName("calculation_id");
            entity.Property(x => x.SeparationId).HasColumnName("separation_id").IsRequired();
            entity.Property(x => x.BaseMonthlyWage).HasColumnName("base_monthly_wage").HasColumnType("numeric(14,2)").IsRequired();
            entity.Property(x => x.ServiceYears).HasColumnName("service_years").IsRequired();
            entity.Property(x => x.ServiceMonths).HasColumnName("service_months").IsRequired();
            entity.Property(x => x.SeverancePayAmount).HasColumnName("severance_pay_amount").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.ServiceAwardAmount).HasColumnName("service_award_amount").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.RightsCompensationAmount).HasColumnName("rights_compensation_amount").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.SeparationAllowanceAmount).HasColumnName("separation_allowance_amount").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.TotalSeveranceGross).HasColumnName("total_severance_gross").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.TaxDeduction).HasColumnName("tax_deduction").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.TotalSeveranceNet).HasColumnName("total_severance_net").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.IsApprovedByFinanceDirector).HasColumnName("is_approved_by_finance_director").HasDefaultValue(false);
            entity.Property(x => x.FinanceApprovalNotes).HasColumnName("finance_approval_notes");
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at").HasColumnType("timestamp with time zone");

            entity.HasIndex(x => x.SeparationId);
            entity.HasIndex(x => x.Status);
        }
    }
}
