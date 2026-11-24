using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.PayrollManagement
{
    /// <summary>Kebijakan Tunjangan umum. Nama constraint/indeks sama persis dengan DDL (laporan PAY-BE-FIX-021).</summary>
    public class PrlAllowancePolicyConfiguration : IEntityTypeConfiguration<PrlAllowancePolicy>
    {
        public void Configure(EntityTypeBuilder<PrlAllowancePolicy> entity)
        {
            entity.ToTable("PrlAllowancePolicy", "public", table =>
            {
                table.HasCheckConstraint("CK_PrlAllowancePolicy_Method", "\"CalculationMethod\" IN ('FixedAmount','PercentageOfBaseSalary')");
                table.HasCheckConstraint("CK_PrlAllowancePolicy_Dates", "\"EffectiveEndDate\" IS NULL OR \"EffectiveEndDate\" >= \"EffectiveStartDate\"");
                table.HasCheckConstraint("CK_PrlAllowancePolicy_Revision", "\"RevisionNo\" >= 1");
            });
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RevisionNo).HasDefaultValue(1);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            PrlAllowancePolicyGradeRateConfiguration.ConfigureIdentity(entity);

            entity.HasOne(x => x.AllowanceType).WithMany().HasForeignKey(x => x.AllowanceTypeId)
                .HasConstraintName("FK_PrlAllowancePolicy_AllowanceType").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId)
                .HasConstraintName("FK_PrlAllowancePolicy_PayrollComponent").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId)
                .HasConstraintName("FK_PrlAllowancePolicy_LegalEntity").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId)
                .HasConstraintName("FK_PrlAllowancePolicy_HospitalSite").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PrlAllowancePolicy>().WithMany().HasForeignKey(x => x.SupersedesPolicyId)
                .HasConstraintName("FK_PrlAllowancePolicy_Supersedes").OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PolicyCode).IsUnique().HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("UX_PrlAllowancePolicy_PolicyCode");
            entity.HasIndex(x => new { x.AllowanceTypeId, x.IsActive, x.IsDelete })
                .HasDatabaseName("IX_PrlAllowancePolicy_AllowanceType_Active");
            entity.HasIndex(x => x.PayrollComponentId).HasDatabaseName("IX_PrlAllowancePolicy_PayrollComponentId");
            entity.HasIndex(x => x.LegalEntityId).HasDatabaseName("IX_PrlAllowancePolicy_LegalEntityId");
            entity.HasIndex(x => x.HospitalSiteId).HasDatabaseName("IX_PrlAllowancePolicy_HospitalSiteId");
            entity.HasIndex(x => x.SupersedesPolicyId).HasDatabaseName("IX_PrlAllowancePolicy_SupersedesPolicyId");
        }
    }

    /// <summary>Tarif kebijakan per Grade Gaji.</summary>
    public class PrlAllowancePolicyGradeRateConfiguration : IEntityTypeConfiguration<PrlAllowancePolicyGradeRate>
    {
        public void Configure(EntityTypeBuilder<PrlAllowancePolicyGradeRate> entity)
        {
            entity.ToTable("PrlAllowancePolicyGradeRate", "public", table =>
            {
                table.HasCheckConstraint("CK_PrlAllowancePolicyGradeRate_Amounts",
                    "\"Amount\" >= 0 AND \"Percentage\" >= 0 AND \"Percentage\" <= 100 AND (\"MaximumAmount\" IS NULL OR \"MaximumAmount\" > 0)");
                table.HasCheckConstraint("CK_PrlAllowancePolicyGradeRate_Value", "\"Amount\" > 0 OR \"Percentage\" > 0");
                table.HasCheckConstraint("CK_PrlAllowancePolicyGradeRate_Dates", "\"EffectiveEndDate\" IS NULL OR \"EffectiveEndDate\" >= \"EffectiveStartDate\"");
            });
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Amount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.Percentage).HasPrecision(7, 2).HasDefaultValue(0m);
            entity.Property(x => x.MaximumAmount).HasPrecision(18, 2);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.AllowancePolicy).WithMany(x => x.GradeRates).HasForeignKey(x => x.AllowancePolicyId)
                .HasConstraintName("FK_PrlAllowancePolicyGradeRate_Policy").OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryGrade).WithMany().HasForeignKey(x => x.SalaryGradeId)
                .HasConstraintName("FK_PrlAllowancePolicyGradeRate_SalaryGrade").OnDelete(DeleteBehavior.Restrict);

            // Tarif untuk kebijakan + grade + tanggal mulai yang sama hanya satu; tumpang tindih rentang dijaga layanan dan pemeriksaan DDL.
            entity.HasIndex(x => new { x.AllowancePolicyId, x.SalaryGradeId, x.EffectiveStartDate })
                .IsUnique().HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("UX_PrlAllowancePolicyGradeRate_Policy_Grade_Start");
            entity.HasIndex(x => new { x.SalaryGradeId, x.IsActive, x.IsDelete })
                .HasDatabaseName("IX_PrlAllowancePolicyGradeRate_SalaryGrade_Active");
        }

        internal static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity) where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
