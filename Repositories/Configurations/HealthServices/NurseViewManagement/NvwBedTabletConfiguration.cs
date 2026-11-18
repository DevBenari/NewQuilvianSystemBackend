using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NurseViewManagement
{
    public class NvwBedTabletConfiguration : IEntityTypeConfiguration<NvwBedTablet>
    {
        public void Configure(EntityTypeBuilder<NvwBedTablet> builder)
        {
            builder.ToTable("NvwBedTablet", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeviceCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DeviceName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LocationNote).HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.DeviceStatus)
                .HasConversion<int>()
                .HasDefaultValue(NvwBedTabletStatus.PendingLogin)
                .IsRequired();
            builder.Property(x => x.DeactivationReason).HasMaxLength(500);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasOne(x => x.Bed)
                .WithMany()
                .HasForeignKey(x => x.BedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LoginUser)
                .WithMany()
                .HasForeignKey(x => x.LoginUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.DeviceCode).IsUnique();
            builder.HasIndex(x => x.LoginUserId)
                .IsUnique()
                .HasFilter("\"LoginUserId\" IS NOT NULL");
            builder.HasIndex(x => x.BedId, "IX_NvwBedTablet_BedId_InUse")
                .IsUnique()
                .HasFilter("\"DeviceStatus\" <> 3 AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.BedId, x.DeviceStatus });
        }
    }
}
