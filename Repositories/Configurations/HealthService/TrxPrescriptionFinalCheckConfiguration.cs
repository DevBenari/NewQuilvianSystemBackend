using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionFinalCheckConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionFinalCheck>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionFinalCheck> entity)
        {
            entity.ToTable("TrxPrescriptionFinalCheck", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionId).IsRequired();
            entity.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionFinalCheckStatus.Pending)
                .IsRequired();

            entity.Property(x => x.CheckedByUserId).IsRequired(false);
            ConfigureNullableTimestamp(entity, x => x.StartedAt);
            ConfigureNullableTimestamp(entity, x => x.CompletedAt);

            entity.Property(x => x.CheckNote)
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
                .HasForeignKey(x => x.CheckedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.PrescriptionFinalCheck)
                .HasForeignKey(x => x.PrescriptionFinalCheckId)
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
                x.CheckedByUserId,
                x.Status,
                x.IsDelete
            });
        }

        private static void ConfigureNullableTimestamp(
            EntityTypeBuilder<TrxPrescriptionFinalCheck> entity,
            System.Linq.Expressions.Expression<Func<TrxPrescriptionFinalCheck, DateTime?>> property)
        {
            entity.Property(property)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionFinalCheck> entity)
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
