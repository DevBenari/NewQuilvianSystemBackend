using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxLicenseRenewalRequestConfiguration : IEntityTypeConfiguration<TrxLicenseRenewalRequest>
    {
        public void Configure(EntityTypeBuilder<TrxLicenseRenewalRequest> entity)
        {
            entity.ToTable("TrxLicenseRenewalRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OldExpiryDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NewIssueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NewExpiryDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SupportingDocumentJson).HasColumnType("jsonb");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialLicense)
                .WithMany(x => x.RenewalRequests)
                .HasForeignKey(x => x.CredentialLicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingApplication)
                .WithMany()
                .HasForeignKey(x => x.CredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LicenseType)
                .WithMany()
                .HasForeignKey(x => x.LicenseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RenewalRequestNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.CredentialLicenseId, x.RenewalStatus });

            entity.HasIndex(x => new { x.NewExpiryDate, x.RenewalStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxLicenseRenewalRequest> entity)
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
