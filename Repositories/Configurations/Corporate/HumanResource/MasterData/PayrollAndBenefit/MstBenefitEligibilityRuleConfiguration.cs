using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstBenefitEligibilityRuleConfiguration : IEntityTypeConfiguration<MstBenefitEligibilityRule>
    {
        public void Configure(EntityTypeBuilder<MstBenefitEligibilityRule> entity)
        {
            entity.ToTable("MstBenefitEligibilityRule", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BenefitPlanId).IsRequired();
            entity.Property(x => x.EligibilityRuleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EligibilityRuleName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.MinimumServiceMonths).HasDefaultValue(0);
            entity.Property(x => x.AllowProbationEmployee).HasDefaultValue(false);
            entity.Property(x => x.AllowContractEmployee).HasDefaultValue(true);
            entity.Property(x => x.RequireFullTimeEmployment).HasDefaultValue(false);
            entity.Property(x => x.MinimumWeeklyHours).HasPrecision(8, 2).HasDefaultValue(0m);
            entity.Property(x => x.CoverageStartOffsetDays).HasDefaultValue(0);
            entity.Property(x => x.CoverageEndAfterTerminationDays).HasDefaultValue(0);
            entity.Property(x => x.RequireManagerApproval).HasDefaultValue(false);
            entity.Property(x => x.RequireHrVerification).HasDefaultValue(true);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.BenefitPlan)
                .WithMany(x => x.EligibilityRules)
                .HasForeignKey(x => x.BenefitPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeCategory)
                .WithMany()
                .HasForeignKey(x => x.EmployeeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmploymentType)
                .WithMany()
                .HasForeignKey(x => x.EmploymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeGrade)
                .WithMany()
                .HasForeignKey(x => x.EmployeeGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SalaryGrade)
                .WithMany(x => x.BenefitEligibilityRules)
                .HasForeignKey(x => x.SalaryGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EligibilityRuleCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EligibilityRuleName);
            entity.HasIndex(x => x.BenefitPlanId);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId, x.EmployeeGradeId, x.SalaryGradeId });
            entity.HasIndex(x => new { x.Priority, x.IsActive, x.IsDelete });
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
