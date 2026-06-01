using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstCityConfiguration : IEntityTypeConfiguration<MstCity>
    {
        public void Configure(EntityTypeBuilder<MstCity> entity)
        {
            entity.ToTable("MstCity", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProvinceId)
                .IsRequired();

            entity.Property(x => x.CityCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CityName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.CityType)
                .HasMaxLength(30);

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

            entity.HasOne(x => x.Province)
                .WithMany(x => x.Cities)
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.ProvinceId,
                x.CityCode
            }).IsUnique();

            entity.HasIndex(x => new
            {
                x.ProvinceId,
                x.CityName
            });

            entity.HasIndex(x => x.CityType);

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
