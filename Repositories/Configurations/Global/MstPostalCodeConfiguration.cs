using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstPostalCodeConfiguration : IEntityTypeConfiguration<MstPostalCode>
    {
        public void Configure(EntityTypeBuilder<MstPostalCode> entity)
        {
            entity.ToTable("MstPostalCode", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DistrictId)
                .IsRequired();

            entity.Property(x => x.PostalCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.VillageName)
                .HasMaxLength(150);

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

            entity.HasOne(x => x.District)
                .WithMany(x => x.PostalCodes)
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PostalCode);

            entity.HasIndex(x => new
            {
                x.DistrictId,
                x.PostalCode
            });

            entity.HasIndex(x => new
            {
                x.DistrictId,
                x.VillageName
            });

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
