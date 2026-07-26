using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstSalaryStructureConfiguration : IEntityTypeConfiguration<MstSalaryStructure>
    {
        public void Configure(EntityTypeBuilder<MstSalaryStructure> entity)
        {
            entity.ToTable("MstSalaryStructure", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SalaryGradeId).IsRequired();
            entity.Property(x => x.SalaryStructureCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SalaryStructureName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.PaymentFrequency).HasMaxLength(50).HasDefaultValue("Monthly").IsRequired();
            entity.Property(x => x.DefaultBaseSalary).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.MinimumBaseSalary).HasPrecision(18, 2);
            entity.Property(x => x.MaximumBaseSalary).HasPrecision(18, 2);
            entity.Property(x => x.StandardWorkingDaysPerMonth).HasPrecision(8, 2).HasDefaultValue(22m);
            entity.Property(x => x.StandardWorkingHoursPerMonth).HasPrecision(8, 2).HasDefaultValue(173m);
            entity.Property(x => x.IsProrated).HasDefaultValue(true);
            entity.Property(x => x.IncludeOvertime).HasDefaultValue(true);
            entity.Property(x => x.IncludeShiftAllowance).HasDefaultValue(true);
            entity.Property(x => x.IncludeOnCallAllowance).HasDefaultValue(true);
            entity.Property(x => x.IncludeHazardAllowance).HasDefaultValue(true);
            entity.Property(x => x.IncludeBenefitDeduction).HasDefaultValue(true);
            entity.Property(x => x.ComponentConfigurationJson).HasColumnType("jsonb").IsRequired(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.SalaryGrade)
                .WithMany(x => x.SalaryStructures)
                .HasForeignKey(x => x.SalaryGradeId)
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

            entity.HasIndex(x => x.SalaryStructureCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.SalaryStructureName);
            entity.HasIndex(x => x.SalaryGradeId);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.SalaryGradeId, x.IsDefault, x.IsActive, x.IsDelete });
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
