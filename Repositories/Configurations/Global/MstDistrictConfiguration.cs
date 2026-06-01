using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstDistrictConfiguration : IEntityTypeConfiguration<MstDistrict>
    {
        public void Configure(EntityTypeBuilder<MstDistrict> entity)
        {
            entity.ToTable("MstDistrict", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CityId)
                .IsRequired();

            entity.Property(x => x.DistrictCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.DistrictName)
                .HasMaxLength(150)
                .IsRequired();

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

            entity.HasOne(x => x.City)
                .WithMany(x => x.Districts)
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.CityId,
                x.DistrictCode
            }).IsUnique();

            entity.HasIndex(x => new
            {
                x.CityId,
                x.DistrictName
            });

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
