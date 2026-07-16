using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPrescriptionReviewCriterionConfiguration
        : IEntityTypeConfiguration<MstPrescriptionReviewCriterion>
    {
        public void Configure(EntityTypeBuilder<MstPrescriptionReviewCriterion> entity)
        {
            entity.ToTable("MstPrescriptionReviewCriterion", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CriterionCode)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.CriterionName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(x => x.Category)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.DefaultSeverity)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionIssueSeverity.Warning)
                .IsRequired();

            entity.Property(x => x.IsRequired).HasDefaultValue(true);
            entity.Property(x => x.IsApplicableToRegular).HasDefaultValue(true);
            entity.Property(x => x.IsApplicableToCompound).HasDefaultValue(true);
            entity.Property(x => x.IsSystemCheckSupported).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);

            ConfigureAuditColumns(entity);

            entity.HasMany(x => x.ReviewItems)
                .WithOne(x => x.Criterion)
                .HasForeignKey(x => x.CriterionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CriterionCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.Category,
                x.IsActive,
                x.IsDelete,
                x.SortOrder
            });

            entity.HasIndex(x => new
            {
                x.IsApplicableToRegular,
                x.IsApplicableToCompound,
                x.IsActive,
                x.IsDelete
            });
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<MstPrescriptionReviewCriterion> entity)
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
