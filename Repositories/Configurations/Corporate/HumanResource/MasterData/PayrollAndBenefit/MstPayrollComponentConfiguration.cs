using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstPayrollComponentConfiguration : IEntityTypeConfiguration<MstPayrollComponent>
    {
        public void Configure(EntityTypeBuilder<MstPayrollComponent> entity)
        {
            entity.ToTable("MstPayrollComponent", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PayrollComponentCategoryId).IsRequired();
            entity.Property(x => x.PayrollComponentCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PayrollComponentName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ComponentType).HasMaxLength(50).HasDefaultValue("Earning").IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(50).HasDefaultValue("Fixed").IsRequired();
            entity.Property(x => x.FormulaExpression).HasMaxLength(1000);
            entity.Property(x => x.DefaultAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.DefaultPercentage).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.IsRecurring).HasDefaultValue(true);
            entity.Property(x => x.IsTaxable).HasDefaultValue(true);
            entity.Property(x => x.IsProrated).HasDefaultValue(true);
            entity.Property(x => x.IsAttendanceBased).HasDefaultValue(false);
            entity.Property(x => x.IsOvertimeBased).HasDefaultValue(false);
            entity.Property(x => x.IsBenefitBased).HasDefaultValue(false);
            entity.Property(x => x.IsEmployerContribution).HasDefaultValue(false);
            entity.Property(x => x.IsEmployeeContribution).HasDefaultValue(false);
            entity.Property(x => x.IsDisplayedOnPayslip).HasDefaultValue(true);
            entity.Property(x => x.IsEditableDuringPayroll).HasDefaultValue(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.PayrollComponentCategory)
                .WithMany(x => x.PayrollComponents)
                .HasForeignKey(x => x.PayrollComponentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BaseComponent)
                .WithMany(x => x.DerivedComponents)
                .HasForeignKey(x => x.BaseComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PayrollComponentCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PayrollComponentName);
            entity.HasIndex(x => x.PayrollComponentCategoryId);
            entity.HasIndex(x => x.BaseComponentId);
            entity.HasIndex(x => new { x.ComponentType, x.CalculationMethod, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsTaxable, x.IsProrated, x.IsRecurring, x.IsActive });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
