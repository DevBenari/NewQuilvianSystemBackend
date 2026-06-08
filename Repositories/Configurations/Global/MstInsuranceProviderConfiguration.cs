using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstInsuranceProviderConfiguration : IEntityTypeConfiguration<MstInsuranceProvider>
    {
        public void Configure(EntityTypeBuilder<MstInsuranceProvider> entity)
        {
            entity.ToTable("MstInsuranceProvider", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InsuranceProviderCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.InsuranceProviderName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.InsuranceGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.ProviderType)
                .HasMaxLength(50)
                .HasDefaultValue("PrivateInsurance")
                .IsRequired();

            entity.Property(x => x.ClaimMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Cashless")
                .IsRequired();

            entity.Property(x => x.ExternalProviderCode)
                .HasMaxLength(50);

            entity.Property(x => x.IntegrationCode)
                .HasMaxLength(50);

            entity.Property(x => x.ContractNumber)
                .HasMaxLength(100);

            entity.Property(x => x.ContractStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ContractEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.IsUsingInsuranceTariffBook)
                .HasDefaultValue(true);

            entity.Property(x => x.IsUsingHospitalTariff)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedEligibilityCheck)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedGuaranteeLetter)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedReferralLetter)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedApprovalForProcedure)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedApprovalForDrug)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCoverageLimitedByPlan)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowExcessPaymentByPatient)
                .HasDefaultValue(true);

            entity.Property(x => x.PicName)
                .HasMaxLength(100);

            entity.Property(x => x.PicPhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.PicWhatsAppNumber)
                .HasMaxLength(30);

            entity.Property(x => x.PicEmail)
                .HasMaxLength(200);

            entity.Property(x => x.OfficeAddress)
                .HasMaxLength(500);

            entity.Property(x => x.LogoPath)
                .HasMaxLength(500);

            entity.Property(x => x.BillingInstruction)
                .HasMaxLength(250);

            entity.Property(x => x.ClaimInstruction)
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

            entity.HasIndex(x => x.InsuranceProviderCode)
                .IsUnique();

            entity.HasIndex(x => x.InsuranceProviderName);

            entity.HasIndex(x => x.ProviderType);

            entity.HasIndex(x => x.ClaimMethod);

            entity.HasIndex(x => x.ExternalProviderCode)
                .HasFilter("\"ExternalProviderCode\" IS NOT NULL");

            entity.HasIndex(x => x.IntegrationCode)
                .HasFilter("\"IntegrationCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ProviderType,
                x.ClaimMethod,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
