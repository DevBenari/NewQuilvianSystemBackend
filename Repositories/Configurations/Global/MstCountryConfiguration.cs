using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstCountryConfiguration : IEntityTypeConfiguration<MstCountry>
    {
        public void Configure(EntityTypeBuilder<MstCountry> entity)
        {
            entity.ToTable("MstCountry", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CountryCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CountryName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.PhoneCode)
                .HasMaxLength(10);

            entity.Property(x => x.IsDefault)
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

            entity.HasIndex(x => x.CountryCode)
                .IsUnique();

            entity.HasIndex(x => x.CountryName);

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}