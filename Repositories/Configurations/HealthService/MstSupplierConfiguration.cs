using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstSupplierConfiguration : IEntityTypeConfiguration<MstSupplier>
    {
        public void Configure(EntityTypeBuilder<MstSupplier> entity)
        {
            entity.ToTable("MstSupplier", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SupplierCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SupplierName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.LegalName)
                .HasMaxLength(150);

            entity.Property(x => x.SupplierType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.SupplierGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.TaxNumber)
                .HasMaxLength(50);

            entity.Property(x => x.BusinessLicenseNumber)
                .HasMaxLength(100);

            entity.Property(x => x.ContactPersonName)
                .HasMaxLength(100);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(50);

            entity.Property(x => x.WhatsAppNumber)
                .HasMaxLength(50);

            entity.Property(x => x.Email)
                .HasMaxLength(150);

            entity.Property(x => x.Website)
                .HasMaxLength(150);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.CityName)
                .HasMaxLength(100);

            entity.Property(x => x.ProvinceName)
                .HasMaxLength(100);

            entity.Property(x => x.PostalCode)
                .HasMaxLength(20);

            entity.Property(x => x.CountryName)
                .HasMaxLength(100);

            entity.Property(x => x.BankName)
                .HasMaxLength(100);

            entity.Property(x => x.BankAccountNumber)
                .HasMaxLength(100);

            entity.Property(x => x.BankAccountName)
                .HasMaxLength(150);

            entity.Property(x => x.PaymentTermDays)
                .HasDefaultValue(0);

            entity.Property(x => x.LeadTimeDays)
                .HasDefaultValue(0);

            entity.Property(x => x.MinimumPurchaseAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.CreditLimitAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.IsTaxable)
                .HasDefaultValue(false);

            entity.Property(x => x.TaxPercent)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.IsPrincipal)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDistributor)
                .HasDefaultValue(false);

            entity.Property(x => x.IsManufacturer)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPharmacySupplier)
                .HasDefaultValue(true);

            entity.Property(x => x.IsMedicalDeviceSupplier)
                .HasDefaultValue(false);

            entity.Property(x => x.IsLaboratorySupplier)
                .HasDefaultValue(false);

            entity.Property(x => x.IsConsumableSupplier)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPreferredSupplier)
                .HasDefaultValue(false);

            entity.Property(x => x.IsBlacklisted)
                .HasDefaultValue(false);

            entity.Property(x => x.BlacklistReason)
                .HasMaxLength(250);

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

            entity.HasIndex(x => x.SupplierCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.SupplierName);

            entity.HasIndex(x => x.SupplierType);

            entity.HasIndex(x => x.SupplierGroupName);

            entity.HasIndex(x => x.TaxNumber);

            entity.HasIndex(x => x.Email);

            entity.HasIndex(x => x.PhoneNumber);

            entity.HasIndex(x => new
            {
                x.SupplierType,
                x.SupplierGroupName,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsPharmacySupplier,
                x.IsMedicalDeviceSupplier,
                x.IsLaboratorySupplier,
                x.IsConsumableSupplier,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsPreferredSupplier,
                x.IsBlacklisted,
                x.IsActive,
                x.IsDelete
            });
        }
    }

}
