using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class SysAppVersionConfiguration : IEntityTypeConfiguration<SysAppVersion>
    {
        public void Configure(EntityTypeBuilder<SysAppVersion> entity)
        {
            entity.ToTable("SysAppVersion", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AppName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.BackendVersion)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ApiVersion)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.FrontendMinimumVersion)
                .HasMaxLength(50);

            entity.Property(x => x.FrontendRecommendedVersion)
                .HasMaxLength(50);

            entity.Property(x => x.ReleaseName)
                .HasMaxLength(200);

            entity.Property(x => x.IsLatest)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.ReleaseDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.IsLatest);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.BackendVersion);
            entity.HasIndex(x => x.ApiVersion);
        }
    }
}
