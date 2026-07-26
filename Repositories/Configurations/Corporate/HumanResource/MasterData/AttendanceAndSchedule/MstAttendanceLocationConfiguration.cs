using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstAttendanceLocationConfiguration : IEntityTypeConfiguration<MstAttendanceLocation>
    {
        public void Configure(EntityTypeBuilder<MstAttendanceLocation> entity)
        {
            entity.ToTable("MstAttendanceLocation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttendanceLocationCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AttendanceLocationName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.LocationType).HasMaxLength(50).HasDefaultValue("OnSite").IsRequired();
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Latitude).HasPrecision(10, 7);
            entity.Property(x => x.Longitude).HasPrecision(10, 7);
            entity.Property(x => x.RadiusMeters).HasDefaultValue(100);
            entity.Property(x => x.AllowMobileAttendance).HasDefaultValue(false);
            entity.Property(x => x.AllowDeviceAttendance).HasDefaultValue(true);
            entity.Property(x => x.RequiresGeolocation).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkLocation)
                .WithMany()
                .HasForeignKey(x => x.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AttendanceLocationCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AttendanceLocationName);
            entity.HasIndex(x => new { x.HospitalSiteId, x.OrganizationUnitId, x.WorkLocationId });
            entity.HasIndex(x => new { x.LocationType, x.AllowMobileAttendance, x.AllowDeviceAttendance, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstAttendanceLocation> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
