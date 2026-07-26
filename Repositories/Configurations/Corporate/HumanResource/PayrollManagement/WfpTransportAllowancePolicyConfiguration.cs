using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class WfpTransportAllowancePolicyConfiguration : IEntityTypeConfiguration<WfpTransportAllowancePolicy>
    {
        public void Configure(EntityTypeBuilder<WfpTransportAllowancePolicy> entity)
        {

            entity.ToTable("WfpTransportAllowancePolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(30).HasDefaultValue("FixedMonthly").IsRequired();
            entity.Property(x => x.FixedMonthlyAmount).HasPrecision(18, 2);
            entity.Property(x => x.PerAttendanceAmount).HasPrecision(18, 2);
            entity.Property(x => x.DailyLimitAmount).HasPrecision(18, 2);
            entity.Property(x => x.MonthlyLimitAmount).HasPrecision(18, 2);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PolicyCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.EmployeeGradeId, x.IsActive, x.IsDelete });
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
