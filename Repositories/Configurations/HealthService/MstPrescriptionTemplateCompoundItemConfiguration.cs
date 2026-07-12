using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPrescriptionTemplateCompoundItemConfiguration
        : IEntityTypeConfiguration<MstPrescriptionTemplateCompoundItem>
    {
        public void Configure(EntityTypeBuilder<MstPrescriptionTemplateCompoundItem> entity)
        {
            entity.ToTable("MstPrescriptionTemplateCompoundItem", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionTemplateCompoundId)
                .IsRequired();

            entity.Property(x => x.DrugId)
                .IsRequired();

            entity.Property(x => x.AmountPerPackage)
                .HasColumnType("numeric(18,4)");

            entity.Property(x => x.TotalQuantity)
                .HasColumnType("numeric(18,4)");

            entity.Property(x => x.IngredientInstruction)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.PrescriptionTemplateCompound)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PrescriptionTemplateCompoundId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Drug)
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.QuantityUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.QuantityUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PrescriptionTemplateCompoundId);

            entity.HasIndex(x => x.DrugId);

            entity.HasIndex(x => x.QuantityUnitMeasurementId);

            entity.HasIndex(x => new
            {
                x.PrescriptionTemplateCompoundId,
                x.SortOrder,
                x.IsDelete
            });
        }
    }
}
