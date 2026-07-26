using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstOnCallAllowancePolicyConfiguration : IEntityTypeConfiguration<MstOnCallAllowancePolicy>
    {
        public void Configure(EntityTypeBuilder<MstOnCallAllowancePolicy> entity)
        {
            entity.ToTable("MstOnCallAllowancePolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AllowanceTypeId).IsRequired();
            entity.Property(x => x.OnCallAllowancePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OnCallAllowancePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(50).HasDefaultValue("FixedPerAssignment").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.BaseRateAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.ActualCallRateAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.HourlyRateAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.PercentageOfBaseSalary).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.MinimumOnCallHours).HasDefaultValue(0);
            entity.Property(x => x.MaximumAmountPerPeriod).HasPrecision(18, 2);
            entity.Property(x => x.WeekendMultiplier).HasPrecision(9, 4).HasDefaultValue(1m);
            entity.Property(x => x.HolidayMultiplier).HasPrecision(9, 4).HasDefaultValue(1m);
            entity.Property(x => x.RequireAttendanceEvidence).HasDefaultValue(false);
            entity.Property(x => x.RequireSupervisorVerification).HasDefaultValue(true);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.AllowanceType)
                .WithMany(x => x.OnCallAllowancePolicies)
                .HasForeignKey(x => x.AllowanceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OnCallType)
                .WithMany()
                .HasForeignKey(x => x.OnCallTypeId)
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

            entity.HasIndex(x => x.OnCallAllowancePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.OnCallAllowancePolicyName);
            entity.HasIndex(x => x.AllowanceTypeId);
            entity.HasIndex(x => x.OnCallTypeId);
            entity.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.Priority, x.IsDefault, x.IsActive, x.IsDelete });
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
