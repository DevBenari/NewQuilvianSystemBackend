using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstShiftAllowancePolicyConfiguration : IEntityTypeConfiguration<MstShiftAllowancePolicy>
    {
        public void Configure(EntityTypeBuilder<MstShiftAllowancePolicy> entity)
        {
            // Nama constraint/indeks sama persis dengan DDL cakupan Grade Gaji (laporan PAY-BE-FIX-018).
            entity.ToTable("MstShiftAllowancePolicy", "public", table =>
                table.HasCheckConstraint(
                    "CK_MstShiftAllowancePolicy_StructureRequiresGrade",
                    "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL"));
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AllowanceTypeId).IsRequired();
            entity.Property(x => x.ShiftAllowancePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ShiftAllowancePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(50).HasDefaultValue("FixedPerShift").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.RateAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.PercentageOfBaseSalary).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.MinimumEligibleMinutes).HasDefaultValue(0);
            entity.Property(x => x.MaximumAmountPerPeriod).HasPrecision(18, 2);
            entity.Property(x => x.ApplyOnWorkday).HasDefaultValue(true);
            entity.Property(x => x.ApplyOnWeekend).HasDefaultValue(true);
            entity.Property(x => x.ApplyOnHoliday).HasDefaultValue(true);
            entity.Property(x => x.ApplyOnlyNightShift).HasDefaultValue(false);
            entity.Property(x => x.RequireAttendanceMatch).HasDefaultValue(true);
            entity.Property(x => x.RequireCompletedShift).HasDefaultValue(true);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.AllowanceType)
                .WithMany(x => x.ShiftAllowancePolicies)
                .HasForeignKey(x => x.AllowanceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Shift)
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ShiftGroup)
                .WithMany()
                .HasForeignKey(x => x.ShiftGroupId)
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

            entity.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId)
                .HasConstraintName("FK_MstShiftAllowancePolicy_EmployeeGrade").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId)
                .HasConstraintName("FK_MstShiftAllowancePolicy_SalaryGrade").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId)
                .HasConstraintName("FK_MstShiftAllowancePolicy_SalaryStructure").OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ShiftAllowancePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ShiftAllowancePolicyName);
            entity.HasIndex(x => x.AllowanceTypeId);
            entity.HasIndex(x => new { x.ShiftId, x.ShiftGroupId });
            entity.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.SalaryGradeId, x.SalaryStructureId, x.IsActive, x.IsDelete })
                .HasDatabaseName("IX_MstShiftAllowancePolicy_SalaryGrade_SalaryStructure");
            entity.HasIndex(x => x.SalaryStructureId).HasDatabaseName("IX_MstShiftAllowancePolicy_SalaryStructureId");
            entity.HasIndex(x => x.EmployeeGradeId).HasDatabaseName("IX_MstShiftAllowancePolicy_EmployeeGradeId");
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
