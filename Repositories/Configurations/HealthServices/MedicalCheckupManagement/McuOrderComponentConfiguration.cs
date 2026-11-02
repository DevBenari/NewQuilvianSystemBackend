using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan komponen order — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>
    /// Satu prosedur atau satu jenis pemeriksaan hanya sekali per order selama komponennya belum
    /// dibatalkan. Angka <c>6</c> pada filter adalah <c>McuOrderComponentStatus.Cancelled</c>.
    /// </remarks>
    public class McuOrderComponentConfiguration : IEntityTypeConfiguration<McuOrderComponent>
    {
        public void Configure(EntityTypeBuilder<McuOrderComponent> builder)
        {
            builder.ToTable("McuOrderComponent", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComponentType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ComponentStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.IsPackageCovered).HasDefaultValue(true).ValueGeneratedNever();
            builder.Property(x => x.RefundAmountSnapshot).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(x => x.MedicalFeeAmountSnapshot).HasPrecision(18, 2).HasDefaultValue(0m);

            builder.HasIndex(x => new { x.McuOrderId, x.ProcedureId })
                .IsUnique()
                .HasFilter("\"ProcedureId\" IS NOT NULL AND \"ComponentStatus\" <> 6 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_McuOrderComponent_Order_Procedure");

            builder.HasIndex(x => new { x.McuOrderId, x.ExaminationTypeId })
                .IsUnique()
                .HasFilter("\"ExaminationTypeId\" IS NOT NULL AND \"ComponentStatus\" <> 6 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_McuOrderComponent_Order_ExaminationType");

            builder.HasIndex(x => x.ComponentStatus);
            builder.HasIndex(x => x.LabOrderId);
            builder.HasIndex(x => x.RadOrderId);
            builder.HasIndex(x => x.MedicalFeeReleasedAt);

            builder.HasOne(x => x.McuOrder)
                .WithMany(x => x.Components)
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PackageComponent)
                .WithMany()
                .HasForeignKey(x => x.PackageComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RadModality)
                .WithMany()
                .HasForeignKey(x => x.RadModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExaminationType)
                .WithMany()
                .HasForeignKey(x => x.ExaminationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabOrder)
                .WithMany()
                .HasForeignKey(x => x.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RadOrder)
                .WithMany()
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PerformingDoctor)
                .WithMany()
                .HasForeignKey(x => x.PerformingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByDoctor)
                .WithMany()
                .HasForeignKey(x => x.CancelledByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RefundCase)
                .WithMany()
                .HasForeignKey(x => x.RefundCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AddedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.AddedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
