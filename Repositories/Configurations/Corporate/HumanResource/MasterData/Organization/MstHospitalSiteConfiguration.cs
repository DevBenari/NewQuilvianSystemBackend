using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstHospitalSiteConfiguration : IEntityTypeConfiguration<MstHospitalSite>
    {
        public void Configure(EntityTypeBuilder<MstHospitalSite> entity)
        {
            entity.ToTable("MstHospitalSite", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.SiteCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SiteName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.SiteType)
                .HasMaxLength(50)
                .HasDefaultValue("Hospital")
                .IsRequired();

            entity.Property(x => x.AccreditationNumber)
                .HasMaxLength(100);

            entity.Property(x => x.TimeZoneId)
                .HasMaxLength(100)
                .HasDefaultValue("Asia/Jakarta");

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.IsMainSite)
                .HasDefaultValue(false);

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

            entity.HasOne(x => x.LegalEntity)
                .WithMany(x => x.HospitalSites)
                .HasForeignKey(x => x.LegalEntityId)
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

            entity.HasIndex(x => new { x.LegalEntityId, x.SiteCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.LegalEntityId, x.SiteName });

            entity.HasIndex(x => x.LegalEntityId)
                .IsUnique()
                .HasFilter("\"IsMainSite\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.CountryId,
                x.ProvinceId,
                x.CityId,
                x.DistrictId,
                x.PostalCodeId
            });

            entity.HasIndex(x => new { x.SiteType, x.IsActive, x.IsDelete });
        }
    }
}
