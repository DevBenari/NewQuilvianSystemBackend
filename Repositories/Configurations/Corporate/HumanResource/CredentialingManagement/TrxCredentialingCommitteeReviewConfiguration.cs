using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxCredentialingCommitteeReviewConfiguration : IEntityTypeConfiguration<TrxCredentialingCommitteeReview>
    {
        public void Configure(EntityTypeBuilder<TrxCredentialingCommitteeReview> entity)
        {
            entity.ToTable("TrxCredentialingCommitteeReview", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MeetingDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AssessmentScore).HasPrecision(18, 2);
            entity.Property(x => x.ReviewEvidenceJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.CredentialingApplication)
                .WithMany(x => x.CommitteeReviews)
                .HasForeignKey(x => x.CredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReviewNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.CredentialingApplicationId, x.MeetingDate });

            entity.HasIndex(x => new { x.ReviewerUserId, x.Recommendation });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxCredentialingCommitteeReview> entity)
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
