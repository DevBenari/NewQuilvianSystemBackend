using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class SysActionAccessConfiguration : IEntityTypeConfiguration<SysActionAccess>
    {
        public void Configure(EntityTypeBuilder<SysActionAccess> entity)
        {
            entity.ToTable("SysActionAccess", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.HttpMethod)
                .HasMaxLength(20);

            entity.Property(x => x.RoutePath)
                .HasMaxLength(250);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.AccessType)
                .HasMaxLength(50)
                .HasDefaultValue("Read");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.VisibleInRoleAccess)
                .HasDefaultValue(true);

            entity.Property(x => x.IsSystemOnly)
                .HasDefaultValue(false);

            entity.HasOne(x => x.ControllerAccess)
                .WithMany(x => x.Actions)
                .HasForeignKey(x => x.ControllerAccessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ControllerAccessId, x.ActionName })
                .IsUnique();

            entity.HasIndex(x => x.ActionName);
            entity.HasIndex(x => x.AccessType);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.VisibleInRoleAccess);
            entity.HasIndex(x => x.IsSystemOnly);
        }
    }
}
