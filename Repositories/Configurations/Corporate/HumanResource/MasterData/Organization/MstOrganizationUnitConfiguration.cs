using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstOrganizationUnitConfiguration : IEntityTypeConfiguration<MstOrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<MstOrganizationUnit> entity)
        {
            entity.ToTable("MstOrganizationUnit", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.UnitCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.UnitName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.UnitType)
                .HasMaxLength(50)
                .HasDefaultValue("Unit")
                .IsRequired();

            entity.Property(x => x.LevelNumber)
                .HasDefaultValue(1);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsOperationalUnit)
                .HasDefaultValue(true);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.LegalEntity)
                .WithMany(x => x.OrganizationUnits)
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany(x => x.OrganizationUnits)
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ParentOrganizationUnit)
                .WithMany(x => x.ChildOrganizationUnits)
                .HasForeignKey(x => x.ParentOrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.LegalEntityId, x.UnitCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.LegalEntityId, x.UnitName });

            entity.HasIndex(x => x.ParentOrganizationUnitId);

            entity.HasIndex(x => x.HospitalSiteId);

            entity.HasIndex(x => x.DepartmentId);

            entity.HasIndex(x => new
            {
                x.LegalEntityId,
                x.HospitalSiteId,
                x.ParentOrganizationUnitId,
                x.UnitType,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
