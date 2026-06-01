using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstPositionConfiguration : IEntityTypeConfiguration<MstPosition>
    {
        public void Configure(EntityTypeBuilder<MstPosition> entity)
        {
            entity.ToTable("MstPosition", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DepartmentId)
                .IsRequired();

            entity.Property(x => x.PositionCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PositionName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Positions)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.DepartmentId, x.PositionCode })
                .IsUnique();

            entity.HasIndex(x => new { x.DepartmentId, x.PositionName });
        }
    }
}
