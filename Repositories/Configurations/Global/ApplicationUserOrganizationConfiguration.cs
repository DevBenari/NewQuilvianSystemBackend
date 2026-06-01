using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class ApplicationUserOrganizationConfiguration : IEntityTypeConfiguration<ApplicationUserOrganization>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserOrganization> entity)
        {
            entity.ToTable("AspNetUserOrganization", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.HasOne(x => x.User)
                .WithMany(x => x.DepartmentPositions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.DepartmentId,
                x.PositionId,
                x.EffectiveStartDate
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DepartmentId,
                x.PositionId
            });

            entity.HasIndex(x => new
            {
                x.UserId,
                x.DepartmentId,
                x.PositionId
            }).IsUnique();
        }
    }
}
