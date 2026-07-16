using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionReviewConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionReview>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionReview> entity)
        {
            entity.ToTable("TrxPrescriptionReview", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionId).IsRequired();
            entity.Property(x => x.ReviewVersion)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionReviewStatus.Pending)
                .IsRequired();

            entity.Property(x => x.ReviewedByPharmacistId).IsRequired(false);
            ConfigureNullableTimestamp(entity, x => x.StartedAt);
            ConfigureNullableTimestamp(entity, x => x.CompletedAt);

            entity.Property(x => x.HasAdministrativeProblem).HasDefaultValue(false);
            entity.Property(x => x.HasPharmaceuticalProblem).HasDefaultValue(false);
            entity.Property(x => x.HasClinicalProblem).HasDefaultValue(false);
            entity.Property(x => x.HasCompoundFormulaProblem).HasDefaultValue(false);
            entity.Property(x => x.RequiresDoctorClarification).HasDefaultValue(false);

            entity.Property(x => x.GeneralNote)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.PrescriptionSignatureSnapshot)
                .HasMaxLength(500)
                .HasDefaultValue(string.Empty)
                .IsRequired();

            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.Prescription)
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ReviewedByPharmacistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.PrescriptionReview)
                .HasForeignKey(x => x.PrescriptionReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Clarifications)
                .WithOne(x => x.PrescriptionReview)
                .HasForeignKey(x => x.PrescriptionReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.PrescriptionId, x.ReviewVersion })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PrescriptionId,
                x.Status,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ReviewedByPharmacistId,
                x.Status,
                x.IsDelete
            });
        }

        private static void ConfigureNullableTimestamp(
            EntityTypeBuilder<TrxPrescriptionReview> entity,
            System.Linq.Expressions.Expression<Func<TrxPrescriptionReview, DateTime?>> property)
        {
            entity.Property(property)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionReview> entity)
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
