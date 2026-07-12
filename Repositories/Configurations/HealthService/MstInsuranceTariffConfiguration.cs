using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstInsuranceTariffConfiguration : IEntityTypeConfiguration<MstInsuranceTariff>
    {
        public void Configure(EntityTypeBuilder<MstInsuranceTariff> entity)
        {
            entity.ToTable("MstInsuranceTariff", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InsuranceProviderId).IsRequired();
            entity.Property(x => x.TariffId).IsRequired();
            entity.Property(x => x.InsuranceTariffCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.InsuranceTariffName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.ExternalServiceCode).HasMaxLength(50);
            entity.Property(x => x.ExternalClassCode).HasMaxLength(50);
            entity.Property(x => x.BenefitPlanCode).HasMaxLength(100);
            entity.Property(x => x.BenefitPlanName).HasMaxLength(150);
            entity.Property(x => x.ContractPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.HospitalPriceSnapshot).HasColumnType("numeric(18,2)");
            entity.Property(x => x.DiscountAmount).HasColumnType("numeric(18,2)");
            entity.Property(x => x.DiscountPercent).HasColumnType("numeric(5,2)");
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date");
            entity.Property(x => x.BillingInstruction).HasMaxLength(250);
            entity.Property(x => x.ClaimInstruction).HasMaxLength(250);
            entity.Property(x => x.Description).HasMaxLength(250);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.InsuranceProvider).WithMany().HasForeignKey(x => x.InsuranceProviderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Tariff).WithMany().HasForeignKey(x => x.TariffId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PatientClass).WithMany().HasForeignKey(x => x.PatientClassId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.InsuranceTariffCode).IsUnique();
            entity.HasIndex(x => x.InsuranceProviderId);
            entity.HasIndex(x => x.TariffId);
            entity.HasIndex(x => x.PatientClassId);
            entity.HasIndex(x => x.BenefitPlanCode);
            entity.HasIndex(x => new { x.InsuranceProviderId, x.TariffId, x.BenefitPlanCode, x.PatientClassId, x.Priority, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }
    }
}
