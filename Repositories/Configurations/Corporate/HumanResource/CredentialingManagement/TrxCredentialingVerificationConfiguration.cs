using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxCredentialingVerificationConfiguration : IEntityTypeConfiguration<TrxCredentialingVerification>
    {
        public void Configure(EntityTypeBuilder<TrxCredentialingVerification> entity)
        {
            entity.ToTable("TrxCredentialingVerification", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.VerificationStartedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerificationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.FollowUpDueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.CredentialingApplication)
                .WithMany(x => x.Verifications)
                .HasForeignKey(x => x.CredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingDocument)
                .WithMany()
                .HasForeignKey(x => x.CredentialingDocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CredentialingRequirement)
                .WithMany()
                .HasForeignKey(x => x.CredentialingRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifierUser)
                .WithMany()
                .HasForeignKey(x => x.VerifierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.CredentialingApplicationId, x.VerificationType, x.CredentialingDocumentId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.VerificationStatus, x.FollowUpDueDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxCredentialingVerification> entity)
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
