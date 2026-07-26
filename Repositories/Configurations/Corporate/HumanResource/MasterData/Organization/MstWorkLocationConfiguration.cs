using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstWorkLocationConfiguration : IEntityTypeConfiguration<MstWorkLocation>
    {
        public void Configure(EntityTypeBuilder<MstWorkLocation> entity)
        {
            entity.ToTable("MstWorkLocation", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.HospitalSiteId)
                .IsRequired();

            entity.Property(x => x.LocationCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.LocationName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.LocationType)
                .HasMaxLength(50)
                .HasDefaultValue("WorkArea")
                .IsRequired();

            entity.Property(x => x.BuildingName)
                .HasMaxLength(150);

            entity.Property(x => x.FloorName)
                .HasMaxLength(50);

            entity.Property(x => x.RoomName)
                .HasMaxLength(100);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

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
                .WithMany(x => x.WorkLocations)
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany(x => x.WorkLocations)
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany(x => x.WorkLocations)
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.HospitalSiteId, x.LocationCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.HospitalSiteId, x.LocationName });

            entity.HasIndex(x => new
            {
                x.LegalEntityId,
                x.HospitalSiteId,
                x.OrganizationUnitId,
                x.DepartmentId,
                x.LocationType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new { x.IsPrimary, x.IsActive, x.IsDelete });
        }
    }
}
