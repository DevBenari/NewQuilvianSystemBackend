using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPrescriptionTemplateCompoundConfiguration
        : IEntityTypeConfiguration<MstPrescriptionTemplateCompound>
    {
        public void Configure(EntityTypeBuilder<MstPrescriptionTemplateCompound> entity)
        {
            entity.ToTable("MstPrescriptionTemplateCompound", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionTemplateId)
                .IsRequired();

            entity.Property(x => x.CompoundName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CompoundForm)
                .HasMaxLength(100);

            entity.Property(x => x.TotalPackage)
                .HasColumnType("numeric(18,4)");

            entity.Property(x => x.DosePerUse)
                .HasColumnType("numeric(18,4)");

            entity.Property(x => x.FrequencyCode)
                .HasMaxLength(50);

            entity.Property(x => x.FrequencyText)
                .HasMaxLength(150);

            entity.Property(x => x.FrequencyPerDay)
                .HasColumnType("numeric(18,4)");

            entity.Property(x => x.DurationValue)
                .HasColumnType("numeric(18,4)");

            entity.Property(x => x.DurationUnit)
                .HasMaxLength(30);

            entity.Property(x => x.AdministrationTime)
                .HasMaxLength(250);

            entity.Property(x => x.Signa)
                .HasMaxLength(500);

            entity.Property(x => x.CompoundingInstruction)
                .HasMaxLength(1000);

            entity.Property(x => x.AdministrationInstruction)
                .HasMaxLength(500);

            entity.Property(x => x.DoctorNote)
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

            entity.HasOne(x => x.PrescriptionTemplate)
                .WithMany(x => x.Compounds)
                .HasForeignKey(x => x.PrescriptionTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PackageUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.PackageUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DoseUnitMeasurement)
                .WithMany()
                .HasForeignKey(x => x.DoseUnitMeasurementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PrescriptionTemplateId);

            entity.HasIndex(x => x.PackageUnitMeasurementId);

            entity.HasIndex(x => x.DoseUnitMeasurementId);

            entity.HasIndex(x => new
            {
                x.PrescriptionTemplateId,
                x.SortOrder,
                x.IsDelete
            });
        }
    }
}
