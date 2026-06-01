using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstProvinceConfiguration : IEntityTypeConfiguration<MstProvince>
    {
        public void Configure(EntityTypeBuilder<MstProvince> entity)
        {
            entity.ToTable("MstProvince", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CountryId)
                .IsRequired();

            entity.Property(x => x.ProvinceCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.ProvinceName)
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

            entity.HasOne(x => x.Country)
                .WithMany(x => x.Provinces)
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.CountryId,
                x.ProvinceCode
            }).IsUnique();

            entity.HasIndex(x => new
            {
                x.CountryId,
                x.ProvinceName
            });

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
