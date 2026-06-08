using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstIdentityScannerProfileConfiguration : IEntityTypeConfiguration<MstIdentityScannerProfile>
    {
        public void Configure(EntityTypeBuilder<MstIdentityScannerProfile> entity)
        {
            entity.ToTable("MstIdentityScannerProfile", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProfileCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ProfileName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ProfileType)
                .HasConversion<int>()
                .HasDefaultValue(IdentityScannerProfileType.Unknown)
                .IsRequired();

            entity.Property(x => x.ScannerVendorName)
                .HasMaxLength(100);

            entity.Property(x => x.ScannerModel)
                .HasMaxLength(100);

            entity.Property(x => x.InputFormat)
                .HasMaxLength(100);

            entity.Property(x => x.OutputFormat)
                .HasMaxLength(100);

            entity.Property(x => x.IdentityNumberRegex)
                .HasMaxLength(250);

            entity.Property(x => x.MemberNumberRegex)
                .HasMaxLength(250);

            entity.Property(x => x.CardNumberRegex)
                .HasMaxLength(250);

            entity.Property(x => x.IdentityNumberFieldName)
                .HasMaxLength(100);

            entity.Property(x => x.FullNameFieldName)
                .HasMaxLength(100);

            entity.Property(x => x.BirthDateFieldName)
                .HasMaxLength(100);

            entity.Property(x => x.GenderFieldName)
                .HasMaxLength(100);

            entity.Property(x => x.AddressFieldName)
                .HasMaxLength(100);

            entity.Property(x => x.IsForIdentityCard)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForPatientCard)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForMembershipCard)
                .HasDefaultValue(true);

            entity.Property(x => x.IsForInsuranceCard)
                .HasDefaultValue(false);

            entity.Property(x => x.IsOcrEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.IsBarcodeEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.IsQrEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.IsManualInputAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAutoCreatePatientAllowed)
                .HasDefaultValue(false);

            entity.Property(x => x.IsVerificationRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.ConfigurationJson)
                .HasMaxLength(500);

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

            entity.HasIndex(x => x.ProfileCode)
                .IsUnique();

            entity.HasIndex(x => x.ProfileName);

            entity.HasIndex(x => x.ProfileType);

            entity.HasIndex(x => new
            {
                x.ProfileType,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
