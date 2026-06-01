using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstExternalUserConfiguration : IEntityTypeConfiguration<MstExternalUser>
    {
        public void Configure(EntityTypeBuilder<MstExternalUser> entity)
        {
            entity.ToTable("MstExternalUser", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.ExternalCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ExternalUserType)
                .HasConversion<int>()
                .HasDefaultValue(ExternalUserType.Unknown)
                .IsRequired();

            entity.Property(x => x.ExternalUserStatus)
                .HasConversion<int>()
                .HasDefaultValue(ExternalUserStatus.Active)
                .IsRequired();

            entity.Property(x => x.EngagementType)
                .HasConversion<int>()
                .HasDefaultValue(ExternalEngagementType.ContractBased)
                .IsRequired();

            entity.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CompanyName)
                .HasMaxLength(200);

            entity.Property(x => x.CompanyCode)
                .HasMaxLength(100);

            entity.Property(x => x.JobTitle)
                .HasMaxLength(100);

            entity.Property(x => x.ContactPersonName)
                .HasMaxLength(200);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.WhatsAppNumber)
                .HasMaxLength(30);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.IdentityType)
                .HasMaxLength(50);

            entity.Property(x => x.IdentityNumber)
                .HasMaxLength(100);

            entity.Property(x => x.TaxNumber)
                .HasMaxLength(100);

            entity.Property(x => x.BusinessLicenseNumber)
                .HasMaxLength(100);

            entity.Property(x => x.WorkLocation)
                .HasMaxLength(50);

            entity.Property(x => x.ContractStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ContractEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.AccessStartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.AccessEndDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.AccessPurpose)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.WorkforceProfile)
                .WithOne(x => x.ExternalUser)
                .HasForeignKey<MstExternalUser>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryDepartment)
                .WithMany()
                .HasForeignKey(x => x.PrimaryDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryPosition)
                .WithMany()
                .HasForeignKey(x => x.PrimaryPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Province)
                .WithMany()
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.District)
                .WithMany()
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PostalCode)
                .WithMany()
                .HasForeignKey(x => x.PostalCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique();

            entity.HasIndex(x => x.ExternalCode)
                .IsUnique();

            entity.HasIndex(x => x.FullName);

            entity.HasIndex(x => x.CompanyName);

            entity.HasIndex(x => x.CompanyCode);

            entity.HasIndex(x => x.Email);

            entity.HasIndex(x => x.PhoneNumber);

            entity.HasIndex(x => x.WhatsAppNumber);

            entity.HasIndex(x => new
            {
                x.ExternalUserType,
                x.ExternalUserStatus,
                x.EngagementType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PrimaryDepartmentId,
                x.PrimaryPositionId
            });

            entity.HasIndex(x => new
            {
                x.CountryId,
                x.ProvinceId,
                x.CityId,
                x.DistrictId,
                x.PostalCodeId
            });

            entity.HasIndex(x => new
            {
                x.ContractStartDate,
                x.ContractEndDate,
                x.AccessStartDate,
                x.AccessEndDate
            });

            entity.HasIndex(x => x.IsActive);
        }
    }
}
