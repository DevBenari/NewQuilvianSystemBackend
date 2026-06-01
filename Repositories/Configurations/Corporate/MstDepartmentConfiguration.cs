using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstDepartmentConfiguration : IEntityTypeConfiguration<MstDepartment>
    {
        public void Configure(EntityTypeBuilder<MstDepartment> entity)
        {
            entity.ToTable("MstDepartment", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DepartmentCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DepartmentName)
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

            entity.HasIndex(x => x.DepartmentCode)
                .IsUnique();

            entity.HasIndex(x => x.DepartmentName);
        }
    }
}
