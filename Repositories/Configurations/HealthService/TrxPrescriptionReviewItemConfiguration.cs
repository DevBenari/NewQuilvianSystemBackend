using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionReviewItemConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionReviewItem>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionReviewItem> entity)
        {
            entity.ToTable("TrxPrescriptionReviewItem", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionReviewId).IsRequired();
            entity.Property(x => x.CriterionId).IsRequired(false);

            entity.Property(x => x.Category)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.CriterionCodeSnapshot)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.CriterionNameSnapshot)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Result)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionReviewResult.NotReviewed)
                .IsRequired();

            entity.Property(x => x.Severity)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionIssueSeverity.Warning)
                .IsRequired();

            entity.Property(x => x.Finding)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.Recommendation)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.IsSystemDetected).HasDefaultValue(false);
            entity.Property(x => x.SystemRuleCode)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(x => x.PrescriptionItemId).IsRequired(false);
            entity.Property(x => x.PrescriptionCompoundId).IsRequired(false);
            entity.Property(x => x.PrescriptionCompoundItemId).IsRequired(false);
            entity.Property(x => x.ReviewedByUserId).IsRequired(false);
            ConfigureNullableTimestamp(entity, x => x.ReviewedAt);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.PrescriptionReview)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PrescriptionReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Criterion)
                .WithMany(x => x.ReviewItems)
                .HasForeignKey(x => x.CriterionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionCompound>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionCompoundId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionCompoundItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionCompoundItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.PrescriptionReviewId,
                x.CriterionCodeSnapshot,
                x.PrescriptionItemId,
                x.PrescriptionCompoundId,
                x.PrescriptionCompoundItemId
            });

            entity.HasIndex(x => new
            {
                x.PrescriptionReviewId,
                x.Category,
                x.Result,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.CriterionId);
            entity.HasIndex(x => x.PrescriptionItemId);
            entity.HasIndex(x => x.PrescriptionCompoundId);
            entity.HasIndex(x => x.PrescriptionCompoundItemId);
        }

        private static void ConfigureNullableTimestamp(
            EntityTypeBuilder<TrxPrescriptionReviewItem> entity,
            System.Linq.Expressions.Expression<Func<TrxPrescriptionReviewItem, DateTime?>> property)
        {
            entity.Property(property)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionReviewItem> entity)
        {
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
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
