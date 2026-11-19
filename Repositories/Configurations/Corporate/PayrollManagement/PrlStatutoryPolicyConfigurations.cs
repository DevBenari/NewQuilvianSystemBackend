using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.PayrollManagement
{
    /// <summary>Konfigurasi kebijakan tarif PPh berversi (PrlTaxPolicy).</summary>
    public class PrlTaxPolicyConfiguration : IEntityTypeConfiguration<PrlTaxPolicy>
    {
        public void Configure(EntityTypeBuilder<PrlTaxPolicy> entity)
        {
            entity.ToTable("PrlTaxPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PolicyVersion).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LegalBasis).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PolicyStatus).HasMaxLength(30).HasDefaultValue(StatutoryPolicyStatuses.Draft).IsRequired();
            entity.Property(x => x.PositionCostRatePercent).HasPrecision(9, 4);
            entity.Property(x => x.PositionCostMonthlyCap).HasPrecision(18, 2);
            entity.Property(x => x.PositionCostAnnualCap).HasPrecision(18, 2);
            entity.Property(x => x.NoNpwpSurchargePercent).HasPrecision(9, 4);
            entity.Property(x => x.PkpRoundingUnit).HasPrecision(18, 2);
            entity.Property(x => x.EmployerPremiumPrograms).HasMaxLength(50);
            entity.Property(x => x.PensionDeductionPrograms).HasMaxLength(50);
            entity.Property(x => x.NonEmployeeBasisPercent).HasPrecision(9, 4);
            entity.Property(x => x.NonEmployeeMethod).HasMaxLength(30);
            entity.Property(x => x.Pph26RatePercent).HasPrecision(9, 4);
            entity.Property(x => x.PtkpJson).HasColumnType("text").HasDefaultValue("[]").IsRequired();
            entity.Property(x => x.TerRatesJson).HasColumnType("text").HasDefaultValue("[]").IsRequired();
            entity.Property(x => x.BracketsJson).HasColumnType("text").HasDefaultValue("[]").IsRequired();
            entity.Property(x => x.StatusReason).HasMaxLength(500);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RetiredAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            PrlStatutoryPolicyConfigurationSupport.ConfigureIdentity(entity);

            entity.HasIndex(x => x.PolicyVersion).IsUnique().HasFilter("\"IsDelete\" = false");
            // Mesin hitung memuat kebijakan APPROVED yang berlaku pada periode.
            entity.HasIndex(x => new { x.PolicyStatus, x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
        }
    }

    /// <summary>Konfigurasi kebijakan tarif dan batas iuran BPJS berversi (PrlBpjsPolicy).</summary>
    public class PrlBpjsPolicyConfiguration : IEntityTypeConfiguration<PrlBpjsPolicy>
    {
        public void Configure(EntityTypeBuilder<PrlBpjsPolicy> entity)
        {
            entity.ToTable("PrlBpjsPolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PolicyVersion).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LegalBasis).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PolicyStatus).HasMaxLength(30).HasDefaultValue(StatutoryPolicyStatuses.Draft).IsRequired();
            entity.Property(x => x.JkkRiskBasis).HasMaxLength(1000);
            entity.Property(x => x.HealthCapAmount).HasPrecision(18, 2);
            entity.Property(x => x.HealthEmployeeRate).HasPrecision(9, 4);
            entity.Property(x => x.HealthEmployerRate).HasPrecision(9, 4);
            entity.Property(x => x.JhtEmployeeRate).HasPrecision(9, 4);
            entity.Property(x => x.JhtEmployerRate).HasPrecision(9, 4);
            entity.Property(x => x.JpCapAmount).HasPrecision(18, 2);
            entity.Property(x => x.JpEmployeeRate).HasPrecision(9, 4);
            entity.Property(x => x.JpEmployerRate).HasPrecision(9, 4);
            entity.Property(x => x.JkkRate).HasPrecision(9, 4);
            entity.Property(x => x.JkmRate).HasPrecision(9, 4);
            entity.Property(x => x.StatusReason).HasMaxLength(500);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RetiredAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            PrlStatutoryPolicyConfigurationSupport.ConfigureIdentity(entity);

            entity.HasIndex(x => x.PolicyVersion).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PolicyStatus, x.EffectiveStartDate, x.EffectiveEndDate, x.IsDelete });
        }
    }

    internal static class PrlStatutoryPolicyConfigurationSupport
    {
        public static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
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
