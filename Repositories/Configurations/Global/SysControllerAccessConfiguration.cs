using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class SysControllerAccessConfiguration : IEntityTypeConfiguration<SysControllerAccess>
    {
        public void Configure(EntityTypeBuilder<SysControllerAccess> entity)
        {
            entity.ToTable("SysControllerAccess", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ControllerName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.RoutePath)
                .HasMaxLength(250);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.VisibleInRoleAccess)
                .HasDefaultValue(true);

            entity.Property(x => x.IsSystemOnly)
                .HasDefaultValue(false);

            entity.HasOne(x => x.Module)
                .WithMany(x => x.Controllers)
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ModuleId, x.ControllerName })
                .IsUnique();

            entity.HasIndex(x => x.ControllerName);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.VisibleInRoleAccess);
            entity.HasIndex(x => x.IsSystemOnly);
        }
    }
}
