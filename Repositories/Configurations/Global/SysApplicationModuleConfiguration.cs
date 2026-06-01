using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class SysApplicationModuleConfiguration : IEntityTypeConfiguration<SysApplicationModule>
    {
        public void Configure(EntityTypeBuilder<SysApplicationModule> entity)
        {
            entity.ToTable("SysApplicationModule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ModuleCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ModuleName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.AreaName)
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.ModuleCode)
                .IsUnique();

            entity.HasIndex(x => x.IsActive);
        }
    }
}
