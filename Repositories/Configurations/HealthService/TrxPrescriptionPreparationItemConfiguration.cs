using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionPreparationItemConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionPreparationItem>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionPreparationItem> entity)
        {
            entity.ToTable("TrxPrescriptionPreparationItem", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionPreparationId).IsRequired();
            entity.Property(x => x.PrescriptionItemId).IsRequired(false);
            entity.Property(x => x.PrescriptionCompoundItemId).IsRequired(false);
            entity.Property(x => x.DrugId).IsRequired();

            entity.Property(x => x.TheoreticalQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(x => x.ActualQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(x => x.WasteQuantity)
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(x => x.MeasurementId).IsRequired(false);
            entity.Property(x => x.MeasurementNameSnapshot)
                .HasMaxLength(100)
                .IsRequired(false);
            entity.Property(x => x.BatchNumber)
                .HasMaxLength(100)
                .IsRequired(false);
            entity.Property(x => x.ExpiryDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
            entity.Property(x => x.Note)
                .HasMaxLength(500)
                .IsRequired(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.PrescriptionPreparation)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PrescriptionPreparationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionCompoundItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionCompoundItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MstDrug>()
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MstMeasurement>()
                .WithMany()
                .HasForeignKey(x => x.MeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.PrescriptionPreparationId,
                x.SortOrder,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.PrescriptionItemId);
            entity.HasIndex(x => x.PrescriptionCompoundItemId);
            entity.HasIndex(x => x.DrugId);
            entity.HasIndex(x => x.MeasurementId);
            entity.HasIndex(x => x.BatchNumber);
            entity.HasIndex(x => x.ExpiryDate);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionPreparationItem> entity)
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
