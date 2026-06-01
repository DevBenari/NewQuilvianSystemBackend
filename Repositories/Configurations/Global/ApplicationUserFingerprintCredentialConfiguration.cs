using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class ApplicationUserFingerprintCredentialConfiguration : IEntityTypeConfiguration<ApplicationUserFingerprintCredential>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserFingerprintCredential> entity)
        {
            entity.ToTable("AspNetUserFingerprint", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FingerPosition)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TemplateFormat)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.TemplateVersion)
                .HasMaxLength(50);

            entity.Property(x => x.TemplateDataEncrypted)
                .HasColumnType("bytea")
                .IsRequired();

            entity.Property(x => x.TemplateHash)
                .HasMaxLength(128);

            entity.Property(x => x.DeviceId)
                .HasMaxLength(100);

            entity.Property(x => x.DeviceModel)
                .HasMaxLength(100);

            entity.Property(x => x.SampleFormat)
                .HasMaxLength(50);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(true);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.RegisteredAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.RegisteredIpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.RegisteredUserAgent)
                .HasMaxLength(500);

            entity.Property(x => x.RevokedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RevokedReason)
                .HasMaxLength(250);

            entity.HasOne(x => x.User)
                .WithMany(x => x.FingerprintCredentials)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.EmployeeId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.FingerPosition
            })
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.UserId,
                x.IsPrimary
            })
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => x.TemplateHash);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
