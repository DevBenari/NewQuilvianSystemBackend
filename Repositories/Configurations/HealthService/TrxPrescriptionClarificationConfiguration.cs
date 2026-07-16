using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionClarificationConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionClarification>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionClarification> entity)
        {
            entity.ToTable("TrxPrescriptionClarification", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionId).IsRequired();
            entity.Property(x => x.PrescriptionReviewId).IsRequired();
            entity.Property(x => x.PrescriptionReviewItemId).IsRequired(false);
            entity.Property(x => x.PrescriptionItemId).IsRequired(false);
            entity.Property(x => x.PrescriptionCompoundId).IsRequired(false);
            entity.Property(x => x.PrescriptionCompoundItemId).IsRequired(false);

            entity.Property(x => x.ProblemCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ProblemDescription)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.PharmacistRecommendation)
                .HasMaxLength(2000)
                .IsRequired(false);

            entity.Property(x => x.Severity)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionIssueSeverity.Warning)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionClarificationStatus.Open)
                .IsRequired();

            entity.Property(x => x.RequestedByPharmacistId).IsRequired();
            entity.Property(x => x.RequestedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.RespondedByDoctorId).IsRequired(false);
            ConfigureNullableTimestamp(entity, x => x.RespondedAt);

            entity.Property(x => x.DoctorResponse)
                .HasMaxLength(2000)
                .IsRequired(false);

            entity.Property(x => x.ClosedByUserId).IsRequired(false);
            ConfigureNullableTimestamp(entity, x => x.ClosedAt);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.Prescription)
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrescriptionReview)
                .WithMany(x => x.Clarifications)
                .HasForeignKey(x => x.PrescriptionReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrescriptionReviewItem)
                .WithMany()
                .HasForeignKey(x => x.PrescriptionReviewItemId)
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
                .HasForeignKey(x => x.RequestedByPharmacistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.RespondedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.PrescriptionId,
                x.Status,
                x.Severity,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PrescriptionReviewId,
                x.Status,
                x.IsDelete
            });

            entity.HasIndex(x => x.PrescriptionReviewItemId);
            entity.HasIndex(x => x.PrescriptionItemId);
            entity.HasIndex(x => x.PrescriptionCompoundId);
            entity.HasIndex(x => x.PrescriptionCompoundItemId);
            entity.HasIndex(x => x.RequestedAt);
        }

        private static void ConfigureNullableTimestamp(
            EntityTypeBuilder<TrxPrescriptionClarification> entity,
            System.Linq.Expressions.Expression<Func<TrxPrescriptionClarification, DateTime?>> property)
        {
            entity.Property(property)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionClarification> entity)
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
