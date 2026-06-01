using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstInsuranceCoverageRuleConfiguration : IEntityTypeConfiguration<MstInsuranceCoverageRule>
    {
        public void Configure(EntityTypeBuilder<MstInsuranceCoverageRule> entity)
        {
            entity.ToTable("MstInsuranceCoverageRule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InsuranceProviderId)
                .IsRequired();

            entity.Property(x => x.RuleCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RuleName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ItemType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TariffId)
                .IsRequired(false);

            entity.Property(x => x.DrugId)
                .IsRequired(false);

            entity.Property(x => x.DrugCategoryId)
                .IsRequired(false);

            entity.Property(x => x.ProcedureId)
                .IsRequired(false);

            entity.Property(x => x.TariffCategoryId)
                .IsRequired(false);

            entity.Property(x => x.BenefitPlanCode)
                .HasMaxLength(100);

            entity.Property(x => x.BenefitPlanName)
                .HasMaxLength(150);

            entity.Property(x => x.PatientClassName)
                .HasMaxLength(100);

            entity.Property(x => x.CoverageStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Covered");

            entity.Property(x => x.CoveragePercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(100);

            entity.Property(x => x.MaxCoverageAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.CoPaymentPercent)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.CoPaymentAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.IsCovered)
                .HasDefaultValue(true);

            entity.Property(x => x.IsExcluded)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedGuaranteeLetter)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAllowExcessPaymentByPatient)
                .HasDefaultValue(true);

            entity.Property(x => x.MaxQuantityPerVisit)
                .IsRequired(false);

            entity.Property(x => x.MaxQuantityPerMonth)
                .IsRequired(false);

            entity.Property(x => x.MaxAmountPerVisit)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.MaxAmountPerMonth)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ApprovalInstruction)
                .HasMaxLength(250);

            entity.Property(x => x.BillingInstruction)
                .HasMaxLength(250);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.InsuranceProvider)
                .WithMany()
                .HasForeignKey(x => x.InsuranceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Drug)
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DrugCategory)
                .WithMany()
                .HasForeignKey(x => x.DrugCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TariffCategory)
                .WithMany()
                .HasForeignKey(x => x.TariffCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.InsuranceProviderId);

            entity.HasIndex(x => x.RuleCode)
                .IsUnique();

            entity.HasIndex(x => x.ItemType);

            entity.HasIndex(x => x.TariffId);

            entity.HasIndex(x => x.DrugId);

            entity.HasIndex(x => x.DrugCategoryId);

            entity.HasIndex(x => x.ProcedureId);

            entity.HasIndex(x => x.TariffCategoryId);

            entity.HasIndex(x => x.BenefitPlanCode);

            entity.HasIndex(x => new
            {
                x.InsuranceProviderId,
                x.ItemType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.InsuranceProviderId,
                x.BenefitPlanCode,
                x.ItemType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.InsuranceProviderId,
                x.CoverageStatus,
                x.IsNeedApproval,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
