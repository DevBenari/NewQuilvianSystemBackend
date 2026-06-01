using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPaymentMethodConfiguration : IEntityTypeConfiguration<MstPaymentMethod>
    {
        public void Configure(EntityTypeBuilder<MstPaymentMethod> entity)
        {
            entity.ToTable("MstPaymentMethod", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PaymentMethodCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PaymentMethodName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.PaymentMethodType)
                .HasMaxLength(50)
                .HasDefaultValue("Cash")
                .IsRequired();

            entity.Property(x => x.PaymentGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.IsCash)
                .HasDefaultValue(false);

            entity.Property(x => x.IsBankTransfer)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCardPayment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsQris)
                .HasDefaultValue(false);

            entity.Property(x => x.IsInsurance)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCompanyGuarantor)
                .HasDefaultValue(false);

            entity.Property(x => x.IsMembership)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedReferenceNumber)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedAttachment)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAvailableForRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForBilling)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForRefund)
                .HasDefaultValue(true);

            entity.Property(x => x.BankName)
                .HasMaxLength(100);

            entity.Property(x => x.BankAccountNumber)
                .HasMaxLength(100);

            entity.Property(x => x.BankAccountName)
                .HasMaxLength(200);

            entity.Property(x => x.MerchantId)
                .HasMaxLength(100);

            entity.Property(x => x.TerminalId)
                .HasMaxLength(100);

            entity.Property(x => x.ExternalPaymentCode)
                .HasMaxLength(50);

            entity.Property(x => x.IntegrationCode)
                .HasMaxLength(50);

            entity.Property(x => x.AdminFeeAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.AdminFeePercent)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasIndex(x => x.PaymentMethodCode)
                .IsUnique();

            entity.HasIndex(x => x.PaymentMethodName);

            entity.HasIndex(x => x.PaymentMethodType);

            entity.HasIndex(x => x.PaymentGroupName);

            entity.HasIndex(x => x.ExternalPaymentCode)
                .HasFilter("\"ExternalPaymentCode\" IS NOT NULL");

            entity.HasIndex(x => x.IntegrationCode)
                .HasFilter("\"IntegrationCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.PaymentMethodType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAvailableForRegistration,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAvailableForBilling,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsCash,
                x.IsInsurance,
                x.IsCompanyGuarantor,
                x.IsMembership,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
