using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxBenefitClaimDocumentConfiguration : IEntityTypeConfiguration<TrxBenefitClaimDocument>
    {
        public void Configure(EntityTypeBuilder<TrxBenefitClaimDocument> entity)
        {
            entity.ToTable("TrxBenefitClaimDocument", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.BenefitClaim)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.BenefitClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BenefitClaimItem)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.BenefitClaimItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BenefitClaimId, x.DocumentType });

            entity.HasIndex(x => new { x.VerificationStatus, x.IsActive });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxBenefitClaimDocument> entity)
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
