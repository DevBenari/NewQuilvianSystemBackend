using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class SysAccessPolicyConfiguration : IEntityTypeConfiguration<SysAccessPolicy>
    {
        public void Configure(EntityTypeBuilder<SysAccessPolicy> entity)
        {
            entity.ToTable("SysAccessPolicy", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.IsAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ControllerAccess)
                .WithMany()
                .HasForeignKey(x => x.ControllerAccessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ActionAccess)
                .WithMany()
                .HasForeignKey(x => x.ActionAccessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.DepartmentId,
                x.PositionId,
                x.ControllerAccessId,
                x.ActionAccessId
            }).IsUnique();

            entity.HasIndex(x => new
            {
                x.DepartmentId,
                x.PositionId,
                x.IsAllowed,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ControllerAccessId,
                x.ActionAccessId,
                x.IsAllowed,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
