using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxCredentialingDocumentConfiguration : IEntityTypeConfiguration<TrxCredentialingDocument>
    {
        public void Configure(EntityTypeBuilder<TrxCredentialingDocument> entity)
        {
            entity.ToTable("TrxCredentialingDocument", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IssueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiryDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.CredentialingApplication)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.CredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingRequirement)
                .WithMany()
                .HasForeignKey(x => x.CredentialingRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Certification)
                .WithMany()
                .HasForeignKey(x => x.CertificationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialLicense)
                .WithMany()
                .HasForeignKey(x => x.CredentialLicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClinicalPrivilege)
                .WithMany()
                .HasForeignKey(x => x.ClinicalPrivilegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.CredentialingApplicationId, x.DocumentType });

            entity.HasIndex(x => new { x.VerificationStatus, x.ExpiryDate });

            entity.HasIndex(x => x.FileChecksum);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxCredentialingDocument> entity)
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
