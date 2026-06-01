using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> entity)
        {
            entity.Property(x => x.UserCode)
                    .HasMaxLength(50)
                    .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.Property(x => x.UserType)
                .HasConversion<int>();

            entity.Property(x => x.ProfilePhotoPath)
                .HasMaxLength(500);

            entity.Property(x => x.GeolocationBypassReason)
                .HasMaxLength(250);

            entity.Property(x => x.GeolocationBypassUntil)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EmployeeId)
                .IsRequired(false);

            entity.Property(x => x.DoctorId)
                .IsRequired(false);

            entity.Property(x => x.ExternalUserId)
                .IsRequired(false);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired(false);

            entity.HasOne(x => x.Employee)
                .WithOne()
                .HasForeignKey<ApplicationUser>(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithOne()
                .HasForeignKey<ApplicationUser>(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ExternalUser)
                .WithOne()
                .HasForeignKey<ApplicationUser>(x => x.ExternalUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithOne(x => x.UserAccount)
                .HasForeignKey<ApplicationUser>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryDepartment)
                .WithMany()
                .HasForeignKey(x => x.PrimaryDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryPosition)
                .WithMany()
                .HasForeignKey(x => x.PrimaryPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.UserCode)
                .IsUnique();

            entity.HasIndex(x => x.UserType);

            entity.HasIndex(x => x.EmployeeId)
                .IsUnique()
                .HasFilter("\"EmployeeId\" IS NOT NULL");

            entity.HasIndex(x => x.DoctorId)
                .IsUnique()
                .HasFilter("\"DoctorId\" IS NOT NULL");

            entity.HasIndex(x => x.ExternalUserId)
                .IsUnique()
                .HasFilter("\"ExternalUserId\" IS NOT NULL");

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique()
                .HasFilter("\"WorkforceProfileId\" IS NOT NULL");

            entity.HasIndex(x => new { x.PrimaryDepartmentId, x.PrimaryPositionId });

            entity.HasIndex(x => x.IsActive);

            entity.HasIndex(x => x.IsGeolocationBypassEnabled);

            entity.Property(x => x.IsFingerprintRegistrationEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.FingerprintRegistrationReason)
                .HasMaxLength(250);

            entity.Property(x => x.FingerprintRegistrationEnabledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.HasIndex(x => x.IsFingerprintRegistrationEnabled);
        }
    }
}
