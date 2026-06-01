using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpCredentialLicenseConfiguration : IEntityTypeConfiguration<WfpCredentialLicense>
    {
        public void Configure(EntityTypeBuilder<WfpCredentialLicense> entity)
        {
            entity.ToTable("WfpCredentialLicense", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.RequirementCode)
                .HasMaxLength(100);

            entity.Property(x => x.LicenseType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LicenseNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Issuer)
                .HasMaxLength(200);

            entity.Property(x => x.IssueDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.ExpiredDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.PracticeLocation)
                .HasMaxLength(200);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.FileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.VerificationStatus)
                .HasConversion<int>()
                .HasDefaultValue(CredentialVerificationStatus.Unverified)
                .IsRequired();

            entity.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.VerifiedByUserId)
                .IsRequired(false);

            entity.Property(x => x.VerifiedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.VerificationNote)
                .HasMaxLength(250);

            entity.Property(x => x.RejectedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RejectedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectedReason)
                .HasMaxLength(250);

            entity.Property(x => x.RevokedByUserId)
                .IsRequired(false);

            entity.Property(x => x.RevokedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RevokedReason)
                .HasMaxLength(250);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.CredentialLicenses)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RevokedByUser)
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.VerifiedByUserId);

            entity.HasIndex(x => x.RejectedByUserId);

            entity.HasIndex(x => x.RevokedByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.LicenseType,
                x.LicenseNumber
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.LicenseType,
                x.IsPrimary
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false AND \"IsPrimary\" = true");

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.VerificationStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.IsVerified,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.LicenseType,
                x.ExpiredDate,
                x.VerificationStatus
            });

            entity.HasIndex(x => new
            {
                x.ExpiredDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
