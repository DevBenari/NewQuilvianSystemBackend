using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class WfpCredentialLicenseConfiguration : IEntityTypeConfiguration<WfpCredentialLicense>
    {
        public void Configure(EntityTypeBuilder<WfpCredentialLicense> entity)
        {
            entity.ToTable("WfpCredentialLicense", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IssueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiredDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LicenseTypeMaster)
                .WithMany()
                .HasForeignKey(x => x.LicenseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingRequirement)
                .WithMany()
                .HasForeignKey(x => x.CredentialingRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RevokedByUser)
                .WithMany()
                .HasForeignKey(x => x.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.LicenseType, x.LicenseNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.ExpiredDate, x.VerificationStatus, x.IsActive });

            entity.HasIndex(x => new { x.WorkforceProfileId, x.IsPrimary });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpCredentialLicense> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
