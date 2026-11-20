using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.PayrollManagement
{
    public class PrlTransportAllowancePolicyConfiguration : IEntityTypeConfiguration<PrlTransportAllowancePolicy>
    {
        public void Configure(EntityTypeBuilder<PrlTransportAllowancePolicy> entity)
        {

            // Nama check constraint sama persis dengan DDL yang diterapkan pemilik (PAY-BE-FIX-013 bagian 3, terkoreksi).
            entity.ToTable("PrlTransportAllowancePolicy", "public", table =>
                table.HasCheckConstraint(
                    "CK_PrlTransportAllowancePolicy_StructureRequiresGrade",
                    "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL"));
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

            // PAY-BE-FIX-014: cakupan Grade Gaji dan Struktur Gaji. Nama diberikan eksplisit (maksimum 58 karakter, di bawah batas 63
            // PostgreSQL) dan identik dengan DDL, karena nama otomatis EF yang terlalu panjang dipotong dan tidak akan cocok dengan database.
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId)
                .HasConstraintName("FK_PrlTransportAllowancePolicy_SalaryGrade").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryStructure).WithMany().HasForeignKey(x => x.SalaryStructureId)
                .HasConstraintName("FK_PrlTransportAllowancePolicy_SalaryStructure").OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PolicyCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.EmployeeGradeId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
            entity.HasIndex(x => new { x.SalaryGradeId, x.SalaryStructureId, x.IsActive, x.IsDelete })
                .HasDatabaseName("IX_PrlTransportAllowancePolicy_SalaryGrade_SalaryStructure");
            entity.HasIndex(x => x.SalaryStructureId).HasDatabaseName("IX_PrlTransportAllowancePolicy_SalaryStructureId");
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
