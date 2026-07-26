using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstTravelDestinationZoneConfiguration : IEntityTypeConfiguration<MstTravelDestinationZone>
    {
        public void Configure(EntityTypeBuilder<MstTravelDestinationZone> entity)
        {
            entity.ToTable("MstTravelDestinationZone", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DestinationZoneCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DestinationZoneName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ZoneType).HasMaxLength(50).HasDefaultValue("Domestic").IsRequired();
            entity.Property(x => x.DistanceFromBaseKilometers).HasPrecision(12, 2);
            entity.Property(x => x.RiskLevel).HasMaxLength(50);
            entity.Property(x => x.IsDomestic).HasDefaultValue(true);
            entity.Property(x => x.RequiresSpecialApproval).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

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

            entity.HasIndex(x => x.DestinationZoneCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.DestinationZoneName);
            entity.HasIndex(x => new { x.CountryId, x.ProvinceId, x.CityId });
            entity.HasIndex(x => new { x.ZoneType, x.IsDomestic, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.RiskLevel, x.RequiresSpecialApproval, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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
        }
    }
}
