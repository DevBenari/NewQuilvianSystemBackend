using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstTariffCategoryConfiguration : IEntityTypeConfiguration<MstTariffCategory>
    {
        public void Configure(EntityTypeBuilder<MstTariffCategory> entity)
        {
            entity.ToTable("MstTariffCategory", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TariffCategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TariffCategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.TariffGroupName).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(250);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasIndex(x => x.TariffCategoryCode).IsUnique();
            entity.HasIndex(x => x.TariffCategoryName);
            entity.HasIndex(x => x.TariffGroupName);
            entity.HasIndex(x => new { x.IsProcedure, x.IsLaboratory, x.IsRadiology, x.IsPharmacy, x.IsActive, x.IsDelete });
        }
    }
}
