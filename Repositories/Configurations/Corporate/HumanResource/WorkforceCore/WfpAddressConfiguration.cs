using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpAddressConfiguration : IEntityTypeConfiguration<WfpAddress>
    {
        public void Configure(EntityTypeBuilder<WfpAddress> builder)
        {
            builder.ToTable("WfpAddress", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.AddressType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.AddressLine).HasMaxLength(500).IsRequired();
            builder.Property(x => x.VillageName).HasMaxLength(150);
            builder.Property(x => x.Latitude).HasMaxLength(30);
            builder.Property(x => x.Longitude).HasMaxLength(30);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Province).WithMany().HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PostalCode).WithMany().HasForeignKey(x => x.PostalCodeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.AddressType, x.IsActive });
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
