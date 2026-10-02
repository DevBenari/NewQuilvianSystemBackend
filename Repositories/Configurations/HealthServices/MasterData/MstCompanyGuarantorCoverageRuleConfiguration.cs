using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstCompanyGuarantorCoverageRuleConfiguration : IEntityTypeConfiguration<MstCompanyGuarantorCoverageRule>
    {
        public void Configure(EntityTypeBuilder<MstCompanyGuarantorCoverageRule> entity)
        {
            entity.ToTable("MstCompanyGuarantorCoverageRule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CompanyGuarantorId).IsRequired();
            entity.Property(x => x.RuleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RuleName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ItemType).HasMaxLength(30).HasDefaultValue("Tariff").IsRequired();
            entity.Property(x => x.BenefitPlanCode).HasMaxLength(100);
            entity.Property(x => x.BenefitPlanName).HasMaxLength(150);
            entity.Property(x => x.EmployeeGrade).HasMaxLength(50);
            entity.Property(x => x.CoverageStatus).HasMaxLength(30).HasDefaultValue("Covered").IsRequired();
            entity.Property(x => x.CoveragePercent).HasColumnType("numeric(5,2)").HasDefaultValue(100m).IsRequired();
            entity.Property(x => x.MaxCoverageAmount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.CoPaymentPercent).HasColumnType("numeric(5,2)");
            entity.Property(x => x.CoPaymentAmount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.IsNeedApproval).HasDefaultValue(false).IsRequired();
            entity.Property(x => x.IsNeedGuaranteeLetter).HasDefaultValue(false).IsRequired();
            entity.Property(x => x.IsAllowExcessPaymentByPatient).HasDefaultValue(true).IsRequired();
            entity.Property(x => x.MaxQuantityPerVisit).HasColumnType("numeric(18,3)");
            entity.Property(x => x.MaxQuantityPerMonth).HasColumnType("numeric(18,3)");
            entity.Property(x => x.MaxAmountPerVisit).HasColumnType("numeric(18,2)");
            entity.Property(x => x.MaxAmountPerMonth).HasColumnType("numeric(18,2)");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.Priority).HasDefaultValue(0).IsRequired();
            entity.Property(x => x.ApprovalInstruction).HasMaxLength(500);
            entity.Property(x => x.BillingInstruction).HasMaxLength(500);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

            // IdentityModel audit fields
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            // Relationships (all Restrict)
            entity.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
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

            entity.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Filtered unique index: unik per (CompanyGuarantorId, RuleCode) untuk baris yang belum dihapus
            entity.HasIndex(x => new { x.CompanyGuarantorId, x.RuleCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_MstCompanyGuarantorCoverageRule_Company_RuleCode");

            // Other indexes
            entity.HasIndex(x => x.CompanyGuarantorId);
            entity.HasIndex(x => x.ItemType);
            entity.HasIndex(x => x.TariffId);
            entity.HasIndex(x => x.DrugId);
            entity.HasIndex(x => x.DrugCategoryId);
            entity.HasIndex(x => x.ProcedureId);
            entity.HasIndex(x => x.TariffCategoryId);
            entity.HasIndex(x => x.PatientClassId);
            entity.HasIndex(x => x.BenefitPlanCode);
            entity.HasIndex(x => x.EmployeeGrade);
            entity.HasIndex(x => x.EffectiveStartDate);
            entity.HasIndex(x => x.EffectiveEndDate);
            entity.HasIndex(x => x.Priority);
            entity.HasIndex(x => x.IsActive);
        }
    }
}
