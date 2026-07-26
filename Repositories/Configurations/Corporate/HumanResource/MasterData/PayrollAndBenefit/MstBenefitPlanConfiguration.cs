using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstBenefitPlanConfiguration : IEntityTypeConfiguration<MstBenefitPlan>
    {
        public void Configure(EntityTypeBuilder<MstBenefitPlan> entity)
        {
            entity.ToTable("MstBenefitPlan", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BenefitTypeId).IsRequired();
            entity.Property(x => x.BenefitPlanCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BenefitPlanName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ProviderName).HasMaxLength(200);
            entity.Property(x => x.ExternalPlanCode).HasMaxLength(100);
            entity.Property(x => x.PolicyNumber).HasMaxLength(100);
            entity.Property(x => x.CoverageType).HasMaxLength(50).HasDefaultValue("Individual").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.CoverageLimitAmount).HasPrecision(18, 2);
            entity.Property(x => x.EmployerContributionAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.EmployerContributionPercentage).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.EmployeeContributionAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.EmployeeContributionPercentage).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.WaitingPeriodMonths).HasDefaultValue(0);
            entity.Property(x => x.MaximumDependents).HasDefaultValue(0);
            entity.Property(x => x.EnrollmentStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EnrollmentEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.BenefitType)
                .WithMany(x => x.BenefitPlans)
                .HasForeignKey(x => x.BenefitTypeId)
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

            entity.HasIndex(x => x.BenefitPlanCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.BenefitPlanName);
            entity.HasIndex(x => x.BenefitTypeId);
            entity.HasIndex(x => x.ProviderName);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.BenefitTypeId, x.IsDefault, x.IsActive, x.IsDelete });
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
