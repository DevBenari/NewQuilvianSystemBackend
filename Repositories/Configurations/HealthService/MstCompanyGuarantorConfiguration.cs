using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstCompanyGuarantorConfiguration : IEntityTypeConfiguration<MstCompanyGuarantor>
    {
        public void Configure(EntityTypeBuilder<MstCompanyGuarantor> entity)
        {
            entity.ToTable("MstCompanyGuarantor", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CompanyGuarantorCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CompanyGuarantorName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CompanyGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.GuarantorType)
                .HasMaxLength(50)
                .HasDefaultValue("Corporate")
                .IsRequired();

            entity.Property(x => x.BillingMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Invoice")
                .IsRequired();

            entity.Property(x => x.ExternalCompanyCode)
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

            entity.Property(x => x.IsUsingCompanyTariffBook)
                .HasDefaultValue(true);

            entity.Property(x => x.IsUsingHospitalTariff)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedGuaranteeLetter)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedEmployeeVerification)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedApprovalForProcedure)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedApprovalForDrug)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCoverageLimitedByEmployeeGrade)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowExcessPaymentByPatient)
                .HasDefaultValue(true);

            entity.Property(x => x.CreditLimitAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.CurrentOutstandingAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.PaymentDueDays)
                .HasDefaultValue(30);

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

            entity.HasIndex(x => x.CompanyGuarantorCode)
                .IsUnique();

            entity.HasIndex(x => x.CompanyGuarantorName);

            entity.HasIndex(x => x.CompanyGroupName);

            entity.HasIndex(x => x.GuarantorType);

            entity.HasIndex(x => x.BillingMethod);

            entity.HasIndex(x => x.ExternalCompanyCode)
                .HasFilter("\"ExternalCompanyCode\" IS NOT NULL");

            entity.HasIndex(x => x.IntegrationCode)
                .HasFilter("\"IntegrationCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.GuarantorType,
                x.BillingMethod,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ContractStartDate,
                x.ContractEndDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
