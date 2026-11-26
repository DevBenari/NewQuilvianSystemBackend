using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstHazardAllowancePolicyConfiguration : IEntityTypeConfiguration<MstHazardAllowancePolicy>
    {
        public void Configure(EntityTypeBuilder<MstHazardAllowancePolicy> entity)
        {
            // Nama constraint/indeks sama persis dengan DDL cakupan Grade Gaji (laporan PAY-BE-FIX-018).
            entity.ToTable("MstHazardAllowancePolicy", "public", table =>
                table.HasCheckConstraint(
                    "CK_MstHazardAllowancePolicy_StructureRequiresGrade",
                    "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL"));
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AllowanceTypeId).IsRequired();
            entity.Property(x => x.HazardAllowancePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.HazardAllowancePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.HazardLevel).HasMaxLength(50).HasDefaultValue("Low").IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(50).HasDefaultValue("FixedMonthly").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.RateAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.PercentageOfBaseSalary).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.MinimumExposureDays).HasDefaultValue(0);
            entity.Property(x => x.MaximumAmountPerPeriod).HasPrecision(18, 2);
            entity.Property(x => x.RequireOccupationalHealthClearance).HasDefaultValue(true);
            entity.Property(x => x.RequireActiveAssignment).HasDefaultValue(true);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.AllowanceType)
                .WithMany(x => x.HazardAllowancePolicies)
                .HasForeignKey(x => x.AllowanceTypeId)
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

            entity.HasOne(x => x.WorkLocation)
                .WithMany()
                .HasForeignKey(x => x.WorkLocationId)
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
                .HasConstraintName("FK_MstHazardAllowancePolicy_EmployeeGrade").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId)
                .HasConstraintName("FK_MstHazardAllowancePolicy_SalaryGrade").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId)
                .HasConstraintName("FK_MstHazardAllowancePolicy_SalaryStructure").OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.HazardAllowancePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.HazardAllowancePolicyName);
            entity.HasIndex(x => x.AllowanceTypeId);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.WorkLocationId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.SalaryGradeId, x.SalaryStructureId, x.IsActive, x.IsDelete })
                .HasDatabaseName("IX_MstHazardAllowancePolicy_SalaryGrade_SalaryStructure");
            entity.HasIndex(x => x.SalaryStructureId).HasDatabaseName("IX_MstHazardAllowancePolicy_SalaryStructureId");
            entity.HasIndex(x => x.EmployeeGradeId).HasDatabaseName("IX_MstHazardAllowancePolicy_EmployeeGradeId");
            entity.HasIndex(x => new { x.HazardLevel, x.Priority, x.IsDefault, x.IsActive, x.IsDelete });
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
