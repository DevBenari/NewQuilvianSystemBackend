using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionPreparationConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionPreparation>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionPreparation> entity)
        {
            entity.ToTable("TrxPrescriptionPreparation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionId).IsRequired();
            entity.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionPreparationStatus.Pending)
                .IsRequired();

            entity.Property(x => x.PreparedByUserId).IsRequired(false);
            ConfigureNullableTimestamp(entity, x => x.PreparationStartedAt);
            ConfigureNullableTimestamp(entity, x => x.PreparationCompletedAt);

            entity.Property(x => x.PreparationNote)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.Prescription)
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.PreparedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.PrescriptionPreparation)
                .HasForeignKey(x => x.PrescriptionPreparationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.PrescriptionId,
                x.Status,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PreparedByUserId,
                x.Status,
                x.IsDelete
            });
        }

        private static void ConfigureNullableTimestamp(
            EntityTypeBuilder<TrxPrescriptionPreparation> entity,
            System.Linq.Expressions.Expression<Func<TrxPrescriptionPreparation, DateTime?>> property)
        {
            entity.Property(property)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionPreparation> entity)
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
