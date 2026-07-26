using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxPeerFeedbackConfiguration : IEntityTypeConfiguration<TrxPeerFeedback>
    {
        public void Configure(EntityTypeBuilder<TrxPeerFeedback> entity)
        {
            entity.ToTable("TrxPeerFeedback", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.OverallRating).HasPrecision(18, 4);
            entity.Property(x => x.FeedbackJson).HasColumnType("jsonb");
            entity.Property(x => x.IsAnonymous).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.PerformanceCycle)
                .WithMany()
                .HasForeignKey(x => x.PerformanceCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubjectWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.SubjectWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReviewerWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.ReviewerWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PerformanceCycleId, x.SubjectWorkforceProfileId, x.ReviewerWorkforceProfileId })
                .IsUnique()
                .HasFilter("\"ReviewerWorkforceProfileId\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => new { x.SubjectWorkforceProfileId, x.FeedbackStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxPeerFeedback> entity)
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
