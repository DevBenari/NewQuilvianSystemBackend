using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionFinalCheckItemConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionFinalCheckItem>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionFinalCheckItem> entity)
        {
            entity.ToTable("TrxPrescriptionFinalCheckItem", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionFinalCheckId).IsRequired();
            entity.Property(x => x.CriterionCode)
                .HasMaxLength(80)
                .IsRequired();
            entity.Property(x => x.CriterionName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Result)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionReviewResult.NotReviewed)
                .IsRequired();

            entity.Property(x => x.Finding)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.PrescriptionFinalCheck)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PrescriptionFinalCheckId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.PrescriptionFinalCheckId,
                x.CriterionCode,
                x.IsDelete
            })
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.PrescriptionFinalCheckId,
                x.Result,
                x.SortOrder,
                x.IsActive
            });
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionFinalCheckItem> entity)
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
